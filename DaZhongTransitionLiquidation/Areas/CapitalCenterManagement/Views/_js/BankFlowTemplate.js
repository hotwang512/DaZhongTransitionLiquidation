//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },

    $txtDatedTime: function () { return $("#txtDatedTime") },
    $AddNewBankDataDialog: function () { return $("#AddNewBankDataDialog") },
    $AddNewBankData_OKButton: function () { return $("#AddNewBankData_OKButton") },
    $AddNewBankData_CancelBtn: function () { return $("#AddNewBankData_CancelBtn") },

    $VoucherSubject: function () { return $("#VoucherSubject") },
    $VoucherSummary: function () { return $("#VoucherSummary") }, 

    $txtBankAccountName_Dialog: function () { return $("#txtBankAccountName_Dialog") },
    $txtBankAccount_Dialog: function () { return $("#txtBankAccount_Dialog") },
    $txtBank_Dialog: function () { return $("#txtBank_Dialog") },

    $pushPeopleDropDownButton: function () { return $("#pushPeopleDropDownButton") },
    $pushTree: function () { return $("#pushTree") },

    $txtChannel_Dialog: function () { return $("#txtChannel_Dialog") },
    $txtRemark_Dialog: function () { return $("#txtRemark_Dialog") },
    $EditPermission: function () { return $("#EditPermission") },
    $uploadFileHuiDouQuan: function () { return $("#uploadFileHuiDouQuan") },
}; //selector end

