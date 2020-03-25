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
        getBankInfo();
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
        $("#btnAdd").on("click", function () {
            window.location.href = "/CapitalCenterManagement/CapitalAllocationDetail/Index";
        })
    }

    function initTable() {
        var source =
            {
                datafields:
                [
                    //{ name: "checkbox", type: null },
                    { name: 'No', type: 'string' },
                    { name: 'TurnInAccountModeName', type: 'string' },
                    { name: 'TurnOutAccountModeName', type: 'string' },
                    { name: 'ApplyDate', type: 'date' },
                    { name: 'TurnInCompanyName', type: 'string' },
                    { name: 'TurnInBankAccount', type: 'string' },
                    { name: 'TurnInBankName', type: 'string' },
                    { name: 'TurnOutCompanyName', type: 'string' },
                    { name: 'TurnOutBankAccount', type: 'string' },
                    { name: 'TurnOutBankName', type: 'string' },
                    { name: 'Money', type: 'string' },
                    { name: 'CheckNo', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Cashier', type: 'string' },
                    { name: 'Auditor', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'TurnInAccountModeCode', type: 'string' },
                    { name: 'TurnOutAccountModeCode', type: 'string' },
                    { name: 'TurnInCompanyCode', type: 'string' },
                    { name: 'TurnOutCompanyCode', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "ApplyDate": $("#ApplyDate").val(), "TurnInBankAccount": $("#TurnInBankName").val(), "TurnOutBankAccount": $("#TurnOutBankName").val() },
                url: "/CapitalCenterManagement/CapitalAllocation/GetCapitalAllocationData"   //获取数据源的路径
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
                    { text: '申请日期', datafield: 'ApplyDate', pinned: true, width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    //{ text: '调入账套', datafield: 'TurnInAccountModeName',hidden:true, width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '调入单位', datafield: 'TurnInCompanyName', width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '调入账号', datafield: 'TurnInBankAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '调入开户行', datafield: 'TurnInBankName', width: 200, align: 'center', cellsAlign: 'center' },
                    //{ text: '调出账套', datafield: 'TurnOutAccountModeName', hidden: true, width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '调出单位', datafield: 'TurnOutCompanyName', width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '调出账号', datafield: 'TurnOutBankAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '调出开户行', datafield: 'TurnOutBankName', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '申请金额', datafield: 'Money', cellsFormat: "d2", width: 100, align: 'center', cellsAlign: 'center', },
                    { text: '用途', datafield: 'Remark', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '付款单位总经理', datafield: 'Auditor', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '用款单位出纳', datafield: 'Cashier', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'TurnInAccountModeCode', datafield: 'TurnInAccountModeCode', hidden: true },
                    { text: 'TurnInCompanyCode', datafield: 'TurnInCompanyCode', hidden: true },
                    { text: 'TurnOutAccountModeCode', datafield: 'TurnOutAccountModeCode', hidden: true },
                    { text: 'TurnOutCompanyCode', datafield: 'TurnOutCompanyCode', hidden: true },
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
    window.location.href = "/CapitalCenterManagement/CapitalAllocationDetail/Index?VGUID=" + VGUID;
}

function getBankInfo() {
    $.ajax({
        url: "/CapitalCenterManagement/CapitalAllocation/GetBankInfo",
        async: false,
        data: {},
        type: "post",
        success: function (result) {
            uiEngineHelper.bindSelect('#TurnInBankName', result, "BankAccount", "BankName");
            $("#TurnInBankName").prepend("<option value=\"\" selected='true'></>");
            uiEngineHelper.bindSelect('#TurnOutBankName', result, "BankAccount", "BankName");
            $("#TurnOutBankName").prepend("<option value=\"\" selected='true'></>");
        }
    });
}