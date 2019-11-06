//所有元素选择器
var selector = {
    $grid: function () { return $("#roleList") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $AddBankChannelDialog: function () { return $("#AddBankChannelDialog") },
    $AddBankChannel_OKButton: function () { return $("#AddBankChannel_OKButton") },
    $AddBankChannel_CancelBtn: function () { return $("#AddBankChannel_CancelBtn") },
    $RoleName_Search: function () { return $("#RoleName_Search") },

    $EditPermission: function () { return $("#EditPermission") }
}; //selector end


var $page = function () {

    this.init = function () {
        addEvent();
    }




    //所有事件
    function addEvent() {

        //加载列表数据
        initTable();

        //
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            selector.$RoleName_Search().val("");
        });

        //新增
        selector.$btnAdd().on("click", function () {
            window.location.href = "/Systemmanagement/AuthorityManagement/AuthorityDetail?isEdit=false";
            //$("#RoleName").val("");
            //$("#Description").val("");
            //selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
            //selector.$AddBankChannelDialog().modal("show");
        });
        //弹出框中的取消按钮
        selector.$AddBankChannel_CancelBtn().on("click", function () {
            selector.$AddBankChannelDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddBankChannel_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate($("#RoleName"))) {
                validateError++;
            }
            if (validateError <= 0) {
                $.ajax({
                    url: "/SystemManagement/AuthorityManagement/SaveRoleInfo",
                    data: {
                        Role: $("#RoleName").val(),
                        Description: $("#Description").val(),
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
                                selector.$grid().jqxDataTable('updateBoundData');
                                selector.$AddBankChannelDialog().modal("hide");
                                break;
                            case "2":
                                jqxNotification("角色名称已经存在！", null, "error");
                                break;
                        }

                    }
                });
            }
        });
        //删除
        selector.$btnDelete().on("click", function () {
            var selection = [];
            var grid = selector.$grid();
            var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
            checedBoxs.each(function () {
                var th = $(this);
                if (th.is(":checked")) {
                    var index = th.attr("index");
                    var data = grid.jqxDataTable('getRows')[index];
                    selection.push(data.Vguid);
                }
            });
            if (selection.length < 1) {
                jqxNotification("请选择您要删除的数据！", null, "error");
            } else {
                WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消");
            }
        });

    }; //addEvent end


    function initTable() {
        var roleTypeSource =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Role', type: 'string' },
                    { name: 'Description', type: 'string' },
                    { name: 'Vguid', type: 'string' }
                ],
                datatype: "json",
                id: "Vguid",
                async: true,
                data: { "Role": selector.$RoleName_Search().val() },
                url: "/Systemmanagement/AuthorityManagement/GetRoleInfos"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(roleTypeSource, {
            downloadComplete: function (data) {
                roleTypeSource.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid().jqxDataTable(
            {
                pageable: true,
                width: "100%",
                height: 400,
                pageSize: 10,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                columns: [
                  { width: 35, text: "", datafield: "checkbox", align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                  { text: '角色名称', datafield: 'Role', width: '20%', align: 'center', cellsAlign: 'center', cellsRenderer: roleDetailFunc },
                  { text: '描述', datafield: 'Description', align: 'center', cellsAlign: 'center' },
                   { text: 'Vguid', datafield: 'Vguid', hidden: true }
                ]
            });

    }

    function roleDetailFunc(row, column, value, rowData) {
        var container = container = "<a href='AuthorityDetail?Vguid=" + rowData.Vguid + "&isEdit=true' style=\"text-decoration: underline;color: #333;\">" + rowData.Role + "</a>";
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


    //删除
    function dele() {
        var selection = [];
        var grid = selector.$grid();
        var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
        checedBoxs.each(function () {
            var th = $(this);
            if (th.is(":checked")) {
                var index = th.attr("index");
                var data = grid.jqxDataTable('getRows')[index];
                selection.push(data.Vguid);
            }
        });
        $.ajax({
            url: "/Systemmanagement/AuthorityManagement/DeleteRoleInfos",
            data: { vguids: selection },
            traditional: true,
            type: "post",
            success: function (msg) {
                if (msg.IsSuccess) {
                    jqxNotification("删除成功！", null, "success");
                    selector.$grid().jqxDataTable('updateBoundData');
                } else {
                    jqxNotification("删除失败！", null, "error");
                }
            }
        });
    }
};

$(function () {
    var page = new $page();
    page.init();
});


