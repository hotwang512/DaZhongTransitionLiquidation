using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList;
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
                //从订单配置表中取出数据
                var data = _db.Queryable<Business_OrderList>().WhereIF(BusinessType != null, i => i.BusinessType == BusinessType)
                           .WhereIF(BusinessProject != null, i => i.BusinessProject == BusinessProject)
                           .WhereIF(BusinessSubItem1 != null, i => i.BusinessSubItem1 == BusinessSubItem1)
                           .WhereIF(BusinessSubItem2 != null, i => i.BusinessSubItem2 == BusinessSubItem2)
                           .WhereIF(BusinessSubItem3 != null, i => i.BusinessSubItem3 == BusinessSubItem3)
                           .ToList();
                //数据存入订单草稿表，生成订单
                if(data != null)
                {
                    OrderListAPI.PaymentCompany = "";
                    OrderListAPI.CollectionCompany = "";
                    OrderListAPI.Mode = data[0].Mode;
                    OrderListAPI.VehicleType = data[0].VehicleType;
                    OrderListAPI.SubmitDate = DateTime.Now;
                    //OrderListAPI.PaymentMethod = "";
                    OrderListAPI.AttachmentNumber = data[0].AttachmentNumber;
                    OrderListAPI.InvoiceNumber = data[0].InvoiceNumber;
                    OrderListAPI.VGUID = Guid.NewGuid();
                    _db.Insertable<Business_OrderListDraft>(OrderListAPI).ExecuteCommand();
                }
            }
        }
        public static void SaveVoucherListDetail(VoucherListDetail voucher)
        {

        }
    }
}