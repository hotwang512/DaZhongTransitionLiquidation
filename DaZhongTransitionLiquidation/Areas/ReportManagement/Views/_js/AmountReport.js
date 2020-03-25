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

        initTable();
        //查询
        selector.$btnSearch().on("click", function () {
            initTable();
        });


        //重置
        selector.$btnReset().on("click", function () {
            selector.$txtMonth().val("");
            selector.$txtChannel().val("");
        });
        //导出
        selector.$btnExport().on("click", function () {
            var pivotColumns = $('#datatable').jqxPivotGrid('getPivotColumns').visibleLeafItems;
            var companyName = "";
            for (i = 0; i < pivotColumns.length; i++) {
                if (i == pivotColumns.length) {
                    continue;
                }
                if (i % 3 == 0) {
                    companyName += pivotColumns[i].parentItem.text + ",";
                }
            }

            location.href = "/ReportManagement/AmountReport/GetAmountReportDataOut?month=" + selector.$txtMonth().val() + "&companyName=" + companyName + "&channel=" + selector.$txtChannel().val() + "";
            //$.ajax({
            //    url: "/ReportManagement/AmountReport/GetAmountReportDataOut",
            //    data: { "month": selector.$txtMonth().val(),"companyName":companyName,"channel": selector.$txtChannel().val() },
            //    datatype: "json",
            //    type: "post",
            //    success: function (result) {
                   
            //    }
            //});
        })

       

        function initTable() {
            getData(function (data) {


                var source =
                    {
                        localdata: data,
                        datafields:
                        [
                            { name: 'ChannelName', type: 'string' },
                            { name: 'Channel_Id', type: 'string' },
                            { name: 'SubjectNmae', type: 'string' },
                            { name: 'SubjectId', type: 'string' },
                            { name: 'RevenueMonth', type: 'string' },
                            { name: 'OrganizationName', type: 'string' },
                            { name: 'RevenueDate', type: 'string' },
                            { name: 'ActualAmountTotal', type: 'number' },
                            { name: 'PaymentAmountTotal', type: 'number' },
                            { name: 'DriverBearFeesTotal', type: 'number' },
                            { name: 'CompanyBearsFeesTotal', type: 'number' }
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
                       pivotValuesOnRows: false,
                       totals: { rows: { subtotals: false, grandtotals: true }, columns: { subtotals: false, grandtotals: true } },
                       rows: [{ dataField: 'ChannelName' }, { dataField: 'RevenueDate' }],
                       columns: [{ dataField: 'OrganizationName', align: 'center', }],
                       values: [

                           {
                               dataField: 'PaymentAmountTotal',
                               'function': 'sum',
                               text: '营收缴款',
                               align: 'center',
                               formatSettings: { prefix: '', decimalPlaces: 2 },
                               sortable: false
                           },
                           {
                               dataField: 'CompanyBearsFeesTotal',
                               'function': 'sum',
                               text: '手续费',
                               align: 'center',
                               formatSettings: { prefix: '', decimalPlaces: 2 },
                               sortable: false
                           },
                           {
                               dataField: 'ActualAmountTotal',
                               'function': 'sum',
                               text: '银行收款',
                               align: 'center',
                               formatSettings: { prefix: '', decimalPlaces: 2 },
                               sortable: false
                           }
                       ],
                       
                   });
                selector.$grid().jqxPivotGrid(
                   {
                       source: pivotDataSource,
                       treeStyleRows: true,
                       selectionEnabled: true,
                       //sortable: false
                   });
                var pivotGrid = selector.$grid().jqxPivotGrid('getInstance');
                var pivotRows = pivotGrid.getPivotRows();
                for (var i = 0; i < pivotRows.items.length; i++) {
                    pivotRows.items[i].expand();
                }
                pivotGrid.refresh();
            });

        }
        var tableValue = "";
        function getData(callback) {
            $.ajax({
                url: "/ReportManagement/AmountReport/GetAmountReportData",
                data: { "month": selector.$txtMonth().val(), "channel": selector.$txtChannel().val() },
                datatype: "json",
                type: "post",
                success: function (result) {
                    tableValue = result;
                    callback(result);
                }
            });
        }

        function getExplorer() {
            if (window.navigator.userAgent.indexOf("MSIE") >= 0) {
                return 1;
            } else if (window.navigator.userAgent.indexOf("Firefox") >= 0) {
                return 0;
            } else if (window.navigator.userAgent.indexOf("Chrome") >= 0) {
                return 0;
            } else if (window.navigator.userAgent.indexOf("Opera") >= 0) {
                return 0;
            } else if (window.navigator.userAgent.indexOf("Safari") >= 0) {
                return 0;
            } else {
                return 1;
            }
        }
        function Msie(id) {
            var tabID = document.getElementById(id);
            var aXO = new ActiveXObject("Excel.Application");
            var oWB = oXL.Workbooks.Add();
            var oSheet = oWB.ActiveSheet;
            var len = tabID.rows.length;   //取得表格行数 
            for (i = 0; i < len; i++) {
                var Lenc = tabID.rows(i).cells.length;  //取得每行的列数 
                for (j = 0; j < Lenc; j++) {
                    oSheet.Cells(i + 1, j + 1).value = tabID.rows(i).cells(j).innerText;
                    //赋值 
                }
            }
            aXO.Visible = true;
        }
        function other(mytalbe) {
            var table = document.getElementById(mytalbe);
            // 克隆（复制）此table元素，这样对复制品进行修改（如添加或改变table的标题等），导出复制品，而不影响原table在浏览器中的展示。
            table = table.cloneNode(true);
            var uri = 'data:application/vnd.ms-excel;base64,',
                template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><?xml version="1.0" encoding="UTF-8" standalone="yes"?><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table style="vnd.ms-excel.numberformat:@">{table}</table></body></html>',
                base64 = function (s) {
                    return window.btoa(unescape(encodeURIComponent(s)));
                },
                format = function (s, c) {
                    return s.replace(/{(\w+)}/g, function (m, p) {
                        return c[p];
                    });
                };
            if (!table.nodeType) table = document.getElementById(table);
            var ctx = {
                worksheet: name || 'Worksheet',
                table: table.innerHTML
            };
            window.location.href = uri + base64(format(template, ctx));
        }
        function tabletoExcel(tableID) {
            if (getExplorer() == 1) {
                Msie(tableID)
            } else {
                other(tableID)
            }
        }
    }; //addEvent end

};

$(function () {
    var page = new $page();
    page.init();

});
