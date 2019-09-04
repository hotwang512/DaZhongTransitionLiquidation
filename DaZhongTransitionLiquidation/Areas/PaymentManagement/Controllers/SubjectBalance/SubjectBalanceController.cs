using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.SubjectBalance
{
    public class SubjectBalanceController : BaseController
    {
        public SubjectBalanceController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: PaymentManagement/SubjectBalance
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("dcb08053-9a75-45f0-b5a6-02332d6dc7e3");
            ViewBag.CompanyCode = GetCompanyCode();
            ViewBag.AccountMode = GetAccountMode();
            return View();
        }
        public List<Business_SevenSection> GetCompanyCode()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public JsonResult GetSubjectBalance(string companyCode,string accountModeCode, string year , string month, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_Business_SubjectSettingInfo>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Ado.SqlQuery<v_Business_SubjectSettingInfo>("exec usp_SubjectSettingInfo @AccountModeCode,@CompanyCode,@Year,@Month",
                    new { AccountModeCode = accountModeCode, CompanyCode = companyCode, Year = year, Month = month }).ToList();
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveSubjectBalance(decimal Balance,string Code,string Year,string Month,string AccountModeCode,string CompanyCode)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if(Code != "")
                    {
                        db.Deleteable<Business_SubjectBalance>(x => x.Code == Code && x.Year == Year && x.Month == Month && x.AccountModeCode == AccountModeCode && x.CompanyCode == CompanyCode).ExecuteCommand();
                        Business_SubjectBalance balance = new Business_SubjectBalance();
                        balance.VGUID = Guid.NewGuid();
                        balance.Code = Code;
                        balance.Balance = Balance;
                        balance.Year = Year;
                        balance.Month = Month;
                        balance.AccountModeCode = AccountModeCode;
                        balance.CompanyCode = CompanyCode;
                        db.Insertable(balance).ExecuteCommand();
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