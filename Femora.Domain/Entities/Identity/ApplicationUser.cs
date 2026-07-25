using Femora.Domain.Entities.AI;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Entities.Onboarding;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities;
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Guid? OnboardingGoalId { get; set; }
    public OnboardingGoal? OnboardingGoal { get; set; }
    public ICollection<OnboardingInterest> OnboardingInterests { get; set; } = new List<OnboardingInterest>();
    public InstructorProfile? InstructorProfile { get; set; }
    public SellerProfile? SellerProfile { get; set; }
    public TraineeProfile? TraineeProfile { get; set; }
    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<AIConversation> AIConversations { get; set; } = new List<AIConversation>();
    public ICollection<AIRecommendation> AIRecommendations { get; set; } = new List<AIRecommendation>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<ProfileApplicationRequest> ProfileApplicationRequests { get; set; } = new List<ProfileApplicationRequest>();
}
