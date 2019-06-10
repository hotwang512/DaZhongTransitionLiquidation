using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BusinessTypeSet;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderListDetail
{
    public class OrderListDetailController : BaseController
    {
        public OrderListDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: CapitalCenterManagement/OrderListDetail
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            ViewBag.PayAccount = GetCompanyBankInfo();
            ViewBag.OrderBaseType = GetOrderBaseType();
            ViewBag.UserCompanySet = GetUserCompanySets();
            ViewBag.AccountMode = GetAccountMode();
            return View();
        }
        public JsonResult SaveOrderListDetail(Business_OrderList sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var guid = new Guid(sevenSection.CollectionCompany);
                    sevenSection.CollectionCompanyName = db.Queryable<Business_CustomerBankInfo>().Single(x => x.VGUID == guid).CompanyOrPerson;
                    var isAny = db.Queryable<Business_OrderList>().Any(x => x.VGUID == sevenSection.VGUID);
                    if (!isAny)
                    {
                        //sevenSection.VGUID = Guid.NewGuid();
                        sevenSection.CreateTime = DateTime.Now;
                        db.Insertable(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        db.Updateable(sevenSection).ExecuteCommand();
                    }
                    //账套公司各项配置
                    //var gjson = sevenSection.OrderDetailValue.JsonToModel<List<Business_UserCompanySetDetail>>();
                    //db.Deleteable<Business_UserCompanySetDetail>(x => x.OrderVGUID == sevenSection.VGUID.TryToString()).ExecuteCommand();
                    //foreach (var item in gjson)
                    //{
                    //    item.OrderVGUID = sevenSection.VGUID.TryToString();
                    //    item.VGUID = Guid.NewGuid();
                    //}
                    //db.Insertable<Business_UserCompanySetDetail>(gjson).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult GetOrderListDetail(Guid vguid)
        {
            Business_OrderList orderList = new Business_OrderList();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.SqlQueryable<Business_OrderList>(@" select a.BusinessSubItem1,a.BusinessProject,a.VGUID,b.BusinessType,b.Founder,b.Status,b.CollectionCompany,b.Number,b.PaymentMethod,b.AttachmentNumber,
b.InvoiceNumber,b.CollectionAccount,b.CollectionBankAccount,b.CollectionBankAccountName,b.CollectionBank,b.CompanyCode,b.OrderDetailValue from v_Business_BusinessTypeSet as a
left join Business_OrderList as b on a.VGUID = b.OrderDetailValue").Single(x => x.VGUID == vguid);
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult GetCollectionCompany()
        {
            List<Business_CustomerBankInfo> orderList = new List<Business_CustomerBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_CustomerBankInfo>().PartitionBy(it => new { it.CompanyOrPerson }).Take(1).ToList();
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult GetBusinessType()
        {
            List<Business_BusinessType> orderList = new List<Business_BusinessType>();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_BusinessType>().ToList();
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult GetCompanyChange(Guid CollectionCompany)
        {
            List<Business_CustomerBankInfo> orderList = new List<Business_CustomerBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_CustomerBankInfo>().Where(x => x.VGUID == CollectionCompany).ToList();
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult GetCollectionBankChange(string CollectionCompany)
        {
            List<Business_CustomerBankInfo> orderList = new List<Business_CustomerBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_CustomerBankInfo>().Where(x => x.CompanyOrPerson == CollectionCompany).ToList();
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult SaveBusinessTypeName(string BusinessTypeName, string BusinessVGUID)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (BusinessVGUID == "")
                    {
                        Business_BusinessType bus = new Business_BusinessType();
                        bus.ListKey = BusinessTypeName;
                        bus.BusinessTypeName = BusinessTypeName;
                        bus.VGUID = Guid.NewGuid();
                        db.Insertable(bus).ExecuteCommand();
                    }
                    else
                    {
                        Guid VGUID = new Guid(BusinessVGUID);
                        db.Updateable<Business_BusinessType>().UpdateColumns(it => new Business_BusinessType()
                        {
                            ListKey = BusinessTypeName,
                            BusinessTypeName = BusinessTypeName,
                        }).Where(it => it.VGUID == VGUID).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public List<Business_CompanyBankInfo> GetCompanyBankInfo()
        {
            var result = new List<Business_CompanyBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                var cache = CacheManager<Sys_User>.GetInstance();
                var loginCompany = cache[PubGet.GetUserKey].CompanyCode;
                result = db.Queryable<Business_CompanyBankInfo>().Where(x => x.CompanyCode == loginCompany).OrderBy("BankStatus desc").ToList();
            });
            return result;
        }
        public JsonResult GetPayBankInfo(string companyCode, string accountModeCode)
        {
            var result = new List<Business_CompanyBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                //var currentDepartment = UserInfo.Department;
                //var mainDepVguid = db.Queryable<Master_Organization>().Where(i => i.ParentVguid == null).Select(i => i.Vguid).Single();
                result = db.Queryable<Business_CompanyBankInfo>().Where(x => x.CompanyCode == companyCode && x.AccountModeCode == accountModeCode).OrderBy("BankStatus desc").ToList();
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBankInfo(string PayBank)
        {
            var result = new Business_CompanyBankInfo();
            DbBusinessDataService.Command(db =>
            {
                var cache = CacheManager<Sys_User>.GetInstance();
                var loginCompany = cache[PubGet.GetUserKey].CompanyCode;
                result = db.Queryable<Business_CompanyBankInfo>().Where(x => x.CompanyCode == loginCompany && x.BankName == PayBank).First();
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public List<CS_Master_2> GetOrderBaseType()
        {
            var result = new List<CS_Master_2>();
            DbService.Command(db =>
            {
                //var cache = CacheManager<Sys_User>.GetInstance();
                //var loginCompany = cache[PubGet.GetUserKey].CompanyCode;
                var vguid = "F8B45774-1B8B-47FC-9E3F-6C25E0F4D0B5".TryToGuid();
                result = db.Queryable<CS_Master_2>().Where(x => x.VGUID == vguid).ToList();
            });
            return result;
        }
        public JsonResult GetBusinessTypeTree()
        {
            var organizations = new List<Business_BusinessTypeSet>();
            DbBusinessDataService.Command(db =>
            {
                var userVGUID = UserInfo.Vguid;
                if(UserInfo.LoginName == "admin")
                {
                    organizations = db.Queryable<Business_BusinessTypeSet>().OrderBy("Code asc").ToList();
                }
                else
                {
                    //账号权限下的版块（出租/租赁）
                    var data = db.SqlQueryable<Business_BusinessTypeSet>(@"select * from dbo.Business_BusinessTypeSet where Code = (
     select BusinessCode from Business_UserCompanySet where UserVGUID = '" + userVGUID + "' and IsCheck='1' and Block='2')").First();
                    organizations.Add(data);
                    GetBusinessTree(data.VGUID, organizations);
                }
            });
            return Json(organizations, JsonRequestBehavior.AllowGet);
        }
        public void GetBusinessTree(Guid Vguid, List<Business_BusinessTypeSet> organizations)
        {
            DbBusinessDataService.Command(db =>
            {
                var datas = db.Queryable<Business_BusinessTypeSet>().Where(x => x.ParentVGUID == Vguid.TryToString()).OrderBy("Code asc").ToList();
                foreach (var item in datas)
                {
                    organizations.Add(item);
                    GetBusinessTree(item.VGUID, organizations);
                }
            });
        }
        public JsonResult GetUserCompanySet(string orderVguid)
        {
            var result = new List<UserCompanySetDetail>();
            DbBusinessDataService.Command(db =>
            {
                var cache = CacheManager<Sys_User>.GetInstance();
                var UserVGUID = cache[PubGet.GetUserKey].Vguid;
                if (UserInfo.LoginName == "admin")
                {
                    result = db.SqlQueryable<UserCompanySetDetail>(@"select a.Code,a.Descrption,a.CompanyCode,a.CompanyName,a.VGUID as Guids,a.KeyData as KeyDatas ,b.* from Business_UserCompanySet as a
left join Business_UserCompanySetDetail as b on b.KeyData = a.KeyData where a.UserVGUID='" + UserVGUID + "' and b.OrderVGUID = '" + orderVguid + @"'  and a.Block='1' ").OrderBy("Code asc,CompanyCode asc").ToList();
                }
                else
                {
                    result = db.SqlQueryable<UserCompanySetDetail>(@"select a.Code,a.Descrption,a.CompanyCode,a.CompanyName,a.VGUID as Guids,a.KeyData as KeyDatas ,b.* from Business_UserCompanySet as a
left join Business_UserCompanySetDetail as b on b.KeyData = a.KeyData where a.UserVGUID='" + UserVGUID + @"' and b.OrderVGUID = '" + orderVguid + @"' and a.IsCheck='1' and a.Block='1' ").OrderBy("Code asc,CompanyCode asc").ToList();
                }   
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public List<UserCompanySetDetail> GetUserCompanySets()
        {
            var result = new List<UserCompanySetDetail>();
            DbBusinessDataService.Command(db =>
            {
                var cache = CacheManager<Sys_User>.GetInstance();
                var UserVGUID = cache[PubGet.GetUserKey].Vguid;
                if (UserInfo.LoginName == "admin")
                {
                    result = db.SqlQueryable<UserCompanySetDetail>(@"select a.Code,a.Descrption,a.CompanyCode,a.CompanyName,a.VGUID as Guids,a.KeyData as KeyDatas
from Business_UserCompanySet as a where a.Block='1' and a.UserVGUID='" + UserVGUID + "'").OrderBy("Code asc,CompanyCode asc").ToList();
                }
                else
                {
                    result = db.SqlQueryable<UserCompanySetDetail>(@"select a.Code,a.Descrption,a.CompanyCode,a.CompanyName,a.VGUID as Guids,a.KeyData as KeyDatas ,b.VGUID,b.Isable,b.PayBank,b.PayAccount,b.PayBankAccountName,b.Borrow,b.Loan,b.KeyData,b.OrderVGUID from Business_UserCompanySet as a
left join Business_UserCompanySetDetail as b on b.KeyData = a.KeyData where a.UserVGUID='" + UserVGUID + @"' and a.IsCheck='1' and a.Block='1'").OrderBy("Code asc,CompanyCode asc").ToList();
                }         
            });
            return result;
        }
        public JsonResult GetSubjectBalance(string companyCode, string accountModeCode, GridParams para)
        {
            var jsonResult = new List<v_Business_SubjectSettingInfo>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult = db.SqlQueryable<v_Business_SubjectSettingInfo>(@"select a.*,b.Balance from v_Business_SubjectSettingInfo as a
                               left join Business_SubjectBalance as b on b.Code = a.BusinessCode
                               where SubjectVGUID='B63BD715-C27D-4C47-AB66-550309794D43' and AccountModeCode='" + accountModeCode + "' and SubjectCode='" + companyCode + "'")
                               .ToPageList(para.pagenum, para.pagesize, ref pageCount);
                //jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSettingTable(string OrderVGUID)
        {
            var result = new List<Business_UserCompanySetDetail>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_UserCompanySetDetail>().Where(x => x.OrderVGUID == OrderVGUID).ToList();
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveUserCompanySet(Business_UserCompanySetDetail sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "1" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    sevenSection.AccountModeName = db.Queryable<Business_SevenSection>().Single(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == sevenSection.AccountModeCode).Descrption;
                    sevenSection.CompanyName = db.Queryable<Business_SevenSection>().Single(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == sevenSection.AccountModeCode && x.Code == sevenSection.CompanyCode).Descrption;
                    var isAny = db.Queryable<Business_UserCompanySetDetail>().Any(x => x.OrderVGUID == sevenSection.OrderVGUID && x.CompanyName == sevenSection.CompanyName && x.VGUID != sevenSection.VGUID);
                    if (isAny)
                    {
                        resultModel.Status = "0";
                        return;
                    }
                    if (sevenSection.VGUID == Guid.Empty)
                    {
                        sevenSection.VGUID = Guid.NewGuid();
                        db.Insertable(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        db.Updateable(sevenSection).IgnoreColumns(it => new { it.Isable }).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                if(resultModel.Status != "0")
                {
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult UpdataIsable(Guid vguids,bool ischeck)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    db.Updateable<Business_UserCompanySetDetail>().UpdateColumns(it => new Business_UserCompanySetDetail()
                    { Isable = ischeck }).Where(it => it.VGUID == vguids).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}