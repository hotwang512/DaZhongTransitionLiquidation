
//所有元素选择器
var selector = {

    //表格元素
    //$grid: function () { return $("#modulePermissionsList") },
    $grid: function () { return $("#moduletree") },
    $userGrid: function () { return $("#userGrid") },

    //按钮元素
    $btnSave: function () { return $("#btnSave") }, //保存
    $btnBack: function () { return $("#btnBack") }, //返回
    $addUserBtn: function () { return $("#addUserBtn") },
    $OKBtn: function () { return $("#OKBtn") },
    $CancelBtn: function () { return $("#CancelBtn") },
    $btnSearch: function () { return $("#btnSearch") },

    $userListDialog: function () { return $("#userListDialog") },
    $jqx_datatable_checkbox: function () { return $(".jqx_datatable_checkbox") },
    $selectedAllCheckbox: function () { return $(".selectedAllCheckbox") },//全选列的checkbox


    //表单元素
    $roleForm: function () { return $("#roleForm") },
    $isEdit: function () { return $("#isEdit") },
    $RoleName: function () { return $("#RoleName") },
    $Description: function () { return $("#Description") },
    $RoleVGUID: function () { return $("#RoleVGUID") },

}; //selector end

var dataTotalRows = 0;



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

        initInput();
        //加载列表数据
        //loadTable();

        //返回
        selector.$btnBack().on('click', function () {
            window.location.href = "/Systemmanagement/AuthorityManagement/AuthorityInfo";
        });

        //保存
        selector.$btnSave().unbind('click').on('click', function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate(selector.$RoleName())) {
                validateError++;
            }
            if (validateError <= 0) {
                var rolePermissionMode = function (moduleName, rightType) {
                    this.ModuleName = moduleName;
                    this.RightType = rightType;
                }
                var rolePermissionArray = new Array();

                $('.permission').each(function () {
                    if ($(this).is(":checked")) {
                        var pageid = $(this).attr("pageid");
                        var buttonid = $(this).attr("buttonid");
                        var rolePermission = new rolePermissionMode(pageid, buttonid);
                        rolePermissionArray.push(rolePermission);
                    }
                });
                console.log(JSON.stringify(rolePermissionArray));
                selector.$roleForm().ajaxSubmit({
                    url: '/Systemmanagement/AuthorityManagement/SaveRole?isEdit=' + selector.$isEdit().val(),
                    type: "post",
                    data: { permissionList: JSON.stringify(rolePermissionArray) },
                    dataType: "json",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                window.location.href = "/Systemmanagement/AuthorityManagement/AuthorityInfo";
                                break;
                            case "2":
                                jqxNotification("角色名称已经存在！", null, "error");
                                break;
                        }
                    }
                });
            }
        });

    }; //addEvent end

    function getModules(callback) {
        $.ajax({
            url: "/SystemManagement/AuthorityManagement/GetModules",
            type: "get",
            dataType: "json",
            data: { "roleVguid": selector.$RoleVGUID().val() },
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
                    { name: 'Vguid', type: 'string' },
                    { name: 'Reads', type: 'string' },
                    { name: 'Adds', type: 'string' },
                    { name: 'Edit', type: 'string' },
                    { name: 'Deletes', type: 'string' },
                    { name: 'Enable', type: 'string' },
                    { name: 'Disable', type: 'string' },
                    { name: 'Import', type: 'string' },
                    { name: 'Export', type: 'string' },
                    { name: 'Vguid', type: 'string' },
                    { name: 'ModuleVGUID', type: 'string' }
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
            showHeader: true,
            source: dataAdapter,
            checkboxes: false,
            ready: function () {
                $("#moduletree").jqxTreeGrid('expandAll');
            },
            columns: [
              { text: '模块名称', dataField: 'ModuleName', width: 200, },
              //{ text: '查看', dataField: 'Reads', width: 100, },
              { text: '查看', datafield: 'Reads', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Reads },
                  { text: '新增', datafield: 'Adds', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Adds },
                  { text: '编辑', datafield: 'Edit', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Edit },
                  { text: '删除', datafield: 'Deletes', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Deletes },
                  { text: '启用', datafield: 'Enable', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Submit },
                  { text: '禁用', datafield: 'Disable', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Approved },
                  { text: '导入', datafield: 'Import', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Import },
                  { text: '导出', datafield: 'Export', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Export },
                  { text: '全选', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, rendered: renderedFunc },
                  { text: 'PageID', datafield: 'PageID', hidden: true },
                  { text: 'Vguid', datafield: 'Vguid', hidden: true },
                  { text: 'ModuleVGUID', datafield: 'ModuleVGUID', hidden: true }
              //{ text: '', dataField: 'Parent', width: "100%", hidden: true },
              //{ text: '', dataField: 'Vguid', width: "100%", hidden: true },
            ]
        });
    }

    //加载页面内容
    function initInput() {
        $.ajax({
            url: "/SystemManagement/AuthorityManagement/GetRoleInfoByVguid",
            data: { vguid: selector.$RoleVGUID().val() },
            type: "post",
            dataType: "json",
            success: function (msg) {
                selector.$RoleName().val(msg.Role);
                selector.$Description().val(msg.Description);
            }
        });
    }
    //加载权限列表

    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"selectedAllCheckbox\" id=\"" + row + "\" index=\"" + row + "\" type=\"checkbox\" onclick=\"selectAll('" + row + "')\" style=\"margin:auto;width: 17px;height: 17px;\" />";
    }

    function renderedFunc(element) {
        var grid = selector.$grid();
        var rows = grid.jqxTreeGrid('getRows');
        for (var i = 0; i < rows.length; i++) {

        }

        //pageID
        //var grid = selector.$grid();
        //element.jqxCheckBox();
        //element.on('change', function (event) {
        //    var checked = element.jqxCheckBox('checked');

        //    if (checked) {
        //        var rows = grid.jqxDataTable('getRows');
        //        for (var i = 0; i < rows.length; i++) {
        //            grid.jqxDataTable('selectRow', i);
        //            grid.find(".jqx_datatable_checkbox").attr("checked", "checked")
        //        }
        //    } else {
        //        grid.jqxDataTable('clearSelection');
        //        grid.find(".jqx_datatable_checkbox").removeAttr("checked", "checked")
        //    }
        //});
        return true;
    }

    function cellsRendererFunc_Reads(row, column, value, rowData) {
        if (rowData.Reads == "0") {
            return "";
        }
        else if (rowData.Reads == "1") {
            return "<div><input type=\"checkbox\" class=\"permission\"  style=\"width: 17px;height: 17px;\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"1\" /></div>";
        }
        else if (rowData.Reads == "2") {
            return "<input type=\"checkbox\" class=\"permission\"  style=\"width: 17px;height: 17px;\" checked=\"checked\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"1\" />";
        }
    }

    function cellsRendererFunc_Adds(row, column, value, rowData) {
        if (rowData.Adds == "0") {
            return "";
        }
        else if (rowData.Adds == "1") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"2\" />";
        }
        else if (rowData.Adds == "2") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" checked=\"checked\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"2\" />";
        }
    }

    function cellsRendererFunc_Edit(row, column, value, rowData) {
        if (rowData.Edit == "0") {
            return "";
        }
        else if (rowData.Edit == "1") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"3\" />";
        }
        else if (rowData.Edit == "2") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" checked=\"checked\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"3\" />";
        }
    }

    function cellsRendererFunc_Deletes(row, column, value, rowData) {
        if (rowData.Deletes == "0") {
            return "";
        }
        else if (rowData.Deletes == "1") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"4\" />";
        }
        else if (rowData.Deletes == "2") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" checked=\"checked\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"4\" />";
        }
    }

    function cellsRendererFunc_Submit(row, column, value, rowData) {
        if (rowData.Enable == "0") {
            return "";
        }
        else if (rowData.Enable == "1") {
            return "<input type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"5\" />";
        }
        else if (rowData.Enable == "2") {
            return "<input type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" checked=\"checked\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"5\" />";
        }
    }

    function cellsRendererFunc_Approved(row, column, value, rowData) {
        if (rowData.Disable == "0") {
            return "";
        }
        else if (rowData.Disable == "1") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"6\" />";
        }
        else if (rowData.Disable == "2") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" checked=\"checked\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"6\" />";
        }
    }

    function cellsRendererFunc_Import(row, column, value, rowData) {
        if (rowData.Import == "0") {
            return "";
        }
        else if (rowData.Import == "1") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"7\" />";
        }
        else if (rowData.Import == "2") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" checked=\"checked\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"7\" />";
        }
    }

    function cellsRendererFunc_Export(row, column, value, rowData) {
        if (rowData.Export == "0") {
            return "";
        }
        else if (rowData.Export == "1") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Export == "2") {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" checked=\"checked\" pageid=\"" + rowData.ModuleVGUID + "\" buttonid=\"8\" />";
        }
    }
};

