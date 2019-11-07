using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo
{
    public class CustomerBankInfoController : BaseController
    {
        public CustomerBankInfoController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: CapitalCenterManagement/CustomerBankInfo
        public ActionResult Index()
        {
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            return View();
        }
        /// <summary>
        /// 获取所有的渠道信息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public JsonResult GetCustomerBankInfo(string BankAccount, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_CustomerBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_CustomerBankInfo>()
                .WhereIF(!string.IsNullOrEmpty(BankAccount), i => i.BankAccount.Contains(BankAccount))
                .OrderBy(i => i.CreateTime, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        ///  保存数据
        /// </summary>
        /// <param name="bankChannel"></param>
        /// <returns></returns>
        public JsonResult SaveCustomerBankInfo(Business_CustomerBankInfo bankInfo, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (!isEdit)
            {
                bankInfo.Founder = UserInfo.LoginName;
                bankInfo.CreateTime = DateTime.Now;
                bankInfo.VGUID = Guid.NewGuid();
            }
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (isEdit)
                    {
                        db.Updateable(bankInfo).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(bankInfo).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteCustomerBankInfo(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = db.Deleteable<Business_CustomerBankInfo>(vguids).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}