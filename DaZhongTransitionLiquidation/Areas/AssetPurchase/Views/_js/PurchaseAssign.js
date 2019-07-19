//资产基础信息维护列表
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var $page = function () {

    this.init = function () {
        initSelectPurchaseGoods();
        addEvent();
    }
    //所有事件
    function addEvent() {
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#PurchaseGoods").val("");
            $("#SubmitStatus").val("");
        });
        $("#OrderBelongToDialog_OKBtn").on("click",
            function () {
                debugger;
                $.ajax({
                    url: "/AssetPurchase/PurchaseAssign/SubmitAssign",
                    data: {
                        "vguid": $("#AssetsOrderVguid").val()
                    },
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("提交失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("提交成功！", null, "success");
                                $("#OrderBelongToDialog").modal("hide");
                                initTable();
                                break;
                            case "2":
                                jqxNotification(msg.ResultInfo, null, "success");
                                $("#OrderBelongToDialog").modal("hide");
                                break;
                        }
                    }
                });
            }
        );
        //$("#OrderDetailsDialog_OKBtn").on("click",
        //    function () {
        //        $("#OrderDetailsDialog").modal("hide");
        //        initTable();
        //    }
        //);
        $("#OrderDetailsDialog_CancelBtn").on("click",
            function () {
                $("#OrderDetailsDialog").modal("hide");
            }
        );
        //$("#SettingDialog_OKBtn").on("click",
        //    function () {
        //        $("#SettingModalDialog").modal("hide");
        //    }
        //);
        $("#SettingDialog_CancelBtn").on("click",
            function () {
                $("#SettingModalDialog").modal("hide");
            }
        );
        $("#OrderBelongToDialog_CancelBtn").on("click",
            function () {
                $("#OrderBelongToDialog").modal("hide");
            }
        );
        //统一上传文件
        $("#LocalFileInput").on("change",
            function () {
                var filePath = this.value;
                var fileExt = filePath.substring(filePath.lastIndexOf("."))
                    .toLowerCase();
                if (!checkFileExt(fileExt)) {
                    jqxNotification("您上传的文件类型不允许,请重新上传！！", null, "error");
                    this.value = "";
                    return;
                } else {
                    layer.load();
                    $("#localFormFile").ajaxSubmit({
                        url: "/AssetPurchase/PurchaseAssign/ImportAssignFile",
                        type: "post",
                        data: {
                            'vguid': $("#AssetsOrderVguid").val()
                        },
                        success: function (msg) {
                            layer.closeAll('loading');
                            switch (msg.Status) {
                                case "0":
                                    if (msg.ResultInfo != null || msg.ResultInfo2 != null) {
                                        jqxNotification((msg.ResultInfo == null ? "" : msg.ResultInfo) + " " + (msg.ResultInfo2 == null ? "" : msg.ResultInfo2), null, "error");
                                    } else {
                                        jqxNotification("导入失败", null, "error");
                                    }
                                $('#LocalFileInput').val('');
                                break;
                            case "1":
                                jqxNotification("导入成功！", null, "success");
                                $('#LocalFileInput').val('');
                                getAttachment();
                                break;
                            }
                        }
                    });
                }
            });
    }; //addEvent end
  

    function initTable() {
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
        var source =
            {
                datafields:
                [
                    { name: 'VGUID', type: 'string' },
                    { name: 'FixedAssetsOrderVguid', type: 'string' },
                    { name: 'PurchaseGoods', type: 'string' },
                    { name: 'OrderQuantity', type: 'number' },
                    { name: 'PurchasePrices', type: 'float' },
                    { name: 'ContractAmount', type: 'float' },
                    { name: 'AssetDescription', type: 'string' },
                    { name: 'SubmitStatus', type: 'number' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'ChangeDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'ChangeUser', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "PurchaseGoodsVguid": $("#PurchaseGoods").val(), "SubmitStatus": $("#SubmitStatus").val() },
                url: "/AssetPurchase/PurchaseAssign/GetBusiness_PurchaseAssignListDatas"   //获取数据源的路径
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
                    { text: '提交状态', datafield: 'SubmitStatus', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererSubmit },
                    { text: '配置资产', datafield: 'Setting', hidden: false, width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: cellsSettingRenderer },
                    { text: 'FixedAssetsOrderVguid', datafield: 'FixedAssetsOrderVguid', width: 150, align: 'center', cellsAlign: 'center', hidden:true },
                    { text: '采购物品', datafield: 'PurchaseGoods', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购数量', datafield: 'OrderQuantity', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购单价', datafield: 'PurchasePrices', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '合同金额', datafield: 'ContractAmount', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购说明', datafield: 'AssetDescription', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '创建时间', datafield: 'CreateDate', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CreateUser', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '修改时间', datafield: 'ChangeDate', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
    }
    //配置改为导入
    function cellsSettingRenderer(row, column, value, rowData) {
        var FixedAssetsOrderVguid = rowData.FixedAssetsOrderVguid;
        debugger;
        if (rowData.SubmitStatus == 0) {
            //出租车时导入Excel分配，其它需要手动分配
            if (rowData.PurchaseGoods == "出租车") {
                return '<div style="margin: 8px; margin-top:6px;">' +
                    '<a style="cursor:pointer"  onclick="Import(\'' +
                    FixedAssetsOrderVguid +
                    '\')" id="' +
                    FixedAssetsOrderVguid +
                    '">导入清册</a>' +
                    '</div>';
            } else {
                return '<div style="margin: 8px; margin-top:6px;">' +
                    '<a style="cursor:pointer"  onclick="Setting(\'' + FixedAssetsOrderVguid + '\')" id="' + FixedAssetsOrderVguid + '">配置</a>' +
                    '&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp<div style="display:inline-block;margin-top:-15px;margin-bottom:-18px;width: 1px;height:48px; background: darkgray;"></div>' +
                    '&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp<a style="cursor:pointer"  onclick="ViewAssign(\'' +
                    FixedAssetsOrderVguid +
                    '\')">提交</a>' +
                    '</div>';
            }
        } else {
            if (rowData.PurchaseGoods == "出租车") {
                return '<div style="margin: 8px; margin-top:6px;">' +
                    '<a style="cursor:pointer"  onclick="Import(\'' +
                    FixedAssetsOrderVguid +
                    '\')" id="' +
                    FixedAssetsOrderVguid +
                    '">导入清册</a>' +
                    '</div>';
            } else {
                return '<div style="margin-top:6px;">' +
                    '<a style="cursor:pointer"  onclick="ViewAssign(\'' +
                    FixedAssetsOrderVguid +
                    '\')">查看</a>' +
                    '</div>';
            }

        }
        
    }
    function cellsRendererSubmit(row, column, value, rowData) {
        if (value === 1) {
            return '<span style="margin: 4px; margin-top:8px;">已提交</span>';
        } else if (value === 0) {
            return '<span style="margin: 4px; margin-top:8px;">待提交</span>';
        }
    }
    function initSelectPurchaseGoods() {
        //使用部门
        $.ajax({
            url: "/AssetPurchase/FixedAssetsOrderDetail/GetPurchaseGoods",
            data: { "OrderCategory": 0 },
            type: "POST",
            dataType: "json",
            async: false,
            success: function (msg) {
                uiEngineHelper.bindSelect('#PurchaseGoods', msg, "VGUID", "PurchaseGoods");
                $("#PurchaseGoods").prepend("<option value=\"\" selected='true'>请选择</>");
            }
        });
    }
};
function Setting(vguid) {
    $("#AssetsOrderVguid").val(vguid);
    var source =
    {
        url: "/AssetPurchase/PurchaseAssign/GetAssetOrderDetails",
        data: { AssetType: "vehicle", AssetsOrderVguid: vguid },
        datatype: "json",
        updaterow: function (rowid, rowdata, commit) {
            $.ajax({
                url: "/AssetPurchase/FixedAssetsOrderDetail/UpdateAssetNum",
                data: { vguid: rowdata.VGUID, AssetNum: rowdata.AssetNum },
                async: false,
                type: "post",
                success: function (result) {
                }
            });
            commit(true);
        },
        datafields:
        [
            { name: 'VGUID', type: 'string' },
            { name: 'AssetsOrderVguid', type: 'string' },
            { name: 'AssetManagementCompany', type: 'string' },
            { name: 'AssetNum', type: 'number' }
        ]
    };
    var dataAdapter = new $.jqx.dataAdapter(source);
    $("#grid").jqxGrid(
    {
        width: "545",
        autoheight: true,
        source: dataAdapter,
        showstatusbar: true,
        statusbarheight: 25,
        editable: true,
        enabletooltips: true,
        showaggregates: true,
        selectionmode: 'multiplecellsadvanced',
        columns: [
            { text: 'VGUID', datafield: 'VGUID', columntype: 'textbox', width: 190, align: 'center', cellsAlign: 'center', hidden: true, editable: false },
            { text: '资产订单关联ID', datafield: 'AssetsOrderVguid', columntype: 'textbox', width: 190, align: 'center', cellsAlign: 'center', hidden: true, editable: false },
            { text: '资产管理公司', datafield: 'AssetManagementCompany',  width: 325, align: 'center', cellsAlign: 'center', editable: false },
            {
                text: '数量', datafield: 'AssetNum', width: 120, align: 'center', cellsalign: 'center', columntype: 'textbox',
                validation: function (cell, value) {
                    debugger;
                    if (value == "")
                        return true;
                    if (!isNumber(value)) {
                        return { result: false, message: "请输入数字" };
                    }
                    return true;
                },
                aggregates: [
                    {
                        '合计':
                            function (aggregatedValue, currentValue) {
                                if (currentValue != "") {
                                    aggregatedValue += currentValue;
                                }
                                return aggregatedValue;
                            }
                    }
                ]
            },
            { text: '资产归属配置',columntype: 'textbox', width: 100, editable: false, cellsrenderer: cellsrenderer, align: 'center', cellsAlign: 'center' }
        ]
    });
    $("#OrderDetailsDialog").modal("show");
}

function ViewBelongTo(vguid) {
    var source =
    {
        url: "/AssetPurchase/PurchaseAssign/GetOrderBelong",
        data: { AssetsOrderVguid: vguid },
        datatype: "json",
        datafields:
        [
            { name: 'VGUID', type: 'string' },
            { name: 'AssetsOrderVguid', type: 'string' },
            { name: 'BelongToCompany', type: 'string' },
            { name: 'AssetNum', type: 'number' },
            { name: 'PurchasePrices', type: 'float' },
            { name: 'PurchaseCountPrices', type: 'float' }
        ]
    };
    var dataAdapter = new $.jqx.dataAdapter(source);
    $("#gridOrderBelong").jqxGrid(
        {
            width: "470",
            autoheight: true,
            source: dataAdapter,
            statusbarheight: 25,
            enabletooltips: true,
            selectionmode: 'singlerow',
            columns: [
                { text: 'VGUID', datafield: 'VGUID', columntype: 'textbox', width: 190, align: 'center', cellsAlign: 'center', hidden: true, editable: false },
                { text: '资产订单关联ID', datafield: 'AssetsOrderVguid', columntype: 'textbox', width: 190, align: 'center', cellsAlign: 'center', hidden: true, editable: false },
                { text: '资产归属公司', datafield: 'BelongToCompany', columntype: 'textbox', width: 130, align: 'center', cellsAlign: 'center', editable: false },
                {
                    text: '数量', datafield: 'AssetNum', width: 100, align: 'center', cellsalign: 'center'
                },
                { text: '单价', datafield: 'PurchasePrices', columntype: 'textbox', width: 120, align: 'center', cellsAlign: 'center', editable: false },
                { text: '总价', datafield: 'PurchaseCountPrices', columntype: 'textbox', width: 120, align: 'center', cellsAlign: 'center', editable: false }
            ]
        });
    $("#OrderBelongToDialog").modal("show");
}
function Import(vguid) {
    $("#AssetsOrderVguid").val(vguid);
    $("#LocalFileInput").click();
}
function ViewAssign(vguid) {
    $("#AssetsOrderVguid").val(vguid);
    var source =
    {
        url: "/AssetPurchase/PurchaseAssign/GetOrderBelong",
        data: { AssetsOrderVguid: vguid },
        datatype: "json",
        datafields:
        [
            { name: 'VGUID', type: 'string' },
            { name: 'AssetsOrderVguid', type: 'string' },
            { name: 'BelongToCompany', type: 'string' },
            { name: 'AssetNum', type: 'number' },
            { name: 'PurchasePrices', type: 'float' },
            { name: 'PurchaseCountPrices', type: 'float' }
        ]
    };
    var dataAdapter = new $.jqx.dataAdapter(source);
    $("#gridOrderBelong").jqxGrid(
        {
            width: "470",
            autoheight: true,
            source: dataAdapter,
            statusbarheight: 25,
            enabletooltips: true,
            pageable: true,
            columnsresize: true,
            pagesize: 5,
            selectionmode: 'singlerow',
            columns: [
                { text: 'VGUID', datafield: 'VGUID', columntype: 'textbox', width: 190, align: 'center', cellsAlign: 'center', hidden: true, editable: false },
                { text: '资产订单关联ID', datafield: 'AssetsOrderVguid', columntype: 'textbox', width: 190, align: 'center', cellsAlign: 'center', hidden: true, editable: false },
                { text: '资产归属公司', datafield: 'BelongToCompany', columntype: 'textbox', width: 130, align: 'center', cellsAlign: 'center', editable: false },
                {
                    text: '数量', datafield: 'AssetNum', width: 100, align: 'center', cellsalign: 'center'
                },
                { text: '单价', datafield: 'PurchasePrices', columntype: 'textbox', width: 120, align: 'center', cellsAlign: 'center', editable: false },
                { text: '总价', datafield: 'PurchaseCountPrices', columntype: 'textbox', width: 120, align: 'center', cellsAlign: 'center', editable: false }
            ]
        });
    $("#OrderBelongToDialog").modal("show");
}
function cellsrenderer(row, column, value, rowData) {
    var vguid = $('#grid').jqxGrid('getrowdata', row).VGUID;
    return '<div style="margin: 35px; margin-top:6px;"><a style="cursor:pointer"  onclick="settingBelongTo(\'' + vguid + '\')" id="' + vguid + '">配置</a></div>';
}
function settingBelongTo(AssetOrderDetailsVguid) {
    var orderdetailsurl = "/AssetPurchase/PurchaseAssign/GetPurchaseAssign";
    var ordersSource =
    {
        dataFields: [
            { name: 'VGUID', type: 'string' },
            { name: 'AssetsOrderVguid', type: 'string' },
            { name: 'AssetOrderDetailsVguid', type: 'string' },
            { name: 'AssetManagementCompany', type: 'string' },
            { name: 'BelongToCompany', type: 'string' },
            { name: 'AssetNum', type: 'string' }
        ],
        dataType: "json",
        url: orderdetailsurl,
        data: { AssetOrderDetailsVguid: AssetOrderDetailsVguid },
        addRow: function (rowID, rowData, position, commit) {
            commit(true);
        },
        updateRow: function (rowID, rowData, commit) {
            debugger;
            $.ajax({
                url: "/AssetPurchase/PurchaseAssign/SaveBelongToRow",
                data: { vguid: rowData.VGUID, AssetNum: rowData.AssetNum, AssetOrderDetailsVguid: AssetOrderDetailsVguid, AssetsOrderVguid: $("#AssetsOrderVguid").val(), BelongToCompany: rowData.BelongToCompany },
                async: false,
                type: "post",
                success: function (result) {
                    commit(true);
                }
            });
        },
        deleteRow: function (rowID, commit) {
            var selection = $("#table").jqxDataTable('getSelection');
            var rowData = selection[0];
            debugger;
            $.ajax({
                url: "/AssetPurchase/PurchaseAssign/DeleteBelongToRow",
                data: { vguid: rowData.VGUID},
                async: false,
                type: "post",
                success: function (result) {
                }
            });
            commit(true);
        }
    };
    var dataAdapter = new $.jqx.dataAdapter(ordersSource, {
        loadComplete: function () {
            // data is loaded.
        }
    });
    var getEditorDataAdapter = function (datafield) {
        var source =
        {
            url: "/AssetPurchase/PurchaseAssign/GetBelongToCompany",
            dataType: "json",
            dataFields:
            [
                { name: 'BelongToCompany', type: 'string' }
            ]
        };
        var dataAdapter = new $.jqx.dataAdapter(source, { uniqueDataFields: [datafield] });
        return dataAdapter;
    }
    $("#table").jqxDataTable(
    {
        width: 500,filterHeight: 40,
        source: dataAdapter,
        height:"330px",
        pageable: true,
        editable: true,
        autoRowHeight: false,
        showToolbar: true,
        ready: function () {
        },
        pagerButtonsCount: 8,
        toolbarHeight: 35,
        renderToolbar: function (toolBar) {
            var toTheme = function (className) {
                if (theme === "") return className;
                return className + " " + className + "-" + theme;
            }
            // appends buttons to the status bar.
            var container = $("<div style='overflow: hidden; position: relative; height: 100%; width: 100%;'></div>");
            var buttonTemplate = "<div style='float: left; padding: 3px; margin: 2px;'><div style='margin: 4px; width: 16px; height: 16px;'></div></div>";
            var addButton = $(buttonTemplate);
            var editButton = $(buttonTemplate);
            var deleteButton = $(buttonTemplate);
            var cancelButton = $(buttonTemplate);
            var updateButton = $(buttonTemplate);
            container.append(addButton);
            container.append(editButton);
            container.append(deleteButton);
            container.append(cancelButton);
            container.append(updateButton);
            toolBar.append(container);
            addButton.jqxButton({ cursor: "pointer", enableDefault: false, height: 25, width: 25 });
            addButton.find('div:first').addClass(toTheme('jqx-icon-plus'));
            addButton.jqxTooltip({ position: 'bottom', content: "Add" });
            editButton.jqxButton({ cursor: "pointer", disabled: true, enableDefault: false, height: 25, width: 25 });
            editButton.find('div:first').addClass(toTheme('jqx-icon-edit'));
            editButton.jqxTooltip({ position: 'bottom', content: "Edit" });
            deleteButton.jqxButton({ cursor: "pointer", disabled: true, enableDefault: false, height: 25, width: 25 });
            deleteButton.find('div:first').addClass(toTheme('jqx-icon-delete'));
            deleteButton.jqxTooltip({ position: 'bottom', content: "Delete" });
            updateButton.jqxButton({ cursor: "pointer", disabled: true, enableDefault: false, height: 25, width: 25 });
            updateButton.find('div:first').addClass(toTheme('jqx-icon-save'));
            updateButton.jqxTooltip({ position: 'bottom', content: "Save Changes" });
            cancelButton.jqxButton({ cursor: "pointer", disabled: true, enableDefault: false, height: 25, width: 25 });
            cancelButton.find('div:first').addClass(toTheme('jqx-icon-cancel'));
            cancelButton.jqxTooltip({ position: 'bottom', content: "Cancel" });
            var updateButtons = function (action) {
                switch (action) {
                    case "Select":
                        addButton.jqxButton({ disabled: false });
                        deleteButton.jqxButton({ disabled: false });
                        editButton.jqxButton({ disabled: false });
                        cancelButton.jqxButton({ disabled: true });
                        updateButton.jqxButton({ disabled: true });
                        break;
                    case "Unselect":
                        addButton.jqxButton({ disabled: false });
                        deleteButton.jqxButton({ disabled: true });
                        editButton.jqxButton({ disabled: true });
                        cancelButton.jqxButton({ disabled: true });
                        updateButton.jqxButton({ disabled: true });
                        break;
                    case "Edit":
                        addButton.jqxButton({ disabled: true });
                        deleteButton.jqxButton({ disabled: true });
                        editButton.jqxButton({ disabled: true });
                        cancelButton.jqxButton({ disabled: false });
                        updateButton.jqxButton({ disabled: false });
                        break;
                    case "End Edit":
                        addButton.jqxButton({ disabled: false });
                        deleteButton.jqxButton({ disabled: false });
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
            addButton.click(function (event) {
                if (!addButton.jqxButton('disabled')) {
                    // add new empty row.
                    $("#table").jqxDataTable('addRow', null, {}, 'first');
                    // select the first row and clear the selection.
                    $("#table").jqxDataTable('clearSelection');
                    $("#table").jqxDataTable('selectRow', 0);
                    // edit the new row.
                    $("#table").jqxDataTable('beginRowEdit', 0);
                    updateButtons('add');
                }
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
            deleteButton.click(function () {
                if (!deleteButton.jqxButton('disabled')) {
                    $("#table").jqxDataTable('deleteRow', rowIndex);
                    updateButtons('delete');
                }
            });
        },
        columns: [
            { text: 'VGUID', editable: false, dataField: 'VGUID', width: 200, hidden: true },
            { text: 'AssetsOrderVguid', editable: false, dataField: 'AssetsOrderVguid', width: 200, hidden: true },
            { text: 'AssetOrderDetailsVguid', editable: false, dataField: 'AssetOrderDetailsVguid', width: 200, hidden: true },
            {
                text: '资产归属公司', columntype: 'template', dataField: 'BelongToCompany', cellsAlign: 'center', align: 'center', width: 240,
                createEditor: function (row, cellvalue, editor, cellText, width, height) {
                    editor.jqxDropDownList({
                        source: getEditorDataAdapter('BelongToCompany'), displayMember: 'BelongToCompany', valueMember: 'BelongToCompany', width: width, height: height
                    });
                },
                initEditor: function (row, cellvalue, editor, celltext, width, height) {
                    editor.jqxDropDownList({ width: width, height: height });
                    editor.val(cellvalue);
                },
                getEditorValue: function (row, cellvalue, editor) {
                    return editor.val();
                }
            },
            {
                text: '数量', dataField: 'AssetNum', cellsAlign: 'center', align: 'center', width: 260,
                columnType: 'custom',
                createEditor: function (row, cellValue, editor, width, height) {
                    var textBox = $("<input style='padding-left: 4px; box-sizing: border-box; -moz-box-sizing: border-box; border: none;' type='number'/>").appendTo(editor);;
                    textBox.jqxInput({width: '100%', height: '100%' });
                    textBox.val(cellValue);
                },
                initEditor: function (row, cellValue, editor) {
                    editor.find('input').val(cellValue);
                },
                getEditorValue: function (index, value, editor) {
                    return editor.find('input').val();
                }
            }
        ]
    });
    $("#SettingModalDialog").modal("show");
}
function checkFileExt(ext) {
    if (!ext.match(/.xls|.xlsx/i)) {
        return false;
    }
    return true;
}
function isNumber(val) {
    var regPos = /^\d+(\.\d+)?$/; //非负浮点数
    var regNeg = /^(-(([0-9]+\.[0-9]*[1-9][0-9]*)|([0-9]*[1-9][0-9]*\.[0-9]+)|([0-9]*[1-9][0-9]*)))$/; //负浮点数
    if (regPos.test(val) || regNeg.test(val)) {
        return true;
    } else {
        return false;
    }
}
$(function () {
    var page = new $page();
    page.init();
});
