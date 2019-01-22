//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $pushPeopleDropDownButton: function () { return $("#pushPeopleDropDownButton") },
    $pushTree: function () { return $("#pushTree") },
}; //selector end
var isEdit = false;
var vguid = "";
var index = 0;//切换借贷
//var CompanyCode = loadCompanyCode("A");
var collectionCompany = loadCollectionCompany();
var businessType = loadBusinessType();
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

        initOrganization();

        //var id0 = "#CompanyCode";
        //uiEngineHelper.bindSelect(id0, CompanyCode, "Code", "Descrption");

        //uiEngineHelper.bindSelect('#CollectionCompany', collectionCompany, "VGUID", "CompanyOrPerson");
        //$("#CollectionCompany").prepend("<option value=\"\" selected='true'>请选择</>");
        var guid = $.request.queryString().VGUID;
        $("#VGUID").val(guid);
        var myDate = new Date();
        var date = myDate.toLocaleDateString();     //获取当前日期
        $("#SubmitDate").val($.action.replaceAll(date, '/', '-'));
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
            $.ajax({
                url: "/CapitalCenterManagement/OrderListDetail/SaveOrderListDetail",
                //data: { vguids: selection },
                data: {
                    "VGUID": $("#VGUID").val(),
                    "BusinessType": $("#pushPeopleDropDownButton").val(),
                    "BusinessProject": $("#BusinessProject").text(),//拼接类型
                    "BusinessSubItem1": $("#BusinessSubItem1").val(),//拼接编号

                    //"BusinessSubItem2": $("#BusinessSubItem2").val(),
                    //"BusinessSubItem3": $("#BusinessSubItem3").val(),
                    //"CompanySection": $("#CompanyCode").val(),
                    //"SubjectName": $("#SubjectName").val(),
                    //"SubjectSection": $("#SubjectSection").val(),
                    //"AccountSection": $("#AccountSection").val(),
                    //"CostCenterSection": $("#CostCenterSection").val(),
                    //"SpareOneSection": $("#SpareOneSection").val(),
                    //"SpareTwoSection": $("#SpareTwoSection").val(),
                    //"IntercourseSection": $("#IntercourseSection").val(),

                    "Status": $("#Status").val(),
                    "Founder": $("#LoginName").val(),

                    "CollectionCompany": $("#CollectionCompany").val(),
                    "CollectionAccount": $("#CollectionAccount").val(),
                    "CollectionBankAccount": $("#CollectionBankAccount").val(),
                    "CollectionBank": $("#CollectionBank").val(),
                    "CollectionBankAccountName": $("#CollectionBankAccountName").val(),
                    "PaymentMethod": $("#PaymentMethod").val(),
                    "PayAccount": $("#PayAccount").val(),
                    "PayBankAccountName": $("#PayBankAccountName").val(),
                    "PayBank": $("#PayBank").val(),
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
        //弹出框中的取消按钮
        $("#AddBankChannel_CancelBtn").on("click", function () {
            $("#AddBusinessType").modal("hide");
        });
        //弹出框中的保存按钮
        $("#AddBankChannel_OKButton").on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate($("#BusinessTypeName"))) {
                validateError++;
            }
            if (validateError <= 0) {
                $.ajax({
                    url: "/CapitalCenterManagement/OrderListDetail/SaveBusinessTypeName",
                    data: {
                        BusinessTypeName: $("#BusinessTypeName").val(),
                        BusinessVGUID: $("#BusinessVGUID").val()
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
                                $("#AddBusinessType").modal("hide");
                                var busin = $("#BusinessTypeName").val();
                                var mySelect = document.getElementById("BusinessType");
                                loadBusinessType();
                                $("#BusinessType").val(busin);
                                //addOption1(mySelect, busin, busin);
                                break;
                        }
                    }
                });
            }
        });
        //新增按钮
        $("#btnAdd").on("click", function () {
            $("#AddBusinessType").modal("show");
            $("#BusinessTypeName").val("");
            $("#BusinessVGUID").val("");
            initBusinessTypeName();
        })
        //
        $('#CollectionCompany').on('select', function (event) {
            var args = event.args;
            var item = args.item;
            if (args) {
                var value = item.value;
                companyChange(value);
            }
        });
    }; //addEvent end

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
                if ($("#Status").val() == "1") {
                    $("#hideButton").show();
                }
                //AccountSection = loadCompanyCode("C", msg.CompanySection, msg.SubjectSection);
                //CostCenterSection = loadCompanyCode("D", msg.CompanySection, msg.SubjectSection);
                //SpareOneSection = loadCompanyCode("E", msg.CompanySection, msg.SubjectSection);
                //SpareTwoSection = loadCompanyCode("F", msg.CompanySection, msg.SubjectSection);
                //IntercourseSection = loadCompanyCode("G", msg.CompanySection, msg.SubjectSection);
                //loadSelectFun();
                $("#pushPeopleDropDownButton").val(msg.BusinessType);
                $("#BusinessProject").text(msg.BusinessProject);
                $("#BusinessSubItem1").val(msg.BusinessSubItem1);

                //$("#BusinessSubItem2").val(msg.BusinessSubItem2);
                //$("#BusinessSubItem3").val(msg.BusinessSubItem3);
                //$("#CompanyCode").val(msg.CompanySection);
                //$("#SubjectName").val(msg.SubjectName);
                //$("#SubjectSection").val(msg.SubjectSection);
                //$("#AccountSection").val(msg.AccountSection);
                //$("#CostCenterSection").val(msg.CostCenterSection);
                //$("#SpareOneSection").val(msg.SpareOneSection);
                //$("#SpareTwoSection").val(msg.SpareTwoSection);
                //$("#IntercourseSection").val(msg.IntercourseSection);

                $("#CollectionCompany").val(msg.CollectionCompany);
                $("#CollectionAccount").val(msg.CollectionAccount);
                $("#CollectionBankAccount").val(msg.CollectionBankAccount);
                $("#CollectionBankAccountName").val(msg.CollectionBankAccountName);
                $("#CollectionBank").val(msg.CollectionBank);
                $("#PaymentMethod").val(msg.PaymentMethod);
                $("#PayAccount").val(msg.PayAccount);
                $("#PayBankAccountName").val(msg.PayBankAccountName);
                $("#PayBank").val(msg.PayBank);
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

