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
        //if (status == "1") {
        //    $("#buttonList").show();
        //}
        //if (status == "2") {
        //    $("#buttonList2").show();
        //}
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
                    { name: 'GROUP_ID', type: 'string' },
                    { name: 'ORGANIZATION_NUM', type: 'string' },
                    { name: 'ENGINE_NUMBER', type: 'string' },
                    { name: 'CHASSIS_NUMBER', type: 'string' },
                    { name: 'BOOK_TYPE_CODE', type: 'string' },
                    { name: 'TAG_NUMBER', type: 'string' },
                    { name: 'DESCRIPTION', type: 'string' },
                    { name: 'QUANTITY', type: 'number' },
                    { name: 'ASSET_CATEGORY_MAJOR', type: 'string' },
                    { name: 'ASSET_CATEGORY_MINOR', type: 'string' },
                    { name: 'ASSET_CREATION_DATE', type: 'date' },
                    { name: 'ASSET_COST', type: 'float' },
                    { name: 'SALVAGE_TYPE', type: 'string' },
                    { name: 'SALVAGE_PERCENT', type: 'float' },
                    { name: 'SALVAGE_VALUE', type: 'float' },
                    { name: 'YTD_DEPRECIATION', type: 'float' },
                    { name: 'ACCT_DEPRECIATION', type: 'string' },
                    { name: 'METHOD', type: 'string' },
                    { name: 'LIFE_MONTHS', type: 'number' },
                    { name: 'AMORTIZATION_FLAG', type: 'string' },
                    { name: 'EXP_ACCOUNT_SEGMENT1', type: 'float' },
                    { name: 'EXP_ACCOUNT_SEGMENT2', type: 'float' },
                    { name: 'EXP_ACCOUNT_SEGMENT3', type: 'float' },
                    { name: 'EXP_ACCOUNT_SEGMENT4', type: 'float' },
                    { name: 'EXP_ACCOUNT_SEGMENT5', type: 'float' },
                    { name: 'EXP_ACCOUNT_SEGMENT6', type: 'float' },
                    { name: 'EXP_ACCOUNT_SEGMENT7', type: 'float' },
                    { name: 'FA_LOC_1', type: 'string' },
                    { name: 'FA_LOC_2', type: 'string' },
                    { name: 'FA_LOC_3', type: 'string' },
                    { name: 'RETIRE_FLAG', type: 'string' },
                    { name: 'RETIRE_QUANTITY', type: 'number' },
                    { name: 'RETIRE_COST', type: 'float' },
                    { name: 'RETIRE_DATE', type: 'date' },
                    { name: 'TRANSACTION_ID', type: 'number' },
                    { name: 'LAST_UPDATE_DATE', type: 'date' },
                    { name: 'LISENSING_FEE', type: 'float' },
                    { name: 'OUT_WAREHOUSE_FEE', type: 'float' },
                    { name: 'DOME_LIGHT_FEE', type: 'float' },
                    { name: 'ANTI_ROBBERY_FEE', type: 'float' },
                    { name: 'LOADING_FEE', type: 'float' },
                    { name: 'INNER_ROOF_FEE', type: 'float' },
                    { name: 'TAXIMETER_FEE', type: 'float' },
                    { name: 'OBD_FEE', type: 'float' },
                    { name: 'CREATE_DATE', type: 'date' },
                    { name: 'CHANGE_DATE', type: 'date' },
                    { name: 'CREATE_USER', type: 'string' },
                    { name: 'CHANGE_USER', type: 'string' },
                    { name: 'STATUS', type: 'string' },
                    { name: 'VGUID', type: 'string' },
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
                    { text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'GroupID', datafield: 'GROUP_ID', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '组织编号', datafield: 'ORGANIZATION_NUM', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '发动机号', datafield: 'ENGINE_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '车架号', datafield: 'CHASSIS_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产账簿', datafield: 'BOOK_TYPE_CODE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '标签号', datafield: 'TAG_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '说明', datafield: 'DESCRIPTION', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '数量', datafield: 'QUANTITY', width: 50, align: 'center', cellsAlign: 'center' },
                    { text: '资产主类', datafield: 'ASSET_CATEGORY_MAJOR', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产次类', datafield: 'ASSET_CATEGORY_MINOR', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '启用日期', datafield: 'ASSET_CREATION_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '当前成本', datafield: 'ASSET_COST', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '残值类型', datafield: 'SALVAGE_TYPE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '残值百分比', datafield: 'SALVAGE_PERCENT', width: 100, align: 'center', cellsAlign: 'center', cellsrenderer: cellsrenderer },
                    { text: '残值金额', datafield: 'SALVAGE_VALUE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: 'YTD折旧', datafield: 'YTD_DEPRECIATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '累计折旧', datafield: 'ACCT_DEPRECIATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '折旧方法', datafield: 'METHOD', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '使用寿命', datafield: 'LIFE_MONTHS', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '摊销标记', datafield: 'AMORTIZATION_FLAG', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '费用账户组合段1', datafield: 'EXP_ACCOUNT_SEGMENT1', width: 140, align: 'center', cellsAlign: 'center' },
                    { text: '费用账户组合段2', datafield: 'EXP_ACCOUNT_SEGMENT2', width: 140, align: 'center', cellsAlign: 'center' },
                    { text: '费用账户组合段3', datafield: 'EXP_ACCOUNT_SEGMENT3', width: 140, align: 'center', cellsAlign: 'center' },
                    { text: '费用账户组合段4', datafield: 'EXP_ACCOUNT_SEGMENT4', width: 140, align: 'center', cellsAlign: 'center' },
                    { text: '费用账户组合段5', datafield: 'EXP_ACCOUNT_SEGMENT5', width: 140, align: 'center', cellsAlign: 'center' },
                    { text: '费用账户组合段6', datafield: 'EXP_ACCOUNT_SEGMENT6', width: 140, align: 'center', cellsAlign: 'center' },
                    { text: '费用账户组合段7', datafield: 'EXP_ACCOUNT_SEGMENT7', width: 140, align: 'center', cellsAlign: 'center' },
                    { text: '存放地点1', datafield: 'FA_LOC_1', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '存放地点2', datafield: 'FA_LOC_2', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '存放地点3', datafield: 'FA_LOC_3', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '报废标识', datafield: 'RETIRE_FLAG', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '报废数量', datafield: 'RETIRE_QUANTITY', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '报废成本', datafield: 'RETIRE_COST', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '报废日期', datafield: 'RETIRE_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '事务处理标识', datafield: 'TRANSACTION_ID', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '最后更新时间', datafield: 'LAST_UPDATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '上牌费', datafield: 'LISENSING_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '出库费', datafield: 'OUT_WAREHOUSE_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '顶灯费', datafield: 'DOME_LIGHT_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '防劫费', datafield: 'ANTI_ROBBERY_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '装车费', datafield: 'LOADING_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '内顶费', datafield: 'INNER_ROOF_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '计价器', datafield: 'TAXIMETER_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: 'OBD', datafield: 'OBD_FEE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '创建人', datafield: 'CREATE_USER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'CHANGE_USER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '修改日期', datafield: 'CHANGE_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '处理状态', datafield: 'STATUS', width: 100, align: 'center', cellsAlign: 'center' },
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
        return '<span style="margin: 4px; margin-top:8px;">' + value + '%</span>';
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

$(function () {
    var page = new $page();
    page.init();
});
