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

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.BankChannelMapping
{
    public class BankChannelMappingController : BaseController
    {
        public BankChannelMappingController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: SystemManagement/BankChannelMapping
        public ActionResult Index()
        {
            //ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.ChannelManagement);
            ViewBag.Channel = GetChannel();
            return View();
        }
        /// <summary>
        /// 获取所有的渠道信息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public JsonResult GetBankChannelMappingInfos(string BankAccount, string Channel, GridParams para)
        {
            var jsonResult = new JsonResultModel<V_BankChannelMapping>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<V_BankChannelMapping>()
                .WhereIF(!string.IsNullOrEmpty(BankAccount), i => i.BankAccount.Contains(BankAccount))
                .WhereIF(!string.IsNullOrEmpty(Channel), i => i.ChannelName.Contains(Channel))
                .OrderBy(i => i.BankAccountName, OrderByType.Asc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
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
        /// <summary>
        ///  保存数据
        /// </summary>
        /// <param name="bankChannel"></param>
        /// <returns></returns>
        public JsonResult SaveBankChannelInfo(T_BankChannelMapping bankChannel, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (isEdit)
            {
                bankChannel.VMDFTIME = DateTime.Now;
                bankChannel.VMDFUSER = UserInfo.LoginName;
            }
            else
            {
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
                        db.Updateable(bankChannel).ExecuteCommand();
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
        /// <summary>
        /// 删除银行渠道映射
        /// </summary>
        /// <param name="vguids"></param>
        /// <returns></returns>
        public JsonResult DeleteBankChannelInfo(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = db.Deleteable<T_BankChannelMapping>(vguids).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateIsUnable(List<Guid> vguids,string isUnable)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 0;
                foreach (var item in vguids)
                {
                    saveChanges = db.Updateable<T_BankChannelMapping>().UpdateColumns(it => new T_BankChannelMapping()
                    {
                        IsUnable = isUnable,
                    }).Where(it => it.VGUID == item).ExecuteCommand();
                }
                resultModel.IsSuccess = saveChanges == 1;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}