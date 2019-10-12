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
    //所有事件
    function addEvent() {
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#PlateNumber").val("");
        });
        //计算数据
        $("#btnCompute").on("click", function () {
            layer.load();
            $.ajax({
                url: "/AssetManagement/DisposeProfitLoss/ComputeProfitLoss",
                //traditional: true,
                type: "post",
                success: function (msg) {
                    layer.closeAll('loading');
                    switch (msg.Status) {
                    case "0":
                        jqxNotification("计算失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("计算成功！", null, "success");
                        initTable();
                        break;
                    }
                }
            });
        });
    }; //addEvent end
    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: 'VGUID', type: 'string' },
                    { name: 'VehicleOwner', type: 'string' },
                    { name: 'DepartmentVehiclePlateNumber', type: 'string' },
                    { name: 'OraclePlateNumber', type: 'string' },
                    { name: 'ImportPlateNumber', type: 'string' },
                    { name: 'VehicleModel', type: 'string' },
                    { name: 'OriginalValue', type: 'float' },
                    { name: 'AcctDepreciation', type: 'float' },
                    { name: 'NetValue', type: 'float' },
                    { name: 'Price', type: 'float' },
                    { name: 'DriverRentCarFee', type: 'string' },
                    { name: 'Taxes', type: 'float' },
                    { name: 'RealizedProfitLoss', type: 'float' },
                    { name: 'BelongToCompany', type: 'string' },
                    { name: 'ManageCompany', type: 'string' },
                    { name: 'CommissioningDate', type: 'date' },
                    { name: 'BackCarDate', type: 'date' },
                    { name: 'BackCarAge', type: 'number' },
                    { name: 'SaleMonth', type: 'string' },
                    { name: 'SaleType', type: 'string' },
                    { name: 'BusinessModel', type: 'string' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'ChangeDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'AssetID', type: 'string' },
                    { name: 'ChangeUser', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "PlateNumber": $("#PlateNumber").val() },
                url: "/AssetManagement/DisposeProfitLoss/GetAssetsDisposeProfitLossListDatas"   //获取数据源的路径
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
                    //{ text: '车主', datafield: 'VehicleOwner', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产编号', datafield: 'AssetID', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '车管车牌号', datafield: 'DepartmentVehiclePlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '出售车牌号', datafield: 'ImportPlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'Oracle车牌号', datafield: 'OraclePlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '车型', datafield: 'VehicleModel', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '处置收入', datafield: 'Price', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '处置税金', datafield: 'Taxes', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '处置收入返还驾驶员', datafield: 'DriverRentCarFee', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '导入的车牌号', datafield: 'ImportPlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '处置净值', datafield: 'NetValue', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '处置原值', datafield: 'OriginalValue', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '处置损益', datafield: 'RealizedProfitLoss', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '累计折旧', datafield: 'AcctDepreciation', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产所属公司', datafield: 'BelongToCompany', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '管理公司', datafield: 'ManageCompany', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '投产日期', datafield: 'CommissioningDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '退车日期', datafield: 'BackCarDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '退车车龄', datafield: 'BackCarAge', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '出售月份', datafield: 'SaleMonth', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '出售方式', datafield: 'SaleType', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '营运模式', datafield: 'BusinessModel', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '创建日期', datafield: 'CreateDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CreateUser', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '修改日期', datafield: 'ChangeDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'ChangeUser', width: 150, align: 'center', cellsAlign: 'center' }
                ]
            });
    }
};

$(function () {
    var page = new $page();
    page.init();
});
function checkFileExt(ext) {
    if (!ext.match(/.xls|.xlsx/i)) {
        return false;
    }
    return true;
}