﻿//所有元素选择器
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
        
        uiEngineHelper.bindSelect('#CompanyCode', CompanyCode, "Code", "Descrption");
        var guid = $.request.queryString().VGUID;
        $("#VGUID").val(guid)
        if (guid != "" && guid != null) {
            getVoucherDetail();
            $("#VoucherType").attr("disabled", "disableds");
        } else {
            addSectionDiv();
            $("#hideButton").show();
        }
        $("#DocumentMaker").val($("#LoginName").val());
        //控件ID后缀
        var str = "";
        //新增
        $("#btnAddDetail").on("click", function () {
            addSectionDiv();
        });
        //取消
        $("#btnCancel").on("click", function () {
            window.close();
        })
        //切换标签页
        $('#jqxTabs').on('tabclick', function (event) {
            index = event.args.item;
            console.log(index);
            var length2 = $(".nav-i2").length;
            if (length2 == 0) {
                addSectionDiv();
            } else {
                autoHeight(index);
            }
        });
        var initWidgets = function (tab) {
            switch (tab) {
                case 0:
                    //initTable0();
                    break;
                case 1:
                    //initTable1();
                    break;
            }
        }
        $('#jqxTabs').jqxTabs({ width: '1560px', height: 300, initTabContent: initWidgets });
        //双击选择科目
        $("#jqxSubjectSection").on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row key.
            var key = args.key;
            // data field
            $("#SubjectSection_" + z1 + "_" + z2).val(row.Descrption);
            $("#hidSubjectSection_" + z1 + "_" + z2).val(row.Code);
            $("#AddCompanyDialog").modal("hide");
            var code = $("#CompanySection_" + z1 + "_" + z2).val();
            AccountSection = loadCompanyCode("C", code, row.Code);
            CostCenterSection = loadCompanyCode("D", code, row.Code);
            SpareOneSection = loadCompanyCode("E", code, row.Code);
            SpareTwoSection = loadCompanyCode("F", code, row.Code);
            IntercourseSection = loadCompanyCode("G", code, row.Code);
            var str = "_" + z1 + "_" + z2;
            loadSelectFun(str);
        });
        //保存
        $("#btnSave").on("click", function () {
            var validateError = 0;//未通过验证的数量
            //if (!Validate($("#")) {
            //    validateError++;
            //}
            
            if (validateError <= 0) {
                var detail = [];
                var length = $(".nav-i").length;
                for (var i = 0; i < length; i++) {
                    var ii = $(".nav-i")[i].id.split("_")[1];//获取DIV的ID下标
                    var remark = $("#Remark_A_" + ii).val();
                    var CompanySection = $("#CompanySection_A_" + ii).val();
                    var SubjectSection = $("#hidSubjectSection_A_" + ii).val();
                    var SubjectSectionName = $("#SubjectSection_A_" + ii).val();
                    var AccountSection = $("#AccountSection_A_" + ii).val();
                    var CostCenterSection = $("#CostCenterSection_A_" + ii).val();
                    var SpareOneSection = $("#SpareOneSection_A_" + ii).val();
                    var SpareTwoSection = $("#SpareTwoSection_A_" + ii).val();
                    var IntercourseSection = $("#IntercourseSection_A_" + ii).val();
                    var BorrowMoney = $("#BorrowMoney_A_" + ii).val();
                    var borrowDetail = {
                        "VGUID": "",
                        "Abstract": remark,
                        "CompanySection": CompanySection,
                        "SubjectSection": SubjectSection,
                        "SubjectSectionName": SubjectSectionName,
                        "AccountSection": AccountSection,
                        "CostCenterSection": CostCenterSection,
                        "SpareOneSection": SpareOneSection,
                        "SpareTwoSection": SpareTwoSection,
                        "IntercourseSection": IntercourseSection,
                        "BorrowMoney": parseInt(BorrowMoney),
                        "LoanMoney": -1
                    }
                    detail.push(borrowDetail);
                }
                var lengths = $(".nav-i2").length;
                for (var j = 0; j < lengths; j++) {
                    var jj = $(".nav-i2")[j].id.split("_")[1];//获取DIV的ID下标
                    var remark = $("#Remark_B_" + jj).val();
                    var CompanySection = $("#CompanySection_B_" + jj).val();
                    var SubjectSection = $("#hidSubjectSection_B_" + jj).val();
                    var SubjectSectionName = $("#SubjectSection_B_" + jj).val();
                    var AccountSection = $("#AccountSection_B_" + jj).val();
                    var CostCenterSection = $("#CostCenterSection_B_" + jj).val();
                    var SpareOneSection = $("#SpareOneSection_B_" + jj).val();
                    var SpareTwoSection = $("#SpareTwoSection_B_" + jj).val();
                    var IntercourseSection = $("#IntercourseSection_B_" + jj).val();
                    var LoanMoney = $("#LoanMoney_B_" + jj).val();
                    var loanDetail = {
                        "VGUID": "",
                        "Abstract": remark,
                        "CompanySection": CompanySection,
                        "SubjectSection": SubjectSection,
                        "SubjectSectionName": SubjectSectionName,
                        "AccountSection": AccountSection,
                        "CostCenterSection": CostCenterSection,
                        "SpareOneSection": SpareOneSection,
                        "SpareTwoSection": SpareTwoSection,
                        "IntercourseSection": IntercourseSection,
                        "BorrowMoney": -1,
                        "LoanMoney": parseInt(LoanMoney)
                    }
                    detail.push(loanDetail);
                }
                var y = JSON.stringify(detail);
                console.log(detail);
                //console.log(y);
                $("#VoucherType").removeAttr("disabled");
                $.ajax({
                    url: "/VoucherManageManagement/VoucherListDetail/SaveVoucherListDetail",
                    //data: { vguids: selection },
                    data: {
                        "VGUID": $("#VGUID").val(),
                        "CompanyCode": "",
                        "CompanyName": "营运公司",
                        "VoucherType": $("#VoucherType").val(),
                        "AccountingPeriod": $("#AccountingPeriod").val(),
                        "BatchName": $("#BatchName").val(),
                        "Currency": $("#Currency").val(),
                        "VoucherNo": $("#VoucherNo").val(),
                        "VoucherDate": $("#VoucherDate").val(),
                        "FinanceDirector": $("#FinanceDirector").val(),
                        "Bookkeeping": $("#Bookkeeping").val(),
                        "Auditor": $("#Auditor").val(),
                        "DocumentMaker": $("#DocumentMaker").val(),
                        "Cashier": $("#Cashier").val(),
                        "Attachment": $("#Attachment").val(),
                        "Detail": detail
                    },
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败！", null, "error");
                                $("#VoucherType").attr("disabled", "disableds");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                window.close();
                                window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                                break;
                        }
                    }
                });
            }
        })
    }; //addEvent end

    function addSectionDiv() {
        var id = "";//生成div的ID
        var className = "";
        var removeId = "";
        var money = "借方金额";
        var moneyId = "BorrowMoney_A_" + selectIndex;
        if (index == 0) {
            className = "nav-i";
            id = "Borrow_" + selectIndex;
            str = "_A_" + selectIndex;
            removeId = "removeBorrow_" + selectIndex;
        } else {
            className = "nav-i2";
            id = "Loan_" + selectIndex;
            str = "_B_" + selectIndex;
            removeId = "removeLoan_" + selectIndex;
            money = "贷方金额";
            moneyId = "LoanMoney_B_" + selectIndex;
        }
        var html = '<div id="' + id + '" class="' + className + '" style="border:2px solid #999;position:relative;border-radius:5px;margin: 15px 0;">' +
           '<table id="" style="width:100%;">' +
               '<tr style="height:30px;">' +
                   '<td colspan="8" style="text-align: right;">' +
                       '<div id="' + removeId + '" class="iconfont btn_icon remove" onclick="removes(' + removeId + ')" style="color: red !important;font-size: 20px !important;cursor:pointer;margin-left: 1480px;">&#xe6f2;</div>' +
                   '</td>' +
               '</tr>' +
               '<tr style="height:55px">' +
                   '<td style="text-align: right;">摘要</td>' +
                   '<td colspan="7" style="vertical-align: middle;padding-left: 0.8rem"><input id="Remark' + str + '" type="text" style="width: 1370px;" class="input_text form-control" validatetype="required" /></td>' +
               '</tr>' +
               ' <tr style="height:55px">' +
                   '<td style="text-align: right;">公司段</td>' +
                   '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                        ' <select id="CompanySection' + str + '" class="input_text form-control" disabled="disabled" onchange="gradeChange(CompanySection' + str + ')">' +
                        ' </select>' +
                   '</td>' +
                   '<td style="text-align: right;">科目段</td>' +
                   '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                        '<input id="SubjectSection' + str + '" type="text" style="width: 200px;" class="input_text form-control" validatetype="required" readonly="readonly" />&nbsp;&nbsp;&nbsp;&nbsp;' +
                        '<input id="hidSubjectSection' + str + '" name="hidSubjectSection' + str + '" class="hide" />' +
                        '<button id="btnSearch' + str + '" type="button" class="buttons" onclick="searchSubject(btnSearch' + str + ')"><i class="iconfont btn_icon">&#xe679;</i><span style="margin-left: 7px; float: left;">选择</span></button>' +
                   '</td>' +
                   '<td style="text-align: right;">核算段</td>' +
                   '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                        '<select id="AccountSection' + str + '" class="input_text form-control">' +
                        '</select>' +
                   ' </td>' +
                   ' <td style="text-align: right;">成本中心</td>' +
                   '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                        '<select id="CostCenterSection' + str + '" class="input_text form-control">' +
                        '</select>' +
                   ' </td>' +
                '</tr>' +
                '<tr style="height:55px">' +
                   '<td style="text-align: right;">备用1</td>' +
                   '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                        '<select id="SpareOneSection' + str + '" class="input_text form-control">' +
                        '</select>' +
                   '</td>' +
                   '<td style="text-align: right;">备用2</td>' +
                   '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                        '<select id="SpareTwoSection' + str + '" class="input_text form-control">' +
                        '</select>' +
                   '</td>' +
                   '<td style="text-align: right;">往来段</td>' +
                   '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                        '<select id="IntercourseSection' + str + '" class="input_text form-control">' +
                        '</select>' +
                   '</td>' +
                   '<td style="text-align: right;">' + money + '</td>' +
                   '<td style="vertical-align: middle;padding-left: 0.8rem"><input id="' + moneyId + '" style="width: 200px;" type="text" class="input_text form-control" validatetype="required,decimalNumber" /></td>' +
               '</tr>' +
        '</table>' +
      '</div>';
        if (index == 0) {
            $("#BorrowTable").append(html);
        } else {
            $("#LoanTable").append(html);
        }
        autoHeight(index);
        selectIndex++;
        var id0 = "#CompanySection" + str;
        uiEngineHelper.bindSelect(id0, CompanyCode, "Code", "Descrption");
        $("[validatetype]").on('keyup', function () {
            if (this.id != undefined) {
                Validate("#" + this.id);
            }
        }).on('blur', function () {
            if (this.id != undefined) {
                Validate("#" + this.id);
            }
        }).on('change', function () {
            if (this.id != undefined) {
                Validate("#" + this.id);
            }
        });
    }

    function loadSelectFun(str) {

        var id1 = "#AccountSection" + str;
        var id2 = "#CostCenterSection" + str;
        var id3 = "#SpareOneSection" + str;
        var id4 = "#SpareTwoSection" + str;
        var id5 = "#IntercourseSection" + str;

        uiEngineHelper.bindSelect(id1, AccountSection, "Code", "Descrption");
        uiEngineHelper.bindSelect(id2, CostCenterSection, "Code", "Descrption");
        uiEngineHelper.bindSelect(id3, SpareOneSection, "Code", "Descrption");
        uiEngineHelper.bindSelect(id4, SpareTwoSection, "Code", "Descrption");
        uiEngineHelper.bindSelect(id5, IntercourseSection, "Code", "Descrption");
    }

    function getVoucherDetail() {
        $.ajax({
            url: "/VoucherManageManagement/VoucherListDetail/GetVoucherDetail",
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
                var voucherDate = parseInt(msg.VoucherDate.replace(/[^0-9]/ig,""));//转时间戳
                var accountingPeriod = parseInt(msg.AccountingPeriod.replace(/[^0-9]/ig, ""));//转时间戳
                $("#VoucherDate").val($.convert.toDate(new Date(voucherDate), "yyyy-MM-dd"));
                $("#AccountingPeriod").val($.convert.toDate(new Date(accountingPeriod), "yyyy-MM"));
                $("#BatchName").val(msg.BatchName);
                $("#Currency").val(msg.Currency);
                $("#VoucherNo").val(msg.VoucherNo);
                $("#FinanceDirector").val(msg.FinanceDirector);
                $("#Bookkeeping").val(msg.Bookkeeping);
                $("#Auditor").val(msg.Auditor);
                $("#DocumentMaker").val(msg.DocumentMaker);
                $("#Cashier").val(msg.Cashier); 
                var datas = msg.Detail;
                setVoucherDetail(datas);
                loadAttachments(msg.Attachment);
            }
        });
    }

    function setVoucherDetail(datas) {
        var id = "";//生成div的ID
        var className = "";
        var removeId = "";
        var money = "借方金额";
        for (var i = 0; i < datas.length; i++) {
            var moneyId = "BorrowMoney_A_" + selectIndex;
            var data = datas[i];
            var moneyValue = 0;
            if (data.LoanMoney == -1) {
                //绑定借
                className = "nav-i";
                id = "Borrow_" + selectIndex;
                str = "_A_" + selectIndex;
                removeId = "removeBorrow_" + selectIndex;
                moneyValue = data.BorrowMoney;
            } else {
                //绑定贷
                className = "nav-i2";
                id = "Loan_" + selectIndex;
                str = "_B_" + selectIndex;
                removeId = "removeLoan_" + selectIndex;
                money = "贷方金额";
                moneyId = "LoanMoney_B_" + selectIndex;
                moneyValue = data.LoanMoney;
            }
            var html = '<div id="' + id + '" class="' + className + '" style="border:2px solid #999;position:relative;border-radius:5px;margin: 15px 0; width: 1510px;margin-left: 12px !important;">' +
               '<table id="" style="width:100%;">' +
                   '<tr style="height:30px;">' +
                       '<td colspan="8" style="text-align: right;">' +
                           '<div id="' + removeId + '" class="iconfont btn_icon remove" onclick="removes(' + removeId + ')" style="color: red !important;font-size: 20px !important;cursor:pointer;margin-left: 1480px;">&#xe6f2;</div>' +
                       '</td>' +
                   '</tr>' +
                   '<tr style="height:55px">' +
                       '<td style="text-align: right;">摘要</td>' +
                       '<td colspan="7" style="vertical-align: middle;padding-left: 0.8rem"><input id="Remark' + str + '" type="text" style="width: 1370px;" class="input_text form-control" validatetype="required" value="' + data.Abstract + '" /></td>' +
                   '</tr>' +
                   ' <tr style="height:55px">' +
                       '<td style="text-align: right;">公司段</td>' +
                       '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                            ' <select id="CompanySection' + str + '" class="input_text form-control" disabled="disabled"  onchange="gradeChange(CompanySection' + str + ')" >' +
                            ' </select>' +
                       '</td>' +
                       '<td style="text-align: right;">科目段</td>' +
                       '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                            '<input id="SubjectSection' + str + '" type="text" style="width: 200px;" class="input_text form-control" validatetype="required" readonly="readonly" value="' + data.SubjectSectionName + '"/>&nbsp;&nbsp;&nbsp;&nbsp;' +
                            '<input id="hidSubjectSection' + str + '" name="hidSubjectSection' + str + '" class="hide" value="' + data.SubjectSection + '"/>' +
                            '<button id="btnSearch' + str + '" type="button" class="buttons" onclick="searchSubject(btnSearch' + str + ')"><i class="iconfont btn_icon">&#xe679;</i><span style="margin-left: 7px; float: left;">选择</span></button>' +
                       '</td>' +
                       '<td style="text-align: right;">核算段</td>' +
                       '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                            '<select id="AccountSection' + str + '" class="input_text form-control" > ' +
                            '</select>' +
                       ' </td>' +
                       ' <td style="text-align: right;">成本中心</td>' +
                       '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                            '<select id="CostCenterSection' + str + '" class="input_text form-control" >' +
                            '</select>' +
                       ' </td>' +
                    '</tr>' +
                    '<tr style="height:55px">' +
                       '<td style="text-align: right;">备用1</td>' +
                       '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                            '<select id="SpareOneSection' + str + '" class="input_text form-control" >' +
                            '</select>' +
                       '</td>' +
                       '<td style="text-align: right;">备用2</td>' +
                       '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                            '<select id="SpareTwoSection' + str + '" class="input_text form-control" >' +
                            '</select>' +
                       '</td>' +
                       '<td style="text-align: right;">往来段</td>' +
                       '<td style="vertical-align: middle;padding-left: 0.8rem">' +
                            '<select id="IntercourseSection' + str + '" class="input_text form-control" >' +
                            '</select>' +
                       '</td>' +
                       '<td style="text-align: right;">' + money + '</td>' +
                       '<td style="vertical-align: middle;padding-left: 0.8rem"><input id="' + moneyId + '" style="width: 200px;" type="text" class="input_text form-control" validatetype="required,decimalNumber" /></td>' +
                   '</tr>' +
            '</table>' +
          '</div>';
            selectIndex++;
            if (data.LoanMoney == -1) {
                $("#BorrowTable").append(html);
            } else {
                $("#LoanTable").append(html);
            }
            autoHeight(index);
            var id0 = "#CompanySection" + str;
            uiEngineHelper.bindSelect(id0, CompanyCode, "Code", "Descrption");
            AccountSection = loadCompanyCode("C", data.CompanySection, data.SubjectSection);
            CostCenterSection = loadCompanyCode("D", data.CompanySection, data.SubjectSection);
            SpareOneSection = loadCompanyCode("E", data.CompanySection, data.SubjectSection);
            SpareTwoSection = loadCompanyCode("F", data.CompanySection, data.SubjectSection);
            IntercourseSection = loadCompanyCode("G", data.CompanySection, data.SubjectSection);
            loadSelectFun(str);
            $("#CompanySection" + str).val(data.CompanySection);
            $("#AccountSection" + str).val(data.AccountSection);
            $("#CostCenterSection" + str).val(data.CostCenterSection);
            $("#SpareOneSection" + str).val(data.SpareOneSection);
            $("#SpareTwoSection" + str).val(data.SpareTwoSection);
            $("#IntercourseSection" + str).val(data.IntercourseSection);
        }
        console.log(selectIndex);
    }
};
//移除明细内容
function removes(event) {
    WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消", event);
}
function dele(event) {
    var str = event.id;
    var x = str.split("_")[1];
    if (index == 0) {
        $("#Borrow_" + x).remove();
    } else {
        $("#Loan_" + x).remove();
    }
    autoHeight(index)
}
//选择科目段
function searchSubject(event) {
    var str = event.id;
    var x = str.split("_")[1];
    var y = str.split("_")[2];
    var companyCode = $("#CompanySection_" + x + "_" + y).val();
    if (companyCode == "") {
        jqxNotification("请选择公司段", null, "error");
        return;
    }
    z1 = "";
    z2 = "";
    initSubjectTable(companyCode, x, y);
    $("#AddCompanyDialog").modal({ backdrop: "static", keyboard: false });
    $("#AddCompanyDialog").modal("show");
}
let z1 = "";
let z2 = "";
function initSubjectTable(companyCode, x, y) {
    z1 = x;
    z2 = y;
    var source ={
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
    var str = event.id;
    var x = str.split("_")[1];
    var y = str.split("_")[2];
    $("#SubjectSection_" + x + "_" + y).val("");
    $("#hidSubjectSection_" + x + "_" + y).val("");
    $("#AccountSection_" + x + "_" + y).val("");
    $("#CostCenterSection_" + x + "_" + y).val("");
    $("#SpareOneSection_" + x + "_" + y).val("");
    $("#SpareTwoSection_" + x + "_" + y).val("");
    $("#IntercourseSection_" + x + "_" + y).val("");
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

function autoHeight(index) {
    if (index == 0) {
        var length = $(".nav-i").length;
        if (length == 1) {
            $('#jqxTabs').jqxTabs({ height: 300 });//1个
        }
        if (length >= 2) {
            $('#jqxTabs').jqxTabs({ height: 500 });//2个
        }
    }
    else {
        var length2 = $(".nav-i2").length;  
        if (length2 == 1) {
            $('#jqxTabs').jqxTabs({ height: 300 });//1个
        }
        if (length2 >= 2) {
            $('#jqxTabs').jqxTabs({ height: 500 });//2个
        }
    }
}

$(function () {
    var buttonText = {
        browseButton: '上传',
        uploadButton: '提交',
        cancelButton: '清空',
        uploadFileTooltip: '上传',
        cancelFileTooltip: '删除'
    };
    $('#btn_Attachment').jqxFileUpload({ width: '600px', height: '', fileInputName: 'AttachmentFile', browseTemplate: 'success', uploadTemplate: 'primary', cancelTemplate: 'danger', localization: buttonText, multipleFilesUpload: true });
    $("#btn_Attachment").on("select", function (event) {
        if (event.args.size > (1024 * 1024 * 10)) {
            jqxAlert("单文件大小不能超过10M");
            $("#btn_AttachmentCancelButton").trigger('click');
        }
    });
    $("#btn_Attachment").on("uploadStart", function (event) {
        //获取文件名
        fileName = event.args.file;
        var extStart = fileName.lastIndexOf(".");
        //判断是文件还是图片
        var ext = fileName.substring(extStart, fileName.length).toUpperCase();
        if (ext != ".BMP" && ext != ".PNG" && ext != ".GIF" && ext != ".JPG" && ext != ".JPEG") {//上传文件
            $('#btn_Attachment').jqxFileUpload({ uploadUrl: '/File/UploadFile?allowSize=' + 20 });
        }
        else {//上传图片
            $('#btn_Attachment').jqxFileUpload({ uploadUrl: '/File/UploadImage?allowSize=' + 20 });
        }
    })
    $("#btn_Attachment").on("uploadEnd", function (event) {
        var args = event.args;
        //var msg = $.convert.strToJson($(args.response).html());
        uploadFiles(event)
    })
})
var fileName = "";
function uploadFiles(event) {
    var args = event.args;
    var msg = $.convert.strToJson($(args.response).html());
    fileName = event.args.file;
    var attachments = $("#Attachment").val();
    var type = $("#AttachmentType").val();
    if (attachments == "") {
        attachments = type + "&" + msg.WebPath + "&" + fileName;
    }
    else {
        attachments = attachments + "," + type + "&" + msg.WebPath + "&" + fileName;
    }

    $("#attachments")[0].innerHTML += "<span>" + type + "&nbsp;&nbsp;<a href='" + msg.WebPath + "' target='_blank'>" + fileName + "</a><button class='closes' type='button' onclick='removeAttachment(this)'>×</button></br></span>"
    $("#Attachment").val(attachments);


}
function loadAttachments(attachments) {
    $("#Attachment").val(attachments);
    if (attachments != "") {
        var attachValues = attachments.split(",");
        for (var i = 0; i < attachValues.length; i++) {
            var attach = attachValues[i].split("&");
            $("#attachments")[0].innerHTML += "<span>" + attach[0] + "&nbsp;&nbsp;<a href='" + attach[1] + "' target='_blank'>" + attach[2] + "</a><button class='closes' type='button' onclick='removeAttachment(this)'>×</button></br></span>"
        }
    }
}
function removeAttachment(obj) {

    var id = obj.previousSibling.attributes["href"].value;
    var type = obj.parentElement.textContent.trim().split(/\s+/)[0];//按空格拆分字符串
    var name = obj.parentElement.textContent.trim().split(/\s+/)[1].substring(0, obj.parentElement.textContent.trim().split(/\s+/)[1].length - 1);
    var replaceStr = type + "&" + id + "&" + name;
    var attachmentValues = $("#Attachment").val();
    attachmentValues = $.action.replaceAll(attachmentValues, replaceStr, '');

    $("#Attachment").val(attachmentValues);
    $(obj).parent().remove();
    return false;
}

$(function () {
    var page = new $page();
    page.init();
});























function initTable() {
    //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
    var status = $.request.queryString().Status;
    var source =
        {
            datafields:
            [
                { name: "checkbox", type: null },
                { name: 'CompanyCode', type: 'string' },
                { name: 'Remark', type: 'string' },
                { name: 'SubjectAndDescrption', type: 'string' },
                { name: 'DebitAmount', type: 'number' },
                { name: 'CreditAmount', type: 'number' },
                { name: 'VGUID', type: 'string' },
                { name: 'Status', type: 'string' },
            ],
            datatype: "json",
            id: "VGUID",
            data: { "Status": status },
            url: "/VoucherManageManagement/VoucherList/GetVoucherListDatas"   //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    //创建卡信息列表（主表）
    selector.$grid().jqxDataTable(
        {
            pageable: false,
            width: "100%",
            height: 400,
            pageSize: 10,
            serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 25,
            aggregatesHeight: 25,
            showAggregates: true,
            columns: [
                { text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                { text: '摘要', datafield: 'Remark', width: 350, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                { text: '科目及描述', datafield: 'SubjectAndDescrption', width: 350, align: 'center', cellsAlign: 'center' },
                {
                    text: '借方金额', datafield: 'DebitAmount', width: 200, cellsFormat: "d2", align: 'center', cellsAlign: 'center', aggregates: [{
                        'Total':
                          function (aggregatedValue, currentValue, column, record) {
                              var total = currentValue * parseInt(record['Currency']);
                              return aggregatedValue + total;
                          }
                    }],
                    aggregatesRenderer: function (aggregates, column, element) {
                        var total = aggregates.Total == null ? "0.00" : aggregates.Total;
                        var renderString = "<div style='margin: 4px; float: center;  height: 100%;'>";
                        renderString += "<strong>合 计: </strong>" + total + "</div>";
                        return renderString;
                    }
                },
                {
                    text: '贷方金额', datafield: 'CreditAmount', align: 'center', cellsFormat: "d2", cellsAlign: 'center', aggregates: [{
                        'Total':
                          function (aggregatedValue, currentValue, column, record) {
                              var total = currentValue * parseInt(record['BatchName']);
                              return aggregatedValue + total;
                          }
                    }],
                    aggregatesRenderer: function (aggregates, column, element) {
                        var total = aggregates.Total == null ? "0.00" : aggregates.Total;
                        var renderString = "<div style='margin: 4px; float: center;  height: 100%;'>";
                        renderString += "<strong>合 计: </strong>" + total + "</div>";
                        return renderString;
                    }
                },
                { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                { text: 'VGUID', datafield: 'VGUID', hidden: true },
            ]
        });

}

function detailFunc(row, column, value, rowData) {
    var container = "";
    if (selector.$EditPermission().val() == "1") {
        container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.VoucherSubject + "','" + rowData.VoucherSummary + "','" + rowData.VoucherSubjectName + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Batch + "</a>";
    } else {
        container = "<span>" + rowData.Batch + "</span>";
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



//var length = $(".nav-i").length;
//for (var i = 0; i < length; i++) {
//    var remark = $("#Remark_A_" + i).val();
//    var CompanySection = $("#CompanySection_A_" + i).val();
//    var SubjectSection = $("#SubjectSection_A_" + i).val();
//    var SubjectSectionName = $("#SubjectSectionName_A_" + i).val();
//    var AccountSection = $("#AccountSection_A_" + i).val();
//    var CostCenterSection = $("#CostCenterSection_A_" + i).val();
//    var SpareOneSection = $("#SpareOneSection_A_" + i).val();
//    var SpareTwoSection = $("#SpareTwoSection_A_" + i).val();
//    var IntercourseSection = $("#IntercourseSection_A_" + i).val();
//    var BorrowMoney = $("#BorrowMoney_A_" + i).val();
//    var x = {
//        "VGUID": "",
//        "Remark": remark,
//        "CompanySection": CompanySection,
//        "SubjectSection": SubjectSection,
//        "SubjectSectionName": SubjectSectionName,
//        "AccountSection": AccountSection,
//        "CostCenterSection": CostCenterSection,
//        "SpareOneSection": SpareOneSection,
//        "SpareTwoSection": SpareTwoSection,
//        "IntercourseSection": IntercourseSection,
//        "BorrowMoney": BorrowMoney,
//        "LoanMoney": 0
//    }
//    detail.push(x);
//}

//var data = {
//    "VGUID": "",
//    "CompanyCode": "",
//    "CompanyName": "",
//    "VoucherType": "现金类",
//    "AccountingPeriod": "2018-09",
//    "BatchName": "",
//    "Currency": "CNY",
//    "VoucherNo": "",
//    "VoucherDate": "2018-11-29",
//    "FinanceDirector": "",
//    "Bookkeeping": "",
//    "Auditor": "",
//    "DocumentMaker": "",
//    "Cashier": "",
//    "Attachment": "借款单&/_theme/temp/img/2018113011943974182.jpg&花,借款单&/_theme/temp/img/20181130119433326410.jpg&水母",文件流转存本地
//    "Detail": detail
//}
//var detail = [
//              {
//                  "Remark": 2,
//                  "CompanySection": "",
//                  "SubjectSection": "",
//                  "SubjectSectionName": "",
//                  "AccountSection": "",
//                  "CostCenterSection": "",
//                  "SpareOneSection": "",
//                  "SpareTwoSection": "",
//                  "IntercourseSection": "",
//                  "BorrowMoney": 17354318.68,
//                  "LoanMoney": 0
//              },
//             {
//                 "Remark": 2,
//                 "CompanySection": "",
//                 "SubjectSection": "",
//                 "SubjectSectionName": "",
//                 "AccountSection": "",
//                 "CostCenterSection": "",
//                 "SpareOneSection": "",
//                 "SpareTwoSection": "",
//                 "IntercourseSection": "",
//                 "BorrowMoney": 0,
//                 "LoanMoney": -17354318.68
//             }
//]


//var data = [
//              {
//                  "BusinessType": "",
//                  "BusinessProject": "",
//                  "BusinessSubItem": "",
//                  "BusinessUnit": "",
//                  "Number": "",
//                  "Money": "",
//              }
//]