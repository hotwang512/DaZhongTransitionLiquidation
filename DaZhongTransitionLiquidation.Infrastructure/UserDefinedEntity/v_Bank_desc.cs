using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity
{
    public class v_Bank_desc
    {
        public string ReceiveBank { get; set; }
        public string ReceiveBankAccountName { get; set; }
        public string ReceiveBankAccount { get; set; }
        public string ExpendBank { get; set; }
        public string ExpendBankAccountName { get; set; }
        public string ExpendBankAccount { get; set; }
        public DateTime? ArrivedTime { get; set; }
        public decimal? ArrivedTotal { get; set; }

        public string Channel_Id { get; set; }

        public string Name { get; set; }

        public string remark { get; set; }

        public Guid VGUID { get; set; }


        public DateTime? VCRTTIME { get; set; }


        public string VCRTUSER { get; set; }


        public DateTime? VMDFTIME { get; set; }


        public string VMDFUSER { get; set; }


    }
}
