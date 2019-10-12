//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $txtDatedTime: function () { return $("#txtDatedTime") },
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
            $("#TradingBank").val("");
            $("#TransactionDate").val("");
            $("#TransactionDateEnd").val("");
            $("#PaymentUnit").val("");
        });
    }; //addEvent end


    function initTable() {
        var source =
            {
                datafields:
                [
                    //{ name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Model', type: 'string' },
                    { name: 'ClassType', type: 'string' },
                    { name: 'CarType', type: 'string' },
                    { name: 'Business', type: 'string' },
                    { name: 'BusinessKey', type: 'string' },
                    { name: 'BusinessType', type: 'string' },
                    { name: 'YearMonth', type: 'string' },
                    { name: 'DAYS', type: 'string' },
                    { name: 'Money', type: 'number' },
                    { name: 'Account', type: 'number' },
                    { name: 'MANAGEMENT_COMPANY', type: 'string' },
                    { name: 'BELONGTO_COMPANY', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "YearMonth": $("#YearMonth").val() },
                url: "/VoucherManageManagement/VehicleCount/GeVehicleData"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source);
        //创建卡信息列表（主表）
        selector.$grid().jqxGrid({
            pageable: true,
            width: "100%",
            height: 450,
            pageSize: 999999999,
            //serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            groupable: true,
            groupsexpandedbydefault: true,
            groups: ['MANAGEMENT_COMPANY', 'BELONGTO_COMPANY', 'Model', 'ClassType', 'CarType'],
            showgroupsheader: false,
            columnsHeight: 30,
            pagermode: 'simple',
            selectionmode: 'singlerow',
            columns: [
                //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                //{ text: '编码', datafield: 'ORIGINALID', width: 100, align: 'center', cellsAlign: 'center' },
                { text: '资产管理公司', datafield: 'MANAGEMENT_COMPANY', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '资产所属公司', datafield: 'BELONGTO_COMPANY', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '模式', datafield: 'Model', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '班型', datafield: 'ClassType', width: 200, align: 'center', cellsAlign: 'center' },
                { text: '车型', datafield: 'CarType', width: 150, align: 'center', cellsAlign: 'center' },
                { text: '日期', datafield: 'YearMonth', width: 120, align: 'center', cellsAlign: 'center' },
                { text: '车牌号', datafield: 'PLATE_NUMBER', width: 120, align: 'center', cellsAlign: 'center' },
                { text: '结算标准', datafield: 'Money', align: 'center', cellsAlign: 'center' },
                { text: '平均车辆数', datafield: 'DAYS', align: 'center', cellsAlign: 'center' },
                { text: '结算合计', datafield: 'Account', align: 'center', cellsAlign: 'center' },
                { text: 'VGUID', datafield: 'VGUID', hidden: true },
            ]
        });
    }
};

$(function () {
    var page = new $page();
    page.init();
});
