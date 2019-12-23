var selector = {
    //表格
    $grid: function () { return $("#datatable") },
    $txtMonth: function () { return $("#txtMonth") },
    $txtChannel: function () { return $("#txtChannel") },
    //按钮
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
};

var $page = function () {

    this.init = function () {
        //initControl();
        addEvent();
    }

    //所有事件
    function addEvent() {
        initTable();
        //查询
        selector.$btnSearch().on("click", function () {
            //initTable();
        });

        //重置
        selector.$btnReset().on("click", function () {
            $("#YearMonth").val("");
        });

        function initTable() {
            //layer.load();
            $.ajax({
                url: "/VoucherManageManagement/SettlementImportCount/GetSettlementData",
                data: {},
                async: false,
                type: "post",
                success: function (mps) {
                    //layer.closeAll('loading');
                    if (mps != null) {
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
                        $("#SettlementImportTable").pivot(mps, {
                            rows: ["Business", "BusinessType"],
                            cols: ["Model", "ClassType", "CarType"],
                            //aggregatorName: "Sum",
                            aggregator: sumOverSum(["Money"]),
                            //vals: ["Money"],
                            sorters: {
                                Business: $.pivotUtilities.sortAs(business),
                                BusinessType: $.pivotUtilities.sortAs(businessType),
                                Model: $.pivotUtilities.sortAs(model),
                                ClassType: $.pivotUtilities.sortAs(classType),
                                CarType: $.pivotUtilities.sortAs(carType),
                            },
                            rendererOptions: {
                                table: {
                                    clickCallback: function (e, value, filters, pivotData) {
                                        $("#txtModel").val(filters.Model),
                                        $("#txtClassType").val(filters.ClassType),
                                        $("#txtCarType").val(filters.CarType),
                                        $("#txtBusiness").val(filters.Business),
                                        $("#txtBusinessType").val(filters.BusinessType),
                                        $("#txtMoney").val(value),
                                        selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
                                        selector.$AddBankChannelDialog().modal("show");
                                    }
                                }
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
                        $("#SettlementImportTable").show();
                    }
                }
            });
        }
    };
};

$(function () {
    var page = new $page();
    page.init();

});
