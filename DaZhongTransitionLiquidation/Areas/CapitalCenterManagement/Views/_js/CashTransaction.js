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
    }
    function initTable() {
        var DateEnd = $("#TransactionDateEnd").val();
        var source =
            {
                datafields:
                [
                    //{ name: "checkbox", type: null },
                    { name: 'AccountModeName', type: 'string' },
                    { name: 'TradingBank', type: 'string' },
                    { name: 'TransactionDate', type: 'date' },
                    { name: 'TurnOut', type: 'number' },
                    { name: 'TurnIn', type: 'number' },
                    { name: 'Currency', type: 'string' },
                    { name: 'PaymentUnit', type: 'string' },
                    { name: 'PayeeAccount', type: 'string' },
                    { name: 'PaymentUnitInstitution', type: 'string' },
                    { name: 'ReceivingUnit', type: 'string' },
                    { name: 'ReceivableAccount', type: 'string' },
                    { name: 'ReceivingUnitInstitution', type: 'string' },
                    { name: 'Purpose', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Batch', type: 'string' },
                    { name: 'VoucherSubject', type: 'string' },
                    { name: 'VoucherSubjectName', type: 'string' },
                    { name: 'VoucherSummary', type: 'string' },
                    { name: 'Balance', type: 'string' },
                    { name: 'CreateTime', type: 'date' },
                    { name: 'CreatePerson', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "TradingBank": $("#TradingBank").val(), "TransactionDate": $("#TransactionDate").val(), "PaymentUnit": $("#PaymentUnit").val() },
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
                    { text: '账套', datafield: 'AccountModeName', pinned: true, width: 200, align: 'center', cellsAlign: 'center', },
                    { text: '公司', datafield: 'PaymentUnit', pinned: true, width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '币种', datafield: 'Currency', hidden: true, align: 'center', cellsAlign: 'center' },
                    { text: '我方银行', datafield: 'TradingBank', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '我方账号', datafield: 'PayeeAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '我方开户机构', datafield: 'PaymentUnitInstitution', width: 350, align: 'center', cellsAlign: 'center' },
                    { text: '交易流水号', datafield: 'Batch', width: 200, align: 'center', cellsAlign: 'center', },
                    { text: '交易日期', datafield: 'TransactionDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '转入(贷)', datafield: 'TurnOut', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '转出(借)', datafield: 'TurnIn', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '余额', datafield: 'Balance', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '对方单位名称', datafield: 'ReceivingUnit', width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '对方账号', datafield: 'ReceivableAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '对方开户机构', datafield: 'ReceivingUnitInstitution', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '用途', datafield: 'Purpose', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '备注', datafield: 'Remark', width: 200, align: 'center', cellsAlign: 'center' },
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