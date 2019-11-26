var guid = $.request.queryString().VGUID;
var vguid = "";
var payVGUID = "";
var isEdit = "";
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },

    $txtBankName: function () { return $("#txtBankName") },
    $txtChannelName: function () { return $("#txtChannelName") },

    $AddBankChannelDialog: function () { return $("#AddBankChannelDialog") },
    $AddBankChannel_OKButton: function () { return $("#AddBankChannel_OKButton") },
    $AddBankChannel_CancelBtn: function () { return $("#AddBankChannel_CancelBtn") },
    $txtBankAccount_Dialog: function () { return $("#txtBankAccount_Dialog") },
    $txtBankAccountName_Dialog: function () { return $("#txtBankAccountName_Dialog") },
    $txtChannel_Dialog: function () { return $("#txtChannel_Dialog") },
    $txtBank_Dialog: function () { return $("#txtBank_Dialog") },
    $EditPermission: function () { return $("#EditPermission") },
    $btnEditIsable: function () { return $("#EditIsable") },
};
var $page = function () {

    this.init = function () {
        addEvent();
        getCashManagerDetail();
    };

    function addEvent() {
        if (guid != "" && guid != null) {
            $("#UseBalance").val("");
            payVGUID = guid;
        } else {
            payVGUID = $("#VGUID").val();
        }
        getCompanyCode();
        //加载借贷数据
        initTable();
        //获取当前日期
        var tradeDate = new Date();
        var month = (tradeDate.getMonth() + 1) > 9 ? (tradeDate.getMonth() + 1) : "0" + (tradeDate.getMonth() + 1);
        var day = tradeDate.getDate() > 9 ? tradeDate.getDate() : "0" + tradeDate.getDate();
        var date = tradeDate.getFullYear() + "-" + month + "-" + day;
        if ($("#ApplyDate").val() == "") {
            $("#ApplyDate").val(date);
        }
        $("#Cashier").val($("#LoginName").val());
        //金额转化大写
        $("#Money").blur(function () {
            var money = $("#Money").val().replace(/,/g, "");//string.replace(new RegExp(key,'g'),"b");
            var money0 = "";
            var money1 = "";
            var moneyList = money.split(".");
            if (moneyList.length == 2) {
                $("#Money").val(moneyList[0].replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,') + "." + moneyList[1]);
            } else {
                $("#Money").val(moneyList[0].replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,'));
            }
            if (money != "") {
                var value = smalltoBIG(money);
                $("#MoneyA").val(value);
            } else {
                $("#MoneyA").val("");
            }
        });
        //预览
        $("#Preview").on("click", function () {
            var companyName = $('#CompanyCode option:selected').text();
            $("#lblCompanyName").text(companyName);
            $("#lblApplyDate").text($("#ApplyDate").val());
            $("#lblNo").text($("#lblNoA").text());
            $("#lblBankAccountName").text($("#BankAccountName").val());
            $("#lblBankAccount").text($("#BankAccount").val());
            $("#lblBankName").text($("#BankName").val());
            $("#lblMoneyA").text($("#MoneyA").val());
            $("#lblMoney").text($("#Money").val());
            $("#lblCheckNo").text($("#CheckNo").val());
            $("#lblRemark").text($("#Remark2").val());
            $("#lblCashier").text($("#Cashier").val());
            $("#lblAuditor").text($("#Auditor").val());
            $("#ShowDialog").modal({ backdrop: "static", keyboard: false });
            $("#ShowDialog").modal("show");
        });
        //保存
        $("#btnSave").on("click", function () {
            $.ajax({
                url: "/CapitalCenterManagement/CashManagerDetail/SaveCashManagerDetail",
                data: {
                    AccountModeCode: $("#AccountModeCode").val(),
                    AccountModeName: $('#AccountModeCode option:selected').text(),
                    CompanyCode: $("#CompanyCode").val(),
                    CompanyName: $('#CompanyCode option:selected').text(),
                    ApplyDate: $("#ApplyDate").val(),
                    No: $("#lblNoA").text(),
                    BankAccountName: $("#BankAccountName").val(),
                    BankAccount: $("#BankAccount").val(),
                    BankName: $("#BankName").val(),
                    Money: $("#Money").val().replace(/,/g, ""),
                    CheckNo: $("#CheckNo").val(),
                    Remark: $("#Remark2").val(),
                    Cashier: $("#Cashier").val(),
                    Auditor: $("#Auditor").val(),
                    VGUID: $("#VGUID").val(),
                    Status: $("#Status").val(),
                },
                type: "POST",
                dataType: "json",
                async: false,
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("保存失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("保存成功！", null, "success");
                            history.go(-1);
                            //window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                            break;
                    }
                }
            });
        });
        //打印
        $("#btnPrint").on("click", function () {
            $(".printTable").printArea();
        })
        //取消
        $("#AddNewBankData_CancelBtn").on("click", function () {
            $("#ShowDialog").modal({ backdrop: "static", keyboard: false });
            $("#ShowDialog").modal("hide");
        });
        $("#btnCancel").on("click", function () {
            history.go(-1);
        });
        //借贷保存
        $("#btnAddBorrow").on("click", function () {
            add("B");
        });
        $("#btnAddLoan").on("click", function () {
            add("L");
        });
        //弹出框中的取消按钮
        selector.$AddBankChannel_CancelBtn().on("click", function () {
            selector.$AddBankChannelDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddBankChannel_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            var borrow = $("#dropDownButtonContentjqxdropdownbutton1")[0].innerText;
            var loan = $("#dropDownButtonContentjqxdropdownbutton2")[0].innerText;
            var channelName = $("#txtChannelName  option:selected").text();
            if (validateError <= 0) {
                $.ajax({
                    url: "/CapitalCenterManagement/CashTransactionDetail/SaveCashBorrowLoan?isEdit=" + isEdit,
                    data: {
                        Borrow: borrow,
                        Loan: loan,
                        VGUID: vguid,
                        PayVGUID: payVGUID,
                        Remark: $("#Remark").val(),
                        Money: $("#Money2").val(),
                    },
                    type: "post",
                    dataType: "json",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                //jqxNotification("保存失败！", null, "error");
                                break;
                            case "1":
                                //jqxNotification("保存成功！", null, "success");
                                selector.$grid().jqxDataTable('updateBoundData');
                                selector.$AddBankChannelDialog().modal("hide");
                                break;
                            case "2":
                                //jqxNotification("当前月份该配置已经存在！", null, "error");
                                break;
                        }

                    }
                });
            }
        });
    }
    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Borrow', type: 'string' },
                    { name: 'Loan', type: 'string' },
                    { name: 'CompanyCode', type: 'string' },
                    { name: 'PayVGUID', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Money', type: 'number' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "PayVGUID": payVGUID },
                url: "/CapitalCenterManagement/CashTransactionDetail/GetCashBorrowLoan"   //获取数据源的路径
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
                    { width: 35, text: "", datafield: "checkbox", align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '借', datafield: "Borrow", align: 'center', cellsAlign: 'center', cellsRenderer: channelDetailFuncB },
                    { text: '贷', datafield: "Loan", align: 'center', cellsAlign: 'center', cellsRenderer: channelDetailFuncL },
                    { text: '摘要', datafield: 'Remark', align: 'center', cellsAlign: 'center', },
                    { text: '金额', datafield: 'Money', cellsFormat: "d2", align: 'center', cellsAlign: 'center', },
                    { text: '公司', datafield: 'CompanyCode', hidden: true },
                    { text: 'PayVGUID', datafield: 'PayVGUID', hidden: true, cellsRenderer: setVGUID },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });

    }

    function channelDetailFuncB(row, column, value, rowData) {
        var container = "";
        var borrow = "";
        var loan = "";
        if (rowData.Borrow != null) {
            borrow = rowData.Borrow.split(/[\s\n]/)[0];
            container = "<a href='#' onclick=edit('" + rowData.VGUID + "','"
                + borrow + "','"
                + loan + "','"
                + rowData.Remark + "','"
                + rowData.Money + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Borrow + "</a>";
        }
        return container;
    }
    function channelDetailFuncL(row, column, value, rowData) {
        var container = "";
        var borrow = "";
        var loan = "";
        if (rowData.Loan != null) {
            loan = rowData.Loan.split(/[\s\n]/)[0];
            container = "<a href='#' onclick=edit('" + rowData.VGUID + "','"
                + borrow + "','"
                + loan + "','"
                + rowData.Remark + "','"
                + rowData.Money + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Loan + "</a>";
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
    function setVGUID(row, column, value, rowData) {
        if (value != "" && value != null) {
            $("#VGUID").val(value);
        }
    }
    //删除
    function dele(selection) {
        $.ajax({
            url: "/CapitalCenterManagement/CashTransactionDetail/DeleteCashBorrowLoan",
            data: { vguids: selection },
            traditional: true,
            type: "post",
            success: function (msg) {
                if (msg.IsSuccess) {
                    jqxNotification("删除成功！", null, "success");
                    selector.$grid().jqxDataTable('updateBoundData');
                } else {
                    jqxNotification("删除失败！", null, "error");
                }
            }
        });
    }
};

function add(type) {
    if (type == "B") {
        $("#BorrowTr").show();
        $("#LoanTr").hide();
    } else {
        $("#BorrowTr").hide();
        $("#LoanTr").show();
    }
    $("#jqxdropdownbutton1").jqxDropDownButton('setContent', "");
    $("#jqxdropdownbutton2").jqxDropDownButton('setContent', "");
    $("#Remark").val("");
    $("#Month").val("");
    isEdit = false;
    vguid = "";
    $("#myModalLabel_title").text("新增借/贷方信息");
    selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddBankChannelDialog().modal("show");
    //initBorrowTable(companyCode, accountMode);
}
function edit(guid2, Borrow, Loan, Remark, Money) {
    $("#Remark").val("");
    $("#Money2").val("");
    isEdit = true;
    vguid = guid2;
    $("#myModalLabel_title").text("编辑借/贷方信息");
    $("#Remark").val(Remark);
    $("#Money2").val(Money);
    //initBorrowTable(CompanyCode, accountMode);
    if (Remark == null || Remark == "null") {
        $("#Remark").val("");
    }
    if (Borrow != null && Borrow != "") {
        $("#BorrowTr").show();
        $("#LoanTr").hide();
    } else {
        $("#BorrowTr").hide();
        $("#LoanTr").show();
    }
    var val = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + Borrow + '</div>';
    $("#jqxdropdownbutton1").jqxDropDownButton('setContent', val);
    var val2 = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + Loan + '</div>';
    $("#jqxdropdownbutton2").jqxDropDownButton('setContent', val2);

    $(".msg").remove();
    //selector.$txtBankAccount_Dialog().removeClass("input_Validate");
    //selector.$txtBankAccountName_Dialog().removeClass("input_Validate");
    //selector.$txtBank_Dialog().removeClass("input_Validate");
    //selector.$txtChannel_Dialog().removeClass("input_Validate");

    selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddBankChannelDialog().modal("show");
}

$(function () {
    var page = new $page();
    page.init();
});

function getCompanyCode() {
    var accountMode = $("#AccountModeCode").val();
    $.ajax({
        url: "/HomePage/CompanyHomePage/GetCompanyCode",
        data: { accountMode: accountMode },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#CompanyCode', msg, "CompanyCode", "CompanyName");
            //$("#CompanyCode").prepend("<option value=\"\" selected='true'>请选择</>");
            getBankInfo();
        }
    });
    //companyCode = $("#CompanyCode").val();
}
function getBankInfo() {
    var accountMode = $("#AccountModeCode").val();
    var companyCode = $("#CompanyCode").val();
    $.ajax({
        url: "/CapitalCenterManagement/CashManagerDetail/GetBankInfo",
        data: { accountMode: accountMode, companyCode: companyCode },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            if (msg != null && msg != "") {
                $("#BankAccountName").val(msg.BankAccountName);
                $("#BankAccount").val(msg.BankAccount);
                $("#BankName").val(msg.BankName);
            }
            companyChange();
        }
    });
}
function initBorrowTable(companyCode, accountMode) {
    var source = {
        datafields:
        [
            { name: 'BusinessCode', type: 'string' },
            { name: 'Company', type: 'string' },
            { name: 'CompanyCode', type: 'string' },
            { name: 'AccountingCode', type: 'string' },
            { name: 'CostCenterCode', type: 'string' },
            { name: 'SpareOneCode', type: 'string' },
            { name: 'SpareTwoCode', type: 'string' },
            { name: 'IntercourseCode', type: 'string' },
            { name: 'Accounting', type: 'string' },
            { name: 'CostCenter', type: 'string' },
            { name: 'SpareOne', type: 'string' },
            { name: 'SpareTwo', type: 'string' },
            { name: 'Intercourse', type: 'string' },
            { name: 'SubjectCode', type: 'string' },
            { name: 'SubjectVGUID', type: 'string' },
            { name: 'Checked', type: 'string' },
            { name: 'Balance', type: 'number' },
        ],
        datatype: "json",
        cache: false,
        id: "SectionVGUID",
        data: { companyCode: companyCode, accountModeCode: accountMode },
        url: "/PaymentManagement/SubjectBalance/GetSubjectBalance"    //获取数据源的路径
    };
    var typeAdapter = new $.jqx.dataAdapter(source);
    //创建卡信息列表（主表）
    $("#grid1").jqxGrid({
        pageable: true,
        width: "100%",
        autoheight: false,
        height: 350,
        columnsresize: true,
        pageSize: 15,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        rendergridrows: function (obj) {
            return obj.data;
        },
        virtualmode: false,
        pagermode: 'simple',
        columnsHeight: 40,
        columns: [
            //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
            { text: '编码', datafield: 'BusinessCode', width: 250, pinned: true, align: 'center', cellsAlign: 'center', },
            { text: '科目段', datafield: 'Company', width: 300, pinned: false, align: 'center', cellsAlign: 'center' },
            { text: '核算段', datafield: 'Accounting', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '成本中心段', datafield: 'CostCenter', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '备用1', datafield: 'SpareOne', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '备用2', datafield: 'SpareTwo', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '往来段', datafield: 'Intercourse', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '余额', datafield: 'Balance', cellsFormat: "d2", align: 'center', cellsAlign: 'center' },

            { text: '核算段', datafield: 'AccountingCode', hidden: true },
            { text: '成本中心段', datafield: 'CostCenterCode', hidden: true },
            { text: '备用1', datafield: 'SpareOneCode', hidden: true },
            { text: '备用2', datafield: 'SpareTwoCode', hidden: true },
            { text: '往来段', datafield: 'IntercourseCode', hidden: true },
            { text: 'SubjectCode', datafield: 'ParentCode', hidden: true },
            //{ text: 'BusinessCode', datafield: 'BusinessCode', hidden: true },
            { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
        ]
    });
    $("#jqxdropdownbutton1").jqxDropDownButton({
        width: 210, height: 30
    });
    $("#grid1").on('rowclick', function (event) {
        var args = event.args;
        var row = $("#grid1").jqxGrid('getrowdata', args.rowindex);
        var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + row['BusinessCode'] + '</div>';
        $("#jqxdropdownbutton1").jqxDropDownButton('setContent', dropDownContent);
    });

    $("#grid2").jqxGrid({
        pageable: true,
        width: "100%",
        autoheight: false,
        height: 350,
        columnsresize: true,
        pageSize: 15,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        rendergridrows: function (obj) {
            return obj.data;
        },
        virtualmode: false,
        pagermode: 'simple',
        columnsHeight: 40,
        columns: [
            //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
            { text: '编码', datafield: 'BusinessCode', width: 250, pinned: true, align: 'center', cellsAlign: 'center', },
            { text: '科目段', datafield: 'Company', width: 200, pinned: false, align: 'center', cellsAlign: 'center' },
            { text: '核算段', datafield: 'Accounting', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '成本中心段', datafield: 'CostCenter', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '备用1', datafield: 'SpareOne', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '备用2', datafield: 'SpareTwo', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '往来段', datafield: 'Intercourse', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '余额', datafield: 'Balance', cellsFormat: "d2", align: 'center', cellsAlign: 'center' },

            { text: '核算段', datafield: 'AccountingCode', hidden: true },
            { text: '成本中心段', datafield: 'CostCenterCode', hidden: true },
            { text: '备用1', datafield: 'SpareOneCode', hidden: true },
            { text: '备用2', datafield: 'SpareTwoCode', hidden: true },
            { text: '往来段', datafield: 'IntercourseCode', hidden: true },
            { text: 'SubjectCode', datafield: 'ParentCode', hidden: true },
            //{ text: 'BusinessCode', datafield: 'BusinessCode', hidden: true },
            { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
        ]
    });
    $("#jqxdropdownbutton2").jqxDropDownButton({
        width: 210, height: 30
    });
    $("#grid2").on('rowclick', function (event) {
        var args = event.args;
        var row = $("#grid2").jqxGrid('getrowdata', args.rowindex);
        var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + row['BusinessCode'] + '</div>';
        $("#jqxdropdownbutton2").jqxDropDownButton('setContent', dropDownContent);
    });
}
function companyChange() {
    $("#jqxdropdownbutton1").jqxDropDownButton('setContent', "");
    $("#jqxdropdownbutton2").jqxDropDownButton('setContent', "");
    var accountMode = $("#AccountModeCode").val();
    var companyCode = $("#CompanyCode").val();
    initBorrowTable(companyCode, accountMode);
}

function getCashManagerDetail() {
    var guid = $.request.queryString().VGUID;
    $.ajax({
        url: "/CapitalCenterManagement/CashManagerDetail/GetCashManagerInfo",
        data: {
            "vguid": guid,
        },
        type: "post",
        dataType: "json",
        success: function (msg) {
            $("#AccountModeCode").val(msg.AccountModeCode);
            $("#CompanyCode").val(msg.CompanyCode);
            var date = ChangeDateFormat(msg.ApplyDate);
            $("#ApplyDate").val(date);
            $("#lblNoA").text(msg.No);
            $("#BankAccountName").val(msg.BankAccountName);
            $("#BankAccount").val(msg.BankAccount);
            $("#BankName").val(msg.BankName);
            $("#Money").val(msg.Money);
            $("#CheckNo").val(msg.CheckNo);
            $("#Remark2").val(msg.Remark);
            $("#Cashier").val(msg.Cashier);
            $("#VGUID").val(msg.VGUID);
            $("#Status").val(msg.Status);
            payVGUID = msg.VGUID;
            if (msg.Money != "") { var value = smalltoBIG(msg.Money); $("#MoneyA").val(value); }
            if (msg.Status == "2" || msg.Status == "3") {
                $("#btnSave").hide();
                $("#btnAddBorrow").hide();
                $("#btnAddLoan").hide();
                $("#btnDelete").hide();
                $("#Auditor").val(msg.Auditor);
                if (msg.Auditor == "" || msg.Auditor == null) {
                    $("#Auditor").val($("#LoginName").val());
                }
            }
        }
    });
}
/** 时间转换 */
function ChangeDateFormat(val) {
    if (val != null) {
        var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", ""), 10));
        //月份为0-11，所以+1，月份小于10时补个0
        var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
        var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
        return date.getFullYear() + "-" + month + "-" + currentDate;
    }
    return "";
}
/** 数字金额大写转换(可以处理整数,小数,负数) */
function smalltoBIG(n) {
    var fraction = ['角', '分'];
    var digit = ['零', '壹', '贰', '叁', '肆', '伍', '陆', '柒', '捌', '玖'];
    var unit = [['元', '万', '亿'], ['', '拾', '佰', '仟']];
    var head = n < 0 ? '欠' : '';
    n = Math.abs(n);

    var s = '';

    for (var i = 0; i < fraction.length; i++) {
        s += (digit[Math.floor(n * 10 * Math.pow(10, i)) % 10] + fraction[i]).replace(/零./, '');
    }
    s = s || '整';
    n = Math.floor(n);

    for (var i = 0; i < unit[0].length && n > 0; i++) {
        var p = '';
        for (var j = 0; j < unit[1].length && n > 0; j++) {
            p = digit[n % 10] + unit[1][j] + p;
            n = Math.floor(n / 10);
        }
        s = p.replace(/(零.)*零$/, '').replace(/^$/, '零') + unit[0][i] + s;
    }
    return head + s.replace(/(零.)*零元/, '元').replace(/(零.)+/g, '零').replace(/^整$/, '零元整');
}