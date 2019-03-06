using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.ViewEntity
{
    public class v_Channel_Desc
    {
        public string ContactBank { get; set; }
        public string Department { get; set; }
        public string Id { get; set; }
        public string MerchantsCode { get; set; }
        public string Name { get; set; }
        public decimal? Offset { get; set; }
        public string OrganizationName { get; set; }
        public string SupplierName { get; set; }
        public string Temp1 { get; set; }
        public string Temp2 { get; set; }
        public DateTime? VCRTTIME { get; set; }
        public string VCRTUSER { get; set; }
        public Guid Vguid { get; set; }
        public DateTime? VMDFTIME { get; set; }
        public string VMDFUSER { get; set; }
        public string PaymentEncoding { get; set; }


    }
}
