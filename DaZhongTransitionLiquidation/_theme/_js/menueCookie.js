var CookieHelper = {
    SaveCookie: function (menueId) { //保存信息至Cookie    
        if (window.sessionStorage) {
            sessionStorage.setItem('DaZhongTransitionLiquidationSystemMenue', escape(menueId));
        } else {
            var Days = 30;
            var exp = new Date();
            exp.setTime(exp.getTime() + Days * 24 * 60 * 60 * 1000);
            document.cookie = "DaZhongTransitionLiquidationSystemMenue=" + escape(menueId) + ";expires=" + exp.toGMTString() + ";path=/";
        }
        // window.location.href = Config.Html.html_Index;
    },
    GetCookie: function () {  //获取Cookie中信息
        if (window.sessionStorage) {
            return sessionStorage.getItem("DaZhongTransitionLiquidationSystemMenue");
        }
        var arr, reg = new RegExp("(^| )DaZhongTransitionLiquidationSystemMenue=([^;]*)(;|$)");
        if (arr = document.cookie.match(reg)) {
            return unescape(arr[2]);
        }
        else {
            return null;
            // window.location.href = Config.Html.html_Login;
        }
    },
    CheckCookie: function () {  //检测cookie是否存在      
        if (document.cookie.indexOf("DaZhongTransitionLiquidationSystemMenue=") <= -1) {
            return false;
        }
        else {
            return true;
        }
    },
    ClearCookie: function () {//清空Cookie中的用户信息
        var expires = new Date();
        expires.setTime(expires.getTime() - 1000); //当前时间减去一秒,相当于立即过期(可以增减) 
        document.cookie = "DaZhongTransitionLiquidationSystemMenue='';expires=" + expires.toGMTString() + ";path=/"; //expires是对应过期时间的设置,不设这个值,cookie默认在关闭浏览器时失效 

    }
}



var menueHelp = {
    drawMenueStyle: function () {
        var menueId = CookieHelper.GetCookie();
        if (menueId == "" || menueId == null) {
            return;
        }
        var id = menueId.split("_")[1];
        $(".menue_header").css("display", "none");
        $("." + id).show();
        $(".menue_body").css("display", "none");
        $("#" + menueId).parents(".menue_body").css("display", "block");
        $("#" + id).show();
        //$(".menue_bodychild").css("display", "none");
        //$("#" + menueId).parents(".menue_bodychild").css("display", "block");
        $("[id*=" + id + "]").parents(".menue_bodychild").css("display", "block");
        $("[id*=" + id + "]").parents(".menue_bodychildchild").css("display", "block");
        $(".menue_item").removeClass("munue_ItemSelcted");
        $("#" + menueId).addClass("munue_ItemSelcted");

        switch (id) {
            case "CapitalFlow": $("#homeModel").text("--资金结算中心")
                break;
            case "AccountReport": $("#homeModel").text("--资金结算中心")
                break;
            case "CapitalFloM": $("#homeModel").text("--资金结算中心")
                break;
            case "CapitalFloL": $("#homeModel").text("--资金结算中心")
                break;
            case "CapitalFloC": $("#homeModel").text("--资金结算中心")
                break;
            case "PaymentOrder": $("#homeModel").text("--资金结算中心") //*************
                break;
            case "VoucherManage": $("#homeModel").text("--会计核算中心")
                break;
            case "VoucherManageBank": $("#homeModel").text("--会计核算中心")
                break;
            case "VoucherCheck": $("#homeModel").text("--会计核算中心")
                break;
            case "QuerySubject": $("#homeModel").text("--会计核算中心")//*************
                break;
            case "SettlementStandards": $("#homeModel").text("--会计核算中心")//*************
                break; 
            case "ROrderListManage": $("#homeModel").text("--费用报销中心")
                break;
            case "ReportCenter": $("#homeModel").text("--报表管理中心")
                break;
            case "RevenueBudget": $("#homeModel").text("--预算管理中心")
                break;
            case "BaseData": $("#homeModel").text("--驾驶员结算中心")
                break;
            case "Reconc": $("#homeModel").text("--驾驶员结算中心")
                break;
            case "AmountReport": $("#homeModel").text("--驾驶员结算中心")//*************
                break;
            case "AssetManagement": $("#homeModel").text("--资产管理中心")
                break;
            case "TaxCalculation": $("#homeModel").text("--税务管理中心")
                break;
            case "LiXinAudit": $("#homeModel").text("--审计管理中心")
                break;
            case "IndustryStatistics": $("#homeModel").text("--统计管理中心")
                break; 
            case "FinancialStatements": $("#homeModel").text("--财务管理中心")
                break;
            case "SystemManage": $("#homeModel").text("--系统管理")//*************
                break;
            case "OracleButt": $("#homeModel").text("--系统管理")
                break;
            case "OrderSetting": $("#homeModel").text("--系统管理")
                break;
            case "PaySetting": $("#homeModel").text("--系统管理")
                break;
            default:
        }
    }
}



$(function () {

    menueHelp.drawMenueStyle();
});

