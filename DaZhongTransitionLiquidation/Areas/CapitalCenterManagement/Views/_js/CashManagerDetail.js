var $page = function () {

    this.init = function () {
        addEvent();
        getCashManagerDetail();
    };
    var selector = this.selector = {};

    function addEvent() {
        getCompanyCode();
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
            var money = $("#Money").val();
            if (money != "") {var value = smalltoBIG(money);$("#MoneyA").val(value);}
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
            $("#lblRemark").text($("#Remark").val());
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
                    Money: $("#Money").val(),
                    CheckNo: $("#CheckNo").val(),
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
        }
    });
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