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
            });
    }; //addEvent end

    function getIntangibleAssetsOrderDetail() {
        $.post("/AssetPurchase/IntangibleAssetsOrderDetail/GetIntangibleAssetsOrder", { vguid: $("#VGUID").val() }, function (msg) {
            $("#OrderType").val(msg.OrderType);
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
            $("#PaymentInformation").jqxDropDownButton('setContent', dropDownContent);
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
    
    //付款单位及相关账户信息
    debugger;
    var source =
    {
        datafields:
        [
            { name: 'CompanyOrPerson', type: 'string' },
            { name: 'BankAccountName', type: 'string' },
            { name: 'Bank', type: 'string' },
            { name: 'BankAccount', type: 'string' },
            { name: 'BankNo', type: 'string' },
            { name: 'VGUID', type: 'string' }
        ],
        datatype: "json",
        id: "Vguid",
        data: { "BankAccount": "" },
        url: "/CapitalCenterManagement/CustomerBankInfo/GetCustomerBankInfo",   //获取数据源的路径
        updaterow: function (rowid, rowdata) {
            // synchronize with the server - send update command   
        }
    };
    var dataAdapter = new $.jqx.dataAdapter(source);
    $("#PaymentInformationGrid").jqxGrid(
     {
         width: 550,
         source: dataAdapter,
         pageable: true,
         autoheight: true,
         columnsresize: true,
         columns: [
             { text: '公司/单位/个人', datafield: 'CompanyOrPerson', width: '150px', align: 'center', cellsAlign: 'center' },
             { text: '账号', datafield: 'BankAccount', align: 'center', width: '150px', cellsAlign: 'center', },
             { text: '户名', datafield: 'BankAccountName', align: 'center', width: '250px', cellsAlign: 'center' },
             { text: '开户行', datafield: 'Bank', align: 'center', width: '250px', cellsAlign: 'center' },
             { text: '行号', datafield: 'BankNo', align: 'center', cellsAlign: 'center', width: '150px' },
             { text: 'VGUID', datafield: 'VGUID', hidden: true }
         ]
     });
    // initialize jqxGrid
    $("#PaymentInformation").jqxDropDownButton({
        width: 198, height: 33
    });
    $("#PaymentInformationGrid").on('rowselect', function (event) {
        var args = event.args;
        var row = $("#PaymentInformationGrid").jqxGrid('getrowdata', args.rowindex);
        debugger;
        if (row != undefined) {
            var dropDownContent = '<div style="position: relative; margin-left: 8px; margin-top: 10px;">' + row['CompanyOrPerson'] + '</div>';
            $("#PaymentInformation").jqxDropDownButton('setContent', dropDownContent);
            $("#hiddenPaymentInformationVguid").val(row['VGUID']);
            $("#hiddenPaymentInformation").val(row['CompanyOrPerson']);
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