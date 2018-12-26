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
        $("#VGUID").val(guid);
        var myDate = new Date();
        var date = myDate.toLocaleDateString();     //获取当前日期
        $("#FillingDate").val($.action.replaceAll(date, '/', '-'));
        if (guid != "" && guid != null) {
            getOrderListDetail();
        } else {
            //$("#hideButton").show();
        }
        $("#Money").blur(function () {
            var money = $("#Money").val();
            if (money != "") {
                var value = smalltoBIG(money);
                $("#CapitalizationMoney").text(value);
                //$("#CapitalizationMoney").attr("title", $("#CapitalizationMoney").val())
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

        $('.types').click(function (event) {
            var id = event.target.id;
            switch (id) {
                case "Cash": $("#Cash").text("√"); $("#Bank").text("选择"); $("#Other").text("选择"); $("#PaymentMethod").val("现金");
                    break;
                case "Bank": $("#Bank").text("√"); $("#Cash").text("选择"); $("#Other").text("选择"); $("#PaymentMethod").val("银行");
                    break;
                case "Other": $("#Other").text("√"); $("#Cash").text("选择"); $("#Bank").text("选择"); $("#PaymentMethod").val("其他");
                    break;
                default:

            }
        })
        //取消
        $("#btnCancel").on("click", function () {
            window.close();
        })
        //保存
        $("#btnSave").on("click", function () {
            $.ajax({
                url: "/ReimbursementCenter/ROrderListDraftDetail/SaveOrderListDetail",
                //data: { vguids: selection },
                data: {
                    "VGUID": $("#VGUID").val(),
                    //"BusinessType": $("#BusinessType").val(),
                    //"BusinessProject": $("#BusinessProject").val(),
                    //"BusinessSubItem1": $("#BusinessSubItem1").val(),
                    //"BusinessSubItem2": $("#BusinessSubItem2").val(),
                    //"BusinessSubItem3": $("#BusinessSubItem3").val(),
                    //"OrderDate": $("#OrderDate").val(),
                    //"OrderTime": $("#OrderTime").val(),
                    "PaymentCompany": $("#PaymentCompany").val(),
                    //"CollectionCompany": $("#CollectionCompany").val(),
                    //"VisitorsNumber": $("#VisitorsNumber").val(),
                    //"EscortNumber": $("#EscortNumber").val(),
                    //"NumberCount": $("#NumberCount").val(),
                    //"VehicleType": $("#VehicleType").val(),
                    //"Money": $("#Money").val(),
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
                    "InvoiceNumber": $("#InvoiceNumber").val(),
                    "AttachmentNumber": $("#AttachmentNumber").val(),
                    "PaymentContents": $("#PaymentContents").val(),
                    "FillingDate": $("#FillingDate").val(),
                    "PaymentMethod": $("#PaymentMethod").val()
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

        $("#btnUp").on("click", function () {
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
                jqxNotification("请选择您要提交的数据！", null, "error");
            } else {
                WindowConfirmDialog(submit, "您确定要提交选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
        //审核
        $("#btnCheck").on("click", function () {
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
                jqxNotification("请选择您要提交的数据！", null, "error");
            } else {
                WindowConfirmDialog(check, "您确定要提交选中的数据？", "确认框", "确定", "取消", selection);
            }
        });


        //提交
        function submit(selection) {
            $.ajax({
                url: "/ReimbursementCenter/OrderListDraft/UpdataOrderListInfo",
                data: { vguids: selection, status: "2" },
                //traditional: true,
                type: "post",
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("提交失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("提交成功！", null, "success");
                            window.close();
                            window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                            break;
                    }
                }
            });
        }
        //审核
        function check(selection) {
            $.ajax({
                url: "/ReimbursementCenter/OrderListDraft/UpdataOrderListInfo",
                data: { vguids: selection, status: "3" },
                //traditional: true,
                type: "post",
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("提交失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("提交成功！", null, "success");
                            window.close();
                            window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                            break;
                    }
                }
            });
        }

    }; //addEvent end

    function getOrderListDetail() {
        $.ajax({
            url: "/ReimbursementCenter/ROrderListDraftDetail/GetOrderListDetail",
            data: {
                "vguid": $("#VGUID").val(),
            },
            type: "post",
            dataType: "json",
            success: function (msg) {
                $("#Status").val(msg.Status);
                if ($("#Status").val() == "1") {
                    $("#btnUp").show();
                }
                if ($("#Status").val() == "2") {
                    $("#btnCheck").show();
                }
                //$("#BusinessType").val(msg.BusinessType);
                //$("#BusinessProject").val(msg.BusinessProject);
                //$("#BusinessSubItem1").val(msg.BusinessSubItem1);
                //$("#BusinessSubItem2").val(msg.BusinessSubItem2);
                //$("#BusinessSubItem3").val(msg.BusinessSubItem3);
                //$("#OrderDate").val(msg.OrderDate);
                //$("#OrderTime").val(msg.OrderTime);
                $("#PaymentCompany").val(msg.PaymentCompany);
                //$("#CollectionCompany").val(msg.CollectionCompany);
                //$("#VisitorsNumber").val(msg.VisitorsNumber);
                //$("#EscortNumber").val(msg.EscortNumber);
                //$("#NumberCount").val(msg.NumberCount);

                $("#Money").val(msg.Money);
                $("#CapitalizationMoney").text(msg.CapitalizationMoney);
                $("#EnterpriseLeader").val(msg.EnterpriseLeader);
                $("#ResponsibleLeader").val(msg.ResponsibleLeader);
                $("#JiCaiBuExamine").val(msg.JiCaiBuExamine);
                $("#DepartmentHead").val(msg.DepartmentHead);
                $("#Cashier").val(msg.Cashier);
                $("#Payee").val(msg.Payee);
                $("#InvoiceNumber").text(msg.InvoiceNumber);
                $("#AttachmentNumber").text(msg.AttachmentNumber);
                $("#PaymentContents").val(msg.PaymentContents);
                //$("#FillingDate").val(msg.FillingDate);
                var fillingDate = parseInt(msg.FillingDate.replace(/[^0-9]/ig, ""));//转时间戳
                var date = $.convert.toDate(new Date(fillingDate), "yyyy-MM-dd");
                //var d = date.split("-")[0] + " 年 " + date.split("-")[1] + " 月 " + date.split("-")[2] + " 日";
                $("#FillingDate").val(date);
                $("#PaymentMethod").val(msg.PaymentMethod);
                switch (msg.PaymentMethod) {
                    case "现金": $("#Cash").text("√");
                        break;
                    case "银行": $("#Bank").text("√");
                        break;
                    case "其他": $("#Other").text("√");
                        break;
                    default:
                }
                //$("#CapitalizationMoney").attr("title", $("#CapitalizationMoney").val())
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
        //var reg = /,$/gi;
        //attValue = attValue.replace(reg, "");
        var count = (attValue.split('发票&')).length - 1;
        var counts = (attValue.split('其他&')).length - 1;
        //$("#InvoiceNumber").val(count);
        //$("#AttachmentNumber").val(count + counts);
        $("#InvoiceNumber").text(count);
        $("#AttachmentNumber").text(count + counts);
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
    var type = obj.parentElement.textContent.substring(0, 2);
    var name = obj.parentElement.textContent.substring(4, obj.parentElement.textContent.length - 1);
    var replaceStr = type + "&" + id + "&" + name;
    var attachmentValues = $("#Attachment").val();
    attachmentValues = $.action.replaceAll(attachmentValues, replaceStr, '');

    $("#Attachment").val(attachmentValues);
    $(obj).parent().remove();
    var attValue = attachmentValues;
    //var reg = /,$/gi;
    //attValue = attValue.replace(reg, "");
    var count = (attValue.split('发票&')).length - 1;
    var counts = (attValue.split('其他&')).length - 1;
    $("#InvoiceNumber").text(count);
    $("#AttachmentNumber").text(count + counts);
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