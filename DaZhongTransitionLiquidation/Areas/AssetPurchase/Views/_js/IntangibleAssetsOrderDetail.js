//无形资产订单维护明细
var vguid = "";
var mydate = new Date();
var vehicleDefaultData;
//拍照数据（base64）
var baseUrl = "ws://127.0.0.1:12345";
var socket;
var $page = function () {
    this.init = function () {
        initSelect();
        initSelectPurchaseGoods();
        initPaymentInformationComboBox();
        initPayCompanyDropdown();
        initSelectPurchaseDepartment();
        addEvent();
        $("#PaymentInformation").find("input")[0].setAttribute("style", "box-sizing: border-box;margin: 0px;border: 0px;width: 100%;height: 33px;");
    }
    //所有事件
    function addEvent() {
        var guid = $.request.queryString().VGUID;
        debugger;
        if (guid != "" && guid != null) {
            $("#VGUID").val(guid);
            $("#btnSave").parent().hide();
            getIntangibleAssetsOrderDetail();
            getAttachment();
        } else {
            $("#btnPrint").parent().hide();
            $("#VGUID").val(newguid());
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
                if (!Validate($("#PurchaseGoods"))) {
                    validateError++;
                }
                //尾款不能为0
                if (!Validate($("#TailPayment")) && $("#TailPayment").val() != "0") {
                    validateError++;
                }
                if (validateError <= 0) {
                    var checkedItems = $("#PurchaseDepartment").jqxDropDownList('getCheckedItems');
                    var DepartmentModelList = [];
                    for (var i = 0; i < checkedItems.length; i++) {
                        DepartmentModelList.push(checkedItems[i].value);
                    };
                    $.ajax({
                        url: "/AssetPurchase/IntangibleAssetsOrderDetail/SaveIntangibleAssetsOrder",
                        data: {
                            "VGUID": $("#VGUID").val(),
                            "PurchaseGoods": $("#PurchaseGoods option:selected").text(),
                            "PurchaseDepartmentIDs": DepartmentModelList.join(","),
                            "PurchaseGoodsVguid": $("#PurchaseGoods").val(),
                            "AssetDescription": $("#AssetDescription").val(),
                            "PaymentInformationVguid": $("#hiddenPaymentInformationVguid").val(),
                            "PaymentInformation": $("#hiddenPaymentInformation").val(),
                            "SumPayment": $("#SumPayment").val(),
                            "InterimPayment": $("#InterimPayment").val(),
                            "FirstPayment": $("#FirstPayment").val(),
                            "TailPayment": $("#TailPayment").val(),
                            "ContractAmount": $("#ContractAmount").val(),
                            "SupplierInformation": $("#SupplierInformation").val(),
                            "ContractName": $("#Attachment").attr("title"),
                            "ContractFilePath": $("#Attachment").attr("href"),
                            "PayCompany": $("#PayCompanyDropdown").text(),
                            "SupplierBankAccountName": $("#BankAccountName").val(),
                            "SupplierBankAccount": $("#BankAccount").val(),
                            "SupplierBank": $("#Bank").val(),
                            "SupplierBankNo": $("#BankNo").val(),
                            "PayType": $("#PayMode").val(),
                            "CompanyBankName": $("#CompanyBankName").val(),
                            "CompanyBankAccount": $("#CompanyBankAccount").val(),
                            "CompanyBankAccountName": $("#CompanyBankAccountName").val(),
                            "AccountType": $("#AccountType").val(),
                            "PayCompanyVguid": $("#PayCompanyDropdown").val()
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
        //$("#OrderQuantity").on("click",
        //    function () {
        //        initTable();
        //    });
        $("#UploadContractFile").on("click",
            function () {
                $("#ContractFileInput").click();
            });
        //拍照
        $(".camera").on("click",
            function () {
                $("#AttachmentType").val($(this).attr("AttachmentType"));
                baseUrl = "ws://127.0.0.1:12345";
                openSocket();
                $("#devPhoto").hide();
                $("#Upload_OKBtn").hide();
                $("#photographPri").show();
                //$("#AcceptDialog").modal("hide");
                //showLoading();
                $("#UploadPictureDialog").modal({ backdrop: "static", keyboard: false });
                $("#UploadPictureDialog").modal("show");
            });
        //本地上传
        $(".upload").on("click",
            function () {
                $("#AttachmentType").val($(this).attr("AttachmentType"));
                $("#LocalFileInput").click();
            });
        $("#photographPri").on("click",
           function () {
               $("#Upload_OKBtn").show();
               $("#photographPri").hide();
               $("#devPhoto").show();
           });
        $("#Upload_OKBtn").on("click", function () {
            //$('#jqxLoader').jqxLoader('open');
            debugger;
            if ($("#devPhoto").attr("src") != undefined) {
                layer.load();
                $.ajax({
                    url: "/AssetPurchase/FixedAssetsOrderDetail/UploadToImageServer",
                    data: {
                        "Vguid": $("#VGUID").val(),
                        "ImageBase64Str": $("#devPhoto").attr("src"),
                        "AttachmentType": $("#AttachmentType").val()
                    },
                    type: "post",
                    success: function (msg) {
                        $('#jqxLoader').jqxLoader('close');
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("上传失败！", null, "error");
                                layer.closeAll('loading');
                                break;
                            case "1":
                                jqxNotification("上传成功！", null, "success");
                                //上传成功后调用清算平台、付款凭证附件上传接口
                                var guid = $.request.queryString().VGUID;
                                if (guid != "" && guid != null) {
                                    PendingPaymentAttachmentUpload();
                                }
                                layer.closeAll('loading');
                                getAttachment();
                                $("#UploadPictureDialog").modal("hide");
                                break;
                        }
                    }
                });
            } else {
                jqxNotification("未拍照！", null, "error");
            }
        });
        $("#Upload_CancelBtn").on("click", function () {
            $("#UploadPictureDialog").modal("hide");
        });
        //确定
        $("#btnPrint").on("click",
            function () {
                debugger;
                if ($("#ifrPrint").attr("src") != undefined) {
                    document.getElementById('ifrPrint').src = $("#ifrPrint").attr("src");
                }
                $("#CreditDialog").modal("show");
            });
        $("#CreditDialog_OKBtn").on("click",
            function () {
                $("#CreditDialog").modal("hide");
            }
        );
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
        //计算金额
        $("#FirstPayment").on("blur",
            function () {
                debugger;
                computeValue();
            });
        $("#InterimPayment").on("blur",
            function () {
                debugger;
                computeValue();
            });
        $('#PayCompanyDropdown').on('select', function (event) {
            debugger;
            $("#CompanyBankName").val("");
            $("#CompanyBankAccount").val("");
            $("#CompanyBankAccountName").val("");
            $("#AccountType").val("");
            var args = event.args;
            if (args && $("#PayMode").val() != "现金") {
                var item = args.item;
                $("#PayCompany").val(item.label);
                $.post("/AssetPurchase/FixedAssetsOrderDetail/GetCompanyBankInfo",
                    { Vguid: $("#PayCompanyDropdown").val() },
                    function (msg) {
                        debugger;
                        $("#CompanyBankName").val(msg.PayBank);
                        $("#CompanyBankAccount").val(msg.PayAccount);
                        $("#CompanyBankAccountName").val(msg.PayBankAccountName);
                        $("#AccountType").val(msg.AccountType);
                    });
            }
        });
        $('#PaymentInformation').on('select', function (event) {
            var args = event.args;
            if (args) {
                var item = args.item;
                $("#hiddenPaymentInformationVguid").val(item.value);
                $("#hiddenPaymentInformation").val(item.label);
                $.post("/AssetPurchase/FixedAssetsOrderDetail/GetCustomerBankInfo", { vguid: item.value }, function (msg) {
                    $("#BankAccountName").val(msg.BankAccountName);
                    $("#BankAccount").val(msg.BankAccount);
                    $("#Bank").val(msg.Bank);
                    $("#BankNo").val(msg.BankNo);
                });
            }
        });
        $("#PurchaseDepartment").on('checkChange', function (event) {
            if (event.args) {
                var checkedItems = $("#PurchaseDepartment").jqxDropDownList('getCheckedItems');
                var DepartmentModelList = [];
                for (var i = 0; i < checkedItems.length; i++) {
                    DepartmentModelList.push(checkedItems[i].value);
                };
                initSelectPurchaseGoods(DepartmentModelList);
            }
        });
        $("#PayMode").on("change",
            function () {
                if ($("#PayMode").val() == "现金") {
                    $("#CompanyBankName").val("");
                    $("#CompanyBankAccount").val("");
                    $("#CompanyBankAccountName").val("");
                    $("#AccountType").val("");
                }
            });
        $("#PurchaseGoods").on("change",
             function () {
                 initPaymentInformationComboBox();
                 $("#PurchaseDepartment").jqxDropDownList({ disabled: true });
                 $("#PurchaseGoods").attr("disabled", true);
                 getCompanyBankInfoDropdownByCode();
             });
    }; //addEvent end
    function getCompanyBankInfoDropdownByCode() {
        var url = "/AssetPurchase/FixedAssetsOrderDetail/GetCompanyBankInfoDropdownByCode";
        var source =
        {
            datatype: "json",
            datafields: [
                { name: 'VGUID' },
                { name: 'CompanyName' }
            ],
            url: url,
            data: { "PurchaseOrderSetting": $("#PurchaseGoods").val() },
            async: false
        };
        var dataAdapter = new $.jqx.dataAdapter(source);
        debugger;
        $('#PayCompanyDropdown').jqxDropDownList({
            enableSelection: true,
            filterable: true, selectedIndex: 0, source: dataAdapter, displayMember: "CompanyName", dropDownWidth:
                310, filterHeight: 30, valueMember: "VGUID", itemHeight: 30, height: 33, width: 200, searchMode: 'contains',
            renderer: function (index, label, value) {
                var table = '<table style="min-width: 130px;height:30px"><tr><td>' + label + '</td></tr></table>';
                return table;
            },
            selectionRenderer: function (element, index, label, value) {
                var text = label.replace(/\n/g, " ");
                return "<span style='left: 5px; top: 6px; position: relative;'>" + text + "</span>";
            }
        });
    }
    function getIntangibleAssetsOrderDetail() {
        $.post("/AssetPurchase/IntangibleAssetsOrderDetail/GetIntangibleAssetsOrder", { vguid: $("#VGUID").val() }, function (msg) {
            debugger;
            var PurchaseDepartment = msg.PurchaseDepartmentIDs.split(",");
            for (var i = 0; i < PurchaseDepartment.length; i++) {
                var item = $("#PurchaseDepartment").jqxDropDownList('getItemByValue', PurchaseDepartment[i]);
                $("#PurchaseDepartment").jqxDropDownList('checkItem', item);
            }
            $("#PurchaseDepartment").jqxDropDownList({ disabled: true });
            $("#PurchaseGoods").attr("disabled", true);
            $("#PurchaseGoods").val(msg.PurchaseGoodsVguid);
            initPaymentInformationComboBox();
            getCompanyBankInfoDropdownByCode();
            $("#hiddenPaymentInformationVguid").val(msg.PaymentInformationVguid);
            $("#hiddenPaymentInformation").val(msg.PaymentInformation);
            $("#AssetDescription").val(msg.AssetDescription);
            $("#SumPayment").val(msg.SumPayment);
            $("#FirstPayment").val(msg.FirstPayment);
            $("#TailPayment").val(msg.TailPayment);
            $("#SupplierInformation").val(msg.SupplierInformation);
            $("#ContractName").val(msg.ContractName);
            if (msg.ContractName != null && msg.ContractFilePath != null) {
                $("#Attachment").show();
                $("#Attachment").attr("href", msg.ContractFilePath);
                $("#Attachment").attr("title", msg.ContractName);
            }
            $("#SupplierBankAccountName").val(msg.SupplierBankAccountName);
            $("#SupplierBankAccount").val(msg.SupplierBankAccount);
            $("#SupplierBank").val(msg.SupplierBank);
            $("#SupplierBankNo").val(msg.SupplierBankNo);
            $("#PayMode").val(msg.PayType);
            $("#PayCompanyDropdown").val(msg.PayCompanyVguid);
            $("#CompanyBankName").val(msg.CompanyBankName);
            $("#InterimPayment").val(msg.InterimPayment);
            $("#CompanyBankAccount").val(msg.CompanyBankAccount);
            $("#CompanyBankAccountName").val(msg.CompanyBankAccountName);
            $("#hidPayCompany").val(msg.PayCompany);
            $("#AccountType").val(msg.AccountType);
            $("#PaymentInformation").val(msg.PaymentInformationVguid);
            $("#ifrPrint").attr("src", msg.PaymentVoucherUrl);
            $("#PaymentVoucherVguid").val(msg.PaymentVoucherVguid);
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
                                //上传成功后调用清算平台、付款凭证附件上传接口
                                var guid = $.request.queryString().VGUID;
                                if (guid != "" && guid != null) {
                                    PendingPaymentAttachmentUpload();
                                }
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
    if ($("#SumPayment").val() != "" && $("#FirstPayment").val() != "" && $("#InterimPayment").val() != "") {
        var value = $("#SumPayment").val() - $("#FirstPayment").val() - $("#InterimPayment").val();
        $("#TailPayment").val(value);
    } else if ($("#SumPayment").val() != "" && $("#FirstPayment").val()) {
        $("#TailPayment").val($("#SumPayment").val() - $("#FirstPayment").val());
    }
}
function PendingPaymentAttachmentUpload() {
    $.ajax({
        url: "/AssetPurchase/IntangibleAssetsOrderDetail/PendingPaymentAttachmentUpload",
        data: { "PaymentVoucherVguid": $("#PaymentVoucherVguid").val(), "Vguid": $("#VGUID").val() },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            switch (msg.Status) {
            case "0":
                jqxNotification("调用接口失败！", null, "error");
                break;
            }
        }
    });
}
function initSelectPurchaseDepartment() {
    var source =
    {
        datatype: "json",
        type: "post",
        datafields: [
            { name: 'Descrption' },
            { name: 'VGUID' }
        ],
        url: "/Systemmanagement/PurchaseOrderSettingDetail/GetPurchaseDepartmentListDatas",
        async: false
    };
    var dataAdapter = new $.jqx.dataAdapter(source);
    $("#PurchaseDepartment").jqxDropDownList({ checkboxes: true, selectedIndex: 0, placeHolder: "请选择", source: dataAdapter, displayMember: "Descrption", valueMember: "VGUID", width: 192, height: 33 });
    $("#PurchaseDepartment").jqxDropDownList({ itemHeight: 33 });
}

function initSelectPurchaseGoods(PurchaseDepartment) {
    //使用部门
    $.ajax({
        url: "/AssetPurchase/IntangibleAssetsOrderDetail/GetPurchaseGoods",
        data: { "OrderCategory": 1, "PurchaseDepartment": PurchaseDepartment },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#PurchaseGoods', msg, "VGUID", "PurchaseGoods");
            $("#PurchaseGoods").prepend("<option value=\"\" selected='true'>请选择</>");
            debugger;
        }
    });
}
//统一上传文件
$("#LocalFileInput").on("change",
    function () {

        var filePath = this.value;
        var fileExt = filePath.substring(filePath.lastIndexOf("."))
            .toLowerCase();
        if (!checkFileExt(fileExt)) {
            jqxNotification("您上传的文件类型不允许,请重新上传！！", null, "error");
            this.value = "";
            return;
        } else {
            layer.load();

            $("#localFormFile").ajaxSubmit({
                url: "/AssetPurchase/FixedAssetsOrderDetail/AllUploadLocalFile",
                type: "post",
                data: {
                    'Vguid': $("#VGUID").val(),
                    'AttachmentType': $("#AttachmentType").val()
                },
                success: function (msg) {
                    layer.closeAll('loading');
                    switch (msg.Status) {
                    case "0":
                        jqxNotification("上传失败！", null, "error");
                        $('#LocalFileInput').val('');
                        break;
                    case "1":
                        jqxNotification("上传成功！", null, "success");
                        $('#LocalFileInput').val('');
                        getAttachment();
                        break;
                    }
                }
            });
        }
    });
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
                searchMode: 'contains',
                width: 198, height: 33
            });
            $.post("/AssetPurchase/FixedAssetsOrderDetail/GetCustomerBankInfo", { vguid: $("#PaymentInformation").val() }, function (msg) {
                $("#BankAccountName").val(msg.BankAccountName);
                $("#BankAccount").val(msg.BankAccount);
                $("#Bank").val(msg.Bank);
                $("#BankNo").val(msg.BankNo);
            });
        }
    });
}

