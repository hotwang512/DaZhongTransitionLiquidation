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
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#ApplyDate").val("");
            $("#BankName").val("");
        });
        $("#btnAdd").on("click", function () {
            window.location.href = "/CapitalCenterManagement/CashManagerDetail/Index";
        })
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
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'No', type: 'string' },
                    { name: 'AccountModeName', type: 'string' },
                    { name: 'CompanyName', type: 'string' },
                    { name: 'ApplyDate', type: 'date' },
                    { name: 'BankAccountName', type: 'string' },
                    { name: 'BankAccount', type: 'string' },
                    { name: 'BankName', type: 'string' },
                    { name: 'Money', type: 'number' },
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
                data: { "ApplyDate": $("#ApplyDate").val(), "BankAccount": $("#BankName").val(), "Status": status },
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
                    { text: "", datafield: "checkbox", pinned: true, width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '流水号', datafield: 'No', pinned: true, width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '账套', datafield: 'AccountModeName', pinned: true, width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '公司', datafield: 'CompanyName', pinned: true, width: 280, align: 'center', cellsAlign: 'center' },
                    { text: '申请日期', datafield: 'ApplyDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '户名', datafield: 'BankAccountName', width: 280, align: 'center', cellsAlign: 'center' },
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

function getBankInfo() {
    $.ajax({
        url: "/CapitalCenterManagement/BankFlowTemplate/GetBankInfo",
        async: false,
        data: {},
        type: "post",
        success: function (result) {
            uiEngineHelper.bindSelect('#BankName', result, "BankAccount", "BankName");
            $("#BankName").prepend("<option value=\"\" selected='true'></>");

        }
    });
}

//提交
function submit(selection) {
    $.ajax({
        url: "/CapitalCenterManagement/CashManager/UpdataCashManager",
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
        url: "/CapitalCenterManagement/CashManager/UpdataCashManager",
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
        url: "/CapitalCenterManagement/CashManager/UpdataCashManager",
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