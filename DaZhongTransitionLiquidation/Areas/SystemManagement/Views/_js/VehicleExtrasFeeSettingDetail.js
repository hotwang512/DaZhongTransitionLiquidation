//车辆类资产费用标准配置明细
var vguid = "";
var mydate = new Date();
var ManagementCompany = [];
var $page = function () {
    this.init = function () {
        GetVehicleModelDropDown();
        getOrderSettingInfoList();
        addEvent();
        var guid = $.request.queryString().VGUID;
        $("#VGUID").val(guid);
        if (guid != "" && guid != null) {
            $("#VehicleModel").val($.request.queryString().Code);
            $("#VehicleModel").trigger("change");
        }
    }
    //所有事件
    function addEvent() {
        //取消
        $("#btnCancel").on("click",
            function () {
                history.go(-1);
            });
        //保存
        $("#btnSave").on("click",
            function () {
                var rows = $('#table').jqxGrid('getboundrows');
                var SettingList = [];
                for (var j = 0; j < rows.length; j++) {
                    SettingList.push({ "VGUID": rows[j].VGUID, "Status": rows[j].Status, "Fee": rows[j].Fee, "BusinessSubItem": rows[j].BusinessSubItem, "VehicleModelCode": rows[j].VehicleModelCode });
                };
                $.ajax({
                    url: "/Systemmanagement/VehicleExtrasFeeSettingDetail/SaveVehicleExtrasFeeSetting",
                    data: {
                        "FeeSettingList": SettingList
                    },
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                history.go(-1);
                                window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                                break;
                        }
                    }
                });
            });
        $("#VehicleModel").on("change",
            function() {
                //
                $.ajax({
                    url: "/Systemmanagement/VehicleExtrasFeeSettingDetail/GetVehicleExtrasFeeSettingDetail",
                    data: {"VehicleModel":$("#VehicleModel").val()},
                    type: "GET",
                    dataType: "json",
                    async: false,
                    success: function (msg) {
                        getOrderSettingInfoList();
                    }
                });
            });

        $("#VehicleExtrasFeeSettingDialog_OKBtn").on("click", function () {
            var validateError = 0; //未通过验证的数量
            if (!Validate($("#StartDate"))) {
                validateError++;
            }
            if (!Validate($("#PurchaseItem"))) {
                validateError++;
            }
            //$("#VehicleExtrasFeeSettingDialog_OKBtn").attr("vguid", dataRecord.VGUID);
            if (validateError <= 0) {
                $.ajax({
                    url: "/Systemmanagement/VehicleExtrasFeeSettingDetail/UpdateExtrasFeeSetting",
                    data: { StartDate: $("#StartDate").val(), Fee: $("#Fee").val(), VGUID: $("#VehicleExtrasFeeSettingDialog_OKBtn").attr("vguid") },
                    traditional: true,
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                        case "0":
                            jqxNotification("修改失败！", null, "error");
                            break;
                        case "1":
                            $("#VehicleExtrasFeeSettingModalDialog").modal("hide");
                            jqxNotification("修改成功！", null, "success");
                            getOrderSettingInfoList();
                            break;
                        }
                    }
                });
            }
        });
        $("#VehicleExtrasFeeSettingDialog_CancelBtn").on("click", function () {
            $("#VehicleExtrasFeeSettingModalDialog").modal("hide");
        });
    }; //addEvent end
    function GetVehicleModelDropDown() {
        $.ajax({
            url: "/Systemmanagement/VehicleExtrasFeeSettingDetail/GetVehicleModelDropDown",
            type: "GET",
            dataType: "json",
            async: false,
            success: function (msg) {
                uiEngineHelper.bindSelect('#VehicleModel', msg, "Code", "Descrption");
                $("#VehicleModel").prepend("<option value=\"\" selected='true'>请选择</>");
            }
        });
    }
    function getOrderSettingInfoList() {
        var url = "/SystemManagement/VehicleExtrasFeeSettingDetail/GetVehicleExtrasFeeSettingListDatas";
        var ordersSource =
        {
            dataFields: [
                { name: 'VehicleModel', type: 'string' },
                { name: 'BusinessProject', type: 'string' },
                { name: 'StartDate', type: 'date' },
                { name: 'BusinessSubItem', type: 'string' },
                { name: 'Fee', type: 'number' },
                { name: 'strShow', type: 'string' },
                { name: 'Status', type: 'bool' },
                { name: 'VGUID', type: 'string' }
            ],
            dataType: "json",
            id: 'VGUID',
            url: url,
            data: { "VehicleModel": $("#VehicleModel").val() },
            updateRow: function (rowID, rowData, commit) {
                commit(true);
            }
        };
        var dataAdapter = new $.jqx.dataAdapter(ordersSource, {
            loadComplete: function () {
            }
        });
        var editrow = -1;
        $("#table").jqxGrid(
        {
            width: "100%",
            height:"350px",
            source: dataAdapter,
            selectionmode: 'singlerow',
            editable: true,
            editmode: 'selectedcell',
            ready: function () {
            },
            toolbarHeight: 35,
            renderToolbar: function (toolBar) {
                var toTheme = function (className) {
                    if (theme == "") return className;
                    return className + " " + className + "-" + theme;
                }
                var container = $("<div style='overflow: hidden; position: relative; height: 100%; width: 100%;'></div>");
                var buttonTemplate = "<div style='float: left; padding: 3px; margin: 2px;'><div style='margin: 4px; width: 16px; height: 16px;'></div></div>";
                var editButton = $(buttonTemplate);
                var cancelButton = $(buttonTemplate);
                var updateButton = $(buttonTemplate);
                container.append(editButton);
                container.append(cancelButton);
                container.append(updateButton);
                toolBar.append(container);
                editButton.jqxButton({ cursor: "pointer", disabled: true, enableDefault: false, height: 25, width: 25 });
                editButton.find('div:first').addClass(toTheme('jqx-icon-edit'));
                editButton.jqxTooltip({ position: 'bottom', content: "编辑" });
                updateButton.jqxButton({ cursor: "pointer", disabled: true, enableDefault: false, height: 25, width: 25 });
                updateButton.find('div:first').addClass(toTheme('jqx-icon-save'));
                updateButton.jqxTooltip({ position: 'bottom', content: "保存" });
                cancelButton.jqxButton({ cursor: "pointer", disabled: true, enableDefault: false, height: 25, width: 25 });
                cancelButton.find('div:first').addClass(toTheme('jqx-icon-cancel'));
                cancelButton.jqxTooltip({ position: 'bottom', content: "取消" });
                var updateButtons = function (action) {
                    switch (action) {
                        case "Select":
                            editButton.jqxButton({ disabled: false });
                            cancelButton.jqxButton({ disabled: true });
                            updateButton.jqxButton({ disabled: true });
                            break;
                        case "Unselect":
                            editButton.jqxButton({ disabled: true });
                            cancelButton.jqxButton({ disabled: true });
                            updateButton.jqxButton({ disabled: true });
                            break;
                        case "Edit":
                            editButton.jqxButton({ disabled: true });
                            cancelButton.jqxButton({ disabled: false });
                            updateButton.jqxButton({ disabled: false });
                            break;
                        case "End Edit":
                            editButton.jqxButton({ disabled: false });
                            cancelButton.jqxButton({ disabled: true });
                            updateButton.jqxButton({ disabled: true });
                            break;
                    }
                }
                var rowIndex = null;
                $("#table").on('rowSelect', function (event) {
                    var args = event.args;
                    rowIndex = args.index;
                    updateButtons('Select');
                });
                $("#table").on('rowUnselect', function (event) {
                    updateButtons('Unselect');
                });
                $("#table").on('rowEndEdit', function (event) {
                    updateButtons('End Edit');
                });
                $("#table").on('rowBeginEdit', function (event) {
                    updateButtons('Edit');
                });
                cancelButton.click(function (event) {
                    if (!cancelButton.jqxButton('disabled')) {
                        // cancel changes.
                        $("#table").jqxDataTable('endRowEdit', rowIndex, true);
                    }
                });
                updateButton.click(function (event) {
                    if (!updateButton.jqxButton('disabled')) {
                        // save changes.
                        $("#table").jqxDataTable('endRowEdit', rowIndex, false);
                    }
                });
                editButton.click(function () {
                    if (!editButton.jqxButton('disabled')) {
                        $("#table").jqxDataTable('beginRowEdit', rowIndex);
                        updateButtons('edit');
                    }
                });
            },
            columns: [
                { text: '业务项目', editable: false, dataField: 'BusinessProject', width: 200, cellsAlign: 'center', align: 'center' },
                { text: '业务编码', editable: false, dataField: 'BusinessSubItem', width: 200, cellsAlign: 'center', align: 'center' },
                { text: '启用日期', editable: false, datafield: 'StartDate', width: 200, align: 'center', cellsalign: 'center', datatype: 'date', cellsformat: "yyyy-MM" },
                {
                    text: '费用', editable: false, columntype: 'template', cellsAlign: 'center', datafield: 'Fee', width: 180, height: 90, align: 'center', createEditor: function (row, cellvalue, editor, cellText, width, height) {
                        // construct the editor.
                        var inputElement = $("<input style='padding-left: 4px; border: none;width:200px;height:20px'/>").appendTo(editor);
                        inputElement.jqxInput({ width: '100%', height: '100%' });
                    },
                    initEditor: function (row, cellvalue, editor,celltext, width, height) {
                        var inputField = editor.find('input');
                        inputField.val(cellvalue);
                    },
                    getEditorValue: function (row, cellvalue, editor) {
                        return editor.find('input').val();
                    }
                },
                { text: '状态', datafield: 'Status', editable: true, align: 'center', columntype: 'checkbox', width: 67, cellsAlign: 'center' },
                {
                    text: '编辑', editable: false, datafield: 'Edit', align: 'center', width: 67, cellsrenderer: function (row, columnfield, value, defaulthtml, columnproperties) {
                        var dataRecord = $("#table").jqxGrid('getrowdata', row);
                        debugger;
                        if (dataRecord.strShow != "") {
                            return "<div style='margin-top:6px;margin-left:20px'><a href='#' title=" +
                                dataRecord.strShow +
                                " onclick=Edit('" +
                                row +
                                "') style=\"text-decoration: underline;color: #333;\">编辑</a></div>";
                        } else {
                            return "<div style='margin-top:6px;margin-left:20px'><a href='#'onclick=Edit('" +
                                row +
                                "') style=\"text-decoration: underline;color: #333;\">编辑</a></div>";
                        }
                    }
                },
                { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ]
        });
    }
};
function Edit(row) {
    var dataRecord = $("#table").jqxGrid('getrowdata', row);
    $("#Fee").val(dataRecord.Fee);
    $("#StartDate").val(formatDate(dataRecord.StartDate));
    $("#VehicleExtrasFeeSettingModalDialog").modal("show");
    $("#VehicleExtrasFeeSettingDialog_OKBtn").attr("vguid", dataRecord.VGUID);
}
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}
$(function () {
    var page = new $page();
    page.init();
});
function formatDate(NewDtime) {
    var d = NewDtime;
    var datetime = d.getFullYear() + '-' + (d.getMonth() + 1);//  + '-' + d.getDate() + ' ' + d.getHours() + ':' + d.getMinutes() + ':' + d.getSeconds()
    return datetime;
}