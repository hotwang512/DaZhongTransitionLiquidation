﻿//所有元素选择器
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
        debugger
        var myDate = new Date();
        // var myMonth = $.convert.toDate(addMonth(myDate, -1), "yyyy-MM");
        var month2 = (myDate.getMonth() + 1) < 10 ? "0" + (myDate.getMonth() + 1) : (myDate.getMonth() + 1);
        $("#YearMonth").val(myDate.getFullYear() + "-" + month2);
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
        //同步车辆数据
        $("#btnSync").on("click", function () {
            layer.load();
            $.ajax({
                url: "/VoucherManageManagement/VehicleBusiness/GetVehicleBusinessInfo",
                data: { YearMonth: $("#YearMonth").val() },
                type: "post",
                dataType: "json",
                success: function (msg) {
                    if (msg.IsSuccess == true) {
                        layer.closeAll('loading');
                        jqxNotification("同步成功！", null, "success");
                        var month2 = $("#YearMonth").val();
                        initTable(month2);
                    } else {
                        layer.closeAll('loading');
                        jqxNotification("同步失败！", null, "error");
                    }
                }
            });
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
                if (mps.length > 0) {
                    var utils = $.pivotUtilities;
                    var heatmap = utils.renderers["Heatmap"];
                    var sumOverSum = utils.aggregators["Sum"];
                    var model = [];
                    var classType = [];
                    var carType = [];
                    for (var i = 0; i < mps.length; i++) {
                        if (model.indexOf(mps[i].MODEL_MAJOR) < 0) {
                            model.push(mps[i].MODEL_MAJOR);
                        }
                        if (classType.indexOf(mps[i].MODEL_MINOR) < 0) {
                            classType.push(mps[i].MODEL_MINOR);
                        }
                        if (carType.indexOf(mps[i].CarType) < 0) {
                            carType.push(mps[i].CarType);
                        }
                    }
                    $("#jqxTable").pivot(mps, {
                        rows: [ "BELONGTO_COMPANY"],
                        cols: ["MODEL_MAJOR", "MODEL_MINOR", "CarType"],
                        //aggregatorName: "Sum",
                        aggregator: sumOverSum(["MODEL_DAYS"]),
                        //vals: ["Money"],
                        sorters: {
                            MODEL_MAJOR: $.pivotUtilities.sortAs(model),
                            MODEL_MINOR: $.pivotUtilities.sortAs(classType),
                            CarType: $.pivotUtilities.sortAs(carType),
                        },
                    });
                    $("#jqxTable2").pivot(mps, {
                        rows: ["MANAGEMENT_COMPANY"],
                        cols: ["MODEL_MAJOR", "MODEL_MINOR", "CarType"],
                        //aggregatorName: "Sum",
                        aggregator: sumOverSum(["MODEL_DAYS"]),
                        //vals: ["Money"],
                        sorters: {
                            MODEL_MAJOR: $.pivotUtilities.sortAs(model),
                            MODEL_MINOR: $.pivotUtilities.sortAs(classType),
                            CarType: $.pivotUtilities.sortAs(carType),
                        },
                    });
                    $("#jqxTable .pvtAxisLabel").eq(0).text("模式");
                    $("#jqxTable .pvtAxisLabel").eq(0).css("text-align", "center")
                    $("#jqxTable .pvtAxisLabel").eq(1).text("班型");
                    $("#jqxTable .pvtAxisLabel").eq(1).css("text-align", "center")
                    $("#jqxTable .pvtAxisLabel").eq(2).text("车型");
                    $("#jqxTable .pvtAxisLabel").eq(2).css("text-align", "center")
                    $("#jqxTable .pvtAxisLabel").eq(3).text("所属公司");
                    $("#jqxTable .pvtAxisLabel").eq(3).css("text-align", "center")
                    $(".pvtRowTotalLabel").css("text-align", "center")
                    $("#jqxTable2 tr").eq(0).hide();
                    $("#jqxTable2 tr").eq(1).hide();
                    $("#jqxTable2 tr").eq(2).hide();
                    //$("#jqxTable2 .pvtAxisLabel").eq(0).text("模式");
                    //$("#jqxTable2 .pvtAxisLabel").eq(0).css("text-align", "center")
                    //$("#jqxTable2 .pvtAxisLabel").eq(1).text("班型");
                    //$("#jqxTable2 .pvtAxisLabel").eq(1).css("text-align", "center")
                    //$("#jqxTable2 .pvtAxisLabel").eq(2).text("车型");
                    //$("#jqxTable2 .pvtAxisLabel").eq(2).css("text-align", "center")
                    $("#jqxTable2 .pvtAxisLabel").eq(3).text("管理公司");
                    $("#jqxTable2 .pvtAxisLabel").eq(3).css("text-align", "center")
                    //$(".pvtAxisLabel").eq(4).text("所属公司");
                    //$(".pvtAxisLabel").eq(4).css("text-align", "center")
                    $("#jqxTable").show();
                    $("#jqxTable2").show();
                } else {
                    jqxNotification("当前月份没有数据！", null, "error");
                }
            }
        });
    }
};

$(function () {
    var page = new $page();
    page.init();
});
function addMonth(date, num) {
    num = parseInt(num);
    var sDate = dateToDate(date);

    var sYear = sDate.getFullYear();
    var sMonth = sDate.getMonth() + 1;
    var sDay = sDate.getDate();

    var eYear = sYear;
    var eMonth = sMonth + num;
    var eDay = sDay;
    while (eMonth > 12) {
        eYear++;
        eMonth -= 12;
    }

    var eDate = new Date(eYear, eMonth - 1, eDay);

    while (eDate.getMonth() != eMonth - 1) {
        eDay--;
        eDate = new Date(eYear, eMonth - 1, eDay);
    }

    return eDate;
};