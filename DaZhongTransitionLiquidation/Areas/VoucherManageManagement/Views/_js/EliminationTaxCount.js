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
        //加载列表数据
        var myDate = new Date();
        var month2 = (myDate.getMonth() + 1) < 10 ? "0" + (myDate.getMonth() + 1) : (myDate.getMonth() + 1);
        $("#YearMonth").val(myDate.getFullYear() + "-" + month2);
        var month = $("#YearMonth").val();
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
            url: "/VoucherManageManagement/EliminationTaxCount/GeVehicleData",
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
                    var taxType = ["增值税税金", "城建税税金", "教育费附加", "地方教育费附加"];
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
                        rows: ["BELONGTO_COMPANY"],
                        cols: ["MODEL_MAJOR", "MODEL_MINOR", "CarType", "TaxType"],
                        //aggregatorName: "Sum",
                        aggregator: sumOverSum(["MODEL_DAYS"]),
                        //vals: ["Money"],
                        sorters: {
                            MODEL_MAJOR: $.pivotUtilities.sortAs(model),
                            MODEL_MINOR: $.pivotUtilities.sortAs(classType),
                            CarType: $.pivotUtilities.sortAs(carType),
                            TaxType: $.pivotUtilities.sortAs(taxType),
                        },
                    });

                    $("#jqxTable .pvtAxisLabel").eq(0).text("模式");
                    $("#jqxTable .pvtAxisLabel").eq(0).css("text-align", "center")
                    $("#jqxTable .pvtAxisLabel").eq(1).text("班型");
                    $("#jqxTable .pvtAxisLabel").eq(1).css("text-align", "center")
                    $("#jqxTable .pvtAxisLabel").eq(2).text("车型");
                    $("#jqxTable .pvtAxisLabel").eq(2).css("text-align", "center")
                    $("#jqxTable .pvtAxisLabel").eq(3).text("税种");
                    $("#jqxTable .pvtAxisLabel").eq(3).css("text-align", "center")
                    $("#jqxTable .pvtAxisLabel").eq(4).text("所属公司");
                    $("#jqxTable .pvtAxisLabel").eq(4).css("text-align", "center")
     
                    $("#jqxTable").show();
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