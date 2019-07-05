using System;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using System.Collections.Generic;
using System.Web;
using System.IO;
using Aspose.Cells;
using System.Data;
using System.Text;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.NextDayData
{
    public class NextDayDataController : BaseController
    {
        // GET: PaymentManagement/NextDayData
        public NextDayDataController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.NextDayData);
            ViewBag.Channel = GetChannel();
            ViewData["transactionId"] = Request["transactionId"] ?? "";
            return View();
        }


        /// <summary>
        /// 获取t+1数据
        /// </summary>
        /// <param name="searchParas">搜索条件</param>
        /// <param name="para">表格参数</param>
        /// <returns></returns>
        public JsonResult GetNextDayDatas(U_NextDay_Search searchParas, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_Business_T1Data_Information_Desc>();
            var start = !string.IsNullOrEmpty(searchParas.PayDateFrom) ? DateTime.Parse(searchParas.PayDateFrom + " 00:00:00") : DateTime.Parse("1900-01-01");
            var end = !string.IsNullOrEmpty(searchParas.PayDateTo) ? DateTime.Parse(searchParas.PayDateTo + " 23:59:59") : DateTime.MaxValue;
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                List<v_Business_T1Data_Information_Desc> t1Datas = new List<v_Business_T1Data_Information_Desc>();
                t1Datas = db.Queryable<v_Business_T1Data_Information_Desc>()
                .WhereIF(!string.IsNullOrEmpty(searchParas.Channel_Id2), i => i.SubjectId.Contains(searchParas.Channel_Id2))
                .WhereIF(!string.IsNullOrEmpty(searchParas.Channel_Id), i => i.Channel_Id == searchParas.Channel_Id)
                .Where(i => SqlFunc.Between(i.Revenuetime, start, end))
                .OrderBy(i => i.Revenuetime, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                foreach (var t1Data in t1Datas)
                {
                    t1Data.DriverBearFees = t1Data.PaidAmount - t1Data.Remitamount;
                    t1Data.CompanyBearsFees = t1Data.RevenueFee - t1Data.DriverBearFees;
                    t1Data.ChannelPayableAmount = t1Data.PaidAmount - t1Data.RevenueFee;
                }
                jsonResult.Rows = t1Datas;
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存手动新增的T+1数据
        /// </summary>
        /// <param name="t1Data"></param>
        /// <param name="isEdit"></param>
        /// <returns></returns>
        public JsonResult SaveNextDayData(Business_T1Data_Information t1Data, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (isEdit)
            {
                t1Data.ChangeDate = DateTime.Now;
                t1Data.ChangeUser = UserInfo.LoginName;
            }
            else
            {
                t1Data.CreatedUser = UserInfo.LoginName;
                t1Data.CreatedDate = DateTime.Now;
                t1Data.Vguid = Guid.NewGuid();
            }
            DbBusinessDataService.Command<NextDayDataPack>((db, o) =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (o.IsExistTranscationId(db, t1Data, isEdit))
                    {
                        resultModel.Status = "2";
                        return;
                    }
                    if (isEdit)
                    {
                        db.Updateable(t1Data).IgnoreColumns(i => new { i.CreatedDate, i.CreatedUser }).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(t1Data).ExecuteCommand();
                    }
                });
                if (resultModel.Status == "2") return;
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        public JsonResult GetSubject(string Channel)
        {
            var Subject = new List<T_Channel_Subject>();
            DbBusinessDataService.Command(db =>
            {
                string sql = string.Format(@"  SELECT * FROM T_Channel_Subject WHERE ChannelVguid=(SELECT Vguid FROM T_Channel WHERE id='{0}')", Channel);
                Subject = db.SqlQueryable<T_Channel_Subject>(sql).ToList();
            });
            return Json(Subject, JsonRequestBehavior.AllowGet); ;
        }



        /// <summary>
        /// 导出T+1模板
        /// </summary>
        public void DownLoadNextDayData()
        {
            ExcelHelper.ExportExcel("Business_T1Data_Information.xlsx", "Business_T1Data_Information.xls");
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

        public ActionResult GetChannelInfor()
        {
            return Json(GetChannel());
        }
        /// <summary>
        /// 导入文件
        /// </summary>
        /// <param name="file">上传文件对象</param>
        /// <returns></returns>
        public ActionResult UploadImportFile(HttpPostedFileBase file)
        {
            string fileName = file.FileName;
            string fileDirectory = Server.MapPath(Global.Temp);
            if (Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }
            string filePath = Path.Combine(fileDirectory, fileName);
            file.SaveAs(filePath);
            return Content(fileName);

        }


        public ActionResult ImportDataHuiDouQuan(string fileName)
        {
            ResultModel<string> data = new ResultModel<string>
            {
                IsSuccess = true,
                Status = "0"
            };
            data = new NextDayDataPack().ImportDataHuiDouQuan(fileName, UserInfo.LoginName);
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 根据字段索引获取字符行中的字段数据
        /// </summary>
        /// <param name="fieldIndex">字段索引</param>
        /// <param name="line">字符行</param>
        /// <param name="fileLineDataRule">字段长度规则</param>
        /// <returns></returns>
        private string GetLineFieldData(int fieldIndex, string line, string[] fileLineDataRule)
        {
            int startIndex = 0;
            for (int j = 0; j < fieldIndex; j++)
            {
                if (j != 0)
                {
                    startIndex++;
                }
                startIndex += fileLineDataRule[j].Length;
            }

            byte[] bytes = System.Text.Encoding.Default.GetBytes(line);
            List<byte> listByte = new List<byte>();

            if (startIndex != 0)
            {
                startIndex++;
            }

            int endIndex = startIndex + fileLineDataRule[fieldIndex].Length;
            if (endIndex > bytes.Length)
            {
                endIndex = bytes.Length;
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                listByte.Add(bytes[i]);
            }
            var lineFieldData = System.Text.Encoding.Default.GetString(listByte.ToArray()).Trim();
            return lineFieldData;
        }


        public ActionResult ImportSelfServicePaymentMachine(string fileName)
        {
            ResultModel<string> data = new ResultModel<string>
            {
                IsSuccess = true,
                Status = "0"
            };
            Worksheet worksheet = new Workbook(Path.Combine(base.Server.MapPath(Global.Temp), fileName)).Worksheets[0];
            DataTable fileDataTable = worksheet.Cells.ExportDataTableAsString(0, 0, worksheet.Cells.MaxDataRow + 1, worksheet.Cells.MaxDataColumn + 1, false);
            this.SortOutSelfServicePaymentMachineData(fileDataTable);
            string str = this.ImportSelfServicePayment(fileDataTable);
            if (!string.IsNullOrEmpty(str))
            {
                data.IsSuccess = false;
            }
            data.ResultInfo = str;
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }


        private void SortOutSelfServicePaymentMachineData(DataTable fileDataTable)
        {
            if ((fileDataTable != null) && (fileDataTable.Rows.Count > 4))
            {
                fileDataTable.Rows.RemoveAt(0);
                fileDataTable.Rows.RemoveAt(0);
                fileDataTable.Rows.RemoveAt(0);
                DataRow row = fileDataTable.Rows[0];
                foreach (DataColumn column in fileDataTable.Columns)
                {
                    column.ColumnName = row[column.ColumnName].ToString();
                }
                fileDataTable.Rows.RemoveAt(0);
            }
        }

        private string ImportSelfServicePayment(DataTable fileDataTable)
        {
            List<Business_T1Data_Information> t1Datas = new List<Business_T1Data_Information>();
            List<Business_Revenuepayment_Information> brDatas = new List<Business_Revenuepayment_Information>();
            if ((fileDataTable != null) && (fileDataTable.Rows.Count > 0))
            {
                foreach (DataRow row in fileDataTable.Rows)
                {
                    Business_T1Data_Information t1Data = new Business_T1Data_Information();
                    string serialnumber = Convert.ToString(row["商户订单号"]);
                    if (!string.IsNullOrEmpty(serialnumber))
                    {

                        string channelId = Convert.ToString(row["商户号"]);
                        bool existChannel = false;
                        DbBusinessDataService.Command(dbs =>
                        {
                            existChannel = dbs.Queryable<T_Channel>().Any(i => i.Id == channelId);
                        });
                        if (!existChannel)
                        {
                            return channelId;
                        }


                        t1Data.Vguid = Guid.NewGuid();
                        t1Data.serialnumber = serialnumber;

                        t1Data.Channel_Id = channelId;
                        string str2 = Convert.ToString(row["交易金额"]);
                        t1Data.Remitamount = Convert.ToDecimal(str2);
                        string str3 = Convert.ToString(row["手续费"]);
                        t1Data.RevenueFee = Convert.ToDecimal(str3);
                        string str4 = Convert.ToString(row["清算金额"]);
                        t1Data.PaidAmount = Convert.ToDecimal(str4);
                        string str5 = Convert.ToString(row["银商订单号"]);
                        t1Data.WechatNo = str5;
                        string str6 = Convert.ToString(row["交易日期"]);
                        string str7 = Convert.ToString(row["交易时间"]);
                        t1Data.Revenuetime = Convert.ToDateTime(str6 + " " + str7);
                        t1Data.ChangeDate = DateTime.Now;
                        t1Data.ChangeUser = UserInfo.LoginName;
                        t1Data.CreatedDate = DateTime.Now;
                        t1Data.CreatedUser = UserInfo.LoginName;
                        Business_Revenuepayment_Information brData = new Business_Revenuepayment_Information
                        {
                            VGUID = t1Data.Vguid,
                            TransactionID = t1Data.serialnumber,
                            PaymentAmount = t1Data.Remitamount,
                            copeFee = t1Data.RevenueFee,
                            ActualAmount = t1Data.PaidAmount,
                            Channel_Id = t1Data.Channel_Id,
                            SubjectId = t1Data.SubjectId,
                            PayDate = t1Data.Revenuetime,
                            ChangeDate = t1Data.ChangeDate,
                            ChangeUser = t1Data.ChangeUser + "[T1_Import]",
                            CreateDate = t1Data.CreatedDate,
                            CreateUser = t1Data.CreatedUser + "[T1_Import]"
                        };
                        bool isHaveT1 = false;
                        DbBusinessDataService.Command<NextDayDataPack>((db, o) =>
                        {
                            isHaveT1 = o.IsExistTranscationId(db, t1Data, false);
                        });
                        if (!isHaveT1)
                        {
                            t1Datas.Add(t1Data);
                        }
                        bool isHaveBr = false;
                        DbBusinessDataService.Command<NextDayDataPack>((db, o) =>
                        {
                            isHaveBr = o.ExistReconciliation(db, brData, false);
                        });
                        if (!isHaveBr)
                        {
                            brDatas.Add(brData);
                        }
                    }
                }
            }
            if ((t1Datas.Count > 0) || (brDatas.Count > 0))
            {
                DbBusinessDataService.Command<NextDayDataPack>((db, o) =>
                {
                    db.Ado.UseTran(delegate
                    {
                        o.ImportData(db, t1Datas);
                        o.ImportData_Reconciliation(db, brDatas);
                    });
                });
            }

            return string.Empty;
        }
    }
}