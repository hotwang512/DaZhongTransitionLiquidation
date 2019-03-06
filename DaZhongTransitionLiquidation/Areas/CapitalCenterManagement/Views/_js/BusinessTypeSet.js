//所有元素选择器
var selector = {
    $grid: function () { return $("#moduletree") },
    $btnAdd: function () { return $("#btnAdd") },
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
        //新增
        selector.$btnAdd().click(function () {
            var checkrow = selector.$grid().jqxTreeGrid('getCheckedRows');
            if (checkrow.length != 1) {
                jqxNotification("请选择一个节点！", null, "error");
                return;
            } else {
                parentVguid = checkrow[0].VGUID;
                hideParentMenu = checkrow[0].BusinessName;
                var code = checkrow[0].Code;
                if ($("#FirstMenu").val() == "1") {
                    $("#SubjectCode").hide();//父级菜单
                    $("#FirstMenu").val("");
                } else {
                    $("#SubjectCode").show();
                    $("#hideParentMenu").val(parentVguid);
                    $("#ParentMenu").val(hideParentMenu);
                    $("#ModuleCode").val(code);
                }
            }


            $("#ModuleName").val("");

            $("#FirstMenu").val("0");
            //$("#txtParentCode").val("");
            //$("#txtRemark").val("");
            isEdit = false;
            vguid = "";
            $("#myModalLabel_title").text("新增数据");
            //$("#AddNewBankDataDialog table tr").eq(1).show();
            $(".msg").remove();
            $("#ModuleName").removeClass("input_Validate");


            selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
            selector.$AddNewBankDataDialog().modal("show");

        });
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
                selection.push(rowdata.Vguid);
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
            if (!Validate($("#hideParentMenu")) && $("#FirstMenu").val() == "0") {
                validateError++;
            }
            var url = "/CapitalCenterManagement/BusinessTypeSet/SaveBusiness?isEdit=";
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
        //改变一级节点值
        $('#FirstMenu').on('change', function (event) {
            if ($("#FirstMenu").val() == "1") {
                $("#SubjectCode").hide();//父级菜单
                $("#ParentMenu").val("");
                $("#hideParentMenu").val("");
            } else {
                $("#SubjectCode").show();
                $("#hideParentMenu").val(parentVguid);
                $("#ParentMenu").val(hideParentMenu);
            }
        })
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
        url: "/CapitalCenterManagement/BusinessTypeSet/GetBusiness",
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
                    { name: 'Code', type: 'string' },
                    { name: 'BusinessName', type: 'string' },
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
        width: selector.$grid().width(),
        showHeader: false,
        source: dataAdapter,
        checkboxes: true,
        ready: function () {
            $("#moduletree").jqxTreeGrid('expandAll');
        },
        columns: [
          { text: '业务名称', dataField: 'BusinessName', width: "100%", cellsRenderer: detailFuncs },
          { text: '', dataField: 'ParentVGUID', width: "100%", hidden: true },
          { text: '', dataField: 'Code', width: "100%", hidden: true },
          { text: '', dataField: 'VGUID', width: "100%", hidden: true },
        ]
    });
}
function detailFuncs(row, column, value, rowData) {
    var container = "";

    if (rowData.parent == null) {
        container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.Code + "','" + rowData.BusinessName + "','" + rowData.ParentVGUID + "','" + rowData.parent + "','" + rowData.parent + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Code + "-" + rowData.BusinessName + "</a>";
    } else {
        container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.Code + "','" + rowData.BusinessName + "','" + rowData.ParentVGUID + "','" + rowData.parent.Code + "','" + rowData.parent.BusinessName + "','" + rowData.parent.VGUID + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Code + "-" + rowData.BusinessName + "</a>";
    }
    return container;
}
function edit(guid, Code, BusinessName, Parent, parentCode, parentBusinessName, parentVGUID) {
    isEdit = true;
    vguid = guid;
    $("#ModuleCode").val(Code);
    $("#ModuleName").val(BusinessName);
    $("#myModalLabel_title").text("编辑数据");
    //$("#AddNewBankDataDialog table tr").eq(1).hide();
    $(".msg").remove();
    $("#ModuleName").removeClass("input_Validate");
    selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewBankDataDialog().modal("show");
    if (parentVguid == "null" || parentVguid == null) {
        $("#FirstMenu").val("1");
        $("#ParentMenu").val("");
        $("#hideParentMenu").val("");
        $("#SubjectCode").hide()
    } else {
        $("#FirstMenu").val("0");
        $("#ParentMenu").val(parentBusinessName);
        $("#hideParentMenu").val(parentVGUID);
        $("#SubjectCode").show();
    }
}


$(function () {
    var page = new $page();
    page.init();
});