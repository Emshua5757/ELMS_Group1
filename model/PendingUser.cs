using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
public class PendingUser
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("registered_at")]
    public DateTime RegisteredAt { get; set; }

    [JsonPropertyName("id_number")]
    public string IdNumber { get; set; } = string.Empty;

    [JsonPropertyName("course_year")]
    public string CourseYear { get; set; } = string.Empty;

    [JsonPropertyName("is_approved")]
    public bool? IsApproved { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("admin_feedback")]
    public string? AdminFeedback { get; set; }

    [JsonPropertyName("reviewed_at")]
    public DateTime? ReviewedAt { get; set; }
    [JsonPropertyName("reviewed_by")]
    public string? ReviewedBy { get; set; }
}