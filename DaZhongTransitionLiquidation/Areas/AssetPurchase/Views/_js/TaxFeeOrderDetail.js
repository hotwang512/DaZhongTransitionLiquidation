//固定资产订单维护明细
var vguid = "";
var mydate = new Date();
var vehicleDefaultData;
//拍照数据（base64）
var baseUrl = "ws://127.0.0.1:12345";
var socket;
var $page = function () {
    this.init = function () {
        //initSelect();
        initSelectPayItem();
        GetVehicleModelDropDown();
        initPaymentInformationComboBox();
        initPayCompanyDropdown();
        initSelectPurchaseDepartment();
        InitSelectPurchaseOrderNum();
        addEvent();
        $("#PaymentInformation").find(".jqx-combobox-input")[0].setAttribute("style", "box-sizing: border-box;margin: 0px;border: 0px;width: 100%;height: 33px;");
    }
    //所有事件
    function addEvent() {
        var guid = $.request.queryString().VGUID;
        var paymentVoucherVguid = $.request.queryString().PaymentVoucherVguid;
        debugger;
        $("#VGUID").val(guid);
        if (guid != "" && guid != null) {
            if (paymentVoucherVguid != null && paymentVoucherVguid != "") {
                debugger;
                $("#btnSave").parent().hide();
                $("#tdPrint").show();
            }
            getOrderDetail();
            getAttachment();
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
                if (!Validate($("#PayItem"))) {
                    validateError++;
                }
                if (validateError <= 0) {
                    var checkedItems = $("#PurchaseDepartment").jqxDropDownList('getCheckedItems');
                    var DepartmentModelList = [];
                    for (var i = 0; i < checkedItems.length; i++) {
                        DepartmentModelList.push(checkedItems[i].value);
                    };
                    var checkedOrderNumItems = $("#OrderQuantity").jqxDropDownList('getCheckedItems');
                    var OrderNumList = [];
                    for (var k = 0; k < checkedOrderNumItems.length; k++) {
                        OrderNumList.push(checkedOrderNumItems[k].value);
                    };
                    var pvguid = $("#PaymentInformation").val();
                    var paymentInformation = $("#PaymentInformation").jqxComboBox('getItemByValue', pvguid);
                    debugger;
                    $.ajax({
                        url: "/AssetPurchase/TaxFeeOrderDetail/SaveTaxFeeOrder",
                        data: {
                            "VGUID": $("#VGUID").val(),
                            "PayItem": $("#PayItem option:selected").text(),
                            "PurchaseDepartmentIDs": DepartmentModelList.join(","),
                            "PayItemCode": $("#PayItem").val(),
                            "VehicleModel": $("#VehicleModel option:selected").text(),
                            "VehicleModelCode": $("#VehicleModel").val(),
                            "OrderNumData": OrderNumList.join(","),
                            "OrderQuantity": $("#SumQuantity").val(),
                            "UnitPrice": $("#PurchasePrices").val(),
                            "SumPayment": $("#ContractAmount").val(),
                            "PurchaseDescription": $("#AssetDescription").val(),
                            "PaymentInformationVguid": $("#PaymentInformation").val(),
                            "PayCompanyVguid": $("#PayCompanyDropdown").val(),
                            "PaymentInformation": paymentInformation.label,
                            "PaymentDate": $("#PaymentDate").val(),
                            "PayCompany": $("#PayCompanyDropdown").text(),
                            "SupplierBankAccountName": $("#BankAccountName").val(),
                            "SupplierBankAccount": $("#BankAccount").val(),
                            "SupplierBank": $("#Bank").val(),
                            "SupplierBankNo": $("#BankNo").val(),
                            "PayType": $("#PayMode").val(),
                            "CompanyBankName": $("#CompanyBankName").val(),
                            "CompanyBankAccount": $("#CompanyBankAccount").val(),
                            "CompanyBankAccountName": $("#CompanyBankAccountName").val(),
                            "AccountType": $("#AccountType").val()
                        },
                        type: "post",
                        success: function (msg) {
                            switch (msg.Status) {
                                case "0":
                                    jqxNotification("保存失败！", null, "error");
                                    break;
                                case "1":
                                    jqxNotification("保存成功！", null, "success");
                                    //$("#btnSave").parent().hide();
                                    //$("#tdPrint").show();
                                    //$("#btnPrint").trigger("click");
                                    history.go(-1);
                                    window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                                    break;
                            }
                        }
                    });
                }
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
        $("#CreditDialog_OKBtn").on("click",
            function () {
                $("#CreditDialog").modal("hide");
            }
        );
        //打印
        $("#btnPrint").on("click",
            function () {
                document.getElementById('ifrPrint').src = $("#ifrPrint").attr("src");
                $("#CreditDialog").modal("show");
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
            
            if ($("#devPhoto").attr("src") != undefined) {
                layer.load();
                $.ajax({
                    url: "/AssetPurchase/TaxFeeOrderDetail/UploadToImageServer",
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
        //提交
        $("#btnSubmit").on("click",
            function () {
                $.post("/AssetPurchase/TaxFeeOrderDetail/SubmitTaxFeeOrder", { vguid: $("#VGUID").val() }, function (msg) {
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
            });
        $('#PaymentInformation').on('select', function (event) {
            var args = event.args;
            if (args) {
                var item = args.item;
                $("#hiddenPaymentInformationVguid").val(item.value);
                $("#hiddenPaymentInformation").val(item.label);
                $.post("/AssetPurchase/TaxFeeOrderDetail/GetCustomerBankInfo", { vguid: item.value }, function (msg) {
                    $("#BankAccountName").val(msg.BankAccountName);
                    $("#BankAccount").val(msg.BankAccount);
                    $("#Bank").val(msg.Bank);
                    $("#BankNo").val(msg.BankNo);
                });
            }
        });
        $('#PayCompanyDropdown').on('select', function (event) {
            $("#CompanyBankName").val("");
            $("#CompanyBankAccount").val("");
            $("#CompanyBankAccountName").val("");
            $("#AccountType").val("");
            var args = event.args;
            if (args && $("#PayMode").val() != "现金") {
                var item = args.item;
                $("#PayCompany").val(item.label);
                $.post("/AssetPurchase/TaxFeeOrderDetail/GetCompanyBankInfo",
                    { Vguid: $("#PayCompanyDropdown").val() },
                    function (msg) {
                        
                        $("#CompanyBankName").val(msg.PayBank);
                        $("#CompanyBankAccount").val(msg.PayAccount);
                        $("#CompanyBankAccountName").val(msg.PayBankAccountName);
                        $("#AccountType").val(msg.AccountType);
                    });
            }
        });
        $("#PayItem").on("change",
            function () {
                initPaymentInformationComboBox();
                $("#PurchaseDepartment").jqxDropDownList({ disabled: true });
                //$("#PayItem").attr("disabled", true);
                GetCompanyBankInfoDropdownByCode();
                GetSelectPurchaseOrderNum();
            });
        $("#PurchasePrices").on("change",
            function () {
                computeValue();
            });
        $("#VehicleModel").on("change",
            function () {
                GetFeeByVehicleModel();
            });
        $("#PurchaseDepartment").on('checkChange', function (event) {
            if (event.args) {
                var checkedItems = $("#PurchaseDepartment").jqxDropDownList('getCheckedItems');
                var DepartmentModelList = [];
                for (var i = 0; i < checkedItems.length; i++) {
                    DepartmentModelList.push(checkedItems[i].value);
                };
                initSelectPayItem(DepartmentModelList);
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
        $("#jqxLoader").jqxLoader({ isModal: true, width: 100, height: 60, imagePosition: 'top' });
    }; //addEvent end
    function GetPurchaseOrderNumDetail() {
        debugger;
        var source =
        {
            datatype: "json",
            type: "post",
            datafields: [
                { name: 'OrderDesc' },
                { name: 'FixedAssetsOrderVguid' }
            ],
            data: { PayItemCode: $("#PayItem").val(), TaxFeeOrderVguid: $("#VGUID").val() },
            url: "/AssetPurchase/TaxFeeOrderDetail/GetPurchaseOrderNumDetail",
            async: false
        };
        var dataAdapter = new $.jqx.dataAdapter(source);
        $("#OrderQuantity").jqxDropDownList({ checkboxes: true, selectedIndex: 0, disabled: true, placeHolder: "请选择", source: dataAdapter, displayMember: "OrderDesc", valueMember: "FixedAssetsOrderVguid", width: 192, height: 33 });
        $("#OrderQuantity").jqxDropDownList({ itemHeight: 33 });
        $("#OrderQuantity").jqxDropDownList('checkAll');
    }
    function GetSelectPurchaseOrderNum() {
        var source =
        {
            datatype: "json",
            type: "post",
            datafields: [
                { name: 'OrderDesc' },
                { name: 'FixedAssetsOrderVguid' }
            ],
            data: { PayItemCode: $("#PayItem").val() },
            url: "/AssetPurchase/TaxFeeOrderDetail/GetPurchaseOrderNum",
            async: false
        };
        var dataAdapter = new $.jqx.dataAdapter(source);
        $("#OrderQuantity").jqxDropDownList({ checkboxes: true, selectedIndex: 0, placeHolder: "请选择", source: dataAdapter, displayMember: "OrderDesc", valueMember: "FixedAssetsOrderVguid", width: 192, height: 33 });
        $("#OrderQuantity").jqxDropDownList({ itemHeight: 33 });
    }
    function InitSelectPurchaseOrderNum() {
        $("#OrderQuantity").jqxDropDownList({ checkboxes: true, selectedIndex: 0, placeHolder: "请选择", source: null, displayMember: "OrderDesc", valueMember: "FixedAssetsOrderVguid", width: 192, height: 33 });
        $("#OrderQuantity").jqxDropDownList({ itemHeight: 33 });
    }
    function GetCompanyBankInfoDropdownByCode() {
        var url = "/AssetPurchase/TaxFeeOrderDetail/GetCompanyBankInfoDropdownByCode";
        var source =
        {
            datatype: "json",
            datafields: [
                { name: 'VGUID' },
                { name: 'CompanyName' }
            ],
            url: url,
            data: { "PayItem": $("#PayItem").val() },
            async: false
        };
        var dataAdapter = new $.jqx.dataAdapter(source);
        
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
    function GetFeeByVehicleModel() {
        if ($("#PayItem").val() != "" && $("#VehicleModel").val() != "") {
            $.ajaxSettings.async = false;
            $.post("/AssetPurchase/TaxFeeOrderDetail/GetFeeByVehicleModel", { PayItem: $("#PayItem").val(), VehicleModel: $("#VehicleModel").val() },
                function (msg) {
                    uiEngineHelper.bindSelect('#PurchasePrices', msg, "Fee", "Fee");
                    $("#PurchasePrices").prepend("<option value=\"\" selected='true'>请选择</>");
                });
        }
    }
    function checkFileExt(ext) {
        if (!ext.match(/.jpg|.png|.jpeg|.doc|.docx|.xls|.xlsx|.pdf|.bmp/i)) {
            return false;
        }
        return true;
    }
    function getOrderDetail() {
        $.post("/AssetPurchase/TaxFeeOrderDetail/GetTaxFeeOrder", { vguid: $("#VGUID").val() }, function (msg) {
            if (msg.PurchaseDepartmentIDs != null) {
                var PurchaseDepartment = msg.PurchaseDepartmentIDs.split(",");
                for (var i = 0; i < PurchaseDepartment.length; i++) {
                    var item = $("#PurchaseDepartment").jqxDropDownList('getItemByValue', PurchaseDepartment[i]);
                    $("#PurchaseDepartment").jqxDropDownList('checkItem', item);
                }
            }
            $("#PurchaseDepartment").jqxDropDownList({ disabled: true });
            initSelectPayItem();
            $("#PayItem").val(msg.PayItemCode);
            $("#VehicleModel").val(msg.VehicleModelCode);
            GetFeeByVehicleModel();
            initPaymentInformationComboBox();
            GetCompanyBankInfoDropdownByCode();
            $("#hiddenPaymentInformationVguid").val(msg.PaymentInformationVguid);
            $("#hiddenPaymentInformation").val(msg.PaymentInformation);
            $("#OrderQuantity").val(msg.OrderQuantity);
            debugger;
            $("#PurchasePrices").val(msg.UnitPrice);
            $("#ContractAmount").val(msg.SumPayment);
            $("#AssetDescription").val(msg.PurchaseDescription);
            //$("#UseDepartment").val(msg.UseDepartmentVguid);

            //if (msg.AcceptanceDate != null && msg.AcceptanceDate != "") {
            //    $("#AcceptanceDate").val(formatDate(msg.AcceptanceDate));
            //}
            if (msg.PaymentDate != null && msg.PaymentDate != "") {
                $("#PaymentDate").val(formatDate(msg.PaymentDate));
            }
            $("#ContractName").val(msg.ContractName);
            if (msg.ContractName != null && msg.ContractFilePath != null) {
                $("#Attachment").show();
                $("#Attachment").attr("href", msg.ContractFilePath);

                $("#Attachment").attr("title", msg.ContractName);
            }
            $("#ContractFilePath").val(msg.ContractFilePath);
            $("#SupplierBankAccountName").val(msg.SupplierBankAccountName);
            $("#SupplierBankAccount").val(msg.SupplierBankAccount);
            $("#SupplierBank").val(msg.SupplierBank);
            $("#SupplierBankNo").val(msg.SupplierBankNo);
            $("#PayMode").val(msg.PayType);
            $("#PayCompanyDropdown").val(msg.PayCompanyVguid);
            $("#CompanyBankName").val(msg.CompanyBankName);
            $("#CompanyBankAccount").val(msg.CompanyBankAccount);
            $("#CompanyBankAccountName").val(msg.CompanyBankAccountName);
            $("#hidPayCompany").val(msg.PayCompany);
            $("#AccountType").val(msg.AccountType);
            $("#PaymentInformation").val(msg.PaymentInformationVguid);
            $("#ifrPrint").attr("src", msg.PaymentVoucherUrl);
            $("#PaymentVoucherVguid").val(msg.PaymentVoucherVguid);
            GetPurchaseOrderNumDetail();
        });
    }
    //采购合同上传文件
    $("#ContractFileInput").on("change",
        function() {
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
                    url: "/AssetPurchase/TaxFeeOrderDetail/UploadContractFile",
                    type: "post",
                    data: {
                        'Vguid': $("#VGUID").val()
                    },
                    success: function(msg) {
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
        });
    //采购数量中OA审批单上传文件
    $("#FileInput").on("change",
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
                $("#formFile").ajaxSubmit({
                    url: "/AssetPurchase/TaxFeeOrderDetail/UploadLocalFile",
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
                    url: "/AssetPurchase/TaxFeeOrderDetail/AllUploadLocalFile",
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
                                //上传成功后调用清算平台、付款凭证附件上传接口
                                var guid = $.request.queryString().VGUID;
                                if (guid != "" && guid != null) {
                                    PendingPaymentAttachmentUpload();
                                }
                                getAttachment();
                                break;
                        }
                    }
                });
            }
        });
};
function PendingPaymentAttachmentUpload() {
    $.ajax({
        url: "/AssetPurchase/TaxFeeOrderDetail/PendingPaymentAttachmentUpload",
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
function computeValue() {
    if ($("#PurchasePrices").val() != "") {
        debugger;
        var checkedItems = $("#OrderQuantity").jqxDropDownList('getCheckedItems');
        var num = 0;
        for (var i = 0; i < checkedItems.length; i++) {
            var n = checkedItems[i].label.substring(checkedItems[i].label.indexOf("量") + 1, checkedItems[i].label.length);
            num += parseInt(n);
        };
        $("#SumQuantity").val(num);
        var value = $("#PurchasePrices").val() * num;
        $("#ContractAmount").val(value);
    }
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
    $("#PurchaseDepartment").jqxDropDownList({ checkboxes: true,disabled: true, selectedIndex: 0, placeHolder: "请选择", source: dataAdapter, displayMember: "Descrption", valueMember: "VGUID", width: 192, height: 33 });
    $("#PurchaseDepartment").jqxDropDownList({ itemHeight: 33 });
    $("#PurchaseDepartment").jqxDropDownList('checkIndex', 0);
}

function initPaymentInformationComboBox() {
    //付款单位及相关账户信息
    $.ajax({
        url: "/AssetPurchase/TaxFeeOrderDetail/GetCustomerBankInfoList",
        data: { "PayItem": $("#PayItem").val() },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (data) {
            
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
            $.post("/AssetPurchase/TaxFeeOrderDetail/GetCustomerBankInfo", { vguid: $("#PaymentInformation").val() }, function (msg) {
                $("#BankAccountName").val(msg.BankAccountName);
                $("#BankAccount").val(msg.BankAccount);
                $("#Bank").val(msg.Bank);
                $("#BankNo").val(msg.BankNo);
            });
        }
    });
}
function initSelectPayItem() {
    $.ajax({
        url: "/AssetPurchase/TaxFeeOrderDetail/GetPayItem",
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#PayItem', msg, "BusinessSubItem1", "BusinessProject");
            $("#PayItem").prepend("<option value=\"\" selected='true'>请选择</>");
        }
    });
}
function initPayCompanyDropdown() {
    $('#PayCompanyDropdown').jqxDropDownList({
        enableSelection: false,
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
function initTable() {
    getDetailData();
    var source =
    {
        localdata: vehicleDefaultData,
        datatype: "json",
        updaterow: function (rowid, rowdata, commit) {

            $.ajax({
                url: "/AssetPurchase/TaxFeeOrderDetail/UpdateAssetNum",
                data: { vguid: rowdata.VGUID, AssetNum: rowdata.AssetNum },
                async: false,
                type: "post",
                success: function (result) {
                }
            });
            commit(true);
        },
        datafields:
        [
            { name: 'VGUID', type: 'string' },
            { name: 'AssetsOrderVguid', type: 'string' },
            { name: 'AssetManagementCompany', type: 'string' },
            { name: 'ApprovalFormFileName', type: 'string' },
            { name: 'ApprovalFormFilePath', type: 'string' },
            { name: 'AssetNum', type: 'number' }
        ]
    };
    var dataAdapter = new $.jqx.dataAdapter(source);
    $("#grid").jqxGrid(
    {
        width: "460",
        autoheight: true,
        source: dataAdapter,
        showstatusbar: true,
        statusbarheight: 25,
        editable: true,
        enabletooltips: true,
        showaggregates: true,
        selectionmode: 'multiplecellsadvanced',
        columns: [
            { text: 'VGUID', datafield: 'VGUID', columntype: 'textbox', width: 190, align: 'center', cellsAlign: 'center', hidden: true, editable: false },
            { text: '资产订单关联ID', datafield: 'AssetsOrderVguid', columntype: 'textbox', width: 190, align: 'center', cellsAlign: 'center', hidden: true, editable: false },
            { text: '资产管理公司', datafield: 'AssetManagementCompany', columntype: 'textbox', width: 340, align: 'center', cellsAlign: 'center', editable: false },
            {
                text: '数量', datafield: 'AssetNum', width: 120, align: 'center', cellsalign: 'center', columntype: 'textbox',
                validation: function (cell, value) {
                    if (value == "")
                        return true;
                    if (!isNumber(value)) {
                        return { result: false, message: "请输入数字" };
                    }
                    return true;
                },
                aggregates: [
                    {
                        '合计':
                            function (aggregatedValue, currentValue) {
                                if (currentValue != "") {
                                    aggregatedValue += currentValue;
                                    //$("#OrderQuantity").val(aggregatedValue);
                                }
                                return aggregatedValue;
                            }
                    }
                ]
            },
            { text: '各单位OA审批单上传', datafield: 'ApprovalFormFilePath', columntype: 'textbox', width: 150, editable: false, cellsrenderer: cellsrenderer, align: 'center', cellsAlign: 'center', hidden: true }
        ]
    });
    $("#OrderDetailsDialog").modal("show");
}
function cellsrenderer(row, column, value, rowData) {
    var vguid = $('#grid').jqxGrid('getrowdata', row).VGUID;
    if (value == "") {
        return '<div style="margin: 58px; margin-top:6px;"><a style="cursor:pointer"  onclick="uploadApprovalFormFile(\'' + vguid + '\')" id="' + vguid + '">上传</a></div>';
    } else {
        return '<div style="margin: 45px; margin-top:6px;">' +
            '<a style="cursor:pointer" href="' + value + '" target="_blank">查看</a>' +
            '&nbsp<a style="cursor:pointer" onclick="deleteApprovalFile(\'' + vguid + '\')" adata=' + vguid + '>删除</a>' +
            '</div>';
    }
}
function getDetailData() {

    $.ajax({
        url: "/AssetPurchase/TaxFeeOrderDetail/GetAssetOrderDetails",
        data: { AssetsOrderVguid: $("#VGUID").val(), PurchaseOrderSettingVguid: $("#PayItem").val() },
        async: false,
        type: "get",
        success: function (result) {
            vehicleDefaultData = result;
        }
    });
};
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
function deleteApprovalFile(vguid) {

    //删除文件
    $.ajax({
        url: "/AssetPurchase/TaxFeeOrderDetail/DeleteApprovalFile",
        data: { vguid: vguid },
        async: false,
        type: "post",
        success: function (msg) {
            switch (msg.Status) {
                case "0":
                    break;
                case "1":
                    initTable();
                    break;
            }
        }
    });
}

function uploadApprovalFormFile(vguid) {
    $("#FileInput").attr("cdata", vguid);
    $("#FileInput").click();
}
function getAttachment() {
    $.ajax({
        url: "/AssetPurchase/TaxFeeOrderDetail/GetAttachmentInfo",
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
                
                var num;
                var fileName = "";
                var fileType = "";
                if (msg[i].Attachment.indexOf("https") != -1) {
                    num = msg[i].Attachment.split("/").length - 1;
                    fileName = msg[i].Attachment.split("/")[num];
                    fileType = fileName.split(".")[1];
                } else {
                    
                    num = msg[i].Attachment.lastIndexOf("/") + 1;
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
        url: "/AssetPurchase/TaxFeeOrderDetail/DeleteAttachment",
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
function GetVehicleModelDropDown() {
    $.ajax({
        url: "/Systemmanagement/VehicleExtrasFeeSettingDetail/GetVehicleModelDropDown",
        type: "GET",
        dataType: "json",
        async: false,
        success: function (msg) {
            
            uiEngineHelper.bindSelect('#VehicleModel', msg, "Code", "Descrption");
            $("#VehicleModel").prepend("<option value=\"\" selected='true'>请选择</>");
            
        }
    });
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
function checkFileExt(ext) {
    if (!ext.match(/.jpg|.png|.doc|.docx|.xls|.xlsx|.pdf|.bmp/i)) {
        return false;
    }
    return true;
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