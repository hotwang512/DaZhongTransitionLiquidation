//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxManageCompanyPeriodTable") },
    $grid1: function () { return $("#jqxBelongToCompanyPeriodTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
};
var vguid = "";
var mydate = new Date();
var ManagementCompany = [];
var dataArr = [];
var $page = function () {
    this.init = function () {
        var date = new Date();
        $("#DateOfYear").val(date.getFullYear());
        GetVehicleAgeReportDetail();
        addEvent();
        var winHeight = document.body.scrollWidth;
        $("#assetReport").css("width", winHeight - 260);
        $('#tabs').jqxTabs({ width: 1500, height: "100%", position: 'top' });
    }
    //所有事件
    function addEvent() {
        $("#btnSearch").on("click",
            function () {
                GetVehicleAgeReportDetail();
            });
    }; //addEvent end
};

function parseToInt(str) {
    if (str != null) {
        return parseInt(str);
    } else {
        return 0;
    }
}
function GetVehicleAgeReportDetail() {
    layer.load();
    getPeriodData(function (data) {
        var mps = data.Rows;
        if (mps.length > 0) {
            debugger;
            var datafields = new Array();
            datafields.push({ name: 'FA_LOC_1', type: 'string' });
            datafields.push({ name: 'FA_LOC_2', type: 'string' });
            datafields.push({ name: 'MONTHS', type: 'string' });
            datafields.push({ name: 'DESCRIPTION', type: 'string' });
            datafields.push({ name: 'PERIOD_CODE', type: 'string' });
            datafields.push({ name: 'QUANTITY', type: 'number' });
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
                totals: { rows: { subtotals: false, grandtotals: false }, columns: { subtotals: false, grandtotals: true } },
                rows: [{ dataField: 'FA_LOC_1' }, { dataField: 'DESCRIPTION' }, { dataField: 'MONTHS' }],
                columns: [{ dataField: 'PERIOD_CODE', width: 110, align: 'center' }],
                values: [
                    {
                        dataField: 'QUANTITY',
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
                for (var n = 0; n < pivotRows.items[k].items.length; n++) {
                    pivotRows.items[k].items[n].expand();
                }
                pivotRows.items[k].expand();
            }
            var pivotCols = pivotGrid.getPivotColumns();
            for (var i = 0; i < pivotCols.items.length; i++) {
                pivotCols.items[i].expand();
            }
            pivotGrid.refresh();
            var dataAdapter1 = new $.jqx.dataAdapter(source);
            dataAdapter.dataBind();
            var pivotDataSource1 = new $.jqx.pivot(dataAdapter,
                {
                    pivotValuesOnRows: false,
                    totals: { rows: { subtotals: false, grandtotals: false }, columns: { subtotals: false, grandtotals: true } },
                    rows: [{ dataField: 'FA_LOC_2' }, { dataField: 'DESCRIPTION' }, { dataField: 'MONTHS' }],
                    columns: [{ dataField: 'PERIOD_CODE', width: 110, align: 'center' }],
                    values: [
                        {
                            dataField: 'QUANTITY',
                            'function': 'sum',
                            text: '总数',
                            formatSettings: { decimalPlaces: 0, align: 'center' },
                            isHidden: true,
                            sortable: false
                        }
                    ]
                });
            selector.$grid1().jqxPivotGrid(
                {
                    source: pivotDataSource1,
                    treeStyleRows: true,
                    autoResize: true,
                    selectionEnabled: true
                });
            var pivotGrid1 = selector.$grid1().jqxPivotGrid('getInstance');
            var pivotRows1 = pivotGrid1.getPivotRows();
            for (var p = 0; p < pivotRows1.items.length; p++) {
                for (var m = 0; m < pivotRows1.items[p].items.length; m++) {
                    pivotRows1.items[p].items[m].expand();
                }
                pivotRows1.items[p].expand();
            }
            var pivotCols1 = pivotGrid.getPivotColumns();
            for (var l = 0; l < pivotCols1.items.length; l++) {
                pivotCols1.items[l].expand();
            }
            pivotGrid1.refresh();
            layer.closeAll('loading');
            $("#assetReport").show();
        } else {
            jqxNotification("当前年份没有数据！", null, "error");
        }
        layer.closeAll('loading');
    });
}

var tableValue = "";
function getPeriodData(callback) {
    $.ajax({
        url: "/AnalysisManagementCenter/VehicleAgeReport/GetVehicleAgeReport",
        data: { "DateOfYear": $("#DateOfYear").val() },
        datatype: "json",
        type: "post",
        success: function (result) {
            tableValue = result;
            callback(result);
        }
    });
}
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}
$(function () {
    var page = new $page();
    page.init();
});
function formatDate(NewDtime) {
    var d = NewDtime;
    var datetime = d.getFullYear() + '-' + (d.getMonth() + 1);//  + '-' + d.getDate() + ' ' + d.getHours() + ':' + d.getMinutes() + ':' + d.getSeconds()
    return datetime;
}