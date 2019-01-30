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
                    parentVguid = checkrow[0].Vguid;
                    hideParentMenu = checkrow[0].ModuleName;
                    if ($("#FirstMenu").val() == "1") {
                        $("#SubjectCode").hide();//父级菜单
                        $("#FirstMenu").val("");
                    } else {
                        $("#SubjectCode").show();
                        $("#hideParentMenu").val(parentVguid);
                        $("#ParentMenu").val(hideParentMenu);
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
            var url = "/SystemManagement/ModuleManagement/SaveModules?isEdit=";
            if (validateError <= 0) {
                $.ajax({
                    url: url + isEdit,
                    data: {
                        "ModuleName": $("#ModuleName").val(),
                        "VGUID": vguid,
                        "Parent": $("#hideParentMenu").val()
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
                                jqxNotification("菜单已存在", null, "error");
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
            url: "/SystemManagement/ModuleManagement/DeleteModule",
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
        url: "/SystemManagement/ModuleManagement/GetModules",
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
                    { name: 'ModuleName', type: 'string' },
                    { name: 'Parent', type: 'string' },
                    { name: 'Vguid', type: 'string' }
                ],
                hierarchy:
                {
                    keyDataField: { name: 'Vguid' },
                    parentDataField: { name: 'Parent' }
                },
                id: 'Vguid',
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
          { text: '模块名称', dataField: 'ModuleName', width: "100%", cellsRenderer: detailFuncs },
          { text: '', dataField: 'Parent', width: "100%",hidden:true  },
          { text: '', dataField: 'Vguid', width: "100%", hidden: true },
        ]
    });
}
function detailFuncs(row, column, value, rowData) {
    var container = "";
   
    if (rowData.parent == null) {
        container = "<a href='#' onclick=edit('" + rowData.Vguid + "','" + rowData.ModuleName + "','" + rowData.Parent + "','" + rowData.parent + "','" + rowData.parent + "') style=\"text-decoration: underline;color: #333;\">" + rowData.ModuleName + "</a>";
    } else {
        container = "<a href='#' onclick=edit('" + rowData.Vguid + "','" + rowData.ModuleName + "','" + rowData.Parent + "','" + rowData.parent.ModuleName + "','" + rowData.parent.Vguid + "') style=\"text-decoration: underline;color: #333;\">" + rowData.ModuleName + "</a>";
    }
    return container;
}
function edit(guid, ModuleName, Parent, parentModuleName, parentVguid) {
    isEdit = true;
    vguid = guid;
    $("#ModuleName").val(ModuleName);
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
        $("#ParentMenu").val(parentModuleName);
        $("#hideParentMenu").val(parentVguid);
        $("#SubjectCode").show();
    }
}


$(function () {
    var page = new $page();
    page.init();
});