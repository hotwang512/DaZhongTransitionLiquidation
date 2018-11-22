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
        $("#btnSync").on("click", function () {
            $.ajax({
                url: "/CapitalCenterManagement/BankFlowTemplate/SyncCurrentDayBankData",
                data: {},
                type: "post",
                dataType: "json",
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("同步失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("同步成功！", null, "success");
                            selector.$grid().jqxDataTable('updateBoundData');
                            break;
                    }
                }
            });
        });
        $("#btnYesterdaySync").on("click", function () {
            $.ajax({
                url: "/CapitalCenterManagement/BankFlowTemplate/SyncYesterdayBankData",
                data: {},
                type: "post",
                dataType: "json",
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("同步失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("同步成功！", null, "success");
                            selector.$grid().jqxDataTable('updateBoundData');
                            break;
                    }
                }
            });
        });
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#TradingBank").val("");
            $("#TransactionDate").val("");
            $("#TransactionDateEnd").val("");
            $("#PaymentUnit").val("");
        });

        //弹出框中的取消按钮
        selector.$AddNewBankData_CancelBtn().on("click", function () {
            selector.$AddNewBankDataDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddNewBankData_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate(selector.$VoucherSubject())) {
                validateError++;
            }
            if (!Validate(selector.$VoucherSummary())) {
                validateError++;
            }
            if (validateError <= 0) {
                $.ajax({
                    url: "/CapitalCenterManagement/BankFlowTemplate/SaveBankFlow?isEdit=" + isEdit,
                    data: {
                        "VoucherSummary": selector.$VoucherSummary().val(),
                        "VoucherSubject": selector.$VoucherSubject().val(),
                        "VoucherSubjectName": $("#VoucherSubjectName").val(),
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
                        }
                    }
                });
            }
        });

    }; //addEvent end


    function initTable() {
        var DateEnd = $("#TransactionDateEnd").val();
        var source =
            {
                datafields:
                [
                    //{ name: "checkbox", type: null },
                    { name: 'TradingBank', type: 'string' },                   
                    { name: 'TransactionDate', type: 'date' },
                    { name: 'TurnOut', type: 'number' },
                    { name: 'TurnIn', type: 'number' },
                    { name: 'Currency', type: 'string' },
                    { name: 'PaymentUnit', type: 'string' },
                    { name: 'PayeeAccount', type: 'string' },
                    { name: 'PaymentUnitInstitution', type: 'string' },
                    { name: 'ReceivingUnit', type: 'string' },
                    { name: 'ReceivableAccount', type: 'string' },
                    { name: 'ReceivingUnitInstitution', type: 'string' },
                    { name: 'Purpose', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Batch', type: 'string' },
                    { name: 'VoucherSubject', type: 'string' },
                    { name: 'VoucherSubjectName', type: 'string' },
                    { name: 'VoucherSummary', type: 'string' },
                    { name: 'Balance', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "TradingBank": $("#TradingBank").val(), "TransactionDate": $("#TransactionDate").val(), "PaymentUnit": $("#PaymentUnit").val() },
                url: "/CapitalCenterManagement/BankFlowTemplate/GetBankFlowData?TransactionDateEnd=" + DateEnd   //获取数据源的路径
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
                    //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'T24交易流水号', datafield: 'Batch', width: 200, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '交易银行', datafield: 'TradingBank', width: 100, pinned: true, align: 'center', cellsAlign: 'center' },
                    { text: '交易日期', datafield: 'TransactionDate', width: 150, pinned: true, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd hh:mm:ss" },
                    { text: '转出(借)', datafield: 'TurnOut', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '转入(贷)', datafield: 'TurnIn', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '币种', datafield: 'Currency', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '付款单位', datafield: 'PaymentUnit', width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '付款人账号', datafield: 'PayeeAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '付款单位开户机构', datafield: 'PaymentUnitInstitution', width: 350, align: 'center', cellsAlign: 'center' },
                    { text: '收款单位名称', datafield: 'ReceivingUnit', width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '收款账号', datafield: 'ReceivableAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '收款单位开户机构', datafield: 'ReceivingUnitInstitution', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '用途', datafield: 'Purpose', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '备注', datafield: 'Remark', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '凭证科目Code', hidden: true,datafield: 'VoucherSubject', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '凭证科目', datafield: 'VoucherSubjectName', width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '凭证摘要', datafield: 'VoucherSummary', width: 300, align: 'center', cellsAlign: 'center' },
                    { text: 'Balance', datafield: 'Balance', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }

    function detailFunc(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "1") {
            container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.VoucherSubject + "','" + rowData.VoucherSummary + "','" + rowData.VoucherSubjectName + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Batch + "</a>";
        } else {
            container = "<span>" + rowData.Batch + "</span>";
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

function edit(guid, voucherSubject,voucherSummary,voucherSubjectName) {
    selector.$VoucherSubject().val("");
    selector.$VoucherSummary().val("");
    isEdit = true;
    vguid = guid;
    if (voucherSubject == null || voucherSubject == "null") {
        voucherSubject = "";
    }
    if (voucherSubjectName == null || voucherSubjectName == "null") {
        voucherSubjectName = "";
    }
    if (voucherSummary == null || voucherSummary == "null") {
        voucherSummary = "";
    }
    selector.$VoucherSubject().val(voucherSubject);
    $("#VoucherSubjectName").val(voucherSubjectName);
    selector.$VoucherSummary().val(voucherSummary);
    selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', "");
    $("#myModalLabel_title").text("编辑银行数据");
    //$("#AddNewBankDataDialog table tr").eq(1).hide();
    var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 5px;">' + voucherSubjectName + '</div>';
    selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', dropDownContent);
    $(".msg").remove();
    selector.$VoucherSubject().removeClass("input_Validate");
    selector.$VoucherSummary().removeClass("input_Validate");
    selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewBankDataDialog().modal("show");
}

$(function () {
    var page = new $page();
    page.init();
});
