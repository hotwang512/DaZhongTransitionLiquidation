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
        getBankInfo();
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#TradingBank").val("");
            $("#TransactionDate").val("");
            $("#TransactionDateEnd").val("");
            $("#PaymentUnit").val("");
        });
        $("#btnAdd").on("click", function () {
            window.location.href = "/CapitalCenterManagement/CashTransactionDetail/Index";
        });
    }
    function initTable() {
        var DateEnd = $("#TransactionDateEnd").val();
        var source =
            {
                datafields:
                [
                    //{ name: "checkbox", type: null },
                    { name: 'AccountModeName', type: 'string' },
                    { name: 'AccountModeCode', type: 'string' },
                    { name: 'CompanyName', type: 'string' },
                    { name: 'CompanyCode', type: 'string' },
                    { name: 'TransactionDate', type: 'date' },   
                    { name: 'UseBalance', type: 'number' },
                    { name: 'TurnOut', type: 'number' },
                    { name: 'Balance', type: 'number' },
                    { name: 'ReimbursementOrgName', type: 'string' },
                    { name: 'ReimbursementOrgCode', type: 'string' },
                    { name: 'ReimbursementMan', type: 'string' },
                    { name: 'Purpose', type: 'string' },
                    { name: 'VoucherSubject', type: 'string' },
                    { name: 'VoucherSubjectName', type: 'string' },
                    { name: 'VoucherSummary', type: 'string' },
                    { name: 'CreateTime', type: 'date' },
                    { name: 'CreatePerson', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "ReimbursementMan": $("#ReimbursementMan").val(), "TransactionDate": $("#TransactionDate").val(), "CompanyName": $("#CompanyName").val() },
                url: "/CapitalCenterManagement/CashTransaction/GetCashTransaction?TransactionDateEnd=" + DateEnd   //获取数据源的路径
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
                columnsHeight: 35,
                columns: [
                    //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '交易流水号', datafield: 'Batch', pinned: true, width: 200, align: 'center', cellsAlign: 'center', },
                    { text: '账套', datafield: 'AccountModeName', pinned: true, width: 200, align: 'center', cellsAlign: 'center', },
                    { text: '公司', datafield: 'CompanyName', pinned: true, width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '支付日期', datafield: 'TransactionDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '支付金额', datafield: 'TurnOut', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '现金余额', datafield: 'Balance', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '报销部门', datafield: 'ReimbursementOrgName', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '报销人员', datafield: 'ReimbursementMan', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '用途', datafield: 'Purpose', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '写入日期', datafield: 'CreateTime', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '写入人', datafield: 'CreatePerson', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '凭证科目Code', hidden: true, datafield: 'VoucherSubject', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '凭证科目', hidden: true, datafield: 'VoucherSubjectName', width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '凭证摘要', hidden: true, datafield: 'VoucherSummary', width: 300, align: 'center', cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }
};

$(function () {
    var page = new $page();
    page.init();
});

function getBankInfo() {
    $.ajax({
        url: "/CapitalCenterManagement/BankFlowTemplate/GetBankInfo",
        async: false,
        data: {},
        type: "post",
        success: function (result) {
            uiEngineHelper.bindSelect('#TradingBank', result, "BankAccount", "BankName");
            $("#TradingBank").prepend("<option value=\"\" selected='true'></>");

        }
    });
}