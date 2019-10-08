//资产维护
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxStartPeriodTable") },
    $grid1: function () { return $("#jqxStartPeriodTable1") },
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
        initStartPeriodTable();
        initEndPeriodTable();
        initAddedTable();
        initReduceTable();
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
            initStartPeriodTable();
            initEndPeriodTable();
        });
        //关闭
        $("#AssetReviewDialog_CancelBtn").on("click",
            function () {
                $("#AssetReviewDialog").modal("hide");
            }
        );
    }; //addEvent end
    function initStartPeriodTable() {
        layer.load();
        getPeriodData(function (data) {
            $("#PeriodType").val("StartPeriod");
            var source =
            {
                localdata: data,
                datafields:
                [
                    { name: 'BelongToCompany', type: 'string' },
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
                    totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: true } },
                    rows: [{ dataField: 'ManageCompany', text: '管理公司', align: 'center', width: 150 }],
                    columns: [{ dataField: 'VehicleModel', width: 110, align: 'center' }],
                    values: [
                        {
                            dataField: 'Quantity',
                            'function': 'sum',
                            showHeader: false,
                            text: $("#PeriodType").val() == "StartPeriod"?"期初":"期末",
                            width: 80,
                            align: 'center',
                            formatSettings: { align: 'center', prefix: '', decimalPlaces: 0 },
                            sortable: false
                        }
                    ]
                });
            var pivotDataSource1 = new $.jqx.pivot(
                dataAdapter,
                {
                    pivotValuesOnRows: false,
                    totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: true } },
                    rows: [{ dataField: 'BelongToCompany', text: '所属公司', align: 'center', width: 150 }],
                    columns: [{ dataField: 'VehicleModel', width: 110, align: 'center' }],
                    values: [
                        {
                            dataField: 'Quantity',
                            'function': 'sum',
                            showHeader: false,
                            text: $("#PeriodType").val() == "StartPeriod" ? "期初" : "期末",
                            width: 80,
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
                    autoResize: true,
                    selectionEnabled: true
                });
            selector.$grid1().jqxPivotGrid(
                {
                    source: pivotDataSource1,
                    treeStyleRows: true,
                    autoResize: true,
                    selectionEnabled: true
                });
            layer.closeAll('loading');
        });
    }
    function initEndPeriodTable() {
        layer.load();
        getPeriodData(function (data) {
            $("#PeriodType").val("EndPeriod");
            var source =
            {
                localdata: data,
                datafields:
                [
                    { name: 'BelongToCompany', type: 'string' },
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
                    totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: true } },
                    rows: [{ dataField: 'ManageCompany', text: '管理公司', align: 'center', width: 150 }],
                    columns: [{ dataField: 'VehicleModel', width: 110, align: 'center' }],
                    values: [
                        {
                            dataField: 'Quantity',
                            'function': 'sum',
                            showHeader: false,
                            text: $("#PeriodType").val() == "StartPeriod" ? "期初" : "期末",
                            width: 80,
                            align: 'center',
                            formatSettings: { align: 'center', prefix: '', decimalPlaces: 0 },
                            sortable: false
                        }
                    ]
                });
            var pivotDataSource1 = new $.jqx.pivot(
                dataAdapter,
                {
                    pivotValuesOnRows: false,
                    totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: true } },
                    rows: [{ dataField: 'BelongToCompany', text: '所属公司', align: 'center', width: 150 }],
                    columns: [{ dataField: 'VehicleModel', width: 110, align: 'center' }],
                    values: [
                        {
                            dataField: 'Quantity',
                            'function': 'sum',
                            showHeader: false,
                            text: $("#PeriodType").val() == "StartPeriod" ? "期初" : "期末",
                            width: 80,
                            align: 'center',
                            formatSettings: { align: 'center', prefix: '', decimalPlaces: 0 },
                            sortable: false
                        }
                    ]
                });
            $("#jqxEndPeriodTable").jqxPivotGrid(
                {
                    source: pivotDataSource,
                    treeStyleRows: true,
                    autoResize: true,
                    selectionEnabled: true
                });
            $("#jqxEndPeriodTable1").jqxPivotGrid(
                {
                    source: pivotDataSource1,
                    treeStyleRows: true,
                    autoResize: true,
                    selectionEnabled: true
                });
            layer.closeAll('loading');
        });
    }
    function initAddedTable() {
        layer.load();
        $("#ShowType").val("Added");
        getChangedData(function (data) {
            var source =
            {
                localdata: data,
                datafields:
                [
                    { name: 'BelongToCompany', type: 'string' },
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
                    totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: true } },
                    rows: [{ dataField: 'ManageCompany', text: '管理公司', align: 'center', width: 150 }],
                    columns: [{ dataField: 'VehicleModel', width: 110, align: 'center' }],
                    values: [
                        {
                            dataField: 'Quantity',
                            'function': 'sum',
                            showHeader: false,
                            text: $("#ShowType").val() == "Added" ? "增加" : "减少",
                            width: 80,
                            align: 'center',
                            formatSettings: { align: 'center', prefix: '', decimalPlaces: 0 },
                            sortable: false
                        }
                    ]
                });
            var pivotDataSource1 = new $.jqx.pivot(
                dataAdapter,
                {
                    pivotValuesOnRows: false,
                    totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: true } },
                    rows: [{ dataField: 'BelongToCompany', text: '所属公司', align: 'center', width: 150 }],
                    columns: [{ dataField: 'VehicleModel', width: 110, align: 'center' }],
                    values: [
                        {
                            dataField: 'Quantity',
                            'function': 'sum',
                            showHeader: false,
                            text: $("#ShowType").val() == "Added" ? "增加" : "减少",
                            width: 80,
                            align: 'center',
                            formatSettings: { align: 'center', prefix: '', decimalPlaces: 0 },
                            sortable: false
                        }
                    ]
                });
            $("#jqxAddedTable").jqxPivotGrid(
                {
                    source: pivotDataSource,
                    treeStyleRows: true,
                    autoResize: true,
                    selectionEnabled: true
                });
            $("#jqxAddedTable1").jqxPivotGrid(
                {
                    source: pivotDataSource1,
                    treeStyleRows: true,
                    autoResize: true,
                    selectionEnabled: true
                });
            layer.closeAll('loading');
        });
    }
    function initReduceTable() {
        layer.load();
        $("#ShowType").val("Reduce");
        getChangedData(function (data) {
            var source =
            {
                localdata: data,
                datafields:
                [
                    { name: 'BelongToCompany', type: 'string' },
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
                    totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: true } },
                    rows: [{ dataField: 'ManageCompany', text: '管理公司', align: 'center', width: 150 }],
                    columns: [{ dataField: 'VehicleModel', width: 110, align: 'center' }],
                    values: [
                        {
                            dataField: 'Quantity',
                            'function': 'sum',
                            showHeader: false,
                            text: $("#ShowType").val() == "Added" ? "增加" : "减少",
                            width: 80,
                            align: 'center',
                            formatSettings: { align: 'center', prefix: '', decimalPlaces: 0 },
                            sortable: false
                        }
                    ]
                });
            var pivotDataSource1 = new $.jqx.pivot(
                dataAdapter,
                {
                    pivotValuesOnRows: false,
                    totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: true } },
                    rows: [{ dataField: 'BelongToCompany', text: '所属公司', align: 'center', width: 150 }],
                    columns: [{ dataField: 'VehicleModel', width: 110, align: 'center' }],
                    values: [
                        {
                            dataField: 'Quantity',
                            'function': 'sum',
                            showHeader: false,
                            text: $("#ShowType").val() == "Added" ? "增加" : "减少",
                            width: 80,
                            align: 'center',
                            formatSettings: { align: 'center', prefix: '', decimalPlaces: 0 },
                            sortable: false
                        }
                    ]
                });
            $("#jqxReduceTable").jqxPivotGrid(
                {
                    source: pivotDataSource,
                    treeStyleRows: true,
                    autoResize: true,
                    selectionEnabled: true
                });
            $("#jqxReduceTable1").jqxPivotGrid(
                {
                    source: pivotDataSource1,
                    treeStyleRows: true,
                    autoResize: true,
                    selectionEnabled: true
                });
            layer.closeAll('loading');
        });
    }
    var tableValue = "";
    function getPeriodData(callback) {
        debugger;
        $.ajax({
            url: "/AssetManagement/AssetReport/GetAssetReportPeriodListDatas",
            data: { "YearMonth": $("#YearMonth").val(), "PeriodType": $("#PeriodType").val() },
            datatype: "json",
            type: "post",
            success: function (result) {
                debugger;
                tableValue = result;
                callback(result);
            }
        });
    }
    function getChangedData(callback) {
        debugger;
        $.ajax({
            url: "/AssetManagement/AssetReport/GetAssetReportChangedListDatas",
            data: { "YearMonth": $("#YearMonth").val(), "ShowType": $("#ShowType").val() },
            datatype: "json",
            type: "post",
            success: function (result) {
                debugger;
                tableValue = result;
                callback(result);
            }
        });
    }
};
$(function () {
    var page = new $page();
    page.init();
    debugger;
});