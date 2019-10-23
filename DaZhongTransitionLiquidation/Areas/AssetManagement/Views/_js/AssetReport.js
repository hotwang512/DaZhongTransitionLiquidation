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
var values = new Array();
var $page = function () {
    this.init = function () {
        var date = new Date;
        var year = date.getFullYear();
        var month = date.getMonth() + 1;
        month = (month < 10 ? "0" + month : month);
        var currentDate = (year.toString() + "-" + month.toString());
        $("#YearMonth").val(currentDate);
        //获取车型
        $.ajax({
            data: { YearMonth: $("#YearMonth").val() },
            url: "/AssetManagement/AssetReport/GetAssetReportVehicleModel",
            //traditional: true,
            type: "post",
            success: function (msg) {
                debugger;
                for (var i = 0; i < msg.length; i++) {
                    var valueObj = new Object();
                    valueObj.dataField = 'VehicleModel' + i;
                    valueObj['function'] = 'sum';
                    valueObj.text = msg[i];
                    valueObj.width = 80;
                    valueObj.align = 'center';
                    valueObj.sortable = false;
                    valueObj.formatSettings = { prefix: '', decimalPlaces: 2 };
                    values.push(valueObj);
                }
                var valueObj1 = new Object();
                valueObj1.dataField = 'Quantity';
                valueObj1['function'] = 'sum';
                valueObj1.text = 'total';
                valueObj1.width = 80;
                valueObj1.align = 'center';
                valueObj1.sortable = false;
                valueObj1.formatSettings = { prefix: '', decimalPlaces: 2 };
                values.push(valueObj1);
                addEvent();
            }
        });
        
    }
    //所有事件
    function addEvent() {
        //加载列表数据
        initPeriodTable();
        initPeriodTable1();
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
            //initEndPeriodTable();
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
        getPeriodData(function (data) {
            var datashow = fillData(data);
            var datafields = new Array();
            datafields.push({ name: 'Company', type: 'string' });
            datafields.push({ name: 'PeriodType', type: 'string' });
            datafields.push({ name: 'Quantity', type: 'number' });
            for (var i = 0; i < values.length; i++) {
                datafields.push({ name: 'VehicleModel' + i, type: 'number' });
            }
            debugger;
            var source =
            {
                localdata: datashow,
                datafields:datafields,
                datatype: "json"
            };
            var dataAdapter = new $.jqx.dataAdapter(source);
            dataAdapter.dataBind();
            var pivotDataSource = new $.jqx.pivot(
                dataAdapter,
                {
                    pivotValuesOnRows: false,
                    totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: false } },
                    rows: [{ dataField: 'Company', text: '管理公司', align: 'center', width: 150, isHidden: true}],
                    columns: [{ dataField: 'PeriodType', width: 110, align: 'center' }],
                    values: values
                });
                selector.$grid().jqxPivotGrid(
                {
                    source: pivotDataSource,
                    treeStyleRows: false,
                    autoResize: true,
                    selectionEnabled: false
                });
            layer.closeAll('loading');
        });
    }
    function fillData(data) {
        var newData = [];
        for (var j = 0; j < data.length; j++) {
            var currentData = { "Company": data[j].Company, "PeriodType": data[j].PeriodType }//PeriodType
            var sum = 0;
            debugger;
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
    function initPeriodTable1() {
        layer.load();
        getPeriodData1(function (data) {
            var datashow = fillData(data);
            var datafields = new Array();
            datafields.push({ name: 'Company', type: 'string' });
            datafields.push({ name: 'PeriodType', type: 'string' });
            datafields.push({ name: 'Quantity', type: 'number' });
            for (var i = 0; i < values.length; i++) {
                datafields.push({ name: 'VehicleModel' + i, type: 'number' });
            }
            debugger;
            var source =
            {
                localdata: datashow,
                datafields: datafields,
                datatype: "json"
            };
            var dataAdapter = new $.jqx.dataAdapter(source);
            dataAdapter.dataBind();
            var pivotDataSource = new $.jqx.pivot(
                dataAdapter,
                {
                    pivotValuesOnRows: false,
                    totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: false } },
                    rows: [{ dataField: 'Company', text: '所属公司', align: 'center', width: 150, isHidden: true }],
                    columns: [{ dataField: 'PeriodType', width: 110, align: 'center' }],
                    values: values
                });
            selector.$grid1().jqxPivotGrid(
                {
                    source: pivotDataSource,
                    treeStyleRows: false,
                    autoResize: true,
                    selectionEnabled: false
                });
            layer.closeAll('loading');
        });
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
    function getPeriodData1(callback) {
        $.ajax({
            url: "/AssetManagement/AssetReport/GetBelongToCompanyAssetReport",
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