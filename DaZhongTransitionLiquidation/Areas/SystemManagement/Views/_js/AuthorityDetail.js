$(".input_text").attr("autocomplete", "new-password");
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
                var rolePermissionArray = new Array();
                $('.permission').each(function () {
                    if ($(this).is(":checked")) {
                        var modulVGUID = $(this).attr("pageid");
                        var name = $(this).attr("name");
                        var look = false; var review = false; 
                        var news = false; var goBack = false;
                        var edit = false; var imports = false;
                        var strikeOut = false; var exports = false;
                        var obsolete = false; var generate = false;
                        var submit = false; var calculation = false;
                        var preview = false; var enable = false;
                        var comorman = "2";
                        if (name.length > 40) {
                            name = name.substring(0, 8);
                        }
                        switch (name) {
                            case "Look": look = true; break; case "Review": review = true; break;
                            case "New": news = true; break; case "GoBack": goBack = true; break;
                            case "Edit": edit = true; break; case "Import": imports = true; break;
                            case "StrikeOut": strikeOut = true; break; case "Export": exports = true; break;
                            case "Obsolete": obsolete = true; break; case "Generate": generate = true; break;
                            case "Submit": submit = true; break; case "Calculation": calculation = true; break;
                            case "Preview": preview = true; break; case "Enable": enable = true; break;
                            case "ComOrMan": comorman = $("input[name='ComOrMan" + modulVGUID + "']:checked").val(); break;
                            default:
                        }
                        var rolePermissionMode = function () {
                            this.ModuleMenuVGUD = modulVGUID;
                            this.Look = look; this.Review = review;
                            this.New = news; this.GoBack = goBack;
                            this.Edit = edit; this.Import = imports;
                            this.StrikeOut = strikeOut; this.Export = exports;
                            this.Obsolete = obsolete; this.Generate = generate;
                            this.Submit = submit; this.Calculation = calculation;
                            this.Preview = preview; this.Enable = enable;
                            this.ComOrMan = comorman;
                        };
                        var rolePermission = new rolePermissionMode();
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
            url: "/SystemManagement/AuthorityManagement/GetNewModules",
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
                    { name: 'Name', type: 'string' },
                    { name: 'KeyVGUID', type: 'string' },
                    { name: 'Parent', type: 'string' },
                    { name: 'IsLook', type: 'bool' },
                    { name: 'IsNew', type: 'bool' },
                    { name: 'IsEdit', type: 'bool' },
                    { name: 'IsStrikeOut', type: 'bool' },
                    { name: 'IsObsolete', type: 'bool' },
                    { name: 'IsSubmit', type: 'bool' },
                    { name: 'IsReview', type: 'bool' },
                    { name: 'IsGoBack', type: 'bool' },
                    { name: 'IsImport', type: 'bool' },
                    { name: 'IsExport', type: 'bool' },
                    { name: 'IsGenerate', type: 'bool' },
                    { name: 'IsCalculation', type: 'bool' },
                    { name: 'IsPreview', type: 'bool' },
                    { name: 'IsEnable', type: 'bool' },
                    { name: 'Look', type: 'bool' },
                    { name: 'New', type: 'bool' },
                    { name: 'Edit', type: 'bool' },
                    { name: 'StrikeOut', type: 'bool' },
                    { name: 'Obsolete', type: 'bool' },
                    { name: 'Submit', type: 'bool' },
                    { name: 'Review', type: 'bool' },
                    { name: 'GoBack', type: 'bool' },
                    { name: 'Import', type: 'bool' },
                    { name: 'Export', type: 'bool' },
                    { name: 'Generate', type: 'bool' },
                    { name: 'Calculation', type: 'bool' },
                    { name: 'Preview', type: 'bool' },
                    { name: 'Enable', type: 'bool' },
                    { name: 'ComOrMan', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'RoleVGUID', type: 'string' },
                    { name: 'ModuleMenuVGUD', type: 'string' },
                    ],
                    hierarchy:
                    {
                        keyDataField: { name: 'KeyVGUID' },
                        parentDataField: { name: 'Parent' }
                    },
                    id: '',
                    localData: modules
                };
        var dataAdapter = new $.jqx.dataAdapter(source);
        selector.$grid().jqxTreeGrid({
            width: "100%",
            height: 400,
            altRows: true,
            showHeader: true,
            source: dataAdapter,
            checkboxes: false,
            editable: false,
            //selectionMode: "singleCell",
            editSettings:{ saveOnBlur: false, saveOnSelectionChange: false, editOnDoubleClick: true, },
            ready: function () {
                $("#moduletree").jqxTreeGrid('expandAll');
            },
            columns: [
                    { text: '模块名称', dataField: 'Name', editable: false, width: 250, },
                    { text: '查看', dataField: 'Look', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Look },
                    { text: '新增', dataField: 'New', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_New },
                    { text: '编辑', dataField: 'Edit', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Edit },
                    { text: '删除', dataField: 'StrikeOut', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_StrikeOut },
                    { text: '作废', dataField: 'Obsolete', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Obsolete },
                    { text: '提交', dataField: 'Submit', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Submit },
                    { text: '审核', dataField: 'Review', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Review },
                    { text: '预览/打印', dataField: 'Preview', width: 100, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Preview },
                    { text: '启用', dataField: 'Enable', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Enable },
                    { text: '退回', dataField: 'GoBack', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_GoBack },
                    { text: '导入', dataField: 'Import', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Import },
                    { text: '导出', dataField: 'Export', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Export },
                    { text: '生成/同步/校验', dataField: 'Generate', width: 120, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Generate },
                    { text: '计算', dataField: 'Calculation', width: 80, align: 'center', editable: false, cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Calculation },
                    {
                        text: '数据权限', dataField: 'ComOrMan', width: 200, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_ComOrMan
                        //createEditor: function (rowKey, cellvalue, editor, cellText, width, height) {
                        //    var dropDownList = $("<div class='dropDownList' style='border: none;'></div>").appendTo(editor);
                        //    dropDownList.jqxDropDownList({width: '100%', height: '100%', autoDropDownHeight: true, source: ["个人", "公司"] });
                        //},
                        //initEditor: function (rowKey, cellvalue, editor, celltext, width, height) {
                        //    $(editor.find('.dropDownList')).val(cellvalue);
                        //},
                        //// returns the value of the custom editor.
                        //getEditorValue: function (rowKey, cellvalue, editor) {
                        //    return $(editor.find('.dropDownList')).val();
                        //}
                    },
                    //cellsRenderer: cellsRendererFunc_ComOrMan
                  //{ text: '查看', datafield: 'Reads', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Reads },
                  //{ text: '新增', datafield: 'Adds', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Adds },
                  //{ text: '编辑', datafield: 'Edit', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Edit },
                  //{ text: '删除', datafield: 'Deletes', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Deletes },
                  //{ text: '启用', datafield: 'Enable', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Submit },
                  //{ text: '禁用', datafield: 'Disable', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Approved },
                  //{ text: '导入', datafield: 'Import', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Import },
                  //{ text: '导出', datafield: 'Export', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc_Export },
                  //{ text: '全选', align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, rendered: renderedFunc },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                    { text: 'RoleVGUID', datafield: 'RoleVGUID', hidden: true },
                    { text: 'ModuleMenuVGUD', datafield: 'ModuleMenuVGUD', hidden: true }
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

    function cellsRendererFunc_Look(row, column, value, rowData) {
        if (rowData.IsLook == false) {
            return "";
        }
        else if (rowData.Look == false) {
            return "<input   type=\"checkbox\" class=\"permission\" style=\"margin:auto;width: 17px;height: 17px;\" name=\"Look\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Look == true) {
            return "<input   type=\"checkbox\" class=\"permission\" style=\"margin:auto;width: 17px;height: 17px;\" name=\"Look\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_New(row, column, value, rowData) {
        if (rowData.IsNew == false) {
            return "";
        }
        else if (rowData.New == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"New\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.New == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"New\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_Edit(row, column, value, rowData) {
        if (rowData.IsEdit == false) {
            return "";
        }
        else if (rowData.Edit == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Edit\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Edit == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Edit\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_StrikeOut(row, column, value, rowData) {
        if (rowData.IsStrikeOut == false) {
            return "";
        }
        else if (rowData.StrikeOut == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"StrikeOut\"  pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.StrikeOut == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"StrikeOut\"  checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_Obsolete(row, column, value, rowData) {
        if (rowData.IsObsolete == false) {
            return "";
        }
        else if (rowData.Obsolete == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Obsolete\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Obsolete == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Obsolete\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_Submit(row, column, value, rowData) {
        if (rowData.IsSubmit == false) {
            return "";
        }
        else if (rowData.Submit == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Submit\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Submit == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Submit\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_Review(row, column, value, rowData) {
        if (rowData.IsReview == false) {
            return "";
        }
        else if (rowData.Review == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Review\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Review == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Review\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_Preview(row, column, value, rowData) {
        if (rowData.IsPreview == false || rowData.IsEnable == undefined || rowData.IsEnable == null) {
            return "";
        }
        else if (rowData.Preview == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Preview\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Preview == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Preview\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_Enable(row, column, value, rowData) {
        if (rowData.IsEnable == false || rowData.IsEnable == undefined || rowData.IsEnable == null) {
            return "";
        }
        else if (rowData.Enable == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Enable\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Enable == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Enable\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_GoBack(row, column, value, rowData) {
        if (rowData.IsGoBack == false) {
            return "";
        }
        else if (rowData.GoBack == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"GoBack\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.GoBack == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"GoBack\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_Import(row, column, value, rowData) {
        if (rowData.IsImport == false) {
            return "";
        }
        else if (rowData.Import == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Import\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Import == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Import\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_Export(row, column, value, rowData) {
        if (rowData.IsExport == false) {
            return "";
        }
        else if (rowData.Export == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Export\"  pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Export == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Export\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_Generate(row, column, value, rowData) {
        if (rowData.IsGenerate == false) {
            return "";
        }
        else if (rowData.Generate == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Generate\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Generate == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Generate\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_Calculation(row, column, value, rowData) {
        if (rowData.IsCalculation == false) {
            return "";
        }
        else if (rowData.Calculation == false) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Calculation\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
        else if (rowData.Calculation == true) {
            return "<input   type=\"checkbox\" class=\"permission\"  style=\"margin:auto;width: 17px;height: 17px;\" name=\"Calculation\" checked=\"checked\" pageid=\"" + rowData.KeyVGUID + "\" buttonid=\"8\" />";
        }
    }

    function cellsRendererFunc_ComOrMan(row, column, value, rowData) {
        if (value == "1" || value == null || value == "") {
            return com = "<input type=\"radio\" name=\"ComOrMan" + rowData.KeyVGUID + "\" class=\"permission\"  style=\"margin:auto;width: 20px;height: 16px;\" pageid=\"" + rowData.KeyVGUID + "\" value=\"1\" checked>:个人&nbsp;" +
                    "<input type=\"radio\" class=\"permission\" name=\"ComOrMan" + rowData.KeyVGUID + "\" style=\"margin:auto;width: 20px;height: 16px;\" pageid=\"" + rowData.KeyVGUID + "\" value=\"2\" >:公司";
        } else {
            return com = "<input type=\"radio\" name=\"ComOrMan" + rowData.KeyVGUID + "\" class=\"permission\"  style=\"margin:auto;width: 20px;height: 16px;\" pageid=\"" + rowData.KeyVGUID + "\" value=\"1\" >:个人&nbsp;" +
                    "<input type=\"radio\" class=\"permission\" name=\"ComOrMan" + rowData.KeyVGUID + "\"  style=\"margin:auto;width: 20px;height: 16px;\" pageid=\"" + rowData.KeyVGUID + "\" value=\"2\" checked>:公司";
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