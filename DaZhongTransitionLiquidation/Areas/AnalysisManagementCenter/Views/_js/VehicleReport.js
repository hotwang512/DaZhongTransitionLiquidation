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
        $("#continer").css("width", winHeight - 243);
    }
    //所有事件
    function addEvent() {
        //加载列表数据
        initPeriodTable();
        //提交
        $("#btnSubmit").on("click", function () {
            $.ajax({
                url: "/AnalysisManagementCenter/VehicleReport/SubmitVehicleReport",
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
            $.ajax({
                url: "/AnalysisManagementCenter/VehicleReport/GetVehicleModel",
                data: { YearMonth: $("#YearMonth").val() },
                type: "post",
                success: function (msg) {
                    initPeriodTable(msg);
                }
            });
        });
        //关闭
        $("#AssetReviewDialog_CancelBtn").on("click",
            function () {
                $("#AssetReviewDialog").modal("hide");
            }
        );
    }; //addEvent end
    function initPeriodTable(data) {
        layer.load();
        getPeriodData(function (datashow) {
            var datafields = new Array();
            datafields.push({ name: 'CompanyType', type: 'string' });
            datafields.push({ name: 'CompanyName', type: 'string' });
            datafields.push({ name: 'PeriodType', type: 'string' });
            datafields.push({ name: 'VehicleModel', type: 'string' });
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
                rows: [{ dataField: 'CompanyType' }, { dataField: 'CompanyName' }],
                columns: [{ dataField: 'PeriodType', width: 110, align: 'center' }, { dataField: 'VehicleModel', width: 110, align: 'center' }],
                values: [
                    {
                        dataField: 'Quantity','function': 'sum', text: 'sum', formatSettings: { decimalPlaces:0, align: 'right' }
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
            url: "/AnalysisManagementCenter/VehicleReport/GetManageCompanyVehicleReport",
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