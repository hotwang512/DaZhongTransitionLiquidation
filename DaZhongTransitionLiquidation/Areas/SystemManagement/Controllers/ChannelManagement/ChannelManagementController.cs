using System;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.ChannelManagement
{
    public class ChannelManagementController : BaseController
    {
        // GET: SystemManagement/ChannelManagement
        public ChannelManagementController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        public ActionResult Channel()
        {
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.ChannelManagement);
            return View();
        }
        /// <summary>
        /// 获取所有的渠道信息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public JsonResult GetChannelInfos(T_Channel channel, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_Channel_Desc>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<v_Channel_Desc>().WhereIF(!string.IsNullOrEmpty(channel.Name), i => i.Name.Contains(channel.Name))
                .OrderBy(i => i.VCRTTIME, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存渠道信息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isEdit"></param>
        /// <returns></returns>
        public JsonResult SaveChannelInfo(T_Channel channel, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (isEdit)
            {
                channel.VMDFTIME = DateTime.Now;
                channel.VMDFUSER = UserInfo.LoginName;
            }
            else
            {
                channel.Id = AutoGenerateId("channel", "ID", "", "5");
                channel.VCRTUSER = UserInfo.LoginName;
                channel.VCRTTIME = DateTime.Now;
                channel.Vguid = Guid.NewGuid();
            }
            DbBusinessDataService.Command<ChannelManagementPack>((db, o) =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (o.IsExistChannel(db, channel, isEdit))
                    {
                        resultModel.Status = "2";
                        return;
                    }
                    if (isEdit)
                    {
                        db.Updateable(channel).IgnoreColumns(i => new { i.Id, i.VCRTTIME, i.VCRTUSER }).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(channel).ExecuteCommand();
                    }
                });
                if (resultModel.Status == "2") return;
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        /// <summary>
        /// 删除销售渠道
        /// </summary>
        /// <param name="vguids"></param>
        /// <returns></returns>
        public JsonResult DeleteChannelInfos(Guid[] vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = db.Deleteable<T_Channel>().In(vguids).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Length;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}