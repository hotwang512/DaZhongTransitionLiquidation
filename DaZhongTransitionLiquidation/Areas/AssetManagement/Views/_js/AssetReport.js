﻿//资产维护
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var $page = function () {
    this.init = function () {
        var date = new Date;
        var year = date.getFullYear();
        var month = date.getMonth() + 1;
        month = (month < 10 ? "0" + month : month);
        var currentDate = (year.toString() + "-" + month.toString());
        $("#YearMonth").val(currentDate);
        addEvent();
    }
    //所有事件
    function addEvent() {
        //加载列表数据
        initTable();
        //提交
        $("#btnSubmit").on("click", function () {
            $.ajax({
                url: "/AssetManagement/AssetReport/SubmitAssetReport",
                data: { YearMonth: $("#YearMonth").val() },
                //traditional: true,
                type: "post",
                success: function (msg) {
                    layer.closeAll('loading');
                    switch (msg.Status) {
                    case "0":
                        jqxNotification("提交失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("提交成功！", null, "success");
                        break;
                    case "2":
                        jqxNotification("已提交不允许重复提交！", null, "success");
                        break;
                    }
                }
            });
        });
        $("#btnGetData").on("click", function () {
            initTable();
        });
        //关闭
        $("#AssetReviewDialog_CancelBtn").on("click",
            function () {
                $("#AssetReviewDialog").modal("hide");
            }
        );
    }; //addEvent end
    function initTable() {
        layer.load();
        getData(function (data) {
            var source =
                {
                    localdata: data,
                    datafields:
                    [
                        { name: 'ManageCompany', type: 'string' },
                        { name: 'VehicleModel', type: 'string' },
                        { name: 'Quantity', type: 'number' }
                    ],
                    datatype: "json"
                };
            var dataAdapter = new $.jqx.dataAdapter(source);
            dataAdapter.dataBind();
            var pivotDataSource = new $.jqx.pivot(
               dataAdapter,
               {
                   pivotValuesOnRows: false,
                   totals: { rows: { subtotals: false, grandtotals: true}, columns: { subtotals: false, grandtotals: true } },
                   rows: [{ dataField: 'ManageCompany', text: '管理公司', align: 'center' }],
                   columns: [{ dataField: 'VehicleModel', width: 500, align: 'center' }],
                   values: [
                       {
                           dataField: 'Quantity',
                           'function': 'sum',
                           showHeader:false,
                           text: '数量',
                           width: 160,
                           align: 'center',
                           formatSettings: { align: 'center', prefix: '', decimalPlaces: 0 },
                           sortable: false
                       }
                   ]
               });
            selector.$grid().jqxPivotGrid(
               {
                   source: pivotDataSource,
                   treeStyleRows: true, 
                   //autoResize: true,
                   selectionEnabled: true
               });
            layer.closeAll('loading');
        });
    }
    var tableValue = "";
    function getData(callback) {
        debugger;
        $.ajax({
            url: "/AssetManagement/AssetReport/GetAssetReportListDatas",
            data: { "YearMonth": $("#YearMonth").val() },
            datatype: "json",
            type: "post",
            success: function (result) {
                debugger;
                tableValue = result;
                callback(result);
                $(".jqx-pivotgrid-item")[16].style['text-align'] = "center";
                $(".jqx-pivotgrid-item")[5].style['text-align'] = "center";
            }
        });
    }
};
$(function () {
    var page = new $page();
    page.init();
    debugger;
});