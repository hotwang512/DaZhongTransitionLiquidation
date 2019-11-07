using Aspose.Cells;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.SettlementImport
{
    public class SettlementImportController : BaseController
    {
        public SettlementImportController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: VoucherManageManagement/SettlementImport
        public ActionResult Index()
        {
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetSettlementData(Business_SettlementImport searchParams, GridParams para)
        {
            var response = new List<Business_SettlementImport>();
            DbBusinessDataService.Command(db =>
            {
                response = db.Queryable<Business_SettlementImport>()
                .WhereIF(searchParams.Model != null, i => i.Model.Contains(searchParams.Model))
                .WhereIF(searchParams.ClassType != null, i => i.ClassType.Contains(searchParams.ClassType))
                .OrderBy("MoneyRow asc,MoneyColumns asc").ToList();
                foreach (var item in response)
                {
                    if (item.BusinessType.Contains("-"))
                    {
                        item.Business = item.BusinessType.Split("-")[0];
                        item.BusinessType = item.BusinessType.Split("-")[1];
                    }
                    else
                    {
                        item.Business = item.BusinessType;
                    }
                }
                //jsonResult.TotalRows = pageCount;
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveSettlementImport(string Model, string ClassType, string CarType, string Business, string BusinessType, decimal? Money, decimal? NewMoney)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 0;
                BusinessType = Business + "-" + BusinessType;
                var data = db.Queryable<Business_SettlementImport>().Where(x => x.Model == Model && x.ClassType == ClassType && x.CarType == CarType
                             && x.BusinessType == BusinessType && x.Money == Money).ToList();
                if(data.Count == 1)
                {
                    data[0].Money = NewMoney;
                    saveChanges = db.Updateable(data[0]).ExecuteCommand();
                }
                resultModel.IsSuccess = saveChanges == 1;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ImportSettlementData(string fileName)
        {
            ResultModel<string> data = new ResultModel<string>
            {
                IsSuccess = true,
                Status = "0"
            };
            Workbook workbook = new Workbook(Path.Combine(Server.MapPath(Global.Temp), fileName));
            Worksheet worksheet = workbook.Worksheets[14];
            Cells cells = worksheet.Cells;
            var settlementList = new List<Business_SettlementImport>();
            DbBusinessDataService.Command(db =>
            {
                if (worksheet.Cells.MaxDataRow > 0)
                {
                    var modelList = new List<string>();//模式
                    var classList = new List<string>();//班型
                    var carList = new List<string>();//车型
                    var businessList = new List<string>();//暂时不用
                    var businessTypeList = new List<string>();//营业收入类型(父类,子类合并)
                    var moneyList = new List<decimal>();//金额
                    var moneyRow = new List<int>();//金额行
                    var moneyColumns = new List<int>();//金额列
                    var moneyIndex = new List<int>();
                    var modelIndexList = new List<int>();//模式合并单元格数
                    var result = new List<string>();
                    var settlementData = db.Queryable<Business_SettlementSubject>().ToList();
                    //构造所需数据(不包含金额)
                    GetSettlementData(worksheet, modelList, classList, carList, modelIndexList);
                    //构造所需数据(营业收入类型--父类,子类合并)
                    GetSettlementBusinessType(worksheet, moneyIndex, businessTypeList);
                    //构造所需数据(金额--单独处理)
                    GetSettlementMoney(worksheet,moneyIndex, moneyList, businessTypeList, moneyRow, moneyColumns);
                    //整理数据成表,构造新的数组
                    GetDescartesData(modelList, carList, result, modelIndexList, classList, businessTypeList);
                    //构造实体类,插入表中
                    for (int i = 0; i < result.Count; i++)
                    {
                        var businessVguid = "";
                        var resultList = result[i].Split(",").ToList();
                        if(resultList[3].Split("-").Count() == 2)
                        {
                            var busin1 = resultList[3].Split("-")[0];//结算项目父类
                            var busin2 = resultList[3].Split("-")[1];//结算项目子类
                            var business = settlementData.Where(x => x.BusinessType == busin1).ToList();
                            if (business.Count == 1)
                            {
                                var vguid = business[0].VGUID;
                                var businessType = settlementData.Where(x => x.ParentVGUID == vguid && x.BusinessType == busin2).ToList();
                                if (businessType.Count == 1)
                                {
                                    //结算项目标准匹配项目所对应配置
                                    businessVguid = businessType[0].VGUID.TryToString();
                                }
                            }
                        }
                        else
                        {
                            var business = settlementData.Where(x => x.BusinessType == resultList[3]).ToList();
                            if (business.Count == 1)
                            {
                                //结算项目标准匹配项目所对应配置
                                var vguid = business[0].VGUID;                               
                                businessVguid = vguid.TryToString();
                            }
                        }
                       
                        Business_SettlementImport settlement = new Business_SettlementImport();
                        settlement.VGUID = Guid.NewGuid();
                        settlement.Model = resultList[0];
                        settlement.ClassType = resultList[1];
                        settlement.CarType = resultList[2];
                        settlement.BusinessType = resultList[3];
                        settlement.Business = businessVguid;
                        settlement.Money = moneyList[i];
                        settlement.MoneyRow = moneyRow[i];
                        settlement.MoneyColumns = moneyColumns[i];
                        settlement.Founder = UserInfo.LoginName;
                        settlement.CreatTime = DateTime.Now;
                        settlementList.Add(settlement);
                    };
                    if (settlementList.Count > 0)
                    {
                        db.Deleteable<Business_SettlementImport>().ExecuteCommand();
                        db.Insertable(settlementList).ExecuteCommand();
                        data.IsSuccess = true;
                    }
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine(item);   
                    data.IsSuccess = false;
                    data.ResultInfo = "导入文件数据不正确！";
                }
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        private void GetSettlementData(Worksheet worksheet, List<string> modelList, List<string> classList, List<string> carList, List<int> modelIndexList)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < worksheet.Cells.Columns.Count; j++)
                {
                    //var value1 = worksheet.Cells.Rows[4].GetCellOrNull(0).Value;
                    //var value2 = worksheet.Cells.Rows[9].GetCellOrNull(0).Value;
                    //var value3 = worksheet.Cells.Rows[9].GetCellOrNull(1).Value;
                    //var value4 = worksheet.Cells.Rows[0].GetCellOrNull(0).GetArrayRange();
                    //var value5 = worksheet.Cells.Rows[0].GetCellOrNull(1).GetArrayRange();
                    //var value6 = worksheet.Cells.Rows[0].GetCellOrNull(2).GetMergedRange();//10
                    //var value7 = worksheet.Cells.Rows[0].GetCellOrNull(12).GetMergedRange();//15

                    var value = worksheet.Cells.Rows[i].GetCellOrNull(j).Value;
                    if (i == 0)
                    {
                        var ranges = worksheet.Cells.Rows[i].GetCellOrNull(j).GetMergedRange().ColumnCount;
                        if (ranges >= 5 && value != null)
                        {
                            //模式合并单元格数
                            modelIndexList.Add(ranges);
                        }
                    }
                    if (value != null)
                    {
                        var cellValue = value.ToString();
                        switch (i)
                        {
                            case 0:
                                if (cellValue != "模式")
                                {
                                    modelList.Add(cellValue);//模式 
                                }
                                break;
                            case 1:
                                if (cellValue != "班型")
                                {
                                    classList.Add(cellValue);//班型
                                }
                                break;
                            case 2:
                                if (cellValue != "车型" && !carList.Contains(cellValue))
                                {
                                    carList.Add(cellValue);//车型
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
            }
        }
        private void GetSettlementBusinessType(Worksheet worksheet, List<int> moneyIndex, List<string> businessTypeList)
        {
            var valueNull = "";
            for (int i = 4; i < worksheet.Cells.MaxDataRow - 2; i++)
            {
                try
                {
                    var value1 = worksheet.Cells.Rows[i].GetCellOrNull(0).Value;
                    var value2 = worksheet.Cells.Rows[i].GetCellOrNull(1).Value;
                    if (value1 != null && value1.ToString() == "二、营业成本")
                    {
                        moneyIndex.Add(i);//金额排除该行
                        continue;
                    }
                    if (value1 != null)
                    {
                        valueNull = value1.ToString();
                    }
                    if(value2 == null)
                    {
                        businessTypeList.Add(valueNull);
                    }
                    else
                    {
                        var cellValue = valueNull + "-" + value2.ToString();
                        businessTypeList.Add(cellValue);
                    } 
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                
            }
        }
        private void GetSettlementMoney(Worksheet worksheet, List<int> moneyIndex, List<decimal> moneyList, List<string> businessTypeList, List<int> moneyRow, List<int> moneyColumns)
        {
            decimal money = 0;
            for (int j = 2; j < worksheet.Cells.Columns.Count; j++)
            {
                for (int i = 4; i < worksheet.Cells.MaxDataRow - 2; i++)
                {
                    if (i == moneyIndex[0])
                    {
                        continue;
                    }
                    else if (j > 1)
                    {
                        var value = worksheet.Cells.Rows[i].GetCellOrNull(j).Value;
                        if (value == null)
                        {
                            money = 0;
                        }
                        else
                        {
                            money = Convert.ToDecimal(value);
                        }
                        moneyList.Add(money);
                        moneyRow.Add(i);
                        moneyColumns.Add(j);
                    }
                }
            }
        }
        private void GetDescartesData(List<string> modelList, List<string> carList, List<string> result, List<int> modelIndexList, List<string> classList, List<string> businessTypeList)
        {
            var k = 0;
            for (int i = 0; i < modelList.Count; i++)
            {
                var modelIndex = modelIndexList[i];
                var carIndex = carList.Count;
                var classIndex = modelIndex / carIndex;
                var str = new List<string>();
                var classIndexList = new List<int>();
                classIndexList.Add(classIndex);
                if (i == 0)
                {
                    //str = classList.GetRange(0, classIndex);
                    for (int j = 0; j < classIndex; j++)
                    {
                        str.Add(classList[j]);
                        k = j;
                    }
                }
                else
                {
                    //str = classList.GetRange(classIndexList[i-1], classIndex);
                    var l = k;
                    for (int j = l + 1; j < classIndex + l + 1; j++)
                    {
                        str.Add(classList[j]);
                        k = j;
                    }
                }
                string[] str1 = { modelList[i] };
                string[] str2 = str.ToArray(); //classList.ToArray();classList[2], classList[3], classList[4]
                string[] str3 = carList.ToArray();
                string[] str4 = businessTypeList.ToArray();
                List<string[]> list = new List<string[]>();
                list.Add(str1);
                list.Add(str2);
                list.Add(str3);
                list.Add(str4);
                //笛卡尔积 算法
                Descartes(list, 0, result, string.Empty);
            }
        }
        private static string Descartes(List<string[]> list, int count, List<string> result, string data)
        {
            string temp = data;
            //获取当前数组
            string[] astr = list[count];
            //循环当前数组
            foreach (var item in astr)
            {
                if (count + 1 < list.Count)
                {
                    temp += Descartes(list, count + 1, result, data + item + ",");
                }
                else
                {
                    result.Add(data + item);
                }

            }

            return temp;
        }
    }
}