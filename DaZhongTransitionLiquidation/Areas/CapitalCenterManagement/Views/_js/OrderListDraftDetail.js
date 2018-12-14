﻿//所有元素选择器
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
                    "Founder": $("#LoginName").val(),
                    "Attachment": $("#Attachment").val(),
                    "InvoiceNumber":$("#InvoiceNumber").val(),
                    "AttachmentNumber":$("#AttachmentNumber").val()
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
                $("#InvoiceNumber").val(msg.InvoiceNumber);
                $("#AttachmentNumber").val(msg.AttachmentNumber);
                $("#CapitalizationMoney").attr("title", $("#CapitalizationMoney").val())
                loadAttachments(msg.Attachment);
            }
        });
    }
};


$(function () {
    var page = new $page();
    page.init();
});

$(function () {
    var buttonText = {
        browseButton: '上传',
        uploadButton: '提交',
        cancelButton: '清空',
        uploadFileTooltip: '上传',
        cancelFileTooltip: '删除'
    };
    $('#btn_Attachment').jqxFileUpload({ width: '600px', height: '', fileInputName: 'AttachmentFile', browseTemplate: 'success', uploadTemplate: 'primary', cancelTemplate: 'danger', localization: buttonText, multipleFilesUpload: true });
    $("#btn_Attachment").on("select", function (event) {
        if (event.args.size > (1024 * 1024 * 10)) {
            jqxAlert("单文件大小不能超过10M");
            $("#btn_AttachmentCancelButton").trigger('click');
        }
    });
    $("#btn_Attachment").on("uploadStart", function (event) {
        //获取文件名
        fileName = event.args.file;
        var extStart = fileName.lastIndexOf(".");
        //判断是文件还是图片
        var ext = fileName.substring(extStart, fileName.length).toUpperCase();
        if (ext != ".BMP" && ext != ".PNG" && ext != ".GIF" && ext != ".JPG" && ext != ".JPEG") {//上传文件
            $('#btn_Attachment').jqxFileUpload({ uploadUrl: '/File/UploadFile?allowSize=' + 20 });
        }
        else {//上传图片
            $('#btn_Attachment').jqxFileUpload({ uploadUrl: '/File/UploadImage?allowSize=' + 20 });
        }
    })
    $("#btn_Attachment").on("uploadEnd", function (event) {
        var args = event.args;
        //var msg = $.convert.strToJson($(args.response).html());
        uploadFiles(event)
        var attValue = $("#Attachment").val();
        var count = (attValue.split('发票&')).length - 1;
        var counts = (attValue.split('其他&')).length - 1;
        $("#InvoiceNumber").val(count);
        $("#AttachmentNumber").val(counts);
    })
})
var fileName = "";
function uploadFiles(event) {
    var args = event.args;
    var msg = $.convert.strToJson($(args.response).html());
    fileName = event.args.file;
    var attachments = $("#Attachment").val();
    var type = $("#AttachmentType").val();
    if (attachments == "") {
        attachments = type + "&" + msg.WebPath + "&" + fileName;
    }
    else {
        attachments = attachments + "," + type + "&" + msg.WebPath + "&" + fileName;
    }

    $("#attachments")[0].innerHTML += "<span>" + type + "&nbsp;&nbsp;<a href='" + msg.WebPath + "' target='_blank'>" + fileName + "</a><button class='closes' type='button' onclick='removeAttachment(this)'>×</button></br></span>"
    $("#Attachment").val(attachments);


}
function loadAttachments(attachments) {
    $("#Attachment").val(attachments);
    if (attachments != "") {
        var attachValues = attachments.split(",");
        for (var i = 0; i < attachValues.length; i++) {
            var attach = attachValues[i].split("&");
            $("#attachments")[0].innerHTML += "<span>" + attach[0] + "&nbsp;&nbsp;<a href='" + attach[1] + "' target='_blank'>" + attach[2] + "</a><button class='closes' type='button' onclick='removeAttachment(this)'>×</button></br></span>"
        }
    }
}
function removeAttachment(obj) {

    var id = obj.previousSibling.attributes["href"].value;
    var attachmentValues = $("#Attachment").val();
    attachmentValues = $.action.replaceAll(attachmentValues, id, '');

    $("#Attachment").val(attachmentValues);
    $(obj).parent().remove();
    return false;
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