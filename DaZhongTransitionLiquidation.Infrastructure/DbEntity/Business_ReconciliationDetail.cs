using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.DbEntity
{
    public class Business_ReconciliationDetail
    {
        public Guid? VGUID { get; set; }

        public Guid? Business_ReconciliationVGUID { get; set; }

        public DateTime? RevenueDate { get; set; }


        public decimal? RevenueTotalAmount { get; set; }


        public decimal? T1DataTotalAmount { get; set; }


        public DateTime? CreatedDate { get; set; }

        public string CreatedUser { get; set; }

        public DateTime? ChangeDate { get; set; }


        public string ChangeUser { get; set; }









    }
}
