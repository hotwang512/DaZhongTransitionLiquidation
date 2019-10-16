using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Aspose.Cells;
using AutoMapper;
using DaZhongTransitionLiquidation.Common;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetsLedger
{
    public class AssetsLedgerController : BaseController
    {
        public AssetsLedgerController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetManagement/AssetsLedger
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("675f1c4c-5ad6-4930-b227-b31d19a98c05");
            return View();
        }
        public JsonResult GetAssetsLedgerListDatas(string PERIOD, string TagNumber, string CategoryMajor, string CategoryMinor, GridParams para)
        {
            var jsonResult = new JsonResultModel<AssetsLedger_Swap>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<AssetsLedger_Swap>()
                    .PartitionBy(it => new { it.ASSET_ID }).Take(1)
                    .WhereIF(PERIOD != null, i => i.PERIOD_CODE.Contains(PERIOD))
                    .WhereIF(TagNumber != null, i => i.TAG_NUMBER.Contains(TagNumber))
                    .WhereIF(CategoryMajor != null, i => i.ASSET_CATEGORY_MAJOR.Contains(CategoryMajor))
                    .WhereIF(CategoryMinor != null, i => i.ASSET_CATEGORY_MINOR.Contains(CategoryMinor))
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public FileResult ExportExcel(string PERIOD, string TagNumber, string CategoryMajor, string CategoryMinor)
        {
            DataTable dt = new DataTable();
            DbBusinessDataService.Command(db =>
            {
                dt = db.Queryable<AssetsLedger_Swap>()
                    .PartitionBy(it => new { it.ASSET_ID }).Take(1)
                    .WhereIF(PERIOD != null, i => i.PERIOD_CODE.Contains(PERIOD))
                    .WhereIF(TagNumber != null, i => i.TAG_NUMBER.Contains(TagNumber))
                    .WhereIF(CategoryMajor != null, i => i.ASSET_CATEGORY_MAJOR.Contains(CategoryMajor))
                    .WhereIF(CategoryMinor != null, i => i.ASSET_CATEGORY_MINOR.Contains(CategoryMinor))
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToDataTable();
            });
            dt.TableName = "AssetsLedger_Swap";
            var ms = ExcelHelper.OutModelFileToStream(dt, "/Template/AssetsLedger.xlsx", "资产台账");
            byte[] fileContents = ms.ToArray();
            return File(fileContents, "application/ms-excel", "资产台账" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
        }

        public JsonResult CheckData()
        {
            var resultModel = new ResultModel<string,List<AssetsLedger_SwapCompare>>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var listAsset = db.Queryable<Business_AssetMaintenanceInfo>().ToList();
                var listAssetTAG = listAsset.Select(x => new { TAG_NUMBER = x.TAG_NUMBER }).ToList();
                var listAssetLedgerTAG = db.Queryable<AssetsLedger_Swap>()
                    .Select(x => new { TAG_NUMBER = x.TAG_NUMBER }).ToList();
                //资产表存在，Oracle台账不存在
                var infoExcept = listAssetTAG.Except(listAssetLedgerTAG).ToList();
                var dt = infoExcept.TryToDataTable();
                //Oracle台账存在，资产表不存在
                var ledgerExcept = listAssetLedgerTAG.Except(listAssetTAG).ToList();
                var allExcept = infoExcept.Union(ledgerExcept).ToList();
                var allExceptList = new List<string>();
                foreach (var itemExcept in allExcept)
                {
                    allExceptList.Add(itemExcept.TAG_NUMBER);
                }
                if (allExcept.Count > 0)
                {
                    var exceptLedgerData = db.Queryable<AssetsLedger_Swap>().In(it => it.TAG_NUMBER, allExceptList).ToList();
                    var compareLedgerData = Mapper.Map<List<AssetsLedger_Swap>, List<AssetsLedger_SwapCompare>>(exceptLedgerData).ToList();
                    compareLedgerData.ForEach(x => x.Message = "主表标签号不存在");
                    var exceptaAssetData = db.Queryable<Business_AssetMaintenanceInfo>().In(it => it.TAG_NUMBER, allExceptList).ToList();
                    var compareAssetData = Mapper.Map<List<Business_AssetMaintenanceInfo>, List<AssetsLedger_SwapCompare>>(exceptaAssetData).ToList();
                    compareAssetData.ForEach(x => x.Message = "Oracle台账标签号不存在");
                    var compareData = compareLedgerData.Union(compareAssetData).ToList();
                    var data = compareData.TryToDataTable();
                    resultModel.IsSuccess = true;
                    resultModel.Status = "1";
                    resultModel.ResultInfo = "数量对比不一致";
                    resultModel.ResultInfo2 = compareData;
                }
                else
                {
                    var listLedger = db.Queryable<AssetsLedger_Swap>().ToList();
                    var compareData = Mapper.Map<List<AssetsLedger_Swap>, List<AssetsLedger_SwapCompare>>(listLedger).ToList();
                    var errorCount = 0;
                    foreach (var item in compareData)
                    {
                        if (listAsset.Any(x => x.TAG_NUMBER == item.TAG_NUMBER))
                        {
                            var assetInfo = listAsset.Where(x => x.TAG_NUMBER == item.TAG_NUMBER).First();
                            if (item.ASSET_ID != assetInfo.ASSET_ID)
                            {
                                item.Message += "资产编号不一致;";
                                errorCount++;
                            }
                            if (item.DESCRIPTION != assetInfo.DESCRIPTION)
                            {
                                item.Message += "说明不一致;";
                                errorCount++;
                            }
                            if (item.ASSET_CATEGORY_MAJOR != assetInfo.ASSET_CATEGORY_MAJOR)
                            {
                                item.Message += "资产主类不一致;";
                                errorCount++;
                            }
                            if (item.ASSET_CATEGORY_MINOR != assetInfo.ASSET_CATEGORY_MINOR)
                            {
                                item.Message += "资产子类不一致;";
                                errorCount++;
                            }
                            if (item.ASSET_CREATION_DATE != assetInfo.LISENSING_DATE)
                            {
                                item.Message += "上牌日期不一致;";
                                errorCount++;
                            }
                            if (item.ASSET_COST != assetInfo.ASSET_COST)
                            {
                                item.Message += "资产原值不一致;";
                                errorCount++;
                            }
                            //if (item.SALVAGE_TYPE != assetInfo.SALVAGE_TYPE)
                            //{
                            //    item.Message += "残值类型不一致;";
                            //    errorCount++;
                            //}
                            //if (item.SALVAGE_PERCENT != assetInfo.SALVAGE_PERCENT)
                            //{
                            //    item.Message += "残值百分比不一致;";
                            //    errorCount++;
                            //}
                            //if (item.SALVAGE_VALUE != assetInfo.SALVAGE_VALUE)
                            //{
                            //    item.Message += "残值金额不一致;";
                            //    errorCount++;
                            //}
                            if (item.YTD_DEPRECIATION != assetInfo.YTD_DEPRECIATION)
                            {
                                item.Message += "年累计折旧不一致;";
                                errorCount++;
                            }
                            if (item.ACCT_DEPRECIATION.ObjToDecimal() != assetInfo.ACCT_DEPRECIATION)
                            {
                                item.Message += "月累计折旧不一致;";
                                errorCount++;
                            }
                            //if (item.METHOD != assetInfo.METHOD)
                            //{
                            //    item.Message += "折旧方法不一致;";
                            //    errorCount++;
                            //}
                            //if (item.LIFE_MONTHS != assetInfo.LIFE_MONTHS)
                            //{
                            //    item.Message += "上牌日期不一致;";
                            //}
                            if (item.FA_LOC_1 != assetInfo.BELONGTO_COMPANY)
                            {
                                item.Message += "存放地点1不一致;";
                                errorCount++;
                            }
                            if (item.FA_LOC_2 != assetInfo.MANAGEMENT_COMPANY)
                            {
                                item.Message += "存放地点2不一致;";
                                errorCount++;
                            }
                            if (item.FA_LOC_3 != assetInfo.ORGANIZATION_NUM)
                            {
                                item.Message += "部门不一致;";
                                errorCount++;
                            }
                            if (item.MODEL_MAJOR != assetInfo.MODEL_MAJOR)
                            {
                                item.Message += "经营模式主类不一致;";
                                errorCount++;
                            }
                            if (item.MODEL_MINOR != assetInfo.MODEL_MINOR)
                            {
                                item.Message += "经营模式子类不一致;";
                                errorCount++;
                            }
                        }
                    }

                    compareData = compareData.Where(x => !x.Message.IsNullOrEmpty()).ToList();
                    if (errorCount == 0)
                    {
                        resultModel.ResultInfo = "数据对比一致";
                        resultModel.ResultInfo2 = compareData;
                        resultModel.IsSuccess = true;
                        resultModel.Status = "3";
                    }
                    else
                    {
                        resultModel.ResultInfo = "数据对比不一致";
                        resultModel.ResultInfo2 = compareData;
                        resultModel.IsSuccess = true;
                        resultModel.Status = "2";
                    }
                }
            });
            return Json(
                resultModel,
                "application/json",
                Encoding.UTF8,
                JsonRequestBehavior.AllowGet
            );
        }
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }
        public JsonResult CheckNumber()
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var ledgerCount = db.Queryable<AssetsLedger_Swap>().Count();
                var assetInfoCount = db.Queryable<Business_AssetMaintenanceInfo>().Count();
                if (ledgerCount == assetInfoCount)
                {
                    resultModel.IsSuccess = true;
                    resultModel.Status = "1";
                }
                else
                {
                    resultModel.ResultInfo = "数量不一致,Oracle资产清册：" + ledgerCount + "条; " + "资产管理台账：" + assetInfoCount + "条";
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}