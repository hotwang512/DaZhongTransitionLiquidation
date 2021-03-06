﻿//资产基础信息维护列表
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") },
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
            function () {
                window.location.href = "/AssetManagement/AssetsRetirement/ExportExcel?PERIOD=" +
                    $("#PERIOD").val() +
                    "&TagNumber=" +
                    $("#TagNumber").val() +
                    "&CategoryMajor=" +
                    $("#CategoryMajor").val() +
                    "&CategoryMinor=" +
                    $("#CategoryMinor").val();
            }
        );
    }; //addEvent end

    function initTable() {
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
        var source =
            {
                datafields:
                [
                    { name: 'VGUID', type: 'string' },
                    { name: 'BOOK_TYPE_CODE', type: 'string' },
                    { name: 'PERIOD', type: 'string' },
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
                    { name: 'METHOD', type: 'string' },
                    { name: 'LIFE_MONTHS', type: 'number' },
                    { name: 'RETIRE_QUANTITY', type: 'number' },
                    { name: 'RETIRE_COST', type: 'float' },
                    { name: 'RETIRE_DATE', type: 'date' },
                    { name: 'RETIRE_ACCT_DEPRECIATION', type: 'string' },
                    { name: 'RETIRE_PL', type: 'float' },
                    { name: 'LAST_UPDATE_DATE', type: 'date' },
                    { name: 'CREATE_DATE', type: 'date' },
                    { name: 'ASSET_ID', type: 'string' },
                    { name: 'DISPOSA_TYPE', type: 'string' },
                    { name: 'DISPOSA_AMOUNT', type: 'float' },
                    { name: 'DISPOSAL_TAX', type: 'float' },
                    { name: 'DISPOSAL_PL', type: 'float' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "PERIOD": $("#PERIOD").val(), "TagNumber": $("#TagNumber").val(), "CategoryMajor": $("#CategoryMajor").val(), "CategoryMinor": $("#CategoryMinor").val() },
                url: "/AssetManagement/AssetsRetirement/GetAssetsRetirementListDatas"   //获取数据源的路径
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
                    { text: '期间', datafield: 'PERIOD', width: 80, align: 'center', cellsAlign: 'center' },
                    { text: '标签号', datafield: 'TAG_NUMBER', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '说明', datafield: 'DESCRIPTION', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '数量', datafield: 'QUANTITY', width: 50, align: 'center', cellsAlign: 'center' },
                    { text: '资产主类', datafield: 'ASSET_CATEGORY_MAJOR', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产次类', datafield: 'ASSET_CATEGORY_MINOR', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '启用日期', datafield: 'ASSET_CREATION_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '当前成本', datafield: 'ASSET_COST', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '残值类型', datafield: 'SALVAGE_TYPE', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '残值百分比', datafield: 'SALVAGE_PERCENT', width: 150, align: 'center', cellsAlign: 'center', cellsrenderer: cellsrenderer },
                    { text: '残值金额', datafield: 'SALVAGE_VALUE', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '折旧方法', datafield: 'METHOD', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '使用寿命', datafield: 'LIFE_MONTHS', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '报废数量', datafield: 'RETIRE_QUANTITY', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '报废成本', datafield: 'RETIRE_COST', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '报废日期', datafield: 'RETIRE_DATE', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '报废累计折旧', datafield: 'RETIRE_ACCT_DEPRECIATION', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产处置方式', datafield: 'DISPOSA_TYPE', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产处置金额', datafield: 'DISPOSA_AMOUNT', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产处置税金', datafield: 'DISPOSAL_TAX', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产处置损益', datafield: 'DISPOSAL_PL', width: 150, align: 'center', cellsAlign: 'center' },
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
};

$(function () {
    var page = new $page();
    page.init();
});
