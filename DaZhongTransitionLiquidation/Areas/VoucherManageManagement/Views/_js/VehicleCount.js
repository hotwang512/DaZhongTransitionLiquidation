//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnExport: function () { return $("#btnExport") },
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
        var myMonth = $.convert.toDate(addMonth(myDate, - 1), "yyyy-MM");
        $("#YearMonth").val(myMonth);
        var month = $("#YearMonth").val();
        var type = "S";
        initTable(month, type);
        selector.$btnSearch().unbind("click").on("click", function () {
            $("#jqxTable").hide();
            month = $("#YearMonth").val();
            type = "S";
            initTable(month, type);
        });
        //计算
        $("#btnCount").on("click", function () {
            var month3 = $("#YearMonth").val();
            $.ajax({
                url: "/VoucherManageManagement/VehicleCount/SelectVehicleData",
                data: { YearMonth: month3 },
                async: false,
                type: "post",
                success: function (mps) {
                    if (mps.length > 0) {
                        WindowConfirmDialog(count, "当前月份已有数据,是否覆盖？", "确认框", "确定", "取消","");
                    } else {
                        count();
                    }
                }
            });
        });
        //html导出
        selector.$btnExport().on("click", function () {
            tableToExcel("pvtTable", "结算单位金额计算");//class名,excel名
        });
    }; //addEvent end 
    function count() {
        $("#jqxTable").hide();
        var month2 = $("#YearMonth").val();
        var type2 = "C";
        initTable(month2, type2);
    }
    function initTable(month, type) {
        layer.load();
        $.ajax({
            url: "/VoucherManageManagement/VehicleCount/GetVehicleData",
            data: { YearMonth: month, Type: type },
            async: true,
            type: "post",
            success: function (mps) {
                layer.closeAll('loading');
                if (mps.length > 0) {
                    var utils = $.pivotUtilities;
                    var heatmap = utils.renderers["Heatmap"];
                    var sumOverSum = utils.aggregators["Sum"];
                    var model = [];
                    var classType = [];
                    var carType = [];
                    var business = [];
                    var businessType = [];
                    for (var i = 0; i < mps.length; i++) {
                        if (model.indexOf(mps[i].Model) < 0) {
                            model.push(mps[i].Model);
                        }
                        if (classType.indexOf(mps[i].ClassType) < 0) {
                            classType.push(mps[i].ClassType);
                        }
                        if (carType.indexOf(mps[i].CarType) < 0) {
                            carType.push(mps[i].CarType);
                        }
                        if (business.indexOf(mps[i].Business) < 0) {
                            business.push(mps[i].Business);
                        }
                        if (businessType.indexOf(mps[i].BusinessType) < 0) {
                            businessType.push(mps[i].BusinessType);
                        }
                    }
                    $("#jqxTable").pivot(mps, {
                        rows: ["Business", "BusinessType"],
                        cols: ["Model", "ClassType", "CarType"],
                        //aggregatorName: "Sum",
                        aggregator: sumOverSum(["Account"]),
                        sorters: {
                            Business: $.pivotUtilities.sortAs(business),
                            BusinessType: $.pivotUtilities.sortAs(businessType),
                            Model: $.pivotUtilities.sortAs(model),
                            ClassType: $.pivotUtilities.sortAs(classType),
                            CarType: $.pivotUtilities.sortAs(carType),
                        },
                        //vals: ["Money"],
                        rendererOptions: {
                        }
                    });
                    $(".pvtAxisLabel").eq(0).text("模式");
                    $(".pvtAxisLabel").eq(0).css("text-align", "center")
                    $(".pvtAxisLabel").eq(1).text("班型");
                    $(".pvtAxisLabel").eq(1).css("text-align", "center")
                    $(".pvtAxisLabel").eq(2).text("车型");
                    $(".pvtAxisLabel").eq(2).css("text-align", "center")
                    $(".pvtAxisLabel").eq(3).text("营收主类");
                    $(".pvtAxisLabel").eq(3).css("text-align", "center")
                    $(".pvtAxisLabel").eq(4).text("营收子类");
                    $(".pvtAxisLabel").eq(4).css("text-align", "center")
                    $(".pvtRowTotalLabel").css("text-align", "center")
                    $("#jqxTable").show();
                } else {
                    if (type == "S") {
                        jqxNotification("当前月份没有数据！请先计算", null, "error");
                    }
                    else {
                        jqxNotification("当前月份没有可计算数据！", null, "error");
                    }
                }
            }
        });
    }
};

$(function () {
    var page = new $page();
    page.init();
});
