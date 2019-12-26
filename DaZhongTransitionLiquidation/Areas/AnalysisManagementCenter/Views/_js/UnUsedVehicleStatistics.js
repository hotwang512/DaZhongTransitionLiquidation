//资产维护
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxManageCompanyPeriodTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
function pickedFunc() {
}
var $page = function () {
    this.init = function () {;
        var date = new Date;
        var year = date.getFullYear();
        var currentDate = (year.toString());
        $("#YearMonth").val(currentDate);
        addEvent();
        var winHeight = document.body.scrollWidth;
        $("#assetReport").css("width", winHeight - 240);
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
        debugger;
        getPeriodData(function (mps) {
            if (mps.length > 0) {
                var datafields = new Array();
                datafields.push({ name: 'Period', type: 'string' });
                datafields.push({ name: 'OrganizationType', type: 'string' });
                datafields.push({ name: 'CompanyType', type: 'string' });
                datafields.push({ name: 'CompanyCode', type: 'string' });
                datafields.push({ name: 'CompanyName', type: 'string' });
                datafields.push({ name: 'Quantity', type: 'number' });
                datafields.push({ name: 'StopVehicleQuantity', type: 'number' });
                datafields.push({ name: 'StopLicence', type: 'number' });
                datafields.push({ name: 'StopAll', type: 'number' });
                var source =
                {
                    localdata: mps,
                    datafields: datafields,
                    datatype: "json"
                };
                debugger;
                var dataAdapter = new $.jqx.dataAdapter(source);
                dataAdapter.dataBind();
                var pivotDataSource = new $.jqx.pivot(dataAdapter,
                {
                    pivotValuesOnRows: false,
                    totals: { rows: { subtotals: false, grandtotals: false }, columns: { subtotals: true, grandtotals: false } },
                    rows: [{ dataField: 'OrganizationType' }, { dataField: 'CompanyType' }, { dataField: 'CompanyName' }],
                    columns: [{ dataField: 'Period', width: 110, align: 'center' }],
                    values: [
                        {
                            dataField: 'StopVehicleQuantity',
                            'function': 'sum',
                            text: '搁车',
                            formatSettings: { decimalPlaces: 0, align: 'center' },
                            isHidden: true,
                            sortable: false
                        },
                        {
                            dataField: 'StopLicence',
                            'function': 'sum',
                            text: '搁牌',
                            formatSettings: { decimalPlaces: 0, align: 'center' }
                        },
                        {
                            dataField: 'StopAll',
                            'function': 'sum',
                            text: '总数',
                            formatSettings: { decimalPlaces: 0, align: 'center' }
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
            } else {
                jqxNotification("当前年份没有数据！", null, "error");
            }
            layer.closeAll('loading');
            $("#assetReport").show();
        });
    }
    var tableValue = "";
    function getPeriodData(callback) {
        $.ajax({
            url: "/AnalysisManagementCenter/UnUsedVehicleStatistics/GetUnUsedVehicleStatistics",
            data: { "Year": $("#YearMonth").val() },
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