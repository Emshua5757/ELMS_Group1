using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ELMS_Group1.model
{
    public static class ReportTypes
    {
        public const string DeleteUser = "delete_user";
        public const string ApproveUser = "approve_user";
        public const string RejectUser = "reject_user";
        public const string DeleteInventory = "delete_inventory";
        public const string InventoryExport = "inventory_export";
        public const string MissingItem = "missing_item";
        public const string ReturnLate = "return_late";
        public const string InventoryAddition = "inventory_addition";
        public const string UsersExport = "users_export";
        public const string PendingUsersExport = "pending_users_export";
    }

    internal class Report
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long Id { get; set; }

        [JsonPropertyName("admin_id")]
        public long AdminId { get; set; }

        [JsonPropertyName("report_type")]
        public string ReportType { get; set; }  // e.g. "inventory_export", "missing_item", etc.

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;  // Long-form content like CSV, JSON, plain text

        [JsonPropertyName("format")]
        public string Format { get; set; } = "text";  // "text", "csv", "json", "markdown"

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
