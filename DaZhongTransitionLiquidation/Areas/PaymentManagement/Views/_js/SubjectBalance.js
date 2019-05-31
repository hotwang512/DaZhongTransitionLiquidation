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

    $txtDatedAmount_Dialog: function () { return $("#txtDatedAmount_Dialog") },
    $txtDatedTime_Dialog: function () { return $("#txtDatedTime_Dialog") },

    $txtBankAccountName_Dialog: function () { return $("#txtBankAccountName_Dialog") },
    $txtBankAccount_Dialog: function () { return $("#txtBankAccount_Dialog") },
    $txtBank_Dialog: function () { return $("#txtBank_Dialog") },


    $txtChannel_Dialog: function () { return $("#txtChannel_Dialog") },
    $txtRemark_Dialog: function () { return $("#txtRemark_Dialog") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end

var isEdit = false;
var vguid = "";
var companyCode = $("#CompanyCode").val();
var accountModeCode = $("#AccountModeCode").val();
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        getCompanyCode();
        //加载列表数据
        initTable();

        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            selector.$txtDatedTime().val("");
        });

        selector.$txtBankAccountName_Dialog().on("change", function () {
            var backAccount = $(this).val();
            var option = $("#" + backAccount);
            selector.$txtBankAccount_Dialog().val(backAccount);
            selector.$txtBank_Dialog().val(option.attr("bank"));
            selector.$txtChannel_Dialog().val(option.attr("channel"));
        });

        //弹出框中的取消按钮
        selector.$AddNewBankData_CancelBtn().on("click", function () {
            selector.$AddNewBankDataDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddNewBankData_OKButton().on("click", function () {
            $.ajax({
                url: "/PaymentManagement/SubjectBalance/SaveSubjectBalance",
                data: {
                    Balance: $("#Balance").val(),
                    Code: $("#AllCode").val(),
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
                })
        })
        //});

        //切换公司值
        $('#CompanyCode').on('change', function (event) {
            companyCode = $("#CompanyCode").val();
            console.log(companyCode);
            initTable();
            //$("#jqxTable2").jqxTreeGrid('updateBoundData');
        })
        $('#AccountModeCode').on('change', function (event) {
            accountModeCode = $("#AccountModeCode").val();
            console.log(accountModeCode);
            initTable();
            //$("#jqxTable2").jqxTreeGrid('updateBoundData');
        })
        //切换月份
        $('#Month').on('change', function (event) {
            initTable();
        })
    }; //addEvent end
};


function initTable() {
    var source =
        {
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
            //cache: false,
            async: false,
            id: "VGUID",
            data: { companyCode: companyCode, accountModeCode: accountModeCode },
            url: "/PaymentManagement/SubjectBalance/GetSubjectBalance"    //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    //创建卡信息列表（主表）
    $("#jqxTable").jqxDataTable({
        pageable: true,
        width: "100%",
        height: 450,
        pageSize: 15,
        serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        columnsHeight: 40,
        columns: [
            //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
            { text: '编码', datafield: 'BusinessCode', width: 250, align: 'center', cellsAlign: 'center', cellsRenderer: detailFuncs },
            { text: '科目段', datafield: 'Company', width: 260, align: 'center', cellsAlign: 'center' },
            { text: '核算段', datafield: 'Accounting', width: 180, align: 'center', cellsAlign: 'center', },
            { text: '成本中心段', datafield: 'CostCenter', width: 180, align: 'center', cellsAlign: 'center', },
            { text: '备用1', datafield: 'SpareOne', width: 180, align: 'center', cellsAlign: 'center', },
            { text: '备用2', datafield: 'SpareTwo', width: 180, align: 'center', cellsAlign: 'center', },
            { text: '往来段', datafield: 'Intercourse', width: 180, align: 'center', cellsAlign: 'center', },
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

}

function detailFuncs(row, column, value, rowData) {
    var container = "";
    if (selector.$EditPermission().val() == "1") {
        container = "<a href='#' onclick=edit('" + rowData.Balance + "','" + rowData.BusinessCode + "') style=\"text-decoration: underline;color: #333;\">" + rowData.BusinessCode + "</a>";
    } else {
        container = "<span>" + rowData.BusinessCode + "</span>";
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

function edit(balance, allCode) {
    $("#Balance").val("");
    $("#AllCode").val("");
    isEdit = true; 
    $("#Balance").val(balance);
    if (balance == "null") {
        $("#Balance").val("");
    }
    $("#AllCode").val(allCode);
    //$("#AddNewBankDataDialog table tr").eq(1).hide();
    $(".msg").remove();
    $("#Balance").removeClass("input_Validate");

    selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewBankDataDialog().modal("show");
}

$(function () {
    var page = new $page();
    page.init();
});

function getCompanyCode() {
    var accountMode = $("#AccountModeCode").val();
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
    initTable();
}