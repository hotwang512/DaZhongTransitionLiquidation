using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.ViewEntity
{
    public class V_BankChannelMapping
    {
        public Guid? VGUID { get; set; }

        public string Bank { get; set; }

        public string BankAccountName { get; set; }

        public string BankAccount { get; set; } = string.Empty;

        public string Channel { get; set; } = string.Empty;

        public string ChannelName { get; set; }

        public DateTime? VCRTTIME { get; set; }

        public string VCRTUSER { get; set; }

        public DateTime? VMDFTIME { get; set; }

        public string VMDFUSER { get; set; }
    }
}
