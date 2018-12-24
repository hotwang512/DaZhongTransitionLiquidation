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
            $("#BusinessType").val("");
            //$("#TransactionDate").val("");
            //$("#TransactionDateEnd").val("");
            //$("#PaymentUnit").val("");
        });

        //新增
        $("#btnAdd").on("click", function () {
            //window.location.open = "/VoucherManageManagement/VoucherListDetail/Index";
            window.open("/CapitalCenterManagement/OrderListDetail/Index");
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
            url: "/CapitalCenterManagement/OrderList/DeleteOrderListInfo",
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
            url: "/CapitalCenterManagement/OrderList/UpdataOrderListInfo",
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
            url: "/CapitalCenterManagement/OrderList/UpdataOrderListInfo",
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
                    { name: 'PaymentCompany', type: 'string' },
                    { name: 'CollectionCompany', type: 'string' },
                    { name: 'BusinessUnit', type: 'string' },

                    { name: 'Abstract', type: 'string' },
                    { name: 'Money', type: 'number' },
                    { name: 'CompanySection', type: 'string' },
                    { name: 'CompanyName', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },

                    { name: 'CollectionAccount', type: 'string' },
                    { name: 'CollectionBankAccount', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "status": status, "BusinessProject": $("#BusinessProject").val() },
                url: "/CapitalCenterManagement/OrderList/GetOrderListDatas"   //获取数据源的路径
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
                    { text: '业务类型', datafield: 'BusinessType', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '业务项目', datafield: 'BusinessProject', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '业务子项1', datafield: 'BusinessSubItem1', width: 200, align: 'center', cellsAlign: 'center', },
                    { text: '业务子项2', datafield: 'BusinessSubItem2', width: 200, align: 'center', cellsAlign: 'center', },
                    { text: '业务子项3', datafield: 'BusinessSubItem3', width: 200, align: 'center', cellsAlign: 'center', },
                    { text: '收款公司', datafield: 'CollectionCompany', width: 200, align: 'center', cellsAlign: 'center', },
                    { text: '收款账号', datafield: 'CollectionAccount', align: 'center', cellsAlign: 'center', },

                    { text: '摘要', datafield: 'Abstract', width: 200, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '金额', datafield: 'Money', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '公司', datafield: 'CompanyName', align: 'center', cellsAlign: 'center', hidden: true },
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
            window.open("/CapitalCenterManagement/OrderListDetail/Index?VGUID=" + row.VGUID);
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