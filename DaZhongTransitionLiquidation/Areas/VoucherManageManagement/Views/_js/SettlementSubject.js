//所有元素选择器
var selector = {
    $grid: function () { return $("#moduletree") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnEdit: function () { return $("#btnEdit") },
    $btnDelete: function () { return $("#btnDelete") },
    $AddNewBankDataDialog: function () { return $("#AddNewBankDataDialog") },
    $AddNewBankData_OKButton: function () { return $("#AddNewBankData_OKButton") },
    $AddNewBankData_CancelBtn: function () { return $("#AddNewBankData_CancelBtn") },
}

var parentVguid = "";
var hideParentMenu = "";
var vguid = "";

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
            if (!Validate($("#ModuleName"))) {
                validateError++;
            }
            if (!Validate($("#hideParentMenu"))) {
                validateError++;
            }
            var url = "/VoucherManageManagement/BusinessTypeSet/SettlementSubject?isEdit=";
            if (validateError <= 0) {
                $.ajax({
                    url: url + isEdit,
                    data: {
                        "Code": $("#ModuleCode").val(),
                        "BusinessName": $("#ModuleName").val(),
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
                                jqxNotification("编号已存在", null, "error");
                                break;
                            case "3":
                                jqxNotification("父级菜单不存在", null, "error");
                                break;
                        }

                    }
                });
            }
        });
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
    selector.$grid().on('rowClick',function (event) {
        // event args.
        var args = event.args;
        // row data.
        var row = args.row;
        // row key.
        var key = args.key;
        // data field
        var dataField = args.dataField;
        // original click event.
        var clickEvent = args.originalEvent;
        initTable(row.VGUID);
        $("#VGUID").val(row.VGUID);
    });
}

function initTable(vguid) {
    var source =
   {
       datafields:
       [
           //{ name: "checkbox", type: null },
           { name: 'AccountModeName', type: 'string' },
           { name: 'CompanyName', type: 'string' },
           { name: 'Borrow', type: 'string' },
           { name: 'Loan', type: 'string' },
           { name: 'AccountModeCode', type: 'string' },
           { name: 'CompanyCode', type: 'string' },
           { name: 'SettlementVGUID', type: 'string' },
           { name: 'VGUID', type: 'string' },
       ],
       datatype: "json",
       id: "VGUID",
       data: { settlementVGUID: vguid },
       url: "/VoucherManageManagement/SettlementSubject/GetSettlementData" //获取数据源的路径
   };
    var typeAdapter = new $.jqx.dataAdapter(source);
    //创建卡信息列表（主表）
    $("#datatable").jqxGrid(
        {
            pageable: false,
            width: "100%",
            height: 450,
            pageSize: 10,
            //serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            groupable: false,
            //groupsexpandedbydefault: true,
            //groups: ['Model', 'ClassType', 'CarType'],
            showgroupsheader: false,
            columnsHeight: 40,
            pagermode: 'simple',
            selectionmode: 'singlerow',
            columns: [
                //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                { text: '账套', datafield: 'AccountModeName', width: 300, align: 'center', cellsAlign: 'center', },
                { text: '公司', datafield: 'CompanyName', width: 350, align: 'center', cellsAlign: 'center' },
                { text: '借', datafield: 'Borrow', width: 300, align: 'center', cellsAlign: 'center' },
                { text: '贷', datafield: 'Loan',align: 'center', cellsAlign: 'center' },
                //{ text: '金额', datafield: 'Money', cellsFormat: "d2", align: 'center', cellsAlign: 'center' },
                { text: 'SettlementVGUID', datafield: 'SettlementVGUID', hidden: true },
                { text: 'VGUID', datafield: 'VGUID', hidden: true },
            ]
        });
}

$(function () {
    var page = new $page();
    page.init();
});