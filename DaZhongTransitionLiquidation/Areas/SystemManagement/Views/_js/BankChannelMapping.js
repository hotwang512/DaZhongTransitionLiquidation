//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },

    $txtBankName: function () { return $("#txtBankName") },
    $txtChannelName: function () { return $("#txtChannelName") },

    $AddBankChannelDialog: function () { return $("#AddBankChannelDialog") },
    $AddBankChannel_OKButton: function () { return $("#AddBankChannel_OKButton") },
    $AddBankChannel_CancelBtn: function () { return $("#AddBankChannel_CancelBtn") },
    $txtBankAccount_Dialog: function () { return $("#txtBankAccount_Dialog") },
    $txtBankAccountName_Dialog: function () { return $("#txtBankAccountName_Dialog") },
    $txtChannel_Dialog: function () { return $("#txtChannel_Dialog") },
    $txtBank_Dialog: function () { return $("#txtBank_Dialog") },
    $EditPermission: function () { return $("#EditPermission") },
    $btnEditIsable: function () { return $("#EditIsable") },
}; //selector end

var isEdit = false;
var vguid = "";
var companyCode = "";
var accountMode = "";
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {

        //加载列表数据
        initTable();
        getCompanyCode();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            selector.$txtChannelName().val("");
            selector.$txtBankName().val("");
        });
        selector.$btnAdd().on("click", function () {
            add();
        });

        //弹出框中的取消按钮
        selector.$AddBankChannel_CancelBtn().on("click", function () {
            selector.$AddBankChannelDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddBankChannel_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate(selector.$txtBankAccount_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtBankAccountName_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtBank_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtChannel_Dialog())) {
                validateError++;
            }
            if (validateError <= 0) {
                $.ajax({
                    url: "/SystemManagement/BankChannelMapping/SaveBankChannelInfo?isEdit=" + isEdit,
                    data: {
                        BankAccount: selector.$txtBankAccount_Dialog().val(),
                        BankAccountName: selector.$txtBankAccountName_Dialog().val(),
                        Bank: selector.$txtBank_Dialog().val(),
                        Channel: selector.$txtChannel_Dialog().val(),
                        VGUID: vguid
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
                                selector.$AddBankChannelDialog().modal("hide");
                                break;
                            case "2":
                                jqxNotification("渠道名称已经存在！", null, "error");
                                break;
                        }

                    }
                });
            }
        });
        //删除
        selector.$btnDelete().on("click", function () {
            var selection = [];
            var grid = selector.$grid();
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
                jqxNotification("请选择您要删除的数据！", null, "error");
            } else {
                WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消",selection);
            }
        });
        //编辑是否启用
        $("#EditIsableTrue").on('click', function (event) {
            isUnable = "";
            var selection = [];
            var grid = selector.$grid();
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
                jqxNotification("请选择您要启用的数据！", null, "error");
            } else {
                WindowConfirmDialog(updateIsUnable, "您确定要启用选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
        //编辑是否禁用
        $("#EditIsableFalse").on('click', function (event) {
            isUnable = "禁用";
            var selection = [];
            var grid = selector.$grid();
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
                jqxNotification("请选择您要禁用的数据！", null, "error");
            } else {
                WindowConfirmDialog(updateIsUnable, "您确定要禁用选中的数据？", "确认框", "确定", "取消", selection);
            }
        });

    }; //addEvent end


    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'BankAccountName', type: 'string' },
                    { name: 'Bank', type: 'string' },
                    { name: 'BankAccount', type: 'string' },
                    { name: 'Channel', type: 'string' },
                    { name: 'ChannelName', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'IsUnable', type: 'string' },
                    { name: 'Borrow', type: 'string' },
                    { name: 'Loan', type: 'string' },
                    { name: 'CompanyCode', type: 'string' },
                ],
                datatype: "json",
                id: "Vguid",
                data: { "BankAccount": selector.$txtBankName().val(), "Channel": selector.$txtChannelName().val() },
                url: "/Systemmanagement/BankChannelMapping/GetBankChannelMappingInfos"   //获取数据源的路径
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
                columnsHeight: 40,
                columns: [
                    { width: 35, text: "", datafield: "checkbox", align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '账号', width: 250, datafield: 'BankAccount', align: 'center', cellsAlign: 'center', cellsRenderer: channelDetailFunc },
                    { text: '户名', width: 350, datafield: 'BankAccountName', align: 'center', cellsAlign: 'center', },
                    { text: '开户行', width: 450, datafield: 'Bank', align: 'center', cellsAlign: 'center', },
                    { text: '渠道名称', datafield: 'ChannelName', align: 'center', cellsAlign: 'center', },
                    { text: '借', datafield: "Borrow",  width: '180px',hidden: true, align: 'center', cellsAlign: 'center' },
                    { text: '贷', datafield: "Loan", width: '180px', hidden: true,align: 'center',cellsAlign: 'center' },
                    { text: '是否禁用', datafield: "IsUnable", align: 'center', cellsAlign: 'center', },
                    { text: '渠道编码', datafield: 'Channel', hidden: true },
                    { text: '公司', datafield: 'CompanyCode', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });

    }

    function channelDetailFunc(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "1") {
            container = "<a href='#' onclick=edit('" + rowData.VGUID + "','"
                + rowData.BankAccount + "','"
                + rowData.BankAccountName + "','"
                + rowData.Bank + "','"
                
                + rowData.Channel + "') style=\"text-decoration: underline;color: #333;\">" + rowData.BankAccount + "</a>";
        } else {
            container = "<span>" + rowData.BankAccount + "</span>";
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


    //删除
    function dele(selection) {
        $.ajax({
            url: "/Systemmanagement/BankChannelMapping/DeleteBankChannelInfo",
            data: { vguids: selection },
            traditional: true,
            type: "post",
            success: function (msg) {
                if (msg.IsSuccess) {
                    jqxNotification("删除成功！", null, "success");
                    selector.$grid().jqxDataTable('updateBoundData');
                } else {
                    jqxNotification("删除失败！", null, "error");
                }
            }
        });
    }
    var isUnable = "";
    function updateIsUnable(selection) {
        $.ajax({
            url: "/SystemManagement/BankChannelMapping/UpdateIsUnable",
            data: { vguids: selection, isUnable: isUnable },
            //traditional: true,
            type: "post",
            success: function (msg) {
                if (msg.IsSuccess) {
                    if (isUnable == "") {
                        jqxNotification("启用成功！", null, "success");
                    } else {
                        jqxNotification("禁用成功！", null, "success");
                    }
                    selector.$grid().jqxDataTable('updateBoundData');
                } else {
                    if (isUnable == "") {
                        jqxNotification("启用失败！", null, "success");
                    } else {
                        jqxNotification("禁用失败！", null, "error");
                    }
                    
                }
            }
        });
    }
};

