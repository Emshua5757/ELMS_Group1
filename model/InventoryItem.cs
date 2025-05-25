using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ELMS_Group1.model
{
    public enum InventoryItemStatus
    {
        Available,
        Borrowed,
        Lost,
        Damaged,
        Unavailable
    }

    public class InventoryItem
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        public long? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; } = string.Empty;

        [JsonPropertyName("serial_number")]
        public string SerialNumber { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string? Location { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = InventoryItemStatus.Available.ToString();

        [JsonPropertyName("created_by")]
        public long? CreatedBy { get; set; }  

        [JsonPropertyName("acquisition_date")]
        public DateTime? AcquisitionDate { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("value")]
        public decimal Value { get; set; } = 0m;

        [JsonIgnore]
        public InventoryItemStatus StatusEnum
        {
            get
            {
                if (Enum.TryParse(Status, out InventoryItemStatus result))
                    return result;
                return InventoryItemStatus.Available;
            }
            set
            {
                Status = value.ToString();
            }
        }
    }
}
