using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BusinessTypeSet;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList
{
    public class OrderListController : BaseController
    {
        public OrderListController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/OrderList
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetOrderListDatas(Business_OrderList searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_OrderList>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_OrderList>()
                .Where(i => i.Status == searchParams.Status)
                .WhereIF(searchParams.BusinessProject != null, i => i.BusinessProject.Contains(searchParams.BusinessProject))
                .WhereIF(searchParams.CollectionCompany != null, i => i.CollectionCompany == searchParams.CollectionCompany)
                .OrderBy("BusinessSubItem1 asc").ToPageList(para.pagenum, para.pagesize, ref pageCount);
                var data = db.Queryable<Business_CustomerBankInfo>().ToList();
                foreach (var item in jsonResult.Rows)
                {
                    try
                    {
                        var vguid = new Guid(item.CollectionBankAccountName);
                        if (vguid != Guid.Empty)
                        {
                            var bankAccountName = data.Single(x => x.VGUID == vguid).BankAccountName;
                            item.CollectionBankAccountName = bankAccountName;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                jsonResult.TotalRows = pageCount;

                //var data = db.Queryable<Business_BusinessTypeSet>().Where(x => x.ParentVGUID != null).OrderBy("Code asc").ToList();
                //var czGuid = "c86ca480-74b7-415c-a8a4-741955627727";
                //var zlGuid = "c86ca480-74b7-415c-a8a4-741955627728";
                //for (int i = 0; i < 2; i++)
                //{
                //    if (i == 0)
                //    {
                //        var projectCode = "cz";
                //        var projectName = "出租";
                //        var isAny = data.Any(x => x.ParentVGUID == czGuid);
                //        if (isAny)
                //        {
                //            var datas = data.Where(x => x.ParentVGUID == czGuid).ToList();
                //            foreach (var item in datas)
                //            {
                //                projectCode += "|" + item.Code;
                //                projectName += "|" + item.BusinessName;
                //                GetNextItem(db, item, data, projectCode, projectName);
                //            }
                //        }
                //    }
                //    else
                //    {
                //        var isAny = data.Any(x => x.ParentVGUID == zlGuid);
                //        if (isAny)
                //        {
                //            var datas = data.Where(x => x.ParentVGUID == zlGuid).ToList();
                //            foreach (var item in datas)
                //            {
                //                //GetNextItem(db, item, item.VGUID.ToString());
                //            }
                //        }
                //    }
                //}  
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        private void GetNextItem(SqlSugarClient db, Business_BusinessTypeSet item,List<Business_BusinessTypeSet> data, string projectCode,string projectName)
        {
            var isAny = data.Where(x => x.ParentVGUID == item.VGUID.ToString()).ToList();
            if (isAny.Count > 0)
            {
                //var datas = data.Where(x => x.ParentVGUID == zlGuid).ToList();
                foreach (var it in isAny)
                {
                    projectCode += "|" + it.Code;
                    projectName += "|" + it.BusinessName;
                    GetNextItem(db, it, data, projectCode, projectName);
                }
            }
            else
            {

            }
        }
        public JsonResult DeleteOrderListInfo(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                foreach (var item in vguids)
                {
                    int saveChanges = 1;
                    //删除主表信息
                    saveChanges = db.Deleteable<Business_OrderList>(x => x.VGUID == item).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == 1;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult UpdataOrderListInfo(List<Guid> vguids, string status)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                foreach (var item in vguids)
                {
                    int saveChanges = 1;
                    //更新主表信息
                    saveChanges = db.Updateable<Business_OrderList>().UpdateColumns(it => new Business_OrderList()
                    {
                        Status = status,
                    }).Where(it => it.VGUID == item).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
    }
}