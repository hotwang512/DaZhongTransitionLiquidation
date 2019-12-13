//资产维护
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxManageCompanyPeriodTable") },
    $grid1: function () { return $("#jqxBelongToCompanyPeriodTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
function pickedFunc() {
}
var $page = function () {
    this.init = function () {;
        var date = new Date;
        var year = date.getFullYear();
        var month = date.getMonth() + 1;
        month = (month < 10 ? "0" + month : month);
        var currentDate = (year.toString() + "-" + month.toString());
        $("#YearMonth").val(currentDate);
        addEvent();
        var winHeight = document.body.scrollWidth;
        $("#assetReport").css("width", winHeight - 260);
        $('#tabs').jqxTabs({ width:1500, height: "100%", position: 'top' });
    }
    //所有事件
    function addEvent() {
        //加载列表数据
        initPeriodTable();
        $("#btnGetData").on("click", function () {
            initPeriodTable();
        });
        //关闭
        $("#AssetReviewDialog_CancelBtn").on("click",
            function () {
                $("#AssetReviewDialog").modal("hide");
            }
        );
    }; //addEvent end
    function initPeriodTable() {
        layer.load();
        debugger;
        getPeriodData(function (mps) {
            if (mps.length > 0) {
                debugger;
                var utils = $.pivotUtilities;
                var sumOverSum = utils.aggregators["Sum"];
                selector.$grid().pivot(mps, {
                    rows: ["CompanyName", "VehicleModel"],
                    cols: ["Period"],
                    aggregator: sumOverSum(["Quantity"])
                });
                $(".pvtAxisLabel").eq(0).text("月份");
                $(".pvtAxisLabel").eq(0).css("text-align", "center");
                $(".pvtAxisLabel").eq(1).text("管理公司");
                $(".pvtAxisLabel").eq(1).css("text-align", "center");
                $(".pvtAxisLabel").eq(2).text("车型");
                $(".pvtAxisLabel").eq(2).css("text-align", "center");
                $(".pvtRowTotalLabel").css("text-align", "center");
            } else {
                jqxNotification("当前年份没有数据！", null, "error");
            }
            layer.closeAll('loading');
            $("#assetReport").show();
        });
        getUsePeriodData(function (mps) {
            if (mps.length > 0) {
                debugger;
                var utils = $.pivotUtilities;
                var sumOverSum = utils.aggregators["Sum"];
                selector.$grid1().pivot(mps, {
                    rows: ["CompanyName", "VehicleModel"],
                    cols: ["Period"],
                    aggregator: sumOverSum(["Quantity"])
                });
                $("#jqxBelongToCompanyPeriodTable .pvtAxisLabel").eq(0).text("月份");
                $("#jqxBelongToCompanyPeriodTable .pvtAxisLabel").eq(0).css("text-align", "center");
                $("#jqxBelongToCompanyPeriodTable .pvtAxisLabel").eq(1).text("管理公司");
                $("#jqxBelongToCompanyPeriodTable .pvtAxisLabel").eq(1).css("text-align", "center");
                $("#jqxBelongToCompanyPeriodTable .pvtAxisLabel").eq(2).text("车型");
                $("#jqxBelongToCompanyPeriodTable .pvtAxisLabel").eq(2).css("text-align", "center");
                $("#jqxBelongToCompanyPeriodTable .pvtRowTotalLabel").css("text-align", "center");
            }
        });
    }
    var tableValue = "";
    function getPeriodData(callback) {
        $.ajax({
            url: "/AnalysisManagementCenter/OutageReport/GetManageCompanyOutageReport",
            data: { "YearMonth": $("#YearMonth").val() },
            datatype: "json",
            type: "post",
            success: function (result) {
                tableValue = result;
                callback(result);
            }
        });
    }
    function getUsePeriodData(callback) {
        $.ajax({
            url: "/AnalysisManagementCenter/OutageReport/GetUseCompanyOutageReport",
            data: { "YearMonth": $("#YearMonth").val() },
            datatype: "json",
            type: "post",
            success: function (result) {
                tableValue = result;
                callback(result);
            }
        });
    }
};
$(function () {
    var page = new $page();
    page.init();
});