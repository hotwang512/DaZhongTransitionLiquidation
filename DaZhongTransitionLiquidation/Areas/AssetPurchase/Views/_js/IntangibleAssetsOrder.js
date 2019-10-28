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
        initSelectPurchaseGoods();
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
        });
        //新增
        $("#btnAdd").on("click", function () {
            window.location.href = "/AssetPurchase/IntangibleAssetsOrderDetail/Index";
        });
        //删除
        $("#btnDelete").on("click", function () {
            var selection = [];
            var grid = $("#jqxTable");
            var checedBoxs = grid.find("#tablejqxTable .jqx_datatable_checkbox:checked");
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
            if (selection.length != 1) {
                jqxNotification("请选择一条数据！", null, "error");
            }  else {
                WindowConfirmDialog(submit, "您确定要提交选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
        $("#CreditDialog_OKBtn").on("click",
            function () {
                $("#CreditDialog").modal("hide");
            }
        );
    }; //addEvent end
    function initSelectPurchaseGoods() {
        //使用部门
        $.ajax({
            url: "/AssetPurchase/FixedAssetsOrderDetail/GetPurchaseGoods",
            data: { "OrderCategory": 1 },//无形资产
            type: "POST",
            dataType: "json",
            async: false,
            success: function (msg) {
                uiEngineHelper.bindSelect('#PurchaseGoods', msg, "VGUID", "PurchaseGoods");
                $("#PurchaseGoods").prepend("<option value=\"\" selected='true'>请选择</>");
                debugger;
            }
        });
    }
    //删除
    function dele(selection) {
        $.ajax({
            url: "/AssetPurchase/IntangibleAssetsOrder/DeleteIntangibleAssetsOrder",
            data: { vguids: selection },
            traditional: true,
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                case "0":
                    jqxNotification("删除失败！", null, "error");
                    break;
                case "2":
                    jqxNotification(msg.ResultInfo, null, "error");
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
            url: "/AssetPurchase/IntangibleAssetsOrder/SubmitIntangibleAssetsOrder",
            data: { vguids: selection },
            //traditional: true,
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

    function initTable() {
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'OrderNumber', type: 'string' },
                    { name: 'PurchaseGoods', type: 'string' },
                    { name: 'PaymentInformationVguid', type: 'string' },
                    { name: 'PaymentInformation', type: 'string' },
                    { name: 'SumPayment', type: 'float' },
                    { name: 'FirstPayment', type: 'float' },
                    { name: 'InterimPayment', type: 'float' },
                    { name: 'TailPayment', type: 'float' },
                    { name: 'ContractName', type: 'string' },
                    { name: 'ContractFilePath', type: 'string' },
                    { name: 'PayType', type: 'string' },
                    { name: 'PayCompany', type: 'string' },
                    { name: 'BankStatus', type: 'string' },
                    { name: 'OSNO', type: 'string' },
                    { name: 'PaymentVoucherVguid', type: 'string' },
                    { name: 'SubmitStatus', type: 'string' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'ChangeDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'ChangeUser', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "PurchaseGoodsVguid": $("#PurchaseGoods").val(), "SubmitStatus": $("#SubmitStatus").val(), "OSNO": $("#OSNO").val() },
                url: "/AssetPurchase/IntangibleAssetsOrder/GetIntangibleAssetsOrderListDatas"   //获取数据源的路径
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
                    { text: "", datafield: "checkbox", hidden:true,width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '采购状态', datafield: 'SubmitStatus', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererSubmit },
                    { text: '采购编号', datafield: 'OrderNumber', width: 150, align: 'center', cellsAlign: 'center' ,cellsRenderer: detailFunc},
                    { text: '订单编号', datafield: 'OSNO', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购物品', datafield: 'PurchaseGoods', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '合同总价', datafield: 'SumPayment', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '合同首付款', datafield: 'FirstPayment', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '合同中期款', datafield: 'InterimPayment', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '合同尾款', datafield: 'TailPayment', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '供应商名称', datafield: 'PaymentInformation', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '付款方式', datafield: 'PayType', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '付款公司', datafield: 'PayCompany', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '采购合同', datafield: 'ContractName', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
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
            var PaymentVoucherVguid = row.PaymentVoucherVguid == null ? "" : row.PaymentVoucherVguid;
            window.location.href = "/AssetPurchase/IntangibleAssetsOrderDetail/Index?VGUID=" + row.VGUID + "&PaymentVoucherVguid=" + PaymentVoucherVguid;
        });
    }
    function detailFunc(row, column, value, rowData) {
        var container = "";
        rowData.PaymentVoucherVguid = rowData.PaymentVoucherVguid == null ? "" : rowData.PaymentVoucherVguid;
        if (selector.$EditPermission().val() == "1") {
            container = "<a href='#' onclick=link('" + rowData.VGUID + "','" + rowData.PaymentVoucherVguid + "') style=\"text-decoration: underline;color: #333;\">" + rowData.OrderNumber + "</a>";
        } else {
            container = "<span>" + rowData.OrderNumber + "</span>";
        }
        return container;
    }
    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
    }
    function cellsRendererSubmit(row, column, value, rowData) {
        if (value == 6) {
            return '<span style="margin: 4px; margin-top:8px;">支付成功</span>';
        } else if (value == 1) {
            return '<span style="margin: 4px; margin-top:8px;">首付款支付中</span>';
        } else if (value == 0) {
            return '<span style="margin: 4px; margin-top:8px;">首付款待发起支付</span>';
        } else if (value == 2) {
            return '<span style="margin: 4px; margin-top:8px;">中期款待发起支付</span>';
        } else if (value == 3) {
            return '<span style="margin: 4px; margin-top:8px;">中期款支付中</span>';
        } else if (value == 4) {
            return '<span style="margin: 4px; margin-top:8px;">尾款待发起支付</span>';
        } else if (value == 5) {
            return '<span style="margin: 4px; margin-top:8px;">尾款支付中</span>';
        } else if (value == 7) {
            return '<span style="margin: 4px; margin-top:8px;">支付失败-' + rowData.BankStatus + '</span>';
        } else if (value == 8) {
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

function link(VGUID, PaymentVoucherVguid) {
    debugger;
    PaymentVoucherVguid = PaymentVoucherVguid == null ? "" : PaymentVoucherVguid;
    window.location.href = "/AssetPurchase/IntangibleAssetsOrderDetail/Index?VGUID=" + VGUID + "&PaymentVoucherVguid=" + PaymentVoucherVguid;
}
$(function () {
    var page = new $page();
    page.init();
});
