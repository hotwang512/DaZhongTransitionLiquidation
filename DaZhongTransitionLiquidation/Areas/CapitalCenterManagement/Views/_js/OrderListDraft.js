//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var $page = function () {

    this.init = function () {
        addEvent();
    }
    var status = $.request.queryString().Status;
    //所有事件
    function addEvent() {

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
            $("#FillingDate").val("");
            //$("#TransactionDate").val("");
            //$("#TransactionDateEnd").val("");
            //$("#PaymentUnit").val("");
        });

        //新增
        $("#btnAdd").on("click", function () {
            window.location.href = "/CapitalCenterManagement/OrderListDraftDetail/Index";
            //window.open("/CapitalCenterManagement/OrderListDraftDetail/Index");
        });
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
                jqxNotification("请选择您要提交的数据！", null, "error");
            } else {
                WindowConfirmDialog(check, "您确定要提交选中的数据？", "确认框", "确定", "取消", selection);
            }
        });

    }; //addEvent end

    //删除
    function dele(selection) {
        $.ajax({
            url: "/CapitalCenterManagement/OrderListDraft/DeleteOrderListInfo",
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
            url: "/CapitalCenterManagement/OrderListDraft/UpdataOrderListInfo",
            data: { vguids: selection, status: "2" },
            //traditional: true,
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
    //审核
    function check(selection) {
        $.ajax({
            url: "/CapitalCenterManagement/OrderListDraft/UpdataOrderListInfo",
            data: { vguids: selection, status: "3" },
            //traditional: true,
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

    function initTable() {
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'BusinessType', type: 'string' },
                    { name: 'BusinessProject', type: 'string' },
                    { name: 'BusinessSubItem1', type: 'string' },
                    { name: 'BusinessSubItem2', type: 'string' },
                    { name: 'BusinessSubItem3', type: 'string' },
                    { name: 'FillingDate', type: 'date' },
                    { name: 'OrderTime', type: 'string' },
                    { name: 'VisitorsNumber', type: 'string' },
                    { name: 'EscortNumber', type: 'string' },
                    { name: 'NumberCount', type: 'string' },
                    { name: 'PaymentMethod', type: 'string' },
                    { name: 'PaymentCompany', type: 'string' },
                    { name: 'Money', type: 'number' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "status": status, "FillingDate": $("#FillingDate").val() },
                url: "/CapitalCenterManagement/OrderListDraft/GetOrderListDatas"   //获取数据源的路径
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
                    { text: 'CompanySection', datafield: 'CompanySection', hidden: true },
                    { text: '收款人/部门/单位', datafield: 'PaymentCompany', width: 350, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '订单日期', datafield: 'FillingDate', width: 350, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd", },
                    { text: '支付方式', datafield: 'PaymentMethod', width: 350, align: 'center', cellsAlign: 'center' },
                    { text: '金额', datafield: 'Money', align: 'center', cellsAlign: 'center' },
                    { text: '订单时间', datafield: 'OrderTime', width: 200, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '来客人数', datafield: 'VisitorsNumber', width: 200, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '陪同人数', datafield: 'EscortNumber', width: 200, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '人数合计', datafield: 'NumberCount', width: 200, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '状态', datafield: 'Status', align: 'center', cellsAlign: 'center', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });
        selector.$grid().on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row index.
            window.location.href ="/CapitalCenterManagement/OrderListDraftDetail/Index?VGUID=" + row.VGUID;
        });
    }

    function detailFunc(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "1") {
            container = "<a href='#' onclick=link('" + rowData.VGUID + "') style=\"text-decoration: underline;color: #333;\">" + rowData.PaymentCompany + "</a>";
        } else {
            container = "<span>" + rowData.PaymentCompany + "</span>";
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

function link(VGUID) {
    window.location.href = "/CapitalCenterManagement/OrderListDraftDetail/Index?VGUID=" + VGUID;
}