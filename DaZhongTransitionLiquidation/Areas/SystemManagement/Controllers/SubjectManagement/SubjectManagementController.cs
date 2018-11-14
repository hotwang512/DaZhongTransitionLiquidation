using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.SubjectManagement
{
    public class SubjectManagementController : BaseController
    {
        // GET: SystemManagement/SubjectManagement

        public SubjectManagementController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        { }


        public ActionResult SubjectManagement()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.ChannelManagement);
            ViewBag.Channel = GETChannel();
            return View();
        }

        public List<T_Channel> GETChannel()
        {
            var result = new List<T_Channel>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<T_Channel>().ToList();

            });
            return result;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public JsonResult GetChannelInfos(string SubjectNmae, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_Channel_Subject_Desc>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<v_Channel_Subject_Desc>().WhereIF(!string.IsNullOrEmpty(SubjectNmae), i => i.SubjectNmae.Contains(SubjectNmae))
                .OrderBy(i => i.VCRTTIME, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="vguids"></param>
        /// <returns></returns>
        public JsonResult DeleteChannelInfos(Guid[] vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = db.Deleteable<T_Channel_Subject>().In(vguids).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Length;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        public JsonResult SaveSubjectInfo(T_Channel_Subject channel, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (isEdit)
            {
                channel.VMDFTIME = DateTime.Now;
                channel.VMDFUSER = UserInfo.LoginName;
            }
            else
            {
                //channel.Id = AutoGenerateId("channel", "ID", "", "5");
                channel.VCRTUSER = UserInfo.LoginName;
                channel.VCRTTIME = DateTime.Now;
                channel.Vguid = Guid.NewGuid();
            }
            DbBusinessDataService.Command<SubjectManagementPack>((db, o) =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (o.IsExistChannel(db, channel, isEdit))
                    {
                        resultModel.Status = "2";
                        return;
                    }
                    if (o.IsExistChannelid(db, channel, isEdit))
                    {
                        resultModel.Status = "3";
                        return;
                    }
                    if (isEdit)
                    {
                        db.Updateable(channel).IgnoreColumns(i => new { i.ContractStartTime, i.ContractEndTime, i.VCRTTIME, i.VCRTUSER }).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(channel).ExecuteCommand();
                    }
                });
                if (resultModel.Status == "2") return;
                if (resultModel.Status == "3") return;
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}