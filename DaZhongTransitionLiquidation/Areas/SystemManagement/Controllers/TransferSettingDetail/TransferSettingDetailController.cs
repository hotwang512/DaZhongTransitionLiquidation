using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.TransferSettingDetail
{
    public class TransferSettingDetailController : BaseController
    {
        public TransferSettingDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: SystemManagement/TransferSettingDetail
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
        public JsonResult GetTransferSettingDetail(string PayVGUID, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_TransferSettingDetail>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_TransferSettingDetail>()
                .WhereIF(!string.IsNullOrEmpty(PayVGUID), i => i.PayVGUID == PayVGUID)
                .OrderBy(i => i.Borrow, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveTransferSettingDetail(Business_TransferSettingDetail bankChannel, bool isEdit)
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
                        db.Updateable<Business_TransferSettingDetail>().UpdateColumns(it => new Business_TransferSettingDetail()
                        {
                            TransferCompany = bankChannel.TransferCompany,
                            TransferType = bankChannel.TransferType,
                            Channel = bankChannel.Channel,
                            ChannelName = bankChannel.ChannelName,
                            Borrow = bankChannel.Borrow,
                            Loan = bankChannel.Loan,
                            CompanyCode = bankChannel.CompanyCode,
                            AccountModeCode = UserInfo.AccountModeCode,
                            PayVGUID = bankChannel.PayVGUID,
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
        public JsonResult GetTransferSettingList(Guid PayVGUID)
        {
            var jsonResult = new JsonResultModel<Business_TransferSetting>();
            DbBusinessDataService.Command(db =>
            {
                jsonResult.Rows = db.Queryable<Business_TransferSetting>().Where(x => x.VGUID == PayVGUID).ToList();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteTransferSettingDetail(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = db.Deleteable<Business_TransferSettingDetail>(vguids).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}