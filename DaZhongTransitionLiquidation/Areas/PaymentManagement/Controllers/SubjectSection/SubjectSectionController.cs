using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.SubjectSection
{
    public class SubjectSectionController : BaseController
    {
        // GET: PaymentManagement/SubjectSection
        public SubjectSectionController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
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
        public JsonResult GetCompanySection(string companyCode,string accountModeCodes, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_SevenSection>();
            var response = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                //int pageCount = 0;               
                para.pagenum = para.pagenum + 1;
                //var subjectCode = db.SqlQueryable<Business_SubjectSettingInfo>(@"select SubjectCode from Business_SubjectSettingInfo where CompanyCode='" + companyCode + "'");
                //response = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43" && x.Code != null)
                //.OrderBy(i => i.Code, OrderByType.Asc).ToList();
                response = db.SqlQueryable<Business_SevenSection>(@"select * from Business_SevenSection where SectionVGUID = 'B63BD715-C27D-4C47-AB66-550309794D43' 
                             and CompanyCode='"+ companyCode + "' and AccountModeCode='"+ accountModeCodes + "' and Code is not null ").OrderBy("Code asc").ToList();
                jsonResult.TotalRows = response.Count;
                if (response != null)
                {
                    var data = db.Queryable<Business_SubjectSettingInfo>().ToList();
                    //var datas = db.Queryable<Business_SevenSection>().Where(x=>x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == accountModeCodes && x.CompanyCode == companyCode).ToList();
                    List<Business_SevenSection> sevenList = new List<Business_SevenSection>();
                    List<Business_SevenSection> sevenLists = new List<Business_SevenSection>();
                    foreach (var item in response)
                    {
                        //EditStatus(item.Code,item.VGUID, sevenList, sevenLists, response);
                        var isAnyAccountingCode = data.Any(x => x.SubjectCode == companyCode && x.CompanyCode == item.Code && x.AccountingCode != null && x.AccountModeCode == item.AccountModeCode);
                        if (isAnyAccountingCode)
                        {
                            item.IsAccountingCode = true;
                        }
                        else
                        {
                            item.IsAccountingCode = false;
                        }
                        var isAnyCostCenterCode = data.Any(x => x.SubjectCode == companyCode && x.CompanyCode == item.Code && x.CostCenterCode != null && x.AccountModeCode == item.AccountModeCode);
                        if (isAnyCostCenterCode)
                        {
                            item.IsCostCenterCode = true;
                        }
                        else
                        {
                            item.IsCostCenterCode = false;
                        }
                        var isAnySpareOneCode = data.Any(x => x.SubjectCode == companyCode && x.CompanyCode == item.Code && x.SpareOneCode != null && x.AccountModeCode == item.AccountModeCode);
                        if (isAnySpareOneCode)
                        {
                            item.IsSpareOneCode = true;
                        }
                        else
                        {
                            item.IsSpareOneCode = false;
                        }
                        var isAnySpareTwoCode = data.Any(x => x.SubjectCode == companyCode && x.CompanyCode == item.Code && x.SpareTwoCode != null && x.AccountModeCode == item.AccountModeCode);
                        if (isAnySpareTwoCode)
                        {
                            item.IsSpareTwoCode = true;
                        }
                        else
                        {
                            item.IsSpareTwoCode = false;
                        }
                        var isAnyIntercourseCode = data.Any(x => x.SubjectCode == companyCode && x.CompanyCode == item.Code && x.IntercourseCode != null && x.AccountModeCode == item.AccountModeCode);
                        if (isAnyIntercourseCode)
                        {
                            item.IsIntercourseCode = true;
                        }
                        else
                        {
                            item.IsIntercourseCode = false;
                        }
                    }
                    //db.Updateable(sevenList).ExecuteCommand();
                    //db.Updateable(sevenLists).ExecuteCommand();

                    //List<Business_SubjectSettingInfo> subjectList = new List<Business_SubjectSettingInfo>();
                    //var resp = response.Where(x => x.ParentCode != "" && x.Remark != "1").ToList();
                    //var subjectLists = db.Queryable<Business_SubjectSettingInfo>().Where(x => x.SubjectVGUID == "B63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == accountModeCodes
                    //                    && x.SubjectCode == companyCode).ToList();
                    //foreach (var items in resp)
                    //{
                    //    var isAny = subjectLists.Any(x => x.CompanyCode == items.Code);
                    //    if (!isAny)
                    //    {
                    //        Business_SubjectSettingInfo subject1 = new Business_SubjectSettingInfo();
                    //        subject1.VGUID = Guid.NewGuid();
                    //        subject1.SubjectCode = companyCode;
                    //        subject1.AccountModeCode = accountModeCodes;
                    //        subject1.SubjectVGUID = "B63BD715-C27D-4C47-AB66-550309794D43";
                    //        subject1.Checked = true;
                    //        subject1.AccountingCode = "0";
                    //        subject1.CompanyCode = items.Code;
                    //        subjectList.Add(subject1);
                    //        Business_SubjectSettingInfo subject2 = new Business_SubjectSettingInfo();
                    //        subject2.VGUID = Guid.NewGuid();
                    //        subject2.SubjectCode = companyCode;
                    //        subject2.AccountModeCode = accountModeCodes;
                    //        subject2.SubjectVGUID = "B63BD715-C27D-4C47-AB66-550309794D43";
                    //        subject2.Checked = true;
                    //        subject2.CostCenterCode = "0";
                    //        subject2.CompanyCode = items.Code;
                    //        subjectList.Add(subject2);
                    //        Business_SubjectSettingInfo subject3 = new Business_SubjectSettingInfo();
                    //        subject3.VGUID = Guid.NewGuid();
                    //        subject3.SubjectCode = companyCode;
                    //        subject3.AccountModeCode = accountModeCodes;
                    //        subject3.SubjectVGUID = "B63BD715-C27D-4C47-AB66-550309794D43";
                    //        subject3.Checked = true;
                    //        subject3.SpareOneCode = "0";
                    //        subject3.CompanyCode = items.Code;
                    //        subjectList.Add(subject3);
                    //        Business_SubjectSettingInfo subject4 = new Business_SubjectSettingInfo();
                    //        subject4.VGUID = Guid.NewGuid();
                    //        subject4.SubjectCode = companyCode;
                    //        subject4.AccountModeCode = accountModeCodes;
                    //        subject4.SubjectVGUID = "B63BD715-C27D-4C47-AB66-550309794D43";
                    //        subject4.Checked = true;
                    //        subject4.SpareTwoCode = "0";
                    //        subject4.CompanyCode = items.Code;
                    //        subjectList.Add(subject4);
                    //        Business_SubjectSettingInfo subject5 = new Business_SubjectSettingInfo();
                    //        subject5.VGUID = Guid.NewGuid();
                    //        subject5.SubjectCode = companyCode;
                    //        subject5.AccountModeCode = accountModeCodes;
                    //        subject5.SubjectVGUID = "B63BD715-C27D-4C47-AB66-550309794D43";
                    //        subject5.Checked = true;
                    //        subject5.IntercourseCode = "0";
                    //        subject5.CompanyCode = items.Code;
                    //        subjectList.Add(subject5);
                    //    }
                    //}
                    //if (subjectList.Count > 0)
                    //{
                    //    db.Insertable(subjectList).ExecuteCommand();
                    //}
                }
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        private void EditStatus(string code, Guid guid, List<Business_SevenSection> sevenList, List<Business_SevenSection> sevenLists, List<Business_SevenSection> datas)
        {
            DbBusinessDataService.Command(db =>
            {        
                var isAnyParent = datas.Where(x => x.ParentCode == code).ToList();
                if (isAnyParent.Count > 0)
                {
                    foreach (var item in isAnyParent)
                    {
                        item.Remark = "1";
                        sevenList.Add(item);
                        EditStatus(item.Code, item.VGUID, sevenList, sevenLists, datas);
                    }  
                }
                else
                {
                    var seven = datas.Single(x => x.VGUID == guid);
                    seven.Remark = "";
                    sevenLists.Add(seven);
                }
            });
        }
        public JsonResult GetCompanySectionByCode(string companyCode,string accountModeCode, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_SevenSection>();
            var response = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                //int pageCount = 0;               
                para.pagenum = para.pagenum + 1;
                response = db.SqlQueryable<Business_SevenSection>(@"select bss.checked,bs.Code,bs.Descrption,bs.ParentCode,bs.Remark from Business_SevenSection bs 
 left join Business_SubjectSettingInfo bss on bs.Code=bss.SubjectCode and bss.CompanyCode='" + companyCode + @"'
 and bss.AccountModeCode='" + accountModeCode + "'  where bs.SectionVGUID='B63BD715-C27D-4C47-AB66-550309794D43' and bs.Status='1' and bs.CompanyCode='" + companyCode + "' and bs.AccountModeCode='" + accountModeCode + "' and bs.Code is not null").OrderBy("Code asc").ToList();
                jsonResult.TotalRows = response.Count;
            });
            return Json(response, JsonRequestBehavior.AllowGet);
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
                sevenSection.SectionVGUID = "B63BD715-C27D-4C47-AB66-550309794D43";
                sevenSection.Status = "1";
                sevenSection.VGUID = Guid.NewGuid();
            }
            DbBusinessDataService.Command(db =>
            {
                var IsSuccess = "0";
                var result = db.Ado.UseTran(() =>
                {
                    var guid = sevenSection.VGUID;
                    var code = sevenSection.Code;
                    var parentcode = sevenSection.ParentCode;
                    var isAny = db.Queryable<Business_SevenSection>().Any(x => x.Code == code && x.AccountModeCode == sevenSection.AccountModeCode && x.CompanyCode == sevenSection.CompanyCode && x.VGUID != guid && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43");
                    if (isAny)
                    {
                        IsSuccess = "2";
                        return;
                    }
                    //var isAnyParent = db.Queryable<Business_SevenSection>().Any(x => x.ParentCode == parentcode && x.AccountModeCode == sevenSection.AccountModeCode && x.CompanyCode == sevenSection.CompanyCode && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43");
                    //if (!isAnyParent && parentcode !="" && parentcode != null && !isEdit)
                    //{
                    //    IsSuccess = "3";
                    //    return;
                    //}
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
                if (IsSuccess == "2" || IsSuccess == "3")
                {
                    resultModel.Status = IsSuccess;
                }
            });
            return Json(resultModel);
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
    }
}