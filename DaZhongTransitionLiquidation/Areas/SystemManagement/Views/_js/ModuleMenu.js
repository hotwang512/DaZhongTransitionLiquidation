var selector = this.selector = {
    $AddNewBankDataDialog: function () { return $("#AddNewBankDataDialog") },
};
var isEdit = "";
var vguid = "";
var $page = function () {

    this.init = function () {
        addEvent();
    };

    function addEvent() {
        loadTreeGrid();

        //新增
        $("#btnAdd").click(function () {
            var row = $('#moduletree').treegrid('getSelected');
            if (row == null) {
                jqxNotification("请选择一个节点！", null, "error");
                return;
            } else {
                parentVguid = row.VGUID;
                hideParentMenu = row.Name;
                if ($("#FirstMenu").val() == "1") {
                    $("#SubjectCode").hide();//父级菜单
                    $("#FirstMenu").val("");
                } else {
                    $("#SubjectCode").show();
                    $("#SubjectShow").show();
                    $("#hideParentMenu").val(parentVguid);
                    $("#ParentMenu").val(hideParentMenu);
                }
            }
            $("#ModuleName").val("");
            $("#Url").val("");
            $("#Zorder").val("");
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
        //删除
        $("#btnDelete").click(function () {
            var row = $('#moduletree').treegrid('getSelected');
            if (row == null) {
                jqxNotification("请选择您要删除的数据！", null, "error");
                return;
            } else {
                WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消", row.VGUID);
            }
        });
        //保存
        $('#OKButton').click(function () {
            var url = "/SystemManagement/ModuleMenu/SaveModules?isEdit=";
            $.ajax({
                url: url + isEdit,
                data: {
                    "Name": $("#ModuleName").val(),
                    "Type": $("#Type").val(),
                    "Url": $("#Url").val(),
                    "Zorder": $("#Zorder").val(),
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
                            $('#moduletree').treegrid('reload');
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
        });
        //取消
        $('#CancelBtn').click(function () {
            selector.$AddNewBankDataDialog().modal("hide");
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
};

$(function () {
    var page = new $page();
    page.init();
});

function loadTreeGrid() {
    $('#moduletree').treegrid({
        title: '',
        iconCls: '',
        width: '100%',
        height: 600,
        animate: true,
        collapsible: true,
        fitColumns: true,
        method: 'post',
        showFooter: true,
        url: '/Systemmanagement/ModuleMenu/GetModuleMenu',
        rownumbers: true,
        idField: 'VGUID',
        treeField: 'Name',
        onLoadSuccess: function (row) {
            $(this).treegrid('enableDnd', row ? row.id : null);
        },
        onDblClickRow: function (row) {
            if (row) {
                isEdit = true;
                vguid = row.VGUID;
                $('#ModuleName').val(row.Name);
                $('#Type').val(row.Type);
                $('#Url').val(row.Url);
                $('#Zorder').val(row.Zorder);
                selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
                selector.$AddNewBankDataDialog().modal("show");
                $("#SubjectCode").hide();//父级菜单
                $("#SubjectShow").hide();
                $("#ParentMenu").val("");
                $("#hideParentMenu").val(row.Parent);
            }
        },
        onClickCell: function (field, row) {
            if (field != "Name" && field != "Url" && field != "Type") {
                CheckBoxClick(row.VGUID, field);
            }
        },
        columns: [[
            { title: '菜单名', field: 'Name', width: 200, editor: 'text' },
            { title: 'URL', field: 'Url', width: 300, align: 'left', editor: 'text' },
            { title: '类型', field: 'Type', width: 100, align: 'center', editor: 'text', formatter: formatProgress },
            { title: '查看', field: 'Look', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '新增', field: 'New', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '编辑', field: 'Edit', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '删除', field: 'StrikeOut', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '作废', field: 'Obsolete', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '提交', field: 'Submit', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '审核', field: 'Review', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '预览', field: 'Preview', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '启用', field: 'Enable', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '退回', field: 'GoBack', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '导入', field: 'Import', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '导出', field: 'Export', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '生成', field: 'Generate', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
            { title: '计算', field: 'Calculation', width: 50, align: 'center', editor: 'text', formatter: formatCheckbox },
        ]]
    });

    function formatProgress(value) {
        switch (value) {
            case 0: var s = '<div style="width:100%;"><div style="width:300;text-align: center;">文件夹</div></div>';
                return s;
                break;
            case 1: var s = '<div style="width:100%;"><div style="width:300;text-align: center;">项目</div></div>';
                return s;
                break;
            case 2: var s = '<div style="width:100%;"><div style="width:300;text-align: center;">隐藏</div></div>';
                return s;
                break;
            default:

        }
    }
    function formatCheckbox(value, row) {
        if (value == false) {
            return "<input type=\"checkbox\" class=\"permission\" style=\"margin:auto;width: 17px;height: 17px;margin-top: 4px;\" buttonid=\"2\" />";
        }
        else if (value == true) {
            return "<input type=\"checkbox\" class=\"permission\" style=\"margin:auto;width: 17px;height: 17px;margin-top: 4px;\" checked=\"checked\"  buttonid=\"2\" />";
        }
    }
}
//删除
function dele(selection) {
    $.ajax({
        url: "/SystemManagement/ModuleMenu/DeleteModule",
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
                    $('#moduletree').treegrid('reload');
                    break;
            }
        }
    });
}

function CheckBoxClick(VGUID, Field) {
    $.ajax({
        url: "/Systemmanagement/ModuleMenu/UpdataCheckBox",
        data: { vguid: VGUID, field: Field },
        type: "post",
        success: function (msg) {

        }
    });
}

function onDragEndEvent(row) {
    $.ajax({
        url: "/SystemManagement/ModuleMenu/MoveMenu",
        data: {
            "VGUID": row.VGUID,
            "Parent": row._parentId
        },
        type: "post",
        dataType: "json",
        success: function (msg) {
            switch (msg.Status) {
                case "0":
                    jqxNotification("移动失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("移动成功！", null, "success");
                    $('#moduletree').treegrid('reload');
                    break;
                case "2":
                    break;
            }
        }
    });
}