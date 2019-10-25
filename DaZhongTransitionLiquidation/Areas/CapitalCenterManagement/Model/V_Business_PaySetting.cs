﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class V_Business_PaySetting
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
        public string IsUnable { get; set; }
        public string Borrow { get; set; }
        public string Loan { get; set; }
        public string CompanyCode { get; set; }
        public string IsShow { get; set; }
    }
}