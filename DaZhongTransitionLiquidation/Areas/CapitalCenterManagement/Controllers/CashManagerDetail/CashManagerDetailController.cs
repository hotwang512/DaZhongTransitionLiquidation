﻿using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CashManagerDetail
{
    public class CashManagerDetailController : BaseController
    {
        public CashManagerDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/CashManagerDetail
        public ActionResult Index()
        {
            ViewBag.GetAccountMode = GetAccountModes();
            ViewBag.GetGuid = Guid.NewGuid().TryToString();
            return View();
        }
        public List<Business_SevenSection> GetAccountModes()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public JsonResult GetBankInfo(string accountMode, string companyCode)
        {
            var jsonResult = new Business_CompanyBankInfo();
            DbBusinessDataService.Command(db =>
            {
                jsonResult = db.Queryable<Business_CompanyBankInfo>().Where(x=>x.AccountModeCode == accountMode && x.CompanyCode == companyCode && x.AccountType == "基本户").First();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCashManagerInfo(Guid vguid)
        {
            Business_CashManagerInfo orderList = new Business_CashManagerInfo();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_CashManagerInfo>().Single(x=>x.VGUID == vguid);
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult SaveCashManagerDetail(Business_CashManagerInfo sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var isAny = db.Queryable<Business_CashManagerInfo>().Any(x => x.VGUID == sevenSection.VGUID);
                    if (!isAny)
                    {
                        var cash = "Cash" + UserInfo.AccountModeCode + UserInfo.CompanyCode;
                        //2019110001--备用金
                        var no = CreateNo.GetCreateCashNo(db, cash);
                        //sevenSection.VGUID = sevenSection.VGUID;
                        sevenSection.No = "BYJ" + no;
                        sevenSection.Status = "1";
                        sevenSection.CreateTime = DateTime.Now;
                        sevenSection.Founder = UserInfo.LoginName;
                        db.Insertable(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        sevenSection.ChangeTime = DateTime.Now;
                        sevenSection.Changer = UserInfo.LoginName;
                        db.Updateable(sevenSection).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}