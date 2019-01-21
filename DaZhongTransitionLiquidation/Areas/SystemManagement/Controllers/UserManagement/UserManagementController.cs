using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using SyntacticSugar;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.UserManagement
{
    public class UserManagementController : BaseController
    {
        // GET: SystemManagement/UserManagement

        public UserManagementController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        public ActionResult UserInfos()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.UserManagement);
            ViewData["Role"] = GetRoleInfos();
            ViewBag.UserSubDepartment = GetUserSubDepartment().ModelToJson();
            ViewBag.IsAdmin = UserInfo.Vguid.ToString() == MasterVGUID.AdminVguid.ToLower();
            return View();
        }

        public ActionResult UserInfoDetail()
        {
            ViewBag.IsEdit = Request["isEdit"].TryToBoolean();
            ViewData["Vguid"] = Request["Vguid"] ?? "";
            ViewData["Role"] = GetRoleInfos();
            ViewBag.UserSubDepartment = GetUserSubDepartment().ModelToJson();
            return View();
        }


        /// <summary>
        /// 绑定权限下拉框
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetRoleInfos()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            DbService.Command(db =>
            {
                var roleInfos = db.Queryable<Sys_Role>().ToList();
                foreach (var roleInfo in roleInfos)
                {
                    SelectListItem item = new SelectListItem()
                    {
                        Value = roleInfo.Vguid.ToString(),
                        Text = roleInfo.Role
                    };
                    selectListItems.Add(item);
                }
            });
            return selectListItems;
        }
        /// <summary>
        /// 获取用户信息列表
        /// </summary>
        /// <param name="para">分页信息</param>
        /// <param name="searchParams">查询条件</param>
        /// <returns>用户列表</returns>
        public JsonResult GetUserInfos(Sys_User searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<V_User_Information>();
            DbService.Command<UserManagementPack>((db, o) =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<V_User_Information>().Where(o.GetConditionalModels(searchParams))
                .OrderBy(i => i.CreatedDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 删除用户信息
        /// </summary>
        /// <param name="vguids">主键</param>
        /// <returns></returns>
        public JsonResult DeleteUserInfos(Guid[] vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbService.Command(db =>
            {
                try
                {
                    db.Ado.BeginTran();
                    db.Deleteable<Sys_User>(vguids).ExecuteCommand();
                    db.Ado.CommitTran();
                    resultModel.Status = "1";
                }
                catch (Exception ex)
                {
                    db.Ado.RollbackTran();
                    resultModel.ResultInfo = ex.Message;
                }
            });
            return Json(resultModel);
        }
        /// <summary>
        /// 改变用户状态
        /// </summary>
        /// <param name="vguids">主键</param>
        /// <param name="status">要改变成的状态</param>
        /// <returns></returns>
        public JsonResult ChangeUserStatus(Guid[] vguids, string status)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var users = vguids.Select(vguid => new Sys_User { Vguid = vguid, Enable = status, ChangeDate = DateTime.Now, ChangeUser = UserInfo.LoginName }).ToList();
            DbService.Command(db =>
            {
                try
                {
                    db.Ado.BeginTran();
                    db.Updateable(users).UpdateColumns(i => new { i.Enable, i.ChangeDate, i.ChangeUser }).ExecuteCommand();
                    db.Ado.CommitTran();
                    resultModel.Status = "1";
                }
                catch (Exception ex)
                {
                    db.Ado.RollbackTran();
                    resultModel.ResultInfo = ex.Message;
                }
            });
            return Json(resultModel);
        }

        /// <summary>
        /// 获取部门组织树
        /// </summary>
        /// <returns></returns>
        public JsonResult GetOrganizationTree()
        {
            var organizations = new List<Master_Organization>();
            DbService.Command(db =>
            {
                var currentDepartment = UserInfo.Department;
                var mainDepVguid = db.Queryable<Master_Organization>().Where(i => i.ParentVguid == null).Select(i => i.Vguid).Single();
                organizations = db.Ado.UseStoredProcedure().SqlQuery<Master_Organization>("usp_OrganizationDetail", new { orgvguid = currentDepartment ?? mainDepVguid.ToString() });
            });
            return Json(organizations);
        }
        /// <summary>
        /// 保存用户信息（新增或更新）
        /// </summary>
        /// <param name="userInfo">用户信息</param>
        /// <param name="isEdit">是否编辑</param>
        /// <returns></returns>
        public JsonResult SaveUserInfo(Sys_User userInfo, bool isEdit, string gjson)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var guid = Guid.NewGuid();
            var uGuid = guid.TryToString();
            DbService.Command<UserManagementPack>((db, o) =>
            {
                if (o.IsExistLoginName(db, userInfo, isEdit))
                {
                    resultModel.Status = "2";
                    return;
                }
                if (isEdit)
                {
                    userInfo.ChangeDate = DateTime.Now;
                    userInfo.ChangeUser = userInfo.LoginName;
                    resultModel.IsSuccess = db.Updateable(userInfo).IgnoreColumns(i => new { i.CreatedDate, i.CreatedUser, i.Password }).ExecuteCommand() > 0;
                }
                else
                {
                    userInfo.Password = "123456";
                    userInfo.Vguid = guid;
                    userInfo.CreatedDate = DateTime.Now;
                    userInfo.CreatedUser = userInfo.LoginName;
                    resultModel.IsSuccess = db.Insertable(userInfo).ExecuteCommand() > 0;
                }
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            DbBusinessDataService.Command(db =>
            {
                var data = gjson.JsonToModel<List<Business_UserCompanySet>>();
                db.Deleteable<Business_UserCompanySet>(x => x.UserVGUID == uGuid).ExecuteCommand();
                if(userInfo.Vguid != Guid.Empty)
                {
                    uGuid = userInfo.Vguid.TryToString();
                    data.ForEach(w => w.UserVGUID = uGuid);
                }
                else
                {
                    data.ForEach(w => w.UserVGUID = uGuid);
                }
                data.ForEach(x => x.VGUID = Guid.NewGuid());
                db.Insertable<Business_UserCompanySet>(data).ExecuteCommand();
            });
            return Json(resultModel);
        }

        /// <summary>
        /// 根据主键获取具体人员信息
        /// </summary>
        /// <param name="vguid">主键</param>
        /// <returns></returns>
        public JsonResult GetUserInfoByVguid(Guid vguid)
        {
            Sys_User userInfo = new Sys_User();
            DbService.Command(db =>
            {
                userInfo = db.Queryable<Sys_User>().Single(i => i.Vguid == vguid);
            });
            return Json(userInfo);
        }

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="userInfo"></param>
        ///  <param name="oldPassword">旧密码</param>
        /// <returns></returns>
        public JsonResult ChangePassword(Sys_User userInfo, string oldPassword)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbService.Command(db =>
            {
                var isOldPasswordCorrect = db.Queryable<Sys_User>().Any(i => i.Password == oldPassword && i.Vguid == userInfo.Vguid);
                if (!isOldPasswordCorrect)
                {
                    resultModel.Status = "2";
                    return;
                }
                userInfo.ChangeDate = DateTime.Now;
                userInfo.ChangeUser = UserInfo.LoginName;
                resultModel.IsSuccess = db.Updateable(userInfo).UpdateColumns(i => new { i.Password, i.ChangeUser, i.ChangeDate }).ExecuteCommand() > 0;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="userStr">选中的用户</param>
        /// <returns></returns>
        public JsonResult ResetPassword(string userStr)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var userInfos = userStr.JsonToModel<List<Sys_User>>();
            DbService.Command(db =>
            {
                var changes = db.Updateable(userInfos).UpdateColumns(it => it.Password).ExecuteCommand();
                resultModel.IsSuccess = changes == userInfos.Count;
            });
            return Json(resultModel);
        }


        public JsonResult GetUserCompanyInfo(string UserVGUID)//Guid[] vguids
        {
            var response = new List<Business_UserCompanySet>();
            DbBusinessDataService.Command(db =>
            {
                var data = db.Queryable<Business_UserCompanySet>().Where(x => x.UserVGUID == UserVGUID).Count();
                if(data == 0)
                {
                    response = db.SqlQueryable<Business_UserCompanySet>(@"select t1.Code,t1.Descrption,t2.Code as CompanyCode ,t2.Descrption as CompanyName,
 (t1.Code+t2.Code) as KeyData,t3.IsCheck,t3.Block,t3.UserVGUID from Business_SevenSection t1 
 JOIN Business_SevenSection t2 on cast(t1.VGUID as varchar(36)) <>t2.SectionVGUID
 left join Business_UserCompanySet as t3 on t3.KeyData = (t1.Code+t2.Code)
where t1.SectionVGUID='H63BD715-C27D-4C47-AB66-550309794D43' and t2.SectionVGUID='A63BD715-C27D-4C47-AB66-550309794D43'").OrderBy("Code asc,CompanyCode asc").ToList();
                }
                else
                {
                    response = db.SqlQueryable<Business_UserCompanySet>(@"select t1.Code,t1.Descrption,t2.Code as CompanyCode ,t2.Descrption as CompanyName,
 (t1.Code+t2.Code) as KeyData,t3.IsCheck,t3.Block,t3.UserVGUID from Business_SevenSection t1 
 JOIN Business_SevenSection t2 on cast(t1.VGUID as varchar(36)) <>t2.SectionVGUID
 left join Business_UserCompanySet as t3 on t3.KeyData = (t1.Code+t2.Code)
where t1.SectionVGUID='H63BD715-C27D-4C47-AB66-550309794D43' and t2.SectionVGUID='A63BD715-C27D-4C47-AB66-550309794D43' and t3.UserVGUID='" + UserVGUID + @"'
").OrderBy("Code asc,CompanyCode asc").ToList();
                }
                
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}