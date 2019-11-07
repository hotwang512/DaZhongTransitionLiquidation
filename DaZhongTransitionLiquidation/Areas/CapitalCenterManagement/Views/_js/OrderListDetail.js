//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $pushPeopleDropDownButton: function () { return $("#pushPeopleDropDownButton") },
    $AddNewBankDataDialog: function () { return $("#AddNewBankDataDialog") },
    $AddNewBankData_OKButton: function () { return $("#AddNewBankData_OKButton") },
    $AddNewBankData_CancelBtn: function () { return $("#AddNewBankData_CancelBtn") },
    $pushTree: function () { return $("#pushTree") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var index = 0;//切换借贷
//var CompanyCode = loadCompanyCode("A");
//var collectionCompany = loadCollectionCompany();
var collectionCompany = initiSelectPurchaseItem();
//var businessType = loadBusinessType();
var AccountSection = null;
var CostCenterSection = null;
var SpareOneSection = null;
var SpareTwoSection = null;
var IntercourseSection = null;
var selectIndex = 0;//生成块的数量
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        getCompanyCode();
        var guid = $.request.queryString().VGUID;
        initOrganization();
        //getUserCompanySet(guid);
        $("#VGUID").val(guid);
        initSettingTable();
        var tradeDate = new Date();
        var month = (tradeDate.getMonth() + 1) > 9 ? (tradeDate.getMonth() + 1) : "0" + (tradeDate.getMonth() + 1);
        var day = tradeDate.getDate() > 9 ? tradeDate.getDate() : "0" + tradeDate.getDate();
        var date = tradeDate.getFullYear() + "-" + month + "-" + day;     //获取当前日期
        $("#SubmitDate").val(date);
        if (guid != "" && guid != null) {
            getOrderListDetail();
        } else {
            $("#hideButton").show();
        }

        //取消
        $("#btnCancel").on("click", function () {
            history.go(-1);
        })
        //双击选择科目
        $("#jqxSubjectSection").on('rowDoubleClick', function (event) {
            var args = event.args;
            var row = args.row;
            var key = args.key;
            $("#SubjectName").val(row.Descrption);
            $("#SubjectSection").val(row.Code);
            $("#AddCompanyDialog").modal("hide");
            var code = $("#CompanyCode").val();
            AccountSection = loadCompanyCode("C", code, row.Code);
            CostCenterSection = loadCompanyCode("D", code, row.Code);
            SpareOneSection = loadCompanyCode("E", code, row.Code);
            SpareTwoSection = loadCompanyCode("F", code, row.Code);
            IntercourseSection = loadCompanyCode("G", code, row.Code);
            loadSelectFun();
        });
        //保存
        $("#btnSave").on("click", function () {
            //var params = [];
            //for (var i = 0; i < $(".permissions").length; i++) {
            //    params.push({
            //        "Isable": $(".permission")[i].checked,
            //        "PayBank": $(".PayBank").eq(i).text(),
            //        "PayAccount": $(".PayAccount").eq(i).text(),
            //        "PayBankAccountName": $(".PayBankAccountName").eq(i).text(),
            //        "Borrow": $(".Borrow").eq(i).text(),
            //        "Loan": $(".Loan").eq(i).text(),
            //        "KeyData": $(".KeyData")[i].value,
            //        "OrderVGUID": $("#VGUID").val()
            //    });
            //}
            //var orderDetailValue = JSON.stringify(params);
            $.ajax({
                url: "/CapitalCenterManagement/OrderListDetail/SaveOrderListDetail",
                //data: { vguids: selection },
                data: {
                    "VGUID": $("#VGUID").val(),
                    "OrderDetailValue": $("#VGUID").val(),
                    "BusinessType": $("#pushPeopleDropDownButton").val(),
                    "BusinessProject": $("#BusinessProject").text(),//拼接类型
                    "BusinessSubItem1": $("#BusinessSubItem1").val(),//拼接编号
                    "Status": $("#Status").val(),
                    "Founder": $("#LoginName").val(),
                    "CollectionCompany": $("#CollectionCompany").val(),
                    "PaymentMethod": $("#PaymentMethod").val(),
                    "CompanyCode": $("#LoginCompanyCode").val(),
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
        })
        $('#CollectionCompany').on('select', function (event) {
            var args = event.args;
            var item = args.item;
            var guid = $("#VGUID").val();
            if (args) {
                var label = item.label;
                //collectionBankChange(label);
                initCustomerBank(label);
                //companyChange(value);
            }
        });
        //新增账套信息
        var vguids = [];
        $("#btnAdd").on("click", function () {
            selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
            selector.$AddNewBankDataDialog().modal("show");
        });
        selector.$AddNewBankData_CancelBtn().on("click", function () {
            selector.$AddNewBankDataDialog().modal("hide");
        });
        //确认
        $("#AddNewBankData_OKButton").on("click", function () {
            //for (var i = 0; i < $(".Borrow").length; i++) {
            //    if ($(".Borrow")[i].getAttribute('for') == vguids[0]) {
            //        //var len = $("#dropDownButtonContentjqxdropdownbutton1")[0].innerText.length;
            //        //var val = $("#dropDownButtonContentjqxdropdownbutton1")[0].innerText.substring(0, len - 1);
            //        var val = $("#dropDownButtonContentjqxdropdownbutton1")[0].innerText;
            //        $(".Borrow").eq(i).text(val);
            //        //var len2 = $("#dropDownButtonContentjqxdropdownbutton2")[0].innerText.length;
            //        //var val2 = $("#dropDownButtonContentjqxdropdownbutton2")[0].innerText.substring(0, len2 - 1);
            //        var val2 = $("#dropDownButtonContentjqxdropdownbutton2")[0].innerText;
            //        $(".Loan").eq(i).text(val2);
            //        var val3 = $("#PayBank").val();
            //        $(".PayBank").eq(i).text(val3)
            //        var val4 = $("#PayAccount").val();
            //        $(".PayAccount").eq(i).text(val4)
            //        var val5 = $("#PayBankAccountName").val();
            //        $(".PayBankAccountName").eq(i).text(val5)
            //    }
            //}
            var val = $("#dropDownButtonContentjqxdropdownbutton1")[0].innerText;
            var val2 = $("#dropDownButtonContentjqxdropdownbutton2")[0].innerText;
            $.ajax({
                url: "/CapitalCenterManagement/OrderListDetail/SaveUserCompanySet",
                //data: { vguids: selection },
                data: {
                    "AccountModeCode": $("#AccountModeCode").val(),
                    "CompanyCode": $("#CompanyCode").val(),
                    "PayBank": $("#PayBank").val(),
                    "PayAccount": $("#PayAccount").val(),
                    "PayBankAccountName": $("#PayBankAccountName").val(),
                    "AccountType": $("#AccountType").val(),
                    "Borrow": val,
                    "Loan": val2,
                    "OrderVGUID": $("#VGUID").val(),
                    "VGUID": $("#orderVguid").val()
                },
                traditional: true,
                type: "post",
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("该公司已存在配置！", null, "error");
                            break;
                        case "1":
                            jqxNotification("保存成功！", null, "success");
                            $("#jqxSettingTable").jqxGrid('updateBoundData');
                            break;
                    }
                }
            });
            selector.$AddNewBankDataDialog().modal("hide");
        });

        //编辑启用
        $("#jqxSettingTable").on('cellendedit', function (event) {
            var args = event.args;
            var value = args.value;
            var oldvalue = args.oldvalue;
            var rowData = args.row;
            $.ajax({
                url: "/CapitalCenterManagement/OrderListDetail/UpdataIsable",
                //data: { vguids: selection },
                data: { vguids: rowData.VGUID, ischeck: args.value },
                type: "post",
                success: function (msg) {
                    $("#jqxSettingTable").jqxGrid('updateBoundData');
                }
            });
        });
        $("#jqxCustomerBankInfo").on('cellendedit', function (event) {
            var args = event.args;
            var value = args.value;
            var oldvalue = args.oldvalue;
            var rowData = args.row;
            $.ajax({
                url: "/CapitalCenterManagement/OrderListDetail/UpdataCustomerIsable",
                //data: { vguids: selection },
                data: { vguids: rowData.VGUID, ischeck: args.value, orderVguid: $("#VGUID").val(), purchaseItemID: $("#CollectionCompany").val(), purchaseItem: $("#CollectionCompany").text() },
                type: "post",
                success: function (msg) {
                    $("#jqxCustomerBankInfo").jqxGrid('updateBoundData');
                }
            });
        });
        //删除账套公司
        $("#btnDelete").on("click", function () {
            var array = $("#jqxSettingTable").jqxGrid('getselectedrowindexes');
            var pars = [];
            $(array).each(function (i, v) {
                try {
                    var value = $("#jqxSettingTable").jqxGrid('getcell', v, "VGUID");
                    pars.push(value.value);
                } catch (e) {

                }

            });
            if (pars.length <= 0) {
                jqxNotification("请选择一条数据！", null, "error"); return;
            }
            if (pars.length > 0) {
                $.ajax({
                    url: "/CapitalCenterManagement/OrderListDetail/DeleteAccountMode",
                    type: "post",
                    dataType: "json",
                    data: {
                        VGUID: pars
                    },
                    async: false,
                    success: function (msg) {
                        if (msg.Status == "1") {
                            jqxNotification("删除成功！", null, "success");
                            $("#jqxSettingTable").jqxGrid('updateBoundData');
                        }
                        else {
                            jqxNotification("删除失败！", null, "error");
                        }
                    }
                });

            }
        });
        //清空
        $("#btnClear").on("click", function () {
            $("#CollectionCompany").jqxDropDownList('clearSelection');
            $("#CollectionAccount").val("");
            $("#CollectionBank").val("");
            $("#CollectionBankAccount").val("");
            $("#CollectionBankAccountName").val("");
        })

        $("#CompanyCode").on("change", function () {
            var accountModeCode = $("#AccountModeCode").val();
            var companyCode = $("#CompanyCode").val();
            getPayBankInfo(companyCode, accountModeCode, '', '', '');
        })
    }; //addEvent end

    function getOrderListDetail() {
        $.ajax({
            url: "/CapitalCenterManagement/OrderListDetail/GetOrderListDetail",
            data: {
                "vguid": $("#VGUID").val(),
            },
            type: "post",
            dataType: "json",
            success: function (msg) {
                $("#Status").val(msg.Status);
                $("#BusinessType").val(msg.BusinessType);
                $("#BusinessProject").text(msg.BusinessProject);
                $("#BusinessSubItem1").val(msg.BusinessSubItem1);
                $("#CollectionCompany").val(msg.CollectionCompany);
                if (msg.PaymentMethod == "" || msg.PaymentMethod == null) {
                    $("#PaymentMethod").val("银行");
                } else {
                    $("#PaymentMethod").val(msg.PaymentMethod);
                }
            }
        });
    }

};

