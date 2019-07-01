using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.VehicleExtrasFeeSetting
{
    public class VehicleExtrasFeeSettingDetailController : BaseController
    {
        // GET: SystemManagement/VehicleExtrasFeeSettingDetail
        public VehicleExtrasFeeSettingDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult SaveVehicleExtrasFeeSetting(Business_VehicleExtrasFeeSetting saveModel)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (saveModel.VGUID == Guid.Empty)
                    {
                        saveModel.VGUID = Guid.NewGuid();
                        saveModel.CreateDate = DateTime.Now;
                        saveModel.CreateUser = cache[PubGet.GetUserKey].UserName;
                        db.Insertable<Business_VehicleExtrasFeeSetting>(saveModel).ExecuteCommand();
                    }
                    else
                    {
                        saveModel.ChangeDate = DateTime.Now;
                        saveModel.ChangeUser = cache[PubGet.GetUserKey].UserName;
                        db.Updateable<Business_VehicleExtrasFeeSetting>(saveModel)
                            .IgnoreColumns(x => new { x.CreateDate, x.CreateUser }).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        public JsonResult GetBusinessProject(string BusinessProject)
        {
            var list = new List<BusinessProjectModel>();
            DbBusinessDataService.Command(db =>
            {
                list = BusinessProject != "" ? db.SqlQueryable<BusinessProjectModel>(@"SELECT BusinessSubItem1,BusinessProject FROM v_Business_BusinessTypeSet WHERE BusinessProject LIKE '%" + BusinessProject + "%'").ToList()
                    : db.SqlQueryable<BusinessProjectModel>(@"SELECT BusinessSubItem1,BusinessProject FROM v_Business_BusinessTypeSet").ToList();
            });

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehicleExtrasFeeSettingDetail(Guid Vguid)
        {
            var resultModel = new ResultModel<Business_VehicleExtrasFeeSetting>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                //主信息
                resultModel.ResultInfo = db.Queryable<Business_VehicleExtrasFeeSetting>().Single(x => x.VGUID == Vguid);
                resultModel.IsSuccess = true;
                resultModel.Status = "1";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehicleModelDropDown()
        {
            var list = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                list = db.SqlQueryable<Business_SevenSection>(@"SELECT * FROM Business_SevenSection WHERE SectionVGUID = 'F63BD715-C27D-4C47-AB66-550309794D43'
AND AccountModeCode = '1002' AND status = 1 AND CompanyCode = '01' AND code LIKE '10%'").ToList();
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}