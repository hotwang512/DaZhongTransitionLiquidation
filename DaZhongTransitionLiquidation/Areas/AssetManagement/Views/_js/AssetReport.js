//资产维护
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
                url: "/AssetManagement/AssetReport/SubmitExceptionAsset",
                data: { vguids: pars },
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
                        $("#jqxTable").jqxGrid('updateBoundData');
                        $('#jqxTable').jqxGrid('clearselection');
                        break;
                    case "2":
                        jqxNotification(msg.ResultInfo, null, "success");
                        $("#myModalLabel_title2").html(msg.ResultInfo);
                        ViewReview(msg.ResultInfo2);
                        $("#jqxTable").jqxGrid('updateBoundData');
                        $('#jqxTable').jqxGrid('clearselection');
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
                   totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: true }},
                   rows: [{ dataField: 'ManageCompany', align: 'center' }],
                   columns: [{ dataField: 'VehicleModel', width: 500, align: 'center' }],
                   values: [
                       {
                           dataField: 'Quantity',
                           'function': 'sum',
                           text: '数量',
                           width: 150,
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
            }
        });
    }
};
$(function () {
    var page = new $page();
    page.init();
});
//$(document).ready(function () {
//    // prepare sample data
//    //var data = new Array();
//    //var countries =
//    //[
//    //    "Germany", "France", "United States", "Italy", "Spain", "Finland", "Canada", "Japan", "Brazil", "United Kingdom", "China", "India", "South Korea", "Romania", "Greece"
//    //];
//    //var dataPoints =
//    //[
//    //    "2.25", "1.5", "3.0", "3.3", "4.5", "3.6", "3.8", "2.5", "5.0", "1.75", "3.25", "4.0"
//    //];
//    //for (var i = 0; i < countries.length * 2; i++) {
//    //    var row = {};
//    //    var value = parseFloat(dataPoints[Math.round((Math.random() * 100)) % dataPoints.length]);
//    //    row["country"] = countries[i % countries.length];
//    //    row["value"] = value;
//    //    data[i] = row;
//    //}
//    //debugger;
//    // create a data source and data adapter

//    var source =
//    {   
//        localdata: data,
//        datatype: "json",
//        datafields:
//        [
//            { name: 'country', type: 'string' },
//            { name: 'value', type: 'number' }
//        ]
//    };
//    var dataAdapter = new $.jqx.dataAdapter(source);
//    dataAdapter.dataBind();
//    // create a pivot data source from the dataAdapter
//    var pivotDataSource = new $.jqx.pivot(
//        dataAdapter,
//        {
//            pivotValuesOnRows: false,
//            rows: [{ dataField: 'country', width: 190 }],
//            columns: [],
//            values: [
//                { dataField: 'value', width: 200, 'function': 'sum', text: '4000', formatSettings: { align: 'left', prefix: '', decimalPlaces: 2 } },
//                { dataField: 'value', width: 200, 'function': 'sum', text: 'cells center alignment', formatSettings: { align: 'center', prefix: '', decimalPlaces: 2 } },
//                { dataField: 'value', width: 200, 'function': 'sum', text: 'cells right alignment', formatSettings: { align: 'right', prefix: '', decimalPlaces: 2 } }
//            ]
//        });
//    // create a pivot grid
//    $('#jqxTable').jqxPivotGrid(
//    {
//        source: pivotDataSource,
//        treeStyleRows: true,
//        autoResize: true,
//        multipleSelectionEnabled: true
//    });
//});