function add() {
    selector.$txtBankAccount_Dialog().val("");
    selector.$txtBankAccountName_Dialog().val("");
    selector.$txtBank_Dialog().val("");
    selector.$txtChannel_Dialog().val("");
    isEdit = false;
    vguid = "";
    $("#myModalLabel_title").text("新增银行渠道映射");
    selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddBankChannelDialog().modal("show");
    //initBorrowTable(companyCode, accountMode);
}

function edit(guid, BankAccount, BankAccountName, Bank,  Channel) {
    selector.$txtBankAccount_Dialog().val("");
    selector.$txtBankAccountName_Dialog().val("");
    selector.$txtBank_Dialog().val("");
    selector.$txtChannel_Dialog().val("");
    isEdit = true;
    vguid = guid;
    $("#myModalLabel_title").text("编辑银行渠道映射");
    selector.$txtBankAccount_Dialog().val(BankAccount);
    selector.$txtBankAccountName_Dialog().val(BankAccountName);
    selector.$txtBank_Dialog().val(Bank);
    selector.$txtChannel_Dialog().val(Channel);
    
    //initBorrowTable(CompanyCode, accountMode);
   

    $(".msg").remove();
    selector.$txtBankAccount_Dialog().removeClass("input_Validate");
    selector.$txtBankAccountName_Dialog().removeClass("input_Validate");
    selector.$txtBank_Dialog().removeClass("input_Validate");
    selector.$txtChannel_Dialog().removeClass("input_Validate");

    selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddBankChannelDialog().modal("show");
}



$(function () {
    var page = new $page();
    page.init();
});

