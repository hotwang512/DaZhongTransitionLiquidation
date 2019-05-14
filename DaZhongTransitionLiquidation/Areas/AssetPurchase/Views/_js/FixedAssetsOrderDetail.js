//固定资产订单维护明细
var vguid = "";
var mydate = new Date();
var vehicleDefaultData;
var $page = function () {
    this.init = function () {
        //initSelect();
        initSelectPurchaseGoods();
        initPayCompanyDropdown();
        initComboBox();
        initSelectPurchaseDepartment();
        addEvent();
        $("#PaymentInformation").find(".jqx-combobox-input")[0].setAttribute("style", "box-sizing: border-box;margin: 0px;border: 0px;width: 100%;height: 33px;");
    }
    //所有事件

    function addEvent() {
        var guid = $.request.queryString().VGUID;
        $("#VGUID").val(guid);
        if (guid != "" && guid != null) {
            getFixedAssetsOrderDetail();
        } else {
            $("#VGUID").val(newguid());
            $("#AssetsOrderVguid").val(newguid());
        }
        //取消
        $("#btnCancel").on("click",
            function() {
                history.go(-1);
            });
        //保存
        $("#btnSave").on("click",
            function() {
                var validateError = 0; //未通过验证的数量
                if (!Validate($("#PurchaseGoods"))) {
                    validateError++;
                }
                debugger;
                if (validateError <= 0) {
                    var checkedItems = $("#PurchaseDepartment").jqxDropDownList('getCheckedItems');

                    var DepartmentModelList = [];
                    for (var i = 0; i < checkedItems.length; i++) {
                        DepartmentModelList.push(checkedItems[i].value);
                    };
                    debugger;
                    $.ajax({
                        url: "/AssetPurchase/FixedAssetsOrderDetail/SaveFixedAssetsOrder",
                        data: {
                            "VGUID": $("#VGUID").val(),
                            "PurchaseGoods": $("#PurchaseGoods option:selected").text(),
                            "PurchaseDepartmentIDs": DepartmentModelList.join(","),
                            "PurchaseGoodsVguid": $("#PurchaseGoods").val(),
                            "PaymentInformationVguid": $("#hiddenPaymentInformationVguid").val(),
                            "PayCompanyVguid": $("#PayCompanyDropdown").val(),
                            "PaymentInformation": $("#hiddenPaymentInformation").val(),
                            "OrderQuantity": $("#OrderQuantity").val(),
                            "PurchasePrices": $("#PurchasePrices").val(),
                            "ContractAmount": $("#ContractAmount").val(),
                            "AssetDescription": $("#AssetDescription").val(),
                            //"UseDepartmentVguid": $("#UseDepartment").val(),
                            //"UseDepartment": $("#UseDepartment").find("option:selected").text() == "请选择" ? "" : $("#UseDepartment").find("option:selected").text(),
                            //"AcceptanceDate": $("#AcceptanceDate").val(),
                            "PaymentDate": $("#PaymentDate").val(),
                            "ContractName": $("#Attachment").attr("title"),
                            "ContractFilePath": $("#Attachment").attr("href"),
                            "PayCompany": $("#hidPayCompany").val(),
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
                        success: function(msg) {
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
                debugger;
                if ($("#PurchaseGoods").val() != "") {
                    initTable();
                } else {
                    jqxNotification("请选择采购物品！", null, "error");
                }
            });
        $("#UploadContractFile").on("click",
            function() {
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
        //打印
        $("#btnPrint").on("click",
            function () {
                $("#CreditDialog").modal("show");
            });
        //计算金额
        $("#PurchasePrices").on("blur",
            function () {
                computeValue();
            });
        //提交
        $("#btnSubmit").on("click",
            function () {
                $.post("/AssetPurchase/FixedAssetsOrderDetail/SubmitFixedAssetsOrder", { vguid: $("#VGUID").val() }, function (msg) {
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
                debugger;
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
        $('#PayCompanyDropdown').on('select', function (event) {

            var args = event.args;
            if (args && $("#PayMode").val() != "现金") {
                debugger;
                var item = args.item;
                $("#PayCompany").val(item.lable);
                $.post("/AssetPurchase/FixedAssetsOrderDetail/GetCompanyBankInfo",
                    { Vguid: $("#PayCompanyDropdown").val() },
                    function(msg) {
                        $("#CompanyBankName").val(msg.BankName);
                        $("#CompanyBankAccount").val(msg.BankAccount);
                        $("#CompanyBankAccountName").val(msg.BankAccountName);
                        $("#AccountType").val(msg.AccountType);
                    });
            } else {
                $("#CompanyBankName").val("");
                $("#CompanyBankAccount").val("");
                $("#CompanyBankAccountName").val("");
                $("#AccountType").val("");
            }
        });
        $("#PurchaseGoods").on("change",
            function () {
                initComboBox();
                $("#PurchaseDepartment").jqxDropDownList({ disabled: true });
                $("#PurchaseGoods").attr("disabled", true);
            });
        $("#PurchaseDepartment").on('checkChange', function (event) {
            if (event.args) {
                var checkedItems = $("#PurchaseDepartment").jqxDropDownList('getCheckedItems');
                var DepartmentModelList = [];
                for (var i = 0; i < checkedItems.length; i++) {
                    DepartmentModelList.push(checkedItems[i].value);
                };
                debugger;
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
    }; //addEvent end

    function getFixedAssetsOrderDetail() {
        $.post("/AssetPurchase/FixedAssetsOrderDetail/GetFixedAssetsOrder", { vguid: $("#VGUID").val() }, function (msg) {
            debugger;
            if (msg.PurchaseDepartmentIDs != null) {
                var PurchaseDepartment = msg.PurchaseDepartmentIDs.split(",");
                for (var i = 0; i < PurchaseDepartment.length; i++) {
                    var item = $("#PurchaseDepartment").jqxDropDownList('getItemByValue', PurchaseDepartment[i]);
                    $("#PurchaseDepartment").jqxDropDownList('checkItem', item);
                }
            }
            $("#PurchaseDepartment").jqxDropDownList({ disabled: true });
            initSelectPurchaseGoods();
            $("#PurchaseGoods").val(msg.PurchaseGoodsVguid);
            $("#PurchaseGoods").attr("disabled", true);
            initComboBox();
            $("#hiddenPaymentInformationVguid").val(msg.PaymentInformationVguid);
            $("#hiddenPaymentInformation").val(msg.PaymentInformation);
            $("#OrderQuantity").val(msg.OrderQuantity);
            $("#PurchasePrices").val(msg.PurchasePrices);
            $("#ContractAmount").val(msg.ContractAmount);
            $("#AssetDescription").val(msg.AssetDescription);
            //$("#UseDepartment").val(msg.UseDepartmentVguid);
            debugger;
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
                debugger;
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
                url: "/AssetPurchase/FixedAssetsOrderDetail/UploadContractFile",
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
        function() {
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
                    url: "/AssetPurchase/FixedAssetsOrderDetail/UploadLocalFile",
                    type: "post",
                    data: {
                        'VGUID': $("#FileInput").attr("cdata")
                    },
                    success: function(msg) {
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
    $("#PurchaseDepartment").jqxDropDownList({ checkboxes: true, selectedIndex: 0,placeHolder:"请选择", source: dataAdapter, displayMember: "Descrption", valueMember: "VGUID", width: 192, height: 33 });
    $("#PurchaseDepartment").jqxDropDownList({ itemHeight: 33 });
}
//function initSelect()
//{
//    $.ajax({
//        url: "/AssetPurchase/FixedAssetsOrderDetail/GetUseDepartment",
//        data: {},
//        type: "POST",
//        dataType: "json",
//        async: false,
//        success: function (msg) {
//            uiEngineHelper.bindSelect('#UseDepartment', msg, "VGUID", "Descrption");
//            $("#UseDepartment").prepend("<option value=\"\" selected='true'>请选择</>");
//        }
//    });
//}
function initComboBox() {
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
function initSelectPurchaseGoods(PurchaseDepartment) {
    //使用部门
    $.ajax({
        url: "/AssetPurchase/FixedAssetsOrderDetail/GetPurchaseGoods",
        data: { "OrderCategory": 0, "PurchaseDepartment": PurchaseDepartment },
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
function initPayCompanyDropdown() {
    var url = "/AssetPurchase/FixedAssetsOrderDetail/GetCompanyBankInfoDropdown";
    var source =
    {
        datatype: "json",
        datafields: [
            { name: 'VGUID' },
            { name: 'Descrption' }
        ],
        url: url,
        async: false
    };
    var dataAdapter = new $.jqx.dataAdapter(source);
    $('#PayCompanyDropdown').jqxDropDownList({
        filterable: true, selectedIndex: 2, source: dataAdapter, displayMember: "Descrption", dropDownWidth:
            310, filterHeight: 30, valueMember: "VGUID", itemHeight: 30, height: 33, width: 200,searchMode:'contains',
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
            debugger;
            $.ajax({
                url: "/AssetPurchase/FixedAssetsOrderDetail/UpdateAssetNum",
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
            { text: 'VGUID', datafield: 'VGUID', columntype: 'textbox', width: 190, align: 'center', cellsAlign: 'center', hidden: true,editable:false },
            { text: '资产订单关联ID', datafield: 'AssetsOrderVguid', columntype: 'textbox', width: 190, align: 'center', cellsAlign: 'center', hidden: true, editable: false },
            { text: '资产管理公司', datafield: 'AssetManagementCompany', columntype: 'textbox', width: 190, align: 'center', cellsAlign: 'center', editable: false },
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
                                    $("#OrderQuantity").val(aggregatedValue);
                                    computeValue();
                                }
                                return aggregatedValue;
                            }
                    }
                ]
            },
            { text: '各单位OA审批单上传', datafield: 'ApprovalFormFilePath', columntype: 'textbox', width: 150,editable:false, cellsrenderer: cellsrenderer, align: 'center', cellsAlign: 'center' }
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
    debugger;
    $.ajax({
        url: "/AssetPurchase/FixedAssetsOrderDetail/GetAssetOrderDetails",
        data: { AssetsOrderVguid: $("#VGUID").val(), PurchaseOrderSettingVguid: $("#PurchaseGoods").val() },
        async :false,
        type: "get",
        success: function (result) {
            debugger;
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
    debugger;
    //删除文件
    $.ajax({
        url: "/AssetPurchase/FixedAssetsOrderDetail/DeleteApprovalFile",
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