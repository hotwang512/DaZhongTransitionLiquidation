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
        //加载列表数据
        var myDate = new Date();
        var month = (myDate.getMonth() + 1) < 10 ? "0" + (myDate.getMonth() + 1) : (myDate.getMonth() + 1);
        var date = myDate.getFullYear() + "-" + month;
        $("#YearMonth").val(date);
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
            url: "/VoucherManageManagement/VehicleUnit/GeVehicleData",
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
                        rows: ["BELONGTO_COMPANY"],
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
