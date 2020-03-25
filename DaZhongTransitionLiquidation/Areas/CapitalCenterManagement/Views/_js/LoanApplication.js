$(".input_text").attr("autocomplete", "new-password");
var $page = function () {

    this.init = function () {
        addEvent();
    };
    var selector = this.selector =
        {
            $grid: function () { return $("#jqxTable") },
            $btnSearch: function () { return $("#btnSearch") },
            $btnReset: function () { return $("#btnReset") },
        }

    function addEvent() {
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#ApplyDate").val("");
            $("#BankName").val("");
        });
        //申请借款
        $("#btnLoan").on("click", function () {
            window.location.href = "/CapitalCenterManagement/LoanApplicationDetail/Index";
        })
        //申请还款
        $("#btnRepay").on("click", function () {
            //window.location.href = "/CapitalCenterManagement/LoanApplicationDetail/Index";
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
                    { name: 'CompanyName', type: 'string' },
                    { name: 'ApplyDate', type: 'date' },
                    { name: 'Applicants', type: 'string' },
                    { name: 'OrgName', type: 'string' },
                    { name: 'Purpose', type: 'string' },
                    { name: 'Money', type: 'number' },
                    { name: 'CheckNo', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'GeneralManager', type: 'string' },
                    { name: 'FinancialManager', type: 'string' },
                    { name: 'DivisionDirector', type: 'string' },
                    { name: 'Cashier', type: 'string' },
                    { name: 'OrgId', type: 'string' },
                    { name: 'AccountModeCode', type: 'string' },
                    { name: 'CompanyCode', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "ApplyDate": $("#ApplyDate").val(), "OrgId": $("#OrgId").val() },
                url: "/CapitalCenterManagement/LoanApplication/GetLoanApplicationData"   //获取数据源的路径
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
                    { text: '申请人', datafield: 'Applicants', width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '部门', datafield: 'OrgName', width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '申请日期', datafield: 'ApplyDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '申请金额', datafield: 'Money', cellsFormat: "d2", width: 100, align: 'center', cellsAlign: 'center', },
                    { text: '用途', datafield: 'Purpose', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '支票编号', datafield: 'CheckNo', width: 150, align: 'center', cellsAlign: 'center', },
                    { text: '备注', datafield: 'Remark', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '总经理', datafield: 'GeneralManager', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '财务经理', datafield: 'FinancialManager', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '部门负责人', datafield: 'DivisionDirector', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '出纳', datafield: 'Cashier', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'AccountModeCode', datafield: 'AccountModeCode', hidden: true },
                    { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                    { text: 'OrgId', datafield: 'OrgId', hidden: true },
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
    window.location.href = "/CapitalCenterManagement/LoanApplicationDetail/Index?VGUID=" + VGUID;
}