//选择科目段
function searchSubject(event) {
    var companyCode = $("#CompanyCode").val();
    if (companyCode == "") {
        jqxNotification("请选择公司段", null, "error");
        return;
    }
    initSubjectTable(companyCode);
    $("#AddCompanyDialog").modal({ backdrop: "static", keyboard: false });
    $("#AddCompanyDialog").modal("show");
}
function initSubjectTable(companyCode) {
    var source = {
        datafields:
        [
             { name: 'Code', type: 'string' },
             { name: 'ParentCode', type: 'string' },
             { name: 'Descrption', type: 'string' },
             { name: 'SectionVGUID', type: 'string' },
             { name: 'VGUID', type: 'string' },
             { name: 'Status', type: 'string' },
             { name: 'Remark', type: 'string' },
             { name: 'IsAccountingCode', type: 'string' },
             { name: 'IsCostCenterCode', type: 'string' },
             { name: 'IsSpareOneCode', type: 'string' },
             { name: 'IsSpareTwoCode', type: 'string' },
             { name: 'IsIntercourseCode', type: 'string' },
        ],
        hierarchy:
        {
            keyDataField: { name: 'Code' },
            parentDataField: { name: 'ParentCode' }
        },
        datatype: "json",
        id: "VGUID",
        data: { companyCode: companyCode },
        url: "/PaymentManagement/SubjectSection/GetCompanySectionByCode"    //获取数据源的路径
    };
    var typeAdapter = new $.jqx.dataAdapter(source);
    $("#jqxSubjectSection").jqxTreeGrid({
        pageable: false,
        width: 460,
        height: 300,
        pageSize: 9999999,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        //theme: "energyblue",
        columnsHeight: 30,
        checkboxes: false,
        hierarchicalCheckboxes: false,
        columns: [
            { text: '编码', datafield: 'Code', width: 150, align: 'center', cellsAlign: 'left' },
            { text: '描述', datafield: 'Descrption', align: 'center', cellsAlign: 'center' },
            { text: 'ParentCode', datafield: 'ParentCode', hidden: true },
            { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
            { text: 'VGUID', datafield: 'VGUID', hidden: true },
        ]
    });
}
function initBusinessTypeName() {
    var source = {
        datafields:
        [
            { name: 'VGUID', type: 'string' },
            { name: 'ListKey', type: 'string' },
            { name: 'BusinessTypeName', type: 'string' },
        ],
        datatype: "json",
        id: "",
        data: {},
        url: "/CapitalCenterManagement/OrderListDetail/GetBusinessType"    //获取数据源的路径
    };
    var typeAdapter = new $.jqx.dataAdapter(source);
    $("#jqxBusinessTypeName").jqxDataTable({
        pageable: false,
        //width: 400,
        height: 300,
        pageSize: 9999999,
        pagerButtonsCount: 10,
        source: typeAdapter,
        columnsHeight: 30,
        columns: [
            { text: '业务类型', datafield: 'BusinessTypeName', align: 'center', cellsAlign: 'center' },
            { text: '', datafield: 'ListKey', align: 'center', cellsAlign: 'center', hidden: true },
            { text: '', datafield: 'VGUID', align: 'center', cellsAlign: 'center', hidden: true },
        ]
    });
    $('#jqxBusinessTypeName').on('rowClick', function (event) {
        // event args.
        var args = event.args;
        // row data.
        var row = args.row;
        $("#BusinessTypeName").val(row.BusinessTypeName);
        $("#BusinessVGUID").val(row.VGUID);
    });
}
function gradeChange(event) {
    $("#SubjectSection").val("");
    $("#SubjectName").val("");
    $("#AccountSection").val("");
    $("#CostCenterSection").val("");
    $("#SpareOneSection").val("");
    $("#SpareTwoSection").val("");
    $("#IntercourseSection").val("");
}
function collectionBankChange(label) {
    $.ajax({
        url: "/CapitalCenterManagement/OrderListDetail/GetCollectionBankChange",
        async: false,
        data: { "CollectionCompany": label },
        type: "post",
        success: function (result) {
            //uiEngineHelper.bindSelect('#CollectionBankAccountName', result, "VGUID", "BankAccountName");
            //$("#CollectionBankAccountName").prepend("<option value=\"\" selected='true'>请选择</>");

        }
    });
}
//function companyChange() {
//    var value = $("#CollectionBankAccountName").val();
//    if (value == "") {
//        $("#CollectionAccount").val("");
//        $("#CollectionBank").val("");
//        $("#CollectionBankAccount").val("");
//        return;
//    }
//    $.ajax({
//        url: "/CapitalCenterManagement/OrderListDetail/GetCompanyChange",
//        //async: false,
//        data: { CollectionCompany: value },
//        type: "post",
//        success: function (result) {
//            var values = result[0];
//            $("#CollectionAccount").val(values.BankAccount);
//            $("#CollectionBank").val(values.Bank);
//            $("#CollectionBankAccount").val(values.BankNo);
//        }
//    });
//}
function payBankChange() {
    var payBankValue = $("#PayBank").val();
    $.ajax({
        url: "/CapitalCenterManagement/OrderListDetail/GetBankInfo",
        //async: false,
        data: { PayBank: payBankValue },
        type: "post",
        success: function (result) {
            $("#PayAccount").val(result.BankAccount);
            $("#PayBankAccountName").val(result.BankAccountName);
            $("#AccountType").val(result.AccountType);
        }
    });
}
function loadSelectFun() {

    var id1 = "#AccountSection";
    var id2 = "#CostCenterSection";
    var id3 = "#SpareOneSection";
    var id4 = "#SpareTwoSection";
    var id5 = "#IntercourseSection";

    uiEngineHelper.bindSelect(id1, AccountSection, "Code", "Descrption");
    uiEngineHelper.bindSelect(id2, CostCenterSection, "Code", "Descrption");
    uiEngineHelper.bindSelect(id3, SpareOneSection, "Code", "Descrption");
    uiEngineHelper.bindSelect(id4, SpareTwoSection, "Code", "Descrption");
    uiEngineHelper.bindSelect(id5, IntercourseSection, "Code", "Descrption");
}
//function loadCompanyCode(name, companyCode, subjectCode) {
//    var url = "/VoucherManageManagement/VoucherListDetail/GetSelectSection";
//    var value = null;
//    $.ajax({
//        url: url,
//        async: false,
//        data: { name: name, companyCode: companyCode, subjectCode: subjectCode },
//        type: "post",
//        success: function (result) {
//            value = result;
//        }
//    });
//    return value;
//}

//function loadCollectionCompany() {
//    var url = "/CapitalCenterManagement/OrderListDetail/GetCollectionCompany";
//    var source =
//                {
//                    datatype: "json",
//                    datafields: [
//                        { name: 'VGUID' },
//                        { name: 'CompanyOrPerson' }
//                    ],
//                    url: url,
//                    async: true
//                };
//    var dataAdapter = new $.jqx.dataAdapter(source);
//    $('#CollectionCompany').jqxDropDownList({
//        filterable: true, selectedIndex: 0, source: dataAdapter, displayMember: "CompanyOrPerson", valueMember: "VGUID",
//        itemHeight: '30px', height: '20px', width: '176px', placeHolder: "请选择"
//    });
//}
function initiSelectPurchaseItem() {
    debugger;
    var url = "/CapitalCenterManagement/OrderListDetail/GetPurchaseItem";
    var source =
    {
        datatype: "json",
        datafields: [
            { name: 'VGUID' },
            { name: 'PurchaseGoods' }
        ],
        url: url,
        async: true
    };
    var dataAdapter = new $.jqx.dataAdapter(source);
    $('#CollectionCompany').jqxDropDownList({
        filterable: true, selectedIndex: 0, source: dataAdapter, displayMember: "PurchaseGoods", valueMember: "VGUID",
        itemHeight: '30px', height: '20px', width: '176px', placeHolder: "请选择"
    });
}

//function loadBusinessType() {
//    var url = "/CapitalCenterManagement/OrderListDetail/GetBusinessType";
//    $.ajax({
//        url: url,
//        async: false,
//        data: {},
//        type: "post",
//        success: function (result) {
//            uiEngineHelper.bindSelect('#BusinessType', result, "ListKey", "BusinessTypeName");
//            $("#BusinessType").prepend("<option value=\"\" selected='true'>请选择</>");
//        }
//    });
//}

$(function () {
    var page = new $page();
    page.init();
});

var addOption1 = function (select, txt, value, num) {
    select.add(new Option(txt, value), num);
}
var records = null;
function initOrganization() {
    $.ajax({
        url: "/CapitalCenterManagement/OrderListDetail/GetBusinessTypeTree",
        type: "post",
        dataType: "json",
        success: function (msg) {
            //推送接收人下拉框
            selector.$pushPeopleDropDownButton().jqxDropDownButton({
                width: 175,
                height: 25
            });
            //推送接收人下拉框(树形结构)
            selector.$pushTree().on('select', function (event) {
                var args = event.args;
                var item = selector.$pushTree().jqxTree('getItem', args.element);

                result = "";
                results = "";
                $("#BusinessProject").text("");
                $("#BusinessSubItem1").val("");//编号
                //if (selector.$currentUserDepartment().val().indexOf(item.id) == -1) {
                //    jqxNotification("请选择本公司及其子部门！", null, "error");
                //    return false;
                //}
                //selector.$DepartmentVguid().val(item.id);
                var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 5px;">' + item.label + '</div>';
                selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', dropDownContent);

                var businessText = getBusinessText(item);
                //var businessCodeText = getBusinessCodeText(item.prevItem) + item.value;
                $("#BusinessProject").text(businessText);
                $("#BusinessSubItem1").val(results);//编号

            });
            var source =
               {
                   datatype: "json",
                   datafields: [
                       { name: 'Code' },
                       { name: 'BusinessName' },
                       { name: 'ParentVGUID' },
                       { name: 'VGUID' }
                   ],
                   id: 'Vguid',
                   localdata: msg
               };
            var dataAdapter = new $.jqx.dataAdapter(source);
            // perform Data Binding.
            dataAdapter.dataBind();
            records = dataAdapter.getRecordsHierarchy('VGUID', 'ParentVGUID', 'items',
               [
                   {
                       name: 'Code',
                       map: 'value'
                   },
                   {
                       name: 'BusinessName',
                       map: 'label'
                   },
                   {
                       name: 'VGUID',
                       map: 'id'
                   },
                   {
                       name: 'ParentVGUID',
                       map: 'parentId'
                   }
               ]);
            selector.$pushTree().jqxTree({ source: records, width: '200px', height: '250px', incrementalSearch: true });//, checkboxes: true
            selector.$pushTree().jqxTree('expandAll');
        }
    });
}
var result = "";
var results = "";
//var level = "";
//var currentLever = "";
function getBusinessText(parentItem) {
    result = parentItem.label;
    results = parentItem.value;
    var prevItem = parentItem.prevItem;
    while (prevItem != null) {
        if (parentItem.level > prevItem.level) {
            result = prevItem.label + "|" + result;
            results = prevItem.value + "|" + results;
            parentItem = prevItem;
            prevItem = parentItem.prevItem;
        } else {
            prevItem = prevItem.prevItem;
        }
    }
    //if (parentItem.level != 0) {

    //} else {
    //    result = parentItem.label + "|" + result;
    //}
    return result;
}

function initTable(companyCode, accountModeCode) {
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
        data: { companyCode: companyCode, accountModeCode: accountModeCode },
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
        width: 300, height: 30
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
        width: 300, height: 30
    });
    $("#grid2").on('rowclick', function (event) {
        var args = event.args;
        var row = $("#grid2").jqxGrid('getrowdata', args.rowindex);
        var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + row['BusinessCode'] + '</div>';
        $("#jqxdropdownbutton2").jqxDropDownButton('setContent', dropDownContent);
    });
}
function checkboxOnclick(event) {
    for (var i = 0; i < $(".permissions").length; i++) {
        var id = $(".permissions")[i].getAttribute('pageid');
        if (event.getAttribute('pageid') == id) {
            if (!event.checked) {
                $(".permissions").eq(i).prop("checked", false);
            } else {
                $(".permissions").eq(i).prop("checked", true);
            }
        } else {
            $(".permissions").eq(i).prop("checked", false);
        }
    }
}

