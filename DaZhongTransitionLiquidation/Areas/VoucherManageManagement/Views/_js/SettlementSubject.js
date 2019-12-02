//所有元素选择器
var selector = {
    $grid: function () { return $("#moduletree") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnEdit: function () { return $("#btnEdit") },
    $btnDelete: function () { return $("#btnDelete") },
    $AddNewBankDataDialog: function () { return $("#AddNewBankDataDialog") },
    $AddNewBankData_OKButton: function () { return $("#AddNewBankData_OKButton") },
    $AddNewBankData_CancelBtn: function () { return $("#AddNewBankData_CancelBtn") },
    $AddBankChannelDialog: function () { return $("#AddBankChannelDialog") },
    $AddBankChannel_OKButton: function () { return $("#AddBankChannel_OKButton") },
    $AddBankChannel_CancelBtn: function () { return $("#AddBankChannel_CancelBtn") },
}

var parentVguid = "";
var hideParentMenu = "";
var vguid = "";
var guid = "";
var isEdit = false;

var $page = function () {

    this.init = function () {
        pageload();
        addEvent();
    }

    function pageload() {
        getModules(function (modules) {
            loadGridTree(modules);
        });
    }
    //所有事件
    function addEvent() {
        initTable();

        getCompanyCode();
        //新增
        selector.$btnAdd().click(function () {
            $("#BusinessType").val("");
            $("#ParentMenu").val("");
            $("#hideParentMenu").val("");
            var checkrow = selector.$grid().jqxTreeGrid('getCheckedRows');
            if (checkrow.length == 0) {
                selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
                selector.$AddNewBankDataDialog().modal("show");
                return;
            }
            if (checkrow.length > 1) {
                jqxNotification("请选择一个节点！", null, "error");
                return;
            } else {
                var business = checkrow[0].BusinessType;
                $("#ParentMenu").val(business);
                $("#hideParentMenu").val(checkrow[0].VGUID);
                isEdit = false;
                vguid = "";
                $("#myModalLabel_title").text("新增数据");
                $(".msg").remove();
                $("#ModuleName").removeClass("input_Validate");
                selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
                selector.$AddNewBankDataDialog().modal("show");
            }
        });
        //编辑
        selector.$btnEdit().click(function () {
            $("#BusinessType").val("");
            $("#ParentMenu").val("");
            $("#hideParentMenu").val("");
            var checkrow = selector.$grid().jqxTreeGrid('getCheckedRows');
            if (checkrow.length != 1) {
                jqxNotification("请选择一个节点！", null, "error");
                return;
            } else {
                $("#myModalLabel_title").text("编辑数据");
                var checkrow = selector.$grid().jqxTreeGrid('getCheckedRows');
                var business = checkrow[0].BusinessType;
                if (checkrow[0].parent != null) {
                    $("#ParentMenu").val(checkrow[0].parent.BusinessType);
                    $("#hideParentMenu").val(checkrow[0].parent.VGUID);
                }
                $("#BusinessType").val(business);
                isEdit = true;
                vguid = checkrow[0].VGUID;
                $("#ModuleName").removeClass("input_Validate");
                selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
                selector.$AddNewBankDataDialog().modal("show");
            }
        })
        //弹出框中的取消按钮
        selector.$AddNewBankData_CancelBtn().on("click", function () {
            selector.$AddNewBankDataDialog().modal("hide");
        });
        //删除
        selector.$btnDelete().click(function () {
            var selection = [];
            var checkrow = selector.$grid().jqxTreeGrid('getCheckedRows');
            for (var i = 0; i < checkrow.length; i++) {
                var rowdata = checkrow[i];
                selection.push(rowdata.VGUID);
            }
            if (selection.length < 1) {
                jqxNotification("请选择您要删除的数据！", null, "error");
            } else {
                WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消", selection);
            }

        });
        //保存
        selector.$AddNewBankData_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate($("#BusinessType"))) {
                validateError++;
            }
            if (!Validate($("#hideParentMenu"))) {
                validateError++;
            }
            var url = "/VoucherManageManagement/SettlementSubject/SaveBusiness?isEdit=";
            if (validateError <= 0) {
                $.ajax({
                    url: url + isEdit,
                    data: {
                        "BusinessType": $("#BusinessType").val(),
                        "VGUID": vguid,
                        "ParentVGUID": $("#hideParentMenu").val()
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
                                //selector.$grid().jqxTreeGrid('updateBoundData');
                                pageload();
                                selector.$AddNewBankDataDialog().modal("hide");
                                break;
                            case "2":
                                jqxNotification("结算项目已存在", null, "error");
                                break;
                        }

                    }
                });
            }
        });
        //清除借贷信息
        $("#Remove1").on("click", function () {
            $("#jqxdropdownbutton1").jqxDropDownButton('setContent', "");
        })
        $("#Remove2").on("click", function () {
            $("#jqxdropdownbutton2").jqxDropDownButton('setContent', "");
        })
        //新增借贷
        $("#btnAddBorrow").on("click", function () {
            if ($("#VGUID").val() == "") {
                jqxNotification("请选择项目！", null, "error");
                return;
            }
            add("B");
        });
        $("#btnAddLoan").on("click", function () {
            if ($("#VGUID").val() == "") {
                jqxNotification("请选择项目！", null, "error");
                return;
            }
            add("L");
        });
        //删除
        $("#btnDelete2").on("click", function () {
            var array = $('#datatable').jqxGrid('getselectedrowindexes');
            var pars = [];
            $(array).each(function (i, v) {
                var value = $('#datatable').jqxGrid('getcell', v, "VGUID");
                if (value != null) {
                    pars.push(value.value);
                }
            });
            if (pars.length == 0) {
                jqxNotification("请选择要删除的数据！", null, "error");
                return;
            }
            $.ajax({
                url: "/VoucherManageManagement/SettlementSubject/DeleteSettlementSubject",
                data: { vguids: pars },
                type: "post",
                success: function (msg) {
                    layer.closeAll('loading');
                    if (msg.IsSuccess == true) {
                        jqxNotification("删除成功！", null, "success");
                        $("#datatable").jqxGrid('updateBoundData');
                    } else {
                        jqxNotification("删除失败！", null, "error");
                    }
                }
            });
        })
        //弹出框中的取消按钮
        selector.$AddBankChannel_CancelBtn().on("click", function () {
            selector.$AddBankChannelDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddBankChannel_OKButton().on("click", function () {
            var borrow = $("#dropDownButtonContentjqxdropdownbutton1")[0].innerText;
            var loan = $("#dropDownButtonContentjqxdropdownbutton2")[0].innerText;
            if (borrow == "null") {
                borrow = "";
            }
            if (loan == "null") {
                loan = "";
            }
            $.ajax({
                url: "/VoucherManageManagement/SettlementSubject/SaveSettlementSubject?isEdit=" + isEdit,
                data: {
                    AccountModeCode: $("#AccountModeCode").val(),
                    AccountModeName: $('#AccountModeCode option:selected').text(),
                    CompanyCode: $("#CompanyCode").val(),
                    CompanyName: $('#CompanyCode option:selected').text(),
                    AccountModeCodeOther: $("#AccountModeCodeOther").val(),
                    AccountModeNameOther: $('#AccountModeCodeOther option:selected').text(),
                    CompanyCodeOther: $("#CompanyCodeOther").val(),
                    CompanyNameOther: $('#CompanyCodeOther option:selected').text(),
                    Borrow: borrow,
                    Loan: loan,
                    VGUID: guid,
                    SettlementVGUID: $("#VGUID").val(),
                    Remark: $("#Remark").val(),
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
                            $("#datatable").jqxGrid('updateBoundData');
                            selector.$AddBankChannelDialog().modal("hide");
                            break;
                        case "2":
                            jqxNotification("该项目下,公司已经存在！", null, "error");
                            break;
                    }

                }
            });
        });
    }
}

