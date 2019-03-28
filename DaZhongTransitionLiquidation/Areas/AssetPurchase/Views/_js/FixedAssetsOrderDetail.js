//固定资产订单维护明细
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
                if (!Validate($("#OrderType"))) {
                    validateError++;
                }
                if (validateError <= 0) {
                    $.ajax({
                        url: "/AssetPurchase/FixedAssetsOrderDetail/SaveFixedAssetsOrder",
                        data: {
                            "VGUID": $("#VGUID").val(),
                            "OrderType": $("#OrderType").val(),
                            "PaymentInformationVguid": $("#hiddenPaymentInformationVguid").val(),
                            "PaymentInformation": $("#hiddenPaymentInformation").val(),
                            "OrderQuantity": $("#OrderQuantity").val(),
                            "PurchasePrices": $("#PurchasePrices").val(),
                            "ContractAmount": $("#ContractAmount").val(),
                            "AssetDescription": $("#AssetDescription").val(),
                            "UseDepartmentVguid": $("#UseDepartment").val(),
                            "UseDepartment": $("#UseDepartment").find("option:selected").text() == "请选择" ? "" : $("#UseDepartment").find("option:selected").text(),
                            "SupplierInformation": $("#SupplierInformation").val(),
                            "AcceptanceDate": $("#AcceptanceDate").val(),
                            "PaymentDate": $("#PaymentDate").val(),
                            "ContractName": $("#Attachment").attr("title"),
                            "ContractFilePath": $("#Attachment").attr("href")
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
                initTable();
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
    }; //addEvent end

    function getFixedAssetsOrderDetail() {
        $.post("/AssetPurchase/FixedAssetsOrderDetail/GetFixedAssetsOrder", { vguid: $("#VGUID").val() }, function (msg) {
            $("#OrderType").val(msg.OrderType);
            $("#hiddenPaymentInformationVguid").val(msg.PaymentInformationVguid);
            $("#hiddenPaymentInformation").val(msg.PaymentInformation);
            $("#OrderQuantity").val(msg.OrderQuantity);
            $("#PurchasePrices").val(msg.PurchasePrices);
            $("#ContractAmount").val(msg.ContractAmount);
            $("#AssetDescription").val(msg.AssetDescription);
            $("#UseDepartment").val(msg.UseDepartmentVguid);
            $("#SupplierInformation").val(msg.SupplierInformation);
            $("#AcceptanceDate").val(formatDate(msg.AcceptanceDate));
            $("#PaymentDate").val(formatDate(msg.PaymentDate));
            $("#ContractName").val(msg.ContractName);
            if (msg.ContractName != null && msg.ContractFilePath != null) {
                $("#Attachment").show();
                $("#Attachment").attr("href", msg.ContractFilePath);
                debugger;
                $("#Attachment").attr("title", msg.ContractName);
            }
            $("#ContractFilePath").val(msg.ContractFilePath);

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
function initSelect()
{
    //使用部门
    $("#OrderType").prepend("<option value=\"\" selected='true'>请选择</>");
    $.ajax({
        url: "/AssetPurchase/FixedAssetsOrderDetail/GetUseDepartment",
        data: {},
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#UseDepartment', msg, "VGUID", "Descrption");
            $("#UseDepartment").prepend("<option value=\"\" selected='true'>请选择</>");
        }
    });
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
             { text: '公司/单位/个人', datafield: 'CompanyOrPerson', width: '150px', align: 'center', cellsAlign: 'center'},
             { text: '账号', datafield: 'BankAccount', align: 'center', width: '150px', cellsAlign: 'center', },
             { text: '户名', datafield: 'BankAccountName', align: 'center', width: '250px', cellsAlign: 'center' },
             { text: '开户行', datafield: 'Bank', align: 'center', width: '250px', cellsAlign: 'center' },
             { text: '行号', datafield: 'BankNo', align: 'center', cellsAlign: 'center', width: '150px'},
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
        data: { AssetType: "vehicle", AssetsOrderVguid: $("#VGUID").val() },
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