function getUserCompanySet(guid) {
    var url = "/CapitalCenterManagement/OrderListDetail/GetUserCompanySet";
    $.ajax({
        url: url,
        async: false,
        data: { orderVguid: guid },
        type: "post",
        success: function (result) {
            for (var i = 0; i < result.length; i++) {
                $(".permission").eq(i).prop("checked", result[i].Isable);
                $(".PayBank").eq(i).text(result[i].PayBank);
                $(".PayAccount").eq(i).text(result[i].PayAccount);
                $(".PayBankAccountName").eq(i).text(result[i].PayBankAccountName);
                $(".Borrow").eq(i).text(result[i].Borrow);
                $(".Loan").eq(i).text(result[i].Loan);
            }
            //外部div高度随表格高度递增
            var heigth = $(".KeyData").length * 50 + 120;
            document.getElementById('divList').style.height = "" + heigth + "px";
        }
    });
}

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
    initTable(companyCode, accountMode);
    getPayBankInfo(companyCode, accountMode, '', '', '', '');
}

//付款银行信息（默认）
function getPayBankInfo(companyCode, accountModeCode, val3, val4, val5, val6) {
    var url = "/CapitalCenterManagement/OrderListDetail/GetPayBankInfo";
    $.ajax({
        url: url,
        async: false,
        data: { companyCode: companyCode, accountModeCode: accountModeCode },
        type: "post",
        success: function (result) {
            uiEngineHelper.bindSelect("#PayBank", result, "BankName", "BankName");
            if (result.length == 0) {
                $("#PayAccount").val("");
                $("#PayBankAccountName").val("");
            } else {
                if (val3 != "") {
                    $("#PayBank").val(val3);
                    $("#PayAccount").val(val4);
                    $("#PayBankAccountName").val(val5);
                    $("#AccountType").val(val6)
                } else {
                    $("#PayAccount").val(result[0].BankAccount);
                    $("#PayBankAccountName").val(result[0].BankAccountName);
                    $("#AccountType").val(result[0].AccountType);
                }
            }
        }
    });
}