var isEdit = false;
var vguid = "";
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        getBankInfo();
        $("#btnSync").on("click", function () {
            layer.load();
            $.ajax({
                url: "/CapitalCenterManagement/BankFlowTemplate/SyncCurrentDayBankData",
                data: {},
                type: "post",
                dataType: "json",
                success: function (msg) {
                    layer.closeAll('loading');
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("同步失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("同步成功！", null, "success");
                            selector.$grid().jqxDataTable('updateBoundData');
                            break;
                        case "2":
                            jqxNotification("同步服务正在被调用,请稍作等待！", null, "error");
                            break;
                    }
                }
            });
        });
        $("#btnYesterdaySync").on("click", function () {
            layer.load();
            $.ajax({
                url: "/CapitalCenterManagement/BankFlowTemplate/SyncYesterdayBankData",
                data: {},
                type: "post",
                dataType: "json",
                success: function (msg) {
                    layer.closeAll('loading');
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("同步失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("同步成功！", null, "success");
                            selector.$grid().jqxDataTable('updateBoundData');
                            break;
                        case "2":
                            jqxNotification("同步服务正在被调用,请稍作等待！", null, "error");
                            break;
                    }
                }
            });
        });
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#TradingBank").val("");
            $("#TransactionDate").val("");
            $("#TransactionDateEnd").val("");
            $("#ReceivingUnit").val("");
        });
        //导入建设银行
        $("#btnImportingCBC").on("click", function () {
            $("#uploadFileCBC").val("");
            $("#uploadFileCBC").click();
        });
        //上传文件变更时间
        $("#uploadFileCBC").on('change', function () {
            layer.load();
            uploadFile(this.files[0], function (fileName) {
                runImportDataCBC(fileName, function (result) {
                    if (result.IsSuccess == true) {
                        jqxNotification("导入完成！", null, "success");
                        initTable();
                    }
                    else {
                        jqxNotification("导入失败！" + result.ResultInfo, null, "success");
                    }
                    layer.closeAll('loading');
                });
            })
        });
        //导入交通银行
        $("#btnImportingBCM").on("click", function () {
            $("#uploadFileBCM").val("");
            $("#uploadFileBCM").click();
        });
        //上传文件变更时间
        $("#uploadFileBCM").on('change', function () {
            layer.load();
            uploadFile(this.files[0], function (fileName) {
                runImportDataBCM(fileName, function (result) {
                    if (result.IsSuccess == true) {
                        jqxNotification("导入完成！", null, "success");
                        initTable();
                    }
                    else {
                        jqxNotification("导入失败！" + result.ResultInfo, null, "success");
                    }
                    layer.closeAll('loading');
                });
            })
        });
        //弹出框中的取消按钮
        selector.$AddNewBankData_CancelBtn().on("click", function () {
            selector.$AddNewBankDataDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddNewBankData_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate(selector.$VoucherSubject())) {
                validateError++;
            }
            if (!Validate(selector.$VoucherSummary())) {
                validateError++;
            }
            if (validateError <= 0) {
                $.ajax({
                    url: "/CapitalCenterManagement/BankFlowTemplate/SaveBankFlow?isEdit=" + isEdit,
                    data: {
                        "VoucherSummary": selector.$VoucherSummary().val(),
                        "VoucherSubject": selector.$VoucherSubject().val(),
                        "VoucherSubjectName": $("#VoucherSubjectName").val(),
                        "VGUID": vguid
                    },
                    type: "post",
                    dataType: "json",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                selector.$grid().jqxDataTable('updateBoundData');
                                selector.$AddNewBankDataDialog().modal("hide");
                                break;
                        }
                    }
                });
            }
        });

    }; //addEvent end


    function initTable() {
        var DateEnd = $("#TransactionDateEnd").val();
        var source =
            {
                datafields:
                [
                    //{ name: "checkbox", type: null },
                    { name: 'AccountModeName', type: 'string' },
                    { name: 'TradingBank', type: 'string' },                   
                    { name: 'TransactionDate', type: 'date' },
                    { name: 'TurnOut', type: 'number' },
                    { name: 'TurnIn', type: 'number' },
                    { name: 'Currency', type: 'string' },
                    { name: 'PaymentUnit', type: 'string' },
                    { name: 'PayeeAccount', type: 'string' },
                    { name: 'PaymentUnitInstitution', type: 'string' },
                    { name: 'ReceivingUnit', type: 'string' },
                    { name: 'ReceivableAccount', type: 'string' },
                    { name: 'ReceivingUnitInstitution', type: 'string' },
                    { name: 'Purpose', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Batch', type: 'string' },
                    { name: 'VoucherSubject', type: 'string' },
                    { name: 'VoucherSubjectName', type: 'string' },
                    { name: 'VoucherSummary', type: 'string' },
                    { name: 'Balance', type: 'string' },
                    { name: 'CreateTime', type: 'date' },
                    { name: 'CreatePerson', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "TradingBank": $("#TradingBank").val(), "TransactionDate": $("#TransactionDate").val(), "ReceivingUnit": $("#ReceivingUnit").val() },
                url: "/CapitalCenterManagement/BankFlowTemplate/GetBankFlowData?TransactionDateEnd=" + DateEnd   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid().jqxDataTable(
            {
                pageable: true,
                width: "100%",
                height: 400,
                pageSize: 10,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 30,
                columns: [
                    //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '账套', datafield: 'AccountModeName', pinned: true, width: 200, align: 'center', cellsAlign: 'center', },
                    { text: '公司', datafield: 'PaymentUnit', pinned: true, width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '币种', datafield: 'Currency',  hidden: true, align: 'center', cellsAlign: 'center' },
                    { text: '我方银行', datafield: 'TradingBank', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '我方账号', datafield: 'PayeeAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '我方开户机构', datafield: 'PaymentUnitInstitution', width: 350, align: 'center', cellsAlign: 'center' },
                    { text: 'T24交易流水号', datafield: 'Batch', width: 200, align: 'center', cellsAlign: 'center',  },
                    { text: '交易日期', datafield: 'TransactionDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '转入(贷)', datafield: 'TurnOut', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '转出(借)', datafield: 'TurnIn', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '余额', datafield: 'Balance', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '对方单位名称', datafield: 'ReceivingUnit', width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '对方账号', datafield: 'ReceivableAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '对方开户机构', datafield: 'ReceivingUnitInstitution', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '用途', datafield: 'Purpose', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '备注1', datafield: 'Remark', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '写入日期', datafield: 'CreateTime', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '写入人', datafield: 'CreatePerson', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '凭证科目Code', hidden: true,datafield: 'VoucherSubject', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '备注2', datafield: 'VoucherSubjectName', width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '凭证摘要', hidden: true, datafield: 'VoucherSummary', width: 300, align: 'center', cellsAlign: 'center' },
                    
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }

    function detailFunc(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "1") {
            var voucherSubject = rowData.VoucherSubject == " " ? null : rowData.VoucherSubject;
            container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + voucherSubject + "','" + rowData.VoucherSummary + "','" + rowData.VoucherSubjectName + "') style=\"text-decoration: underline;color: #333;\">" + rowData.AccountModeName + "</a>";
        } else {
            container = "<span>" + rowData.AccountModeName + "</span>";
        }
        return container;
    }

    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
    }

    function rendererFunc() {
        var checkBox = "<div id='jqx_datatable_checkbox_all' class='jqx_datatable_checkbox_all' style='z-index: 999; margin-left:7px ;margin-top: 7px;'>";
        checkBox += "</div>";
        return checkBox;
    }

    function renderedFunc(element) {
        var grid = selector.$grid();
        element.jqxCheckBox();
        element.on('change', function (event) {
            var checked = element.jqxCheckBox('checked');

            if (checked) {
                var rows = grid.jqxDataTable('getRows');
                for (var i = 0; i < rows.length; i++) {
                    grid.jqxDataTable('selectRow', i);
                    grid.find(".jqx_datatable_checkbox").attr("checked", "checked")
                }
            } else {
                grid.jqxDataTable('clearSelection');
                grid.find(".jqx_datatable_checkbox").removeAttr("checked", "checked")
            }
        });
        return true;
    }
};

function edit(guid, voucherSubject,voucherSummary,voucherSubjectName) {
    selector.$VoucherSubject().val("");
    selector.$VoucherSummary().val("");
    isEdit = true;
    vguid = guid;
    if (voucherSubject == null || voucherSubject == "null") {
        voucherSubject = "";
    }
    if (voucherSubjectName == null || voucherSubjectName == "null") {
        voucherSubjectName = "";
    }
    if (voucherSummary == null || voucherSummary == "null") {
        voucherSummary = "";
    }
    selector.$VoucherSubject().val(voucherSubject);
    $("#VoucherSubjectName").val(voucherSubjectName);
    selector.$VoucherSummary().val(voucherSummary);
    selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', "");
    $("#myModalLabel_title").text("编辑银行数据");
    //$("#AddNewBankDataDialog table tr").eq(1).hide();
    var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 5px;">' + voucherSubjectName + '</div>';
    selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', dropDownContent);
    $(".msg").remove();
    selector.$VoucherSubject().removeClass("input_Validate");
    selector.$VoucherSummary().removeClass("input_Validate");
    selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewBankDataDialog().modal("show");
}

$(function () {
    var page = new $page();
    page.init();
});
function getBankInfo() {
    $.ajax({
        url: "/CapitalCenterManagement/BankFlowTemplate/GetBankInfo",
        async: false,
        data: {},
        type: "post",
        success: function (result) {
            uiEngineHelper.bindSelect('#TradingBank', result, "BankAccount", "BankName");
            $("#TradingBank").prepend("<option value=\"\" selected='true'></>");

        }
    });
}
//上传文件
function uploadFile(fileData, callback) {
    var formData = new FormData();
    formData.append("file", fileData);
    formData.append("filename", fileData.name);
    $.ajax({
        url: '/PaymentManagement/NextDayData/UploadImportFile',
        type: 'post',
        data: formData,//这里上传的数据使用了formData 对象
        processData: false, 	//必须false才会自动加上正确的Content-Type
        contentType: false,
        success: function (result) {
            if (callback) {
                callback(result);
            }
        },
        error: function (xmlhttprequest, textstatus, errorthrow) {
            jqxNotification("上传错误！", null, "error");
            layer.closeAll('loading');
        }

    });
}
//执行导入建设银行
function runImportDataCBC(fileName, callback) {
    $.ajax({
        url: '/CapitalCenterManagement/BankFlowTemplate/ImportDataCBC',
        type: 'post',
        data: { fileName: fileName },//这里上传的数据使用了formData 对象
        success: function (result) {
            if (callback) {
                callback(result);
            }
        },
        error: function (xmlhttprequest, textstatus, errorthrow) {
            jqxNotification("导入错误！", null, "error");
            layer.closeAll('loading');
        }

    });
}
//执行导入交通银行
function runImportDataBCM(fileName, callback) {
    $.ajax({
        url: '/CapitalCenterManagement/BankFlowTemplate/ImportDataBCM',
        type: 'post',
        data: { fileName: fileName },//这里上传的数据使用了formData 对象
        success: function (result) {
            if (callback) {
                callback(result);
            }
        },
        error: function (xmlhttprequest, textstatus, errorthrow) {
            jqxNotification("导入错误！", null, "error");
            layer.closeAll('loading');
        }

    });
}