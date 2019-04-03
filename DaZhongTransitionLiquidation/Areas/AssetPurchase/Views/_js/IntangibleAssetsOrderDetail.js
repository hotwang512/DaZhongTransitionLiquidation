//无形资产订单维护明细
var vguid = "";
var mydate = new Date();
var vehicleDefaultData;
var $page = function () {
    this.init = function () {
        addEvent();
    }
    //所有事件
    function addEvent() {
        initSelect();
        var guid = $.request.queryString().VGUID;
        debugger;
        $("#VGUID").val(guid);
        if (guid != "" && guid != null) {
            getIntangibleAssetsOrderDetail();
        }
        //取消
        $("#btnCancel").on("click",
            function () {
                history.go(-1);
            });
        //保存
        $("#btnSave").on("click",
            function () {
                var validateError = 0; //未通过验证的数量
                if (!Validate($("#OrderType"))) {
                    validateError++;
                }
                if (validateError <= 0) {
                    $.ajax({
                        url: "/AssetPurchase/IntangibleAssetsOrderDetail/SaveIntangibleAssetsOrder",
                        data: {
                            "VGUID": $("#VGUID").val(),
                            "OrderType": $("#OrderType").val(),
                            "PaymentInformationVguid": $("#hiddenPaymentInformationVguid").val(),
                            "PaymentInformation": $("#hiddenPaymentInformation").val(),
                            "SumPayment": $("#SumPayment").val(),
                            "FirstPayment": $("#FirstPayment").val(),
                            "TailPayment": $("#TailPayment").val(),
                            "ContractAmount": $("#ContractAmount").val(),
                            "SupplierInformation": $("#SupplierInformation").val(),
                            "ContractName": $("#Attachment").attr("title"),
                            "ContractFilePath": $("#Attachment").attr("href")
                        },
                        type: "post",
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
                }
            });
        $("#OrderQuantity").on("click",
            function () {
                initTable();
            });
        $("#UploadContractFile").on("click",
            function () {
                $("#ContractFileInput").click();
            });
        //确定
        $("#OrderDetailsDialog_OKBtn").on("click",
            function () {
                $("#OrderDetailsDialog").modal("hide");
            });
        //取消
        $("#OrderDetailsDialog_CancelBtn").on("click",
            function () {
                $("#OrderDetailsDialog").modal("hide");
            });
        //提交
        $("#btnSubmit").on("click",
            function () {
                $.post("/AssetPurchase/IntangibleAssetsOrderDetail/SubmitIntangibleAssetsOrder", { vguid: $("#VGUID").val() }, function (msg) {
                    switch (msg.Status) {
                    case "0":
                        jqxNotification("提交失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("提交成功！", null, "success");
                        history.go(-1);
                        window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                        break;
                    }
                });
            }
        );
        $('#PaymentInformation').on('select', function (event) {
            var args = event.args;
            if (args) {
                debugger;
                var item = args.item;
                $("#hiddenPaymentInformationVguid").val(item.value);
                $("#hiddenPaymentInformation").val(item.label);
                //$.post("/AssetPurchase/FixedAssetsOrderDetail/GetCustomerBankInfo", { vguid: item.value }, function (msg) {
                //    $("#BankAccountName").val(msg.BankAccountName);
                //    $("#BankAccount").val(msg.BankAccount);
                //    $("#Bank").val(msg.Bank);
                //    $("#BankNo").val(msg.BankNo);
                //});
            }
        });
    }; //addEvent end

    function getIntangibleAssetsOrderDetail() {
        $.post("/AssetPurchase/IntangibleAssetsOrderDetail/GetIntangibleAssetsOrder", { vguid: $("#VGUID").val() }, function (msg) {
            $("#OrderType").val(msg.OrderType);
            debugger;
            $("#hiddenPaymentInformationVguid").val(msg.PaymentInformationVguid);
            $("#hiddenPaymentInformation").val(msg.PaymentInformation);
            $("#SumPayment").val(msg.SumPayment);
            $("#FirstPayment").val(msg.FirstPayment);
            $("#TailPayment").val(msg.TailPayment);
            $("#SupplierInformation").val(msg.SupplierInformation);
            $("#ContractName").val(msg.ContractName);
            debugger;
            if (msg.ContractName != null && msg.ContractFilePath != null) {
                $("#Attachment").show();
                $("#Attachment").attr("href", msg.ContractFilePath);
                debugger;
                $("#Attachment").attr("title", msg.ContractName);
            }
            var dropDownContent = '<div style="position: relative; margin-left: 8px; margin-top: 10px;">' + msg.PaymentInformation + '</div>';
            $("#PaymentInformation").val(msg.PaymentInformationVguid);
        });
    }
    //上传文件
    $("#ContractFileInput").on("change", function () {
        debugger;
        var filePath = this.value;
        var fileExt = filePath.substring(filePath.lastIndexOf("."))
            .toLowerCase();
        if (!checkFileExt(fileExt)) {
            jqxNotification("您上传的文件类型不允许,请重新上传！！", null, "error");
            this.value = "";
            return;
        } else {
            layer.load();
            $("#contractFormFile").ajaxSubmit({
                url: "/AssetPurchase/IntangibleAssetsOrderDetail/UploadContractFile",
                type: "post",
                data: {
                    'Vguid': $("#VGUID").val()
                },
                success: function (msg) {
                    layer.closeAll('loading');
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("上传失败！", null, "error");
                            $('#ContractFileInput').val('');
                            break;
                        case "1":
                            jqxNotification("上传成功！", null, "success");
                            $("#Attachment").show();
                            $("#Attachment").attr("href", msg.ResultInfo);
                            $("#Attachment").attr("title", msg.ResultInfo2);
                            $('#ContractFileInput').val('');
                            break;
                    }
                }
            });
        }
    })
    //上传文件
    $("#FileInput").on("change",
        function () {
            debugger;
            var filePath = this.value;
            var fileExt = filePath.substring(filePath.lastIndexOf("."))
                .toLowerCase();
            if (!checkFileExt(fileExt)) {
                jqxNotification("您上传的文件类型不允许,请重新上传！！", null, "error");
                this.value = "";
                return;
            } else {
                layer.load();
                $("#formFile").ajaxSubmit({
                    url: "/AssetPurchase/IntangibleAssetsOrderDetail/UploadLocalFile",
                    type: "post",
                    data: {
                        'VGUID': $("#FileInput").attr("cdata")
                    },
                    success: function (msg) {
                        layer.closeAll('loading');
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("上传失败！", null, "error");
                                $('#FileInput').val('');
                                break;
                            case "1":
                                jqxNotification("上传成功！", null, "success");
                                $('#FileInput').val('');
                                initTable()
                                break;
                        }
                    }
                });
            }
        });
    function checkFileExt(ext) {
        if (!ext.match(/.jpg|.png|.doc|.docx|.xls|.xlsx|.pdf|.bmp/i)) {
            return false;
        }
        return true;
    }
};
function computeValue() {
    if ($("#PurchasePrices").val() != "" && $("#OrderQuantity").val() != "") {
        var value = $("#PurchasePrices").val() * $("#OrderQuantity").val();
        $("#ContractAmount").val(value);
    }
}
function initSelect() {
    //使用部门
    $("#OrderType").prepend("<option value=\"\" selected='true'>请选择</>");
    $.ajax({
        url: "/AssetPurchase/FixedAssetsOrderDetail/GetCustomerBankInfoList",
        data: { "PurchaseOrderSetting": $("#PurchaseGoods").val() },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (data) {
            debugger;
            var source = new Array();
            for (var i = 0; i < data.Rows.length; i++) {
                var html = "<div style='padding: 0px; margin: 0px; height: 76px; float: left;'><div style='margin-top: 5px; font-size: 13px;'>"
                    + "<b>户名</b><div>" + data.Rows[i].BankAccountName + "</div><div style='margin-top: 5px;'><b>账号</b><div>" + data.Rows[i].BankAccount + "</div></div></div>";
                source[i] = { html: html, title: data.Rows[i].BankAccountName, value: data.Rows[i].VGUID };
            }
            var dataAdapter = new $.jqx.dataAdapter(source);
            $("#PaymentInformation").jqxComboBox({
                source: dataAdapter, selectedIndex: 0,
                displayMember: "title", valueMember: "value",
                width: 198, height: 33
            });

        }
    });
}

function formatDate(NewDtime) {
    var dt = new Date(parseInt(NewDtime.slice(6, 19)));
    var year = dt.getFullYear();
    var month = dt.getMonth() + 1;
    var date = dt.getDate();
    var hour = dt.getHours();
    var minute = dt.getMinutes();
    var second = dt.getSeconds();
    return year + "-" + month + "-" + date + " ";
    //return year + "-" + month + "-" + date + " " + hour + ":" + minute + ":" + second;
}

$(function () {
    var page = new $page();
    page.init();
});

function isNumber(val) {
    var regPos = /^\d+(\.\d+)?$/; //非负浮点数
    var regNeg = /^(-(([0-9]+\.[0-9]*[1-9][0-9]*)|([0-9]*[1-9][0-9]*\.[0-9]+)|([0-9]*[1-9][0-9]*)))$/; //负浮点数
    if (regPos.test(val) || regNeg.test(val)) {
        return true;
    } else {
        return false;
    }
}