using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Femora.Infrastructure.Repositories;

public class TextChunkerRepository : ITextChunkerRepository
{
    public List<TextChunk> ChunkText(string text, int chunkSizeInWords = 300, int overlapInWords = 50)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<TextChunk>();
        var normalized = Regex.Replace(text, @"\s+", " ").Trim();
        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<TextChunk>();
        var step = chunkSizeInWords - overlapInWords;
        var chunkIndex = 0;

        for (var start = 0; start < words.Length; start += step)
        {
            var length = Math.Min(chunkSizeInWords, words.Length - start);
            var chunkWords = words.Skip(start).Take(length).ToArray();
            chunks.Add(new TextChunk { ChunkIndex = chunkIndex, Content = string.Join(' ', chunkWords), WordCount = chunkWords.Length });
            chunkIndex++;
            if (start + length >= words.Length) break;
        }
        return chunks;
    }
}
