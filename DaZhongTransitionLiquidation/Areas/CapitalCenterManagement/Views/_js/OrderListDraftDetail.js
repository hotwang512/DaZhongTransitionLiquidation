//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
}; //selector end
var isEdit = false;
var vguid = "";

var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        var guid = $.request.queryString().VGUID;
        $("#VGUID").val(guid)
        if (guid != "" && guid != null) {
            getOrderListDetail();
        } else {
            $("#hideButton").show();
        }
        $("#Money").blur(function () {
            var money = $("#Money").val();
            if (money != "") {
                var value = smalltoBIG(money);
                $("#CapitalizationMoney").val(value);
                $("#CapitalizationMoney").attr("title", $("#CapitalizationMoney").val())
            }
        });
        $("#VisitorsNumber").blur(function () {
            var visitorsNumber = $("#VisitorsNumber").val();
            var escortNumber = $("#EscortNumber").val();
            if (visitorsNumber != "" && escortNumber != "") {
                $("#NumberCount").val((parseInt(visitorsNumber) + parseInt(escortNumber)));
            }
        });
        $("#EscortNumber").blur(function () {
            var visitorsNumber = $("#VisitorsNumber").val();
            var escortNumber = $("#EscortNumber").val();
            if (visitorsNumber != "" && escortNumber != "") {
                $("#NumberCount").val((parseInt(visitorsNumber) + parseInt(escortNumber)));
            }
        });
        //取消
        $("#btnCancel").on("click", function () {
            window.close();
        })
        //保存
        $("#btnSave").on("click", function () {
            $.ajax({
                url: "/CapitalCenterManagement/OrderListDraftDetail/SaveOrderListDetail",
                //data: { vguids: selection },
                data: {
                    "VGUID": $("#VGUID").val(),
                    "BusinessType": $("#BusinessType").val(),
                    "BusinessProject": $("#BusinessProject").val(),
                    "BusinessSubItem1": $("#BusinessSubItem1").val(),
                    "BusinessSubItem2": $("#BusinessSubItem2").val(),
                    "BusinessSubItem3": $("#BusinessSubItem3").val(),
                    "OrderDate": $("#OrderDate").val(),
                    "OrderTime": $("#OrderTime").val(),
                    "PaymentCompany": $("#PaymentCompany").val(),
                    "CollectionCompany": $("#CollectionCompany").val(),
                    "VisitorsNumber": $("#VisitorsNumber").val(),
                    "EscortNumber": $("#EscortNumber").val(),
                    "NumberCount": $("#NumberCount").val(),
                    "VehicleType": $("#VehicleType").val(),
                    "Money": $("#Money").val(),
                    "CapitalizationMoney": $("#CapitalizationMoney").val(),
                    "EnterpriseLeader": $("#EnterpriseLeader").val(),
                    "ResponsibleLeader": $("#ResponsibleLeader").val(),
                    "JiCaiBuExamine": $("#JiCaiBuExamine").val(),
                    "DepartmentHead": $("#DepartmentHead").val(),
                    "Cashier": $("#Cashier").val(),
                    "Payee": $("#Payee").val(),
                    "Status": "1",
                    "Founder": $("#LoginName").val()
                },
                type: "post",
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("保存失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("保存成功！", null, "success");
                            window.close();
                            window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                            break;
                    }
                }
            });
        })
    }; //addEvent end

    function getOrderListDetail() {
        $.ajax({
            url: "/CapitalCenterManagement/OrderListDraftDetail/GetOrderListDetail",
            data: {
                "vguid": $("#VGUID").val(),
            },
            type: "post",
            dataType: "json",
            success: function (msg) {
                $("#Status").val(msg.Status);
                if ($("#Status").val() == "1") {
                    $("#hideButton").show();
                }
                $("#BusinessType").val(msg.BusinessType);
                $("#BusinessProject").val(msg.BusinessProject);
                $("#BusinessSubItem1").val(msg.BusinessSubItem1);
                $("#BusinessSubItem2").val(msg.BusinessSubItem2);
                $("#BusinessSubItem3").val(msg.BusinessSubItem3);
                $("#OrderDate").val(msg.OrderDate);
                $("#OrderTime").val(msg.OrderTime);
                $("#PaymentCompany").val(msg.PaymentCompany);
                $("#CollectionCompany").val(msg.CollectionCompany);
                $("#VisitorsNumber").val(msg.VisitorsNumber);
                $("#EscortNumber").val(msg.EscortNumber);
                $("#NumberCount").val(msg.NumberCount);
                $("#VehicleType").val(msg.VehicleType);
                $("#Money").val(msg.Money);
                $("#CapitalizationMoney").val(msg.CapitalizationMoney);
                $("#EnterpriseLeader").val(msg.EnterpriseLeader);
                $("#ResponsibleLeader").val(msg.ResponsibleLeader);
                $("#JiCaiBuExamine").val(msg.JiCaiBuExamine);
                $("#DepartmentHead").val(msg.DepartmentHead);
                $("#Cashier").val(msg.Cashier);
                $("#Payee").val(msg.Payee);
                $("#CapitalizationMoney").attr("title", $("#CapitalizationMoney").val())
            }
        });
    }
};


$(function () {
    var page = new $page();
    page.init();
});


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