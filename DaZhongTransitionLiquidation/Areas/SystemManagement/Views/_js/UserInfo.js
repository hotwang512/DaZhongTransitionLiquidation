var selector = {
    $grid: function () { return $("#UserInfoList") },

    //按钮
    $btnSearch: function () { return $("#btnSearch") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnEnable: function () { return $("#btnEnable") },
    $btnDisable: function () { return $("#btnDisable") },
    $btnReset: function () { return $("#btnReset") },
    $btnChangePwd: function () { return $("#btnChangePwd") },
    //查询
    $txtLoginName: function () { return $("#txtLoginName") },
    $txtUserName: function () { return $("#txtUserName") },
    $drdRole: function () { return $("#drdRole") },
    $drdStatus: function () { return $("#drdStatus") },
    $pushPeopleDropDownButton: function () { return $("#pushPeopleDropDownButton") },
    $pushTree: function () { return $("#pushTree") },
    $EditPermission: function () { return $("#EditPermission") },
    $departmentVguid: function () { return $("#DepartmentVguid") },
    $currentUserDepartment: function () { return $("#currentUserDepartment") }
};


var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        initTable();
        //initOrganization();
    }; //addEvent end

    //查询按钮事件
    selector.$btnSearch().unbind('click').on('click', function () {
        initTable();
    });

    //新增按钮事件
    selector.$btnAdd().on('click', function () {
        window.location.href = "/Systemmanagement/UserManagement/UserInfoDetail?isEdit=false";
    });
    //状态下拉框改变事件
    selector.$drdStatus().on("change", function () {
        initTable();
    });
    //重置按钮事件
    selector.$btnReset().on('click', function () {
        selector.$txtLoginName().val("");
        selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', "");
        selector.$drdRole().val("");
        selector.$drdStatus().val("1");
        selector.$departmentVguid().val("");
    });
    //重置密码
    selector.$btnChangePwd().on('click', function () {
        var selection = [];
        var grid = selector.$grid();
        var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
        var userModel = function (pwd, vguid) {
            this.Password = pwd;
            this.Vguid = vguid;
        }
        checedBoxs.each(function () {
            var th = $(this);
            if (th.is(":checked")) {
                var index = th.attr("index");
                var data = grid.jqxDataTable('getRows')[index];
                var user = new userModel("123456", data.Vguid);
                selection.push(user);
            }
        });
        if (selection.length < 1) {
            jqxNotification("请选择您要重置的数据！", null, "error");
            return false;
        }
        $.ajax({
            url: "/Systemmanagement/UserManagement/ResetPassword",
            data: { userStr: JSON.stringify(selection) },
            traditional: true,
            type: "post",
            dataType: "json",
            success: function (msg) {
                if (msg.IsSuccess) {
                    jqxNotification("重置成功！", null, "success");
                    selector.$grid().jqxDataTable('updateBoundData');
                } else {
                    jqxNotification("重置失败！", null, "error");
                }
            }
        });

    });
    //删除按钮事件
    selector.$btnDelete().on('click', function () {
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
            WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消", selection);
        }
    });

    //启用按钮事件
    selector.$btnEnable().on('click', function () {
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
        })
        if (selection.length < 1) {
            jqxNotification("请选择您要启用的数据！", null, "error");
        } else {
            WindowConfirmDialog(enabled, "您确定要启用选中的数据？", "确认框", "确定", "取消");
        }
    });

    //禁用按钮事件
    selector.$btnDisable().on('click', function () {
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
        })
        if (selection.length < 1) {
            jqxNotification("请选择您要禁用的数据！", null, "error");
        } else {
            WindowConfirmDialog(disabled, "您确定要禁用选中的数据？", "确认框", "确定", "取消");
        }
    });

    //加载部门下拉框
    function initOrganization() {
        $.ajax({
            url: "/SystemManagement/UserManagement/GetOrganizationTree",
            type: "post",
            dataType: "json",
            success: function (msg) {
                //推送接收人下拉框
                selector.$pushPeopleDropDownButton().jqxDropDownButton({
                    width: 185,
                    height: 25
                });
                //推送接收人下拉框(树形结构)
                selector.$pushTree().on('select', function (event) {
                    var args = event.args;
                    var item = selector.$pushTree().jqxTree('getItem', args.element);

                    if (selector.$currentUserDepartment().val().indexOf(item.id) == -1) {
                        jqxNotification("请选择本公司及其子部门！", null, "error");
                        return false;
                    }
                    selector.$departmentVguid().val(item.id);
                    var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 5px;">' + item.label + '</div>';
                    selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', dropDownContent);
                });
                var source =
                        {
                            datatype: "json",
                            datafields: [
                                { name: 'OrganizationName' },
                                { name: 'ParentVguid' },
                                { name: 'Vguid' }
                            ],
                            id: 'Vguid',
                            localdata: msg
                        };
                var dataAdapter = new $.jqx.dataAdapter(source);
                // perform Data Binding.
                dataAdapter.dataBind();
                var records = dataAdapter.getRecordsHierarchy('Vguid', 'ParentVguid', 'items',
                    [
                        {
                            name: 'OrganizationName',
                            map: 'label'
                        },
                        {
                            name: 'Vguid',
                            map: 'id'
                        },
                        {
                            name: 'ParentVguid',
                            map: 'parentId'
                        }
                    ]);
                selector.$pushTree().jqxTree({ source: records, width: '207px', height: '250px', incrementalSearch: true });//, checkboxes: true
                selector.$pushTree().jqxTree('expandAll');
            }
        });
    }
    //加载表格
    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'LoginName', type: 'string' },
                    { name: 'UserName', type: 'string' },
                    { name: 'Company', type: 'string' },
                    { name: 'TranslationCompany', type: 'string' },
                    { name: 'Email', type: 'string' },
                    { name: 'WorkPhone ', type: 'string' },
                    { name: 'MobileNnumber', type: 'string' },
                    { name: 'Enable', type: 'string' },
                    { name: 'TranslationEnable', type: 'string' },
                    { name: 'Role', type: 'string' },
                    { name: 'TranslationRole', type: 'string' },
                    { name: 'Department', type: 'string' },
                    { name: 'TranslationDepartment', type: 'string' },
                    { name: 'CreatedUser', type: 'string' },
                    { name: 'CreatedDate', type: 'date' },
                    { name: 'Vguid', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",//主键
                async: true,
                data: {
                    "LoginName": selector.$txtLoginName().val(),
                    "Department": selector.$departmentVguid().val(),
                    "Enable": selector.$drdStatus().val(),
                    "Role": selector.$drdRole().val()
                },
                url: "/Systemmanagement/UserManagement/GetUserInfos"    //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid().jqxDataTable(
            {
                pageable: true,
                width: "100%",
                height: 440,
                pageSize: 10,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columns: [
                  { width: 35, text: "", datafield: "checkbox", align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                  { text: '登录名称', datafield: 'LoginName', width: '12%', align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                 // { text: '用户名称', datafield: 'UserName', width: '14%', align: 'center', cellsAlign: 'center' },
                  { text: '部门', datafield: 'TranslationDepartment', width: '12%', align: 'center', cellsAlign: 'center',hidden:true },
                  { text: '角色', datafield: 'TranslationRole', width: '12%', align: 'center', cellsAlign: 'center' },
                  { text: '邮箱', datafield: 'Email', align: 'center', width: '12%', cellsAlign: 'center' },
                  { text: '手机号码', datafield: 'MobileNnumber', width: '12%', align: 'center', cellsAlign: 'center' },
                  { text: '是否启用', datafield: 'TranslationEnable', width: '12%', align: 'center', cellsAlign: 'center' },
                  { text: '创建人', datafield: 'CreatedUser', width: '12%', align: 'center', cellsAlign: 'center' },
                  { text: '创建时间', datafield: 'CreatedDate', align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                  { text: 'Vguid', datafield: 'Vguid', hidden: true }
                ]
            });
    }
    function detailFunc(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "1") {
            container = "<a href='UserInfoDetail?Vguid=" + rowData.Vguid + "&isEdit=true' style=\"text-decoration: underline;color: #333;\">" + rowData.LoginName + "</a>";
        }
        else {
            container = "<span>" + rowData.LoginName + "</span>";
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
};

//删除
function dele(selection) {
    showLoading();//显示加载等待框
    //var selection = [];
    //var grid = selector.$grid();
    //var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
    //checedBoxs.each(function () {
    //    var th = $(this);
    //    if (th.is(":checked")) {
    //        var index = th.attr("index");
    //        var data = grid.jqxDataTable('getRows')[index];
    //        selection.push(data.Vguid);
    //    }
    //});
    $.ajax({
        url: "/Systemmanagement/UserManagement/DeleteUserInfos",
        data: { vguids: selection },
        traditional: true,
        type: "post",
        dataType: "json",
        success: function (msg) {
            switch (msg.Status) {
                case "0":
                    jqxNotification("删除失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("删除成功！", null, "success");
                    selector.$grid().jqxDataTable('updateBoundData');
                    break;
            }
            closeLoading(); //关闭加载等待框
        }
    });
}

//启用
function enabled() {
    showLoading();//显示加载等待框
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
        url: "/Systemmanagement/UserManagement/ChangeUserStatus",
        data: { vguids: selection, status: "1" },
        traditional: true,
        type: "post",
        dataType: "json",
        success: function (msg) {
            switch (msg.Status) {
                case "0":
                    jqxNotification("启用失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("启用成功！", null, "success");
                    selector.$grid().jqxDataTable('updateBoundData');
                    break;
            }
            closeLoading();//关闭加载等待框
        }
    });
}

//禁用
function disabled() {
    showLoading();//显示加载等待框
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
        url: "/Systemmanagement/UserManagement/ChangeUserStatus",
        data: { vguids: selection, status: "0" },
        traditional: true,
        type: "post",
        dataType: "json",
        success: function (msg) {
            switch (msg.Status) {
                case "0":
                    jqxNotification("禁用失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("禁用成功！", null, "success");
                    selector.$grid().jqxDataTable('updateBoundData');
                    break;
            }
            closeLoading();//关闭加载等待框
        }
    });
}


$(function () {
    var page = new $page();
    page.init();

});


