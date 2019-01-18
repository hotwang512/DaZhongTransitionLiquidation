using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
                    if (sevenSection.VGUID == Guid.Empty)
                    {
                        sevenSection.VGUID = Guid.NewGuid();
                        sevenSection.CreateTime = DateTime.Now;
                        db.Insertable<Business_OrderList>(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        db.Updateable<Business_OrderList>(sevenSection).ExecuteCommand();
                    }
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
                orderList = db.Queryable<Business_OrderList>().Single(x => x.VGUID == vguid);
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult GetCollectionCompany()
        {
            List<Business_CustomerBankInfo> orderList = new List<Business_CustomerBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_CustomerBankInfo>().ToList();
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
                orderList = db.Queryable<Business_CustomerBankInfo>().Where(x=>x.VGUID == CollectionCompany).ToList();
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult SaveBusinessTypeName(string BusinessTypeName,string BusinessVGUID)
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
        public JsonResult GetBankInfo(string PayBank)
        {
            var result = new Business_CompanyBankInfo();
            DbBusinessDataService.Command(db =>
            {
                var cache = CacheManager<Sys_User>.GetInstance();
                var loginCompany = cache[PubGet.GetUserKey].CompanyCode;
                result = db.Queryable<Business_CompanyBankInfo>().Where(x => x.CompanyCode == loginCompany && x.BankName == PayBank).First();
            });
            return Json(result, JsonRequestBehavior.AllowGet); ;
        }
    }
}