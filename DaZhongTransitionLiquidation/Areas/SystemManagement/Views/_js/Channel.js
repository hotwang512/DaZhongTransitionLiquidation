$(".input_text").attr("autocomplete", "new-password");
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },

    $AddNewChannelDialog: function () { return $("#AddNewChannelDialog") },
    $AddNewChannel_OKButton: function () { return $("#AddNewChannel_OKButton") },
    $AddNewChannel_CancelBtn: function () { return $("#AddNewChannel_CancelBtn") },
    $txtChannelID_Dialog: function () { return $("#txtChannelID_Dialog") },
    $txtChannelName: function () { return $("#txtChannelName") },
    $txtChannelName_Dialog: function () { return $("#txtChannelName_Dialog") },
    $txtOffset_Dialog: function () { return $("#txtOffset_Dialog") },
    $txtDirectBankEnterprise_Dialog: function () { return $("#txtDirectBankEnterprise_Dialog") },
    $txtContactBank_Dialog: function () { return $("#txtContactBank_Dialog") },
    $txtSupplierName_Dialog: function () { return $("#txtSupplierName_Dialog") },
    $txtMerchantsCode_Dialog: function () { return $("#txtMerchantsCode_Dialog") },
    $pushPeopleDropDownButton: function () { return $("#pushPeopleDropDownButton") },
    $pushTree: function () { return $("#pushTree") },
    $DepartmentVguid: function () { return $("#DepartmentVguid") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end

var isEdit = false;
var vguid = "";
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {

        //加载列表数据
        initTable();

        initOrganization();

        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            selector.$txtChannelName().val("");
        });

        //新增
        selector.$btnAdd().on("click", function () {
            $("#AddNewChannelDialog table tr").eq(0).show();
            selector.$txtChannelID_Dialog().val("");
            selector.$txtChannelName_Dialog().val("");
            selector.$txtOffset_Dialog().val("");
            selector.$txtContactBank_Dialog().val("");
            selector.$txtSupplierName_Dialog().val("");
            selector.$txtMerchantsCode_Dialog().val("");
            selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', "");
            selector.$DepartmentVguid().val("");
            selector.$txtDirectBankEnterprise_Dialog().val("");
            vguid = "";
            isEdit = false;
            $(".msg").remove();
            $("#myModalLabel_title").text("新增渠道");
            selector.$txtChannelName_Dialog().removeClass("input_Validate");
            selector.$txtOffset_Dialog().removeClass("input_Validate");
            selector.$AddNewChannelDialog().modal({ backdrop: "static", keyboard: false });
            selector.$AddNewChannelDialog().modal("show");
        });
        //弹出框中的取消按钮
        selector.$AddNewChannel_CancelBtn().on("click", function () {
            selector.$AddNewChannelDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddNewChannel_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate(selector.$txtChannelID_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtChannelName_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtContactBank_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtSupplierName_Dialog())) {
                validateError++;
            }
            //if (!Validate(selector.$txtMerchantsCode_Dialog())) {
            //    validateError++;
            //}
            if (selector.$DepartmentVguid().val() != "") {
                if (validateError <= 0) {
                    $.ajax({
                        url: "/SystemManagement/ChannelManagement/SaveChannelInfo?isEdit=" + isEdit,
                        data: {
                            Id: selector.$txtChannelID_Dialog().val(),
                            Name: selector.$txtChannelName_Dialog().val(),
                            ContactBank: selector.$txtContactBank_Dialog().val(),
                            SupplierName: selector.$txtSupplierName_Dialog().val(),
                            MerchantsCode: selector.$txtMerchantsCode_Dialog().val(),
                            Department: selector.$DepartmentVguid().val(),
                            PaymentEncoding: selector.$txtDirectBankEnterprise_Dialog().val(),
                            Vguid: vguid
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
                                    selector.$AddNewChannelDialog().modal("hide");
                                    break;
                                case "2":
                                    jqxNotification("渠道名称已经存在！", null, "error");
                                    break;
                            }

                        }
                    });
                }
            }
            else {
                jqxNotification("请选择部门！", null, "error");
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
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Id', type: 'string' },
                    { name: 'Name', type: 'string' },
                    { name: 'ContactBank', type: 'string' },
                    { name: 'SupplierName', type: 'string' },
                    { name: 'MerchantsCode', type: 'string' },
                    { name: 'Department', type: 'string' },
                    { name: 'OrganizationName', type: 'string' },
                    { name: 'Offset', type: 'number' },
                    { name: 'PaymentEncoding', type: 'string' },
                    { name: 'Vguid', type: 'string' }
                ],
                datatype: "json",
                id: "Vguid",
                data: { "Name": selector.$txtChannelName().val() },
                url: "/Systemmanagement/ChannelManagement/GetChannelInfos"   //获取数据源的路径
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
                height: 400,
                pageSize: 10,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                columns: [
                    { width: 35, text: "", datafield: "checkbox", align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '渠道编号', datafield: 'Id', align: 'center', cellsAlign: 'center', cellsRenderer: channelDetailFunc },
                    { text: '渠道名称', datafield: 'Name', align: 'center', cellsAlign: 'center' },
                    { text: '收单银行', datafield: 'ContactBank', align: 'center', cellsAlign: 'center' },
                    { text: '供应商', datafield: 'SupplierName', align: 'center', cellsAlign: 'center' },
                    { text: '商户编号', datafield: 'MerchantsCode', align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '部门', datafield: 'OrganizationName', align: 'center', cellsAlign: 'center' },
                    { text: '偏移量', datafield: 'Offset', align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '支付编码', datafield: 'PaymentEncoding', align: 'center', cellsAlign: 'center' },
                    { text: 'Department', datafield: 'Department', hidden: true },
                    { text: 'Vguid', datafield: 'Vguid', hidden: true }
                ]
            });

    }

    function channelDetailFunc(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "true") {

            container = "<a href='#' onclick=edit('" + rowData.Vguid + "','" + rowData.Id + "','" + rowData.Name + "','" + rowData.Offset + "','" + rowData.ContactBank + "','" + rowData.SupplierName + "','" + rowData.MerchantsCode + "','" + rowData.Department + "','" + rowData.OrganizationName + "','" + rowData.PaymentEncoding + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Id + "</a>";
        } else {
            container = "<span>" + rowData.Id + "</span>";
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
            url: "/Systemmanagement/ChannelManagement/DeleteChannelInfos",
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

                //if (selector.$currentUserDepartment().val().indexOf(item.id) == -1) {
                //    jqxNotification("请选择本公司及其子部门！", null, "error");
                //    return false;
                //}
                selector.$DepartmentVguid().val(item.id);
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



function edit(guid, id, name, offset, contactbank, suppliername, merchantscode, department, organizationname, directBankEnterprise) {
    selector.$txtChannelID_Dialog().val("");
    selector.$txtChannelName_Dialog().val("");
    selector.$txtOffset_Dialog().val("");
    selector.$txtContactBank_Dialog().val("");
    selector.$txtSupplierName_Dialog().val("");
    selector.$txtMerchantsCode_Dialog().val("");
    selector.$txtDirectBankEnterprise_Dialog().val("");
    selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', "");
    isEdit = true;
    vguid = guid;
    $("#myModalLabel_title").text("编辑渠道");
    if (!offset) {
        offset = 0;
    }
    selector.$txtChannelID_Dialog().val(id);
    selector.$txtChannelName_Dialog().val(name);
    selector.$txtOffset_Dialog().val(offset);
    selector.$txtContactBank_Dialog().val(contactbank == "null" ? "" : contactbank);
    selector.$txtSupplierName_Dialog().val(suppliername == "null" ? "" : suppliername);
    selector.$txtMerchantsCode_Dialog().val(merchantscode == "null" ? "" : merchantscode);
    selector.$DepartmentVguid().val(department == "null" ? "" : department);
    selector.$txtDirectBankEnterprise_Dialog().val(directBankEnterprise == "null" ? "" : directBankEnterprise);
    $("#AddNewChannelDialog table tr").eq(0).hide();

    var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 5px;">' + organizationname + '</div>';
    selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', dropDownContent);
    selector.$pushTree().jqxTree('val', department);
    //}


    $(".msg").remove();
    selector.$txtChannelName_Dialog().removeClass("input_Validate");
    selector.$txtOffset_Dialog().removeClass("input_Validate");
    selector.$AddNewChannelDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewChannelDialog().modal("show");
}

$(function () {
    var page = new $page();
    page.init();
});