function initPayCompanyDropdown() {
    $('#PayCompanyDropdown').jqxDropDownList({
        disabled:false,
        filterable: true, selectedIndex: 2, source: null, displayMember: "Descrption", dropDownWidth:
            310, filterHeight: 30, valueMember: "VGUID", itemHeight: 30, height: 33, width: 200, searchMode: 'contains',
        renderer: function (index, label, value) {
            var table = '<table style="min-width: 130px;height:30px"><tr><td>' + label + '</td></tr></table>';
            return table;
        },
        selectionRenderer: function (element, index, label, value) {
            var text = label.replace(/\n/g, " ");
            return "<span style='left: 5px; top: 6px; position: relative;'>" + text + "</span>";
        }
    });
}
function initPaymentInformationComboBox() {
    //付款单位及相关账户信息
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
function getAttachment() {
    $.ajax({
        url: "/AssetPurchase/FixedAssetsOrderDetail/GetAttachmentInfo",
        data: {
            "VGUID": $("#VGUID").val()
        },
        type: "post",
        dataType: "json",
        success: function (msg) {
            $("#ImgPaymentReceipt").html("");
            $("#ImgInvoiceReceipt").html("");
            $("#ImgApprovalReceipt").html("");
            $("#ImgContract").html("");
            $("#ImgDetailList").html("");
            $("#ImgOtherReceipt").html("");
            for (var i = 0; i < msg.length; i++) {
                debugger;
                var num;
                var fileName = "";
                var fileType = "";
                if (msg[i].Attachment.indexOf("https") != -1) {
                    num = msg[i].Attachment.split("/").length - 1;
                    fileName = msg[i].Attachment.split("/")[num];
                    fileType = fileName.split(".")[1];
                } else {
                    num = msg[i].Attachment.lastIndexOf("\\") + 1;
                    fileName = msg[i].Attachment.substring(num, msg[i].Attachment.length);
                    fileType = fileName.split(".")[1];
                }
                var ext = "." + fileType.toUpperCase();
                var html = "";
                if (ext != ".BMP" && ext != ".PNG" && ext != ".GIF" && ext != ".JPG" && ext != ".JPEG") {
                    html = '<div class="AttachmentDiv"><img src="/_theme/images/' + fileType + '.png" style="width: 25px;height: 25px;margin: 3px;" /><a href="' + msg[i].Attachment + '" target="_blank">' + fileName + '</a>&nbsp&nbsp&nbsp<a id=' + msg[i].VGUID + ' onclick="delAttachment(\'' + msg[i].VGUID + '\')" class="delAttachment">删除</a></div>';
                } else {
                    html = '<div class="AttachmentDiv"><img src="/_theme/images/Picture.png" style="width: 25px;height: 25px;margin: 3px;" /><a href="' + msg[i].Attachment + '" target="_blank">' + fileName + '</a>&nbsp&nbsp&nbsp<a id=' + msg[i].VGUID + ' onclick="delAttachment(\'' + msg[i].VGUID + '\')" class="delAttachment">删除</a></div>';
                }

                switch (msg[i].AttachmentType) {
                    case "付款凭证": $("#ImgPaymentReceipt").append(html);
                        break;
                    case "发票": $("#ImgInvoiceReceipt").append(html);
                        break;
                    case "OA审批单": $("#ImgApprovalReceipt").append(html);
                        break;
                    case "合同": $("#ImgContract").append(html);
                        break;
                    case "清单、清册": $("#ImgDetailList").append(html);
                        break;
                    case "其他": $("#ImgOtherReceipt").append(html);
                        break;
                    default:
                }
            }
        }
    });
}
function delAttachment(delID) {
    $.ajax({
        url: "/AssetPurchase/FixedAssetsOrderDetail/DeleteAttachment",
        data: { VGUID: delID },
        traditional: true,
        type: "post",
        success: function (msg) {
            switch (msg.Status) {
            case "0":
                jqxNotification("删除失败！", null, "error");
                break;
            case "1":
                jqxNotification("删除成功！", null, "success");
                getAttachment();
                break;
            }
        }
    });
}
//拍照
function openSocket() {

    socket = new WebSocket(baseUrl);
    socket.onclose = function () {
        console.error("web channel closed");
    };
    socket.onerror = function (error) {
        console.error("web channel error: " + error);
    };
    socket.onopen = function () {
        new QWebChannel(socket, function (channel) {
            // make dialog object accessible globally
            window.dialog = channel.objects.dialog;
            //dialog.set_configValue("set_savePath:D:\\img");
            //网页关闭函数
            window.onbeforeunload = function () {
                dialog.get_actionType("closeSignal");
            }
            window.onunload = function () {
                dialog.get_actionType("closeSignal");
            }
            //拍照按钮点击
            document.getElementById("photographPri").onclick = function () {
                dialog.photoBtnClicked("primaryDev_");
                dialog.get_actionType("savePhotoPriDev");
            };
            //纠偏裁边
            document.getElementById("setdeskew").onclick = function () {
                dialog.get_actionType("setdeskew");
            };

            //服务器返回消息
            dialog.sendPrintInfo.connect(function (message) {
                //设备分辨率
                message = message.substr(14);
                //图片保存后返回路径关键字savePhoto_success:
                if (message.indexOf("savePhoto_success:") >= 0) {
                    imgPath = message.substr(18);
                }
            });
            //接收图片流用来展示，多个，较小的base64数据
            dialog.send_priImgData.connect(function (message) {
                var element = document.getElementById("bigPriDev");
                element.src = "data:image/jpg;base64," + message;
            });
            //接收拍照base64
            dialog.send_priPhotoData.connect(function (message) {
                var element = document.getElementById("devPhoto");
                element.src = "data:image/jpg;base64," + message;
            });
            //网页加载完成信号
            dialog.html_loaded("one");
        });
    }
}
function checkFileExt(ext) {
    if (!ext.match(/.jpg|.png|.doc|.docx|.xls|.xlsx|.pdf|.bmp/i)) {
        return false;
    }
    return true;
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
//生成guid
function newguid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
function isNumber(val) {
    var regPos = /^\d+(\.\d+)?$/; //非负浮点数
    var regNeg = /^(-(([0-9]+\.[0-9]*[1-9][0-9]*)|([0-9]*[1-9][0-9]*\.[0-9]+)|([0-9]*[1-9][0-9]*)))$/; //负浮点数
    if (regPos.test(val) || regNeg.test(val)) {
        return true;
    } else {
        return false;
    }
}