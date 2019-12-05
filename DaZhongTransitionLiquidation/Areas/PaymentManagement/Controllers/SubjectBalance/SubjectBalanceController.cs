using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.SubjectBalance
{
    public class SubjectBalanceController : BaseController
    {
        public SubjectBalanceController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: PaymentManagement/SubjectBalance
        public ActionResult Index()
        {
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("dcb08053-9a75-45f0-b5a6-02332d6dc7e3");
            ViewBag.CompanyCode = GetCompanyCode();
            ViewBag.AccountMode = GetAccountMode();
            return View();
        }
        public List<Business_SevenSection> GetCompanyCode()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public JsonResult GetSubjectBalance(string companyCode, string accountModeCode, string year, string month, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_Business_SubjectSettingInfo>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                //重新编译存储过程
                //string sql = string.Format(@" exec sp_recompile @objname = 'usp_SubjectSettingInfo'");
                //var str = db.Ado.SqlQuery<string>(sql).ToString();
                //var data = db.Ado.UseStoredProcedure().SqlQuery<v_Business_SubjectSettingInfo>("usp_SubjectSettingInfo",
                //    new { AccountModeCode = accountModeCode, CompanyCode = companyCode, Year = year, Month = month }).ToList();
                //jsonResult.Rows = data.Skip((para.pagenum - 1) * para.pagesize).Take(para.pagesize).ToList();
                                        string sql = string.Format(@" 
                        declare 	@AccountModeCode		varchar(50),
                        @CompanyCode		varchar(50),
                        @Year             varchar(50),
                        @Month             varchar(50)
                        
                        set @AccountModeCode='{0}'
                        set @CompanyCode='{1}'
                        set @Year=''
                        set @Month=''
                        
                        ;with cte as(
                        select a.AccountModeCode,a.SubjectCode,b.Descrption as Accounting,AccountingCode,c.Descrption as CostCenter,CostCenterCode,d.Descrption as SpareOne
                        ,SpareOneCode,e.Descrption as SpareTwo,SpareTwoCode,f.Descrption as Intercourse,IntercourseCode,a.SubjectVGUID,a.Checked,g.Descrption as Company,a.CompanyCode
                        from Business_SubjectSettingInfo a
                        left join(select * from Business_SevenSection where SectionVGUID='C63BD715-C27D-4C47-AB66-550309794D43') b on b.CompanyCode = a.SubjectCode and b.AccountModeCode = a.AccountModeCode and b.Code=a.AccountingCode
                        left join(select * from Business_SevenSection where SectionVGUID='D63BD715-C27D-4C47-AB66-550309794D43') c on c.CompanyCode = a.SubjectCode and c.AccountModeCode = a.AccountModeCode and c.Code=a.CostCenterCode 
                        left join(select * from Business_SevenSection where SectionVGUID='E63BD715-C27D-4C47-AB66-550309794D43') d on d.CompanyCode = a.SubjectCode and d.AccountModeCode = a.AccountModeCode and d.Code=a.SpareOneCode  
                        left join(select * from Business_SevenSection where SectionVGUID='F63BD715-C27D-4C47-AB66-550309794D43') e on e.CompanyCode = a.SubjectCode and e.AccountModeCode = a.AccountModeCode and e.Code=a.SpareTwoCode
                        left join(select * from Business_SevenSection where SectionVGUID='G63BD715-C27D-4C47-AB66-550309794D43') f on f.CompanyCode = a.SubjectCode and f.AccountModeCode = a.AccountModeCode and f.Code=a.IntercourseCode  
                        left join(select * from Business_SevenSection where SectionVGUID='B63BD715-C27D-4C47-AB66-550309794D43') g on g.CompanyCode = a.SubjectCode and g.AccountModeCode = a.AccountModeCode and g.Code=a.CompanyCode 
                        where a.SubjectVGUID='B63BD715-C27D-4C47-AB66-550309794D43' and a.AccountModeCode=@AccountModeCode and a.SubjectCode=@CompanyCode
                        ),
                        cte1 as(
                        select AccountModeCode,CompanyCode,Company,SubjectCode,SubjectVGUID,Checked,AccountingCode,Accounting,count(AccountingCode) as AccountingCodes,CostCenterCode,CostCenter,count(CostCenterCode) as CostCenterCodes,SpareOneCode,SpareOne,count(SpareOneCode) as 
                        
                        SpareOneCodes,
                        SpareTwoCode,SpareTwo,count(SpareTwoCode) as SpareTwoCodes,IntercourseCode,Intercourse,count(IntercourseCode) as IntercourseCodes
                        from cte group by AccountModeCode,SpareOneCode,CompanyCode,Company,SubjectCode,SubjectVGUID,Checked,AccountingCode,Accounting,CostCenterCode,CostCenter,SpareOne,SpareTwoCode,SpareTwo,IntercourseCode,Intercourse
                        )--select * from cte1
                        
                        select 
                        a.SubjectCode+'.'+(convert(varchar(200),a.CompanyCode)+'.'+isnull(a.AccountingCode,'a')+'.'+isnull(b.CostCenterCode,'b')+'.'+isnull(c.SpareOneCode,'c')+'.'+isnull(d.SpareTwoCode,'d')+'.'+isnull(e.IntercourseCode,'e')) as BusinessCode,
                        a.Company,a.CompanyCode,a.AccountingCode,a.Accounting,
                        b.CostCenterCode,b.CostCenter,c.SpareOneCode,c.SpareOne,d.SpareTwoCode,d.SpareTwo,e.IntercourseCode,e.Intercourse,
                        a.SubjectCode,a.SubjectVGUID,a.Checked,a.AccountModeCode,f.Balance
                        from cte1 a 
                        full join cte1 b on a.AccountingCodes=b.CostCenterCodes and a.CompanyCode=b.CompanyCode 
                        full join cte1 c on a.AccountingCodes=c.SpareOneCodes and a.CompanyCode=c.CompanyCode
                        full join cte1 d on a.AccountingCodes=d.SpareTwoCodes and a.CompanyCode=d.CompanyCode 
                        full join cte1 e on a.AccountingCodes=e.IntercourseCodes and a.CompanyCode=e.CompanyCode
                        left join Business_SubjectBalance as f on f.Code = a.SubjectCode+'.'+(convert(varchar(200),a.CompanyCode)+'.'+isnull(a.AccountingCode,'a')+'.'+isnull(b.CostCenterCode,'b')+'.'+isnull(c.SpareOneCode,'c')+'.'+isnull(d.SpareTwoCode,'d')+'.'+isnull(e.IntercourseCode,'e'))
                        and f.Year = @Year and f.Month = @Month and f.AccountModeCode = @AccountModeCode and f.CompanyCode = @CompanyCode
                        where a.AccountingCodes=1 ", accountModeCode, companyCode);
                var data = db.Ado.SqlQuery<v_Business_SubjectSettingInfo>(sql);




                //string sql = string.Format(@" exec usp_SubjectSettingInfo  '{0}','{1}','{2}','{3}' ", accountModeCode, companyCode, year, month);
                //var data = db.SqlQueryable<v_Business_SubjectSettingInfo>(sql).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.Rows = data;
                jsonResult.TotalRows = pageCount;
            });
            return Json(
                 jsonResult.Rows,
                 "application/json",
                 Encoding.UTF8,
                 JsonRequestBehavior.AllowGet
             );
        }
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }
        public JsonResult SaveSubjectBalance(decimal Balance, string Code, string Year, string Month, string AccountModeCode, string CompanyCode)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (Code != "")
                    {
                        db.Deleteable<Business_SubjectBalance>(x => x.Code == Code && x.Year == Year && x.Month == Month && x.AccountModeCode == AccountModeCode && x.CompanyCode == CompanyCode).ExecuteCommand();
                        Business_SubjectBalance balance = new Business_SubjectBalance();
                        balance.VGUID = Guid.NewGuid();
                        balance.Code = Code;
                        balance.Balance = Balance;
                        balance.Year = Year;
                        balance.Month = Month;
                        balance.AccountModeCode = AccountModeCode;
                        balance.CompanyCode = CompanyCode;
                        db.Insertable(balance).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}