//资产维护
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxManageCompanyPeriodTable") },
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
        var currentDate = (year.toString());
        $("#YearMonth").val(currentDate);
        addEvent();
        var winHeight = document.body.scrollWidth;
        $("#assetReport").css("width", winHeight - 240);
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
                    rows: ["CompanyType","CompanyName", "VehicleModel"],
                    cols: ["YearMonth"], aggregator: sumOverSum(["Quantity"]
                    )
                });
                $(".pvtAxisLabel").eq(0).text("月份");
                $(".pvtAxisLabel").eq(0).css("text-align", "center");
                $(".pvtAxisLabel").eq(1).text("公司类别");
                $(".pvtAxisLabel").eq(1).css("text-align", "center");
                $(".pvtAxisLabel").eq(2).text("公司名称");
                $(".pvtAxisLabel").eq(2).css("text-align", "center");
                $(".pvtAxisLabel").eq(3).text("车型");
                $(".pvtAxisLabel").eq(3).css("text-align", "center");
                $(".pvtRowTotalLabel").css("text-align", "center");
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
            url: "/AnalysisManagementCenter/VehicleModelAnalysis/GetManageCompanyVehicleModelAnalysis",
            data: { "Year": $("#YearMonth").val() },
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