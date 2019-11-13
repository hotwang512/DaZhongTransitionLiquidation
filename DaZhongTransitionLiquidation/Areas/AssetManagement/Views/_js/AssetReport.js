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
        $("#assetReport").css("width", winHeight - 243);
    }
    //所有事件
    function addEvent() {
        //加载列表数据
        initPeriodTable();
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
        getPeriodData(function (msg) {
            switch (msg.Status) {
            case "0":
                jqxNotification("获取数据失败！", null, "error");
                break;
            case "1":
                var values = fillValues(msg.ResultInfo);
                var datashow = fillData(msg.ResultInfo2);
                var datafields = new Array();
                datafields.push({ name: 'CompanyType', type: 'string' });
                datafields.push({ name: 'Company', type: 'string' });
                datafields.push({ name: 'PeriodType', type: 'string' });
                datafields.push({ name: 'Quantity', type: 'number' });
                for (var i = 0; i < values.length; i++) {
                    datafields.push({ name: 'VehicleModel' + i, type: 'number' });
                }
                var source =
                {
                    localdata: datashow,
                    datafields: datafields,
                    datatype: "json"
                };
                var dataAdapter = new $.jqx.dataAdapter(source);
                dataAdapter.dataBind();
                var pivotDataSource = new $.jqx.pivot(dataAdapter,
                {
                    pivotValuesOnRows: false,
                    totals: { rows: { subtotals: false, grandtotals: false }, columns: { subtotals: false, grandtotals: false } },
                    rows: [{ dataField: 'CompanyType' }, { dataField: 'Company' }],
                    columns: [{ dataField: 'PeriodType', width: 110, align: 'center' }],
                    values: values
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
                pivotGrid.refresh();
                break;
            }
            layer.closeAll('loading');
        });
    }
    function fillData(data) {
        var newData = [];
        for (var j = 0; j < data.length; j++) {
            var currentData = {  "CompanyType": data[j].CompanyType, "PeriodType": data[j].PeriodType ,"Company": data[j].Company}//PeriodType
            var sum = 0;
            for (var k = 0; k < data[j].ResultVehicleModelList.length; k++) {
                currentData["VehicleModel" + k] = data[j].ResultVehicleModelList[k].Quantity;
                sum += data[j].ResultVehicleModelList[k].Quantity;
            }
            currentData["Quantity"]= sum;
            newData.push(currentData);
        }
        debugger;
        return newData;
    }
    function fillValues(data) {
        var values = [];
        for (var i = 0; i < data.length; i++) {
            var valueObj = new Object();
            valueObj.dataField = 'VehicleModel' + i;
            valueObj.function = 'sum';
            valueObj.text = data[i];
            valueObj.width = 80;
            valueObj.align = 'center';
            valueObj.sortable = false;
            valueObj.formatSettings = { prefix: '', decimalPlaces: 0 };
            values.push(valueObj);
        }
        var QuantityObj = new Object();
        QuantityObj.dataField = 'Quantity';
        QuantityObj.function = 'sum';
        QuantityObj.text = '小计';
        QuantityObj.width = 80;
        QuantityObj.align = 'center';
        QuantityObj.sortable = false;
        QuantityObj.formatSettings = { prefix: '', decimalPlaces: 0 };
        values.push(QuantityObj);
        return values;
    }
    var tableValue = "";
    function getPeriodData(callback) {
        $.ajax({
            url: "/AssetManagement/AssetReport/GetManageCompanyAssetReport",
            data: { "YearMonth": $("#YearMonth").val()},
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