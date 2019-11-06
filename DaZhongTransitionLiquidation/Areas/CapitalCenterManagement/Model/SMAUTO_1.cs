using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class SMAUTO_1
    {
        public string AID { get; set; }
        public string ADESC { get; set; }
        public string ADESCCHS { get; set; }
        public string APREFIX { get; set; }
        public string ADATE { get; set; }
        public int ALENGTH { get; set; }
        public int ANEXTNO { get; set; }
        public string ALASTDATE { get; set; }
        public Guid VGUID { get; set; }
        public DateTime? VCRTTIME { get; set; }
        public string VCRTUSER { get; set; }
        public DateTime? VMDTIME { get; set; }
        public string VMDUSER { get; set; }
        public string VSTATUS { get; set; }
        public string VLOCK { get; set; }
        public DateTime? VLOCKTIME { get; set; }
    }
}