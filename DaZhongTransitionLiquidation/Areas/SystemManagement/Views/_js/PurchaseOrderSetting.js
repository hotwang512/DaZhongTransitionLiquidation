//资产基础信息维护列表
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $settinggrid: function () { return $("#jqxSettingTable") },
    $settingdepgrid: function () { return $("#jqxDepartmentTable") },
    $bankinfogrid: function () { return $("#jqxBankInfoTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var allvguids = [];
var $page = function () {

    this.init = function () {
        addEvent();
    }
    //所有事件
    function addEvent() {
        //加载列表数据
        initTable();
        InitBankCategorySelect();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#PurchaseGoods").val("");
        });
        //新增
        $("#btnAdd").on("click", function () {
            window.location.href = "/Systemmanagement/PurchaseOrderSettingDetail/Index";
            //window.open("/CapitalCenterManagement/OrderListDetail/Index");
        });
        //删除
        $("#btnDelete").on("click", function () {
            var selection = [];
            var grid = $("#jqxTable");
            var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
            checedBoxs.each(function () {
                var th = $(this);
                if (th.is(":checked")) {
                    var index = th.attr("index");
                    var data = grid.jqxDataTable('getRows')[index];
                    selection.push(data.VGUID);
                }
            });
            if (selection.length < 1) {
                jqxNotification("请选择您要删除的数据！", null, "error");
            } else {
                WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
        $("#btnAddSetting").on("click",
            function() {
                initBankTable();
                $("#BankInfoModalDialog").modal("show");
                
            });
        //设置供应商账户
        $("#BankInfoDialog_OKBtn").on("click", function () {
            var selection = [];
            var grid = selector.$bankinfogrid();
            var checedBoxs = $("#pinnedtablejqxBankInfoTable").find(".jqx_datatable_checkbox:checked");
            
            debugger;
            checedBoxs.each(function () {
                var th = $(this);
                var index = th.attr("index");
                var data = grid.jqxDataTable('getRows')[index];
                if (th.is(":checked")) {
                    debugger;
                    selection.push(data.uid);
                }
            });
            if (selection.length < 1) {
                jqxNotification("请选择数据！", null, "error");
            } else {
                debugger;
                $.ajax({
                    url: "/Systemmanagement/PurchaseOrderSetting/SetPurchaseSupplier",
                    data: { selvguids: selection, allvguids: allvguids, CustomerBankInfoCategory: $("#BankCategory").val(), PurchaseOrderSettingVguid: $("#PurchaseOrderSettingVguid").val() },
                    traditional: true,
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                        case "0":
                            jqxNotification("配置失败！", null, "error");
                            break;
                        case "1":
                            $("#BankInfoModalDialog").modal("hide");
                            jqxNotification("配置成功！", null, "success");
                            $("#jqxSettingTable").jqxDataTable('updateBoundData');
                            break;
                        }
                    }
                });
            }
        });
        $("#BankInfoDialog_CancelBtn").on("click",
            function () {
                $("#BankInfoModalDialog").modal("hide");
            }
        );
        $("#SettingDialog_CancelBtn").on("click",
            function () {
                $("#SettingModalDialog").modal("hide");
            }
        );
        $("#btnModalSearch").on("click",
            function () {
                initBankTable();
            }
        );
        
    }; //addEvent end

    function initBankTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'PurchaseOrderSettingVguid', type: 'string' },
                    { name: 'CompanyOrPerson', type: 'string' },
                    { name: 'BankAccount', type: 'string' },
                    { name: 'BankAccountName', type: 'string' },
                    { name: 'Bank', type: 'string' },
                    { name: 'BankNo', type: 'BankNo' },
                    { name: 'BankNo', type: 'BankNo' },
                    { name: 'IsCheck', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "BankAccount": $("#BankAccount").val(), "BankCategory": $("#BankCategory").val(), "OrderSettingVguid": $("#PurchaseOrderSettingVguid").val() },
                url: "/Systemmanagement/PurchaseOrderSetting/GetCustomerBankInfo"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$bankinfogrid().jqxDataTable(
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
                columnsResize: true,
                rendering: function() {
                    
                },
                columns: [
                    { text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsBankInfoRendererFunc, renderer: rendererBankInfoFunc, rendered: renderedBankFunc, autoRowHeight: false },
                    { text: '供应商类别', datafield: 'CompanyOrPerson', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '账号', datafield: 'BankAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '户名', datafield: 'BankAccountName', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '开户行', datafield: 'Bank', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '行号', datafield: 'BankNo', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
    }
    function rendererBankInfoFunc(row, column, value, rowData) {
        var checkBox =
            "<div id='jqx_datatable_checkbox_all' class='jqx_datatable_checkbox_all' style='z-index: 999; margin-left:7px ;margin-top: 7px;'>";
        checkBox += "</div>";
        return checkBox;
    }
    function cellsBankInfoRendererFunc(row, column, value, rowData) {
        if (allvguids.indexOf(rowData.uid) == -1) {
            allvguids.push(rowData.uid);
        }
        debugger;
        if (rowData.IsCheck == "Checked" && (rowData.PurchaseOrderSettingVguid != null && rowData.PurchaseOrderSettingVguid == $("#PurchaseOrderSettingVguid").val())) {
            return "<input class=\"jqx_datatable_checkbox\" id=\"" + rowData.uid + "\" checked=checked index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
        } else {
            return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
        }
    }
    function renderedBankFunc(element) {
        var grid = selector.$bankinfogrid();
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
    function dele(selection) {
        $.ajax({
            url: "/Systemmanagement/PurchaseOrderSetting/DeletePurchaseOrderSetting",
            data: { vguids: selection },
            traditional: true,
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                    case "0":
                        jqxNotification("删除失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("删除成功！", null, "success");
                        $("#jqxTable").jqxDataTable('updateBoundData');
                        break;
                }
            }
        });
    }

    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "Setting", type: null },
                    { name: "SettingDepartment", type: null },
                    { name: "SettingAssetManagementCompany", type: null },
                    { name: 'PurchaseGoods', type: 'string' },
                    { name: 'AssetCategoryMajor', type: 'string' },
                    { name: 'AssetCategoryMinor', type: 'string' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'ChangeDate', type: 'date' },
                    { name: 'ChangeUser', type: 'string' },
                    { name: 'VGUID', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "PurchaseGoods": $("#PurchaseGoods").val() },
                url: "/Systemmanagement/PurchaseOrderSetting/GetPurchaseOrderSettingListDatas"   //获取数据源的路径
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
                columnsResize: true,
                columns: [
                    { text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '配置供应商', datafield: 'Setting', hidden: false, width: 90, align: 'center', cellsAlign: 'center', cellsRenderer: cellsSettingRenderer },
                    //{ text: '配置部门', datafield: 'SettingDepartment', hidden: false, width: 70, align: 'center', cellsAlign: 'center', cellsRenderer: cellsSettingDepartmentRenderer },
                    //{ text: '配置资产管理公司', datafield: 'SettingAssetManagementCompany', hidden: false, width: 120, align: 'center', cellsAlign: 'center', cellsRenderer: cellsSettingAssetManagementCompany},
                    { text: '采购物品', datafield: 'PurchaseGoods', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '资产主类', datafield: 'AssetCategoryMajor', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '资产子类', datafield: 'AssetCategoryMinor', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '创建时间', datafield: 'CreateDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CreateUser', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '修改时间', datafield: 'ChangeDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'ChangeUser', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
        selector.$grid().on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row index.
            window.location.href = "/Systemmanagement/PurchaseOrderSettingDetail/Index?VGUID=" + row.VGUID;
        });
    }

    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
    }
    function cellsSettingRenderer(row, column, value, rowData) {
        var vguid = rowData.VGUID;
        return '<div style="margin: 8px; margin-top:6px;"><a style="cursor:pointer"  onclick="Setting(\'' + vguid + '\')" id="' + vguid + '">配置</a></div>';
    }
    function cellsSettingDepartmentRenderer(row, column, value, rowData) {
        var vguid = rowData.VGUID;
        return '<div style="margin: 8px; margin-top:6px;"><a style="cursor:pointer"  onclick="cellsSettingDepartment(\'' + vguid + '\')" id="' + vguid + '">配置</a></div>';
    }
    function cellsSettingAssetManagementCompany(row, column, value, rowData) {
        var vguid = rowData.VGUID;
        return '<div style="margin: 8px; margin-top:6px;"><a style="cursor:pointer"  onclick="cellsSettingAssetManagementCompany(\'' + vguid + '\')" id="' + vguid + '">配置</a></div>';
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
function cellsSettingAssetManagementCompany(vguid) {
    $("#PurchaseOrderSettingVguid").val(vguid);
    var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'CompanyOrPerson', type: 'string' },
                    { name: 'BankAccount', type: 'string' },
                    { name: 'BankAccountName', type: 'string' },
                    { name: 'Bank', type: 'string' },
                    { name: 'BankNo', type: 'BankNo' },
                    { name: 'VGUID', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "VGUID": vguid },
                url: "/Systemmanagement/PurchaseOrderSetting/GetPurchaseSupplierListDatas"   //获取数据源的路径
            };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    debugger;
    selector.$settinggrid().jqxDataTable(
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
            columnsResize: true,
            columns: [
                //{ text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                { text: '供应商类别', datafield: 'CompanyOrPerson', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '账号', datafield: 'BankAccount', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '户名', datafield: 'BankAccountName', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '开户行', datafield: 'Bank', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '行号', datafield: 'BankNo', width: 200, align: 'center', cellsAlign: 'center' },
                { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ]
        });
    $("#SettingModalDialog").modal("show");
}
function cellsSettingDepartment(vguid) {
    $("#PurchaseOrderSettingVguid").val(vguid);
    var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Descrption', type: 'string' },
                    { name: 'VGUID', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "VGUID": vguid },
                url: "/Systemmanagement/PurchaseOrderSetting/GetDepartmentListDatas"   //获取数据源的路径
            };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    debugger;
    selector.$settingdepgrid().jqxDataTable(
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
            columnsResize: true,
            columns: [
                { text: '账号', datafield: 'Descrption', width: 200, align: 'center', cellsAlign: 'center' },
                { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ]
        });
    $("#DepartmentModalDialog").modal("show");
}
function Setting(vguid) {
    $("#PurchaseOrderSettingVguid").val(vguid);
    var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'CompanyOrPerson', type: 'string' },
                    { name: 'BankAccount', type: 'string' },
                    { name: 'BankAccountName', type: 'string' },
                    { name: 'Bank', type: 'string' },
                    { name: 'BankNo', type: 'BankNo' },
                    { name: 'VGUID', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "VGUID": vguid },
                url: "/Systemmanagement/PurchaseOrderSetting/GetPurchaseSupplierListDatas"   //获取数据源的路径
            };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    debugger;
    selector.$settinggrid().jqxDataTable(
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
            columnsResize: true,
            columns: [
                //{ text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                { text: '供应商类别', datafield: 'CompanyOrPerson', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '账号', datafield: 'BankAccount', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '户名', datafield: 'BankAccountName', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '开户行', datafield: 'Bank', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '行号', datafield: 'BankNo', width: 200, align: 'center', cellsAlign: 'center' },
                { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ]
        });
    $("#SettingModalDialog").modal("show");
}
function InitBankCategorySelect() {
    var url = "/Systemmanagement/PurchaseOrderSetting/GetBankCategoryListDatas";
    // prepare the data
    var source =
    {
        datatype: "json",
        datafields: [
            { name: 'CompanyOrPerson' }
        ],
        url: url,
        async: false
    };
    var dataAdapter = new $.jqx.dataAdapter(source);
    $("#BankCategory").jqxDropDownList({
        selectedIndex: 2, source: dataAdapter, displayMember: "CompanyOrPerson", valueMember: "CompanyOrPerson", width: 200, height: 30,
    });
    $("#BankCategory").jqxDropDownList({ placeHolder: "请选择" });
}
$(function () {
    var page = new $page();
    page.init();
});
