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
        public static void SavePaymentOrder(Business_OrderListDraft OrderListAPI)
        {
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
            if (OrderListAPI != null)
            {
                var BusinessType = OrderListAPI.BusinessType;
                var BusinessProject = OrderListAPI.BusinessProject;
                var BusinessSubItem1 = OrderListAPI.BusinessSubItem1;
                var BusinessSubItem2 = OrderListAPI.BusinessSubItem2;
                var BusinessSubItem3 = OrderListAPI.BusinessSubItem3;
                OrderListAPI.PaymentCompany = "";
                OrderListAPI.CollectionCompany = "";
                OrderListAPI.Mode = "";
                OrderListAPI.VehicleType = "";
                OrderListAPI.SubmitDate = DateTime.Now;
                OrderListAPI.PaymentMethod = "";
                OrderListAPI.AttachmentNumber = 0;
                OrderListAPI.InvoiceNumber = 0;
                OrderListAPI.VGUID = Guid.NewGuid();
                _db.Insertable<Business_OrderListDraft>(OrderListAPI).ExecuteCommand();
            }
        }
        public static void SaveVoucherListDetail(VoucherListDetail voucher)
        {

        }
    }
}