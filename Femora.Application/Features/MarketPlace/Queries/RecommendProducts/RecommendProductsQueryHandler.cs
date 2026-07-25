using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Domain.Entities.AI;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Marketplace.Queries.RecommendProducts;

public class RecommendProductsQueryHandler(
    IAppDbContext db,
    IEmbeddingRepository embeddingRepository)
    : IRequestHandler<RecommendProductsQuery, List<RecommendedProductDto>>
{
    // How many candidate products to pull from the preferred categories before ranking.
    private const int CandidatePoolSize = 50;
    // Wider pool we sample CandidatePoolSize from, so results rotate across calls
    // instead of always ranking the exact same 50 newest products.
    private const int WideCandidatePoolSize = 200;

    public async Task<List<RecommendedProductDto>> Handle(RecommendProductsQuery request, CancellationToken cancellationToken)
    {
        var user = await db.ApplicationUsers
            .Include(u => u.OnboardingInterests)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        var preferredInterests = user is null
            ? new List<string>()
            : user.OnboardingInterests
                .Select(i => $"{i.NameEn} ({i.NameAr})")
                .ToList();

        // Pull published products.
        var productsQuery = db.Products
            .Include(p => p.ProductCategory)
            .Include(p => p.ProductImages)
            .Include(p => p.ProductVariants)
            .Where(p => p.IsPuplished);

        var widePool = await productsQuery
            .OrderByDescending(p => p.CreatedAt)
            .Take(WideCandidatePoolSize)
            .ToListAsync(cancellationToken);

        // Randomly sample the ranking candidates out of the wider pool. Without this,
        // the handler always ranked the exact same "N newest products" against the
        // exact same interest embedding, so every trainee saw an identical, frozen
        // list on every single call - this keeps ranking meaningful while letting
        // the visible set rotate.
        var candidates = widePool.Count <= CandidatePoolSize
            ? widePool
            : widePool.OrderBy(_ => Random.Shared.Next()).Take(CandidatePoolSize).ToList();

        if (candidates.Count == 0)
        {
            return new List<RecommendedProductDto>();
        }

        // Build a profile-interest text from the user's preferred onboarding interests to embed as the "query".
        var interestText = preferredInterests.Count > 0
            ? $"Products related to: {string.Join(", ", preferredInterests)}"
            : "Popular and generally useful products";

        var interestEmbedding = await embeddingRepository.GenerateEmbeddingAsync(interestText, cancellationToken);

        // Embed each candidate product's name+description and rank by cosine similarity to the interest profile.
        var productTexts = candidates
            .Select(p => $"{p.Name}. {p.Description ?? string.Empty}")
            .ToList();

        var productEmbeddings = await embeddingRepository.GenerateEmbeddingsAsync(productTexts, cancellationToken);

        var ranked = candidates
            .Zip(productEmbeddings, (product, embedding) => new
            {
                Product = product,
                Score = CosineSimilarity(interestEmbedding, embedding)
            })
            .OrderByDescending(x => x.Score)
            .Take(request.Top)
            .ToList();

        // Persist recommendations for tracking/analytics
        if (user is not null)
        {
            foreach (var item in ranked)
            {
                db.AIRecommendations.Add(new AIRecommendation
                {
                    UserId = request.UserId,
                    Type = AIRecommendationType.Product,
                    EntityId = item.Product.Id,
                    EntityType = "Product",
                    IsViewed = false,
                    GeneratedAt = DateTime.UtcNow,
                    Score = item.Score
                });
            }
            await db.SaveChangesAsync(cancellationToken);
        }

        return ranked.Select(item =>
        {
            var primaryImage = item.Product.ProductImages
                .OrderBy(i => i.OrderIndex)
                .FirstOrDefault(i => i.IsPrimary) ?? item.Product.ProductImages.FirstOrDefault();

            var minPrice = item.Product.ProductVariants.Count > 0
                ? item.Product.ProductVariants.Min(v => v.Price)
                : (decimal?)null;

            return new RecommendedProductDto
            {
                ProductId = item.Product.Id,
                Name = item.Product.Name,
                Description = item.Product.Description,
                CategoryName = item.Product.ProductCategory?.Name ?? string.Empty,
                MinPrice = minPrice,
                PrimaryImageUrl = primaryImage?.ImageUrl,
                Score = item.Score
            };
        }).ToList();
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        if (normA == 0 || normB == 0) return 0;
        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
