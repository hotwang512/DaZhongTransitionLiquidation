using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.DbEntity
{
    public class Business_Reconciliation
    {
        public Guid? VGUID { get; set; }

        public DateTime? ReconciliationDate { get; set; }

        public string ReconciliationUser { get; set; }


        public DateTime? BankBillDate { get; set; }

        public decimal? BankBillTotalAmount { get; set; }

        public string Channel_Id { get; set; }

        public string AbnormalReason { get; set; }
        public string BatchBillNo { get; set; }
        public string Status { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string CreatedUser { get; set; }

        public DateTime? ChangeDate { get; set; }


        public string ChangeUser { get; set; }



    }
}
