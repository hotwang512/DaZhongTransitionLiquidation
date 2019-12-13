//资产维护
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxManageCompanyPeriodTable") },
    $grid1: function () { return $("#jqxBelongToCompanyPeriodTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
function pickedFunc() {
}
var $page = function () {
    this.init = function () {
        var date = new Date;
        var year = date.getFullYear();
        var month = date.getMonth() + 1;
        month = (month < 10 ? "0" + month : month);
        var currentDate = (year.toString() + "-" + month.toString());
        $("#YearMonth").val(currentDate);
        addEvent();
        var winHeight = document.body.scrollWidth;
        $("#assetReport").css("width", winHeight - 243);
    }
    //所有事件
    function addEvent() {
        //加载列表数据
        initPeriodTable();
        $("#btnGetData").on("click", function () {
            initPeriodTable();
        });
        //关闭
        $("#AssetReviewDialog_CancelBtn").on("click",
            function () {
                $("#AssetReviewDialog").modal("hide");
            }
        );
    }; //addEvent end
    function initPeriodTable() {
        layer.load();
        getPeriodData(function (datashow) {
            var datafields = new Array();
            datafields.push({ name: 'Period', type: 'string' });
            datafields.push({ name: 'VehicleModel', type: 'string' });
            datafields.push({ name: 'CompanyName', type: 'string' });
            datafields.push({ name: 'Quantity', type: 'number' });
            var source =
            {
                localdata: datashow,
                datafields: datafields,
                datatype: "json"
            };
            debugger;
            var dataAdapter = new $.jqx.dataAdapter(source);
            dataAdapter.dataBind();
            var pivotDataSource = new $.jqx.pivot(dataAdapter,
            {
                pivotValuesOnRows: false,
                totals: { rows: { subtotals: false, grandtotals: false }, columns: { subtotals: false, grandtotals: false } },
                rows: [{ dataField: 'CompanyName' }, { dataField: 'VehicleModel' }],
                columns: [{ dataField: 'Period', width: 110, align: 'center' }],
                values: [
                    {
                        dataField: 'Quantity',
                        'function': 'sum',
                        text: '总数',
                        formatSettings: { decimalPlaces: 0, align: 'center' },
                        isHidden: true,
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
            var pivotGrid = selector.$grid().jqxPivotGrid('getInstance');
            var pivotRows = pivotGrid.getPivotRows();
            for (var k = 0; k < pivotRows.items.length; k++) {
                pivotRows.items[k].expand();
            }
            var pivotCols = pivotGrid.getPivotColumns();
            for (var i = 0; i < pivotCols.items.length; i++) {
                pivotCols.items[i].expand();
            }
            pivotGrid.refresh();
            layer.closeAll('loading');
        });
    }
    var tableValue = "";
    function getPeriodData(callback) {
        $.ajax({
            url: "/AnalysisManagementCenter/OutageReport/GetManageCompanyOutageReport",
            data: { "YearMonth": $("#YearMonth").val() },
            datatype: "json",
            type: "post",
            success: function (result) {
                tableValue = result;
                callback(result);
            }
        });
    }
};
$(function () {
    var page = new $page();
    page.init();
});