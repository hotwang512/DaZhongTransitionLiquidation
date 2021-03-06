﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.PurchaseFaxOrderSetting
{
    public class PurchaseFaxOrderSettingDetailController : BaseController
        {
            // GET: SystemManagement/PurchaseFaxOrderSettingDetail
            public PurchaseFaxOrderSettingDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
            {
            }

            public ActionResult Index()
            {
                ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
                return View();
            }

            public JsonResult SavePurchaseOrderSetting(PurchaseFaxOrderSettingModel saveModel)
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
                            saveModel.CreateUser = cache[PubGet.GetUserKey].LoginName;
                            db.Insertable<Business_PurchaseFaxOrderSetting>(saveModel).ExecuteCommand();
                            if (saveModel.DepartmentModelList != null)
                            {
                                foreach (var departmentModel in saveModel.DepartmentModelList)
                                {
                                    var depModal = new Business_PurchaseDepartment();
                                    depModal.VGUID = Guid.NewGuid();
                                    depModal.PurchaseOrderSettingVguid = saveModel.VGUID;
                                    depModal.DepartmentName = departmentModel.Descrption;
                                    depModal.DepartmentVguid = departmentModel.VGUID;
                                    depModal.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                    depModal.CreateDate = DateTime.Now;
                                    db.Insertable<Business_PurchaseDepartment>(depModal).ExecuteCommand();
                                }
                            }
                            foreach (var managementCompany in saveModel.ManagementCompanyList)
                            {
                                var managementCompanyModal = new Business_PurchaseManagementCompany();
                                managementCompanyModal.VGUID = Guid.NewGuid();
                                managementCompanyModal.PurchaseOrderSettingVguid = saveModel.VGUID;
                                managementCompanyModal.AccountModeCode = managementCompany.AccountModeCode;
                                managementCompanyModal.CompanyCode = managementCompany.CompanyCode;
                                managementCompanyModal.KeyData = managementCompany.KeyData;
                                managementCompanyModal.Descrption = managementCompany.Descrption;
                                managementCompanyModal.ManagementCompany = managementCompany.ManagementCompany;
                                managementCompanyModal.IsCheck = managementCompany.IsCheck;
                                managementCompanyModal.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                managementCompanyModal.CreateDate = DateTime.Now;
                                db.Insertable<Business_PurchaseManagementCompany>(managementCompanyModal).ExecuteCommand();
                            }
                        }
                        else
                        {
                            saveModel.ChangeDate = DateTime.Now;
                            saveModel.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                            db.Updateable<Business_PurchaseFaxOrderSetting>(saveModel)
                                .IgnoreColumns(x => new { x.CreateDate, x.CreateUser }).ExecuteCommand();
                            db.Deleteable<Business_PurchaseDepartment>()
                                .Where(c => c.PurchaseOrderSettingVguid == saveModel.VGUID).ExecuteCommand();
                            foreach (var departmentModel in saveModel.DepartmentModelList)
                            {
                                var depModal = new Business_PurchaseDepartment();
                                depModal.VGUID = Guid.NewGuid();
                                depModal.PurchaseOrderSettingVguid = saveModel.VGUID;
                                depModal.DepartmentName = departmentModel.Descrption;
                                depModal.DepartmentVguid = departmentModel.VGUID;
                                depModal.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                depModal.CreateDate = DateTime.Now;
                                db.Insertable<Business_PurchaseDepartment>(depModal).ExecuteCommand();
                            }
                            db.Deleteable<Business_PurchaseManagementCompany>(x => x.PurchaseOrderSettingVguid == saveModel.VGUID).ExecuteCommand();
                            foreach (var managementCompany in saveModel.ManagementCompanyList)
                            {
                                var managementCompanyModal = new Business_PurchaseManagementCompany();
                                managementCompanyModal.VGUID = Guid.NewGuid();
                                managementCompanyModal.PurchaseOrderSettingVguid = saveModel.VGUID;
                                managementCompanyModal.AccountModeCode = managementCompany.AccountModeCode;
                                managementCompanyModal.CompanyCode = managementCompany.CompanyCode;
                                managementCompanyModal.KeyData = managementCompany.KeyData;
                                managementCompanyModal.Descrption = managementCompany.Descrption;
                                managementCompanyModal.ManagementCompany = managementCompany.ManagementCompany;
                                managementCompanyModal.IsCheck = managementCompany.IsCheck;
                                managementCompanyModal.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                managementCompanyModal.CreateDate = DateTime.Now;
                                db.Insertable<Business_PurchaseManagementCompany>(managementCompanyModal).ExecuteCommand();
                            }
                        }
                    });
                    resultModel.IsSuccess = result.IsSuccess;
                    resultModel.ResultInfo = result.ErrorMessage;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                });
                return Json(resultModel);
            }
            /// <summary>
            /// 供应商配置
            /// </summary>
            /// <param name="vguid"></param>
            /// <returns></returns>
            public JsonResult GetPurchaseFaxOrderSettingDetail(Guid vguid)
            {
                var resultModel = new ResultModel<Business_PurchaseFaxOrderSetting, List<PurchaseDepartmentModel>>() { IsSuccess = false, Status = "0" };
                DbBusinessDataService.Command(db =>
                {
                    //主信息
                    resultModel.ResultInfo = db.Queryable<Business_PurchaseFaxOrderSetting>().Single(x => x.VGUID == vguid);
                    resultModel.ResultInfo2 = db.SqlQueryable<PurchaseDepartmentModel>(@"SELECT *, CASE
                   WHEN bpd.DepartmentVguid IS NULL THEN
                       'NoCheck'
                   ELSE
                       'Checked'
                           END AS IsCheck
                    FROM
                    (
                        SELECT VGUID,Descrption
                        FROM Business_SevenSection
                        WHERE SectionVGUID = 'D63BD715-C27D-4C47-AB66-550309794D43'
                              AND AccountModeCode = '1002'
                              AND CompanyCode = '01'
                              AND Status = '1'
                              AND Code LIKE '10%'
                    ) AS bss
                        RIGHT JOIN
                        (
                            SELECT DepartmentVguid
                            FROM Business_PurchaseDepartment
                            WHERE PurchaseOrderSettingVguid = '" + vguid + @"'
                        ) bpd
                    ON bpd.DepartmentVguid = bss.VGUID").ToList();
                });
                resultModel.IsSuccess = true;
                resultModel.Status = "1";
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
            /// <summary>
            /// 资产分类
            /// </summary>
            /// <param name="MAJOR"></param>
            /// <returns></returns>
            public JsonResult GetMinorListDatas(string MAJOR)
            {
                var list = new List<MinorListData>();
                DbBusinessDataService.Command(db =>
                {
                    list = db.Queryable<Business_AssetsCategory>().WhereIF(MAJOR != null, i => i.ASSET_CATEGORY_MAJOR == MAJOR).Select(c => new MinorListData { AssetMinor = c.ASSET_CATEGORY_MINOR, AssetMinorVguid = c.VGUID }).ToList();
                });
                var result = list.GroupBy(c => new { c.AssetMinor, c.AssetMinorVguid }).Select(c => c.Key).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            public JsonResult GetPurchaseManagementCompanyData(Guid? VGUID)
            {
                var response = new List<Business_PurchaseManagementCompany>();
                DbBusinessDataService.Command(db =>
                {
                    var isExits = db.Queryable<Business_PurchaseManagementCompany>().Any(i => i.PurchaseOrderSettingVguid == VGUID);
                    if (!isExits)
                    {
                        response = db.SqlQueryable<Business_PurchaseManagementCompany>(@"SELECT 
                        t1.Code AS AccountModeCode,
                               t1.Descrption,
                               t2.Code AS CompanyCode,
                               t2.Descrption AS ManagementCompany,
                               (t1.Code + t2.Code) AS KeyData
                        FROM Business_SevenSection t1
                            JOIN Business_SevenSection t2
                                ON t1.Code = t2.AccountModeCode
                        WHERE t1.SectionVGUID = 'H63BD715-C27D-4C47-AB66-550309794D43'
                              AND t2.SectionVGUID = 'A63BD715-C27D-4C47-AB66-550309794D43'
                        ").OrderBy("AccountModeCode asc,CompanyCode asc").ToList();
                    }
                    else
                    {
                        response = db.SqlQueryable<Business_PurchaseManagementCompany>(@"SELECT t1.Code AS AccountModeCode,
                               t1.Descrption,
                               t2.Code AS CompanyCode,
                               t2.Descrption AS ManagementCompany,
                               (t1.Code + t2.Code) AS KeyData,
	                           pmc.Ischeck
                        FROM Business_SevenSection t1
                            JOIN Business_SevenSection t2
                                ON t1.Code = t2.AccountModeCode
                            LEFT JOIN Business_PurchaseManagementCompany pmc
                                ON pmc.AccountModeCode = t2.AccountModeCode
                                   AND pmc.CompanyCode = t2.Code
                        WHERE t1.SectionVGUID = 'H63BD715-C27D-4C47-AB66-550309794D43'
                              AND t2.SectionVGUID = 'A63BD715-C27D-4C47-AB66-550309794D43'
                        ").OrderBy("AccountModeCode asc,CompanyCode asc").ToList();
                    }

                });
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            public JsonResult GetPurchaseDepartmentListDatas()
            {
                var list = new List<PurchaseDepartmentModel>();
                DbBusinessDataService.Command(db =>
                {
                    list = db.SqlQueryable<PurchaseDepartmentModel>(@"SELECT VGUID,Descrption
                    FROM Business_SevenSection
                    WHERE SectionVGUID = 'D63BD715-C27D-4C47-AB66-550309794D43'
                          AND AccountModeCode = '1002'
                          AND CompanyCode = '01'
                          AND Status = '1'
                          AND Code LIKE '10%'").ToList();
                });

                return Json(list, JsonRequestBehavior.AllowGet);
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
        }
}