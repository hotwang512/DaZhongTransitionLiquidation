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
        initOrganization();


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
            var items = selector.$pushTree().jqxTree('getSelectedItem');
            if (!items) {
                jqxNotification("请选择部门！", null, "error");
                return;
            }
            selector.$departmentVguid().val(items.id);
            selector.$frmtable().ajaxSubmit({
                url: '/Systemmanagement/UserManagement/SaveUserInfo',
                type: "post",
                data: { isEdit: selector.$isEdit().val() },
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


$(function () {
    var page = new $page();
    page.init();

});


