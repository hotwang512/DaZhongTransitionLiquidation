var selector = {
    $txtModuleID: function () { return $("#txtModule") },
}
var $page = function () {
    $("#homeModel").text("");
    $("#jqxExpander").jqxExpander({ width: '350px' });
    $("#jqxExpander2").jqxExpander({ width: '350px' });
    this.init = function () {
        $(function () {
            //资金结算中心跳转
            $("#CapitalCenterManagement").click(function () {
                switch (GetHrefPageId("1")) {
                    case "101": CookieHelper.SaveCookie("BankFlowTemplate_CapitalFlow"); window.location.href = "/CapitalCenterManagement/BankFlowTemplate/Index";
                        break;
                    //case "102": CookieHelper.SaveCookie("HRTrainingProposal"); window.location.href = "/TrainingProposal/HRTrainingProposal/HRTrainingProposalIndex"; break;
                    default: jqxNotification("You are not authorized!", null, "error");
                }
            });

            //会计核算中心
            $("#AccountingCenterManagemnet").click(function () {
                if ($("#UserName").val() == "admin") {
                    switch (GetHrefPageId("3")) {
                        case "301": CookieHelper.SaveCookie("VoucherList_VoucherManage"); window.location.href = "/VoucherManageManagement/VoucherList/Index?Status=1&Type=0";
                            break;
                            //case "302": CookieHelper.SaveCookie("DepartmentalTrainingBudgetReport"); window.location.href = "/Report/DepartmentalTrainingBudgetReport/Index"; break;
                            //case "303": CookieHelper.SaveCookie("PersonalTrainingBudgetReport"); window.location.href = "/Report/PersonalTrainingBudgetReport/Index"; break;
                        default: jqxNotification("You are not authorized!", null, "error");
                    }
                } else {
                    $.ajax({
                        url: "/HomePage/HomePage/GetURLInfo",
                        data: { URLName: "VoucherManageManagement" },
                        type: "POST",
                        dataType: "json",
                        async: false,
                        success: function (msg) {
                            if (msg.length > 0) {
                                CookieHelper.SaveCookie(msg[0].CookieName); window.location.href = msg[0].URL;
                            }
                        }
                    });
                }
            });

            //费用报销中心
            $("#ReimbursementCenterManagemnet").click(function () {
                switch (GetHrefPageId("4")) {
                    case "401": CookieHelper.SaveCookie("ROrderListDraft_ROrderListManage"); window.location.href = "/ReimbursementCenter/ROrderListDraft/Index";
                        break;
                    default: jqxNotification("You are not authorized!", null, "error");
                }
            });
            //财务管理中心
            $("#FinancialManagemnet").click(function () {
                switch (GetHrefPageId("5")) {
                    case "501": CookieHelper.SaveCookie("ReportManage_FinancialStatements"); window.location.href = "/FinancialStatementsManagement/SubjectBalanceStatement/Index";
                        break;
                    default: jqxNotification("You are not authorized!", null, "error");
                }
            });

            //驾驶员结算中心
            $("#DriverCenterManagement").click(function () {
                switch (GetHrefPageId("7")) {
                    case "701": CookieHelper.SaveCookie("PaymentHistory_BaseData"); window.location.href = "/PaymentManagement/RevenuePayment/Index";
                        break;
                    default: jqxNotification("You are not authorized!", null, "error");
                }
            });

            //大数据管理中心
            $("#BigDataManagement").click(function () {
                switch (GetHrefPageId("7")) {
                    case "701": CookieHelper.SaveCookie("BigData_AssetPurchase"); window.location.href = "/AssetPurchase/FixedAssetsOrder/Index";
                        break;
                    default: jqxNotification("You are not authorized!", null, "error");
                }
            });

            //税务管理中心
            $("#TaxAuditManagement").click(function () {
                switch (GetHrefPageId("7")) {
                    case "701": CookieHelper.SaveCookie("TaxAudit_TaxCalculation"); window.location.href = "/PaymentManagement/RevenuePayment/Index";
                        break;
                    default: jqxNotification("You are not authorized!", null, "error");

                }
            });
            //预算管理中心
            $("#BudgetManagemnet").click(function () {
                switch (GetHrefPageId("5")) {
                    case "501": CookieHelper.SaveCookie("Budget_RevenueBudget"); window.location.href = "/PaymentManagement/RevenuePayment/Index";
                        break;
                    default: jqxNotification("You are not authorized!", null, "error");
                }
            });
            //审计管理中心
            $("#AuditManagement").click(function () {
                switch (GetHrefPageId("5")) {
                    case "501": CookieHelper.SaveCookie("AuditSelect_LiXinAudit"); window.location.href = "/PaymentManagement/RevenuePayment/Index";
                        break;
                    default: jqxNotification("You are not authorized!", null, "error");
                }
            });
            //统计管理中心
            $("#CountManagement").click(function () {
                switch (GetHrefPageId("5")) {
                    case "501": CookieHelper.SaveCookie("OperationalData_IndustryStatistics"); window.location.href = "/PaymentManagement/RevenuePayment/Index";
                        break;
                    default: jqxNotification("You are not authorized!", null, "error");
                }
            });

            //系统管理
            $("#SystemManagement").click(function () {
                switch (GetHrefPageId("7")) {
                    case "701": CookieHelper.SaveCookie("UserManagement_SystemManage"); window.location.href = "/Systemmanagement/UserManagement/UserInfos";
                        break;
                    default: jqxNotification("You are not authorized!", null, "error");

                }
            });
        });
    }
}

//返回该模块默认加载页面ID
function GetHrefPageId(moduleid) {
    var PageIdList = new Array();
    var PageId = "";
    var PageIdListByModule = new Array();
    PageIdList = selector.$txtModuleID().val().split(",");
    for (var i = 0; i < PageIdList.length; i++) {
        if (PageIdList[i].substr(0, 1).indexOf(moduleid) > -1) {
            PageIdListByModule.push(PageIdList[i]);
        }
    }

    PageId = Math.min.apply(null, PageIdListByModule).toString();
    return PageId;
}
$(function () {
    var page = new $page();
    page.init();
})