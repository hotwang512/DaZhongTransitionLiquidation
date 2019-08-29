//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") },
    $btnImportAuction: function () { return $("#btnImportAuction") },
    $btnImportSale: function () { return $("#btnImportSale") },
    $btnImportScrap: function () { return $("#btnImportScrap") }
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
        selector.$btnImportAuction().on("click",
            function () {
                $("#ImportType").val("Auction");
                $("#LocalFileInput").click();
            }
        );
        selector.$btnImportSale().on("click",
            function () {
                $("#ImportType").val("Sale");
                $("#LocalFileInput").click();
            }
        );
        selector.$btnImportScrap().on("click",
            function () {
                $("#ImportType").val("Scrap");
                $("#LocalFileInput").click();
            }
        );
        //统一上传文件
        $("#LocalFileInput").on("change",
            function () {
                debugger;
                var filePath = this.value;
                var fileExt = filePath.substring(filePath.lastIndexOf("."))
                    .toLowerCase();
                if (!checkFileExt(fileExt)) {
                    jqxNotification("您上传的文件类型不允许,请重新上传！！", null, "error");
                    this.value = "";
                    return;
                } else {
                    layer.load();
                    $("#localFormFile").ajaxSubmit({
                        url: "/AssetManagement/DisposeIncome/ImportDisposeIncomeFile",
                        type: "post",
                        data: { "ImportType": $("#ImportType").val() },
                        success: function (msg) {
                            layer.closeAll('loading');
                            switch (msg.Status) {
                            case "0":
                                if (msg.ResultInfo != null || msg.ResultInfo2 != null) {
                                    jqxNotification((msg.ResultInfo == null ? "" : msg.ResultInfo) + " " + (msg.ResultInfo2 == null ? "" : msg.ResultInfo2), null, "error");
                                } else {
                                    jqxNotification("导入失败", null, "error");
                                }
                                $('#LocalFileInput').val('');
                                break;
                            case "1":
                                jqxNotification("导入成功！", null, "success");
                                $('#LocalFileInput').val('');
                                initTable();
                                break;
                            case "2":
                                jqxNotification(msg.ResultInfo, null, "error");
                                $('#LocalFileInput').val('');
                                initTable();
                                break;
                            }
                        }
                    });
                }
            });
    }; //addEvent end
    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: 'VGUID', type: 'string' },
                    { name: 'DepartmentVehiclePlateNumber', type: 'string' },
                    { name: 'OraclePlateNumber', type: 'string' },
                    { name: 'VehicleModel', type: 'string' },
                    { name: 'CurbWeight', type: 'float' },
                    { name: 'DeductTonnage', type: 'float' },
                    { name: 'SalvageUnitPrice', type: 'float' },
                    { name: 'SalvageValue', type: 'float' },
                    { name: 'TransactionPrice', type: 'float' },
                    { name: 'Commission', type: 'float' },
                    { name: 'ProcedureFee', type: 'float' },
                    { name: 'RealSales', type: 'float' },
                    { name: 'ServiceFee', type: 'float' },
                    { name: 'TowageFee', type: 'float' },
                    { name: 'SettlementPrice', type: 'float' },
                    { name: 'CommissioningDate', type: 'datetime' },
                    { name: 'BackCarDate', type: 'datetime' },
                    { name: 'Remark', type: 'string' },
                    { name: 'VehicleMode', type: 'string' },
                    { name: 'UseDepartment', type: 'string' },
                    { name: 'VehicleOwner', type: 'string' },
                    { name: 'ServiceUnitFee', type: 'float' },
                    { name: 'CreateDate', type: 'datetime' },
                    { name: 'ChangeDate', type: 'datetime' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'ChangeUser', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "PlateNumber": $("#PlateNumber").val() },
                url: "/AssetManagement/DisposeIncome/GetAssetsDisposeIncomeListDatas"   //获取数据源的路径
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
                    { text: '车管车牌号', datafield: 'DepartmentVehiclePlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'Oracle车牌号', datafield: 'OraclePlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '导入的车牌号', datafield: 'ImportPlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '车型', datafield: 'VehicleModel', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '整备质量（KG）', datafield: 'CurbWeight', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '扣除吨位（KG）', datafield: 'DeductTonnage', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '残值单价', datafield: 'SalvageUnitPrice', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '残值金额', datafield: 'SalvageValue', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '成交总额', datafield: 'TransactionPrice', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '佣金', datafield: 'Commission', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '手续费', datafield: 'ProcedureFee', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '实售', datafield: 'RealSales', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '服务费金额', datafield: 'ServiceFee', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '拖运费', datafield: 'TowageFee', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '结算价', datafield: 'SettlementPrice', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '投产日期', datafield: 'CommissioningDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '退车日期', datafield: 'BackCarDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '备注', datafield: 'Remark', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '车辆模式', datafield: 'VehicleMode', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '使用单位', datafield: 'UseDepartment', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '车主', datafield: 'VehicleOwner', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '服务费单价', datafield: 'ServiceUnitFee', width: 150, align: 'center', cellsAlign: 'center' },
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