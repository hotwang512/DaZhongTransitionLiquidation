//资产维护
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
        addEvent();
    }
    //var status = $.request.queryString().Status;
    //所有事件
    function addEvent() {
        debugger;
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#TagNumber").val("");
            $("#CategoryMajor").val("");
            $("#CategoryMinor").val("");
            $("#Status").val("");
        });
        //新增
        $("#btnAdd").on("click", function () {
            window.location.href = "/AssetManagement/AssetMaintenanceInfoDetail/Index";
            //window.open("/AssetManagement/AssetMaintenanceInfoDetail/Index");
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
    }; //addEvent end

    //删除
    function dele(selection) {
        $.ajax({
            url: "/AssetManagement/AssetsMaintenance/DeleteAssetMaintenanceInfo",
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
    //提交
    function submit(selection) {
        $.ajax({
            url: "/AssetManagement/AssetBasicInfoMaintenance/UpdataAssetBasicInfo",
            data: { vguids: selection, status: "2" },
            //traditional: true,
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                    case "0":
                        jqxNotification("提交失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("提交成功！", null, "success");
                        $("#jqxTable").jqxDataTable('updateBoundData');
                        break;
                }
            }
        });
    }

    function initTable() {
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'GROUP_ID', type: 'string' },
                    { name: 'ASSET_ID', type: 'string' },
                    { name: 'PLATE_NUMBER', type: 'string' },
                    { name: 'TAG_NUMBER', type: 'string' },
                    { name: 'VEHICLE_SHORTNAME', type: 'string' },
                    { name: 'ORGANIZATION_NUM', type: 'string' },
                    { name: 'MANAGEMENT_COMPANY', type: 'string' },
                    { name: 'BELONGTO_COMPANY', type: 'string' },
                    { name: 'ASSET_ID', type: 'string' },
                    { name: 'VEHICLE_STATE', type: 'string' },
                    { name: 'OPERATING_STATE', type: 'string' },
                    { name: 'DESCRIPTION', type: 'string' },
                    { name: 'ENGINE_NUMBER', type: 'string' },
                    { name: 'CHASSIS_NUMBER', type: 'string' },
                    { name: 'PRODUCTION_DATE', type: 'date' },
                    { name: 'PURCHASE_DATE', type: 'date' },
                    { name: 'LISENSING_DATE', type: 'date' },
                    { name: 'COMMISSIONING_DATE', type: 'date' },
                    { name: 'VEHICLE_AGE', type: 'float' },
                    { name: 'BACK_CAR_DATE', type: 'date' },
                    { name: 'FUEL_TYPE', type: 'string' },
                    { name: 'DELIVERY_INFORMATION', type: 'string' },
                    { name: 'QUANTITY', type: 'number' },
                    { name: 'ASSET_COST', type: 'float' },
                    { name: 'NUDE_CAR_FEE', type: 'float' },
                    { name: 'PURCHASE_TAX', type: 'float' },
                    { name: 'LISENSING_FEE', type: 'float' },
                    { name: 'OUT_WAREHOUSE_FEE', type: 'float' },
                    { name: 'DOME_LIGHT_FEE', type: 'float' },
                    { name: 'ANTI_ROBBERY_FEE', type: 'float' },
                    { name: 'LOADING_FEE', type: 'float' },
                    { name: 'INNER_ROOF_FEE', type: 'float' },
                    { name: 'TAXIMETER_FEE', type: 'float' },
                    { name: 'ASSET_DISPOSITION_TYPE', type: 'string' },
                    { name: 'SCRAP_INFORMATION', type: 'string' },
                    { name: 'DISPOSAL_AMOUNT', type: 'float' },
                    { name: 'DISPOSAL_TAX', type: 'float' },
                    { name: 'DISPOSAL_PROFIT_LOSS', type: 'float' },
                    { name: 'BAK_CAR_AGE', type: 'float' },
                    { name: 'ASSET_CATEGORY_MAJOR', type: 'string' },
                    { name: 'ASSET_CATEGORY_MINOR', type: 'string' },
                    { name: 'LIFE_YEARS', type: 'number' },
                    { name: 'LIFE_MONTHS', type: 'number' },
                    { name: 'SALVAGE_TYPE', type: 'string' },
                    { name: 'SALVAGE_PERCENT', type: 'float' },
                    { name: 'SALVAGE_VALUE', type: 'float' },
                    { name: 'AMORTIZATION_FLAG', type: 'string' },
                    { name: 'METHOD', type: 'string' },
                    { name: 'BOOK_TYPE_CODE', type: 'string' },
                    { name: 'ASSET_COST_ACCOUNT', type: 'string' },
                    { name: 'ASSET_SETTLEMENT_ACCOUNT', type: 'string' },
                    { name: 'DEPRECIATION_EXPENSE_SEGMENT', type: 'string' },
                    { name: 'ACCT_DEPRECIATION_ACCOUNT', type: 'string' },
                    { name: 'YTD_DEPRECIATION', type: 'float' },
                    { name: 'ACCT_DEPRECIATION', type: 'float' },
                    { name: 'EXP_ACCOUNT_SEGMENT', type: 'string' },
                    { name: 'MODEL_MAJOR', type: 'string' },
                    { name: 'MODEL_MINOR', type: 'string' },
                    { name: 'START_VEHICLE_DATE', type: 'string' },
                    { name: 'CREATE_DATE', type: 'date' },
                    { name: 'CHANGE_DATE', type: 'date' },
                    { name: 'CREATE_USER', type: 'string' },
                    { name: 'CHANGE_USER', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "TAG_NUMBER": $("#TagNumber").val(), "ASSET_CATEGORY_MAJOR": $("#CategoryMajor").val(), "ASSET_CATEGORY_MINOR": $("#CategoryMinor").val(), "STATUS": $("#Status").val() },
                url: "/AssetManagement/AssetsMaintenance/GetAssetMaintenanceInfoListDatas"   //获取数据源的路径
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
                    { text: "", datafield: "checkbox", width: 35, pinned: true, hidden: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'GroupID', datafield: 'GROUP_ID', width: 100, hidden: true, align: 'center', cellsAlign: 'center' },
                    { text: '资产编号', datafield: 'ASSET_ID', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '车牌号', datafield: 'PLATE_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '标签号', datafield: 'TAG_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '车辆简称', datafield: 'VEHICLE_SHORTNAME', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '组织编号', datafield: 'ORGANIZATION_NUM', hidden: true, width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '管理公司', datafield: 'MANAGEMENT_COMPANY', width: 160, align: 'center', cellsAlign: 'center' },
                    { text: '资产所属公司', datafield: 'BELONGTO_COMPANY', width: 160, align: 'center', cellsAlign: 'center' },
                    { text: '车辆状态', datafield: 'VEHICLE_STATE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '营运状态', datafield: 'OPERATING_STATE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产说明', datafield: 'DESCRIPTION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '发动机号', datafield: 'ENGINE_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '车架号', datafield: 'CHASSIS_NUMBER', width: 120, align: 'center', cellsAlign: 'center' },
                    { text: '出厂日期', datafield: 'PRODUCTION_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '购买日期', datafield: 'PURCHASE_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '上牌日期', datafield: 'LISENSING_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '投产日期', datafield: 'COMMISSIONING_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '车龄', datafield: 'VEHICLE_AGE', width: 50, align: 'center', cellsAlign: 'center' },
                    { text: '退车日期', datafield: 'BACK_CAR_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '燃料种类', datafield: 'FUEL_TYPE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '排量信息', datafield: 'DELIVERY_INFORMATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产数量', datafield: 'QUANTITY', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产原值', datafield: 'ASSET_COST', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '裸车价', datafield: 'NUDE_CAR_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '购置税', datafield: 'PURCHASE_TAX', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '上牌费', datafield: 'LISENSING_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '出库费', datafield: 'OUT_WAREHOUSE_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '顶灯费', datafield: 'DOME_LIGHT_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '防劫费', datafield: 'ANTI_ROBBERY_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '装车费', datafield: 'LOADING_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '内顶费', datafield: 'INNER_ROOF_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '计价器', datafield: 'TAXIMETER_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产处置方式', datafield: 'ASSET_DISPOSITION_TYPE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: 'Oracle报废信息', datafield: 'SCRAP_INFORMATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产处置金额', datafield: 'DISPOSAL_AMOUNT', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产处置税金', datafield: 'DISPOSAL_TAX', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产处置损益', datafield: 'DISPOSAL_PROFIT_LOSS', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '退车车龄', datafield: 'BAK_CAR_AGE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产主类', datafield: 'ASSET_CATEGORY_MAJOR', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产次类', datafield: 'ASSET_CATEGORY_MINOR', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '使用年限(年)', datafield: 'LIFE_YEARS', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '使用年限(月)', datafield: 'LIFE_MONTHS', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '残值类型', datafield: 'SALVAGE_TYPE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '残值百分比', datafield: 'SALVAGE_PERCENT', width: 100, align: 'center', cellsAlign: 'center', cellsrenderer: cellsrenderer },
                    { text: '残值金额', datafield: 'SALVAGE_VALUE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '摊销标记', datafield: 'AMORTIZATION_FLAG', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '折旧方法', datafield: 'METHOD', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产账簿', datafield: 'BOOK_TYPE_CODE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产成本帐户', datafield: 'ASSET_COST_ACCOUNT', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产结算帐户', datafield: 'ASSET_SETTLEMENT_ACCOUNT', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '折旧费用段', datafield: 'DEPRECIATION_EXPENSE_SEGMENT', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '累计折旧帐户', datafield: 'ACCT_DEPRECIATION_ACCOUNT', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'YTD折旧', datafield: 'YTD_DEPRECIATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '累计折旧', datafield: 'ACCT_DEPRECIATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '总账帐簿', datafield: 'EXP_ACCOUNT_SEGMENT', width: 180, align: 'center', cellsAlign: 'center' },
                    { text: '经营模式主类', datafield: 'MODEL_MAJOR', width: 180, align: 'center', cellsAlign: 'center' },
                    { text: '模式子类', datafield: 'MODEL_MINOR', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '发车月份', datafield: 'START_VEHICLE_DATE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '创建人', datafield: 'CREATE_USER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'CHANGE_USER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '修改日期', datafield: 'CHANGE_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
        selector.$grid().on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row index.
            window.location.href = "/AssetManagement/AssetMaintenanceInfoDetail/Index?VGUID=" + row.VGUID;
        });
    }
    function cellsrenderer(row, column, value, rowData) {
        if (value != "") {
            return '<span style="margin: 4px; margin-top:8px;">' + value + '%</span>';
        } else {
            return '';
        }
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
function getBusinessModel() {
    $.ajax({
        url: "/AssetManagement/AssetsMaintenance/GetDepreciationMethods",
        //traditional: true,
        type: "post",
        success: function (msg) {
            switch (msg.Status) {
            case "0":
                jqxNotification("获取失败！", null, "error");
                break;
            case "1":
                jqxNotification("获取成功！", null, "success");
                $("#jqxTable").jqxDataTable('updateBoundData');
                break;
            }
        }
    });
}
$(function () {
    var page = new $page();
    page.init();
});