function initSettingTable() {
    var source = {
        datafields:
        [
            { name: 'VGUID', type: 'string' },
            { name: 'OrderVGUID', type: 'string' },
            { name: 'AccountModeName', type: 'string' },
            { name: 'CompanyName', type: 'string' },
            { name: 'Isable', type: 'bool' },
            { name: 'PayBank', type: 'string' },
            { name: 'PayAccount', type: 'string' },
            { name: 'PayBankAccountName', type: 'string' },
            { name: 'AccountType', type: 'string' },
            { name: 'Borrow', type: 'string' },
            { name: 'Loan', type: 'string' },
            { name: 'AccountModeCode', type: 'string' },
            { name: 'CompanyCode', type: 'string' },
        ],
        datatype: "json",
        id: "",
        data: { "OrderVGUID": $("#VGUID").val() },
        url: "/CapitalCenterManagement/OrderListDetail/GetSettingTable"    //获取数据源的路径
    };
    var typeAdapter = new $.jqx.dataAdapter(source);
    $("#jqxSettingTable").jqxGrid({
        pageable: false,
        width: "100%",
        autoheight: false,
        height: 200,
        pageSize: 15,
        //serverProcessing: true,
        //pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        //pagermode: 'checkbox',
        selectionmode: "checkbox",
        columnsHeight: 30,
        editable: true,
        columns: [
            { text: '启用', datafield: 'Isable', width: 60, align: 'center', cellsAlign: 'center', columntype: 'checkbox' },
            { text: '账套', datafield: 'AccountModeName', width: 180, pinned: true, align: 'center', cellsAlign: 'center', editable: false, cellsRenderer: editBankFunc },
            { text: '公司', datafield: 'CompanyName', width: 400, pinned: true, align: 'center', cellsAlign: 'center', editable: false },
            { text: '开户行', datafield: 'PayBank', width: 180, align: 'center', cellsAlign: 'center', editable: false },
            { text: '账号', datafield: 'PayAccount', width: 180, align: 'center', cellsAlign: 'center', editable: false },
            { text: '户名', datafield: 'PayBankAccountName', width: 300, align: 'center', cellsAlign: 'center', editable: false },
            { text: '账户类型', datafield: 'AccountType', width: 120, align: 'center', cellsAlign: 'center', editable: false },
            { text: '借', datafield: 'Borrow', align: 'center', width: 200, cellsAlign: 'center', editable: false },
            { text: '贷', datafield: 'Loan', align: 'center', width: 200, cellsAlign: 'center', editable: false },
            { text: '', datafield: 'AccountModeCode', align: 'center', cellsAlign: 'center', hidden: true },
            { text: '', datafield: 'CompanyCode', align: 'center', cellsAlign: 'center', hidden: true },
            { text: '', datafield: 'VGUID', align: 'center', cellsAlign: 'center', hidden: true },
            { text: '', datafield: 'OrderVGUID', align: 'center', cellsAlign: 'center', hidden: true },
        ]
    });
    $("#jqxSettingTable").on("rowclick", function (event) {
        if ($("#valEdit").val() == "true") {
            var args = event.args;
            // row's bound index.
            var rowdata = args.row.bounddata;
            if (rowdata.level != 0) {
                editBank(rowdata.VGUID, rowdata.AccountModeCode, rowdata.CompanyCode, rowdata.PayBank, rowdata.PayAccount,
                    rowdata.PayBankAccountName, rowdata.AccountType, rowdata.Borrow, rowdata.Loan);
            }
        }
    });
    if ($("#valEnable").val() != "true") {
        $("#jqxSettingTable").jqxGrid({ editable: false });
        //$('#jqxSettingTable').jqxGrid('setcolumnproperty', 'Isable', 'editable', false);
    }
}

