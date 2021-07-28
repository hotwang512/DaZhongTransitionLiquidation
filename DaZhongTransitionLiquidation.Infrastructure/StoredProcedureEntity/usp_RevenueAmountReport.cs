using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity
{
    public class usp_RevenueAmountReport
    {
        public string ChannelName { get; set; }

        public string Channel_Id { get; set; }

        public string SubjectNmae { get; set; }

        public string SubjectId { get; set; }

        public string Department { get; set; }

        public string OrganizationName { get; set; }

        public string RevenueMonth { get; set; }

        public string RevenueDate { get; set; }

        public decimal? ActualAmountTotal { get; set; }

        public decimal? PaymentAmountTotal { get; set; }

        public decimal? DriverBearFeesTotal { get; set; }

        public decimal? CompanyBearsFeesTotal { get; set; }
        public Guid OrgVGUID { get; set; }
    }
}
