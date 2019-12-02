var selector = {
    //表格
    $grid: function () { return $("#datatable") },
    $txtMonth: function () { return $("#txtMonth") },
    $txtChannel: function () { return $("#txtChannel") },
    //按钮
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },

    $btnExport: function () { return $("#btnExport") }


};

var $page = function () {

    this.init = function () {
        //initControl();
        addEvent();
    }

    function initControl() {
        selector.$txtMonth().datepicker({
            format: 'yyyy-mm',
            language: 'cn',
            autoclose: true,
            todayBtn: true,
            startView: 2,
            todayHighlight: 1,
            maxViewMode: 2,
            minViewMode: 1,
            forceParse: false
        });

    }

    //所有事件
    function addEvent() {

        initTable();
        //查询
        selector.$btnSearch().on("click", function () {
            var year = $("#Year").val();
            if (year == "") {
                jqxNotification("请选择条件！", null, "error");
                return;
            }
            initTable();
        });


        //重置
        selector.$btnReset().on("click", function () {
            
        });
        //导出
        selector.$btnExport().on("click", function () {
            
        })

        function initTable() {
            $.ajax({
                url: "/VoucherManageManagement/SettlementCount/GetSettlementCountData",
                data: { "year": $("#Year").val(), "company": $("#Company option:selected").text() },
                datatype: "json",
                type: "post",
                success: function (mps) {
                    if (mps.length > 0) {
                        var utils = $.pivotUtilities;
                        var heatmap = utils.renderers["Heatmap"];
                        var sumOverSum = utils.aggregators["Sum"];
                        $("#datatable").pivot(mps, {
                            rows: ["Business", "BusinessType"],
                            cols: ["YearMonth"],
                            //aggregatorName: "Sum",
                            aggregator: sumOverSum(["Account"]),
                            //vals: ["Money"],
                        });

                        $(".pvtAxisLabel").eq(0).text("月份");
                        $(".pvtAxisLabel").eq(0).css("text-align", "center")
                        $(".pvtAxisLabel").eq(1).text("营收主类");
                        $(".pvtAxisLabel").eq(1).css("text-align", "center")
                        $(".pvtAxisLabel").eq(2).text("营收子类");
                        $(".pvtAxisLabel").eq(2).css("text-align", "center")
                        $(".pvtRowTotalLabel").css("text-align", "center")
                        $("#datatable").show();

                    } else {
                        jqxNotification("当前年份没有数据！", null, "error");
                    }
                }
            });
        }

    }; //addEvent end

};

$(function () {
    var page = new $page();
    page.init();

});