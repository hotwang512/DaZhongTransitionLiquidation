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
                var value = Arabia_To_SimplifiedChinese(money);
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
            }
        });
    }
};


$(function () {
    var page = new $page();
    page.init();
});


//阿拉伯数字转换为简写汉字
function Arabia_To_SimplifiedChinese(Num) {
    for (i = Num.length - 1; i >= 0; i--) {
        Num = Num.replace(",", "")//替换Num中的“,”
        Num = Num.replace(" ", "")//替换Num中的空格
    }
    if (isNaN(Num)) { //验证输入的字符是否为数字
        //alert("请检查小写金额是否正确");
        return;
    }
    //字符处理完毕后开始转换，采用前后两部分分别转换
    part = String(Num).split(".");
    newchar = "";
    //小数点前进行转化
    for (i = part[0].length - 1; i >= 0; i--) {
        if (part[0].length > 10) {
            //alert("位数过大，无法计算");
            return "";
        }//若数量超过拾亿单位，提示
        tmpnewchar = ""
        perchar = part[0].charAt(i);
        switch (perchar) {
            case "0": tmpnewchar = "零" + tmpnewchar; break;
            case "1": tmpnewchar = "壹" + tmpnewchar; break;
            case "2": tmpnewchar = "贰" + tmpnewchar; break;
            case "3": tmpnewchar = "叁" + tmpnewchar; break;
            case "4": tmpnewchar = "肆" + tmpnewchar; break;
            case "5": tmpnewchar = "伍" + tmpnewchar; break;
            case "6": tmpnewchar = "陆" + tmpnewchar; break;
            case "7": tmpnewchar = "柒" + tmpnewchar; break;
            case "8": tmpnewchar = "捌" + tmpnewchar; break;
            case "9": tmpnewchar = "玖" + tmpnewchar; break;
        }
        switch (part[0].length - i - 1) {
            case 0: tmpnewchar = tmpnewchar; break;
            case 1: if (perchar != 0) tmpnewchar = tmpnewchar + "拾"; break;
            case 2: if (perchar != 0) tmpnewchar = tmpnewchar + "佰"; break;
            case 3: if (perchar != 0) tmpnewchar = tmpnewchar + "仟"; break;
            case 4: tmpnewchar = tmpnewchar + "万"; break;
            case 5: if (perchar != 0) tmpnewchar = tmpnewchar + "拾"; break;
            case 6: if (perchar != 0) tmpnewchar = tmpnewchar + "佰"; break;
            case 7: if (perchar != 0) tmpnewchar = tmpnewchar + "仟"; break;
            case 8: tmpnewchar = tmpnewchar + "亿"; break;
            case 9: tmpnewchar = tmpnewchar + "拾"; break;
        }
        newchar = tmpnewchar + newchar;
    }
    //替换所有无用汉字，直到没有此类无用的数字为止
    while (newchar.search("零零") != -1 || newchar.search("零亿") != -1 || newchar.search("亿万") != -1 || newchar.search("零万") != -1) {
        newchar = newchar.replace("零亿", "亿");
        newchar = newchar.replace("亿万", "亿");
        newchar = newchar.replace("零万", "万");
        newchar = newchar.replace("零零", "零");
    }
    //替换以“一十”开头的，为“十”
    if (newchar.indexOf("一十") == 0) {
        newchar = newchar.substr(1);
    }
    //替换以“零”结尾的，为“”
    if (newchar.lastIndexOf("零") == newchar.length - 1) {
        newchar = newchar.substr(0, newchar.length - 1);
    }
    return newchar;
}