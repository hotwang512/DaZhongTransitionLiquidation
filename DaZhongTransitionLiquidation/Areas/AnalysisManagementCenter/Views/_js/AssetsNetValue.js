var vguid = "";
var mydate = new Date();
var ManagementCompany = [];
var dataArr = [];
var $page = function () {
    this.init = function () {
        var date = new Date();
        $("#DateOfYear").val(date.getFullYear());
        GetAssetsNetValueDetail();
        addEvent();
    }
    //所有事件
    function addEvent() {
        $("#btnSearch").on("click",
            function () {
                GetAssetsNetValueDetail();
            });
    }; //addEvent end
};

function parseToInt(str) {
    if (str != null) {
        return parseInt(str);
    } else {
        return 0;
    }
}
function GetAssetsNetValueDetail() {
    layer.load();
    getPeriodData(function (data) {
        var mps = data.Rows;
        if (mps.length > 0) {
            debugger;
            var utils = $.pivotUtilities;
            var sumOverSum = utils.aggregators["Sum"];
            $("#table").pivot(mps, {
                rows: ["YearMonth", "MAJOR", "MINOR", "VMODEL", "ASSETCOUNT", "COST", "ACCT", "DEVALUE", "NETALUE"],
                cols: [],
                aggregator: sumOverSum(["COST"])
            });
            $(".pvtAxisLabel").eq(0).text("月份");
            $(".pvtAxisLabel").eq(0).css("text-align", "center");
            $(".pvtAxisLabel").eq(1).text("主分类段");
            $(".pvtAxisLabel").eq(1).css("text-align", "center");
            $(".pvtAxisLabel").eq(2).text("次分类段");
            $(".pvtAxisLabel").eq(2).css("text-align", "center");
            $(".pvtAxisLabel").eq(3).text("车型");
            $(".pvtAxisLabel").eq(3).css("text-align", "center");
            $(".pvtAxisLabel").eq(4).text("数量");
            $(".pvtAxisLabel").eq(4).css("text-align", "center");
            $(".pvtAxisLabel").eq(5).text("原值");
            $(".pvtAxisLabel").eq(5).css("text-align", "center");
            $(".pvtAxisLabel").eq(6).text("累计折旧");
            $(".pvtAxisLabel").eq(6).css("text-align", "center");
            $(".pvtAxisLabel").eq(7).text("减值准备");
            $(".pvtAxisLabel").eq(7).css("text-align", "center");
            $(".pvtAxisLabel").eq(8).text("账面净值");
            $(".pvtAxisLabel").eq(8).css("text-align", "center");
            $(".pvtRowTotalLabel").css("text-align", "center");
            $(".pvtTotal").hide();
            $(".pvtTotalLabel ").hide();
            $(".pvtGrandTotal ").hide();
        } else {
            jqxNotification("当前年份没有数据！", null, "error");
        }
        layer.closeAll('loading');
        $("#assetReport").show();
    });
}

var tableValue = "";
function getPeriodData(callback) {
    $.ajax({
        url: "/AnalysisManagementCenter/AssetsNetValue/GetAssetsNetValueDetail",
        data: { "DateOfYear": $("#DateOfYear").val() },
        datatype: "json",
        type: "post",
        success: function (result) {
            tableValue = result;
            callback(result);
        }
    });
}
function pickedFunc() {
    GetFilialeStatisticsDetail();
}

function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}
$(function () {
    var page = new $page();
    page.init();
});
function formatDate(NewDtime) {
    var d = NewDtime;
    var datetime = d.getFullYear() + '-' + (d.getMonth() + 1);//  + '-' + d.getDate() + ' ' + d.getHours() + ':' + d.getMinutes() + ':' + d.getSeconds()
    return datetime;
}