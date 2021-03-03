using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
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

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.PaySettingDetail
{
    public class PaySettingDetailController : BaseController
    {
        public PaySettingDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: CapitalCenterManagement/PaySettingDetail
        public ActionResult Index()
        {
            ViewBag.Channel = GetChannel();
            ViewBag.TransferCompany = GetTransferCompany();
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
        public List<Master_Organization> GetTransferCompany()
        {
            var result = new List<Master_Organization>();
            DbService.Command(db =>
            {
                Guid guid = new Guid("1053fdca-ebd7-478c-9c40-1f0dd8f63b7d");
                result = db.Queryable<Master_Organization>().Where(x=>x.ParentVguid == guid).ToList();
            });
            return result;
        }
        public JsonResult GetPaySettingDetail(string PayVGUID, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_PaySettingDetail>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_PaySettingDetail>()
                .WhereIF(!string.IsNullOrEmpty(PayVGUID), i => i.PayVGUID == PayVGUID)
                .OrderBy(i => i.Borrow, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SavePaySettingDetail(Business_PaySettingDetail bankChannel, bool isEdit)
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
                        db.Updateable<Business_PaySettingDetail>().UpdateColumns(it => new Business_PaySettingDetail()
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
                            Remark = bankChannel.Remark,
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
        public JsonResult GetPaySettingList(Guid PayVGUID)
        {
            var jsonResult = new JsonResultModel<V_Business_PaySetting>();
            DbBusinessDataService.Command(db =>
            {
                jsonResult.Rows = db.Queryable<V_Business_PaySetting>().Where(x => x.VGUID == PayVGUID).ToList();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeletePaySettingDetail(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = db.Deleteable<Business_PaySettingDetail>(vguids).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}