function editBankFunc(row, columnfield, value, defaulthtml, columnproperties) {
    var container = "";
    if ($("#valEdit").val() == "true") {
        container = "<div style=\"text-decoration: underline;text-align: center;margin-top: 4px;color: #333;\">" + value + "</div>";
    } else {
        container = "<span>" + value + "</span>";
    }
    return container;
}

function editBank(guid, accountModeCode, companyCode, payBank, payAccount, payBankAccountName, accountType, borrow, loan) {
    $("#PayBank").val("");
    $("#PayAccount").val("");
    $("#PayBankAccountName").val("");
    $("#AccountModeCode").val(accountModeCode);
    $("#AccountType").val("");
    $("#CompanyCode").val(companyCode);
    $("#orderVguid").val(guid)
    isEdit = true;
    //vguid = guid;
    if (payBank == null || payBank == "null") {
        payBank = "";
    }
    if (payAccount == null || payAccount == "null") {
        payAccount = "";
    }
    if (payBankAccountName == null || payBankAccountName == "null") {
        payBankAccountName = "";
    }
    if (AccountType == null || AccountType == "null") {
        AccountType = "";
    }
    if (borrow == null || borrow == "null") {
        borrow = "";
    }
    if (loan == null || loan == "null") {
        loan = "";
    }
    $("#PayBank").val(payBank);
    $("#PayAccount").val(payAccount);
    $("#PayBankAccountName").val(payBankAccountName);
    $("#AccountType").val(accountType);
    $("#myModalLabel_title").text("编辑数据");
    //$("#AddNewBankDataDialog table tr").eq(1).hide();
    $(".msg").remove();
    initTable(companyCode, accountModeCode);
    var val = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + borrow + '</div>';
    $("#jqxdropdownbutton1").jqxDropDownButton('setContent', val);
    var val2 = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + loan + '</div>';
    $("#jqxdropdownbutton2").jqxDropDownButton('setContent', val2);
    selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewBankDataDialog().modal("show");
}

