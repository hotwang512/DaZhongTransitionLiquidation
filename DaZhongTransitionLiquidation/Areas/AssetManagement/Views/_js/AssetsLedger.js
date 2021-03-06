﻿//资产基础信息维护列表
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $gridCompare: function () { return $("#jqxCompareTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") },
    $btnCheckNumber: function () { return $("#btnCheckNumber") },
    $btnCheckData: function () { return $("#btnCheckData") },
    $btnExport: function () { return $("#btnExport") }
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
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#TagNumber").val("");
            $("#CategoryMajor").val("");
            $("#CategoryMinor").val("");
            $("#PERIOD").val("");
        });
        selector.$btnExport().on("click",
            function() {
                window.location.href = "/AssetManagement/AssetsLedger/ExportExcel?StartDate=" +
                    $("#PERIOD").val() +
                    "&TagNumber=" +
                    $("#TagNumber").val() +
                    "&CategoryMajor=" +
                    $("#CategoryMajor").val() +
                    "&CategoryMinor=" +
                    $("#CategoryMinor").val();
            }
        );
        selector.$btnCheckNumber().on("click",
            function () {
                $.ajax({
                    url: "/AssetManagement/AssetsLedger/CheckNumber",
                    type: "POST",
                    dataType: "json",
                    async: false,
                    success: function (msg) {
                        switch (msg.Status) {
                        case "0":
                            jqxNotification(msg.ResultInfo, null, "error");
                            break;
                        case "1":
                            jqxNotification("数量一致！", null, "success");
                            $("#jqxTable").jqxGrid('updateBoundData');
                            $('#jqxTable').jqxGrid('clearselection');
                            break;
                        }
                    }
                });
            });
        selector.$btnCheckData().on("click",
            function () {
                layer.load();
                $.ajax({
                    url: "/AssetManagement/AssetsLedger/CheckData",
                    type: "POST",
                    dataType: "json",
                    async: false,
                    success: function (msg) {
                        layer.closeAll('loading');
                        switch (msg.Status) {
                        case "0":
                            jqxNotification("对比失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification(msg.ResultInfo, null, "error");
                            initCompareTable(msg.ResultInfo2);
                            break;
                        case "2":
                            jqxNotification(msg.ResultInfo, null, "error");
                            initCompareTable(msg.ResultInfo2);
                            break;
                        case "3":
                            jqxNotification("对比成功！", null, "success");
                            break;
                        }
                    }
                });
            });
    }; //addEvent end

    function initTable() {
        selector.$gridCompare().hide();
        selector.$grid().show();
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
        var source =
            {
                datafields:
                [
                    { name: 'BOOK_TYPE_CODE', type: 'string' },
                    { name: 'PERIOD_CODE', type: 'string' },
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
                    { name: 'PTD_DEPRECIATION', type: 'float' },
                    { name: 'YTD_DEPRECIATION', type: 'float' },
                    { name: 'ACCT_DEPRECIATION', type: 'string' },
                    { name: 'METHOD', type: 'string' },
                    { name: 'LIFE_MONTHS', type: 'number' },
                    { name: 'EXP_ACCOUNT', type: 'float' },
                    { name: 'FA_LOC_1', type: 'string' },
                    { name: 'FA_LOC_2', type: 'string' },
                    { name: 'FA_LOC_3', type: 'string' },
                    { name: 'LAST_UPDATE_DATE', type: 'date' },
                    { name: 'ASSET_ID', type: 'string' },
                    { name: 'CREATE_DATE', type: 'date' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "PERIOD": $("#PERIOD").val(), "TagNumber": $("#TagNumber").val(), "CategoryMajor": $("#CategoryMajor").val(), "CategoryMinor": $("#CategoryMinor").val() },
                url: "/AssetManagement/AssetsLedger/GetAssetsLedgerListDatas"   //获取数据源的路径
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
                    { text: '资产账簿', datafield: 'BOOK_TYPE_CODE', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '期间', datafield: 'PERIOD_CODE', width: 80, align: 'center', cellsAlign: 'center' },
                    { text: '标签号', datafield: 'TAG_NUMBER', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '说明', datafield: 'DESCRIPTION', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '数量', datafield: 'QUANTITY', width: 50, align: 'center', cellsAlign: 'center' },
                    { text: '资产主类', datafield: 'ASSET_CATEGORY_MAJOR', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产次类', datafield: 'ASSET_CATEGORY_MINOR', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '启用日期', datafield: 'ASSET_CREATION_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '当前成本', datafield: 'ASSET_COST', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '残值类型', datafield: 'SALVAGE_TYPE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '残值百分比', datafield: 'SALVAGE_PERCENT', width: 100, align: 'center', cellsAlign: 'center', cellsrenderer: cellsrenderer },
                    { text: '残值金额', datafield: 'SALVAGE_VALUE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '本月折旧额', datafield: 'PTD_DEPRECIATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: 'YTD折旧', datafield: 'YTD_DEPRECIATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '累计折旧', datafield: 'ACCT_DEPRECIATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '折旧方法', datafield: 'METHOD', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '使用寿命', datafield: 'LIFE_MONTHS', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '费用账户组合段', datafield: 'EXP_ACCOUNT', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '存放地点1', datafield: 'FA_LOC_1', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '存放地点2', datafield: 'FA_LOC_2', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '存放地点3', datafield: 'FA_LOC_3', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产编号', datafield: 'ASSET_ID', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '最后更新时间', datafield: 'LAST_UPDATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" }
                ]
            });
    }
  
    function cellsrenderer(row, column, value, rowData) {
        if (value != "") {
            return '<span style="margin: 4px; margin-top:8px;">' + value + '%</span>';
        } else {
            return value;
        }
    }

    function initCompareTable(CompareData) {
        selector.$grid().hide();
        selector.$gridCompare().show();
        var source =
            {
                datafields:
                [
                    { name: 'BOOK_TYPE_CODE', type: 'string' },
                    { name: 'PERIOD_CODE', type: 'string' },
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
                    { name: 'PTD_DEPRECIATION', type: 'float' },
                    { name: 'YTD_DEPRECIATION', type: 'float' },
                    { name: 'ACCT_DEPRECIATION', type: 'string' },
                    { name: 'METHOD', type: 'string' },
                    { name: 'LIFE_MONTHS', type: 'number' },
                    { name: 'EXP_ACCOUNT', type: 'float' },
                    { name: 'FA_LOC_1', type: 'string' },
                    { name: 'FA_LOC_2', type: 'string' },
                    { name: 'FA_LOC_3', type: 'string' },
                    { name: 'LAST_UPDATE_DATE', type: 'date' },
                    { name: 'ASSET_ID', type: 'string' },
                    { name: 'Message', type: 'string' },
                    { name: 'CREATE_DATE', type: 'date' }
                ],
                localData: CompareData,
                datatype: "json",
                id: "VGUID"
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (CompareData) {
                source.totalrecords = CompareData.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$gridCompare().jqxDataTable(
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
                    { text: '资产账簿', datafield: 'BOOK_TYPE_CODE', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '期间', datafield: 'PERIOD_CODE', width: 80, align: 'center', cellsAlign: 'center' },
                    { text: '标签号', datafield: 'TAG_NUMBER', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '说明', datafield: 'DESCRIPTION', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '数量', datafield: 'QUANTITY', width: 50, align: 'center', cellsAlign: 'center' },
                    { text: '资产主类', datafield: 'ASSET_CATEGORY_MAJOR', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产次类', datafield: 'ASSET_CATEGORY_MINOR', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '启用日期', datafield: 'ASSET_CREATION_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '当前成本', datafield: 'ASSET_COST', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '残值类型', datafield: 'SALVAGE_TYPE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '残值百分比', datafield: 'SALVAGE_PERCENT', width: 100, align: 'center', cellsAlign: 'center', cellsrenderer: cellsrenderer },
                    { text: '残值金额', datafield: 'SALVAGE_VALUE', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '本月折旧额', datafield: 'PTD_DEPRECIATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: 'YTD折旧', datafield: 'YTD_DEPRECIATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '累计折旧', datafield: 'ACCT_DEPRECIATION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '折旧方法', datafield: 'METHOD', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '使用寿命', datafield: 'LIFE_MONTHS', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '费用账户组合段', datafield: 'EXP_ACCOUNT', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '存放地点1', datafield: 'FA_LOC_1', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '存放地点2', datafield: 'FA_LOC_2', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '存放地点3', datafield: 'FA_LOC_3', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产编号', datafield: 'ASSET_ID', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '错误信息', datafield: 'Message', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '最后更新时间', datafield: 'LAST_UPDATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" }
                ]
            });
    }
};

$(function () {
    var page = new $page();
    page.init();
});
