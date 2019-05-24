var $page = function () {

    this.init = function () {
        addEvent();
        getCashManagerDetail();
    };
    var selector = this.selector = {};

    function addEvent() {
        getCompanyCode();
        $("#btnCancel").on("click", function () {
            history.go(-1);
        });
        //保存
        $("#btnSave").on("click", function () {
            $.ajax({
                url: "/CapitalCenterManagement/CashTransactionDetail/SaveCashTransactionDetail",
                data: {
                    AccountModeCode: $("#AccountModeCode").val(),
                    AccountModeName: $('#AccountModeCode option:selected').text(),
                    CompanyCode: $("#CompanyCode").val(),
                    CompanyName: $('#CompanyCode option:selected').text(),
                    TransactionDate: $("#TransactionDate").val(),
                    UseBalance: $("#UseBalance").val(),
                    TurnOut: $("#TurnOut").val(),
                    Balance: $("#Balance").val(),
                    ReimbursementOrgCode: $("#ReimbursementOrgCode").val(),
                    ReimbursementOrgName: $('#ReimbursementOrgCode option:selected').text(),
                    ReimbursementMan: $("#ReimbursementMan").val(),
                    Purpose: $("#Purpose").val(),
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
        //支付金额修改事件
        $("#TurnOut").on("blur", function () {
            var turnOut = $("#TurnOut").val();
            var useBalance = $("#UseBalance").val();
            if (turnOut != "") {
                $("#UseBalance").val(parseFloat(useBalance) - parseFloat(turnOut));
            }
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
            getOrgInfo();
        }
    });
    //companyCode = $("#CompanyCode").val();
}
function getOrgInfo() {
    var accountMode = $("#AccountModeCode").val();
    var companyCode = $("#CompanyCode").val();
    $.ajax({
        url: "/CapitalCenterManagement/LoanApplicationDetail/GetOrgInfo",
        data: { accountMode: accountMode, companyCode: companyCode },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            if (msg != null && msg != "") {
                uiEngineHelper.bindSelect('#ReimbursementOrgCode', msg, "Code", "Descrption");
            }
        }
    });
}
function getCashManagerDetail() {
    var guid = $.request.queryString().VGUID;
    $.ajax({
        url: "/CapitalCenterManagement/CashTransactionDetail/GetCashTransactionInfo",
        data: {
            "vguid": guid,
        },
        type: "post",
        dataType: "json",
        success: function (msg) {
            $("#AccountModeCode").val(msg.AccountModeCode);
            $("#CompanyCode").val(msg.CompanyCode);
            var date = ChangeDateFormat(msg.ApplyDate);
            $("#TransactionDate").val(date);
            $("#UseBalance").val(msg.UseBalance);
            $("#TurnOut").val(msg.TurnOut);
            $("#Balance").val(msg.Balance);
            $("#ReimbursementOrgCode").val(msg.ReimbursementOrgCode);
            $("#ReimbursementMan").val(msg.ReimbursementMan);
            $("#Purpose").val(msg.Purpose);
            $("#VGUID").val(msg.VGUID);
            
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