function initCustomerBank(label) {
    var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'CompanyOrPerson', type: 'string' },
                    { name: 'BankAccountName', type: 'string' },
                    { name: 'Isable', type: 'bool' },
                    { name: 'Bank', type: 'string' },
                    { name: 'BankAccount', type: 'string' },
                    { name: 'BankNo', type: 'string' },
                    { name: 'VGUID', type: 'string' }
                ],
                datatype: "json",
                id: "Vguid",
                data: { "CollectionCompany": label, "OrderVGUID": $("#VGUID").val(), "PurchaseID": $('#CollectionCompany').val() },
                url: "/CapitalCenterManagement/OrderListDetail/GetCollectionBankChange"   //获取数据源的路径
            };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    var typeAdapter = new $.jqx.dataAdapter(source);
    $("#jqxCustomerBankInfo").jqxGrid({
        pageable: false,
        width: "100%",
        autoheight: false,
        height: 200,
        pageSize: 15,
        //serverProcessing: true,
        //pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        //pagermode: 'checkbox',
        columnsHeight: 30,
        editable: true,
        columns: [
            { text: '启用', datafield: 'Isable', width: 60, align: 'center', cellsAlign: 'center', columntype: 'checkbox' },
            { text: '供应商类别', datafield: 'CompanyOrPerson', width: '250px', align: 'center', cellsAlign: 'center', hidden: true },
            { text: '账号', datafield: 'BankAccount', align: 'center', width: '350px', cellsAlign: 'center', editable: false },
            { text: '户名', datafield: 'BankAccountName', align: 'center', width: '350px', cellsAlign: 'center', editable: false },
            { text: '开户行', datafield: 'Bank', align: 'center', width: '350px', cellsAlign: 'center', editable: false },
            { text: '行号', datafield: 'BankNo', align: 'center', cellsAlign: 'center', editable: false },
            { text: 'VGUID', datafield: 'VGUID', hidden: true }
        ]
    });
    if ($("#valEnable").val() != "true") {
        $("#jqxCustomerBankInfo").jqxGrid({ editable: false });
        //$('#jqxSettingTable').jqxGrid('setcolumnproperty', 'Isable', 'editable', false);
    }
}