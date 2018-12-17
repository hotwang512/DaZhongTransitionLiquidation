//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
}; //selector end
var isEdit = false;
var vguid = "";
var index = 0;//切换借贷
var CompanyCode = loadCompanyCode("A");
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
        var id0 = "#CompanyCode";
        uiEngineHelper.bindSelect(id0, CompanyCode, "Code", "Descrption");
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
            window.close();
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
                    "BusinessType": $("#BusinessType").val(),
                    "BusinessProject": $("#BusinessProject").val(),
                    "BusinessSubItem1": $("#BusinessSubItem1").val(),
                    "BusinessSubItem2": $("#BusinessSubItem2").val(),
                    "BusinessSubItem3": $("#BusinessSubItem3").val(),
                    "Abstract": $("#Abstract").val(),
                    "Money": $("#Money").val(),
                    "CompanySection": $("#CompanyCode").val(),
                    "SubjectName": $("#SubjectName").val(),
                    "SubjectSection": $("#SubjectSection").val(),
                    "AccountSection": $("#AccountSection").val(),
                    "CostCenterSection": $("#CostCenterSection").val(),
                    "SpareOneSection": $("#SpareOneSection").val(),
                    "SpareTwoSection": $("#SpareTwoSection").val(),
                    "IntercourseSection": $("#IntercourseSection").val(),
                    "Status": $("#Status").val(),
                    "Founder": $("#LoginName").val(),

                    "PaymentCompany": $("#PaymentCompany").val(),
                    "CollectionCompany": $("#CollectionCompany").val(),
                    "BusinessUnit": $("#BusinessUnit").val(),
                    "Mode": $("#Mode").val(),
                    "VehicleType": $("#VehicleType").val(),
                    "Number": $("#Number").val(),
                    "SubmitDate": $("#SubmitDate").val(),
                    "PaymentMethod": $("#PaymentMethod").val(),
                    "InvoiceNumber": $("#InvoiceNumber").val(),
                    "AttachmentNumber": $("#AttachmentNumber").val(),
                },
                type: "post",
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("保存失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("保存成功！", null, "success");
                            window.close();
                            window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                            break;
                    }
                }
            });
        })
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
                AccountSection = loadCompanyCode("C", msg.CompanySection, msg.SubjectSection);
                CostCenterSection = loadCompanyCode("D", msg.CompanySection, msg.SubjectSection);
                SpareOneSection = loadCompanyCode("E", msg.CompanySection, msg.SubjectSection);
                SpareTwoSection = loadCompanyCode("F", msg.CompanySection, msg.SubjectSection);
                IntercourseSection = loadCompanyCode("G", msg.CompanySection, msg.SubjectSection);
                loadSelectFun();
                $("#BusinessType").val(msg.BusinessType);
                $("#BusinessProject").val(msg.BusinessProject);
                $("#BusinessSubItem1").val(msg.BusinessSubItem1);
                $("#BusinessSubItem2").val(msg.BusinessSubItem2);
                $("#BusinessSubItem3").val(msg.BusinessSubItem3);
                $("#Abstract").val(msg.Abstract);
                $("#CompanyCode").val(msg.CompanySection);
                $("#SubjectName").val(msg.SubjectName);
                $("#SubjectSection").val(msg.SubjectSection);
                $("#AccountSection").val(msg.AccountSection);
                $("#CostCenterSection").val(msg.CostCenterSection);
                $("#SpareOneSection").val(msg.SpareOneSection);
                $("#SpareTwoSection").val(msg.SpareTwoSection);
                $("#IntercourseSection").val(msg.IntercourseSection);

                $("#PaymentCompany").val(msg.PaymentCompany);
                $("#CollectionCompany").val(msg.CollectionCompany);
                $("#BusinessUnit").val(msg.BusinessUnit);
                $("#Mode").val(msg.Mode);
                $("#VehicleType").val(msg.VehicleType);
                $("#Number").val(msg.Number);
                $("#Money").val(msg.Money);
                var submitDate = parseInt(msg.SubmitDate.replace(/[^0-9]/ig, ""));//转时间戳
                $("#SubmitDate").val($.convert.toDate(new Date(submitDate), "yyyy-MM-dd"));
                $("#PaymentMethod").val(msg.PaymentMethod);
                $("#InvoiceNumber").val(msg.InvoiceNumber);
                $("#AttachmentNumber").val(msg.AttachmentNumber);
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

function gradeChange(event) {
    $("#SubjectSection").val("");
    $("#SubjectName").val("");
    $("#AccountSection").val("");
    $("#CostCenterSection").val("");
    $("#SpareOneSection").val("");
    $("#SpareTwoSection").val("");
    $("#IntercourseSection").val("");
}

function loadCompanyCode(name, companyCode, subjectCode) {
    var url = "/VoucherManageManagement/VoucherListDetail/GetSelectSection";
    var value = null;
    $.ajax({
        url: url,
        async: false,
        data: { name: name, companyCode: companyCode, subjectCode: subjectCode },
        type: "post",
        success: function (result) {
            value = result;
        }
    });
    return value;
}

$(function () {
    var page = new $page();
    page.init();
});