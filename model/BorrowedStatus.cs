using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ELMS_Group1.model
{
    public enum BorrowStatus
    {
        Pending,
        Borrowed,
        Returned,
        ReturnedLate,
        Overdue,
        Lost,
        Damaged,
        Resolved,
        Rejected
    }

    public class BorrowedStatus
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? Id { get; set; }

        [JsonPropertyName("inventory_id")]
        public long InventoryId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("borrower_name")]
        public string BorrowerName { get; set; } = string.Empty;

        [JsonPropertyName("borrowed_date")]
        public DateTime BorrowedDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("due_date")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? DueDate { get; set; }

        [JsonPropertyName("returned_date")]
        public DateTime? ReturnedDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = BorrowStatus.Pending.ToString();

        [JsonPropertyName("approved_by")]
        public long? ApprovedBy { get; set; }  

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public BorrowStatus StatusEnum
        {
            get => Enum.TryParse(Status, out BorrowStatus result) ? result : BorrowStatus.Pending;
            set => Status = value.ToString();
        }
    }
}
