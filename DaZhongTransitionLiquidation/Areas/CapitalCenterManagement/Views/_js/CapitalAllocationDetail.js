var $page = function () {

    this.init = function () {
        addEvent();
        getCapitalAllocationDetail();
    };
    var selector = this.selector = {};

    function addEvent() {
        getTurnInCompanyCode();
        getTurnOutCompanyCode();
        getTurnInBankInfo();
        getTurnOutBankInfo();
        getTurnInBankAccount();
        getTurnOutBankAccount();
        //获取当前日期
        var tradeDate = new Date();
        var month = (tradeDate.getMonth() + 1) > 9 ? (tradeDate.getMonth() + 1) : "0" + (tradeDate.getMonth() + 1);
        var day = tradeDate.getDate() > 9 ? tradeDate.getDate() : "0" + tradeDate.getDate();
        var date = tradeDate.getFullYear() + "-" + month + "-" + day;
        if ($("#ApplyDate").val() == "") {
            $("#ApplyDate").val(date);
        }

        //金额转化大写
        $("#Money").blur(function () {
            var money = $("#Money").val();
            if (money != "") { var value = smalltoBIG(money); $("#MoneyA").val(value); }
        });
        //预览
        $("#Preview").on("click", function () {
            var turnInCompanyName = $('#TurnInCompanyCode option:selected').text();
            var turnOutCompanyName = $('#TurnOutCompanyCode option:selected').text();
            var turnInBankName = $('#TurnInBankName option:selected').text();
            var turnOutBankName = $('#TurnOutBankName option:selected').text();
            $("#lblTurnInCompanyName").text(turnInCompanyName);
            $("#lblTurnOutCompanyName").text(turnOutCompanyName);
            $("#lblApplyDate").text($("#ApplyDate").val());
            $("#lblNo").text($("#lblNoA").text());
            $("#lblTurnInBankAccount").text($("#TurnInBankAccount").val());
            $("#lblTurnInBankName").text(turnInBankName);
            $("#lblTurnOutBankAccount").text($("#TurnOutBankAccount").val());
            $("#lblTurnOutBankName").text(turnOutBankName);
            $("#lblMoneyA").text($("#MoneyA").val());
            $("#lblMoney").text($("#Money").val());
            $("#lblRemark").text($("#Remark").val());
            $("#lblCashier").text($("#Cashier").val());
            $("#lblAuditor").text($("#Auditor").val());
            $("#ShowDialog").modal({ backdrop: "static", keyboard: false });
            $("#ShowDialog").modal("show");
        });
        //保存
        $("#btnSave").on("click", function () {
            $.ajax({
                url: "/CapitalCenterManagement/CapitalAllocationDetail/SaveCapitalAllocationDetail",
                data: {
                    TurnInAccountModeCode: $("#TurnInAccountModeCode").val(),
                    TurnInAccountModeName: $('#TurnInAccountModeCode option:selected').text(),
                    TurnInCompanyCode: $("#TurnInCompanyCode").val(),
                    TurnInCompanyName: $('#TurnInCompanyCode option:selected').text(),
                    TurnOutAccountModeCode: $("#TurnOutAccountModeCode").val(),
                    TurnOutAccountModeName: $('#TurnOutAccountModeCode option:selected').text(),
                    TurnOutCompanyCode: $("#TurnOutCompanyCode").val(),
                    TurnOutCompanyName: $('#TurnOutCompanyCode option:selected').text(),
                    ApplyDate: $("#ApplyDate").val(),
                    No: $("#lblNoA").text(),
                    TurnInBankAccount: $("#TurnInBankAccount").val(),
                    TurnInBankName: $("#TurnInBankName").val(),
                    TurnOutBankAccount: $("#TurnOutBankAccount").val(),
                    TurnOutBankName: $("#TurnOutBankName").val(),
                    Money: $("#Money").val(),
                    Remark: $("#Remark").val(),
                    Cashier: $("#Cashier").val(),
                    Auditor: $("#Auditor").val(),
                    VGUID: $("#VGUID").val()
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
                            window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
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
    }
};

$(function () {
    var page = new $page();
    page.init();
});

function getTurnInCompanyCode() {
    var turnInAccountMode = $("#TurnInAccountModeCode").val();
    $.ajax({
        url: "/HomePage/CompanyHomePage/GetCompanyCode",
        data: { accountMode: turnInAccountMode },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#TurnInCompanyCode', msg, "CompanyCode", "CompanyName");
            //$("#CompanyCode").prepend("<option value=\"\" selected='true'>请选择</>");
            getTurnInBankInfo();
        }
    });
}
function getTurnOutCompanyCode() {
    var turnOutAccountMode = $("#TurnOutAccountModeCode").val();
    $.ajax({
        url: "/HomePage/CompanyHomePage/GetCompanyCode",
        data: { accountMode: turnOutAccountMode },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#TurnOutCompanyCode', msg, "CompanyCode", "CompanyName");
            //$("#CompanyCode").prepend("<option value=\"\" selected='true'>请选择</>");
            getTurnOutBankInfo();
        }
    });
}
function getTurnInBankInfo() {
    var accountMode = $("#TurnInAccountModeCode").val();
    var companyCode = $("#TurnInCompanyCode").val();
    $.ajax({
        url: "/CapitalCenterManagement/CapitalAllocationDetail/GetBankInfo",
        data: { accountMode: accountMode, companyCode: companyCode },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#TurnInBankName', msg, "BankAccount", "BankName");
            getTurnInBankAccount();
        }
    });
}
function getTurnOutBankInfo() {
    var accountMode = $("#TurnOutAccountModeCode").val();
    var companyCode = $("#TurnOutCompanyCode").val();
    $.ajax({
        url: "/CapitalCenterManagement/CapitalAllocationDetail/GetBankInfo",
        data: { accountMode: accountMode, companyCode: companyCode },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#TurnOutBankName', msg, "BankAccount", "BankName");
            getTurnOutBankAccount();
        }
    });
}
function getTurnInBankAccount() {
    var val = $("#TurnInBankName").val();
    $("#TurnInBankAccount").val(val);
}
function getTurnOutBankAccount() {
    var val = $("#TurnOutBankName").val();
    $("#TurnOutBankAccount").val(val);
}
function getCapitalAllocationDetail() {
    var guid = $.request.queryString().VGUID;
    $.ajax({
        url: "/CapitalCenterManagement/CapitalAllocationDetail/GetCapitalAllocation",
        data: {
            "vguid": guid,
        },
        type: "post",
        dataType: "json",
        success: function (msg) {
            $("#TurnInAccountModeCode").val(msg.TurnInAccountModeCode);
            $("#TurnOutAccountModeCode").val(msg.TurnOutAccountModeCode);
            getTurnInCompanyCode();
            getTurnOutCompanyCode();
            $("#TurnInCompanyCode").val(msg.TurnInCompanyCode);
            $("#TurnOutCompanyCode").val(msg.TurnOutCompanyCode);
            getTurnInBankInfo();
            getTurnOutBankInfo();    
            var date = ChangeDateFormat(msg.ApplyDate);
            $("#ApplyDate").val(date);
            $("#lblNoA").text(msg.No);
            $("#TurnInBankAccount").val(msg.TurnInBankAccount);
            $("#TurnInBankName").val(msg.TurnInBankName);
            $("#TurnOutBankAccount").val(msg.TurnOutBankAccount);
            $("#TurnOutBankName").val(msg.TurnOutBankName);
            $("#Money").val(msg.Money);
            $("#Remark").val(msg.Remark);
            $("#Cashier").val(msg.Cashier);
            $("#Auditor").val(msg.Auditor);
            $("#VGUID").val(msg.VGUID);
            if (msg.Money != "") { var value = smalltoBIG(msg.Money); $("#MoneyA").val(value); }
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