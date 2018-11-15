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
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {

        //加载列表数据
        //initTable();


        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            selector.$txtDatedTime().val("");
        });

        //新增
        selector.$btnAdd().on("click", function () {
            if (index == 2) {
                $(".SubjectTr").show();
                var checkrow = $("#jqxTable2").jqxTreeGrid('getCheckedRows');
                if (checkrow.length != 1) {
                    jqxNotification("请选择一个节点！", null, "error");
                    return;
                } else {
                    var parentCode = checkrow[0].Code;
                    if ($("#txtFirstSubjects").val() == "1") {
                        $("#SubjectCode").hide();
                        $("#txtParentCode").val("");
                    } else {
                        $("#SubjectCode").show();
                        hidParentCode = parentCode;
                        $("#txtParentCode").val(parentCode);
                    }
                }
            } else {
                $(".SubjectTr").hide();
                $("#SubjectCode").hide()
            }
            selector.$txtCode().val("");
            selector.$txtDescrption().val("");
            $("#txtFirstSubjects").val("0");
            //$("#txtParentCode").val("");
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
            }
            else {
                var data = $('#jqxTableSetting').jqxGrid('getdisplayrows')
                for (var i = 0; i < data.length; i++) {
                    if (data[i].Checked == "True" || data[i].Checked == true) {
                        otherCode.push(data[i].Code);
                    }
                }
            }
            $.ajax({
                url: "/PaymentManagement/CompanySection/SaveSectionSetting",
                //data: { vguids: selection },
                data: { code: code, otherCode: otherCode, type: type },
                traditional: true,
                type: "post",
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("操作失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("操作成功！", null, "success");
                            $("#AddSectionDialog").modal("hide");
                            $("#jqxTable"+ index).jqxDataTable('updateBoundData');
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
            $.ajax({
                url: "/PaymentManagement/CompanySection/SaveAccoutSetting",
                //data: { vguids: selection },
                data: { code: code, vguid: vguid },
                traditional: true,
                type: "post",
                success: function (msg) {
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
                case 0: url = "/PaymentManagement/SubjectSection/SaveCompanySection?isEdit=";
                    break;
                case 1: url = "/PaymentManagement/AccountModeSection/SaveCompanySection?isEdit=";
                    break;
                case 2: url = "/PaymentManagement/CompanySection/SaveCompanySection?isEdit=";
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
                $.ajax({
                    url: url + isEdit,
                    data: {
                        "Code": selector.$txtCode().val(),
                        "Descrption": selector.$txtDescrption().val(),
                        "Remark": $("#txtRemark").val(),
                        "VGUID": vguid,
                        "ParentCode": $("#txtParentCode").val()
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
            if (validateError <= 0) {
                $.ajax({
                    url: "/PaymentManagement/CompanySection/SaveCompanyBankInfo?isEdit=" + isEdit,
                    data: {
                        "BankName": $("#BankName").val(),
                        "BankAccount": $("#BankAccount").val(),
                        "BankAccountName": $("#BankAccountName").val(),
                        "CompanyCode": $("#CompanyCode").val(),
                        "AccountType": $("#AccountType").val(),
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
            $("#jqxTable2").jqxTreeGrid('updateBoundData');
            if (index == 2) {
                $(".SubjectTr").show();
                if ($("#txtFirstSubjects").val() == "1") {
                    $("#SubjectCode").hide();
                } else {
                    $("#SubjectCode").show();
                }
            } else {
                $(".SubjectTr").hide();
                $("#SubjectCode").hide()
            }
            //if (index != 0 && index != 1) {
            //    $("#SubjectList").show();
            //    initSubjectSection();
            //}
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
        });

    }; //addEvent end

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
            pageSize: 10,
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
                    { name: 'IsAccountingCode', type: 'string' },
                    { name: 'IsCostCenterCode', type: 'string' },
                    { name: 'IsSpareOneCode', type: 'string' },
                    { name: 'IsSpareTwoCode', type: 'string' },
                    { name: 'IsIntercourseCode', type: 'string' },
                    { name: 'IsAccountModeCode', type: 'string' },
                    { name: 'IsSubjectCode', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: {},
                url: "/PaymentManagement/CompanySection/GetCompanySection"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        $("#jqxTable1").jqxDataTable(
            {
                pageable: true,
                width: "100%",
                height: 550,
                pageSize: 10,
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
                    { text: '银行账号设置', datafield: 'Setting', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: setCompanyFunc },
                    { text: '科目段', datafield: 'IsSubjectCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },
                    { text: '核算段', datafield: 'IsAccountingCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },
                    { text: '成本中心段', datafield: 'IsCostCenterCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },
                    { text: '备用1', datafield: 'IsSpareOneCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },
                    { text: '备用2', datafield: 'IsSpareTwoCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },
                    { text: '往来段', datafield: 'IsIntercourseCode', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },
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
                ],
                hierarchy:
                {
                    keyDataField: { name: 'Code' },
                    parentDataField: { name: 'ParentCode' }
                },
                datatype: "json",
                id: "VGUID",
                data: {},
                url: "/PaymentManagement/SubjectSection/GetCompanySection"    //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source);
        //创建卡信息列表（主表）
        $("#jqxTable2").jqxTreeGrid({
                pageable: false,
                width: "100%",
                height: 450,
                pageSize: 15,
                //serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "energyblue",
                columnsHeight: 40,
                checkboxes: true,
                //hierarchicalCheckboxes: true,
                columns: [
                    //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '编码', datafield: 'Code', width: 150, align: 'center', cellsAlign: 'left', pinned: true, cellsRenderer: detailFuncs },
                    { text: '描述', datafield: 'Descrption', width: 200, align: 'center', pinned: true, cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', pinned: true, cellsRenderer: statusFunc },
                    //{ text: '账套段', datafield: 'IsAccountModeCode', width: 180, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },
                    //{ text: '公司段', datafield: 'IsCompanyCode', width: 180, align: 'center', cellsAlign: 'center', cellsRenderer: settingFunc },                
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
                ],
                datatype: "json",
                id: "VGUID",
                data: {},
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
                height: 550,
                pageSize: 10,
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
                    { text: '设置账户', datafield: 'IsSetAccount', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: settingBankFunc },
                    { text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center' },
                    { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }
    //成本中心段
    function initTable4() {
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
                ],
                datatype: "json",
                id: "VGUID",
                data: {},
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
                pageSize: 10,
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
                ],
                datatype: "json",
                id: "VGUID",
                data: {},
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
                pageSize: 10,
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
                ],
                datatype: "json",
                id: "VGUID",
                data: {},
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
                pageSize: 10,
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
                ],
                datatype: "json",
                id: "VGUID",
                data: {},
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
                pageSize: 10,
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
                initTable2();
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
    $('#jqxTabs').jqxTabs({ width: "100%", height: 600, initTabContent: initWidgets });

    function detailFuncs(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "1") {
            container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.Code + "','" + rowData.Descrption + "','" + rowData.Remark + "','" + rowData.ParentCode + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Code + "</a>";
        } else {
            container = "<span>" + rowData.Code + "</span>";
        }
        return container;
    }

    function statusFunc(row, column, value, rowData) {
        var container = '<div style="color:#30dc32">启用</div>';
        if (value == "0") {
            container = '<div style="color:#F00">禁用</div>';
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
        var container = "<a href='#' onclick=settingCompany('" + rowData.Code + "','" + rowData.Descrption + "') style=\"text-decoration: underline;color: #333;\">设置</a>";
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
                        break;
                }
            }
        });
    }
    //编辑状态
    function edits(selection, status) {
        $.ajax({
            url: "/PaymentManagement/CompanySection/EditSectionStatus",
            //data: { vguids: selection },
            data: { vguids: selection, status: status },
            traditional: true,
            type: "post",
            success: function (msg) {
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


function edit(guid, Code, Descrption, Remark, ParentCode) {
    isEdit = true;
    vguid = guid;
    selector.$txtCode().val(Code);
    selector.$txtDescrption().val(Descrption);
    if (Remark == null || Remark == "null") {
        Remark = "";
    }
    $("#txtRemark").val(Remark);
    $("#myModalLabel_title").text("编辑数据");
    //$("#AddNewBankDataDialog table tr").eq(1).hide();
    $(".msg").remove();
    selector.$txtCode().removeClass("input_Validate");
    selector.$txtDescrption().removeClass("input_Validate");
    selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewBankDataDialog().modal("show");
    if (ParentCode == "null" || ParentCode == null) {
        $("#txtFirstSubjects").val("1");
        $("#txtParentCode").val("");
        $("#SubjectCode").hide()
    } else {
        $("#txtFirstSubjects").val("0");
        $("#txtParentCode").val(ParentCode);
        $("#SubjectCode").show();
    }
}
//设置弹出框，列表
function settingSection(column, code) {
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
                data: { Code: code, sectionVGUID: sVGUID, columnName: columnName, keyColumnName: keyColumnName },
                url: "/PaymentManagement/CompanySection/GetSectionInfo"    //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source);
        $("#jqxSubjectSetting").jqxTreeGrid({
            pageable: false,
            width: 460,
            height: 300,
            pageSize: 15,
            //serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "energyblue",
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
                { text: 'Checked', datafield: "Checked", hidden: true },
                { text: 'ParentCode', datafield: 'ParentCode', hidden: true },
                { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                { text: 'VGUID', datafield: 'VGUID', hidden: true },
                { text: 'Count', datafield: 'Count', hidden: true },
            ]
        });
        
        $('#jqxSubjectSetting').on('bindingComplete', function (event) {
            //$("#jqxSubjectSetting").jqxTreeGrid('showColumn', 'Checked');
            $("#jqxSubjectSetting").jqxTreeGrid('checkRow')
            var firstLevelRows = $("#jqxSubjectSetting").jqxTreeGrid('getRows');
            if (firstLevelRows[0].Count != "") {
                count = firstLevelRows[0].Count.split(",");
                for (var i = 0; i < count.length; i++) {
                    // get a row.
                    $("#jqxSubjectSetting").jqxTreeGrid('checkRow', count[i])
                }
            } 
        });
    }
    else {
        $("#jqxTableSetting").show();
        $("#jqxSubjectSetting").hide();
        var source =
        {
            datafields:
            [
                { name: "Checked", type: null },
                { name: 'Code', type: 'string' },
                { name: 'Descrption', type: 'string' },
                //{ name: 'SectionVGUID', type: 'string' },
                //{ name: 'VGUID', type: 'string' },
                //{ name: 'Status', type: 'string' },
                //{ name: 'Remark', type: 'string' },
            ],
            datatype: "json",
            id: "VGUID",
            data: { Code: code, sectionVGUID: sVGUID, columnName: columnName, keyColumnName: keyColumnName },
            url: "/PaymentManagement/CompanySection/GetSectionInfo"   //获取数据源的路径
        };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        $("#jqxTableSetting").jqxGrid({
            pageable: true,
            width: 460,
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
                { text: '选择', datafield: "Checked", width: 60, align: 'center', cellsAlign: 'center', columntype: 'checkbox' },
                { text: '编码', datafield: 'Code', editable: false, width: 120, align: 'center', cellsAlign: 'center' },
                { text: '描述', datafield: 'Descrption', editable: false, align: 'center', cellsAlign: 'center' },
                //{ text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                //{ text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center', hidden: true },
                //{ text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                //{ text: 'VGUID', datafield: 'VGUID', hidden: true },
            ]
        });
    }
    
}
//设置公司下银行及银行账号
function settingCompany(code, companyName) {
    $("#CompanyCode").val(code);
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
               { name: 'VGUID', type: 'string' },
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
           url: "/PaymentManagement/CompanySection/GetCompanyInfo"   //获取数据源的路径
       };
    var typeAdapter = new $.jqx.dataAdapter(source);
    $("#jqxCompanySetting").jqxGrid({
        //pageable: false,           
        width: '800px',
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
        //editable: false,
        //pagermode: 'simple',
        selectionmode: 'checkbox',
        groups: ['BankName'],
        columns: [
            { text: '开户行', datafield: "BankName", groupable: true, width: '200px', align: 'center', cellsAlign: 'center', cellsRenderer: editBankFunc },
            { text: '银行账号', datafield: 'BankAccount', groupable: true, width: '200px', align: 'center', cellsAlign: 'center' },
            { text: '银行户名', datafield: "BankAccountName", groupable: true, width: '200px', align: 'center', cellsAlign: 'center' },
            { text: '账户类别', datafield: "AccountType", groupable: true, align: 'center', cellsAlign: 'center' },
            { text: '公司编码', datafield: 'CompanyCode', hidden: true, align: 'center', cellsAlign: 'center' },
            { text: 'VGUID', datafield: 'VGUID', hidden: true }
        ]
    });
    $("#jqxCompanySetting").on("rowclick", function (event) {
        // event arguments.
        var args = event.args;
        // row's bound index.
        var rowdata = args.row.bounddata;
        if (rowdata.level != 0) {
            editBank(rowdata.VGUID, rowdata.CompanyCode, rowdata.BankName, rowdata.BankAccount, rowdata.BankAccountName);
        }
    });
    //$('#jqxCompanySetting').jqxGrid('expandallgroups');
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
    if (selector.$EditPermission().val() == "1") {
        container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.Code + "','" + rowData.Descrption + "','" + rowData.Remark + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Code + "</a>";
    } else {
        container = "<span>" + rowData.Code + "</span>";
    }
    return container;
}

function editBankFunc(row, columnfield, value, defaulthtml, columnproperties) {
    var container = "";
    if (selector.$EditPermission().val() == "1") {
        container = "<div style=\"text-decoration: underline;text-align: center;margin-top: 4px;color: #333;\">" + value + "</div>";
    } else {
        container = "<span>" + value + "</span>";
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

function editBank(guid, companyCode, bankName, bankAccount, bankAccountName) {
    $("#BankName").val("");
    $("#BankAccount").val("");
    $("#BankAccountName").val("");
    $("#CompanyCode").val(companyCode)
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
    $("#BankName").val(bankName);
    $("#BankAccount").val(bankAccount);
    $("#BankAccountName").val(bankAccountName);
    $("#myModalLabel_title3").text("编辑银行数据");
    //$("#AddNewBankDataDialog table tr").eq(1).hide();
    $(".msg").remove();
    $("#BankName").removeClass("input_Validate");
    $("#BankAccount").removeClass("input_Validate");
    $("#BankAccountName").removeClass("input_Validate");
    $("#AddCompanyBankDataDialog").modal({ backdrop: "static", keyboard: false });
    $("#AddCompanyBankDataDialog").modal("show");
}

$(function () {
    var page = new $page();
    page.init();
});
