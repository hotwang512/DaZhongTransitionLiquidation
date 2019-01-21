var selector = {
    $frmtable: function () { return $("#frmtable") },

    //按钮
    $btnSave: function () { return $("#btnSave") },
    $btnCancel: function () { return $("#btnCancel") },

    //界面元素
    $loginName_Input: function () { return $("#loginName_Input") },
    $userName_Input: function () { return $("#userName_Input") },
    $email_Input: function () { return $("#email_Input") },
    $mobilePhone_Input: function () { return $("#mobilePhone_Input") },
    $workPhone_Input: function () { return $("#workPhone_Input") },
    $role_Input: function () { return $("#role_Input") },
    $company_Input: function () { return $("#company_Input") },
    $department_Input: function () { return $("#department_Input") },
    $enable_Input: function () { return $("#enable_Input") },
    $remark_Input: function () { return $("#remark_Input") },
    $pushPeopleDropDownButton: function () { return $("#pushPeopleDropDownButton") },
    $pushTree: function () { return $("#pushTree") },
    $departmentVguid: function () { return $("#DepartmentVguid") },
    $personnelVguid: function () { return $("#PersonnelVguid") },
    $Vguid: function () { return $("#Vguid") },
    $currentUserDepartment: function () { return $("#currentUserDepartment") },
    $isEdit: function () { return $("#isEdit") }
};


var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        //加载部门下拉框
        //initOrganization();
        initTable1();
        initInput();

    }; //addEvent end
    //编辑界面加载页面上文本框的值
    function initInput() {
        $.ajax({
            url: "/SystemManagement/UserManagement/GetUserInfoByVguid",
            data: { Vguid: selector.$Vguid().val() },
            type: "post",
            dataType: "json",
            success: function (msg) {
                selector.$loginName_Input().val(msg.LoginName);
                selector.$enable_Input().val(msg.Enable);
                selector.$email_Input().val(msg.Email);
                selector.$mobilePhone_Input().val(msg.MobileNnumber);
                selector.$role_Input().val(msg.Role);
                selector.$remark_Input().val(msg.Remark);
                selector.$departmentVguid().val(msg.Department);
                var department = selector.$departmentVguid().val();
                if (department != "") {
                    selector.$pushTree().jqxTree('selectItem', $("#" + department)[0]);
                }
            }
        });
    }
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
                //加载文本框
                initInput();
            }
        });
    }

    //取消按钮事件
    selector.$btnCancel().on('click', function () {
        window.location.href = "/Systemmanagement/UserManagement/UserInfos";
    });

    //保存按钮事件
    selector.$btnSave().unbind('click').on('click', function () {
        var validateError = 0;//未通过验证的数量

        if (!Validate(selector.$loginName_Input())) {
            validateError++;
        }
        if (!Validate(selector.$mobilePhone_Input())) {
            validateError++;
        }
        if (!Validate(selector.$role_Input())) {
            validateError++;
        }
        if (validateError == 0) {
            //var items = selector.$pushTree().jqxTree('getSelectedItem');
            //if (!items) {
            //    jqxNotification("请选择部门！", null, "error");
            //    return;
            //}
            //selector.$departmentVguid().val(items.id);
            //选择的公司
            var selection = [];
            var pars = JSON.stringify($('#jqxTable1').jqxGrid('getdisplayrows')); 

            selector.$frmtable().ajaxSubmit({
                url: '/Systemmanagement/UserManagement/SaveUserInfo',
                type: "post",
                data: { isEdit: selector.$isEdit().val(), gjson: pars },
                dataType: "json",
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("保存失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("保存成功！", null, "success");
                            window.location.href = "/Systemmanagement/UserManagement/UserInfos";
                            break;
                        case "2":
                            jqxNotification("登录名称已存在，请重新输入！", null, "error");
                            break;
                    }
                }
            });
        }
    });
};

//公司段
function initTable1() {
    var source =
        {
            datafields:
            [
                { name: 'Code', type: 'string' },
                { name: 'Descrption', type: 'string' },
                { name: 'CompanyCode', type: 'string' },
                { name: 'CompanyName', type: 'string' },
                { name: 'IsCheck', type: 'bool' },
                { name: 'KeyData', type: 'string' },
                { name: 'Block', type: 'string' },
                { name: 'UserVGUID', type: 'string' },
            ],
            datatype: "json",
            id: "KeyData",
            data: { UserVGUID: $.request.queryString().Vguid },
            url: "/Systemmanagement/UserManagement/GetUserCompanyInfo"   //获取数据源的路径
        };
    //var typeAdapter = new $.jqx.dataAdapter(source, {
    //    downloadComplete: function (data) {
    //        source.totalrecords = data.TotalRows;
    //    }
    //});
    var typeAdapter = new $.jqx.dataAdapter(source);
    var countriesAdapter = [
                 { value: "出租", label: "出租" },
                 { value: "租赁", label: "租赁" },
                 ]
    //创建卡信息列表（主表）
    $("#jqxTable1").jqxGrid(
        {
            pageable: true,
            width: "98%",
            height: 350,
            pageSize: 10,
            //serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 30,
            editable: true,
            pagermode: 'simple',
            selectionmode:'none',
            columns: [
                //{ text: "", datafield: "checkbox", width: 40, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                {
                    text: '选择', datafield: "IsCheck", width: 60, align: 'center', cellsAlign: 'center', columntype: 'checkbox',
                },
                { text: '账套编码', datafield: 'Code', width: 150, align: 'center', cellsAlign: 'center',editable: false, },
                { text: '账套描述', datafield: 'Descrption', width: 250, align: 'center', cellsAlign: 'center', editable: false },
                { text: '公司编码', datafield: 'CompanyCode', width: 150, align: 'center', cellsAlign: 'center', editable: false },
                { text: '公司描述', datafield: 'CompanyName', width: 450, align: 'center', cellsAlign: 'center', editable: false },
                {
                    text: '版块', datafield: 'Block', align: 'center', cellsAlign: 'center', columntype: 'dropdownlist',
                    createeditor: function (row, value, editor) {
                        editor.jqxDropDownList({ source: countriesAdapter, displayMember: 'label', valueMember: 'value' });
                    }
                },
                { text: 'KeyData', datafield: 'SectionVGUID', hidden: true },
            ]
        });

}

function cellsRendererFunc(row, column, value, rowData) {
    return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
}

function rendererFunc() {
    var checkBox = "<div id='jqx_datatable_checkbox_all' class='jqx_datatable_checkbox_all' style='z-index: 999; margin-left:7px ;margin-top: 7px;'>";
    checkBox += "</div>";
    return checkBox;
}

function statusFunc(row, column, value, rowData) {
    var container = '<div style="color:#30dc32">启用</div>';
    if (value == "0") {
        container = '<div style="color:#F00">禁用</div>';
    }
    return container;
}

function renderedFunc(element) {
    var grid = $("#jqxTable1");
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

$(function () {
    var page = new $page();
    page.init();

});


