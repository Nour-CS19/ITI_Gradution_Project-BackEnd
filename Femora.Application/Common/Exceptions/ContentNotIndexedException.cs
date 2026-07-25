namespace Femora.Application.Common.Exceptions;

/// <summary>
/// Thrown when a RAG operation (chat, summarize, quiz generation) is attempted on a
/// lesson/module that has no indexed content yet — i.e. no resource has been uploaded
/// and successfully indexed for it.
/// </summary>
public class ContentNotIndexedException(string message) : Exception(message);
