﻿//资产基础信息维护列表
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
    //所有事件
    function addEvent() {
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#OrderType").val("");
            $("#SubmitStatus").val("");
        });
        //新增
        $("#btnAdd").on("click", function () {
            window.location.href = "/AssetManagement/FixedAssetsOrderDetail/Index";
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
    }; //addEvent end

    //删除
    function dele(selection) {
        $.ajax({
            url: "/AssetManagement/FixedAssetsOrder/DeleteFixedAssetsOrder",
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
            url: "/AssetManagement/FixedAssetsOrder/UpdataFixedAssetsOrder",
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

    function initTable() {
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'OrderType', type: 'string' },
                    { name: 'PaymentInformationVguid', type: 'string' },
                    { name: 'PaymentInformation', type: 'string' },
                    { name: 'OrderQuantity', type: 'number' },
                    { name: 'PurchasePrices', type: 'float' },
                    { name: 'ContractAmount', type: 'float' },
                    { name: 'AssetDescription', type: 'string' },
                    { name: 'UseDepartment', type: 'string' },
                    { name: 'SupplierInformation', type: 'string' },
                    { name: 'AcceptanceDate', type: 'date' },
                    { name: 'PaymentDate', type: 'date' },
                    { name: 'ContractName', type: 'string' },
                    { name: 'ContractFilePath', type: 'string' },
                    { name: 'SubmitStatus', type: 'string' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'ChangeDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'ChangeUser', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "OrderType": $("#OrderType").val(), "SubmitStatus": $("#SubmitStatus").val() },
                url: "/AssetManagement/FixedAssetsOrder/GetFixedAssetsOrderListDatas"   //获取数据源的路径
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
                    { text: '订单类型', datafield: 'OrderType', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '付款信息关联ID', datafield: 'PaymentInformationVguid', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '付款信息', datafield: 'PaymentInformation', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '订单数量', datafield: 'OrderQuantity', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购单价', datafield: 'PurchasePrices', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '合同金额', datafield: 'ContractAmount', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产说明', datafield: 'AssetDescription', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '使用部门', datafield: 'UseDepartment', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '供应商信息维护', datafield: 'SupplierInformation', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '预计验收日期', datafield: 'AcceptanceDate', width: 150, align: 'center', cellsAlign: 'center' ,cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '预计付款日期', datafield: 'PaymentDate', width: 150, align: 'center', cellsAlign: 'center', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '采购合同', datafield: 'ContractName', width: 150, align: 'center', cellsAlign: 'center', hidden:true },
                    { text: '提交状态', datafield: 'SubmitStatus', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '创建时间', datafield: 'CreateDate', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CreateUser', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '修改时间', datafield: 'ChangeDate', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'ChangeUser', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
        selector.$grid().on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row index.
            window.location.href = "/AssetManagement/FixedAssetsOrderDetail/Index?VGUID=" + row.VGUID;
        });
    }

    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
    }
    function cellsrenderer(row, column, value, rowData) {
        return '<span style="margin: 4px; margin-top:8px;">' + value + '%</span>';
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
