using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
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
        public JsonResult GetVehicleExtrasFeeSettingListDatas(Business_VehicleExtrasFeeSetting searchModel, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_VehicleExtrasFeeSettingShow>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                var setting = db.Queryable<Business_VehicleExtrasFeeSetting>()
                    .Where(i => i.VehicleModelCode == searchModel.VehicleModel)
                    .OrderBy(i => i.VehicleModelCode)
                    .OrderBy(i => i.BusinessSubItem, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                var settingShowList = new List<Business_VehicleExtrasFeeSettingShow>();
                foreach (var item in setting)
                {
                    var settingShow = new Business_VehicleExtrasFeeSettingShow();
                    settingShow.VGUID = item.VGUID;
                    settingShow.VehicleModelCode = item.VehicleModelCode;
                    settingShow.VehicleModel = item.VehicleModel;
                    settingShow.Fee = item.Fee;
                    settingShow.BusinessSubItem = item.BusinessSubItem;
                    settingShow.BusinessProject = item.BusinessProject;
                    settingShow.Status = item.Status;
                    settingShow.CreateDate = item.CreateDate;
                    settingShow.ChangeDate = item.ChangeDate;
                    settingShow.CreateUser = item.CreateUser;
                    settingShow.ChangeUser = item.ChangeUser;
                    settingShow.StartDate = item.StartDate;
                    var history = db.Queryable<Business_VehicleExtrasFeeSettingHistory>()
                        .Where(x => x.LGUID == item.VGUID).ToList().OrderByDescending(x => x.CreateDate).ToList();
                    settingShow.SettingHistoryList = history;
                    settingShowList.Add(settingShow);
                }
                jsonResult.Rows = settingShowList;
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveVehicleExtrasFeeSetting(SaveFeeSettingModel saveModel)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                foreach (var item in saveModel.FeeSettingList)
                {
                    item.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                    item.ChangeDate = DateTime.Now;
                    db.Updateable<Business_VehicleExtrasFeeSetting>(item).Where(x => x.VGUID == item.VGUID && (x.Fee != item.Fee || x.Status != item.Status))
                        .UpdateColumns(x => new { x.Fee, x.ChangeDate, x.ChangeUser, x.Status }).ExecuteCommand();
                }
                
                resultModel.IsSuccess = true;
                resultModel.ResultInfo = "保存成功";
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        public JsonResult GetBusinessProject(string BusinessProject)
        {
            var list = new List<BusinessProjectModel>();
            DbBusinessDataService.Command(db =>
            {
                list = BusinessProject != "" ? db.SqlQueryable<BusinessProjectModel>(@"SELECT BusinessSubItem1,BusinessProject FROM v_Business_BusinessTypeSet WHERE  BusinessSubItem1 LIKE 'cz|03|0301|%' AND BusinessSubItem1 != 'cz|03|0301|030101' and BusinessProject LIKE '%" + BusinessProject + "%'").ToList()
                    : db.SqlQueryable<BusinessProjectModel>(@"SELECT BusinessSubItem1,BusinessProject FROM v_Business_BusinessTypeSet Where  BusinessSubItem1 LIKE 'cz|03|0301|%' AND BusinessSubItem1 != 'cz|03|0301|030101'").ToList();
                foreach (var item in list)
                {
                    item.BusinessProject = item.BusinessProject.Substring(item.BusinessProject.LastIndexOf("|") + 1,
                        item.BusinessProject.Length - item.BusinessProject.LastIndexOf("|") - 1);
                }
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetVehicleExtrasFeeSettingDetail(string VehicleModel)
        {
            var resultModel = new ResultModel<List<Business_VehicleExtrasFeeSetting>>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var cache = CacheManager<Sys_User>.GetInstance();
                if (VehicleModel != "")
                {
                    var list = db.SqlQueryable<Business_SevenSection>(@"SELECT * FROM Business_SevenSection WHERE SectionVGUID = 'F63BD715-C27D-4C47-AB66-550309794D43'
AND AccountModeCode = '1002' AND status = 1 AND CompanyCode = '01' AND code LIKE '10%'").ToList();
                    var vDescrptionehicleModelName = list.Where(x => x.Code == VehicleModel).First().Descrption;
                    var feeSettingInsertList = new List<Business_VehicleExtrasFeeSetting>();
                    var feeSettingUpdateList = new List<Business_VehicleExtrasFeeSetting>();
                    var businessProjectList = db.SqlQueryable<BusinessProjectModel>(@"SELECT BusinessSubItem1,BusinessProject FROM v_Business_BusinessTypeSet Where  BusinessSubItem1 LIKE 'cz|03|0301|%' AND BusinessSubItem1 != 'cz|03|0301|030101'").ToList();
                    foreach (var item in businessProjectList)
                    {
                        item.BusinessProject = item.BusinessProject.Substring(item.BusinessProject.LastIndexOf("|") + 1,
                            item.BusinessProject.Length - item.BusinessProject.LastIndexOf("|") - 1);
                        var vehicleExtrasFeeSettingModel = new Business_VehicleExtrasFeeSetting();
                        vehicleExtrasFeeSettingModel.VGUID = Guid.NewGuid();
                        vehicleExtrasFeeSettingModel.VehicleModelCode = VehicleModel;
                        vehicleExtrasFeeSettingModel.VehicleModel = vDescrptionehicleModelName;
                        vehicleExtrasFeeSettingModel.Fee = 0;
                        vehicleExtrasFeeSettingModel.BusinessSubItem = item.BusinessSubItem1;
                        vehicleExtrasFeeSettingModel.BusinessProject = item.BusinessProject;
                        vehicleExtrasFeeSettingModel.Status = true;
                        vehicleExtrasFeeSettingModel.StartDate = DateTime.Now;
                        vehicleExtrasFeeSettingModel.CreateDate= DateTime.Now;
                        vehicleExtrasFeeSettingModel.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        if (!db.Queryable<Business_VehicleExtrasFeeSetting>().Any(x =>
                            x.BusinessSubItem == vehicleExtrasFeeSettingModel.BusinessSubItem && x.VehicleModelCode == VehicleModel))
                        {
                            feeSettingInsertList.Add(vehicleExtrasFeeSettingModel);
                        }
                        if (!db.Queryable<Business_VehicleExtrasFeeSetting>().Any(x =>
                            x.BusinessSubItem == vehicleExtrasFeeSettingModel.BusinessSubItem && x.VehicleModelCode == VehicleModel && x.BusinessProject == item.BusinessProject))
                        {
                            feeSettingUpdateList.Add(vehicleExtrasFeeSettingModel);
                        }
                    }
                    if (feeSettingInsertList.Count > 0)
                    {
                        db.Insertable<Business_VehicleExtrasFeeSetting>(feeSettingInsertList).ExecuteCommand();
                    }
                    if (feeSettingUpdateList.Count > 0)
                    {
                        foreach (var item in feeSettingUpdateList)
                        {
                            db.Updateable<Business_VehicleExtrasFeeSetting>()
                                .UpdateColumns(x => new Business_VehicleExtrasFeeSetting
                                    {BusinessProject = item.BusinessProject})
                                .Where(x => x.BusinessSubItem == item.BusinessSubItem).ExecuteCommand();
                        }
                    }
                }
                resultModel.IsSuccess = true;
                resultModel.Status = "1";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        //UpdateExtrasFeeSetting
        public JsonResult UpdateExtrasFeeSetting(Business_VehicleExtrasFeeSetting UpdateModel)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var feeSetting = db.Queryable<Business_VehicleExtrasFeeSetting>().Where(x => x.VGUID == UpdateModel.VGUID).First();
                if (feeSetting.Fee != UpdateModel.Fee)
                {
                    db.Updateable<Business_VehicleExtrasFeeSetting>().UpdateColumns(x =>
                        new Business_VehicleExtrasFeeSetting {Fee = UpdateModel.Fee, StartDate = UpdateModel.StartDate ,ChangeUser = UserInfo.UserName}).Where(x => x.VGUID == UpdateModel.VGUID).ExecuteCommand();
                    var history = Mapper.Map<Business_VehicleExtrasFeeSettingHistory>(feeSetting);
                    history.VGUID = Guid.NewGuid();
                    history.CreateDate = DateTime.Now;
                    db.Insertable<Business_VehicleExtrasFeeSettingHistory>(history).ExecuteCommand();
                }
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