function add(type) {
    if (type == "B") {
        $("#BorrowTr").show();
        $("#LoanTr").show();
    } else {
        $("#BorrowTr").hide();
        $("#LoanTr").show();
    }
    $("#jqxdropdownbutton1").jqxDropDownButton('setContent', "");
    $("#jqxdropdownbutton2").jqxDropDownButton('setContent', "");
    $("#Remark").val("");
    $("#Month").val("");
    isEdit = false;
    vguid = "";
    $("#myModalLabel_title").text("新增借/贷方信息");
    selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddBankChannelDialog().modal("show");
    initBorrowTable(companyCode, accountMode);
}
//删除
function dele(selection) {
    $.ajax({
        url: "/CapitalCenterManagement/BusinessTypeSet/DeleteBusiness",
        data: { vguids: selection },
        traditional: true,
        type: "post",
        success: function (msg) {
            switch (msg.Status) {
                case "0":
                    jqxNotification("删除失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("删除成功！", null, "success");
                    //selector.$grid().jqxTreeGrid('updateBoundData');
                    pageload();
                    break;
            }
        }
    });
}

function getModules(callback) {
    $.ajax({
        url: "/VoucherManageManagement/SettlementSubject/GetSettlementSubject",
        type: "get",
        dataType: "json",
        success: function (msg) {
            callback(msg);
        }
    });

}

function loadGridTree(modules) {
    var source =
            {
                dataType: "json",
                dataFields: [
                    { name: 'BusinessType', type: 'string' },
                    { name: 'ParentVGUID', type: 'string' },
                    { name: 'VGUID', type: 'string' }
                ],
                hierarchy:
                {
                    keyDataField: { name: 'VGUID' },
                    parentDataField: { name: 'ParentVGUID' }
                },
                id: 'VGUID',
                localData: modules
            };
    var dataAdapter = new $.jqx.dataAdapter(source);
    selector.$grid().jqxTreeGrid({
        width: '100%',
        height:'612px',
        showHeader: false,
        source: dataAdapter,
        checkboxes: true,
        ready: function () {
            $("#moduletree").jqxTreeGrid('expandAll');
        },
        columns: [
          { text: '营业收入', dataField: 'BusinessType', width: "100%", },
          { text: '', dataField: 'ParentVGUID', width: "100%", hidden: true },
          { text: '', dataField: 'Code', width: "100%", hidden: true },
          { text: '', dataField: 'VGUID', width: "100%", hidden: true },
        ]
    });
    selector.$grid().on('rowClick', function (event) {
        var args = event.args;
        var row = args.row;
        initTable(row.VGUID);
        $("#VGUID").val(row.VGUID);
        if ($(document).scrollTop() == 0) {
            $("html,body").animate({ scrollTop: 0 }, 10);//置顶
        }
    });
}

function initTable(vguid) {
    var source = {
        datafields:
        [
            //{ name: "checkbox", type: null },
            { name: 'AccountModeName', type: 'string' },
            { name: 'CompanyName', type: 'string' },
            { name: 'AccountModeNameOther', type: 'string' },
            { name: 'CompanyNameOther', type: 'string' },
            { name: 'Borrow', type: 'string' },
            { name: 'Loan', type: 'string' },
            { name: 'AccountModeCode', type: 'string' },
            { name: 'CompanyCode', type: 'string' },
            { name: 'AccountModeCodeOther', type: 'string' },
            { name: 'CompanyCodeOther', type: 'string' },
            { name: 'SettlementVGUID', type: 'string' },
            { name: 'Remark', type: 'string' },
            { name: 'VGUID', type: 'string' },
        ],
        datatype: "json",
        id: "VGUID",
        data: { settlementVGUID: vguid },
        url: "/VoucherManageManagement/SettlementSubject/GetSettlementData" //获取数据源的路径
    };
    var typeAdapter = new $.jqx.dataAdapter(source);
    //创建卡信息列表（主表）
    $("#datatable").jqxGrid({
        pageable: false,
        width: "100%",
        height: '570px',
        pageSize: 10,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        groupable: false,
        //groupsexpandedbydefault: true,
        //groups: ['Model', 'ClassType', 'CarType'],
        showgroupsheader: false,
        columnsHeight: 30,
        pagermode: 'simple',
        selectionmode: 'checkbox',
        columns: [
            //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
            { text: '我方账套', datafield: 'AccountModeName', pinned:true, width: 250, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
            { text: '我方公司', datafield: 'CompanyName', width: 200,pinned:true, align: 'center', cellsAlign: 'center' },
            { text: '对方账套', datafield: 'AccountModeNameOther',  width: 250, align: 'center', cellsAlign: 'center'},
            { text: '对方公司', datafield: 'CompanyNameOther', width: 200, align: 'center', cellsAlign: 'center' },
            { text: '借', datafield: 'Borrow', width: 250, align: 'center', cellsAlign: 'center' },
            { text: '贷', datafield: 'Loan', width: 250, align: 'center', cellsAlign: 'center' },
            { text: '摘要', datafield: 'Remark',width: 250, align: 'center', cellsAlign: 'center' },
            { text: 'AccountModeCodeOther', datafield: 'AccountModeCodeOther', hidden: true },
            { text: 'CompanyCodeOther', datafield: 'CompanyCodeOther', hidden: true },
            { text: 'AccountModeCode', datafield: 'AccountModeCode', hidden: true },
            { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
            { text: 'SettlementVGUID', datafield: 'SettlementVGUID', hidden: true },
            { text: 'VGUID', datafield: 'VGUID', hidden: true },
        ]
    });
    $("#datatable").on('rowclick', function (event) {
        var args = event.args;
        // row's bound index.
        var boundIndex = args.rowindex;
        var data = $('#datatable').jqxGrid('getrowdata', boundIndex);
        guid = data.VGUID;
        isEdit = true;
        $("#AccountModeCode").val(data.AccountModeCode);
        $("#CompanyCode").val(data.CompanyCode);
        $("#AccountModeCodeOther").val(data.AccountModeCodeOther);
        $("#CompanyCodeOther").val(data.CompanyCodeOther);
        $("#Remark").val(data.Remark);
        getCompanyCode();
        getCompanyCodeOther();
        if (data.Borrow != null && data.Borrow != "") {
            $("#BorrowTr").show();
            $("#LoanTr").hide();
        } else {
            $("#BorrowTr").hide();
            $("#LoanTr").show();
        }
        initBorrowTable(data.CompanyCode, data.AccountModeCode);
        var val = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + data.Borrow + '</div>';
        $("#jqxdropdownbutton1").jqxDropDownButton('setContent', val);
        var val2 = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + data.Loan + '</div>';
        $("#jqxdropdownbutton2").jqxDropDownButton('setContent', val2);
        $("#myModalLabel_title2").text("编辑借/贷方信息");
        selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
        selector.$AddBankChannelDialog().modal("show");
    });
}

function detailFunc(row, column, value, rowData) {
    var container = "<div style=\"text-decoration: underline;text-align: center;margin-top: 4px;color: #333;\">" + value + "</div>";
    return container;
}

$(function () {
    var page = new $page();
    page.init();
});

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

function getCompanyCode() {
    accountMode = $("#AccountModeCode").val();
    $.ajax({
        url: "/HomePage/CompanyHomePage/GetCompanyCode",
        data: { accountMode: accountMode },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#CompanyCode', msg, "CompanyCode", "Abbreviation");
            //$("#CompanyCode").prepend("<option value=\"\" selected='true'>请选择</>");
        }
    });
    companyCode = $("#CompanyCode").val();
}
function companyChange() {
    $("#jqxdropdownbutton1").jqxDropDownButton('setContent', "");
    $("#jqxdropdownbutton2").jqxDropDownButton('setContent', "");
    companyCode = $("#CompanyCode").val();
    initBorrowTable(companyCode, accountMode);
}
function getCompanyCodeOther() {
    var accountModeOther = $("#AccountModeCodeOther").val();
    $.ajax({
        url: "/HomePage/CompanyHomePage/GetCompanyCode",
        data: { accountMode: accountModeOther },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#CompanyCodeOther', msg, "CompanyCode", "Abbreviation");
            //$("#CompanyCode").prepend("<option value=\"\" selected='true'>请选择</>");
        }
    });
}
function companyChangeOther() {
    //$("#jqxdropdownbutton1").jqxDropDownButton('setContent', "");
    //$("#jqxdropdownbutton2").jqxDropDownButton('setContent', "");
    //companyCode = $("#CompanyCodeOther").val();
    //initBorrowTable(companyCode, accountMode);
}