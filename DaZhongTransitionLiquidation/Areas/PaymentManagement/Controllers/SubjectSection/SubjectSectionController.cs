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
        public JsonResult GetCompanySection(string companyCode,GridParams para)
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
                              and Code is not null and Code in (select SubjectCode from Business_SubjectSettingInfo where CompanyCode = '" + companyCode + "')").OrderBy("Code asc").ToList();
                jsonResult.TotalRows = response.Count;
                if (response != null)
                {
                    var data = db.Queryable<Business_SubjectSettingInfo>();
                    foreach (var item in response)
                    {
                        var isAnyAccountingCode = data.Any(x => x.SubjectCode == companyCode && x.CompanyCode == item.Code && x.AccountingCode != null);
                        if (isAnyAccountingCode)
                        {
                            item.IsAccountingCode = true;
                        }
                        else
                        {
                            item.IsAccountingCode = false;
                        }
                        var isAnyCostCenterCode = data.Any(x => x.SubjectCode == companyCode && x.CompanyCode == item.Code && x.CostCenterCode != null);
                        if (isAnyCostCenterCode)
                        {
                            item.IsCostCenterCode = true;
                        }
                        else
                        {
                            item.IsCostCenterCode = false;
                        }
                        var isAnySpareOneCode = data.Any(x => x.SubjectCode == companyCode && x.CompanyCode == item.Code && x.SpareOneCode != null);
                        if (isAnySpareOneCode)
                        {
                            item.IsSpareOneCode = true;
                        }
                        else
                        {
                            item.IsSpareOneCode = false;
                        }
                        var isAnySpareTwoCode = data.Any(x => x.SubjectCode == companyCode && x.CompanyCode == item.Code && x.SpareTwoCode != null);
                        if (isAnySpareTwoCode)
                        {
                            item.IsSpareTwoCode = true;
                        }
                        else
                        {
                            item.IsSpareTwoCode = false;
                        }
                        var isAnyIntercourseCode = data.Any(x => x.SubjectCode == companyCode && x.CompanyCode == item.Code && x.IntercourseCode != null);
                        if (isAnyIntercourseCode)
                        {
                            item.IsIntercourseCode = true;
                        }
                        else
                        {
                            item.IsIntercourseCode = false;
                        }
                    }
                }
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
                    var isAny = db.Queryable<Business_SevenSection>().Any(x => x.Code == code && x.VGUID != guid && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43");
                    if (isAny)
                    {
                        IsSuccess = "2";
                        return;
                    }
                    var isAnyParent = db.Queryable<Business_SevenSection>().Any(x => x.ParentCode == parentcode && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43");
                    if (!isAnyParent && parentcode !="" && !isEdit)
                    {
                        IsSuccess = "3";
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