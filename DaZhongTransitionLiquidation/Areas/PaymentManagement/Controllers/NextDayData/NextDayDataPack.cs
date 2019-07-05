using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.NextDayData
{
    public class NextDayDataPack
    {
        public bool ExistReconciliation(SqlSugarClient db, Business_Revenuepayment_Information revenuepaymentInformationDatas, bool isEdit)
        {
            if (isEdit)//编辑
            {
                return db.Queryable<Business_Revenuepayment_Information>().Any(i => i.TransactionID == revenuepaymentInformationDatas.TransactionID && i.VGUID != revenuepaymentInformationDatas.VGUID);
            }
            return db.Queryable<Business_Revenuepayment_Information>().Any(i => i.TransactionID == revenuepaymentInformationDatas.TransactionID);
        }
        /// <summary>
        /// 流水号是否存在
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="t1Data"></param>
        /// <param name="isEdit">是否编辑</param>
        /// <returns></returns>
        public bool IsExistTranscationId(SqlSugarClient db, Business_T1Data_Information t1Data, bool isEdit)
        {
            if (isEdit)//编辑
            {
                return db.Queryable<Business_T1Data_Information>().Any(i => i.serialnumber == t1Data.serialnumber && i.Vguid != t1Data.Vguid);
            }
            return db.Queryable<Business_T1Data_Information>().Any(i => i.serialnumber == t1Data.serialnumber);
        }

        public ResultModel<string> ImportDataHuiDouQuan(string fileName, string loginName = "sysadmin")
        {
            ResultModel<string> data = new ResultModel<string>
            {
                IsSuccess = true,
                Status = "0"
            };
            using (DbBusinessDataService DbBusinessDataService = new DbBusinessDataService())
            {
                string fileFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Global.Temp.Replace("/", ""));
                string filePath = Path.Combine(fileFolder, fileName);
                string[] fileLines = File.ReadAllLines(filePath, Encoding.Default);
                string str = string.Empty;
                if (fileLines.Length > 0x10)
                {
                    str = ImportHuiDouQuanData(fileLines, DbBusinessDataService, loginName);
                    if (string.IsNullOrEmpty(str))
                    {
                        str = ImportJianLianData(fileLines, DbBusinessDataService, loginName);
                    }
                    if (!string.IsNullOrEmpty(str))
                    {
                        data.IsSuccess = false;
                    }
                    data.ResultInfo = str;
                }
                else
                {
                    data.IsSuccess = true;
                    data.ResultInfo = "导入文件数据不正确！";
                }
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("删除文件错误:文件:{0} 错误信息:{1}", filePath, ex.ToString()));
                }
            }
            return data;
        }

        /// <summary>
        /// 导入慧兜圈数据
        /// </summary>
        /// <param name="fileLines"></param>
        private string ImportHuiDouQuanData(string[] fileLines, DbBusinessDataService DbBusinessDataService, string loginName)
        {

            List<Business_T1Data_Information> t1Datas = new List<Business_T1Data_Information>();
            List<Business_T1Data_Information_2> t1Datas_2 = new List<Business_T1Data_Information_2>();
            List<Business_Revenuepayment_Information> brDatas = new List<Business_Revenuepayment_Information>();

            int dataIndex = 0;
            string[] fileLineDataRule = null;
            for (int i = 0; i < fileLines.Length; i++)
            {
                var fileLine = fileLines[i];
                if (fileLine.Contains("==================================慧兜圈"))
                {
                    dataIndex = i + 4;
                    fileLineDataRule = fileLines[i + 3].Split(' ');
                    break;
                }
            }

            for (int i = dataIndex; i < fileLines.Length; i++)
            {
                var fileLine = fileLines[i];
                if (string.IsNullOrEmpty(fileLine))
                {
                    break;
                }

                Business_T1Data_Information t1Data = new Business_T1Data_Information();
                Business_T1Data_Information_2 t1Data_2 = new Business_T1Data_Information_2();
                t1Data_2.Vguid = t1Data.Vguid = Guid.NewGuid();
                string serialnumber = GetLineFieldData(14, fileLine, fileLineDataRule);
                string channelId = this.GetLineFieldData(0, fileLine, fileLineDataRule);

                bool existChannel = false;
                DbBusinessDataService.Command(delegate (SqlSugarClient dbs)
                {
                    existChannel = dbs.Queryable<T_Channel>().Any(c => c.Id == channelId);
                });
                if (!existChannel)
                {
                    return channelId;
                }

                t1Data_2.serialnumber = t1Data.serialnumber = serialnumber;
                t1Data_2.Channel_Id = t1Data.Channel_Id = channelId;
                string subjectId = this.GetLineFieldData(2, fileLine, fileLineDataRule);
                t1Data_2.SubjectId = t1Data.SubjectId = subjectId;
                bool subjectDeposit = false;
                DbBusinessDataService.Command(delegate (SqlSugarClient dbs)
                {
                    subjectDeposit = dbs.Queryable<T_Channel_Subject>().Any(c => c.SubjectId == subjectId && c.Deposit == true);
                });

                string remitamount = this.GetLineFieldData(10, fileLine, fileLineDataRule);
                t1Data_2.Remitamount = t1Data_2.Remitamount = t1Data.Remitamount = Convert.ToDecimal(remitamount);

                string revenueFee = GetLineFieldData(11, fileLine, fileLineDataRule);
                t1Data_2.RevenueFee = t1Data.RevenueFee = Convert.ToDecimal(revenueFee);

                string paidAmount = GetLineFieldData(10, fileLine, fileLineDataRule);
                t1Data_2.PaidAmount = t1Data.PaidAmount = Convert.ToDecimal(paidAmount);

                string orderNumber = GetLineFieldData(13, fileLine, fileLineDataRule);
                t1Data_2.WechatNo = t1Data.WechatNo = orderNumber;

                string time = GetLineFieldData(8, fileLine, fileLineDataRule);

                t1Data_2.Revenuetime = t1Data.Revenuetime = Convert.ToDateTime(time);
                t1Data_2.ChangeDate = t1Data.ChangeDate = DateTime.Now;
                t1Data_2.ChangeUser = t1Data.ChangeUser = loginName;
                t1Data_2.CreatedDate = t1Data_2.CreatedDate = t1Data.CreatedDate = DateTime.Now;
                t1Data_2.CreatedUser = t1Data.CreatedUser = loginName;

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
                if (subjectDeposit)
                {
                    t1Datas_2.Add(t1Data_2);
                    continue;
                }

                bool isHaveT1 = false;
                DbBusinessDataService.Command((db) =>
                {
                    isHaveT1 = IsExistTranscationId(db, t1Data, false);
                });
                if (!isHaveT1)
                {
                    t1Datas.Add(t1Data);
                }
                bool isHaveBr = false;
                DbBusinessDataService.Command((db) =>
                {
                    isHaveBr = ExistReconciliation(db, brData, false);
                });
                if (!isHaveBr)
                {
                    brDatas.Add(brData);
                }
            };
            if (t1Datas.Count > 0 || brDatas.Count > 0 || t1Datas_2.Count > 0)
            {
                DbBusinessDataService.Command((db) =>
                {
                    db.Ado.UseTran(delegate
                    {
                        if (t1Datas.Count > 0)
                            ImportData(db, t1Datas);
                        if (brDatas.Count > 0)
                            ImportData_Reconciliation(db, brDatas);
                        if (t1Datas_2.Count > 0)
                            ImportDepositData(db, t1Datas_2);
                    });
                });
            }
            return string.Empty;

        }
        /// <summary>
        /// 导入间联数据
        /// </summary>
        /// <param name="fileLines"></param>
        private string ImportJianLianData(string[] fileLines, DbBusinessDataService DbBusinessDataService, string loginName)
        {
            List<Business_T1Data_Information> t1Datas = new List<Business_T1Data_Information>();
            List<Business_T1Data_Information_2> t1Datas_2 = new List<Business_T1Data_Information_2>();
            List<Business_Revenuepayment_Information> brDatas = new List<Business_Revenuepayment_Information>();

            int dataIndex = 0;
            string[] fileLineDataRule = null;
            for (int i = 0; i < fileLines.Length; i++)
            {
                var fileLine = fileLines[i];
                if (fileLine.Contains("==================================间联"))
                {
                    dataIndex = i + 4;
                    fileLineDataRule = fileLines[i + 3].Split(' ');
                    break;
                }
            }

            for (int i = dataIndex; i < fileLines.Length; i++)
            {
                var fileLine = fileLines[i];
                if (string.IsNullOrEmpty(fileLine))
                {
                    break;
                }
                Business_T1Data_Information t1Data = new Business_T1Data_Information();
                Business_T1Data_Information_2 t1Data_2 = new Business_T1Data_Information_2();
                t1Data_2.Vguid = t1Data.Vguid = Guid.NewGuid();
                string serialnumber = GetLineFieldData(17, fileLine, fileLineDataRule);
                string channelId = this.GetLineFieldData(0, fileLine, fileLineDataRule);

                bool existChannel = false;
                DbBusinessDataService.Command(dbs =>
                {
                    existChannel = dbs.Queryable<T_Channel>().Any(c => c.Id == channelId);
                });
                if (!existChannel)
                {
                    return channelId;
                }

                t1Data_2.serialnumber = t1Data.serialnumber = serialnumber;
                t1Data_2.Channel_Id = t1Data.Channel_Id = channelId;

                string subjectId = this.GetLineFieldData(1, fileLine, fileLineDataRule);
                t1Data_2.SubjectId = t1Data.SubjectId = subjectId;
                bool subjectDeposit = false;
                DbBusinessDataService.Command(delegate (SqlSugarClient dbs)
                {
                    subjectDeposit = dbs.Queryable<T_Channel_Subject>().Any(c => c.SubjectId == subjectId && c.Deposit == true);
                });


                string remitamount = GetLineFieldData(9, fileLine, fileLineDataRule);
                t1Data_2.Remitamount = t1Data.Remitamount = Convert.ToDecimal(remitamount);

                string revenueFee = GetLineFieldData(12, fileLine, fileLineDataRule);
                t1Data_2.RevenueFee = t1Data.RevenueFee = Convert.ToDecimal(revenueFee);

                string paidAmount = GetLineFieldData(9, fileLine, fileLineDataRule);
                t1Data_2.PaidAmount = t1Data.PaidAmount = Convert.ToDecimal(paidAmount);

                string orderNumber = GetLineFieldData(16, fileLine, fileLineDataRule);
                t1Data_2.WechatNo = t1Data.WechatNo = orderNumber;

                string date = GetLineFieldData(6, fileLine, fileLineDataRule);
                string time = GetLineFieldData(7, fileLine, fileLineDataRule);

                t1Data_2.Revenuetime = t1Data.Revenuetime = Convert.ToDateTime(date + " " + time);
                t1Data_2.ChangeDate = t1Data.ChangeDate = DateTime.Now;
                t1Data_2.ChangeUser = t1Data.ChangeUser = loginName;
                t1Data_2.CreatedDate = t1Data.CreatedDate = DateTime.Now;
                t1Data_2.CreatedUser = t1Data.CreatedUser = loginName;

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
                if (subjectDeposit)
                {
                    t1Datas_2.Add(t1Data_2);
                    continue;
                }
                bool isHaveT1 = false;
                DbBusinessDataService.Command((db) =>
                {
                    isHaveT1 = IsExistTranscationId(db, t1Data, false);
                });
                if (!isHaveT1)
                {
                    t1Datas.Add(t1Data);
                }
                bool isHaveBr = false;
                DbBusinessDataService.Command((db) =>
                {
                    isHaveBr = ExistReconciliation(db, brData, false);
                });
                if (!isHaveBr)
                {
                    brDatas.Add(brData);
                }

            };
            if (t1Datas.Count > 0 || brDatas.Count > 0 || t1Datas_2.Count > 0)
            {
                DbBusinessDataService.Command((db) =>
                {
                    db.Ado.UseTran(delegate
                    {
                        if (t1Datas.Count > 0)
                            ImportData(db, t1Datas);
                        if (brDatas.Count > 0)
                            ImportData_Reconciliation(db, brDatas);
                        if (t1Datas_2.Count > 0)
                            ImportDepositData(db, t1Datas_2);
                    });
                });
            }
            return string.Empty;

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


        /// <summary>
        /// 导出T+1数据
        /// </summary>
        /// <param name="db"></param>
        /// <param name="t1Datas"></param>
        /// <returns></returns>
        public int ImportData(SqlSugarClient db, List<Business_T1Data_Information> t1Datas)
        {
            return db.Insertable<Business_T1Data_Information>(t1Datas).ExecuteCommand();
        }
        /// <summary>
        /// 导出营收表
        /// </summary>
        /// <param name="db"></param>
        /// <param name="revenuepaymentInformationDatas"></param>
        /// <returns></returns>
        public int ImportData_Reconciliation(SqlSugarClient db, List<Business_Revenuepayment_Information> revenuepaymentInformationDatas)
        {
            return db.Insertable(revenuepaymentInformationDatas).ExecuteCommand();
        }
        /// <summary>
        /// 导出T+1数据 押金
        /// </summary>
        /// <param name="db"></param>
        /// <param name="t1Datas"></param>
        /// <returns></returns>
        public int ImportDepositData(SqlSugarClient db, List<Business_T1Data_Information_2> t1Datas_2)
        {
            return db.Insertable<Business_T1Data_Information_2>(t1Datas_2).ExecuteCommand();
        }


    }
}