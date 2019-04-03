﻿using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
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
            ViewBag.CompanyCode = GetCompanyCode();
            ViewBag.AccountMode = GetAccountMode();
            //ViewBag.BankChannel = GetBankChannel();
            return View();
        }
        public JsonResult GetCompanySection(GridParams para,string accountModeCode)
        {
            var jsonResult = new JsonResultModel<Business_SevenSection>();
            var response = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == accountModeCode)
                .OrderBy(i => i.Code, OrderByType.Asc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                if (jsonResult.Rows != null)
                {
                    var data = db.Queryable<Business_SubjectSettingInfo>().ToList();
                    foreach (var item in jsonResult.Rows)
                    {
                        var isAnySubjectCode = data.Any(x => x.AccountModeCode == accountModeCode && x.CompanyCode == item.Code && x.SubjectCode != null);
                        if (isAnySubjectCode)
                        {
                            item.IsSubjectCode = true;
                        }
                        else
                        {
                            item.IsSubjectCode = false;
                        }
                        var isAnyCompanyBank = db.Queryable<Business_CompanyBankInfo>().Any(x => x.AccountModeCode == accountModeCode && x.CompanyCode == item.Code);
                        if (isAnyCompanyBank)
                        {
                            item.IsCompanyBank = true;
                        }
                        else
                        {
                            item.IsCompanyBank = false;
                        }
                    }
                }
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
                    var isAny = db.Queryable<Business_SevenSection>().Any(x => x.Code == code && x.AccountModeCode == sevenSection.AccountModeCode && x.VGUID != guid && x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43");
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
                            AccountModeCode = sevenSection.AccountModeCode,
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
        public JsonResult GetSectionInfo(string code, string sectionVGUID,string columnName,string keyColumnName,int index,string companyCode,string accountModeCode)//Guid[] vguids
        {
            var response = new List<SubjectSetting>();
            DbBusinessDataService.Command(db =>
            {
                var checkStr = "";
                if(index == 1)
                {
                    response = db.SqlQueryable<SubjectSetting>(@"select bss.checked,bs.Code,bs.Descrption,bs.ParentCode from Business_SevenSection bs 
 left join Business_SubjectSettingInfo bss on bs.Code=bss." + columnName + " and bss." + keyColumnName + "='" + code + @"'
 and bss.AccountModeCode='" + accountModeCode + "'  where bs.SectionVGUID='" + sectionVGUID + "' and bs.Status='1' and bs.CompanyCode='" + code + "' and bs.AccountModeCode='" + accountModeCode + "' and bs.Code is not null").OrderBy("Code asc").ToList();
                }
                if(index == 2)
                {
                    response = db.SqlQueryable<SubjectSetting>(@"select bss.checked,bs.Code,bs.Descrption,bs.ParentCode from Business_SevenSection bs 
 left join Business_SubjectSettingInfo bss on bs.Code=bss." + columnName + " and bss." + keyColumnName + "='" + code + @"'
 and bss.SubjectCode='" + companyCode + "' and bss.AccountModeCode='" + accountModeCode + "'   where bs.SectionVGUID='" + sectionVGUID + "' and bs.Status='1' and bs.CompanyCode='" + companyCode + "' and bs.AccountModeCode='" + accountModeCode + "'  and bs.Code is not null").OrderBy("Code asc").ToList();
                }
                if (index != 1 && index != 2)
                {
                    if(columnName == "CompanyCode")
                    {
                        response = db.SqlQueryable<SubjectSetting>(@"select bss.checked,bs.Code,bs.Descrption,bs.ParentCode from Business_SevenSection bs 
 left join Business_SubjectSettingInfo bss on bs.Code=bss." + columnName + " and bss." + keyColumnName + "='" + code + @"' and bss.SubjectVGUID='H63BD715-C27D-4C47-AB66-550309794D43'
 where bs.SectionVGUID='" + sectionVGUID + "' and bs.Status='1' and bs.AccountModeCode='"+ code + "' and bs.Code is not null").OrderBy("Code asc").ToList();
                    }
                    else
                    {
                        response = db.SqlQueryable<SubjectSetting>(@"select bss.checked,bs.Code,bs.Descrption,bs.ParentCode from Business_SevenSection bs 
 left join Business_SubjectSettingInfo bss on bs.Code=bss." + columnName + " and bss." + keyColumnName + "='" + code + @"' 
 where bs.SectionVGUID='" + sectionVGUID + "' and bs.CompanyCode='" + companyCode + "' and bs.AccountModeCode='" + accountModeCode + "' and  bs.Status='1' and bs.Code is not null").OrderBy("Code asc").ToList();
                    }
                }
                if(columnName == "SubjectCode" && response.Count>0)
                {
                    for (int i = 0; i < response.Count; i++)
                    {
                        if (response[i].Checked == "True")
                        {
                            checkStr += i + ",";
                        }
                    }
                    if (checkStr != "")
                    {
                        checkStr = checkStr.Substring(0, checkStr.Length - 1);
                    }
                    response[0].Count = checkStr;
                } 
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
                var data = db.Queryable<Business_SevenSection>().ToList();
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
                        var companyCode = data.Single(x => x.VGUID == item).CompanyCode;
                        var accountModeCode = data.Single(x => x.VGUID == item).AccountModeCode;
                        if (parentCode != null)
                        {
                            var st = data.Single(x => x.Code == parentCode && x.AccountModeCode == accountModeCode && x.CompanyCode == companyCode).Status;
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

        public JsonResult SaveSectionSetting(string code, List<string> otherCode, string type,string companyCode,string accountModeCode)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var insertObjs = new List<Business_SubjectSettingInfo>();
                switch (type)
                {
                    #region 科目段设置
                    case "0":
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where AccountModeCode = '" + accountModeCode + "' and  CompanyCode = '" + code + "' and SubjectCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.SubjectCode = item;
                                insertObj.SubjectVGUID = "A63BD715-C27D-4C47-AB66-550309794D43";
                                insertObj.AccountModeCode = accountModeCode;
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
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where AccountModeCode = '" + accountModeCode + "' and CompanyCode = '" + code + "' and SubjectCode = '" + companyCode + "'  and AccountingCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.AccountingCode = item;
                                insertObj.SubjectCode = companyCode;//公司键
                                insertObj.AccountModeCode = accountModeCode;//账套
                                insertObj.SubjectVGUID = "B63BD715-C27D-4C47-AB66-550309794D43";
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
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where AccountModeCode = '" + accountModeCode + "' and CompanyCode = '" + code + "' and SubjectCode = '" + companyCode + "' and CostCenterCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.CostCenterCode = item;
                                insertObj.SubjectCode = companyCode;//公司键
                                insertObj.AccountModeCode = accountModeCode;//账套
                                insertObj.SubjectVGUID = "B63BD715-C27D-4C47-AB66-550309794D43";
                                insertObjs.Add(insertObj);
                            }
                            db.Insertable(insertObjs.ToArray()).ExecuteCommand();
                            //db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            //{
                            //    IsCostCenterCode = true,
                            //}).Where(it => it.Code == code).ExecuteCommand();
                        }
                        //else
                        //{
                        //    db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                        //    {
                        //        IsCostCenterCode = false,
                        //    }).Where(it => it.Code == code).ExecuteCommand();
                        //}
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                        break;
                    #endregion
                    #region 备用1段设置
                    case "3":
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where AccountModeCode = '" + accountModeCode + "' and CompanyCode = '" + code + "' and SubjectCode = '" + companyCode + "' and SpareOneCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.SpareOneCode = item;
                                insertObj.SubjectCode = companyCode;//公司键
                                insertObj.AccountModeCode = accountModeCode;//账套
                                insertObj.SubjectVGUID = "B63BD715-C27D-4C47-AB66-550309794D43";
                                insertObjs.Add(insertObj);
                            }
                            db.Insertable(insertObjs.ToArray()).ExecuteCommand();
                            //db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            //{
                            //    IsSpareOneCode = true,
                            //}).Where(it => it.Code == code).ExecuteCommand();
                        }
                        //else
                        //{
                        //    db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                        //    {
                        //        IsSpareOneCode = false,
                        //    }).Where(it => it.Code == code).ExecuteCommand();
                        //}
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                        break;
                    #endregion
                    #region 备用2段设置
                    case "4":
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where AccountModeCode = '" + accountModeCode + "' and CompanyCode = '" + code + "' and SubjectCode = '" + companyCode + "' and SpareTwoCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.SpareTwoCode = item;
                                insertObj.SubjectCode = companyCode;//公司键
                                insertObj.AccountModeCode = accountModeCode;//账套
                                insertObj.SubjectVGUID = "B63BD715-C27D-4C47-AB66-550309794D43";
                                insertObjs.Add(insertObj);
                            }
                            db.Insertable(insertObjs.ToArray()).ExecuteCommand();
                            //db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                            //{
                            //    IsSpareTwoCode = true,
                            //}).Where(it => it.Code == code).ExecuteCommand();
                        }
                        //else
                        //{
                        //    db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                        //    {
                        //        IsSpareTwoCode = false,
                        //    }).Where(it => it.Code == code).ExecuteCommand();
                        //}                            
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                        break;
                    #endregion
                    #region 往来段设置
                    case "5":
                        db.Ado.ExecuteCommand(@"delete Business_SubjectSettingInfo where AccountModeCode = '" + accountModeCode + "' and CompanyCode = '" + code + "' and SubjectCode = '" + companyCode + "' and IntercourseCode is not null");
                        if (otherCode != null)
                        {
                            foreach (var item in otherCode)
                            {
                                var insertObj = new Business_SubjectSettingInfo();
                                insertObj.CompanyCode = code;
                                insertObj.Checked = true;
                                insertObj.VGUID = Guid.NewGuid();
                                insertObj.IntercourseCode = item;
                                insertObj.SubjectCode = companyCode;//公司键
                                insertObj.AccountModeCode = accountModeCode;//账套
                                insertObj.SubjectVGUID = "B63BD715-C27D-4C47-AB66-550309794D43";
                                insertObjs.Add(insertObj);
                            }
                            db.Insertable(insertObjs.ToArray()).ExecuteCommand();
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
                                insertObj.SubjectVGUID = "H63BD715-C27D-4C47-AB66-550309794D43";
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
                    var data = db.Queryable<Business_CompanyBankInfo>();
                    var guid = cBank.VGUID;
                    var isAny = data.Any(x => x.CompanyCode == cBank.CompanyCode && x.AccountModeCode == cBank.AccountModeCode && x.BankName == cBank.BankName && x.BankAccount == cBank.BankAccount && x.VGUID != cBank.VGUID);
                    if (isAny)
                    {
                        any = "2";
                        return;
                    }
                    var isAnyAccount = data.Any(x => x.AccountType == cBank.AccountType && x.CompanyCode == cBank.CompanyCode && x.AccountModeCode == cBank.AccountModeCode && x.VGUID != cBank.VGUID);
                    if (isAnyAccount)
                    {
                        switch (cBank.AccountType)
                        {
                            case "基本户": any = "3";break;
                            case "社保账户": any = "4"; break;
                            default:
                                break;
                        }
                    }
                    if (isEdit)
                    {
                        db.Updateable<Business_CompanyBankInfo>().UpdateColumns(it => new Business_CompanyBankInfo()
                        {
                            BankAccountName = cBank.BankAccountName,
                            BankAccount = cBank.BankAccount,
                            BankName = cBank.BankName,
                            AccountType = cBank.AccountType,
                            InitialBalance = cBank.InitialBalance,
                            AccountModeCode = cBank.AccountModeCode,
                            Borrow = cBank.Borrow,
                            Loan = cBank.Loan
                        }).Where(it => it.VGUID == guid).ExecuteCommand();
                    }
                    else
                    {
                        cBank.VGUID = Guid.NewGuid();
                        db.Insertable(cBank).ExecuteCommand();
                        db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                        {
                            IsCompanyBank = true,
                        }).Where(it => it.Code == cBank.CompanyCode).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                if(any != "")
                {
                    resultModel.Status = any;
                }
            });
            return Json(resultModel);
        }
        public JsonResult GetCompanyInfo(string code,string accountModeCode)//Guid[] vguids
        {
            var response = new List<Business_CompanyBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                if(accountModeCode == null)
                {
                    accountModeCode = UserInfo.AccountModeCode;
                }
                response = db.Queryable<Business_CompanyBankInfo>().Where(x=>x.CompanyCode == code && x.AccountModeCode == accountModeCode).ToList();
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAccountCompanyInfo(string code)//Guid[] vguids
        {
            var response = new List<V_AccountSetting>();
            DbBusinessDataService.Command(db =>
            {
                response = db.SqlQueryable<V_AccountSetting>(@"select bs.VGUID,bs.BankName,bs.BankAccount,bs.CompanyCode
  ,bs.BankAccountName,bs.AccountType,bs.CompanyName,bss.IsChecked  from V_AccountSetting bs 
  left join Business_AccountSettingInfo bss on bs.VGUID=bss.BankVGUID 
  and bss.AccountCode='" + code + "' where bs.CompanyCode in (select SubjectCode from Business_SubjectSettingInfo where AccountingCode= '" + code + "')").ToList();
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteCompanyBankInfo(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var companyCode = "";
                companyCode = db.Queryable<Business_CompanyBankInfo>().Single(x => x.VGUID == vguids[0]).CompanyCode;
                foreach (var item in vguids)
                {
                    int saveChanges = 1;
                    saveChanges = db.Deleteable<Business_CompanyBankInfo>(x => x.VGUID == item).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
                var data = db.Queryable<Business_CompanyBankInfo>().Where(x=>x.CompanyCode == companyCode).ToList();
                if(data.Count == 0)
                {
                    db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                    {
                        IsCompanyBank = false,
                    }).Where(it => it.Code == companyCode).ExecuteCommand();
                }
            });
            return Json(resultModel);
        }
        public JsonResult SaveAccoutSetting(string code, List<Guid> vguid, string companyCode, string accountModeCode)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var insertObjs = new List<Business_AccountSettingInfo>();
                db.Ado.ExecuteCommand(@"delete Business_AccountSettingInfo where CompanyCode = '" + companyCode + "' and AccountModeCode = '" + accountModeCode + "' and AccountCode = '" + code + "'");
                if (vguid != null)
                {
                    foreach (var item in vguid)
                    {
                        var insertObj = new Business_AccountSettingInfo();
                        insertObj.AccountCode = code;
                        insertObj.AccountModeCode = accountModeCode;
                        insertObj.CompanyCode = companyCode;
                        insertObj.IsChecked = true;
                        insertObj.VGUID = Guid.NewGuid();
                        insertObj.BankVGUID = item;
                        insertObjs.Add(insertObj);
                    }
                    db.Insertable(insertObjs).ExecuteCommand();
                    db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                    {
                        IsSetAccount = true,
                    }).Where(it => it.Code == code).ExecuteCommand();
                }
                else
                {
                    db.Updateable<Business_SevenSection>().UpdateColumns(it => new Business_SevenSection()
                    {
                        IsSetAccount = false,
                    }).Where(it => it.Code == code).ExecuteCommand();
                }
                resultModel.IsSuccess = true;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";

            });
            return Json(resultModel);
        }
        public List<Business_SevenSection> GetCompanyCode()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x=>x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public List<Business_SevenSection> GetAccountMode()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public JsonResult UpdataBankStatus(Guid vguids,string datafield,bool ischeck,string accountModeCode,string companyCode)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                if(datafield == "BankStatus")
                {
                    db.Updateable<Business_CompanyBankInfo>().UpdateColumns(it => new Business_CompanyBankInfo()
                    {
                        BankStatus = false,
                    }).Where(x => x.BankStatus == true && x.AccountModeCode == accountModeCode && x.CompanyCode == companyCode).ExecuteCommand();
                    db.Updateable<Business_CompanyBankInfo>().UpdateColumns(it => new Business_CompanyBankInfo()
                    {
                        BankStatus = true,
                    }).Where(it => it.VGUID == vguids).ExecuteCommand();
                }
                else
                {
                    db.Updateable<Business_CompanyBankInfo>().UpdateColumns(it => new Business_CompanyBankInfo()
                    {
                        OpeningDirectBank = ischeck,
                    }).Where(it => it.VGUID == vguids).ExecuteCommand();
                }
            });
            return Json(resultModel);
        }
        public JsonResult SynchronousData(string companyCode, string accountModeCode,string index)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var sectionVguid = "";
                switch (index)
                {
                    case "2": sectionVguid = "B63BD715-C27D-4C47-AB66-550309794D43"; break;//科目
                    case "3": sectionVguid = "C63BD715-C27D-4C47-AB66-550309794D43"; break;//核算
                    case "4": sectionVguid = "D63BD715-C27D-4C47-AB66-550309794D43"; break;//成本中心
                    case "5": sectionVguid = "E63BD715-C27D-4C47-AB66-550309794D43"; break;//备用1
                    case "6": sectionVguid = "F63BD715-C27D-4C47-AB66-550309794D43"; break;//备用2
                    case "7": sectionVguid = "G63BD715-C27D-4C47-AB66-550309794D43"; break;//往来段
                    default:
                        break;
                }
                List<Business_SevenSection> SevenSection = new List<Business_SevenSection>();
                var data = db.Queryable<Business_SevenSection>().Where(x => x.AccountModeCode == "1002" && x.CompanyCode == "01" && x.SectionVGUID == sectionVguid).ToList();
                foreach (var item in data)
                {
                    item.VGUID = Guid.NewGuid();
                    item.VCRTTIME = DateTime.Now;
                    item.CompanyCode = companyCode;
                    item.AccountModeCode = accountModeCode;
                    SevenSection.Add(item);
                }
                db.Insertable(SevenSection).ExecuteCommand();
            });
            return Json(resultModel);
        }
    }
}