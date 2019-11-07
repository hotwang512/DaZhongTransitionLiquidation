//资产基础信息维护列表
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
        initSelectVehicleModel();
        initSelectPayItem();
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
            $("#OSNO").val("");
            $("#SubmitStatus").val("-1");
            $("#PayItem").val("-1");
        });
        //新增
        $("#btnAdd").on("click", function () {
            window.location.href = "/AssetPurchase/TaxFeeOrderDetail/Index";
        });
        //删除
        $("#btnDelete").on("click", function () {
            debugger;
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
                jqxNotification("请选择您要作废的数据！", null, "error");
            } else {
                WindowConfirmDialog(dele, "您确定要作废选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
        $("#CreditDialog_OKBtn").on("click",
            function () {
                $("#CreditDialog").modal("hide");
            }
        );
        //填写信息后提交，调用清算平台、待付款请求生成支付凭证接口
        //先调用接口，成功后再提交
        $("#btnSubmit").on("click", function () {
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
                jqxNotification("请选择数据！", null, "error");
            } else {
                layer.load();
                $.ajax({
                    url: "/AssetPurchase/TaxFeeOrder/CompareTaxFeeOrder",
                    data: { vguids: selection },
                    type: "post",
                    success: function (msg) {
                        layer.closeAll('loading');
                        switch (msg.Status) {
                        case "0":
                            jqxNotification(msg.ResultInfo, null, "error");
                            break;
                        case "1":
                            SubmitTaxFeeOrder(selection);
                            break;
                        }
                    }
                });
            }
        });
    }; //addEvent end
    function SubmitTaxFeeOrder(selection) {
        $.ajax({
            url: "/AssetPurchase/TaxFeeOrder/SubmitTaxFeeOrder",
            data: { vguids: selection },
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                case "0":
                    jqxNotification("提交失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("提交成功！", null, "success");
                    document.getElementById('ifrPrint').src = msg.ResultInfo;
                    $("#CreditDialog").modal("show");
                    $("#jqxTable").jqxDataTable('updateBoundData');
                    break;
                case "2":
                    jqxNotification(msg.ResultInfo, null, "error");
                    break;
                }
            }
        });
    }
    //删除
    function dele(selection) {
        $.ajax({
            url: "/AssetPurchase/TaxFeeOrder/DeleteTaxFeeOrder",
            data: { vguids: selection },
            traditional: true,
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                    case "0":
                        jqxNotification("作废失败！", null, "error");
                        break;
                    case "2":
                        jqxNotification(msg.ResultInfo, null, "error");
                        break;
                    case "1":
                        jqxNotification("作废成功！", null, "success");
                        $("#jqxTable").jqxDataTable('updateBoundData');
                        break;
                }
            }
        });
    }
    //提交
    function submit(selection) {
        $.ajax({
            url: "/AssetPurchase/TaxFeeOrder/SubmitFixedAssetsOrder",
            data: { vguids: selection },
            //traditional: true,
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                    case "0":
                        jqxNotification("发起失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("发起成功！", null, "success");
                        $("#jqxTable").jqxDataTable('updateBoundData');
                        break;
                }
            }
        });
    }
    function initSelectVehicleModel() {
        $.ajax({
            url: "/Systemmanagement/VehicleExtrasFeeSettingDetail/GetVehicleModelDropDown",
            type: "GET",
            dataType: "json",
            async: false,
            success: function (msg) {
                debugger;
                uiEngineHelper.bindSelect('#VehicleModel', msg, "Code", "Descrption");
                $("#VehicleModel").prepend("<option value=\"\" selected='true'>请选择</>");
                debugger;
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
                    { name: 'OrderNumber', type: 'string' },
                    { name: 'PayItem', type: 'string' },
                    { name: 'VehicleModel', type: 'string' },
                    { name: 'PaymentInformation', type: 'string' },
                    { name: 'OrderQuantity', type: 'number' },
                    { name: 'UnitPrice', type: 'float' },
                    { name: 'SumPayment', type: 'float' },
                    { name: 'PurchaseDescription', type: 'string' },
                    { name: 'PaymentDate', type: 'date' },
                    { name: 'BankStatus', type: 'string' },
                    { name: 'ContractName', type: 'string' },
                    { name: 'ContractFilePath', type: 'string' },
                    { name: 'PayType', type: 'string' },
                    { name: 'PayCompany', type: 'string' },
                    { name: 'OSNO', type: 'string' },
                    { name: 'SubmitStatus', type: 'number' },
                    { name: 'PaymentVoucherVguid', type: 'string' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'ChangeDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'ChangeUser', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "VehicleModelCode": $("#VehicleModel").val(), "SubmitStatus": $("#SubmitStatus").val(), "OrderNumber": $("#OrderNumber").val(), "PayItemCode": $("#PayItem").val() },
                url: "/AssetPurchase/TaxFeeOrder/GetOrderListDatas"   //获取数据源的路径
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
                    { text: '采购状态', datafield: 'SubmitStatus', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererSubmit },
                    { text: '采购编号', datafield: 'OrderNumber', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '订单编号', datafield: 'OSNO', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '付款项目', datafield: 'PayItem', width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '车型', datafield: 'VehicleModel', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '供应商名称', datafield: 'PaymentInformation', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '数量', datafield: 'OrderQuantity', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '单价', datafield: 'UnitPrice', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '合同金额', datafield: 'SumPayment', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购说明', datafield: 'PurchaseDescription', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '付款方式', datafield: 'PayType', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '付款公司', datafield: 'PayCompany', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '创建时间', datafield: 'CreateDate', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CreateUser', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '修改时间', datafield: 'ChangeDate', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'ChangeUser', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '付款项目', datafield: 'PaymentVoucherVguid', width: 300, align: 'center',hidden:true, cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
        selector.$grid().on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            var PaymentVoucherVguid = row.PaymentVoucherVguid == null ? "" :  row.PaymentVoucherVguid;
            // row index.
            window.location.href = "/AssetPurchase/TaxFeeOrderDetail/Index?VGUID=" + row.VGUID + "&PaymentVoucherVguid=" + PaymentVoucherVguid;
        });
    }
    function detailFunc(row, column, value, rowData) {
        var container = "";
        rowData.PaymentVoucherVguid = rowData.PaymentVoucherVguid == null ? "" : rowData.PaymentVoucherVguid;
        container = "<a href='#' onclick=link('" + rowData.VGUID + "','" + rowData.PaymentVoucherVguid + "') style=\"text-decoration: underline;color: #333;\">" + rowData.OrderNumber + "</a>";
        return container;
    }
    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
    }
    function cellsRendererSubmit(row, column, value, rowData) {
        if (value == 1) {
            return '<span style="margin: 4px; margin-top:8px;">支付中</span>';
        } else if (value == 0) {
            return '<span style="margin: 4px; margin-top:8px;">待发起支付</span>';
        } else if (value == 2) {
            return '<span style="margin: 4px; margin-top:8px;">已支付</span>';
        } else if (value == 3) {
            return '<span style="margin: 4px; margin-top:8px;">支付失败-' + rowData.BankStatus + '</span>';
        } else if (value == 4) {
            return '<span style="margin: 4px; margin-top:8px;">作废</span>';
        }
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
function initSelectPayItem() {
    $.ajax({
        url: "/AssetPurchase/TaxFeeOrderDetail/GetPayItem",
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#PayItem', msg, "BusinessSubItem1", "BusinessProject");
            $("#PayItem").prepend("<option value=\"-1\" selected='true'>请选择</>");
        }
    });
}
function link(VGUID, PaymentVoucherVguid) {
    debugger;
    PaymentVoucherVguid = PaymentVoucherVguid == null ? "" : PaymentVoucherVguid;
    window.location.href = "/AssetPurchase/TaxFeeOrderDetail/Index?VGUID=" + VGUID + "&PaymentVoucherVguid=" + PaymentVoucherVguid;
}
$(function () {
    var page = new $page();
    page.init();
});
