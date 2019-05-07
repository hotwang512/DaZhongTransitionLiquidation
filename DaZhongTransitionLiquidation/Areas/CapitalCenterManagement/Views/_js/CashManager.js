var $page = function () {

    this.init = function () {
        addEvent();
    };
    var selector = this.selector =
        {
            $grid: function () { return $("#jqxTable") }
        }

    function addEvent() {
        //加载列表数据
        initTable();
        $("#btnAdd").on("click", function () {
            window.location.href = "/CapitalCenterManagement/CashManagerDetail/Index";
        })
    }

    function initTable() {
        var source =
            {
                datafields:
                [
                    //{ name: "checkbox", type: null },
                    { name: 'No', type: 'string' },
                    { name: 'AccountModeName', type: 'string' },
                    { name: 'CompanyName', type: 'date' },
                    { name: 'ApplyDate', type: 'number' },
                    { name: 'BankAccountName', type: 'number' },
                    { name: 'BankAccount', type: 'string' },
                    { name: 'BankName', type: 'string' },
                    { name: 'Money', type: 'string' },
                    { name: 'CheckNo', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Cashier', type: 'string' },
                    { name: 'Auditor', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'AccountModeCode', type: 'string' },
                    { name: 'CompanyCode', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "ApplyDate": $("#ApplyDate").val(), "BankName": $("#BankName").val() },
                url: "/CapitalCenterManagement/CashManager/GetCashManagerData"   //获取数据源的路径
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
                columnsHeight: 30,
                columns: [
                    //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '流水号', datafield: 'No', pinned: true, width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '账套', datafield: 'AccountModeName', pinned: true, width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '公司', datafield: 'CompanyName', pinned: true, width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '申请日期', datafield: 'ApplyDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '全称', datafield: 'BankAccountName', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '账号', datafield: 'BankAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '开户银行', datafield: 'BankName', width: 350, align: 'center', cellsAlign: 'center' },
                    { text: '提现金额', datafield: 'Money', cellsFormat: "d2", width: 100, align: 'center', cellsAlign: 'center', },
                    { text: '支票编号', datafield: 'CheckNo', width: 150, align: 'center', cellsAlign: 'center', },
                    { text: '提现事由', datafield: 'Remark', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '出纳', datafield: 'Cashier', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '审核', datafield: 'Auditor', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'AccountModeCode', datafield: 'AccountModeCode', hidden: true },
                    { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });
    }
    function detailFunc(row, column, value, rowData) {
        var container = "<a href='#' onclick=link('" + rowData.VGUID + "') style=\"text-decoration: underline;color: #333;\">" + rowData.No + "</a>";
        return container;
    }
};

$(function () {
    var page = new $page();
    page.init();
});

function link(VGUID) {
    window.location.href = "/CapitalCenterManagement/CashManagerDetail/Index?VGUID=" + VGUID;
}