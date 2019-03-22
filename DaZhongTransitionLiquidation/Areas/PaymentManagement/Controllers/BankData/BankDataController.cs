using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using System.Linq;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.BankData
{
    public class BankDataController : BaseController
    {
        // GET: PaymentManagement/BankData
        public BankDataController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }

        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            ViewBag.Channel = GetChannel();
            ViewBag.BankChannel = GetBankChannel();
            return View();
        }

        /// <summary>
        /// 分页获取银行数据
        /// </summary>
        /// <param name="searchParams"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public JsonResult GetBankDatas(T_Bank searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_Bank_desc>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<v_Bank_desc>().WhereIF(searchParams.ArrivedTime != null, i => i.ArrivedTime == searchParams.ArrivedTime)
                .WhereIF(searchParams.Channel_Id != null, i => i.Channel_Id == searchParams.Channel_Id)
                .OrderBy(i => i.ArrivedTime, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增或编辑银行数据
        /// </summary>
        /// <param name="bankdata"></param>
        /// <param name="isEdit"></param>
        /// <returns></returns>
        public JsonResult SaveBankData(T_Bank bankdata, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (isEdit)
            {
                bankdata.VMDFTIME = DateTime.Now;
                bankdata.VMDFUSER = UserInfo.LoginName;
            }
            else
            {
                bankdata.VCRTUSER = UserInfo.LoginName;
                bankdata.VCRTTIME = DateTime.Now;
                bankdata.VGUID = Guid.NewGuid();
            }
            DbBusinessDataService.Command(db =>
            {
                var receiveBank = db.Queryable<T_ReceiveBank>().ToList().SingleOrDefault();
                if (receiveBank != null)
                {
                    bankdata.ReceiveBank = receiveBank.Bank;
                    bankdata.ReceiveBankAccount = receiveBank.BankAccount;
                    bankdata.ReceiveBankAccountName = receiveBank.BankAccountName;
                }
            });
            DbBusinessDataService.Command<BankDataPack>((db, o) =>
            {
                var resultisCompleted = isCompleted(bankdata.ArrivedTime);
                if (resultisCompleted)
                {
                    resultModel.Status = "3";//当前时间已经对账成功
                    return;
                }
                var result = db.Ado.UseTran(() =>
                    {
                        if (isEdit)
                        {
                            db.Updateable(bankdata).IgnoreColumns(i => new { i.ArrivedTime }).ExecuteCommand();
                        }
                        else
                        {
                            db.Insertable(bankdata).ExecuteCommand();
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
        /// 删除银行数据
        /// </summary>
        /// <param name="vguids"></param>
        /// <returns></returns>
        public JsonResult DeleteBankDatas(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };

            DbBusinessDataService.Command(db =>
            {
                List<T_Bank> banks = db.Queryable<T_Bank>().In(vguids).ToList();
                List<DateTime> listDate = new List<DateTime>();
                foreach (var item in banks)
                {
                    listDate.Add(Convert.ToDateTime(item.ArrivedTime.Value.ToString("yyyy-MM-dd")));
                }
                var istrue = db.Queryable<v_Business_Reconciliation>().Any(i => listDate.Contains(i.BankBillDate.Value) && i.Status == "2");//
                if (istrue)
                {
                    resultModel.Status = "3";//当前时间已经对账成功
                    return;
                }
                int saveChanges = db.Deleteable<T_Bank>(vguids).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        /// <summary>
        /// 当前日期是否为对账正常
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool isCompleted(DateTime? date)
        {
            var result = false;
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<v_Bank>().Any(i => i.BankDate == date && i.Status == "2");

            });
            return result;
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
        public List<T_BankChannelMapping> GetBankChannel()
        {
            var result = new List<T_BankChannelMapping>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<T_BankChannelMapping>().OrderBy(c => c.VCRTTIME, OrderByType.Desc).ToList();

            });
            return result;
        }
    }
}