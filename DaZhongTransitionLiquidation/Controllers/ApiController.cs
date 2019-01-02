using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Models;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class ApiController : Controller
    {

        public DbBusinessDataService DbBusinessDataService;
        public ApiController(DbBusinessDataService dbBusinessDataService)
        {
            DbBusinessDataService = dbBusinessDataService;
        }


        // GET: NewApi
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 营收系统数据同步至清算平台接口      
        /// </summary>
        /// <param name="revenuepayment"></param>
        /// 请求示例：
        ///{
        ///Name:"李永飞",
        ///JobNumber:"734232",
        ///UserID:"18301914615",
        ///PhoneNumber:"18301914615",
        /// Parameter1:"18301914615",（DriverID）
        ///Parameter2:"18301914615",（OriginalId）
        ///Parameter3: “123123”,（OrganizationID） 
        ///TransactionID:"4009132001201706074709396939"
        ///PaymentBrokers:"ID00008",
        ///PayDate:"2017-06-15 10:34:41.550",
        ///PaymentAmount：0.1,  //应付款
        ///copeFee:0.006    //应收手续费
        ///ActualAmount：0.1,   //实际付款
        ///CompanyAccount:0   //公司到账
        ///}
        /// <returns></returns>
        public JsonResult Revenue_RevenueSystem(U_Business_Revenuepayment_Information revenuepayment)
        {
            //var db = DbConfig.GetInstance();
            var db = DbBusinessDataConfig.GetInstance();

            Business_Revenuepayment_Information business_Revenuepayment = new Business_Revenuepayment_Information();
            Business_T1Data_Information t1Data = new Business_T1Data_Information();
            try
            {
                ExpCheck.Exception(revenuepayment.Name.IsNullOrEmpty(), "人员姓名为空！");
                //ExpCheck.Exception(revenuepayment.Department.IsNullOrEmpty(), "部门为空！");
                ExpCheck.Exception(revenuepayment.TransactionID.IsNullOrEmpty(), "支付流水号为空！");
                //ExpCheck.Exception(revenuepayment.PaymentPersonnel == null, "人员主键为空！");
                ExpCheck.Exception(revenuepayment.PaymentBrokers.IsNullOrEmpty(), "支付人为空！");
                ExpCheck.Exception(revenuepayment.PayDate == null, "支付时间为空！");
                ExpCheck.Exception(revenuepayment.PaymentAmount == null, "应付款为空！");
                ExpCheck.Exception(revenuepayment.ActualAmount == null, "实际付款为空！");
                ExpCheck.Exception(revenuepayment.copeFee == null, "应收手续费为空！");
                ExpCheck.Exception(revenuepayment.Channel_Id == null, "支付渠道为空！");
                var result = false;
                DbBusinessDataService.Command(dbs =>
                {
                    result = dbs.Queryable<Business_Revenuepayment_Information>().Any(i => i.TransactionID == revenuepayment.TransactionID);

                });
                ExpCheck.Exception(result, "支付流水号已存在");
                t1Data.Vguid = Guid.NewGuid();

                business_Revenuepayment.Name = revenuepayment.Name;
                business_Revenuepayment.JobNumber = revenuepayment.JobNumber;
                business_Revenuepayment.UserID = revenuepayment.UserID;
                business_Revenuepayment.Parameter1 = revenuepayment.DriverID;
                business_Revenuepayment.Parameter2 = revenuepayment.OriginalId;
                business_Revenuepayment.Parameter3 = revenuepayment.OrganizationID;
                t1Data.serialnumber = business_Revenuepayment.TransactionID = revenuepayment.TransactionID;
                business_Revenuepayment.PaymentBrokers = revenuepayment.PaymentBrokers;
                t1Data.Revenuetime = business_Revenuepayment.PayDate = revenuepayment.PayDate;
                t1Data.Remitamount = business_Revenuepayment.PaymentAmount = revenuepayment.PaymentAmount;
                t1Data.RevenueFee = business_Revenuepayment.copeFee = revenuepayment.copeFee;
                t1Data.PaidAmount = business_Revenuepayment.ActualAmount = revenuepayment.ActualAmount;
                business_Revenuepayment.CompanyAccount = revenuepayment.CompanyAccount;

                bool ishaveChannelId = false;
                DbBusinessDataService.Command(dbs =>
                {
                    ishaveChannelId = dbs.Queryable<T_Channel>().Any(i => i.Id == revenuepayment.Channel_Id);//科目是否存在
                });
                ExpCheck.Exception(!ishaveChannelId, "渠道不存在");
                t1Data.Channel_Id = business_Revenuepayment.Channel_Id = revenuepayment.Channel_Id;//传过来的渠道为科目

                t1Data.SubjectId = business_Revenuepayment.SubjectId = revenuepayment.SubjectId;



                //var channelid = "";
                //DbBusinessDataService.Command(dbs =>
                //{
                //    var sql = string.Format(@"SELECT Id FROM T_Channel WHERE Vguid=(SELECT ChannelVguid FROM T_Channel_Subject WHERE SubjectId={0})", revenuepayment.Channel_Id);
                //    channelid = dbs.Ado.GetString(sql);//查询该科目所属渠道
                //});
                //business_Revenuepayment.Channel_Id = channelid;

                business_Revenuepayment.PaymentStatus = "1";//付款成功
                t1Data.Vguid = business_Revenuepayment.VGUID = Guid.NewGuid();

                t1Data.CreatedDate = business_Revenuepayment.CreateDate = DateTime.Now;
                t1Data.ChangeUser = business_Revenuepayment.CreateUser = "RevenueSystem";

                db.Ado.UseTran(delegate
                {
                    db.Insertable(business_Revenuepayment).ExecuteCommand();
                    db.Insertable(t1Data).ExecuteCommand();
                });

                return Json(new
                {
                    success = true,
                    errmsg = "",
                    result = business_Revenuepayment.VGUID
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
                return Json(new
                {
                    success = false,
                    errmsg = ex.Message,
                    result = ""
                }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 同步pos机的数据
        /// 清算平台营收标准接口
        /// </summary>
        /// <param name="revenuepayment">
        /// 请求示例:
        /// {
        /// Name:"李永飞",       
        /// JobNumber:"734232",
        /// Parameter1:"18301914615",（DriverID）
        ///Parameter2:"18301914615",（OriginalId）
        ///Parameter3: “123123”,（OrganizationID）  
        /// TransactionID:"4009132001201706074709396939"
        /// PaymentBrokers:"ID00008",
        /// PayDate:"2017-06-15 10:34:41.550",
        /// PaymentAmount：0.1,  //应付款
        /// copeFee:0.006    //应收手续费
        /// ActualAmount：0.1,   //实际付款
        /// CompanyAccount:0   //公司到账
        /// }
        /// updata bing.cheng 2018/6/1
        /// </param>
        /// <returns></returns>
        public JsonResult Revenue(U_Business_Revenuepayment_Information2 revenuepayment)
        {
            //var db = DbConfig.GetInstance();
            var db = DbBusinessDataConfig.GetInstance();
            Business_Revenuepayment_Information business_Revenuepayment = new Business_Revenuepayment_Information();
            try
            {
                ExpCheck.Exception(revenuepayment.Name.IsNullOrEmpty(), "人员姓名为空！");
                //ExpCheck.Exception(revenuepayment.Department.IsNullOrEmpty(), "部门为空！");
                ExpCheck.Exception(revenuepayment.TransactionID.IsNullOrEmpty(), "支付流水号为空！");
                //ExpCheck.Exception(revenuepayment.PaymentPersonnel == null, "人员主键为空！");
                ExpCheck.Exception(revenuepayment.PaymentBrokers.IsNullOrEmpty(), "支付人为空！");
                ExpCheck.Exception(revenuepayment.PayDate == null, "支付时间为空！");
                ExpCheck.Exception(revenuepayment.PaymentAmount == null, "应付款为空！");
                ExpCheck.Exception(revenuepayment.ActualAmount == null, "实际付款为空！");
                ExpCheck.Exception(revenuepayment.copeFee == null, "应收手续费为空！");
                ExpCheck.Exception(revenuepayment.Channel_Id == null, "支付渠道为空！");
                var result = false;
                DbBusinessDataService.Command(dbs =>
                {
                    result = dbs.Queryable<Business_Revenuepayment_Information>().Any(i => i.TransactionID == revenuepayment.TransactionID);

                });
                ExpCheck.Exception(result, "支付流水号已存在");

                business_Revenuepayment.Name = revenuepayment.Name;
                business_Revenuepayment.JobNumber = revenuepayment.JobNumber;
                business_Revenuepayment.Parameter1 = revenuepayment.Parameter1;
                business_Revenuepayment.Parameter2 = revenuepayment.Parameter2;
                business_Revenuepayment.Parameter3 = revenuepayment.Parameter3;

                business_Revenuepayment.TransactionID = revenuepayment.TransactionID;

                business_Revenuepayment.PaymentBrokers = revenuepayment.PaymentBrokers;
                business_Revenuepayment.PayDate = revenuepayment.PayDate;
                business_Revenuepayment.PaymentAmount = revenuepayment.PaymentAmount;
                business_Revenuepayment.copeFee = revenuepayment.copeFee;
                business_Revenuepayment.ActualAmount = revenuepayment.ActualAmount;
                business_Revenuepayment.CompanyAccount = revenuepayment.CompanyAccount;

                bool ishaveSubjectId = false;
                DbBusinessDataService.Command(dbs =>
                {
                    ishaveSubjectId = dbs.Queryable<T_Channel_Subject>().Any(i => i.SubjectId == revenuepayment.Channel_Id);//科目是否存在
                });
                ExpCheck.Exception(!ishaveSubjectId, "渠道不存在");
                business_Revenuepayment.Channel_Id = revenuepayment.Channel_Id;//传过来的渠道为科目
                business_Revenuepayment.SubjectId = revenuepayment.Subject_Id;


                var channelid = "";
                DbBusinessDataService.Command(dbs =>
                {
                    var sql = string.Format(@"SELECT Id FROM T_Channel WHERE Vguid=(SELECT ChannelVguid FROM T_Channel_Subject WHERE SubjectId={0})", revenuepayment.Channel_Id);
                    channelid = dbs.Ado.GetString(sql);//查询该科目所属渠道
                });
                business_Revenuepayment.Channel_Id = channelid;

                business_Revenuepayment.PaymentStatus = "1";//付款成功
                business_Revenuepayment.VGUID = Guid.NewGuid();
                business_Revenuepayment.CreateDate = DateTime.Now;
                business_Revenuepayment.CreateUser = "Revenue";
                db.Insertable(business_Revenuepayment).ExecuteCommand();
                return Json(new
                {
                    errmsg = "ok"
                });
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
                return Json(new
                {
                    errmsg = ex.Message
                });
            }
        }

        /// <summary>
        /// 订单生产接口
        /// </summary>
        /// <param name="OrderListAPI">
        ///  /// 请求示例:
        /// {
        /// PaymentCompany:"大众交通(集团)股份有限公司 付款公司",       
        /// ServiceCategory:"保险（1001）",
        /// BusinessProject:"投保（1001001）",
        /// Amount:"100000",
        /// Sponsor: "安全系统 发起人",
        /// Summary:"安全系统 保险（1001）投保（1001001） 付款内容(摘要)"
        /// }
        /// </param>
        /// <returns></returns>
        public JsonResult Pay_BuildPaymentVoucher(Business_OrderListAPI OrderListAPI)
        {
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
            try
            {
                ExpCheck.Exception(OrderListAPI.PaymentCompany == null, "付款公司为空！");
                ExpCheck.Exception(OrderListAPI.ServiceCategory == null, "类别为空！");
                ExpCheck.Exception(OrderListAPI.BusinessProject == null, "项目为空！");
                ExpCheck.Exception(OrderListAPI.Amount == null, "金额为空！");
                ExpCheck.Exception(OrderListAPI.Sponsor == null, "发起人为空！");
                ExpCheck.Exception(OrderListAPI.Summary == null, "摘要为空！");
                var guid = Guid.NewGuid();
                if (OrderListAPI != null)
                {
                    var BusinessType = OrderListAPI.ServiceCategory;
                    var BusinessProject = OrderListAPI.BusinessProject;
                    //var BusinessSubItem1 = OrderListAPI.BusinessSubItem1;
                    //var BusinessSubItem2 = OrderListAPI.BusinessSubItem2;
                    //var BusinessSubItem3 = OrderListAPI.BusinessSubItem3;
                    //从订单配置表中取出数据
                    var data = _db.Queryable<Business_OrderList>().WhereIF(BusinessType != null, i => i.BusinessType == BusinessType)
                               .WhereIF(BusinessProject != null, i => i.BusinessProject == BusinessProject)
                               //.WhereIF(BusinessSubItem1 != null, i => i.BusinessSubItem1 == BusinessSubItem1)
                               //.WhereIF(BusinessSubItem2 != null, i => i.BusinessSubItem2 == BusinessSubItem2)
                               //.WhereIF(BusinessSubItem3 != null, i => i.BusinessSubItem3 == BusinessSubItem3)
                               .ToList().FirstOrDefault();
                    //数据存入订单草稿表，生成订单
                    
                    Business_OrderListDraft orderListDraft = new Business_OrderListDraft();
                    if (data != null)
                    {
                        orderListDraft.PaymentCompany = OrderListAPI.PaymentCompany;
                        orderListDraft.PaymentContents = OrderListAPI.Summary;
                        orderListDraft.FillingDate = DateTime.Now;
                        orderListDraft.Founder = OrderListAPI.Sponsor;
                        orderListDraft.Money = OrderListAPI.Amount.TryToDecimal();
                        //OrderListAPI.Mode = data.Mode;
                        //OrderListAPI.VehicleType = data.VehicleType;
                        orderListDraft.SubmitDate = DateTime.Now;
                        orderListDraft.PaymentMethod = data.PaymentMethod;
                        //OrderListAPI.AttachmentNumber = data.AttachmentNumber;
                        //OrderListAPI.InvoiceNumber = data.InvoiceNumber;
                        orderListDraft.VGUID = guid;
                        orderListDraft.Status = "1";
                        orderListDraft.CreateTime = DateTime.Now;
                        _db.Insertable<Business_OrderListDraft>(orderListDraft).ExecuteCommand();
                    }
                }
                return Json(new
                {
                    errmsg = "/CapitalCenterManagement/OrderListDraftPrint/Index?VGUID=" + guid
                });
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
                return Json(new
                {
                    errmsg = ex.Message
                });
            }
        }
    }
}