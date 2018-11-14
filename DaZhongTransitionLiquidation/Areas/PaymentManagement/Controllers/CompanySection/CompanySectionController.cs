﻿using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection
{
    public class CompanySectionController : BaseController
    {
        public CompanySectionController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: PaymentManagement/CompanySection
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            //ViewBag.Channel = GetChannel();
            //ViewBag.BankChannel = GetBankChannel();
            return View();
        }
        public JsonResult GetCompanySection(GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_SevenSection>().Where(x=>x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43")
                .OrderBy(i => i.Code, OrderByType.Asc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 新增或编辑银行数据
        /// </summary>
        /// <param name="sevenSection"></param>
        /// <param name="isEdit"></param>
        /// <returns></returns>
        public JsonResult SaveCompanySection(Business_SevenSection sevenSection, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (!isEdit)
            {
                sevenSection.VCRTUSER = UserInfo.LoginName;
                sevenSection.VCRTTIME = DateTime.Now;
                sevenSection.Status = "1";
                sevenSection.SectionVGUID = "A63BD715-C27D-4C47-AB66-550309794D43";
                sevenSection.VGUID = Guid.NewGuid();
            }
            DbBusinessDataService.Command(db =>
            {
                var IsSuccess = "0";
                var result = db.Ado.UseTran(() =>
                {
                    var guid = sevenSection.VGUID;
                    var code = sevenSection.Code;
                    var isAny = db.Queryable<Business_SevenSection>().Any(x => x.Code == code && x.VGUID != guid && x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43");
                    if (isAny)
                    {
                        IsSuccess = "2";
                        return;
                    }
                    if (isEdit)
                    {
                        db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                        {
                            Code = sevenSection.Code,
                            Descrption = sevenSection.Descrption,
                            Remark = sevenSection.Remark,
                            VMDFTIME = DateTime.Now,
                            VMDFUSER = UserInfo.LoginName
                        }).Where(it => it.VGUID == guid).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(sevenSection).ExecuteCommand();
                    }
                });               
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                if (IsSuccess == "2")
                {
                    resultModel.Status = IsSuccess;
                }
            });
            return Json(resultModel);
        }
        /// <summary>
        /// 查询弹出框中其余段的数据
        /// </summary>
        /// <param name="vguids"></param>
        /// <returns></returns>
        public JsonResult GetSectionInfo(string code, string sectionVGUID,string columnName,string keyColumnName)//Guid[] vguids
        {
            var response = new List<SubjectSetting>();
            DbBusinessDataService.Command(db =>
            {
                var checkStr = "";
                response = db.SqlQueryable<SubjectSetting>(@"select bss.checked,bs.Code,bs.Descrption,bs.ParentCode from Business_SevenSection bs 
 left join Business_SubjectSettingInfo bss on bs.Code=bss." + columnName + " and bss."+ keyColumnName + "='" + code + @"'
 where bs.SectionVGUID='"+ sectionVGUID + "' and bs.Status='1' and bs.Code is not null").OrderBy("Code asc").ToList();
                for (int i = 0; i < response.Count; i++)
                {
                    if(response[i].Checked == "True")
                    {
                        checkStr += i + ",";
                    }
                }
                if(checkStr != "")
                {
                    checkStr = checkStr.Substring(0, checkStr.Length - 1);
                }
                response[0].Count = checkStr;
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 删除银行数据
        /// </summary>
        /// <param name="vguids"></param>
        /// <returns></returns>
        public JsonResult DeleteCompanySection(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };

            DbBusinessDataService.Command(db =>
            {
                var data = db.Queryable<Business_SevenSection>();
                foreach (var item in vguids)
                {
                    int saveChanges = 1;
                    var isAny = data.Any(x => x.VGUID == item && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43");
                    if (isAny)
                    {
                        var code = data.Single(x => x.VGUID == item).Code;
                        Delete(code);
                    }
                    else
                    {
                        saveChanges = db.Deleteable<Business_SevenSection>(x => x.VGUID == item).ExecuteCommand();
                    }
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public void Delete(string code)
        {
            DbBusinessDataService.Command(db =>
            {
                var datas = db.Queryable<Business_SevenSection>();
                var isAnyParent = datas.Where(x => x.ParentCode == code && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").ToList();
                if (isAnyParent.Count > 0)
                {
                    foreach (var item in isAnyParent)
                    {
                        Delete(item.Code);
                    }
                    //db.Deleteable<Business_SevenSection>(x => x.ParentCode == code && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").ExecuteCommand();
                }
                else
                {
                    db.Deleteable<Business_SevenSection>(x => x.Code == code && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").ExecuteCommand();
                }
            });
        }
        /// <summary>
        /// 启用禁用数据
        /// </summary>
        /// <param name="vguids"></param>
        /// <returns></returns>
        public JsonResult EditSectionStatus(List<Guid> vguids,string status)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };

            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                var disable = "";
                var data = db.Queryable<Business_SevenSection>();
                foreach (var item in vguids)
                {
                    saveChanges = db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                    { Status = status, VMDFTIME = DateTime.Now, VMDFUSER = UserInfo.LoginName }).Where(it => it.VGUID == item).ExecuteCommand();
                    var isAny = data.Any(x => x.VGUID == item && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43");
                    if (isAny)
                    {
                        //如果是科目表改变状态时要判断上级节点，并递归查询修改
                        var code = data.Single(x => x.VGUID == item && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").Code;
                        var parentCode = data.Single(x => x.VGUID == item && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").ParentCode;
                        if (parentCode != null)
                        {
                            var st = data.Single(x => x.Code == parentCode && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").Status;
                            if(st == "0")
                            {
                                disable = "2";
                            }
                        }
                        EditStatus(code, item, status);
                    }                  
                    resultModel.IsSuccess = true;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                    if (disable == "2") {
                        resultModel.Status = disable;
                    }
                }
            });
            return Json(resultModel);
        }

        private void EditStatus(string code,Guid guid,string status)
        {
            DbBusinessDataService.Command(db =>
            {
                var datas = db.Queryable<Business_SevenSection>();
                var isAnyParent = datas.Where(x => x.ParentCode == code && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").ToList();
                if (isAnyParent.Count > 0)
                {
                    foreach (var item in isAnyParent)
                    {
                        db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                        { Status = status, VMDFTIME = DateTime.Now, VMDFUSER = UserInfo.LoginName }).Where(it => it.VGUID == item.VGUID).ExecuteCommand();
                        EditStatus(item.Code,item.VGUID, status);
                    }
                    //db.Deleteable<Business_SevenSection>(x => x.ParentCode == code && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").ExecuteCommand();
                }
            });
        }

        public JsonResult SaveSectionSetting(string code, List<string> otherCode, string type)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var insertObjs = new List<Business_SubjectSettingInfo>();
                switch (type)
                {
                    #region 科目段设置
                    case "0":
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where CompanyCode = '" + code + "' and SubjectCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.SubjectCode = item;
                                insertObjs.Add(insertObj);
                            }
                            db.Insertable(insertObjs).ExecuteCommand();
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsSubjectCode = true,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        else
                        {
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsSubjectCode = false,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                        break;
                    #endregion
                    #region 核算段设置
                    case "1":
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where CompanyCode = '" + code + "' and AccountingCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.AccountingCode = item;
                                insertObjs.Add(insertObj);
                            }
                            db.Insertable(insertObjs).ExecuteCommand();
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsAccountingCode = true,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        else
                        {
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsAccountingCode = false,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                        break;
                    #endregion
                    #region 成本中心段设置
                    case "2":
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where CompanyCode = '" + code + "' and CostCenterCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.CostCenterCode = item;
                                insertObjs.Add(insertObj);
                            }
                            db.Insertable(insertObjs.ToArray()).ExecuteCommand();
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsCostCenterCode = true,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        else
                        {
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsCostCenterCode = false,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                        break;
                    #endregion
                    #region 备用1段设置
                    case "3":
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where CompanyCode = '" + code + "' and SpareOneCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.SpareOneCode = item;
                                insertObjs.Add(insertObj);
                            }
                            db.Insertable(insertObjs.ToArray()).ExecuteCommand();
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsSpareOneCode = true,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        else
                        {
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsSpareOneCode = false,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                        break;
                    #endregion
                    #region 备用2段设置
                    case "4":
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where CompanyCode = '" + code + "' and SpareTwoCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.SpareTwoCode = item;
                                insertObjs.Add(insertObj);
                            }
                            db.Insertable(insertObjs.ToArray()).ExecuteCommand();
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsSpareTwoCode = true,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        else
                        {
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsSpareTwoCode = false,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }                            
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                        break;
                    #endregion
                    #region 往来段设置
                    case "5":
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where CompanyCode = '" + code + "' and IntercourseCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.IntercourseCode = item;
                                insertObjs.Add(insertObj);
                            }
                            db.Insertable(insertObjs.ToArray()).ExecuteCommand();
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsIntercourseCode = true,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        else
                        {
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsIntercourseCode = false,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }                           
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                        break;
                    #endregion
                    #region 账套段设置
                    case "6":
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where AccountModeCode = '" + code + "' and CompanyCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.AccountModeCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.CompanyCode = item;
                                insertObjs.Add(insertObj);
                            }
                            db.Insertable(insertObjs.ToArray()).ExecuteCommand();
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsCompanyCode = true,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        else
                        {
                            db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            {
                                IsCompanyCode = false,
                            }).Where(it => it.Code == code).ExecuteCommand();
                        }
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                        break;
                    #endregion
                    default:
                        break;
                }               
            });
            return Json(resultModel);
        }
        public JsonResult SaveCompanyBankInfo(Business_CompanyBankInfo cBank, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var any = "";
                var result = db.Ado.UseTran(() =>
                {
                    var guid = cBank.VGUID;
                    var isAny = db.Queryable<Business_CompanyBankInfo>().Any(x => x.CompanyCode == cBank.CompanyCode && x.BankName == cBank.BankName && x.BankAccount == cBank.BankAccount && x.VGUID != cBank.VGUID);
                    if (isAny)
                    {
                        any = "2";
                        return;
                    }
                    if (isEdit)
                    {
                        db.Updateable<Business_CompanyBankInfo>().UpdateColumns(it => new Business_CompanyBankInfo()
                        {
                            BankAccountName = cBank.BankAccountName,
                            BankAccount = cBank.BankAccount,
                            BankName = cBank.BankName,
                        }).Where(it => it.VGUID == guid).ExecuteCommand();
                    }
                    else
                    {
                        cBank.VGUID = Guid.NewGuid();
                        db.Insertable(cBank).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                if(any == "2")
                {
                    resultModel.Status = any;
                }
            });
            return Json(resultModel);
        }
        public JsonResult GetCompanyInfo(string code)//Guid[] vguids
        {
            var response = new List<Business_CompanyBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                response = db.Queryable<Business_CompanyBankInfo>().Where(x=>x.CompanyCode == code).ToList();
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteCompanyBankInfo(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                foreach (var item in vguids)
                {
                    int saveChanges = 1;
                    saveChanges = db.Deleteable<Business_CompanyBankInfo>(x => x.VGUID == item).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
    }
}