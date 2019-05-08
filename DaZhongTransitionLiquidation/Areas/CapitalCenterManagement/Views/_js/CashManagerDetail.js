var $page = function () {

    this.init = function () {
        addEvent();
    };
    var selector = this.selector =
        {
            
        }

    function addEvent() {
        getCompanyCode();
        //获取当前日期
        var tradeDate = new Date();
        var month = (tradeDate.getMonth() + 1) > 9 ? (tradeDate.getMonth() + 1) : "0" + (tradeDate.getMonth() + 1);
        var day = tradeDate.getDate() > 9 ? tradeDate.getDate() : "0" + tradeDate.getDate();
        var date = tradeDate.getFullYear() + "-" + month + "-" + day;
        $("#ApplyDate").val(date);
        //金额转化大写
        $("#Money").blur(function () {
            var money = $("#Money").val();
            if (money != "") {var value = smalltoBIG(money);$("#MoneyA").val(value);}
        });
        //预览
        $("#Preview").on("click", function () {

        });
        //保存
        $("#btnSave").on("click", function () {

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