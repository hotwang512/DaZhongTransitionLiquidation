//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnSubmit: function () { return $("#btnSubmit") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var $page = function () {
    this.init = function () {
        initSelectPurchaseGoods();
        initSelectCompany();
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
            $("#PurchaseGoods").val("");
            $("#SubmitStatus").val("");
        });
        selector.$btnSubmit().on("click", function () {
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
            if (selection.length != 1) {
                jqxNotification("请选择一条数据！", null, "error");
            } else {
                $.ajax({
                    url: "/AssetPurchase/LiquidationReview/SubmitLiquidation",
                    data: { FundClearingVguid: selection[0] },
                    type: "post",
                    success: function(msg) {
                        switch (msg.Status) {
                        case "0":
                            jqxNotification("提交失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("提交成功！", null, "success");
                            $("#jqxTable").jqxDataTable('updateBoundData');
                            break;
                        case "2":
                            jqxNotification(msg.ResultInfo, null, "error");
                            break;
                        }
                    }
                });
            }
        });
    }; //addEvent end
    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'FixedAssetsOrderVguid', type: 'string' },
                    { name: 'PurchaseGoods', type: 'string' },
                    { name: 'OrderQuantity', type: 'number' },
                    { name: 'PurchasePrices', type: 'float' },
                    { name: 'ContractAmount', type: 'float' },
                    { name: 'AssetDescription', type: 'string' },
                    { name: 'SubmitStatus', type: 'number' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'ChangeDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'ChangeUser', type: 'string' }
                ],
                datatype: "json",
                id: "FixedAssetsOrderVguid",
                data: { "PurchaseGoodsVguid": $("#PurchaseGoods").val(), "SubmitStatus": $("#SubmitStatus").val() },
                url: "/AssetPurchase/LiquidationReview/GetListDatas"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        var nestedTables = new Array();
        var initRowDetails = function (id, row, element, rowinfo) {
            debugger;
            element.append($("<div style='margin: 10px;'></div>"));
            var nestedDataTable = $(element.children()[0]);
            var ordersSource = {
                dataFields: [
                    { name: 'VGUID', type: 'string' },
                    { name: 'PurchaseGoods', type: 'string' },
                    { name: 'PaymentInformationVguid', type: 'string' },
                    { name: 'PaymentInformation', type: 'string' },
                    { name: 'OrderQuantity', type: 'number' },
                    { name: 'PurchasePrices', type: 'float' },
                    { name: 'ContractAmount', type: 'float' },
                    { name: 'AssetDescription', type: 'string' },
                    { name: 'BankStatus', type: 'string' },
                    { name: 'AcceptanceDate', type: 'date' },
                    { name: 'PaymentDate', type: 'date' },
                    { name: 'ContractName', type: 'string' },
                    { name: 'ContractFilePath', type: 'string' },
                    { name: 'PaymentVoucherVguid', type: 'string' },
                    { name: 'PayType', type: 'string' },
                    { name: 'PayCompany', type: 'string' },
                    { name: 'OSNO', type: 'string' },
                    { name: 'SubmitStatus', type: 'number' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'ChangeDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'ChangeUser', type: 'string' }
                ],
                datatype: "json",
                id: 'VGUID',
                data: { "FixedAssetsOrderVguid": id },
                url: "/AssetPurchase/LiquidationReview/GetFundClearingOrder"
            }
            if (nestedDataTable != null) {
                var nestedDataTableAdapter = new $.jqx.dataAdapter(ordersSource);
                nestedDataTable.jqxDataTable({
                    source: nestedDataTableAdapter,
                    width: '90%', height: 180,
                    pageable: false,
                    showToolbar: false,
                    columns: [
                        { text: '支付状态', datafield: 'SubmitStatus', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererPayStatus },
                        { text: '订单编号', datafield: 'OSNO', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '采购物品', datafield: 'PurchaseGoods', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '供应商名称', datafield: 'PaymentInformation', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '采购数量', datafield: 'OrderQuantity', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '采购单价', datafield: 'PurchasePrices', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '合同金额', datafield: 'ContractAmount', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '采购说明', datafield: 'AssetDescription', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '预计验收日期', datafield: 'AcceptanceDate', width: 150, align: 'center', cellsAlign: 'center', cellsformat: "yyyy-MM-dd HH:mm:ss", hidden: true },
                        { text: '预计付款日期', datafield: 'PaymentDate', width: 150, align: 'center', cellsAlign: 'center', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                        { text: '采购合同', datafield: 'ContractName', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                        { text: '付款方式', datafield: 'PayType', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '付款公司', datafield: 'PayCompany', width: 320, align: 'center', cellsAlign: 'center' },
                        { text: '创建时间', datafield: 'CreateDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                        { text: '创建人', datafield: 'CreateUser', width: 100, align: 'center', cellsAlign: 'center' },
                        { text: '修改时间', datafield: 'ChangeDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                        { text: '修改人', datafield: 'ChangeUser', width: 100, align: 'center', cellsAlign: 'center' },
                        { text: 'VGUID', datafield: 'VGUID', hidden: true }
                    ]
                });
                nestedTables[id] = nestedDataTable;
                $(nestedDataTable).on('rowDoubleClick', function (event) {
                    var args = event.args;
                    var row = args.row;
                    var PaymentVoucherVguid = row.PaymentVoucherVguid == null ? "" : row.PaymentVoucherVguid;
                    window.location.href = "/AssetPurchase/FundClearingOrderDetail/Index?VGUID=" + row.VGUID + "&PaymentVoucherVguid=" + PaymentVoucherVguid;
                });
            }
        }
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
                rowDetails: true,
                ready: function () {
                    selector.$grid().jqxDataTable('showDetails', 0);
                },
                initRowDetails: initRowDetails,
                columns: [
                    { text: "", datafield: "checkbox", width: 35, pinned: true,align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '提交状态', datafield: 'SubmitStatus', hidden: true, width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererSubmit },
                    { text: 'FixedAssetsOrderVguid', datafield: 'FixedAssetsOrderVguid', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '采购物品', datafield: 'PurchaseGoods', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购数量', datafield: 'OrderQuantity', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购单价', datafield: 'PurchasePrices', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '合同金额', datafield: 'ContractAmount', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购说明', datafield: 'AssetDescription', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '创建时间', datafield: 'CreateDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CreateUser', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '修改时间', datafield: 'ChangeDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
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
    function cellsRendererSubmit(row, column, value, rowData) {
        if (value == 1) {
            return '<span style="margin: 4px; margin-top:8px;">已提交</span>';
        } else if (value == 0) {
            return '<span style="margin: 4px; margin-top:8px;">待提交</span>';
        }
    }
    function cellsRendererPayStatus(row, column, value, rowData) {
        if (value == 2) {
            return '<span style="margin: 4px; margin-top:8px;">支付成功</span>';
        } else if (value == 0) {
            return '<span style="margin: 4px; margin-top:8px;">待发起支付</span>';
        } else if (value == 1) {
            return '<span style="margin: 4px; margin-top:8px;">支付中</span>';
        } else if (value == 3) {
            return '<span style="margin: 4px; margin-top:8px;">支付失败-' + rowData.BankStatus + '</span>';
        }
    }
    function cellsRendererDel(row, column, value, rowData) {
        debugger;
        return '<div style="margin-top:6px;">' +
            '<a style="cursor:pointer"  onclick="DelAssign(\'' + rowData.VGUID +
            '\')">删除</a>' +
            '</div>';
    }
    function initSelectPurchaseGoods() {
        //使用部门
        $.ajax({
            url: "/AssetPurchase/FixedAssetsOrderDetail/GetPurchaseGoods",
            data: { "OrderCategory": 0 },
            type: "POST",
            dataType: "json",
            async: false,
            success: function (msg) {
                uiEngineHelper.bindSelect('#PurchaseGoods', msg, "VGUID", "PurchaseGoods");
                $("#PurchaseGoods").prepend("<option value=\"\" selected='true'>请选择</>");
            }
        });
    }
    function initSelectCompany() {
        //使用部门
        $.ajax({
            url: "/AssetPurchase/LiquidationReview/GetCompanyData",
            type: "POST",
            dataType: "json",
            async: false,
            success: function (msg) {
                uiEngineHelper.bindSelect('#CompanyName', msg, "VGUID", "Descrption");
                $("#Company").prepend("<option value=\"\" selected='true'>请选择</>");
            }
        });
    }
};
$(function () {
    var page = new $page();
    page.init();
});
