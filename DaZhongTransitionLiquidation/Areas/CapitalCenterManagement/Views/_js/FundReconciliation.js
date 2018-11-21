//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },

    $txtDatedTime: function () { return $("#txtDatedTime") },
    $AddNewBankDataDialog: function () { return $("#AddNewBankDataDialog") },
    $AddNewBankData_OKButton: function () { return $("#AddNewBankData_OKButton") },
    $AddNewBankData_CancelBtn: function () { return $("#AddNewBankData_CancelBtn") },

    $VoucherSubject: function () { return $("#VoucherSubject") },
    $VoucherSummary: function () { return $("#VoucherSummary") },

    $txtBankAccountName_Dialog: function () { return $("#txtBankAccountName_Dialog") },
    $txtBankAccount_Dialog: function () { return $("#txtBankAccount_Dialog") },
    $txtBank_Dialog: function () { return $("#txtBank_Dialog") },

    $pushPeopleDropDownButton: function () { return $("#pushPeopleDropDownButton") },
    $pushTree: function () { return $("#pushTree") },

    $txtChannel_Dialog: function () { return $("#txtChannel_Dialog") },
    $txtRemark_Dialog: function () { return $("#txtRemark_Dialog") },
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
        $("#btnAdd").on("click", function () {
            $("#BankBalance").val("");
            $("#BalanceDate").val("");
            $("#ReconciliantDate").val("");
            $("#Reconcilianter").val("");
            selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
            selector.$AddNewBankDataDialog().modal("show");
        });
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
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            //$("#TradingBank").val("");
            //$("#TransactionDate").val("");
            //$("#TransactionDateEnd").val("");
            //$("#PaymentUnit").val("");
        });

        //弹出框中的取消按钮
        selector.$AddNewBankData_CancelBtn().on("click", function () {
            selector.$AddNewBankDataDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddNewBankData_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate($("#BankBalance"))) {
                validateError++;
            }
            if (!Validate($("#Reconcilianter"))) {
                validateError++;
            }
            if (validateError <= 0) {
                $.ajax({
                    url: "/CapitalCenterManagement/FundReconciliation/SaveFundReconciliation?isEdit=" + isEdit,
                    data: {
                        "BankBalance": $("#BankBalance").val(),
                        "BalanceDate": $("#BalanceDate").val(),
                        "ReconciliantDate": $("#ReconciliantDate").val(),
                        "Reconcilianter": $("#Reconcilianter").val(),
                        "VGUID": vguid
                    },
                    type: "post",
                    dataType: "json",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                selector.$grid().jqxDataTable('updateBoundData');
                                selector.$AddNewBankDataDialog().modal("hide");
                                break;
                            case "2":
                                jqxNotification("余额日期有误！", null, "error");
                                break;
                            case "3":
                                jqxNotification("前一天对账失败！", null, "error");
                                break;
                        }
                    }
                });
            }
        });

    }; //addEvent end

    //删除
    function dele(selection) {
        $.ajax({
            url: "/CapitalCenterManagement/FundReconciliation/DeleteFundReconciliation",
            //data: { vguids: selection },
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
    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Reconcilianter', type: 'string' },
                    { name: 'BankBalance', type: 'number' },
                    { name: 'BalanceDate', type: 'date' },
                    { name: 'ReconciliantDate', type: 'date' },
                    { name: 'ReconciliantStatus', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: {},
                url: "/CapitalCenterManagement/FundReconciliation/GetFundReconciliationData"   //获取数据源的路径
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
                //selectionMode: "selectionMode",
                columns: [
                    { text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '银行余额', datafield: 'BankBalance', width: 250, cellsFormat: "d2", pinned: false, align: 'center', cellsAlign: 'center', cellsRenderer: redcolcorFunc },
                    { text: '余额日期', datafield: 'BalanceDate', width: 250, pinned: false, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd", cellsRenderer: redcolcorFunc },
                    { text: '对账日期', datafield: 'ReconciliantDate', width: 250, pinned: false, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd", cellsRenderer: redcolcorFunc },
                    { text: '对账人', datafield: 'Reconcilianter', width: 250, align: 'center', cellsAlign: 'center', cellsRenderer: redcolcorFunc },
                    { text: '对账状态', datafield: 'ReconciliantStatus', width: 250, align: 'center', cellsAlign: 'center', cellsRenderer: redcolcorFunc },
                    { text: '操作', align: 'center', cellsAlign: 'center', cellsRenderer: cellsDoneFunc },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });
    }

    function cellsDoneFunc(row, column, value, rowData) {
        var container = "";
        if (rowData.ReconciliantStatus == "对账失败") {
            container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.BankBalance + "','" + rowData.BalanceDate.Format("yyyy-MM-dd") + "','" + rowData.ReconciliantDate.Format("yyyy-MM-dd") + "','" + rowData.Reconcilianter + "')>重新对账</a>";
        }
        return container;
    }

    function redcolcorFunc(row, column, value, rowData) {
        var container = "";
        if (rowData.ReconciliantStatus == "对账成功") {
            container = "<font style='color:green'>" + value + "</font>";
        }
        else {
            container = "<font style='color:red'>" + value + "</font>";
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

function edit(guid, bankBalance, balanceDate, reconciliantDate, reconcilianter) {
    isEdit = true;
    vguid = guid;
    if (bankBalance == null || bankBalance == "null") {
        bankBalance = "";
    }
    if (balanceDate == null || balanceDate == "null") {
        balanceDate = "";
    }
    if (reconciliantDate == null || reconciliantDate == "null") {
        reconciliantDate = "";
    }
    if (reconcilianter == null || reconcilianter == "null") {
        reconcilianter = "";
    }
    $("#BankBalance").val(bankBalance);
    $("#BalanceDate").val(balanceDate);
    $("#ReconciliantDate").val(reconciliantDate);
    $("#Reconcilianter").val(reconcilianter);
    $("#myModalLabel_title").text("重新对账");
    //$("#AddNewBankDataDialog table tr").eq(1).hide();
    $(".msg").remove();
    selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewBankDataDialog().modal("show");
}

$(function () {
    var page = new $page();
    page.init();
});
