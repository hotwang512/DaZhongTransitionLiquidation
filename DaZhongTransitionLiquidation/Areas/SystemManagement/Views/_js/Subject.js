Date.prototype.Format = function (fmt) { //author: meizz 
    var o = {
        "M+": this.getMonth() + 1, //月份 
        "d+": this.getDate(), //日 
        "h+": this.getHours(), //小时 
        "m+": this.getMinutes(), //分 
        "s+": this.getSeconds(), //秒 
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
        "S": this.getMilliseconds() //毫秒 
    };
    if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}


//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },

    $AddNewSubjectDialog: function () { return $("#AddNewSubjectDialog") },
    $AddNewSubject_OKButton: function () { return $("#AddNewSubject_OKButton") },
    $AddNewSubject_CancelBtn: function () { return $("#AddNewSubject_CancelBtn") },
    $txtSubjectName: function () { return $("#txtSubjectName") },
    $txtRate_Dialog: function () { return $("#txtRate_Dialog") },
    $txtChannel_Dialog: function () { return $("#txtChannel_Dialog") },
    $txtSubjectId_Dialog: function () { return $("#txtSubjectId_Dialog") },
    $txtSubjectName_Dialog: function () { return $("#txtSubjectName_Dialog") },

    $txtTerminalNo_Dialog: function () { return $("#txtTerminalNo_Dialog") },
    $txtShopNo_Dialog: function () { return $("#txtShopNo_Dialog") },
    $txtStoreNo_Dialog: function () { return $("#txtStoreNo_Dialog") },
    $DepartmentVguid: function () { return $("#DepartmentVguid") },
    $txtDeposit_Dialog: function () { return $("#txtDeposit_Dialog") },

    $txtContractStartTime_Dialog: function () { return $("#txtContractStartTime_Dialog") },
    $txtContractEndTime_Dialog: function () { return $("#txtContractEndTime_Dialog") },
    $pushPeopleDropDownButton: function () { return $("#pushPeopleDropDownButton") },
    $pushTree: function () { return $("#pushTree") },
    //$pushPeopleDropDownButton: function () { return $("#pushPeopleDropDownButton") },
    //$pushTree: function () { return $("#pushTree") },
    $SubjectVguid: function () { return $("#SubjectVguid") },
    //$EditPermission: function () { return $("#EditPermission") }
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
            selector.$txtSubjectName().val("");
        });

        //新增
        selector.$btnAdd().on("click", function () {
            isEdit = false;
            selector.$txtChannel_Dialog().val("");
            selector.$txtSubjectId_Dialog().val("");
            selector.$txtSubjectName_Dialog().val("");
            selector.$txtContractStartTime_Dialog().val("");
            selector.$txtContractEndTime_Dialog().val("");
            selector.$txtRate_Dialog().val("");
            selector.$txtTerminalNo_Dialog().val("");
            selector.$txtShopNo_Dialog().val("");
            selector.$txtStoreNo_Dialog().val("");
            selector.$txtDeposit_Dialog().prop("checked", false);
            selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', "");
            selector.$DepartmentVguid().val("");
            vguid = "";
            //selector.$SubjectVguid().val("");
            $("#AddNewSubjectDialog table tr").eq(0).show();

            selector.$AddNewSubjectDialog().modal({ backdrop: "static", keyboard: false });
            selector.$AddNewSubjectDialog().modal("show");
        });
        //弹出框中的取消按钮
        selector.$AddNewSubject_CancelBtn().on("click", function () {
            selector.$AddNewSubjectDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddNewSubject_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量

            if (!Validate(selector.$txtSubjectId_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtSubjectName_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtChannel_Dialog())) {
                validateError++;
            }
            if (selector.$DepartmentVguid().val() != "") {
                if (validateError <= 0) {
                    $.ajax({
                        url: "/SystemManagement/SubjectManagement/SaveSubjectInfo?isEdit=" + isEdit,
                        data: {
                            SubjectId: selector.$txtSubjectId_Dialog().val(),
                            SubjectNmae: selector.$txtSubjectName_Dialog().val(),
                            ChannelVguid: selector.$txtChannel_Dialog().val(),
                            Department: selector.$DepartmentVguid().val(),
                            Deposit: selector.$txtDeposit_Dialog().is(':checked'),
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
                                    selector.$AddNewSubjectDialog().modal("hide");
                                    break;
                                case "2":
                                    jqxNotification("科目名称已经存在！", null, "error");
                                    break;
                                case "3":
                                    jqxNotification("科目编号已经存在！", null, "error");
                                    break;
                            }

                        }
                    });
                }
            }
            else {
                jqxNotification("公司不能为空！", null, "error");
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
                    { name: 'SubjectId', type: 'string' },
                    { name: 'SubjectNmae', type: 'string' },
                    { name: 'Rate', type: 'string' },
                    { name: 'ContractStartTime', type: 'date' },
                    { name: 'ContractEndTime', type: 'date' },
                    { name: 'ChannelVguid', type: 'string' },
                    { name: 'TerminalNo', type: 'string' },
                    { name: 'StoreNo', type: 'string' },
                    { name: 'ShopNo', type: 'string' },
                    { name: 'ChannelName', type: 'string' },
                    { name: 'Department', type: 'string' },
                    { name: 'OrganizationName', type: 'string' },
                    { name: 'Deposit', type: 'bool' },
                    { name: 'Vguid', type: 'string' }
                ],
                datatype: "json",
                id: "Vguid",
                data: { "SubjectNmae": selector.$txtSubjectName().val() },
                url: "/Systemmanagement/SubjectManagement/GetChannelInfos"   //获取数据源的路径
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
                    { text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '二级渠道ID', datafield: 'SubjectId', width: "25%", align: 'center', cellsAlign: 'center', cellsRenderer: channelDetailFunc },
                    { text: '二级渠道名称', datafield: 'SubjectNmae', width: "25%", align: 'center', cellsAlign: 'center' },
                    { text: '渠道名称', datafield: 'ChannelName', width: "25%", align: 'center', cellsAlign: 'center' },
                    { text: '终端号', datafield: 'TerminalNo', width: "25%", align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '慧兜圈商户号', datafield: 'ShopNo', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '慧兜圈门店号', datafield: 'StoreNo', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '费率', datafield: 'Rate', width: 50, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '公司', datafield: 'OrganizationName', minwidth: 150, align: 'center', cellsAlign: 'center' },
                    { text: '押金', datafield: 'Deposit', columntype: 'checkbox', width: "5%", align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererDeposit },
                    { text: '合同开始时间', datafield: 'ContractStartTime', align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd", hidden: true },
                    { text: '合同结束时间', datafield: 'ContractEndTime', align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd", hidden: true },
                    { text: 'Vguid', datafield: 'Vguid', hidden: true }
                ]
            });

    }

    function channelDetailFunc(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "true") {

            var startTime = new Date(rowData.ContractStartTime).Format("yyyy-MM-dd");
            var endTime = new Date(rowData.ContractEndTime).Format("yyyy-MM-dd");

            container = "<a href='#' onclick=edit('" + rowData.Vguid + "','" + rowData.SubjectId + "','" + rowData.SubjectNmae + "','" + rowData.TerminalNo + "','" + rowData.ShopNo + "','" + rowData.StoreNo + "','" + rowData.Rate + "','" + rowData.Department + "','" + rowData.OrganizationName + "','" + startTime + "','" + endTime + "','" + rowData.ChannelVguid + "'," + rowData.Deposit + ") style=\"text-decoration: underline;color: #333;\">" + rowData.SubjectId + "</a>";
        } else {
            container = "<span>" + rowData.SubjectId + "</span>";
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

    function cellsRendererDeposit(row, column, value, rowData) {
        var html = '<input type="checkbox" />';
        if (value == true) {
            html = '<input type="checkbox" checked="checked" />';
        }
        return html;
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
            url: "/Systemmanagement/SubjectManagement/DeleteChannelInfos",
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

function edit(guid, SubjectId, SubjectNmae, terminalNo, shopNo, storeNo, Rate, department, organizationname, contractStartTime, contractEndTime, ChannelVguid, Deposit) {
    selector.$txtChannel_Dialog().val("");
    selector.$txtSubjectId_Dialog().val("");
    selector.$txtSubjectName_Dialog().val("");
    selector.$txtDeposit_Dialog().prop("checked", false);
    selector.$txtDeposit_Dialog().prop("checked", false);
    selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', "");

    isEdit = true;
    vguid = guid;
    //$("#AddNewSubjectDialog table tr").eq(0).hide();

    selector.$txtChannel_Dialog().val(ChannelVguid);
    selector.$txtSubjectId_Dialog().val(SubjectId == "null" ? "" : SubjectId);
    selector.$txtSubjectName_Dialog().val(SubjectNmae == "null" ? "" : SubjectNmae);
    selector.$txtSubjectName_Dialog().val(SubjectNmae == "null" ? "" : SubjectNmae);
    selector.$txtDeposit_Dialog().prop("checked", Deposit);

    selector.$DepartmentVguid().val(department = null ? "" : department);
    if (organizationname != "null") {
        var dropDownContent = '<div style="position: relative; margin-left: 3px; margin-top: 5px;">' + organizationname + '</div>';
        selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', dropDownContent);
        selector.$pushTree().jqxTree('val', department);
    }


    selector.$AddNewSubjectDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewSubjectDialog().modal("show");
}


$(function () {
    var page = new $page();
    page.init();
});
