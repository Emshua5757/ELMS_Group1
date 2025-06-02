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
        public const string ApproveUser = "approve_user";
        public const string BorrowApproval = "borrow_approval";
        public const string BorrowRejection = "borrow_rejection";
        public const string BorrowStatusUpdate = "borrow_status_update";
        public const string DeleteUser = "delete_user";
        public const string InventoryAddition = "inventory_addition";
        public const string InventoryDeletion = "inventory_delete";
        public const string InventoryUpdate = "inventory_update";
        public const string RejectUser = "reject_user";
        public const string UpdateUser = "update_user";
        public const string InventoryExport = "inventory_export";
        public const string MissingItem = "missing_item";
        public const string ReturnLate = "return_late";
        public const string Lost = "lost";
        public const string Damaged = "damaged";    
        public const string UsersExport = "users_export";
        public const string PendingUsersExport = "pending_users_export";
    }

    public class Report
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long Id { get; set; }

        [JsonPropertyName("admin_id")]
        public long AdminId { get; set; }

        [JsonPropertyName("report_type")]
        public string ReportType { get; set; }  

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;  

        [JsonPropertyName("format")]
        public string Format { get; set; } = "text";  

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