function companyChange(value) {
    if (value == "") {
        $("#CollectionAccount").val("");
        $("#CollectionBankAccountName").val("");
        $("#CollectionBank").val("");
        $("#CollectionBankAccount").val("");
        return;
    }
    $.ajax({
        url: "/CapitalCenterManagement/OrderListDetail/GetCompanyChange",
        //async: false,
        data: { CollectionCompany: value },
        type: "post",
        success: function (result) {
            var values = result[0];
            $("#CollectionAccount").val(values.BankAccount);
            $("#CollectionBankAccountName").val(values.BankAccountName);
            $("#CollectionBank").val(values.Bank);
            $("#CollectionBankAccount").val(values.BankNo);
        }
    });
}

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
        }
    });
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

function loadCollectionCompany() {
    var url = "/CapitalCenterManagement/OrderListDetail/GetCollectionCompany";
    var source =
                {
                    datatype: "json",
                    datafields: [
                        { name: 'VGUID' },
                        { name: 'CompanyOrPerson' }
                    ],
                    url: url,
                    async: true
                };
    var dataAdapter = new $.jqx.dataAdapter(source);
    $('#CollectionCompany').jqxDropDownList({
        filterable: true, selectedIndex: 0, source: dataAdapter, displayMember: "CompanyOrPerson", valueMember: "VGUID",
        itemHeight: '30px', height: '20px', width: '176px', placeHolder: "请选择"
    });
}

function loadBusinessType() {
    var url = "/CapitalCenterManagement/OrderListDetail/GetBusinessType";
    $.ajax({
        url: url,
        async: false,
        data: {},
        type: "post",
        success: function (result) {
            uiEngineHelper.bindSelect('#BusinessType', result, "ListKey", "BusinessTypeName");
            $("#BusinessType").prepend("<option value=\"\" selected='true'>请选择</>");
        }
    });
}

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

//function getBusinessCodeText(parentItem) {
//    if (parentItem.level != 0) {
//        results = parentItem.value + "|" + results;
//        getBusinessCodeText(parentItem.prevItem);
//    } else {
//        results = parentItem.value + "|" + results;
//    }
//    return results;
//}