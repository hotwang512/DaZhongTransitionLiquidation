//资产基础信息维护列表
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
            $("#StartDate").val("");
            $("#EndDate").val("");
        });
        selector.$btnExport().on("click",
            function () {
                window.location.href = "/AssetManagement/AssetsRetirement/ExportExcel?StartDate=" +
                    $("#StartDate").val() +
                    "&EndDate=" +
                    $("#EndDate").val();
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
                    { name: 'METHOD', type: 'string' },
                    { name: 'LIFE_MONTHS', type: 'number' },
                    { name: 'RETIRE_QUANTITY', type: 'number' },
                    { name: 'RETIRE_COST', type: 'float' },
                    { name: 'RETIRE_DATE', type: 'date' },
                    { name: 'RETIRE_ACCT_DEPRECIATION', type: 'string' },
                    { name: 'RETIRE_PL', type: 'float' },
                    { name: 'LAST_UPDATE_DATE', type: 'date' },
                    { name: 'CREATE_DATE', type: 'date' },
                    { name: 'CHANGE_DATE', type: 'date' },
                    { name: 'CREATE_USER', type: 'string' },
                    { name: 'CHANGE_USER', type: 'string' },
                    { name: 'VGUID', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "StartDate": $("#StartDate").val(), "EndDate": $("#EndDate").val() },
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
                    { text: '期间', datafield: 'PERIOD_CODE', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '标签号', datafield: 'TAG_NUMBER', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '说明', datafield: 'DESCRIPTION', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '数量', datafield: 'QUANTITY', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产主类', datafield: 'ASSET_CATEGORY_MAJOR', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产次类', datafield: 'ASSET_CATEGORY_MINOR', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '启用日期', datafield: 'ASSET_CREATION_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '当前成本', datafield: 'ASSET_COST', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '残值类型', datafield: 'SALVAGE_TYPE', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '残值百分比', datafield: 'SALVAGE_PERCENT', width: 150, align: 'center', cellsAlign: 'center', cellsrenderer: cellsrenderer },
                    { text: '残值金额', datafield: 'SALVAGE_VALUE', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '折旧方法', datafield: 'METHOD', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '使用寿命', datafield: 'LIFE_MONTHS', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '报废数量', datafield: 'RETIRE_QUANTITY', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '报废成本', datafield: 'RETIRE_COST', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '报废日期', datafield: 'RETIRE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '报废累计折旧', datafield: 'RETIRE_ACCT_DEPRECIATION', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '报废损益额', datafield: 'RETIRE_PL', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '存放地点3', datafield: 'FA_LOC_3', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '最后更新时间', datafield: 'LAST_UPDATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改日期', datafield: 'CHANGE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CREATE_USER', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '修改人', datafield: 'CHANGE_USER', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
    }
    function cellsrenderer(row, column, value, rowData) {
        return '<span style="margin: 4px; margin-top:8px;">' + value + '%</span>';
    }
};

$(function () {
    var page = new $page();
    page.init();
});
