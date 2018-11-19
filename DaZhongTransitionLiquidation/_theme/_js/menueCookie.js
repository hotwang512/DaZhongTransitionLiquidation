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
        var id = menueId.split("_")[1];
        $(".menue_header").css("display", "none");
        $("." + id).show();
        $(".menue_body").css("display", "none");
        $("#" + menueId).parents(".menue_body").css("display", "block");
        $("#" + id).show();
        $(".menue_bodychild").css("display", "none");
        //$("#" + menueId).parents(".menue_bodychild").css("display", "block");
        $("[id*=" + id + "]").parents(".menue_bodychild").css("display", "block");
        $(".menue_item").removeClass("munue_ItemSelcted");
        $("#" + menueId).addClass("munue_ItemSelcted");

        switch (id) {
            case "CapitalFlow": $("#gotoHome").text("大众出租财务共享服务平台--资金结算中心")
                break;
            case "PaymentOrder": $("#gotoHome").text("大众出租财务共享服务平台--资金结算中心") //*************
                break;
            case "VoucherManage": $("#gotoHome").text("大众出租财务共享服务平台--会计核算中心")
                break;
            case "AssetManagement": $("#gotoHome").text("大众出租财务共享服务平台--会计核算中心")
                break;
            case "OracleButt": $("#gotoHome").text("大众出租财务共享服务平台--会计核算中心")
                break;
            case "FinancialStatements": $("#gotoHome").text("大众出租财务共享服务平台--会计核算中心")
                break;
            case "QuerySubject": $("#gotoHome").text("大众出租财务共享服务平台--会计核算中心")//*************
                break;
            case "Reimbursement": $("#gotoHome").text("大众出租财务共享服务平台--费用报销中心")
                break;
            case "ReportCenter": $("#gotoHome").text("大众出租财务共享服务平台--报表管理中心")
                break;
            case "Analysis": $("#gotoHome").text("大众出租财务共享服务平台--预算、分析、考核")
                break;
            case "BaseData": $("#gotoHome").text("大众出租财务共享服务平台--驾驶员结算中心")
                break;
            case "AmountReport": $("#gotoHome").text("大众出租财务共享服务平台--驾驶员结算中心")//*************
                break;
            case "BigCenter": $("#gotoHome").text("大众出租财务共享服务平台--大数据管理中心")
                break;
            case "TaxAuditManage": $("#gotoHome").text("大众出租财务共享服务平台--税务、审计、统计")
                break;
            case "SystemManage": $("#gotoHome").text("大众出租财务共享服务平台--系统管理")//*************
                break;
            default:
        }
    }
}



$(function () {

    menueHelp.drawMenueStyle();
});

