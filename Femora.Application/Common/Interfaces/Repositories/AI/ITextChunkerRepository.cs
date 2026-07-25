using Femora.Application.Common.DTOs;
using System.Collections.Generic;

namespace Femora.Application.Common.Interfaces.Repositories;

public interface ITextChunkerRepository
{
    List<TextChunk> ChunkText(string text, int chunkSizeInWords = 300, int overlapInWords = 50);
}
