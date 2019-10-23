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
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#TradingBank").val("");
            $("#TransactionDate").val("");
            $("#TransactionDateEnd").val("");
            $("#PaymentUnit").val("");
        });
    }; //addEvent end

    function initTable() {
        //layer.load();
        $.ajax({
            url: "/VoucherManageManagement/VehicleUnit/GeVehicleData",
            data: { },
            async: false,
            type: "post",
            success: function (mps) {
                //layer.closeAll('loading');
                if (mps.length > 0) {
                    var utils = $.pivotUtilities;
                    var heatmap = utils.renderers["Heatmap"];
                    var sumOverSum = utils.aggregators["Sum"];
                    $("#jqxTable").pivot(mps, {
                        rows: ["MANAGEMENT_COMPANY", "BELONGTO_COMPANY"],
                        cols: ["MODEL_MAJOR", "MODEL_MINOR", "CarType"],
                        //aggregatorName: "Sum",
                        aggregator: sumOverSum(["DAYS"]),
                        //vals: ["Money"],
                        rendererOptions: {
                            table: {
                            }
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