function getCompanyCode() {
    accountMode = $("#LoginAccountModeCode").val();
    $.ajax({
        url: "/HomePage/CompanyHomePage/GetCompanyCode",
        data: { accountMode: accountMode },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#CompanyCode', msg, "CompanyCode", "CompanyName");
            //$("#CompanyCode").prepend("<option value=\"\" selected='true'>请选择</>");
        }
    });
    companyCode = $("#CompanyCode").val();
}
function initBorrowTable(companyCode, accountMode) {
    var source = {
        datafields:
        [
            { name: 'BusinessCode', type: 'string' },
            { name: 'Company', type: 'string' },
            { name: 'CompanyCode', type: 'string' },
            { name: 'AccountingCode', type: 'string' },
            { name: 'CostCenterCode', type: 'string' },
            { name: 'SpareOneCode', type: 'string' },
            { name: 'SpareTwoCode', type: 'string' },
            { name: 'IntercourseCode', type: 'string' },
            { name: 'Accounting', type: 'string' },
            { name: 'CostCenter', type: 'string' },
            { name: 'SpareOne', type: 'string' },
            { name: 'SpareTwo', type: 'string' },
            { name: 'Intercourse', type: 'string' },
            { name: 'SubjectCode', type: 'string' },
            { name: 'SubjectVGUID', type: 'string' },
            { name: 'Checked', type: 'string' },
            { name: 'Balance', type: 'number' },
        ],
        datatype: "json",
        cache: false,
        id: "SectionVGUID",
        data: { companyCode: companyCode, accountModeCode: accountMode },
        url: "/PaymentManagement/SubjectBalance/GetSubjectBalance"    //获取数据源的路径
    };
    var typeAdapter = new $.jqx.dataAdapter(source);
    //创建卡信息列表（主表）
    $("#grid1").jqxGrid({
        pageable: true,
        width: "100%",
        autoheight: false,
        columnsresize: true,
        pageSize: 15,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        pagermode: 'simple',
        columnsHeight: 40,
        columns: [
            //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
            { text: '编码', datafield: 'BusinessCode', width: 250, pinned: true, align: 'center', cellsAlign: 'center', },
            { text: '科目段', datafield: 'Company', width: 200, pinned: false, align: 'center', cellsAlign: 'center' },
            { text: '核算段', datafield: 'Accounting', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '成本中心段', datafield: 'CostCenter', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '备用1', datafield: 'SpareOne', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '备用2', datafield: 'SpareTwo', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '往来段', datafield: 'Intercourse', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '余额', datafield: 'Balance', cellsFormat: "d2", align: 'center', cellsAlign: 'center' },

            { text: '核算段', datafield: 'AccountingCode', hidden: true },
            { text: '成本中心段', datafield: 'CostCenterCode', hidden: true },
            { text: '备用1', datafield: 'SpareOneCode', hidden: true },
            { text: '备用2', datafield: 'SpareTwoCode', hidden: true },
            { text: '往来段', datafield: 'IntercourseCode', hidden: true },
            { text: 'SubjectCode', datafield: 'ParentCode', hidden: true },
            //{ text: 'BusinessCode', datafield: 'BusinessCode', hidden: true },
            { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
        ]
    });
    $("#jqxdropdownbutton1").jqxDropDownButton({
        width: 210, height: 30
    });
    $("#grid1").on('rowclick', function (event) {
        var args = event.args;
        var row = $("#grid1").jqxGrid('getrowdata', args.rowindex);
        var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + row['BusinessCode'] + '</div>';
        $("#jqxdropdownbutton1").jqxDropDownButton('setContent', dropDownContent);
    });

    $("#grid2").jqxGrid({
        pageable: true,
        width: "100%",
        autoheight: false,
        columnsresize: true,
        pageSize: 15,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        columnsHeight: 40,
        columns: [
            //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
            { text: '编码', datafield: 'BusinessCode', width: 250, pinned: true, align: 'center', cellsAlign: 'center', },
            { text: '科目段', datafield: 'Company', width: 200, pinned: false, align: 'center', cellsAlign: 'center' },
            { text: '核算段', datafield: 'Accounting', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '成本中心段', datafield: 'CostCenter', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '备用1', datafield: 'SpareOne', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '备用2', datafield: 'SpareTwo', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '往来段', datafield: 'Intercourse', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '余额', datafield: 'Balance', cellsFormat: "d2", align: 'center', cellsAlign: 'center' },

            { text: '核算段', datafield: 'AccountingCode', hidden: true },
            { text: '成本中心段', datafield: 'CostCenterCode', hidden: true },
            { text: '备用1', datafield: 'SpareOneCode', hidden: true },
            { text: '备用2', datafield: 'SpareTwoCode', hidden: true },
            { text: '往来段', datafield: 'IntercourseCode', hidden: true },
            { text: 'SubjectCode', datafield: 'ParentCode', hidden: true },
            //{ text: 'BusinessCode', datafield: 'BusinessCode', hidden: true },
            { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
        ]
    });
    $("#jqxdropdownbutton2").jqxDropDownButton({
        width: 210, height: 30
    });
    $("#grid2").on('rowclick', function (event) {
        var args = event.args;
        var row = $("#grid2").jqxGrid('getrowdata', args.rowindex);
        var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + row['BusinessCode'] + '</div>';
        $("#jqxdropdownbutton2").jqxDropDownButton('setContent', dropDownContent);
    });
}
//function companyChange() {
//    $("#jqxdropdownbutton1").jqxDropDownButton('setContent', "");
//    $("#jqxdropdownbutton2").jqxDropDownButton('setContent', "");
//    companyCode = $("#CompanyCode").val();
//    initBorrowTable(companyCode, accountMode);
//}