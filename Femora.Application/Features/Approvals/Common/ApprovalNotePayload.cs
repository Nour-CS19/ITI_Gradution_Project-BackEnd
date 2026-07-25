using System.Text.Json;

namespace Femora.Application.Features.Approvals.Common;

/// <summary>
/// Structured payload stored in ApprovalRequest.Note as JSON.
/// Replaces the old "Key:Value;Key2:Value2" format, which silently
/// truncated/lost data whenever a field contained ';' or ':'.
/// </summary>
public class ApprovalNotePayload
{
    // Instructor
    public string? Bio { get; set; }
    public string? Portfolio { get; set; }

    // Seller
    public string? ShopName { get; set; }
    public string? Description { get; set; }

    // Course / Product (display title in admin list)
    public string? Title { get; set; }

    // Set by admin on review (approve/reject)
    public string? AdminNote { get; set; }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string ToJson() => JsonSerializer.Serialize(this, Options);

    /// <summary>
    /// Parses a Note value. Supports the new JSON format and falls back to the
    /// legacy "Key:Value;Key2:Value2" format for rows written before this fix,
    /// so existing pending requests don't lose their data.
    /// </summary>
    public static ApprovalNotePayload Parse(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return new ApprovalNotePayload();

        // New format: valid JSON object
        if (note.TrimStart().StartsWith('{'))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<ApprovalNotePayload>(note, Options);
                if (parsed is not null)
                    return parsed;
            }
            catch (JsonException)
            {
                // fall through to legacy parsing
            }
        }

        // Legacy format fallback: "Key:Value;Key2:Value2"
        var payload = new ApprovalNotePayload();
        var parts = note.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in parts)
        {
            var colonIndex = part.IndexOf(':');
            if (colonIndex <= 0) continue;
            var key = part[..colonIndex].Trim();
            var value = part[(colonIndex + 1)..].Trim();
            map[key] = value;
        }

        payload.Bio = map.GetValueOrDefault("Bio");
        payload.Portfolio = map.GetValueOrDefault("Portfolio");
        payload.ShopName = map.GetValueOrDefault("ShopName");
        payload.Description = map.GetValueOrDefault("Description");
        payload.Title = map.GetValueOrDefault("Title");
        payload.AdminNote = map.GetValueOrDefault("AdminNote");
        return payload;
    }
}
