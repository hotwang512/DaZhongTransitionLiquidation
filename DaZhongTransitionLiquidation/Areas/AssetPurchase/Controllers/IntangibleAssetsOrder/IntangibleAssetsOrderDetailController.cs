using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SyntacticSugar;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.IntangibleAssetsOrder
{

    public class IntangibleAssetsOrderDetailController : BaseController
    {
        // GET: AssetManagement/IntangibleAssetsOrderDetail
        public IntangibleAssetsOrderDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            var vguid = Request["VGUID"].TryToGuid();
            if (vguid != Guid.Empty)
            {
                ViewBag.SubmitStatus = GetSubmitStatus(Request["VGUID"].TryToGuid());
            }
            return View();
        }
        public JsonResult SaveIntangibleAssetsOrder(Business_IntangibleAssetsOrder sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var model = db.Queryable<Business_IntangibleAssetsOrder>().Where(c => c.VGUID == sevenSection.VGUID);
                    if (model.Count() == 0)
                    {
                        var orderNumberLeft = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                        //查出当前日期数据库中最大的订单号
                        var currentDayFixedAssetOrderList = db.Queryable<Business_FixedAssetsOrder>()
                            .Where(c => c.OrderNumber.StartsWith(orderNumberLeft)).Select(c => new { c.OrderNumber }).ToList();
                        var currentDayIntangibleAssetsOrderList = db.Queryable<Business_IntangibleAssetsOrder>()
                            .Where(c => c.OrderNumber.StartsWith(orderNumberLeft)).Select(c => new { c.OrderNumber }).ToList();
                        var currentDayList = currentDayFixedAssetOrderList.Union(currentDayIntangibleAssetsOrderList).ToList();
                        var maxOrderNumRight = 0;
                        if (currentDayList.Any())
                        {
                            maxOrderNumRight = currentDayList.OrderBy(c => c.OrderNumber.Replace(orderNumberLeft, "").TryToInt()).First().OrderNumber.Replace(orderNumberLeft, "").TryToInt();
                        }
                        maxOrderNumRight = maxOrderNumRight + 1;
                        sevenSection.OrderNumber = orderNumberLeft + maxOrderNumRight.ToString().PadLeft(4, '0');
                        sevenSection.VGUID = Guid.NewGuid();
                        sevenSection.CreateDate = DateTime.Now;
                        sevenSection.CreateUser = cache[PubGet.GetUserKey].UserName;
                        sevenSection.SubmitStatus = IntangibleAssetsSubmitStatusEnum.FirstPaymentUnSubmit.TryToInt();
                        db.Insertable<Business_IntangibleAssetsOrder>(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        sevenSection.ChangeDate = DateTime.Now;
                        sevenSection.ChangeUser = cache[PubGet.GetUserKey].UserName;
                        db.Updateable<Business_IntangibleAssetsOrder>(sevenSection).IgnoreColumns(x => new { x.CreateDate, x.CreateUser, x.SubmitStatus,x.OrderNumber }).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });

            return Json(resultModel);
        }
        public JsonResult GetIntangibleAssetsOrder(Guid vguid)
        {
            Business_IntangibleAssetsOrder model = new Business_IntangibleAssetsOrder();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                model = db.Queryable<Business_IntangibleAssetsOrder>().Single(x => x.VGUID == vguid);
            });
            return Json(model, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult UploadContractFile(Guid Vguid, HttpPostedFileBase File)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            if (File != null)
            {
                var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + File.FileName.Substring(File.FileName.LastIndexOf("."), File.FileName.Length - File.FileName.LastIndexOf("."));
                var uploadPath = "\\" + ConfigSugar.GetAppString("UploadPath") + "\\" + "PurchaseContract\\" + newFileName;
                var filePath = System.AppDomain.CurrentDomain.BaseDirectory + uploadPath;
                try
                {
                    File.SaveAs(filePath);
                    DbBusinessDataService.Command(db =>
                    {
                        var result = db.Ado.UseTran(() =>
                        {
                            var sevenSection = db.Queryable<Business_IntangibleAssetsOrder>().Where(c => c.VGUID == Vguid).First();
                            if (sevenSection != null)
                            {
                                sevenSection.ContractFilePath = uploadPath;
                                sevenSection.ContractName = File.FileName;
                                sevenSection.ChangeDate = DateTime.Now;
                                sevenSection.ChangeUser = cache[PubGet.GetUserKey].UserName;
                                db.Updateable(sevenSection).UpdateColumns(x => new {
                                    x.ChangeDate,
                                    x.ChangeUser,
                                    x.ContractFilePath,
                                    x.ContractName
                                }).ExecuteCommand();
                            }
                        });
                        resultModel.IsSuccess = result.IsSuccess;
                        resultModel.ResultInfo = uploadPath;
                        resultModel.ResultInfo2 = File.FileName;
                        resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", filePath, ex.ToString()));
                }
            }
            return Json(resultModel);
        }
        //public JsonResult GetUseDepartment()
        //{
        //    var departmentData = new List<Business_SevenSection>();
        //    DbBusinessDataService.Command(db =>
        //    {
        //        departmentData = db.Queryable<Business_SevenSection>().Where(x =>
        //            x.SectionVGUID == "D63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == "1002" &&
        //            x.CompanyCode == "01" && x.Status == "1" && x.Code.StartsWith("10")).ToList();
        //    });
        //    return Json(departmentData, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult SubmitIntangibleAssetsOrder(Guid vguid)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var model = db.Queryable<Business_IntangibleAssetsOrder>().Where(c => c.VGUID == vguid).First();
                    if (model.SubmitStatus == IntangibleAssetsSubmitStatusEnum.FirstPaymentUnSubmit.TryToInt())
                    {
                        model.SubmitStatus = IntangibleAssetsSubmitStatusEnum.TailPaymentUnSubmit.TryToInt();
                    }
                    else
                    {
                        model.SubmitStatus = IntangibleAssetsSubmitStatusEnum.Submited.TryToInt();
                    }
                    model.SubmitDate = DateTime.Now;
                    model.SubmitUser = cache[PubGet.GetUserKey].UserName;
                    db.Updateable<Business_IntangibleAssetsOrder>(model).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess.ObjToBool() ? "1" : "0";
            });
            return Json(resultModel);
        }
        public int GetSubmitStatus(Guid vguid)
        {
            Business_IntangibleAssetsOrder model = new Business_IntangibleAssetsOrder();
            DbBusinessDataService.Command(db =>
            {
                model = db.Queryable<Business_IntangibleAssetsOrder>().Single(x => x.VGUID == vguid);
            });
            return model.SubmitStatus.TryToInt();
        }
        public JsonResult GetPurchaseGoods(int OrderCategory, Guid[] PurchaseDepartment)
        {
            var PurchaseDepartmentStr = "";

            var orderTypeData = new List<Business_PurchaseOrderSetting>();
            DbBusinessDataService.Command(db =>
            {
                if (PurchaseDepartment == null)
                {
                    orderTypeData = db.Queryable<Business_PurchaseOrderSetting>().Where(x => x.OrderCategory == OrderCategory).ToList();
                }
                else
                {
                    foreach (var str in PurchaseDepartment)
                    {
                        PurchaseDepartmentStr = PurchaseDepartmentStr + str + "','";
                    }

                    PurchaseDepartmentStr = PurchaseDepartmentStr.Substring(0, PurchaseDepartmentStr.Length - 3);
                    orderTypeData = db.SqlQueryable<Business_PurchaseOrderSetting>(@"SELECT DISTINCT bpos.* FROM  Business_PurchaseOrderSetting bpos INNER JOIN
                    Business_PurchaseDepartment bpd ON bpos.VGUID = bpd.PurchaseOrderSettingVguid
                    WHERE OrderCategory = '1' And bpd.DepartmentVguid IN ('" + PurchaseDepartmentStr + "')").ToList();
                }
            });
            return Json(orderTypeData, JsonRequestBehavior.AllowGet);
        }

    }
}