var status = $.request.queryString().Status;
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
        if (status == "1") {
            $("#btnUp").show();
            $("#btnAddTd").show();
        }
        if (status == "2") {
            $("#btnCheck").show();
            $("#btnGoBack").show();
        } 
        if (status == "3") {
            $("#cashTable").hide();
            $("#jqxTable").jqxDataTable({ height: "480px" });
        }
        $('#jqxTable').on('bindingComplete', function (event) {
            if (status == "3") {
                $("#jqxTable").jqxDataTable('setColumnProperty', 'CreatePerson', 'text', '审核人');
            }
        });
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
        //新增
        $("#btnAdd").on("click", function () {
            window.location.href = "/CapitalCenterManagement/CashTransactionDetail/Index";
        });
        //提交
        $("#btnUp").on("click", function () {
            var selection = [];
            var grid = $("#jqxTable");
            var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
            checedBoxs.each(function () {
                var th = $(this);
                if (th.is(":checked")) {
                    var index = th.attr("index");
                    var data = grid.jqxDataTable('getRows')[index];
                    selection.push(data.VGUID);
                }
            });
            if (selection.length < 1) {
                jqxNotification("请选择您要提交的数据！", null, "error");
            } else {
                WindowConfirmDialog(submit, "您确定要提交选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
        //审核
        $("#btnCheck").on("click", function () {
            var selection = [];
            var grid = $("#jqxTable");
            var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
            checedBoxs.each(function () {
                var th = $(this);
                if (th.is(":checked")) {
                    var index = th.attr("index");
                    var data = grid.jqxDataTable('getRows')[index];
                    selection.push(data.VGUID);
                }
            });
            if (selection.length < 1) {
                jqxNotification("请选择您要审核的数据！", null, "error");
            } else {
                WindowConfirmDialog(check, "您确定要审核选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
        //退回
        $("#btnGoBack").on("click", function () {
            var selection = [];
            var grid = $("#jqxTable");
            var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
            checedBoxs.each(function () {
                var th = $(this);
                if (th.is(":checked")) {
                    var index = th.attr("index");
                    var data = grid.jqxDataTable('getRows')[index];
                    selection.push(data.VGUID);
                }
            });
            if (selection.length < 1) {
                jqxNotification("请选择您要退回的数据！", null, "error");
            } else {
                WindowConfirmDialog(goBack, "您确定要退回选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
    }
    function initTable() {
        var DateEnd = $("#TransactionDateEnd").val();
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Batch', type: 'string' },
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
                    { name: 'Status', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "ReimbursementMan": $("#ReimbursementMan").val(), "TransactionDate": $("#TransactionDate").val(), "CompanyName": $("#CompanyName").val(), "Status": status },
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
                    { text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '交易流水号', datafield: 'Batch', pinned: true, width: 200, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '账套', datafield: 'AccountModeName', pinned: true, width: 200, align: 'center', cellsAlign: 'center', },
                    { text: '公司', datafield: 'CompanyName', pinned: true, width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '支付日期', datafield: 'TransactionDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '可用金额', datafield: 'UseBalance', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
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
                    { text: 'Status', datafield: 'Status', hidden: true },
                ]
            });
        function detailFunc(row, column, value, rowData) {
            var container = "<a href='#' onclick=link('" + rowData.VGUID + "','" + rowData.Status + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Batch + "</a>";
            return container;
        }
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
function link(VGUID,status) {
    window.location.href = "/CapitalCenterManagement/CashTransactionDetail/Index?VGUID=" + VGUID + "&Status=" + status;
}
//提交
function submit(selection) {
    $.ajax({
        url: "/CapitalCenterManagement/CashTransaction/UpdataCashTransaction",
        data: { vguids: selection, status: "2" },
        traditional: true,
        type: "post",
        success: function (msg) {
            switch (msg.Status) {
                case "0":
                    jqxNotification("提交失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("提交成功！", null, "success");
                    $("#jqxTable").jqxDataTable('updateBoundData');
                    break;
                case "2":
                    jqxNotification(msg.ResultInfo + "条凭证借贷不相平,提交失败！", null, "error");
                    break;
            }
        }
    });
}
//审核
function check(selection) {
    $.ajax({
        url: "/CapitalCenterManagement/CashTransaction/UpdataCashTransaction",
        data: { vguids: selection, status: "3" },
        traditional: true,
        type: "post",
        success: function (msg) {
            switch (msg.Status) {
                case "0":
                    jqxNotification("审核失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("审核成功！", null, "success");
                    $("#jqxTable").jqxDataTable('updateBoundData');
                    break;
            }
        }
    });
}
//退回
function goBack(selection) {
    $.ajax({
        url: "/CapitalCenterManagement/CashTransaction/UpdataCashTransaction",
        data: { vguids: selection, status: "1" },
        //traditional: true,
        type: "post",
        success: function (msg) {
            switch (msg.Status) {
                case "0":
                    jqxNotification("退回失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("退回成功！", null, "success");
                    $("#jqxTable").jqxDataTable('updateBoundData');
                    break;
            }
        }
    });
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
    var grid = $("#jqxTable");
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