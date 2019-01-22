using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
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
        /// AccountSetCode:"支付单位账套代码",       
        /// ServiceCategory:"保险（1001）",
        /// BusinessProject:"投保（1001001）",
        /// invoiceNumber:"1",
        /// numberOfAttachments:"2",
        /// Amount:"100000",
        /// Sponsor: "安全系统 发起人",
        /// Summary:"安全系统 保险（1001）投保（1001001） 付款内容(摘要)"
        /// }
        /// </param>
        /// <returns></returns>
        public JsonResult Pay_BuildPaymentVoucher(Business_OrderListAPI OrderListAPI)
        {
            var results = "";
            var errmsg = string.Empty;
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
            try
            {
                ExpCheck.Exception(OrderListAPI.AccountSetCode == null, "支付单位账套代码为空！");
                //ExpCheck.Exception(OrderListAPI.ServiceCategory == null, "类别为空！");
                ExpCheck.Exception(OrderListAPI.BusinessProject == null, "项目为空！");//编码
                ExpCheck.Exception(OrderListAPI.Amount == null, "金额为空！");
                ExpCheck.Exception(OrderListAPI.Sponsor == null, "发起人为空！");
                ExpCheck.Exception(OrderListAPI.Summary == null, "摘要为空！");
                var guid = Guid.NewGuid();
                var Ifsuccess = false;
                if (OrderListAPI != null)
                {
                    var accountSetCode = OrderListAPI.AccountSetCode;
                    var accountModeCode = accountSetCode.Split("|")[0];//账套
                    var companyCode = accountSetCode.Split("|")[1];//公司
                    var BusinessType = OrderListAPI.ServiceCategory;
                    var BusinessProject = OrderListAPI.BusinessProject;
                    //var BusinessSubItem1 = OrderListAPI.BusinessSubItem1;
                    //var BusinessSubItem2 = OrderListAPI.BusinessSubItem2;
                    //var BusinessSubItem3 = OrderListAPI.BusinessSubItem3;
                    //从订单配置表中取出数据
                    var data = _db.Queryable<Business_OrderList>().WhereIF(BusinessProject != null, i => i.BusinessSubItem1 == BusinessProject)
                               .WhereIF(companyCode != null, i => i.CompanyCode == companyCode)
                               //.WhereIF(BusinessSubItem1 != null, i => i.BusinessSubItem1 == BusinessSubItem1)
                               //.WhereIF(BusinessSubItem2 != null, i => i.BusinessSubItem2 == BusinessSubItem2)
                               //.WhereIF(BusinessSubItem3 != null, i => i.BusinessSubItem3 == BusinessSubItem3)
                               .ToList().FirstOrDefault();
                    //数据存入订单草稿表，生成订单
                    Business_OrderListDraft orderListDraft = new Business_OrderListDraft();
                    if (data != null)
                    {
                        var orderCompany = _db.Queryable<Business_SevenSection>().Single(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1" && x.Code == companyCode).Descrption;
                        //var bankInfo = _db.Queryable<Business_CompanyBankInfo>().Where(x => x.CompanyCode == companyCode && x.AccountType == "基本户" && x.AccountModeCode == accountModeCode).First();

                        orderListDraft.OrderBankName = data.PayBank;//付款账号开户行
                        orderListDraft.OrderBankAccouont = data.PayAccount;//付款账号ACON
                        orderListDraft.OrderBankAccouontName = data.PayBankAccountName;//付款账号户名

                        orderListDraft.CollectBankName = data.CollectionBank;//收款账号开户行
                        orderListDraft.CollectBankAccouont = data.CollectionAccount;//收款账号OPAC
                        orderListDraft.CollectBankAccountName = data.CollectionBankAccountName;//收款账号户名
                        orderListDraft.CollectBankNo = data.CollectionBankAccount;//收款银行行号

                        orderListDraft.OrderCompany = orderCompany;//订单抬头
                        orderListDraft.PaymentMethod = data.PaymentMethod;
                        orderListDraft.PaymentCompany = data.CollectionCompanyName;//收款人/单位/公司
                        orderListDraft.PaymentContents = OrderListAPI.Summary;
                        orderListDraft.FillingDate = DateTime.Now;
                        orderListDraft.Founder = OrderListAPI.Sponsor;//发起人
                        orderListDraft.Payee = OrderListAPI.Sponsor;//发起人
                        orderListDraft.Money = OrderListAPI.Amount.TryToDecimal();
                        orderListDraft.CapitalizationMoney = MoneyToUpper(OrderListAPI.Amount);
                        //OrderListAPI.Mode = data.Mode;
                        //OrderListAPI.VehicleType = data.VehicleType;
                        orderListDraft.SubmitDate = DateTime.Now;
                        orderListDraft.InvoiceNumber = OrderListAPI.invoiceNumber;//发票
                        orderListDraft.AttachmentNumber = OrderListAPI.numberOfAttachments;//附件
                        orderListDraft.VGUID = guid;
                        orderListDraft.Status = "1";
                        orderListDraft.CreateTime = DateTime.Now;
                        _db.Insertable<Business_OrderListDraft>(orderListDraft).ExecuteCommand();
                        results = guid.TryToString();
                        Ifsuccess = true;
                    }
                    else
                    {
                        errmsg = "在订单配置表中找不到信息";
                    }
                }
                return Json(new
                {
                    success = Ifsuccess,
                    errmsg = errmsg,
                    result = results
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
                return Json(new
                {
                    success = false,
                    errmsg = ex.Message.ToString(),
                    result = results
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 金额转换成中文大写金额
        /// </summary>
        /// <param name="LowerMoney">eg:10.74</param>
        /// <returns></returns>
        public static string MoneyToUpper(string LowerMoney)
        {
            string functionReturnValue = null;
            bool IsNegative = false; // 是否是负数
            if (LowerMoney.Trim().Substring(0, 1) == "-")
            {
                // 是负数则先转为正数
                LowerMoney = LowerMoney.Trim().Remove(0, 1);
                IsNegative = true;
            }
            string strLower = null;
            string strUpart = null;
            string strUpper = null;
            int iTemp = 0;
            // 保留两位小数 123.489→123.49　　123.4→123.4
            LowerMoney = Math.Round(double.Parse(LowerMoney), 2).ToString();
            if (LowerMoney.IndexOf(".") > 0)
            {
                if (LowerMoney.IndexOf(".") == LowerMoney.Length - 2)
                {
                    LowerMoney = LowerMoney + "0";
                }
            }
            else
            {
                LowerMoney = LowerMoney + ".00";
            }
            strLower = LowerMoney;
            iTemp = 1;
            strUpper = "";
            while (iTemp <= strLower.Length)
            {
                switch (strLower.Substring(strLower.Length - iTemp, 1))
                {
                    case ".":
                        strUpart = "圆";
                        break;
                    case "0":
                        strUpart = "零";
                        break;
                    case "1":
                        strUpart = "壹";
                        break;
                    case "2":
                        strUpart = "贰";
                        break;
                    case "3":
                        strUpart = "叁";
                        break;
                    case "4":
                        strUpart = "肆";
                        break;
                    case "5":
                        strUpart = "伍";
                        break;
                    case "6":
                        strUpart = "陆";
                        break;
                    case "7":
                        strUpart = "柒";
                        break;
                    case "8":
                        strUpart = "捌";
                        break;
                    case "9":
                        strUpart = "玖";
                        break;
                }

                switch (iTemp)
                {
                    case 1:
                        strUpart = strUpart + "分";
                        break;
                    case 2:
                        strUpart = strUpart + "角";
                        break;
                    case 3:
                        strUpart = strUpart + "";
                        break;
                    case 4:
                        strUpart = strUpart + "";
                        break;
                    case 5:
                        strUpart = strUpart + "拾";
                        break;
                    case 6:
                        strUpart = strUpart + "佰";
                        break;
                    case 7:
                        strUpart = strUpart + "仟";
                        break;
                    case 8:
                        strUpart = strUpart + "万";
                        break;
                    case 9:
                        strUpart = strUpart + "拾";
                        break;
                    case 10:
                        strUpart = strUpart + "佰";
                        break;
                    case 11:
                        strUpart = strUpart + "仟";
                        break;
                    case 12:
                        strUpart = strUpart + "亿";
                        break;
                    case 13:
                        strUpart = strUpart + "拾";
                        break;
                    case 14:
                        strUpart = strUpart + "佰";
                        break;
                    case 15:
                        strUpart = strUpart + "仟";
                        break;
                    case 16:
                        strUpart = strUpart + "万";
                        break;
                    default:
                        strUpart = strUpart + "";
                        break;
                }

                strUpper = strUpart + strUpper;
                iTemp = iTemp + 1;
            }

            strUpper = strUpper.Replace("零拾", "零");
            strUpper = strUpper.Replace("零佰", "零");
            strUpper = strUpper.Replace("零仟", "零");
            strUpper = strUpper.Replace("零零零", "零");
            strUpper = strUpper.Replace("零零", "零");
            strUpper = strUpper.Replace("零角零分", "整");
            strUpper = strUpper.Replace("零分", "整");
            strUpper = strUpper.Replace("零角", "零");
            strUpper = strUpper.Replace("零亿零万零圆", "亿圆");
            strUpper = strUpper.Replace("亿零万零圆", "亿圆");
            strUpper = strUpper.Replace("零亿零万", "亿");
            strUpper = strUpper.Replace("零万零圆", "万圆");
            strUpper = strUpper.Replace("零亿", "亿");
            strUpper = strUpper.Replace("零万", "万");
            strUpper = strUpper.Replace("零圆", "圆");
            strUpper = strUpper.Replace("零零", "零");

            // 对壹圆以下的金额的处理
            if (strUpper.Substring(0, 1) == "圆")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper.Substring(0, 1) == "零")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper.Substring(0, 1) == "角")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper.Substring(0, 1) == "分")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper.Substring(0, 1) == "整")
            {
                strUpper = "零圆整";
            }
            functionReturnValue = strUpper;

            if (IsNegative == true)
            {
                return "负" + functionReturnValue;
            }
            else
            {
                return functionReturnValue;
            }
        }
    }
}