using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Models;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class PaymentOrderAPI: Controller
    {
        public static void SavePaymentOrder(List<Business_OrderListDraft> OrderListAPI)
        {
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
            if (OrderListAPI != null)
            {
                foreach (var item in OrderListAPI)
                {
                    item.PaymentCompany = "";
                    item.CollectionCompany = "";
                    item.Mode = "";
                    item.VehicleType = "";
                    item.SubmitDate = DateTime.Now;
                    item.PaymentMethod = "";
                    item.AttachmentNumber = 0;
                    item.InvoiceNumber = 0;
                }
                _db.Insertable<Business_OrderListDraft>(OrderListAPI).ExecuteCommand();
            }
        }
        public static void SaveVoucherListDetail(VoucherListDetail voucher)
        {

        }
    }
}