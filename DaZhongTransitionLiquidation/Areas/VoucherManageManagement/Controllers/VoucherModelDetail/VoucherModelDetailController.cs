using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherModelDetail
{
    public class VoucherModelDetailController : BaseController
    {
        public VoucherModelDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: VoucherManageManagement/VoucherModelDetail
        public ActionResult Index()
        {
            ViewBag.GetGuid = Guid.NewGuid().TryToString();
            return View();
        }
        public JsonResult GetVoucherModelDetail(Guid vguid)
        {
            Business_VoucherModel orderList = new Business_VoucherModel();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_VoucherModel>().Single(x => x.VGUID == vguid);


                //var vguidList = db.SqlQueryable<Business_VoucherModel>(@"select VGUID from Business_VoucherModel").ToList();
                //foreach (var item in vguidList)
                //{
                //    List<Business_CashBorrowLoan> cblList = new List<Business_CashBorrowLoan>();
                //    var vm = db.Queryable<Business_CashBorrowLoan>().Where(x => x.PayVGUID == item.VGUID).OrderBy(x => x.VCRTTIME, OrderByType.Asc).ToList();
                //    int j = 0;
                //    for (int i = 0; i < vm.Count; i++)
                //    {
                //        vm[i].Sort = j;
                //        j += 10;
                //        cblList.Add(vm[i]);
                //    }
                //    if (cblList.Count > 0)
                //    {
                //        db.Updateable(cblList).ExecuteCommand();
                //    }
                //}
                
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult SaveVoucherModelDetail(Business_VoucherModel sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var isAnyName = db.Queryable<Business_VoucherModel>().Any(x => x.ModelName == sevenSection.ModelName && x.AccountModeCode == UserInfo.AccountModeCode && x.CompanyCode == UserInfo.CompanyCode && x.VGUID != sevenSection.VGUID);
                    if (isAnyName)
                    {
                        resultModel.Status = "2";
                    }
                    else
                    {
                        var isAny = db.Queryable<Business_VoucherModel>().Any(x => x.VGUID == sevenSection.VGUID);
                        if (!isAny)
                        {
                            sevenSection.Creater = UserInfo.LoginName;
                            sevenSection.CreateTime = DateTime.Now;
                            sevenSection.Status = "1";
                            db.Insertable(sevenSection).ExecuteCommand();
                        }
                        else
                        {
                            db.Updateable(sevenSection).IgnoreColumns(it => new { it.Creater, it.CreateTime, it.Status }).ExecuteCommand();
                        }
                        resultModel.Status = "1";
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
            });
            return Json(resultModel);
        }
    }
}