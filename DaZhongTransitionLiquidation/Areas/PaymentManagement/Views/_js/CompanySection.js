//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $SubjectCode: function () { return $("#SubjectCode") },
    $txtDatedTime: function () { return $("#txtDatedTime") },
    $AddNewBankDataDialog: function () { return $("#AddNewBankDataDialog") },
    $AddNewBankData_OKButton: function () { return $("#AddNewBankData_OKButton") },
    $AddNewBankData_CancelBtn: function () { return $("#AddNewBankData_CancelBtn") },
    $txtCode: function () { return $("#txtCode") },
    $txtDescrption: function () { return $("#txtDescrption") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end

var isEdit = false;
var vguid = "";
var hidParentCode = "";
var index = 0;
var expandVGUID = [];
var companyCode = $("#CompanyCode").val();
var accountModeCode = $("#AccountModeCode").val();
var typeAdapterJqxTree = null;
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        getCompanyCode();
        //加载列表数据
        //initTable();
        initSubjectTable();

        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            selector.$txtDatedTime().val("");
        });
        //同步现有数据,隐藏
        $("#btnTongBu").on("click", function () {
            $('#jqxSubjectTable').jqxGrid('updatebounddata');
            $('#jqxSubjectTable').jqxGrid('addgroup', 'BOOK')
            $('#jqxSubjectTable').jqxGrid('addgroup', 'VALUE_SET')
            $("#AddSubjectTable").modal({ backdrop: "static", keyboard: false });
            $("#AddSubjectTable").modal("show");
            //$.ajax({
            //    url: "/PaymentManagement/CompanySection/SyncSubjectData",
            //    data: {
            //    },
            //    type: "post",
            //    success: function (msg) {
            //        var data = msg.ResultInfo;
            //    }
            //});
        });
        $("#Refresh").on("click", function () {
            initSubjectTable();
        });
        //新增
        selector.$btnAdd().on("click", function () {
            $(".SubjectTr").hide();
            $("#SubjectCode").hide();
            $("#OrgID").hide();
            $("#Abbreviation").hide();
            if (index == 2) {
                $(".SubjectTr").show();
                var checkrow = $("#jqxTable2").jqxTreeGrid('getCheckedRows');
                var data = $("#jqxTable2").jqxTreeGrid('getView');
                var parentCode = "";
                if (data.length != 0 && checkrow.length == 1) {
                    parentCode = checkrow[0].Code;
                    hidParentCode = parentCode;
                    $("#txtParentCode").val(parentCode);
                }
                if (data.length != 0 && checkrow.length != 1) {
                    jqxNotification("请选择一个节点！", null, "error");
                    return;
                } else {
                    if ($("#txtFirstSubjects").val() == "1") {
                        $("#SubjectCode").hide();
                        $("#txtParentCode").val("");
                    } else {
                        $("#SubjectCode").show();

                    }
                }
            }
            else if (index == 1) {
                $("#OrgID").show();
                $("#Abbreviation").show();
            }
            selector.$txtCode().val("");
            selector.$txtDescrption().val("");
            $("#txtOrgID").val("");
            $("#txtAbbreviation").val("");
            $("#txtRemark").val("");
            isEdit = false;
            vguid = "";
            $("#myModalLabel_title").text("新增数据");
            $("#AddNewBankDataDialog table tr").eq(1).show();
            $(".msg").remove();
            selector.$txtCode().removeClass("input_Validate");
            selector.$txtDescrption().removeClass("input_Validate");

            selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
            selector.$AddNewBankDataDialog().modal("show");
        });
        //弹出框中的取消按钮
        selector.$AddNewBankData_CancelBtn().on("click", function () {
            selector.$AddNewBankDataDialog().modal("hide");
        });
        //公司段设置弹出框中的取消按钮
        $("#AddSection_CancelBtn").on("click", function () {
            $("#AddSectionDialog").modal("hide");
        });
        //核算段设置弹出框中的取消按钮
        $("#AddAccount_CancelBtn").on("click", function () {
            $("#AddAccountSettingDialog").modal("hide");
        });
        //ORACLE同步弹出框中的取消按钮
        $("#AddSubject_CancelBtn").on("click", function () {
            $("#AddSubjectTable").modal("hide");
        });
        //ORACLE同步弹出框中的保存按钮
        $("#AddSubject_OKButton").on("click", function () {
            layer.load();
            var tableData = $('#jqxSubjectTable').jqxGrid('getboundrows');
            var array = $('#jqxSubjectTable').jqxGrid('getselectedrowindexes');
            var pars = [];
            $(array).each(function (i, v) {
                var value = $('#jqxSubjectTable').jqxGrid('getcell', v, "CODE");
                if (value != null) {
                    pars.push(value.value);
                }
            });
            if (tableData != null) {
                $.ajax({
                    url: "/PaymentManagement/CompanySection/SaveSubjectData",
                    data: { jsonData: JSON.stringify(tableData), code: pars },
                    type: "post",
                    success: function (msg) {
                        layer.closeAll('loading');
                        if (msg.IsSuccess == true) {
                            jqxNotification("保存成功！", null, "success");
                            $("#AddSubjectTable").modal("hide");
                            if (index == 2) {
                                $("#jqxTable2").jqxTreeGrid("updateBoundData");
                            }
                        } else {
                            jqxNotification("保存失败！", null, "error");
                        }
                    }
                });
            }
        });
        //公司段设置弹出框中的保存按钮
        $("#AddSection_OKButton").on("click", function () {
            var code = $("#hidSubjectCode").val();
            var type = $("#hidType").val();
            var otherCode = [];
            if (type == "0") {
                var data = $("#jqxSubjectSetting").jqxTreeGrid('getCheckedRows');
                for (var i = 0; i < data.length; i++) {
                    otherCode.push(data[i].Code);
                }
            } else {
                var data = $('#jqxTableSetting').jqxGrid('getdisplayrows')
                for (var i = 0; i < data.length; i++) {
                    if (data[i].Checked == "True" || data[i].Checked == true) {
                        otherCode.push(data[i].Code);
                    }
                }
            }
            layer.load();
            $.ajax({
                url: "/PaymentManagement/CompanySection/SaveSectionSetting",
                //data: { vguids: selection },
                data: { code: code, otherCode: otherCode, type: type, companyCode: companyCode, accountModeCode: accountModeCode },
                traditional: true,
                type: "post",
                success: function (msg) {
                    layer.closeAll('loading');
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("操作失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("操作成功！", null, "success");
                            $("#AddSectionDialog").modal("hide");
                            if (index == 2) {
                                $("#jqxTable2").jqxTreeGrid('updateBoundData');
                                //$("#jqxTable2").jqxTreeGrid('scrollOffset', top, left)
                                $('#jqxTable2').on('bindingComplete', function (event) {
                                    $("#jqxTable2").jqxTreeGrid('scrollOffset', topNum, leftNum)
                                });
                            } else {
                                $("#jqxTable" + index).jqxDataTable('updateBoundData');
                            }
                            break;
                    }
                }
            });
        });
        //核算段设置弹出框中的保存按钮
        $("#AddAccount_OKButton").on("click", function () {
            var code = $("#hidSubjectCode").val();
            var vguid = [];
            var data = $('#jqxAccountSetting').jqxGrid('getdisplayrows')
            for (var i = 0; i < data.length; i++) {
                if (data[i].IsChecked == "True" || data[i].IsChecked == true) {
                    vguid.push(data[i].VGUID);
                }
            }
            layer.load();
            $.ajax({
                url: "/PaymentManagement/CompanySection/SaveAccoutSetting",
                //data: { vguids: selection },
                data: { code: code, vguid: vguid, companyCode: companyCode, accountModeCode: accountModeCode },
                traditional: true,
                type: "post",
                success: function (msg) {
                    layer.closeAll('loading');
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("操作失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("操作成功！", null, "success");
                            $("#AddAccountSettingDialog").modal("hide");
                            $("#jqxTable" + index).jqxDataTable('updateBoundData');
                            break;
                    }
                }
            });
        });
        //保存8个段数据
        selector.$AddNewBankData_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate(selector.$txtCode())) {
                validateError++;
            }
            if (!Validate(selector.$txtDescrption())) {
                validateError++;
            }
            if (index == 2) {
                if (!Validate($("#txtParentCode")) && $("#txtFirstSubjects").val() == "0") {
                    validateError++;
                }
            }
            var url = "";
            switch (index) {
                case 0: url = "/PaymentManagement/AccountModeSection/SaveCompanySection?isEdit=";
                    break;
                case 1: url = "/PaymentManagement/CompanySection/SaveCompanySection?isEdit=";
                    break;
                case 2: url = "/PaymentManagement/SubjectSection/SaveCompanySection?isEdit=";
                    break;
                case 3: url = "/PaymentManagement/AccountingSection/SaveCompanySection?isEdit=";
                    break;
                case 4: url = "/PaymentManagement/CostCenterSection/SaveCompanySection?isEdit=";
                    break;
                case 5: url = "/PaymentManagement/SpareOne/SaveCompanySection?isEdit=";
                    break;
                case 6: url = "/PaymentManagement/SpareTwo/SaveCompanySection?isEdit=";
                    break;
                case 7: url = "/PaymentManagement/IntercourseSection/SaveCompanySection?isEdit=";
                    break;

                default:

            }
            
            if (validateError <= 0) {
                layer.load();
                $.ajax({
                    url: url + isEdit,
                    data: {
                        "Code": selector.$txtCode().val(),
                        "Descrption": selector.$txtDescrption().val(),
                        "Remark": $("#txtRemark").val(),
                        "VGUID": vguid,
                        "ParentCode": $("#txtParentCode").val(),
                        "AccountModeCode": $("#AccountModeCode").val(),
                        "CompanyCode": $("#CompanyCode").val(),
                        "OrgID": $("#txtOrgID").val(),
                        "Abbreviation": $("#txtAbbreviation").val(),
                    },
                    type: "post",
                    dataType: "json",
                    success: function (msg) {
                        layer.closeAll('loading');
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                if (index == 2) {
                                    $("#jqxTable2").jqxTreeGrid('updateBoundData');
                                } else {
                                    $("#jqxTable" + index).jqxDataTable('updateBoundData');
                                }
                                selector.$AddNewBankDataDialog().modal("hide");
                                break;
                            case "2":
                                jqxNotification("编码已存在", null, "error");
                                break;
                            case "3":
                                jqxNotification("父级编码不存在", null, "error");
                                break;
                        }

                    }
                });
            }
        });
        //弹出新增银行信息
        $("#AddCompany_OKButton").on("click", function () {
            vguid = "";
            isEdit = false;
            $("#myModalLabel_title3").text("新增银行数据");
            $("#BankName").val("");
            $("#BankAccount").val("");
            $("#BankAccountName").val("");
            $("#BankName").removeClass("input_Validate");
            $("#BankAccount").removeClass("input_Validate");
            $("#BankAccountName").removeClass("input_Validate");
            initBorrowTable(companyCode, accountModeCode)
            $("#AddCompanyBankDataDialog").modal({ backdrop: "static", keyboard: false });
            $("#AddCompanyBankDataDialog").modal("show");
        })
        //关闭弹出新增银行信息
        $("#AddCompanyBankData_CancelBtn").on("click", function () {
            $("#AddCompanyBankDataDialog").modal("hide");
        })
        //公司段银行-弹出框中的保存按钮
        $("#AddCompanyBankData_OKButton").on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate($("#BankName"))) {
                validateError++;
            }
            if (!Validate($("#BankAccount"))) {
                validateError++;
            }
            if (!Validate($("#BankAccountName"))) {
                validateError++;
            }
            var borrow = $("#dropDownButtonContentjqxdropdownbutton1")[0].innerText;
            var loan = $("#dropDownButtonContentjqxdropdownbutton2")[0].innerText;
            if (validateError <= 0) {
                $.ajax({
                    url: "/PaymentManagement/CompanySection/SaveCompanyBankInfo?isEdit=" + isEdit,
                    data: {
                        "BankName": $("#BankName").val(),
                        "BankAccount": $("#BankAccount").val(),
                        "BankAccountName": $("#BankAccountName").val(),
                        "CompanyCode": $("#CompanyCodeBank").val(),
                        "AccountType": $("#AccountType").val(),
                        "InitialBalance": $("#InitialBalance").val(),
                        "AccountModeCode": $("#AccountModeCode").val(),
                        "Borrow": borrow,
                        "Loan": loan,
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
                                $("#jqxCompanySetting").jqxGrid('updateBoundData');
                                $('#jqxCompanySetting').jqxGrid('addgroup', 'BankName');
                                $("#AddCompanyBankDataDialog").modal("hide");
                                $("#jqxTable1").jqxDataTable('updateBoundData');
                                break;
                            case "2":
                                jqxNotification("银行下已存在该账号！", null, "error");
                                break;
                            case "3":
                                jqxNotification("公司下已存在一个基本户,保存失败！", null, "error");
                                break;
                            case "4":
                                jqxNotification("公司下已存在一个社保账户,保存失败！", null, "error");
                                break;
                        }
                    }
                });
            }
        })
        //删除银行信息
        $("#AddCompany_CancelBtn").on("click", function () {
            var selection = [];
            var indexes = $("#jqxCompanySetting").jqxGrid('getselectedrowindexes');
            if (indexes.length < 1) {
                jqxNotification("请选择您要删除的数据！", null, "error");
            } else {
                for (var i = 0; i < indexes.length; i++) {
                    var rowdata = $('#jqxCompanySetting').jqxGrid('getcellvalue', indexes[i], "VGUID");
                    selection.push(rowdata);
                }
                WindowConfirmDialog(deleCompanyBank, "您确定要删除选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
        //删除
        selector.$btnDelete().on("click", function () {
            var selection = [];
            if (index == 2) {
                var checkrow = $("#jqxTable2").jqxTreeGrid('getCheckedRows');
                for (var i = 0; i < checkrow.length; i++) {
                    var rowdata = checkrow[i];
                    selection.push(rowdata.VGUID);
                }
            } else {
                var grid = $("#jqxTable" + index);
                var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
                checedBoxs.each(function () {
                    var th = $(this);
                    if (th.is(":checked")) {
                        var index = th.attr("index");
                        var data = grid.jqxDataTable('getRows')[index];
                        selection.push(data.VGUID);
                    }
                });
            }
            if (selection.length < 1) {
                jqxNotification("请选择您要删除的数据！", null, "error");
            } else {
                WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
        //切换标签页
        $('#jqxTabs').on('tabclick', function (event) {
            index = event.args.item;
            console.log(index);
            expandVGUID = [];
            loadTable();
            //$("#jqxTable2").jqxTreeGrid('updateBoundData'); 
            if (index == 2) {
                $("#hideCompany").show();
                $("#ACChange").show();
                $(".SubjectTr").show();
                $("#CompanyCode").show();
                $("#AccountModeCode").show();
                if ($("#txtFirstSubjects").val() == "1") {
                    $("#SubjectCode").hide();
                } else {
                    $("#SubjectCode").show();
                }
            } else {
                $("#hideCompany").show();
                $(".SubjectTr").hide();
                $("#SubjectCode").hide()
                $("#CompanyCode").show();
                $("#AccountModeCode").show();
                $("#ACChange").show();
                //if (index == 1) {
                //    $("#AccountModeCode").show();
                //    $("#CompanyCode").hide();
                //}
            }
            if (index == 1) {
                $("#CompanyCode").hide();
                $("#AccountModeCode").show();
                $("#ACChange").show();
                $("#hideCompany").hide()
            }
            if (index == 0) {
                $("#ACChange").hide();
            }
        });
        //改变一级节点值
        $('#txtFirstSubjects').on('change', function (event) {
            if ($("#txtFirstSubjects").val() == "1") {
                $("#SubjectCode").hide();
                $("#txtParentCode").val("");
            } else {
                $("#SubjectCode").show();
                $("#txtParentCode").val(hidParentCode);
            }
        })
        //启用
        $('#btnEditDo').on("click", function () {
            var status = '1';
            var selection = [];
            if (index == 2) {
                var checkrow = $("#jqxTable2").jqxTreeGrid('getCheckedRows');
                for (var i = 0; i < checkrow.length; i++) {
                    var rowdata = checkrow[i];
                    selection.push(rowdata.VGUID);
                }
            } else {
                var grid = $("#jqxTable" + index);
                var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
                checedBoxs.each(function () {
                    var th = $(this);
                    if (th.is(":checked")) {
                        var index = th.attr("index");
                        var data = grid.jqxDataTable('getRows')[index];
                        selection.push(data.VGUID);
                    }
                });
            }
            if (selection.length < 1) {
                jqxNotification("请选择您要操作的数据！", null, "error");
            } else {
                WindowConfirmDialog(edits, "您确定要启用选中的数据？", "确认框", "确定", "取消", selection, status);
            }
        })
        //禁用
        $('#btnEditDis').on("click", function () {
            var status = '0';
            var selection = [];
            if (index == 2) {
                var checkrow = $("#jqxTable2").jqxTreeGrid('getCheckedRows');
                for (var i = 0; i < checkrow.length; i++) {
                    var rowdata = checkrow[i];
                    selection.push(rowdata.VGUID);
                }
            } else {
                var grid = $("#jqxTable" + index);
                var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
                checedBoxs.each(function () {
                    var th = $(this);
                    if (th.is(":checked")) {
                        var index = th.attr("index");
                        var data = grid.jqxDataTable('getRows')[index];
                        selection.push(data.VGUID);
                    }
                });
            }
            if (selection.length < 1) {
                jqxNotification("请选择您要操作的数据！", null, "error");
            } else {
                WindowConfirmDialog(edits, "您确定要禁用选中的数据？", "确认框", "确定", "取消", selection, status);
            }
        })
        //获取展开的节点
        $("#jqxTable2").on('rowExpand', function (event) {
            var args = event.args;
            var row = args.row;
            var key = args.key;
            expandVGUID.push(key);
        });
        //展开节点
        $('#jqxTable2').on('bindingComplete', function (event) {
            for (var i = 0; i < expandVGUID.length; i++) {
                $("#jqxTable2").jqxTreeGrid('expandRow', expandVGUID[i]);
            }
            $("#jqxTable2").unbind("bindingcomplete");
        });
        //切换公司值
        $('#CompanyCode').on('change', function (event) {
            companyCode = $("#CompanyCode").val();
            console.log(companyCode);
            loadTable();
            //$("#jqxTable2").jqxTreeGrid('updateBoundData');
        })
        //切换账套值
        $('#AccountModeCode').on('change', function (event) {
            accountModeCode = $("#AccountModeCode").val();
            console.log(accountModeCode);
            loadTable();
            //$("#jqxTable2").jqxTreeGrid('updateBoundData');
        })
        $('#jqxSubjectSetting').on('bindingComplete', function (event) {
            //$("#jqxSubjectSetting").jqxTreeGrid('checkRow')
            var firstLevelRows = $("#jqxSubjectSetting").jqxTreeGrid('getRows');
            if (firstLevelRows[0].Count != null && firstLevelRows[0].Count != "") {
                count = firstLevelRows[0].Count.split(",");
                for (var i = 0; i < count.length; i++) {
                    // get a row.
                    $("#jqxSubjectSetting").jqxTreeGrid('checkRow', count[i])
                }
            }
            $("#jqxSubjectSetting").unbind("bindingcomplete");
            //$("#jqxSubjectSetting").jqxTreeGrid('expandRow', 0);
            //$("#jqxSubjectSetting").jqxTreeGrid('collapseRow', 0);
        });
        //编辑默认银行
        $("#jqxCompanySetting").on('cellendedit', function (event) {
            var args = event.args;
            var value = args.value;
            var oldvalue = args.oldvalue;
            var rowData = args.row;
            $.ajax({
                url: "/PaymentManagement/CompanySection/UpdataBankStatus",
                //data: { vguids: selection },
                data: { vguids: rowData.VGUID, datafield: args.datafield, ischeck: args.value, accountModeCode: rowData.AccountModeCode, companyCode: rowData.CompanyCode },
                type: "post",
                success: function (msg) {
                    $("#jqxCompanySetting").jqxGrid('updateBoundData');
                    $('#jqxCompanySetting').jqxGrid('addgroup', 'BankName');
                }
            });
        });
        //清除借贷信息
        $("#Remove1").on("click", function () {
            $("#jqxdropdownbutton1").jqxDropDownButton('setContent', "");
        })
        $("#Remove2").on("click", function () {
            $("#jqxdropdownbutton2").jqxDropDownButton('setContent', "");
        })
        //切换年份
        $('#Year').on('change', function (event) {
            console.log($("#Year").val());
            //$("#jqxTaxesSetting").jqxTreeGrid('updateBoundData')
            initTaxesTable();
        })
        //切换月份
        $('#Month').on('change', function (event) {
            console.log($("#Month").val());
            //$("#jqxTaxesSetting").jqxTreeGrid('updateBoundData')
            initTaxesTable();
        })
    }; //addEvent end

    function loadTable() {
        switch (index) {
            case 1: initTable1();
                break;
            case 2: $("#txtFirstSubjects").val("1"); //$("#jqxTable2").jqxTreeGrid('updateBoundData');
                //$("#jqxTable2").jqxTreeGrid('destroy');
                //$("#jqxTable2").remove();
                //$("#jqxTreeTable2").append('<div id="jqxTable2" class="jqxTable"></div>');
                initTable2();
                $('#jqxTable2').on('bindingComplete', function (event) { $("#jqxTable2").jqxTreeGrid('expandAll'); });
                break;
            case 3: initTable3();
                break;
            case 4: initTable4();
                break;
            case 5: initTable5();
                break;
            case 6: initTable6();
                break;
            case 7: initTable7();
                break;
            default:
        }
    }

    //账套段AccountModeSection
    function initTable0() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Code', type: 'string' },
                    { name: 'Descrption', type: 'string' },
                    { name: 'SectionVGUID', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                     { name: 'Status', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'IsCompanyCode', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: {},
                url: "/PaymentManagement/AccountModeSection/GetCompanySection"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        $("#jqxTable0").jqxDataTable({
            pageable: true,
            width: "100%",
            height: 550,
            pageSize: 99999999,
            serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 40,
            columns: [
                { text: "", datafield: "checkbox", width: 40, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                { text: '编码', datafield: 'Code', width: 80, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                { text: '描述', datafield: 'Descrption', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: statusFunc },
                { text: '公司段', datafield: 'IsCompanyCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },
                { text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center' },
                { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                { text: 'VGUID', datafield: 'VGUID', hidden: true },
            ]
        });

    }
    //公司段
    function initTable1() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Code', type: 'string' },
                    { name: 'Descrption', type: 'string' },
                    { name: 'SectionVGUID', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Setting', type: 'string' },
                    { name: 'VCRTUSER', type: 'string' },
                    { name: 'VMDFUSER', type: 'string' },
                    { name: 'VMDFTIME', type: 'date' },
                    { name: 'VCRTTIME', type: 'date' },
                    { name: 'Status', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'IsCompanyCode', type: 'string' },
                    { name: 'IsAccountModeCode', type: 'string' },
                    { name: 'IsSubjectCode', type: 'string' },
                    { name: 'IsCompanyBank', type: 'string' },
                    { name: 'IsTaxes', type: 'string' },
                    { name: 'OrgID', type: 'string' },
                    { name: 'Abbreviation', type: 'string' },
                    { name: 'Sync', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { accountModeCode: accountModeCode },
                url: "/PaymentManagement/CompanySection/GetCompanySection"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        $("#jqxTable1").jqxDataTable({
            pageable: true,
            width: "100%",
            height: 500,
            pageSize: 99999999,
            serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 40,
            columns: [
                { text: "", datafield: "checkbox", width: 40, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                { text: '编码', datafield: 'Code', width: 80, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                { text: '描述', datafield: 'Descrption', width: 400, align: 'center', cellsAlign: 'center' },
                { text: '组织ID', datafield: 'OrgID', width: 80, align: 'center', cellsAlign: 'center' },
                { text: '简称', datafield: 'Abbreviation', width: 120, align: 'center', cellsAlign: 'center' },
                { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: statusFunc },
                { text: '禁止同步', datafield: 'Sync', width: 100, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererClickFunc, autoRowHeight: false },
                { text: '银行账号设置', datafield: 'IsCompanyBank', width: 120, align: 'center', cellsAlign: 'center', cellsRenderer: setCompanyFunc },
                { text: '科目段', datafield: 'IsSubjectCode', width: 120, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },
                { text: '税率设置', datafield: 'IsTaxes', width: 120, align: 'center', cellsAlign: 'center', cellsRenderer: setTaxesFunc },
                { text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center' },
                { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                { text: 'VGUID', datafield: 'VGUID', hidden: true },

            ]
        });

    }
    //科目段
    function initTable2() {
        var source =
                     {
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
                             { name: 'Sync', type: 'string' },
                         ],
                         hierarchy:
                         {
                             keyDataField: { name: 'Code' },
                             parentDataField: { name: 'ParentCode' }
                         },
                         datatype: "json",
                         cache: false,
                         //async: false,
                         id: "VGUID",
                         data: { "companyCode": companyCode, "accountModeCodes": $("#AccountModeCode").val() },
                         url: "/PaymentManagement/SubjectSection/GetCompanySection"    //获取数据源的路径
                     };
        typeAdapterJqxTree = new $.jqx.dataAdapter(source);
        //创建卡信息列表（主表）
        $("#jqxTable2").jqxTreeGrid({
            pageable: false,
            width: "100%",
            height: 500,
            pageSize: 15,
            //serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapterJqxTree,
            theme: "energyblue",
            columnsHeight: 40,
            checkboxes: true,
            filterable: true,
            ready:function(){
                $("#jqxTable2").jqxTreeGrid('expandAll');
            },
            //hierarchicalCheckboxes: true,
            columns: [
                //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                { text: '编码', datafield: 'Code', width: 150, align: 'center', cellsAlign: 'left', pinned: true, cellsRenderer: detailFuncs },
                { text: '描述', datafield: 'Descrption', width: 200, align: 'center', pinned: true, cellsAlign: 'center' },
                { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', pinned: true, cellsRenderer: statusFunc },
                { text: '禁止同步', datafield: 'Sync', width: 100, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererClickFunc, autoRowHeight: false },
                //{ text: '账套段', datafield: 'IsAccountModeCode', width: 180, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },
                //{ text: '公司段', datafield: 'IsCompanyCode', width: 180, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },  
                { text: '核算段', datafield: 'IsAccountingCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFuncSubject },
                { text: '成本中心段', datafield: 'IsCostCenterCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFuncSubject },
                { text: '备用1', datafield: 'IsSpareOneCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFuncSubject },
                { text: '备用2', datafield: 'IsSpareTwoCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFuncSubject },
                { text: '往来段', datafield: 'IsIntercourseCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFuncSubject },
                { text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center' },
                { text: 'ParentCode', datafield: 'ParentCode', hidden: true },
                { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                { text: 'VGUID', datafield: 'VGUID', hidden: true },
            ]
        });
        
    }
    //核算段
    function initTable3() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Code', type: 'string' },
                    { name: 'Descrption', type: 'string' },
                    { name: 'SectionVGUID', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'IsSetAccount', type: 'string' },
                    { name: 'Sync', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { companyCode: companyCode, accountModeCode: accountModeCode },
                url: "/PaymentManagement/AccountingSection/GetCompanySection"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        $("#jqxTable3").jqxDataTable(
            {
                pageable: true,
                width: "100%",
                height: 500,
                pageSize: 99999999,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                columns: [
                    { text: "", datafield: "checkbox", width: 40, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '编码', datafield: 'Code', width: 80, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '描述', datafield: 'Descrption', width: 210, align: 'center', cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: statusFunc },
                    { text: '禁止同步', datafield: 'Sync', width: 100, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererClickFunc, autoRowHeight: false },
                    { text: '设置账户', datafield: 'IsSetAccount', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingBankFunc },
                    { text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center' },
                    { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }
    //成本中心段
    function initTable4() {
        debugger
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Code', type: 'string' },
                    { name: 'Descrption', type: 'string' },
                    { name: 'SectionVGUID', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Sync', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { companyCode: companyCode, accountModeCode: accountModeCode },
                url: "/PaymentManagement/CostCenterSection/GetCompanySection"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        $("#jqxTable4").jqxDataTable(
            {
                pageable: true,
                width: "100%",
                height: 550,
                pageSize: 99999999,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                columns: [
                   { text: "", datafield: "checkbox", width: 40, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '编码', datafield: 'Code', width: 80, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '描述', datafield: 'Descrption', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: statusFunc },
                    { text: '禁止同步', datafield: 'Sync', width: 100, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererClickFunc, autoRowHeight: false },
                    { text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center' },
                    { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }
    //备用1段
    function initTable5() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Code', type: 'string' },
                    { name: 'Descrption', type: 'string' },
                    { name: 'SectionVGUID', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Sync', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { companyCode: companyCode, accountModeCode: accountModeCode },
                url: "/PaymentManagement/SpareOne/GetCompanySection"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        $("#jqxTable5").jqxDataTable(
            {
                pageable: true,
                width: "100%",
                height: 550,
                pageSize: 99999999,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                columns: [
                     { text: "", datafield: "checkbox", width: 40, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '编码', datafield: 'Code', width: 80, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '描述', datafield: 'Descrption', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: statusFunc },
                    { text: '禁止同步', datafield: 'Sync', width: 100, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererClickFunc, autoRowHeight: false },
                    { text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center' },
                    { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }
    //备用2段
    function initTable6() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Code', type: 'string' },
                    { name: 'Descrption', type: 'string' },
                    { name: 'SectionVGUID', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Sync', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { companyCode: companyCode, accountModeCode: accountModeCode },
                url: "/PaymentManagement/SpareTwo/GetCompanySection"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        $("#jqxTable6").jqxDataTable(
            {
                pageable: true,
                width: "100%",
                height: 550,
                pageSize: 99999999,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                columns: [
                     { text: "", datafield: "checkbox", width: 40, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '编码', datafield: 'Code', width: 80, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '描述', datafield: 'Descrption', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: statusFunc },
                    { text: '禁止同步', datafield: 'Sync', width: 100, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererClickFunc, autoRowHeight: false },
                    { text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center' },
                    { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }
    //往来段
    function initTable7() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Code', type: 'string' },
                    { name: 'Descrption', type: 'string' },
                    { name: 'SectionVGUID', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Sync', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { companyCode: companyCode, accountModeCode: accountModeCode },
                url: "/PaymentManagement/IntercourseSection/GetCompanySection"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        $("#jqxTable7").jqxDataTable(
            {
                pageable: true,
                width: "100%",
                height: 550,
                pageSize: 99999999,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                columns: [
                    { text: "", datafield: "checkbox", width: 40, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '编码', datafield: 'Code', width: 80, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '描述', datafield: 'Descrption', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: statusFunc },
                    { text: '禁止同步', datafield: 'Sync', width: 100, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererClickFunc, autoRowHeight: false },
                    { text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center' },
                    { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }


    var initWidgets = function (tab) {
        switch (tab) {
            case 0:
                initTable0();
                break;
            case 1:
                initTable1();
                break;
            case 2:
                //initTable2();
                break;
            case 3:
                initTable3();
                break;
            case 4:
                initTable4();
                break;
            case 5:
                initTable5();
                break;
            case 6:
                initTable6();
                break;
            case 7:
                initTable7();
                break;
        }
    }
    $('#jqxTabs').jqxTabs({ width: "99%", height: 589, initTabContent: initWidgets });

    function detailFuncs(row, column, value, rowData) {
        var container = "";
        container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.Code + "','" + rowData.Descrption + "','" + rowData.Remark + "','" + rowData.ParentCode + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Code + "</a>";
        return container;
    }

    function statusFunc(row, column, value, rowData) {
        var container = '<div style="color:#30dc32">启用</div>';
        if (value == "0") {
            container = '<div style="color:#F00">禁用</div>';
        }
        return container;
    }

    function settingFuncSubject(row, column, value, rowData) {
        var container = "";
        if (rowData.records == null || rowData.Code == "640301" || rowData.Code == "640302") {
            //没有子节点
            container = "<a href='#' onclick=settingSection('" + column + "','" + rowData.Code + "') style=\"text-decoration: underline;color: #333;\">设置</a>";;
            if (value != "" && (value == "True" || value == true)) {
                container = "<a href='#' onclick=settingSection('" + column + "','" + rowData.Code + "') style=\"text-decoration: underline;color: #fb0f0f;\">已设置</a>";;
            }
        }
        return container;
    }
    function settingFunc(row, column, value, rowData) {
        var container = "<a href='#' onclick=settingSection('" + column + "','" + rowData.Code + "') style=\"text-decoration: underline;color: #333;\">设置</a>";;
        if (value != "" && (value == "True" || value == true)) {
            container = "<a href='#' onclick=settingSection('" + column + "','" + rowData.Code + "') style=\"text-decoration: underline;color: #fb0f0f;\">已设置</a>";;
        }
        return container;
    }

    function settingBankFunc(row, column, value, rowData) {
        var code = rowData.Code.substring(2, 0);
        var container = "";
        if (code == "11") {
            container = "<a href='#' onclick=settingAccount('" + rowData.Code + "') style=\"text-decoration: underline;color: #333;\">设置</a>";;
            if (value != "" && (value == "True" || value == true)) {
                container = "<a href='#' onclick=settingAccount('" + rowData.Code + "') style=\"text-decoration: underline;color: #fb0f0f;\">已设置</a>";;
            }
        }
        return container;
    }

    function setCompanyFunc(row, column, value, rowData) {
        var container = "";
        container = "<a href='#' onclick=settingCompany('" + rowData.Code + "','" + rowData.Descrption + "') style=\"text-decoration: underline;color: #333;\">设置</a>";
        if (value != "" && (value == "True" || value == true)) {
            container = "<a href='#' onclick=settingCompany('" + rowData.Code + "','" + rowData.Descrption + "') style=\"text-decoration: underline;color: #fb0f0f;\">已设置</a>";;
        }
        return container;
    }

    function setTaxesFunc(row, column, value, rowData) {
        var container = "";
        container = "<a href='#' onclick=settingTaxes('" + rowData.Code + "','" + rowData.Descrption + "','" + rowData.VGUID + "') style=\"text-decoration: underline;color: #333;\">设置</a>";
        if (value != "" && (value == "True" || value == true)) {
            container = "<a href='#' onclick=settingTaxes('" + rowData.Code + "','" + rowData.Descrption + "') style=\"text-decoration: underline;color: #fb0f0f;\">已设置</a>";;
        }
        return container;
    }
    //删除
    function dele(selection) {
        //var selection = [];
        //var grid = $("#jqxTable"+index);
        //var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
        //checedBoxs.each(function () {
        //    var th = $(this);
        //    if (th.is(":checked")) {
        //        var index = th.attr("index");
        //        var data = grid.jqxDataTable('getRows')[index];
        //        selection.push(data.VGUID);
        //        //selection.push(data.ArrivedTime);
        //    }
        //});
        $.ajax({
            url: "/PaymentManagement/CompanySection/DeleteCompanySection",
            //data: { vguids: selection },
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
                        if (index == 2) {
                            $("#jqxTable2").jqxTreeGrid('updateBoundData');
                        } else {
                            $("#jqxTable" + index).jqxDataTable('updateBoundData');
                        }
                        break;
                }
            }
        });
    }
    //删除银行信息
    function deleCompanyBank(selection) {
        $.ajax({
            url: "/PaymentManagement/CompanySection/DeleteCompanyBankInfo",
            //data: { vguids: selection },
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
                        $("#jqxCompanySetting").jqxGrid('updateBoundData');
                        $('#jqxCompanySetting').jqxGrid('addgroup', 'BankName');
                        $("#jqxTable1").jqxDataTable('updateBoundData');
                        break;
                }
            }
        });
    }
    //编辑状态
    function edits(selection, status) {
        layer.load();
        $.ajax({
            url: "/PaymentManagement/CompanySection/EditSectionStatus",
            //data: { vguids: selection },
            data: { vguids: selection, status: status },
            traditional: true,
            type: "post",
            success: function (msg) {
                layer.closeAll('loading');
                switch (msg.Status) {
                    case "0":
                        jqxNotification("操作失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("操作成功！", null, "success");
                        if (index == 2) {
                            $("#jqxTable2").jqxTreeGrid('updateBoundData');
                        } else {
                            $("#jqxTable" + index).jqxDataTable('updateBoundData');
                        }
                        break;
                    case "2":
                        jqxNotification("部分科目上级节点已经禁用，不能启用！", null, "error");
                        break;
                }
            }
        });
    }

};


function edit(guid, Code, Descrption, Remark, ParentCode, OrgID, Abbreviation) {
    isEdit = true;
    vguid = guid;
    selector.$txtCode().val(Code);
    selector.$txtDescrption().val(Descrption);
    if (Remark == null || Remark == "null" || Remark == "undefined") {
        Remark = "";
    }
    if (OrgID == null || OrgID == "null" || OrgID == "undefined") {
        OrgID = "";
    }
    if (Abbreviation == null || Abbreviation == "null" || Abbreviation == "undefined") {
        Abbreviation = "";
    }
    $("#txtRemark").val(Remark);
    $("#txtOrgID").val(OrgID);
    $("#txtAbbreviation").val(Abbreviation);
    $("#myModalLabel_title").text("编辑数据");
    //$("#AddNewBankDataDialog table tr").eq(1).hide();
    $(".msg").remove();
    selector.$txtCode().removeClass("input_Validate");
    selector.$txtDescrption().removeClass("input_Validate");
    selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewBankDataDialog().modal("show");
    if (ParentCode == "null" || ParentCode == null || ParentCode == "" || ParentCode == "undefined") {
        $("#txtFirstSubjects").val("1");
        $("#txtParentCode").val("");
        $("#SubjectCode").hide();
        $("#OrgID").hide();
        $("#Abbreviation").hide();
        if (index == 1) {
            $("#OrgID").show();
            $("#Abbreviation").show();
        }
    } else {
        $("#txtFirstSubjects").val("0");
        $("#txtParentCode").val(ParentCode);
        $("#SubjectCode").show();
    }
}
var leftNum = 0;
var topNum = 0;
//设置弹出框，列表
function settingSection(column, code) {
    var scrollOffset = $("#jqxTable2").jqxTreeGrid('scrollOffset');
    if (scrollOffset != null) {
        leftNum = scrollOffset.left;
        topNum = scrollOffset.top;
    }
    $("#hidSubjectCode").val(code);
    $("#AddSectionDialog").modal({ backdrop: "static", keyboard: false });
    $("#AddSectionDialog").modal("show");
    var sVGUID = "";
    var columnName = "";
    var keyColumnName = "CompanyCode";
    switch (column) {
        case "IsSubjectCode": sVGUID = "B63BD715-C27D-4C47-AB66-550309794D43"; $("#hidType").val(0); columnName = "SubjectCode"; break;
        case "IsAccountingCode": sVGUID = "C63BD715-C27D-4C47-AB66-550309794D43"; $("#hidType").val(1); columnName = "AccountingCode"; break;
        case "IsCostCenterCode": sVGUID = "D63BD715-C27D-4C47-AB66-550309794D43"; $("#hidType").val(2); columnName = "CostCenterCode"; break;
        case "IsSpareOneCode": sVGUID = "E63BD715-C27D-4C47-AB66-550309794D43"; $("#hidType").val(3); columnName = "SpareOneCode"; break;
        case "IsSpareTwoCode": sVGUID = "F63BD715-C27D-4C47-AB66-550309794D43"; $("#hidType").val(4); columnName = "SpareTwoCode"; break;
        case "IsIntercourseCode": sVGUID = "G63BD715-C27D-4C47-AB66-550309794D43"; $("#hidType").val(5); columnName = "IntercourseCode"; break;
        case "IsCompanyCode": sVGUID = "A63BD715-C27D-4C47-AB66-550309794D43"; $("#hidType").val(6); columnName = "CompanyCode"; keyColumnName = "AccountModeCode"; break;
        default:
    }
    if ($("#hidType").val() == "0") {
        var count = 0;
        $("#jqxTableSetting").hide();
        $("#jqxSubjectSetting").show();
        var source =
            {
                datafields:
                [
                    { name: "Checked", type: null },
                    { name: 'Code', type: 'string' },
                    { name: 'ParentCode', type: 'string' },
                    { name: 'Descrption', type: 'string' },
                    { name: 'SectionVGUID', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Count', type: 'string' },
                ],
                hierarchy:
                {
                    keyDataField: { name: 'Code' },
                    parentDataField: { name: 'ParentCode' }
                },
                datatype: "json",
                id: "VGUID",
                data: { Code: code, sectionVGUID: sVGUID, columnName: columnName, keyColumnName: keyColumnName, index: index, companyCode: companyCode, accountModeCode: accountModeCode },
                url: "/PaymentManagement/CompanySection/GetSectionInfo"    //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source);
        $("#jqxSubjectSetting").jqxTreeGrid({
            pageable: false,
            width: 800,
            height: 300,
            pageSize: 9999999,
            //serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            //theme: "energyblue",
            columnsHeight: 30,
            checkboxes: true,
            hierarchicalCheckboxes: true,
            columns: [
                { text: '编码', datafield: 'Code', width: 150, align: 'center', cellsAlign: 'left' },
                { text: '描述', datafield: 'Descrption', align: 'center', cellsAlign: 'center' },
                //{ text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', pinned: true, cellsRenderer: statusFunc },
                //{ text: '账套段', datafield: 'IsAccountModeCode', width: 180, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },
                //{ text: '公司段', datafield: 'IsCompanyCode', width: 180, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },                
                //{ text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center' },
                { text: 'Count', datafield: 'Count', hidden: true },
                { text: 'Checked', datafield: "Checked", hidden: true },
                { text: 'ParentCode', datafield: 'ParentCode', hidden: true },
                { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                { text: 'VGUID', datafield: 'VGUID', hidden: true },

            ]
        });
    }
    else {
        $("#jqxTableSetting").show();
        $("#jqxSubjectSetting").hide();
        var source =
        {
            datafields:
            [
                { name: "Checked", type: 'bool' },
                { name: 'Code', type: 'string' },
                { name: 'Descrption', type: 'string' },
                //{ name: 'SectionVGUID', type: 'string' },
                //{ name: 'VGUID', type: 'string' },
                //{ name: 'Status', type: 'string' },
                //{ name: 'Remark', type: 'string' },
            ],
            datatype: "json",
            id: "VGUID",
            data: { Code: code, sectionVGUID: sVGUID, columnName: columnName, keyColumnName: keyColumnName, index: index, companyCode: companyCode, accountModeCode: accountModeCode },
            url: "/PaymentManagement/CompanySection/GetSectionInfo"   //获取数据源的路径
        };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        $("#jqxTableSetting").jqxGrid({
            pageable: true,
            width: 800,
            height: 300,
            pageSize: 10,
            //serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 30,
            editable: true,
            pagermode: 'simple',
            columns: [
                {
                    text: '选择', datafield: "Checked", width: 60, align: 'center', cellsAlign: 'center', columntype: 'checkbox',
                },
                { text: '编码', datafield: 'Code', editable: false, width: 120, align: 'center', cellsAlign: 'center' },
                { text: '描述', datafield: 'Descrption', editable: false, align: 'center', cellsAlign: 'center' },
                //{ text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center', hidden: true },
                //{ text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                //{ text: 'VGUID', datafield: 'VGUID', hidden: true },
            ]
        });
    }
}
//设置公司下银行及银行账号
function settingCompany(code, companyName) {
    $("#CompanyCodeBank").val(code);
    $("#AddCompanyDialog").modal({ backdrop: "static", keyboard: false });
    $("#AddCompanyDialog").modal("show");
    $("#myModalLabel_titles2").text("配置银行信息-" + companyName);
    var source =
       {
           datafields:
           [
               { name: "BankName", type: 'string' },
               { name: 'BankAccount', type: 'string' },
               { name: 'BankAccountName', type: 'string' },
               { name: 'AccountType', type: 'string' },
               { name: "CompanyCode", type: 'string' },
               { name: "AccountModeCode", type: 'string' },
               { name: "InitialBalance", type: 'number' },
               { name: 'VGUID', type: 'string' },
               { name: 'BankStatus', type: 'bool' },
               { name: 'OpeningDirectBank', type: 'bool' },
               { name: 'Borrow', type: 'string' },
               { name: 'Loan', type: 'string' },
               //{ name: 'SectionVGUID', type: 'string' },
               //{ name: 'Remark', type: 'string' },
           ],
           datatype: "json",
           id: "VGUID",
           //root: "entry",
           //record: "content",
           data: { Code: code, accountModeCode: accountModeCode },
           url: "/PaymentManagement/CompanySection/GetCompanyInfo"   //获取数据源的路径
       };
    var typeAdapter = new $.jqx.dataAdapter(source);
    $("#jqxCompanySetting").jqxGrid({
        //pageable: false,           
        width: '1700px',
        height: 400,
        //pageSize: 10,
        //serverProcessing: true,
        //pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        groupable: true,
        groupsexpandedbydefault: true,
        columnsHeight: 40,
        showgroupsheader: false,
        editable: true,
        //pagermode: 'simple',
        selectionmode: 'checkbox',
        groups: ['BankName'],
        columns: [
            { text: '开户行', datafield: "BankName", groupable: true, width: '280px', align: 'center', cellsAlign: 'center', editable: false, cellsRenderer: editBankFunc },
            { text: '银行账号', datafield: 'BankAccount', groupable: true, width: '180px', align: 'center', editable: false, cellsAlign: 'center' },
            { text: '银行户名', datafield: "BankAccountName", groupable: true, width: '280px', align: 'center', editable: false, cellsAlign: 'center' },
            { text: '账户类别', datafield: "AccountType", groupable: true, width: '120px', align: 'center', editable: false, cellsAlign: 'center' },
            { text: '初始余额', datafield: 'InitialBalance', cellsFormat: "d2", width: '150px', align: 'center', editable: false, cellsAlign: 'center' },
            { text: '默认支付银行', datafield: "BankStatus", groupable: true, width: '110px', align: 'center', cellsAlign: 'center', columntype: 'checkbox' },
            { text: '是否开通银企直联', datafield: "OpeningDirectBank", width: '140px', groupable: true, align: 'center', cellsAlign: 'center', columntype: 'checkbox' },
            { text: '借', datafield: "Borrow", groupable: true, width: '180px', align: 'center', editable: false, cellsAlign: 'center' },
            { text: '贷', datafield: "Loan", groupable: true, align: 'center', editable: false, cellsAlign: 'center' },
            { text: '公司编码', datafield: 'CompanyCode', hidden: true, align: 'center', cellsAlign: 'center' },
            { text: '账套编码', datafield: 'AccountModeCode', hidden: true, align: 'center', cellsAlign: 'center' },
            { text: 'VGUID', datafield: 'VGUID', hidden: true }
        ]
    });
    $("#jqxCompanySetting").on("rowclick", function (event) {
        // event arguments.
        var args = event.args;
        // row's bound index.
        var rowdata = args.row.bounddata;
        if (rowdata.level != 0) {
            editBank(rowdata.VGUID, rowdata.CompanyCode, rowdata.BankName, rowdata.BankAccount,
                rowdata.BankAccountName, rowdata.AccountType, rowdata.InitialBalance, rowdata.Borrow, rowdata.Loan);
        }
    });
    //$('#jqxCompanySetting').jqxGrid('expandallgroups');
}
//设置公司各税金,税率
function settingTaxes(code, companyName, subjectGuid) {
    $("#CompanyCodeTaxes").val(code);
    $("#AddTaxesDialog").modal({ backdrop: "static", keyboard: false });
    $("#AddTaxesDialog").modal("show");
    $("#myModalLabel_titles3").text("配置税率-" + companyName);
    setYearMonth(code, accountModeCode);
    //var subjectVGUID = subjectGuid;
    initTaxesTable();
    $('#jqxTaxesSetting').on('bindingComplete', function (event) { $("#jqxTaxesSetting").jqxTreeGrid('expandAll'); });
    //新增税率
    $("#btnTaxesAdd").click(function () {
        $("#TaxesType").val("");
        $("#TaxRate").val("");
        $("#TaxCode").val("");
        $("#ParentMenu").val("");
        $("#hideParentMenu").val("");
        var checkrow = $("#jqxTaxesSetting").jqxTreeGrid('getCheckedRows');
        if (checkrow.length == 0) {
            $("#AddTaxDataDialog").modal({ backdrop: "static", keyboard: false });
            $("#AddTaxDataDialog").modal("show");
            return;
        }
        if (checkrow.length > 1) {
            jqxNotification("请选择一个节点！", null, "error");
            return;
        } else {
            var taxesType = checkrow[0].TaxesType;
            $("#ParentMenu").val(taxesType);
            $("#hideParentMenu").val(checkrow[0].VGUID);
            isEdit = false;
            vguid = "";
            $("#myModalLabel_title").text("新增数据");
            $(".msg").remove();
            //$("#ModuleName").removeClass("input_Validate");
            $("#AddTaxDataDialog").modal({ backdrop: "static", keyboard: false });
            $("#AddTaxDataDialog").modal("show");
        }
    });
    //取消税率
    $("#AddTaxData_CancelBtn").click(function () {
        $("#AddTaxDataDialog").modal("hide");
    })
    //保存税率
    $("#AddTaxData_OKButton").click(function () {
        var validateError = 0;//未通过验证的数量
        if (!Validate($("#BusinessType"))) {
            validateError++;
        }
        if (!Validate($("#hideParentMenu"))) {
            validateError++;
        }
        var url = "/PaymentManagement/CompanySection/SaveTaxesInfo?isEdit=";
        if (validateError <= 0) {
            $.ajax({
                url: url + isEdit,
                data: {
                    "TaxesType": $("#TaxesType").val(),
                    "TaxRate": $("#TaxRate").val(),
                    "TaxCode": $("#TaxCode").val(),
                    "AccountModeCode": $("#AccountModeCode").val(),
                    "CompanyCode": $("#CompanyCodeTaxes").val(),
                    "Year": $("#Year").val(),
                    "Month": $("#Month").val(),
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
                            $("#jqxTaxesSetting").jqxTreeGrid('updateBoundData');
                            $("#AddTaxDataDialog").modal("hide");
                            break;
                        case "2":
                            jqxNotification("结算项目已存在", null, "error");
                            break;
                    }

                }
            });
        }
    })
    //编辑税率
    $("#jqxTaxesSetting").on("rowClick", function (event) {
        var args = event.args;
        var rowKey = args.key;
        var rowData = args.row;
        var columnDataField = args.dataField;
        if (columnDataField == "TaxesType") {
            $("#TaxCode").val("");
            $("#ParentMenu").val("");
            $("#hideParentMenu").val("");
            $("#myModalLabel_title").text("编辑数据");
            var taxesType = rowData.TaxesType;
            var taxRate = rowData.TaxRate;
            if (rowData.parent != null) {
                $("#ParentMenu").val(rowData.parent.BusinessType);
                $("#hideParentMenu").val(rowData.parent.VGUID);
            }
            $("#TaxesType").val(taxesType);
            $("#TaxRate").val(taxRate);
            $("#TaxCode").val(rowData.TaxCode);
            isEdit = true;
            vguid = rowData.VGUID;
            $("#TaxesType").removeClass("input_Validate");
            $("#TaxRate").removeClass("input_Validate");
            $("#AddTaxDataDialog").modal({ backdrop: "static", keyboard: false });
            $("#AddTaxDataDialog").modal("show");
        }
    });
    //删除税率
    $("#btnTaxesDelete").click(function () {
        var selection = [];
        var checkrow = $("#jqxTaxesSetting").jqxTreeGrid('getCheckedRows');
        for (var i = 0; i < checkrow.length; i++) {
            var rowdata = checkrow[i];
            selection.push(rowdata.VGUID);
        }
        if (selection.length < 1) {
            jqxNotification("请选择您要删除的数据！", null, "error");
        } else {
            WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消", selection);
        }
    })
    //删除
    function dele(selection) {
        $.ajax({
            url: "/PaymentManagement/CompanySection/DeleteTaxesInfo",
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
                        $("#jqxTaxesSetting").jqxTreeGrid('updateBoundData');
                        break;
                }
            }
        });
    }
}
function initTaxesTable() {
    var source =
       {
           datafields:
           [
               { name: 'TaxesType', type: 'string' },
               { name: 'TaxRate', type: 'string' },
               { name: 'TaxCode', type: 'string' },
               { name: "VGUID", type: 'string' },
               { name: "ParentVGUID", type: 'string' },
               { name: 'AccountModeCode', type: 'string' },
               { name: "CompanyCode", type: 'string' },
           ],
           hierarchy: {
               keyDataField: { name: 'VGUID' },
               parentDataField: { name: 'ParentVGUID' }
           },
           datatype: "json",
           //cache: false,
           id: "VGUID",
           data: { companyCode: $("#CompanyCodeTaxes").val(), accountModeCode: accountModeCode, year: $("#Year").val(), month: $("#Month").val() },
           url: "/PaymentManagement/CompanySection/GetTaxesInfo"   //获取数据源的路径
       };
    var typeAdapter = new $.jqx.dataAdapter(source);
    $("#jqxTaxesSetting").jqxTreeGrid({
        pageable: false,
        width: "100%",
        height: 500,
        pageSize: 15,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "energyblue",
        columnsHeight: 30,
        checkboxes: true,
        filterable: true,
        //selectionMode: 'singlecells',
        //ready: function () {
        //    $("#jqxTaxesSetting").jqxTreeGrid('expandAll');
        //},
        //hierarchicalCheckboxes: true,
        columns: [
            //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
            { text: '税种', datafield: 'TaxesType', cellsRenderer: detailTaxFunc },
            { text: '税率', datafield: 'TaxRate', align: 'center', cellsAlign: 'center', },
            { text: '', datafield: 'VGUID', hidden: true },
            { text: '', datafield: 'TaxCode', hidden: true },
            { text: '', datafield: 'ParentVGUID', hidden: true },
            { text: '', datafield: 'AccountModeCode', hidden: true },
            { text: '', datafield: 'CompanyCode', hidden: true },
        ]
    });
}
function detailTaxFunc(row, column, value, rowData) {
    if (rowData.TaxCode == null) {
        return container = "<a href='#' style=\"text-decoration: underline;color: #333;\">" + rowData.TaxesType + "</a>";
    } else {
        return container = "<a href='#' style=\"text-decoration: underline;color: #333;\">" + rowData.TaxCode + "-" + rowData.TaxesType + "</a>";
    }
}
function setYearMonth(code, accountModeCode) {
    $.ajax({
        url: "/PaymentManagement/CompanySection/GetTaxesYearMonth",
        data: {        
            accountModeCode: accountModeCode,
            companyCode: code,
        },
        type: "post",
        async: false,
        dataType: "json",
        success: function (msg) {
            if (msg != null) {
                $("#Year").val(msg[0].Year);
                $("#Month").val(msg[0].Month);
            }
        }
    })
}
//设置核算段银行账号
function settingAccount(code) {
    $("#hidSubjectCode").val(code);
    $("#AddAccountSettingDialog").modal({ backdrop: "static", keyboard: false });
    $("#AddAccountSettingDialog").modal("show");
    var source =
       {
           datafields:
           [
               { name: "BankName", type: 'string' },
               { name: 'BankAccount', type: 'string' },
               { name: 'BankAccountName', type: 'string' },
               { name: 'AccountType', type: 'string' },
               { name: "CompanyCode", type: 'string' },
               { name: 'VGUID', type: 'string' },
               { name: 'IsChecked', type: 'string' },
               { name: "CompanyName", type: 'string' },
               //{ name: 'SectionVGUID', type: 'string' },
               //{ name: 'VGUID', type: 'string' },
               //{ name: 'Status', type: 'string' },
               //{ name: 'Remark', type: 'string' },
           ],
           datatype: "json",
           id: "VGUID",
           //root: "entry",
           //record: "content",
           data: { Code: code },
           url: "/PaymentManagement/CompanySection/GetAccountCompanyInfo"   //获取数据源的路径
       };
    var typeAdapter = new $.jqx.dataAdapter(source);
    $("#jqxAccountSetting").jqxGrid({
        //pageable: false,           
        width: '1000px',
        height: 400,
        //pageSize: 10,
        //serverProcessing: true,
        //pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        groupable: true,
        groupsexpandedbydefault: true,
        columnsHeight: 40,
        showgroupsheader: false,
        editable: true,
        //pagermode: 'simple',
        //selectionmode: 'checkbox',
        groups: ['BankName'],
        columns: [
            { text: '选择', datafield: "IsChecked", width: 60, align: 'center', cellsAlign: 'center', columntype: 'checkbox' },
            { text: '开户行', datafield: "BankName", groupable: true, editable: false, width: '200px', align: 'center', cellsAlign: 'center' },
            { text: '银行账号', datafield: 'BankAccount', groupable: true, editable: false, width: '200px', align: 'center', cellsAlign: 'center' },
            { text: '银行户名', datafield: "BankAccountName", groupable: true, editable: false, width: '200px', align: 'center', cellsAlign: 'center' },
            { text: '账户类别', datafield: "AccountType", groupable: true, editable: false, width: '150px', align: 'center', cellsAlign: 'center' },
            { text: '公司名称', datafield: "CompanyName", groupable: true, editable: false, align: 'center', cellsAlign: 'center' },
            { text: '公司编码', datafield: 'CompanyCode', hidden: true, align: 'center', cellsAlign: 'center' },
            { text: 'VGUID', datafield: 'VGUID', hidden: true }
        ]
    });
}

function detailFunc(row, column, value, rowData) {
    var container = "";
    container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.Code + "','" + rowData.Descrption + "','" + rowData.Remark + "','" + rowData.ParentCode + "','" + rowData.OrgID + "','" + rowData.Abbreviation + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Code + "</a>";
    return container;
}

function editBankFunc(row, columnfield, value, defaulthtml, columnproperties) {
    var container = "";
    container = "<div style=\"text-decoration: underline;text-align: center;margin-top: 4px;color: #333;\">" + value + "</div>";
    return container;
}
function editTaxesFunc(row, column, value, rowData) {
    var container = "";
    container = "<a href='#' style=\"text-decoration: underline;color: #333;\">" + rowData.Code + "</a>";
    return container;
}

function cellsRendererFunc(row, column, value, rowData) {
    return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\" style=\"margin:auto;width: 17px;height: 17px;\" />";
}
function cellsRendererClickFunc(row, column, value, rowData) {
    if (value == true) {
        return "<input index=\"" + row + "\" type=\"checkbox\" checked onclick=\"CheckBoxClick('" + rowData.VGUID + "','" + value + "')\" style=\"margin:auto;width: 17px;height: 17px;\" />";
    } else {
        return "<input index=\"" + row + "\" type=\"checkbox\" onclick=\"CheckBoxClick('" + rowData.VGUID + "','" + value + "')\" style=\"margin:auto;width: 17px;height: 17px;\" />";
    }
}

function CheckBoxClick(VGUID, value) {
    var changeValue = false;
    if (value == "false") {
        changeValue = true;
    }
    $.ajax({
        url: "/PaymentManagement/CompanySection/UpdataSyncStatus",
        data: { vguids: VGUID, ischeck: changeValue },
        type: "post",
        success: function (msg) {
            if (index == 2) {
                $("#jqxTable2").jqxTreeGrid("updateBoundData");
            }
        }
    });
}

function rendererFunc() {
    var checkBox = "<div id='jqx_datatable_checkbox_all' class='jqx_datatable_checkbox_all' style='z-index: 999; margin-left:7px ;margin-top: 7px;'>";
    checkBox += "</div>";
    return checkBox;
}

function renderedFunc(element) {
    var grid = $("#jqxTable" + index);
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

function editBank(guid, companyCode, bankName, bankAccount, bankAccountName, accountType, initialBalance, borrow, loan) {
    $("#BankName").val("");
    $("#BankAccount").val("");
    $("#BankAccountName").val("");
    $("#CompanyCodeBank").val(companyCode)
    isEdit = true;
    vguid = guid;
    if (bankName == null || bankName == "null") {
        bankName = "";
    }
    if (bankAccount == null || bankAccount == "null") {
        bankAccount = "";
    }
    if (bankAccountName == null || bankAccountName == "null") {
        bankAccountName = "";
    }
    if (borrow == null || borrow == "null") {
        borrow = "";
    }
    if (loan == null || loan == "null") {
        loan = "";
    }
    $("#BankName").val(bankName);
    $("#BankAccount").val(bankAccount);
    $("#BankAccountName").val(bankAccountName);
    $("#AccountType").val(accountType);
    $("#InitialBalance").val(initialBalance);
    $("#myModalLabel_title3").text("编辑银行数据");
    //$("#AddNewBankDataDialog table tr").eq(1).hide();
    $(".msg").remove();
    $("#BankName").removeClass("input_Validate");
    $("#BankAccount").removeClass("input_Validate");
    $("#BankAccountName").removeClass("input_Validate");
    initBorrowTable(companyCode, accountModeCode);
    var val = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + borrow + '</div>';
    $("#jqxdropdownbutton1").jqxDropDownButton('setContent', val);
    var val2 = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + loan + '</div>';
    $("#jqxdropdownbutton2").jqxDropDownButton('setContent', val2);
    $("#AddCompanyBankDataDialog").modal({ backdrop: "static", keyboard: false });
    $("#AddCompanyBankDataDialog").modal("show");
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
}

function initBorrowTable(companyCode, accountModeCode) {
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
        height: 350,
        columnsresize: true,
        pageSize: 15,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        rendergridrows: function (obj) {
            return obj.data;
        },
        virtualmode: false,
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
            { text: '余额', datafield: 'Balance', cellsFormat: "d2", align: 'center', cellsAlign: 'center', hidden: true },

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
        width: 277, height: 30
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
        height: 350,
        columnsresize: true,
        pageSize: 15,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        rendergridrows: function (obj) {
            return obj.data;
        },
        virtualmode: false,
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
            { text: '余额', datafield: 'Balance', cellsFormat: "d2", align: 'center', cellsAlign: 'center', hidden: true },

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
        width: 277, height: 30
    });
    $("#grid2").on('rowclick', function (event) {
        var args = event.args;
        var row = $("#grid2").jqxGrid('getrowdata', args.rowindex);
        var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 6px;">' + row['BusinessCode'] + '</div>';
        $("#jqxdropdownbutton2").jqxDropDownButton('setContent', dropDownContent);
    });
}

function initSubjectTable() {
    var source = {
        datafields:
        [
            { name: 'BOOK', type: 'string' },
            { name: 'VALUE_SET', type: 'string' },
            { name: 'CODE', type: 'string' },
            { name: 'DESCRIPTION', type: 'string' },
            { name: 'ACTIVE_FLAG', type: 'string' },
            { name: 'ParentCode', type: 'string' },
        ],
        datatype: "json",
        id: "",
        data: {},
        url: "/PaymentManagement/CompanySection/SyncSubjectData"   //获取数据源的路径
    };
    var typeAdapter = new $.jqx.dataAdapter(source);

    //创建卡信息列表（主表）
    $("#jqxSubjectTable").jqxGrid({
        pageable: false,
        selectionmode: 'checkBox',
        width: "100%",
        autoheight: false,
        height: 500,
        pageSize: 10,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        groupable: true,
        groupsexpandedbydefault: true,
        groups: ['BOOK', 'VALUE_SET'],
        showgroupsheader: false,
        //showgroupmenuitems: false,
        columnsHeight: 50,
        pagermode: 'simple',
        editable: true,
        columns: [
            { text: '账套', datafield: "BOOK", width: 200, align: 'center', cellsAlign: 'center' },
            { text: '所属科目', datafield: 'VALUE_SET', width: 150, align: 'center', cellsAlign: 'center', editable: false },
            { text: '编码', datafield: 'CODE', width: 120, align: 'center', cellsAlign: 'center', editable: false },
            { text: '描述', datafield: 'DESCRIPTION', width: 300, align: 'center', cellsAlign: 'center', editable: false },
            {
                text: '状态', datafield: 'ACTIVE_FLAG', align: 'center', width: 120, cellsAlign: 'center', editable: false, cellsrenderer: function (row, columnfield, value, defaulthtml, columnproperties) {
                    if (value == "Y") {
                        return "<div style='margin:4px;text-align: center'>启用</div>";
                    }
                    else {
                        return "<div style='margin:4px;text-align: center'>禁用</div>";
                    }
                }
            },
            { text: '父级编码', datafield: 'ParentCode', align: 'center', cellsAlign: 'center', editable: true },
        ]
    });
}