//全选checkbox点击事件
function selectAll(id) {
    //对于HTML元素本身就带有的固有属性，在处理时，使用prop方法。
    //对于HTML元素我们自己自定义的DOM属性，在处理时，使用attr方法。
    if ($("#" + id + "").is(":checked")) {
        $("#" + id + "").parent("td").parent("tr").find(".permission").prop("checked", "checked");
    } else {
        $("#" + id + "").parent("td").parent("tr").find(".permission").prop("checked", false);
    }
}

$(function () {

    var page = new $page();
    page.init();
});


function loadTable() {
    var centerSetUpSource = {
        datafields:
        [
            { name: 'ParentID', type: 'string' },
            { name: 'PageID', type: 'string' },
            { name: 'PageName', type: 'string' },
            { name: 'Reads', type: 'string' },
            { name: 'Adds', type: 'string' },
            { name: 'Edit', type: 'string' },
            { name: 'Deletes', type: 'string' },
            { name: 'Enable', type: 'string' },
            { name: 'Disable', type: 'string' },
            { name: 'Import', type: 'string' },
            { name: 'Export', type: 'string' },
            { name: 'Vguid', type: 'string' },
            { name: 'ModuleVGUID', type: 'string' }
        ],
        datatype: "json",
        id: "Vguid",
        async: true,
        data: { "roleVguid": selector.$RoleVGUID().val() },
        url: "/Systemmanagement/AuthorityManagement/GetModulePermissions"   //获取数据源的路径
    };
    var typeAdapter = new $.jqx.dataAdapter(centerSetUpSource, {
        downloadComplete: function (data) {
            centerSetUpSource.totalrecords = data.TotalRows;
        }
    });
    //创建卡信息列表（主表）
    selector.$grid().jqxDataTable({
        pageable: false,
        width: "100%",
        height: 500,
        pageSize: 10,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        columnsHeight: 40,
        source: typeAdapter,
        theme: "office",
        groups: ['ParentID'],
        groupsRenderer: function (value, rowData, level) {
            return "  " + value + "模块";
        },
        columns: [
          { text: '模块', datafield: 'ParentID', hidden: true, align: 'center', cellsAlign: 'center' },
          { text: '页面', datafield: 'PageName', align: 'center', cellsAlign: 'center' },
          { text: '查看', datafield: 'Reads', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Reads },
          { text: '新增', datafield: 'Adds', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Adds },
          { text: '编辑', datafield: 'Edit', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Edit },
          { text: '删除', datafield: 'Deletes', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Deletes },
          { text: '启用', datafield: 'Enable', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Submit },
          { text: '禁用', datafield: 'Disable', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Approved },
          { text: '导入', datafield: 'Import', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Import },
          { text: '导出', datafield: 'Export', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Export },
          { text: '全选', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, rendered: renderedFunc },
          { text: 'PageID', datafield: 'PageID', hidden: true },
          { text: 'Vguid', datafield: 'Vguid', hidden: true },
          { text: 'ModuleVGUID', datafield: 'ModuleVGUID', hidden: true }
        ]
    });
}