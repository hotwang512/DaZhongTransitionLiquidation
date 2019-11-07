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
var orderType = "";
var $page = function () {
    this.init = function () {
        orderType = $.request.queryString().OrderType;
        if (orderType == "OBD") {
            $("#btnImport").parent().show();
        } else {
            $("#btnAdd").parent().show();
        }
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
            $("#SubmitStatus").val("-1");
            $("#OSNO").val("");
        });
        //新增
        $("#btnAdd").on("click", function () {
            window.location.href = "/AssetPurchase/FixedAssetsOrderDetail/Index?OrderType=" + orderType;
        });
        //导入
        $("#btnImport").on("click", function () {
            $("#LocalOBDFileInput").click();
        }); //OBD导入
        $("#LocalOBDFileInput").on("change",
            function () {
                debugger;
                layer.load();
                var vguid = newguid();
                var filePath = this.value;
                var fileExt = filePath.substring(filePath.lastIndexOf("."))
                    .toLowerCase();
                if (!checkFileExt(fileExt)) {
                    jqxNotification("您上传的文件类型不允许,请重新上传！！", null, "error");
                    this.value = "";
                    return;
                } else {
                    layer.load();
                    $("#localOBDFormFile").ajaxSubmit({
                        url: "/AssetPurchase/FixedAssetsOrder/ImportOBDAssignFile",
                        type: "post",
                        data: {
                            'vguid': vguid
                        },
                        success: function (msg) {
                            layer.closeAll('loading');
                            switch (msg.Status) {
                            case "0":
                                if (msg.ResultInfo != null || msg.ResultInfo2 != null) {
                                    jqxNotification((msg.ResultInfo == null ? "" : msg.ResultInfo) + " " + (msg.ResultInfo2 == null ? "" : msg.ResultInfo2), null, "error");
                                } else {
                                    jqxNotification("导入失败", null, "error");
                                }
                                $('#LocalOBDFileInput').val('');
                                break;
                            case "1":
                                jqxNotification("导入成功！", null, "success");
                                $('#LocalOBDFileInput').val('');
                                initTable();
                                break;
                            }
                        }
                    });
                }
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
        //填写信息后提交，调用清算平台、待付款请求生成支付凭证接口
        //先调用接口，成功后再提交
        $("#btnSubmit").on("click", function () {
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
            if (selection.length > 1) {
                jqxNotification("请选择一条数据！", null, "error");
            } else {
                $.ajax({
                    url: "/AssetPurchase/FixedAssetsOrder/SubmitFixedAssetsOrder",
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
        });
        $("#CreditDialog_OKBtn").on("click",
            function () {
                $("#CreditDialog").modal("hide");
            }
        );
    }; //addEvent end

    //删除
    function dele(selection) {
        $.ajax({
            url: "/AssetPurchase/FixedAssetsOrder/DeleteFixedAssetsOrder",
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
            url: "/AssetPurchase/FixedAssetsOrder/SubmitFixedAssetsOrder",
            data: { vguids: selection},
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
    function initSelectPurchaseGoods() {
        //使用部门
        $.ajax({
            url: "/AssetPurchase/FixedAssetsOrderDetail/GetPurchaseGoods",
            data: { "OrderCategory": 0},//固定资产
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
                id: "VGUID",
                data: { "PurchaseGoodsVguid": $("#PurchaseGoods").val(), "SubmitStatus": $("#SubmitStatus").val(), "OrderNumber": $("#OrderNumber").val(), "OrderType": orderType },
                url: "/AssetPurchase/FixedAssetsOrder/GetFixedAssetsOrderListDatas"   //获取数据源的路径
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
                    { text: "", datafield: "checkbox", width: 35, pinned: true,hidden:true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '采购状态', datafield: 'SubmitStatus', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererSubmit },
                    { text: '采购编号', datafield: 'OrderNumber', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc},
                    { text: '订单编号', datafield: 'OSNO', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购物品', datafield: 'PurchaseGoods', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '供应商名称', datafield: 'PaymentInformation', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购数量', datafield: 'OrderQuantity', width: 80, align: 'center', cellsAlign: 'center' },
                    { text: '采购单价', datafield: 'PurchasePrices',hidden:true, width: 50, align: 'center', cellsAlign: 'center' },
                    { text: '合同金额', datafield: 'ContractAmount', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购说明', datafield: 'AssetDescription', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '预计验收日期', datafield: 'AcceptanceDate', width: 150, align: 'center', cellsAlign: 'center' ,cellsformat: "yyyy-MM-dd HH:mm:ss",hidden:true },
                    { text: '预计付款日期', datafield: 'PaymentDate',hidden:true, width: 150, align: 'center', cellsAlign: 'center', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '采购合同', datafield: 'ContractName', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '付款方式', datafield: 'PayType', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '付款公司', datafield: 'PayCompany', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '创建时间', datafield: 'CreateDate', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CreateUser', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '修改时间', datafield: 'ChangeDate', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'ChangeUser', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
        selector.$grid().on('rowDoubleClick', function (event) {
            var args = event.args;
            var row = args.row;
            var PaymentVoucherVguid = row.PaymentVoucherVguid == null ? "" : row.PaymentVoucherVguid;
            window.location.href = "/AssetPurchase/FixedAssetsOrderDetail/Index?OrderType=" + orderType + "&PaymentVoucherVguid=" + PaymentVoucherVguid + "&VGUID=" + row.VGUID;
        });
    }
    function detailFunc(row, column, value, rowData) {
        var container = "";
        rowData.PaymentVoucherVguid = rowData.PaymentVoucherVguid == null ? "" : rowData.PaymentVoucherVguid;
        debugger;
        container = "<a href='#' onclick=link('" + rowData.VGUID + "','" + rowData.PaymentVoucherVguid + "') style=\"text-decoration: underline;color: #333;\">" + rowData.OrderNumber + "</a>";
        return container;
    }
    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
    }
    function cellsRendererSubmit(row, column, value, rowData) {
        if (value == 2) {
            return '<span style="margin: 4px; margin-top:8px;">支付成功</span>';
        } else if (value == 0) {
            return '<span style="margin: 4px; margin-top:8px;">待发起支付</span>';
        } else if (value == 1) {
            return '<span style="margin: 4px; margin-top:8px;">支付中</span>';
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

function link(VGUID, PaymentVoucherVguid) {
    debugger;
    window.location.href = "/AssetPurchase/FixedAssetsOrderDetail/Index?OrderType=" + orderType + "&PaymentVoucherVguid=" + PaymentVoucherVguid + "&VGUID=" + VGUID;
}
$(function () {
    var page = new $page();
    page.init();
});
function checkFileExt(ext) {
    if (!ext.match(/.xls|.xlsx/i)) {
        return false;
    }
    return true;
}
function isNumber(val) {
    var regPos = /^\d+(\.\d+)?$/; //非负浮点数
    var regNeg = /^(-(([0-9]+\.[0-9]*[1-9][0-9]*)|([0-9]*[1-9][0-9]*\.[0-9]+)|([0-9]*[1-9][0-9]*)))$/; //负浮点数
    if (regPos.test(val) || regNeg.test(val)) {
        return true;
    } else {
        return false;
    }
}
//生成guid
function newguid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}