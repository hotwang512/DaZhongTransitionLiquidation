﻿//资产基础信息维护列表
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
            $("#CategoryMajor").val("");
            $("#CategoryMinor").val("");
        });
        //新增
        $("#btnAdd").on("click", function () {
            window.location.href = "/AssetManagement/AssetBasicInfoMaintenanceDetail/Index";
            //window.open("/CapitalCenterManagement/OrderListDetail/Index");
        });
        //删除
        $("#btnDelete").on("click", function () {
            var selection = [];
            var grid = $("#jqxTable");
            debugger;
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
            url: "/AssetManagement/AssetBasicInfoMaintenance/DeleteAssetBasicInfo",
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
                    { name: 'ASSET_CATEGORY_MAJOR', type: 'string' },
                    { name: 'ASSET_CATEGORY_MINOR', type: 'string' },
                    { name: 'LIFE_YEARS', type: 'number' },
                    { name: 'LIFE_MONTHS', type: 'number' },
                    { name: 'SALVAGE_PERCENT', type: 'float' },
                    { name: 'METHOD', type: 'string' },
                    { name: 'BOOK_TYPE_CODE', type: 'string' },
                    { name: 'ASSET_COST_ACCOUNT', type: 'string' },
                    { name: 'ASSET_SETTLEMENT_ACCOUNT', type: 'string' },
                    { name: 'DEPRECIATION_EXPENSE_SEGMENT', type: 'string' },
                    { name: 'ACCT_DEPRECIATION_ACCOUNT', type: 'string' },
                    { name: 'CREATE_TIME', type: 'date' },
                    { name: 'CREATE_USER', type: 'string' },
                    { name: 'CHANGE_TIME', type: 'date' },
                    { name: 'CHANGE_USER', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "ASSET_CATEGORY_MAJOR": $("#CategoryMajor").val(), "ASSET_CATEGORY_MINOR": $("#CategoryMinor").val() },
                url: "/AssetManagement/AssetBasicInfoMaintenance/GetAssetBasicInfoListDatas"   //获取数据源的路径
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
                    { text: '资产主类', datafield: 'ASSET_CATEGORY_MAJOR', width: 150, align: 'center', cellsAlign: 'center' , cellsRenderer: detailFunc},
                    { text: '资产子类', datafield: 'ASSET_CATEGORY_MINOR', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '使用年限(年)', datafield: 'LIFE_YEARS', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '使用年限(月)', datafield: 'LIFE_MONTHS', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '残值率(%)', datafield: 'SALVAGE_PERCENT', width: 90, align: 'center', cellsAlign: 'center', cellsRenderer: cellsrenderer },
                    { text: '折旧方法', datafield: 'METHOD', width: 90, align: 'center', cellsAlign: 'center' },
                    { text: '帐簿', datafield: 'BOOK_TYPE_CODE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产成本帐户', datafield: 'ASSET_COST_ACCOUNT', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产结算帐户', datafield: 'ASSET_SETTLEMENT_ACCOUNT', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '折旧费用段', datafield: 'DEPRECIATION_EXPENSE_SEGMENT', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '累计折旧帐户', datafield: 'ACCT_DEPRECIATION_ACCOUNT', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '创建时间', datafield: 'CREATE_TIME', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CREATE_USER', width: 100, align: 'center', cellsAlign: 'center', },
                    { text: '修改时间', datafield: 'CHANGE_TIME', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'CHANGE_USER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
        selector.$grid().on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row index.
            window.location.href = "/AssetManagement/AssetBasicInfoMaintenanceDetail/Index?VGUID=" + row.VGUID;
        });
    }
    function detailFunc(row, column, value, rowData) {
        var container = "";
        container = "<a href='#' onclick=link('" + rowData.VGUID + "') style=\"text-decoration: underline;color: #333;\">" + rowData.ASSET_CATEGORY_MAJOR + "</a>";
        return container;
    }
    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
    }
    function cellsrenderer(row, column, value, rowData) {
        return '<span style="margin: 4px; margin-top:8px;">' + value + '%</span>';
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

function link(VGUID) {
    debugger;
    window.location.href = "/AssetManagement/AssetBasicInfoMaintenanceDetail/Index?VGUID=" + VGUID;
}
$(function () {
    var page = new $page();
    page.init();
});
