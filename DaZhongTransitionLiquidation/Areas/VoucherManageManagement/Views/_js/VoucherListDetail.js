//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
}; //selector end
var isEdit = false;
var vguid = "";
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        $('#jqxFileUpload1').jqxFileUpload({ width: '150px', uploadUrl: 'imageUpload.php', fileInputName: 'fileToUpload' });
        $('#jqxFileUpload2').jqxFileUpload({ width: '150px', uploadUrl: 'imageUpload.php', fileInputName: 'fileToUpload' });
        $('#jqxFileUpload3').jqxFileUpload({ width: '150px', uploadUrl: 'imageUpload.php', fileInputName: 'fileToUpload' });
        $('#jqxFileUpload4').jqxFileUpload({ width: '150px', uploadUrl: 'imageUpload.php', fileInputName: 'fileToUpload' });
        $('#jqxFileUpload5').jqxFileUpload({ width: '150px', uploadUrl: 'imageUpload.php', fileInputName: 'fileToUpload' });
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#AccountingPeriod").val("");
            //$("#TransactionDate").val("");
            //$("#TransactionDateEnd").val("");
            //$("#PaymentUnit").val("");
        });

        //新增
        $("#btnAdd").on("click", function () {
            //window.location.open = "/VoucherManageManagement/VoucherListDetail/Index";
            //window.open("/VoucherManageManagement/VoucherListDetail/Index");
        });


    }; //addEvent end


    function initTable() {
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
        var status = $.request.queryString().Status;
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'CompanyCode', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'SubjectAndDescrption', type: 'string' },
                    { name: 'DebitAmount', type: 'number' },
                    { name: 'CreditAmount', type: 'number' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "Status": status },
                url: "/VoucherManageManagement/VoucherList/GetVoucherListDatas"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid().jqxDataTable(
            {
                pageable: false,
                width: "100%",
                height: 400,
                pageSize: 10,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 25,
                aggregatesHeight: 25,
                showAggregates: true,
                columns: [
                    { text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                    { text: '摘要', datafield: 'Remark', width: 350, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '科目及描述', datafield: 'SubjectAndDescrption', width: 350, align: 'center', cellsAlign: 'center'},
                    {
                        text: '借方金额', datafield: 'DebitAmount', width: 200, cellsFormat: "d2", align: 'center', cellsAlign: 'center', aggregates: [{
                            'Total':
                              function (aggregatedValue, currentValue, column, record) {
                                  var total = currentValue * parseInt(record['Currency']);
                                  return aggregatedValue + total;
                              }
                        }],
                        aggregatesRenderer: function (aggregates, column, element) {
                            var total = aggregates.Total == null ? "0.00" : aggregates.Total;
                            var renderString = "<div style='margin: 4px; float: center;  height: 100%;'>";
                            renderString += "<strong>合 计: </strong>" + total + "</div>";
                            return renderString;
                        }
                    },
                    {
                        text: '贷方金额', datafield: 'CreditAmount', align: 'center', cellsFormat: "d2", cellsAlign: 'center', aggregates: [{
                            'Total':
                              function (aggregatedValue, currentValue, column, record) {
                                  var total = currentValue * parseInt(record['BatchName']);
                                  return aggregatedValue + total;
                              }
                        }],
                        aggregatesRenderer: function (aggregates, column, element) {
                            var total = aggregates.Total == null ? "0.00" : aggregates.Total;
                            var renderString = "<div style='margin: 4px; float: center;  height: 100%;'>";
                            renderString += "<strong>合 计: </strong>" + total + "</div>";
                            return renderString;
                        }
                    },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }

    function detailFunc(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "1") {
            container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.VoucherSubject + "','" + rowData.VoucherSummary + "','" + rowData.VoucherSubjectName + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Batch + "</a>";
        } else {
            container = "<span>" + rowData.Batch + "</span>";
        }
        return container;
    }

    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
    }

    function rendererFunc() {
        var checkBox = "<div id='jqx_datatable_checkbox_all' class='jqx_datatable_checkbox_all' style='z-index: 999; margin-left:7px ;margin-top: 7px;'>";
        checkBox += "</div>";
        return checkBox;
    }

    function renderedFunc(element) {
        var grid = selector.$grid();
        element.jqxCheckBox();
        element.on('change', function (event) {
            var checked = element.jqxCheckBox('checked');

            if (checked) {
                var rows = grid.jqxDataTable('getRows');
                for (var i = 0; i < rows.length; i++) {
                    grid.jqxDataTable('selectRow', i);
                    grid.find(".jqx_datatable_checkbox").attr("checked", "checked")
                }
            } else {
                grid.jqxDataTable('clearSelection');
                grid.find(".jqx_datatable_checkbox").removeAttr("checked", "checked")
            }
        });
        return true;
    }
};

$(function () {
    var page = new $page();
    page.init();
});