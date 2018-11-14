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
        var status = $.request.queryString().Status;
        if (status == "1") {
            $("#buttonList").show();
        }
        if (status == "2") {
            $("#buttonList2").show();
        }
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
            window.open("/VoucherManageManagement/VoucherListDetail/Index");
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
                    { name: 'CompanyName', type: 'string' },
                    { name: 'AccountingPeriod', type: 'date' },
                    { name: 'Currency', type: 'string' },
                    { name: 'BatchName', type: 'string' },
                    { name: 'VoucherNo', type: 'string' },
                    { name: 'VoucherDate', type: 'date' },
                    { name: 'VoucherType', type: 'string' },
                    { name: 'DebitAmountTotal', type: 'number' },
                    { name: 'CreditAmountTotal', type: 'number' },
                    { name: 'FinanceDirector', type: 'string' },
                    { name: 'Bookkeeping', type: 'string' },
                    { name: 'Auditor', type: 'string' },
                    { name: 'DocumentMaker', type: 'string' },
                    { name: 'Cashier', type: 'string' },
                    { name: 'Attachment1', type: 'string' },
                    { name: 'Attachment2', type: 'string' },
                    { name: 'Attachment3', type: 'string' },
                    { name: 'Attachment4', type: 'string' },
                    { name: 'Attachment5', type: 'string' },
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
                pageable: true,
                width: "100%",
                height: 400,
                pageSize: 10,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                columns: [
                    { text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                    { text: '营运公司', datafield: 'CompanyName', width: 150, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '会计期', datafield: 'AccountingPeriod', width: 150, pinned: true, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM" },
                    { text: '币种', datafield: 'Currency', width: 150, pinned: true, align: 'center', cellsAlign: 'center', },
                    { text: '批名', datafield: 'BatchName', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '凭证号码', datafield: 'VoucherNo', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '凭证日期', datafield: 'VoucherDate', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '凭证类型', datafield: 'VoucherType', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '借方金额合计', datafield: 'DebitAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '贷方金额合计', datafield: 'CreditAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },                   
                    { text: '财务主管', datafield: 'FinanceDirector', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '记账', datafield: 'Bookkeeping', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '审核', datafield: 'Auditor', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '制单', datafield: 'DocumentMaker', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '出纳', datafield: 'Cashier', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center',hidden: true  },
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