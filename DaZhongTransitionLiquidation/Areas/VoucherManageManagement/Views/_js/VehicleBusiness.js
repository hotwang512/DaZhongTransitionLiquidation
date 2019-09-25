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
        $("#btnSync").on("click", function () {
            layer.load();
            $.ajax({
                url: "/VoucherManageManagement/VehicleBusiness/GetVehicleBusinessInfo",
                data: {},
                type: "post",
                dataType: "json",
                success: function (msg) {
                    if (msg.IsSuccess == true) {
                        layer.closeAll('loading');
                        jqxNotification("同步成功！", null, "success");
                        initTable();
                    } else {
                        layer.closeAll('loading');
                        jqxNotification("同步失败！", null, "error");
                    }
                }
            });
        });
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
                    { name: 'ORIGINALID', type: 'string' },
                    { name: 'PLATE_NUMBER', type: 'string' },
                    { name: 'MODEL_DAYS', type: 'string' },
                    { name: 'MODEL_MINOR', type: 'string' },
                    { name: 'YearMonth', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "YearMonth": $("#YearMonth").val(), "PLATE_NUMBER": $("#PLATE_NUMBER").val(), "MODEL_DAYS": $("#MODEL_DAYS").val(), "MODEL_MINOR": $("#MODEL_MINOR").val() },
                url: "/VoucherManageManagement/VehicleBusiness/GeVehicleData"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid().jqxDataTable({
            pageable: true,
            width: "100%",
            height: 400,
            pageSize: 10,
            serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 30,
            columns: [
                //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                { text: '编码', datafield: 'ORIGINALID', width: 350, align: 'center', cellsAlign: 'center' },
                { text: '日期', datafield: 'YearMonth', width: 350, align: 'center', cellsAlign: 'center' },
                { text: '车牌号', datafield: 'PLATE_NUMBER', width: 350, align: 'center', cellsAlign: 'center' },
                { text: '经营模式', datafield: 'MODEL_MINOR', width: 350, align: 'center', cellsAlign: 'center' },
                { text: '模式运营天数', datafield: 'MODEL_DAYS',align: 'center', cellsAlign: 'center' },
                { text: 'VGUID', datafield: 'VGUID', hidden: true },
            ]
        });
    }
};

$(function () {
    var page = new $page();
    page.init();
});
