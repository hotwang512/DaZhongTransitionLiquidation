using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity
{
    public class usp_GetSubjectAmount
    {
        public decimal? RevenuepaymentTotal { get; set; }

        public string SubjectId { get; set; }

        public decimal? SubjectAmount { get; set; }

        public int? Counts { get; set; }

        public int? SubjectCounts { get; set; }

    }
}
