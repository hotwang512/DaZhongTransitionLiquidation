using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.ComeOnApplication
{
    public class ComeOnApplicationController : BaseController
    {
        public ComeOnApplicationController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/ComeOnApplication
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("93ce22ba-a4f8-41d9-bdcd-2adac74b6372");
            return View();
        }
        public JsonResult GetComeOnApplicationData(Business_ComeOnAllocationInfo searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_ComeOnAllocationInfo>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_ComeOnAllocationInfo>()
                .Where(i=>i.TurnInAccountModeCode == UserInfo.AccountModeCode && i.TurnOutAccountModeCode == UserInfo.AccountModeCode)
                .WhereIF(searchParams.TurnInCompanyCode != null, i => i.TurnInCompanyCode == searchParams.TurnInCompanyCode)
                .WhereIF(searchParams.TurnOutCompanyCode != null,i=> i.TurnOutCompanyCode == searchParams.TurnOutCompanyCode)
                .WhereIF(searchParams.ApplyDate != null, i => i.ApplyDate == searchParams.ApplyDate)
                .WhereIF(searchParams.Status != null, i => i.Status == searchParams.Status)
                .OrderBy(i => i.No, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCompanyInfo()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x=>x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == UserInfo.AccountModeCode).OrderBy("AccountModeCode asc").ToList();
            });
            return Json(result, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult DeleteComeOnApplication(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                foreach (var item in vguids)
                {
                    int saveChanges = 1;
                    //删除主表信息
                    saveChanges = db.Deleteable<Business_ComeOnAllocationInfo>(x => x.VGUID == item).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == 1;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult UpdataComeOnApplication(List<Guid> vguids, string status)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                var comeOn = db.Queryable<Business_ComeOnAllocationInfo>().Where(x=>x.Status == "2").ToList();
                foreach (var item in vguids)
                {
                    //更新主表信息
                    saveChanges = db.Updateable<Business_ComeOnAllocationInfo>().UpdateColumns(it => new Business_ComeOnAllocationInfo()
                    {
                        Status = status,
                    }).Where(it => it.VGUID == item).ExecuteCommand();
                    if(status == "3")
                    {
                        var comeOnOne = comeOn.Single(x => x.VGUID == item);
                        comeOnOne.TurnInMoney = comeOnOne.Money;
                        comeOnOne.Money = null;
                        comeOnOne.No = comeOnOne.No + "N";
                        comeOnOne.Status = "3";
                        comeOnOne.VGUID = Guid.NewGuid();
                        db.Insertable(comeOnOne).ExecuteCommand();
                    }
                }
                resultModel.IsSuccess = saveChanges == 1;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}