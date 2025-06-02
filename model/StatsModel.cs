using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELMS_Group1.model
{
    public class StatsModel
    {
        public long Admins { get; set; }
        public long Users { get; set; }
        public long PendingUsers { get; set; }
        public long InventoryItems { get; set; }
        public long BorrowedItems { get; set; }
        public long OverdueItems { get; set; }
        public long DamagedItems { get; set; }
        public long LostItems { get; set; }
        public long PendingBorrows { get; set; }
    }
}
