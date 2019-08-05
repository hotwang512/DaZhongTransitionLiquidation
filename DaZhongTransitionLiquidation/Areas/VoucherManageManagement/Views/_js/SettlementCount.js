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

        //initTable();
        //查询
        selector.$btnSearch().on("click", function () {
            var yearMonth = $("#YearMonth").val();
            if (yearMonth == "") {
                jqxNotification("请选择条件！", null, "error");
                return;
            }
            initTable();
        });


        //重置
        selector.$btnReset().on("click", function () {
            //selector.$txtMonth().val("");
            var data = new Array();
            var firstNames =
            [
                "Andrew", "Nancy", "Shelley", "Regina", "Yoshi", "Antoni", "Mayumi", "Ian", "Peter", "Lars", "Petra", "Martin", "Sven", "Elio", "Beate", "Cheryl", "Michael", "Guylene"
            ];
            var lastNames =
            [
                "Fuller", "Davolio", "Burke", "Murphy", "Nagase", "Saavedra", "Ohno", "Devling", "Wilson", "Peterson", "Winkler", "Bein", "Petersen", "Rossi", "Vileid", "Saylor", "Bjorn", "Nodier"
            ];
            var productNames =
            [
                "Black Tea", "Green Tea", "Caffe Espresso", "Doubleshot Espresso", "Caffe Latte", "White Chocolate Mocha", "Cramel Latte", "Caffe Americano", "Cappuccino", "Espresso Truffle", "Espresso con Panna", "Peppermint Mocha Twist"
            ];
            var priceValues =
            [
                "2.25", "1.5", "3.0", "3.3", "4.5", "3.6", "3.8", "2.5", "5.0", "1.75", "3.25", "4.0"
            ];
            for (var i = 0; i < 500; i++) {
                var row = {};
                var productindex = Math.floor(Math.random() * productNames.length);
                var price = parseFloat(priceValues[productindex]);
                var quantity = 1 + Math.round(Math.random() * 10);
                row["firstname"] = firstNames[Math.floor(Math.random() * firstNames.length)];
                row["lastname"] = lastNames[Math.floor(Math.random() * lastNames.length)];
                row["productname"] = productNames[productindex];
                row["price"] = price;
                row["quantity"] = quantity;
                row["total"] = price * quantity;
                data[i] = row;
            }
        });
        //导出
        selector.$btnExport().on("click", function () {
            location.href = "/VoucherManageManagement/SettlementCount/ExportSettlementData"
            //$.ajax({
            //    url: "/VoucherManageManagement/SettlementCount/ExportSettlementData",
            //    data: { },
            //    datatype: "json",
            //    type: "post",
            //    success: function (result) {

            //    }
            //});
        })
        //计算
        $("#btnCount").on("click", function () {
            var yearMonth = $("#YearMonth").val();
            if (yearMonth == "") {
                jqxNotification("请选择条件！", null, "error");
                return;
            }
            $.ajax({
                url: "/VoucherManageManagement/SettlementCount/CountSettlementData",
                data: { "YearMonth": yearMonth },
                datatype: "json",
                type: "post",
                success: function (result) {
                    switch (result.Status) {
                        case "1": jqxNotification("计算成功！", null, "success"); break;
                        case "2": jqxNotification("当前月份没有数据！", null, "error"); break; break;
                        case "3": jqxNotification("计算失败！", null, "error"); break;
                        default:

                    }
                }
            });
        });


        function initTable() {
            getData(function (data) {


                var source =
                    {
                        localdata: data,
                        datafields:
                        [
                            { name: 'VGUID', type: 'string' },
                            { name: 'Model', type: 'string' },
                            { name: 'ClassType', type: 'string' },
                            { name: 'CarType', type: 'string' },
                            { name: 'Business', type: 'string' },
                            { name: 'BusinessKey', type: 'string' },
                            { name: 'BusinessType', type: 'string' },
                            { name: 'YearMonth', type: 'string' },
                            { name: 'DAYS', type: 'string' },
                            { name: 'Money', type: 'number' },
                            { name: 'Account', type: 'number' },
                            { name: 'CarAccount1', type: 'number' },
                            { name: 'CarAccount2', type: 'number' },
                            { name: 'CarAccount3', type: 'number' },
                            { name: 'CarAccount4', type: 'number' },
                        ],
                        datatype: "json",
                        //data: { "Month": selector.$txtMonth().val() },
                        //url: "/ReportManagement/AmountReport/GetAmountReportData"    //获取数据源的路径
                    };

                var dataAdapter = new $.jqx.dataAdapter(source);
                dataAdapter.dataBind();
                var pivotDataSource = new $.jqx.pivot(
                   dataAdapter,
                   {
                       pivotValuesOnRows:false,
                       //totals: { rows: { subtotals: false, grandtotals: false }, columns: { subtotals: false, grandtotals: false } },
                       rows: [{ dataField: 'BusinessKey', align: 'center',}, { dataField: 'BusinessType', align: 'center',width:'150px' }],
                       columns: [{ dataField: 'Model', align: 'center' }, { dataField: 'ClassType', align: 'center' }],
                       values: [
                           {
                               dataField: 'CarAccount1',
                               'function': 'sum',
                               text: '4000',
                               align: 'center',
                               formatSettings: { prefix: '', decimalPlaces: 2 },
                               sortable: false
                           },
                           {
                               dataField: 'CarAccount2',
                               'function': 'sum',
                               text: '途安1.4T',
                               align: 'center',
                               formatSettings: { prefix: '', decimalPlaces: 2 },
                               sortable: false
                           },
                           {
                               dataField: 'CarAccount3',
                               'function': 'sum',
                               text: '途安1.6',
                               align: 'center',
                               formatSettings: { prefix: '', decimalPlaces: 2 },
                               sortable: false
                           },
                           {
                               dataField: 'CarAccount4',
                               'function': 'sum',
                               text: '途安1.6L',
                               align: 'center',
                               formatSettings: { prefix: '', decimalPlaces: 2 },
                               sortable: false
                           },
                       ],

                   });
                selector.$grid().jqxPivotGrid(
                   {
                       source: pivotDataSource,
                       treeStyleRows: false,
                       selectionEnabled: true,
                       autoResize: false,
                       multipleSelectionEnabled: false
                       //sortable: false
                   });
                var pivotGrid = selector.$grid().jqxPivotGrid('getInstance');
                var pivotRows = pivotGrid.getPivotRows();
                var pivotColumns = pivotGrid.getPivotColumns();
                for (var i = 0; i < pivotRows.items.length; i++) {
                    pivotRows.items[i].expand();
                }
                for (var i = 0; i < pivotColumns.items.length; i++) {
                    pivotColumns.items[i].expand();
                }
                pivotGrid.refresh();
            });

        }
        var tableValue = "";
        function getData(callback) {
            $.ajax({
                url: "/VoucherManageManagement/SettlementCount/GetSettlementCountData",
                data: { "yearMonth": $("#YearMonth").val() },
                datatype: "json",
                type: "post",
                success: function (result) {
                    tableValue = result;
                    callback(result);
                }
            });
        }
    }; //addEvent end

};

$(function () {
    var page = new $page();
    page.init();

});