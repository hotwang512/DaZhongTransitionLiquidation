//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $txtDatedTime: function () { return $("#txtDatedTime") },
}; //selector end

var isEdit = false;
var vguid = "";
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        $("#btnSync").on("click", function () {
            layer.load();
            $.ajax({
                url: "/VoucherManageManagement/VehicleBusiness/GetVehicleBusinessInfo",
                data: {},
                type: "post",
                dataType: "json",
                success: function (msg) {
                    if (msg.IsSuccess == true) {
                        layer.closeAll('loading');
                        jqxNotification("同步成功！", null, "success");
                        initTable();
                    } else {
                        layer.closeAll('loading');
                        jqxNotification("同步失败！", null, "error");
                    }
                }
            });
        });
        var myDate = new Date();
        var month = (myDate.getMonth() + 1) < 10 ? "0" + (myDate.getMonth() + 1) : (myDate.getMonth() + 1);
        var date = myDate.getFullYear() + "-" + month;
        $("#YearMonth").val(date);
        var month = $("#YearMonth").val();
        //加载列表数据
        initTable(month);
        selector.$btnSearch().unbind("click").on("click", function () {
            month = $("#YearMonth").val();
            initTable(month);
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#TradingBank").val("");
            $("#TransactionDate").val("");
            $("#TransactionDateEnd").val("");
            $("#PaymentUnit").val("");
        });
    }; //addEvent end

    function initTable(month) {
        //layer.load();
        $.ajax({
            url: "/VoucherManageManagement/VehicleBusiness/GeVehicleData",
            data: { YearMonth: month },
            async: false,
            type: "post",
            success: function (mps) {
                //layer.closeAll('loading');
                if (mps != null) {
                    var utils = $.pivotUtilities;
                    var heatmap = utils.renderers["Heatmap"];
                    var sumOverSum = utils.aggregators["Sum"];
                    //var model = [];
                    //var classType = [];
                    //var carType = [];
                    //var business = [];
                    //var businessType = [];
                    //for (var i = 0; i < mps.length; i++) {
                    //    if (model.indexOf(mps[i].Model) < 0) {
                    //        model.push(mps[i].Model);
                    //    }
                    //    if (classType.indexOf(mps[i].ClassType) < 0) {
                    //        classType.push(mps[i].ClassType);
                    //    }
                    //    if (carType.indexOf(mps[i].CarType) < 0) {
                    //        carType.push(mps[i].CarType);
                    //    }
                    //    if (business.indexOf(mps[i].Business) < 0) {
                    //        business.push(mps[i].Business);
                    //    }
                    //    if (businessType.indexOf(mps[i].BusinessType) < 0) {
                    //        businessType.push(mps[i].BusinessType);
                    //    }
                    //}
                    $("#jqxTable").pivot(mps, {
                        rows: ["MANAGEMENT_COMPANY", "BELONGTO_COMPANY"],
                        cols: ["MODEL_MAJOR", "MODEL_MINOR", "CarType"],
                        //aggregatorName: "Sum",
                        aggregator: sumOverSum(["MODEL_DAYS"]),
                        //vals: ["Money"],
                        rendererOptions: {
                        },
                        renderers: {

                        }
                    });
                    $(".pvtAxisLabel").eq(0).text("模式");
                    $(".pvtAxisLabel").eq(0).css("text-align", "center")
                    $(".pvtAxisLabel").eq(1).text("班型");
                    $(".pvtAxisLabel").eq(1).css("text-align", "center")
                    $(".pvtAxisLabel").eq(2).text("车型");
                    $(".pvtAxisLabel").eq(2).css("text-align", "center")
                    $(".pvtAxisLabel").eq(3).text("资产主类");
                    $(".pvtAxisLabel").eq(3).css("text-align", "center")
                    $(".pvtAxisLabel").eq(4).text("资产子类");
                    $(".pvtAxisLabel").eq(4).css("text-align", "center")
                    $("#jqxTable").show();
                }
            }
        });
    }
};

$(function () {
    var page = new $page();
    page.init();
});
