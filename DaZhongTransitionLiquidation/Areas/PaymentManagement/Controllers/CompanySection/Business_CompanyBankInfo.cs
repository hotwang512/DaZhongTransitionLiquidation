﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection
{
    public class Business_CompanyBankInfo
    {
        public Guid VGUID { get; set; }
        public string BankName { get; set; }
        public string BankAccount { get; set; }
        public string CompanyCode { get; set; }
        public string BankAccountName { get; set; }
        public string AccountType { get; set; }
        public decimal? InitialBalance { get; set; }
        public string AccountModeCode { get; set; }
        public bool BankStatus { get; set; }
        public bool OpeningDirectBank { get; set; }
        public string Borrow { get; set; }
        public string Loan { get; set; }
    }
}
