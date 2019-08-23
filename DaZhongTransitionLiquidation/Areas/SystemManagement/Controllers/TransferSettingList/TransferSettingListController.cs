using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.TransferSettingList
{
    public class TransferSettingListController : BaseController
    {
        public TransferSettingListController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: SystemManagement/TransferSettingList
        public ActionResult Index()
        {
            ViewBag.Channel = GetChannel();
            return View();
        }
        public List<T_Channel> GetChannel()
        {
            var result = new List<T_Channel>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<T_Channel>().ToList();

            });
            return result;
        }
        public JsonResult GetTransferSettingList(string Month, string Channel, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_TransferSetting>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_TransferSetting>()
                .WhereIF(!string.IsNullOrEmpty(Month), i => i.Month == Month)
                .WhereIF(!string.IsNullOrEmpty(Channel), i => i.Channel == Channel)
                .Where(i => i.AccountModeCode == UserInfo.AccountModeCode)
                //.Where(x => x.IsUnable == "启用" || x.IsUnable == null)
                .OrderBy(i => i.TransferCompany, OrderByType.Asc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                //Channel = Channel == null ? "" : Channel;
                //var data = db.Ado.SqlQuery<usp_RevenueAmountReport>(@"exec usp_RevenueAmountReport @Month,@Channel",new { Month = Month, Channel= Channel }).ToList();
                //foreach (var item in jsonResult.Rows)
                //{
                //    var dataList = data.Where(x => x.OrganizationName == item.TransferCompany && x.Channel_Id == item.Channel && x.RevenueMonth == item.Month).ToList();
                //    switch (item.TransferType)
                //    {
                //        case "营收缴款":
                //            item.Money = dataList.Sum(x => x.PaymentAmountTotal);
                //            break;
                //        case "手续费":
                //            item.Money = dataList.Sum(x => x.CompanyBearsFeesTotal);
                //            break;
                //        case "银行收款":
                //            item.Money = dataList.Sum(x => x.ActualAmountTotal);
                //            break;
                //        default:
                //            break;
                //    }  
                //}
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveTransferSetting(Business_TransferSetting bankChannel, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (!isEdit)
            {
                bankChannel.AccountModeCode = UserInfo.AccountModeCode;
                bankChannel.VCRTUSER = UserInfo.LoginName;
                bankChannel.VCRTTIME = DateTime.Now;
                bankChannel.VGUID = Guid.NewGuid();
            }
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (isEdit)
                    {
                        //bankChannel.AccountModeCode = UserInfo.AccountModeCode;
                        //bankChannel.VMDFTIME = DateTime.Now;
                        //bankChannel.VMDFUSER = UserInfo.LoginName;
                        //db.Updateable<Business_TransferSetting>().ExecuteCommand();
                        db.Updateable<Business_TransferSetting>().UpdateColumns(it => new Business_TransferSetting()
                        {
                            TransferCompany = bankChannel.TransferCompany,
                            TransferType = bankChannel.TransferType,
                            Channel = bankChannel.Channel,
                            ChannelName = bankChannel.ChannelName,
                            Borrow = bankChannel.Borrow,
                            Loan = bankChannel.Loan,
                            CompanyCode = bankChannel.CompanyCode,
                            AccountModeCode = UserInfo.AccountModeCode,
                            VMDFTIME = DateTime.Now,
                            VMDFUSER = UserInfo.LoginName,
                        }).Where(it => it.VGUID == bankChannel.VGUID).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(bankChannel).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}