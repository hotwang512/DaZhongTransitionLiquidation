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
    var status = $.request.queryString().Status;
    function addEvent() {       
        if (status == "1") {
            $("#buttonList").show();
        }
        if (status == "2") {
            $("#buttonList2").show();
        }
        getCompanyInfo();
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
            window.location.href = "/CapitalCenterManagement/ComeOnApplicationDetail/Index";
        })
        //删除
        $("#btnDelete").on("click", function () {
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
                jqxNotification("请选择您要删除的数据！", null, "error");
            } else {
                WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消", selection);
            }
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
    }

    function initTable() {
        var accountOut = $('#TurnOutCompany').val();
        var accountIn = $('#TurnInCompany').val();
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'No', type: 'string' },
                    { name: 'TurnInAccountModeName', type: 'string' },
                    { name: 'TurnOutAccountModeName', type: 'string' },
                    { name: 'ApplyDate', type: 'date' },
                    { name: 'TurnInCompanyName', type: 'string' },
                    { name: 'TurnOutCompanyName', type: 'string' },
                    { name: 'Money', type: 'string' },
                    { name: 'TurnInMoney', type: 'string' },
                    { name: 'CheckNo', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Cashier', type: 'string' },
                    { name: 'Auditor', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'TurnInAccountModeCode', type: 'string' },
                    { name: 'TurnOutAccountModeCode', type: 'string' },
                    { name: 'TurnInCompanyCode', type: 'string' },
                    { name: 'TurnOutCompanyCode', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: {
                    "ApplyDate": $("#ApplyDate").val(), "TurnInCompany": $("#TurnInCompany").val(),
                    "TurnOutCompany": $("#TurnOutCompany").val(), "Status": status
                },
                url: "/CapitalCenterManagement/ComeOnApplication/GetComeOnApplicationData"   //获取数据源的路径
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
                    { text: '申请日期', datafield: 'ApplyDate', pinned: true, width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    //{ text: '调出账套', datafield: 'TurnOutAccountModeName', hidden: true, width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '调出公司', datafield: 'TurnOutCompanyName', width: 400, align: 'center', cellsAlign: 'center' },
                    //{ text: '调入账套', datafield: 'TurnInAccountModeName',hidden:true, width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '调入公司', datafield: 'TurnInCompanyName', width: 400, align: 'center', cellsAlign: 'center' },
                    { text: '调出金额', datafield: 'Money', cellsFormat: "d2", width: 100, align: 'center', cellsAlign: 'center', },
                    { text: '调入金额', datafield: 'TurnInMoney', cellsFormat: "d2", width: 100, align: 'center', cellsAlign: 'center', },
                    { text: '用途', datafield: 'Remark', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '付款单位总经理', datafield: 'Auditor', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '用款单位出纳', datafield: 'Cashier', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'TurnInAccountModeCode', datafield: 'TurnInAccountModeCode', hidden: true },
                    { text: 'TurnInCompanyCode', datafield: 'TurnInCompanyCode', hidden: true },
                    { text: 'TurnOutAccountModeCode', datafield: 'TurnOutAccountModeCode', hidden: true },
                    { text: 'TurnOutCompanyCode', datafield: 'TurnOutCompanyCode', hidden: true },
                    { text: 'Status', datafield: 'Status', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });
    }
    function detailFunc(row, column, value, rowData) {
        var container = "<a href='#' onclick=link('" + rowData.VGUID + "') style=\"text-decoration: underline;color: #333;\">" + rowData.No + "</a>";
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

function link(VGUID) {
    window.location.href = "/CapitalCenterManagement/ComeOnApplicationDetail/Index?VGUID=" + VGUID;
}

function getCompanyInfo() {
    $.ajax({
        url: "/CapitalCenterManagement/ComeOnApplication/GetCompanyInfo",
        async: false,
        data: { },
        type: "post",
        success: function (result) {
            uiEngineHelper.bindSelect('#TurnOutCompany', result, "Code", "Descrption");
            $("#TurnOutCompany").prepend("<option value=\"\" selected='true'></>");
            uiEngineHelper.bindSelect('#TurnInCompany', result, "Code", "Descrption");
            $("#TurnInCompany").prepend("<option value=\"\" selected='true'></>");
        }
    });
}

//删除
function dele(selection) {
    $.ajax({
        url: "/CapitalCenterManagement/ComeOnApplication/DeleteComeOnApplication",
        data: { vguids: selection },
        traditional: true,
        type: "post",
        success: function (msg) {
            switch (msg.Status) {
                case "0":
                    jqxNotification("删除失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("删除成功！", null, "success");
                    $("#jqxTable").jqxDataTable('updateBoundData');
                    break;
            }
        }
    });
}
//提交
function submit(selection) {
    $.ajax({
        url: "/CapitalCenterManagement/ComeOnApplication/UpdataComeOnApplication",
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
        url: "/CapitalCenterManagement/ComeOnApplication/UpdataComeOnApplication",
        data: { vguids: selection, status: "3" },
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
            }
        }
    });
}