using System;
using System.Collections.Generic;

namespace ELMS_Group1.model
{
    public class BorrowRecordDetail
    {
        public BorrowedStatus BorrowStatus { get; set; }
        public InventoryItem? Inventory { get; set; }
        public Admin? Admin { get; set; }

        public string GetSearchText()
        {
            var parts = new List<string>
            {
                BorrowStatus.BorrowerName ?? "",
                BorrowStatus.Status ?? "",
                BorrowStatus.Id.ToString(),
                BorrowStatus.InventoryId.ToString(),
                BorrowStatus.BorrowedDate.ToString("yyyy-MM-dd"),
                BorrowStatus.DueDate?.ToString("yyyy-MM-dd") ?? ""
            };

            if (Inventory != null)
            {
                parts.Add(Inventory.Name ?? "");
                parts.Add(Inventory.Description ?? "");
                parts.Add(Inventory.SerialNumber ?? "");
                parts.Add(Inventory.Category ?? "");
                parts.Add(Inventory.Status ?? "");
            }

            if (Admin != null)
            {
                parts.Add(Admin.FullName ?? "");
                parts.Add(Admin.Email ?? "");
                parts.Add(Admin.Phone ?? "");
            }

            return string.Join(" ", parts).ToLowerInvariant();
        }
        public decimal PenaltyAmount
        {
            get
            {
                if (BorrowStatus == null || Inventory == null)
                    return 0m;

                switch (BorrowStatus.Status)
                {
                    case "Lost":
                    case "Damaged":
                        return Inventory.Value;

                    case "Overdue":
                        if (!BorrowStatus.DueDate.HasValue)
                            return 0m;

                        var overdueDays = (DateTime.UtcNow.Date - BorrowStatus.DueDate.Value.Date).Days;
                        return overdueDays > 0 ? overdueDays * 2m : 0m;

                    default:
                        return 0m;
                }
            }
        }
    }
}
