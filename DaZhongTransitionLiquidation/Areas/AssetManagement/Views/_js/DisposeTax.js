//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") },
    $btnImport: function () { return $("#btnImport") },
    
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
        selector.$btnImport().on("click",
            function () {
                $("#ImportType").val("Auction");
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
                        url: "/AssetManagement/DisposeTax/ImportDisposeTaxFile",
                        type: "post",
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
                    { name: 'Income', type: 'float' },
                    { name: 'TowageFee', type: 'float' },
                    { name: 'AddedValueTax', type: 'float' },
                    { name: 'ConstructionTax', type: 'float' },
                    { name: 'AdditionalEducationTax', type: 'float' },
                    { name: 'LocalAdditionalEducationTax', type: 'float' },
                    { name: 'ServiceFee', type: 'float' },
                    { name: 'ConsignFee', type: 'float' },
                    { name: 'TaxFreeIncome', type: 'float' },
                    { name: 'Taxes', type: 'float' },
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
                    { text: '车主', datafield: 'VehicleOwner', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '车管车牌号', datafield: 'DepartmentVehiclePlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'Oracle车牌号', datafield: 'OraclePlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '导入的车牌号', datafield: 'ImportPlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '收入', datafield: 'Income', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '增值税', datafield: 'AddedValueTax', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '城建税', datafield: 'ConstructionTax', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '教育附加费', datafield: 'AdditionalEducationTax', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '地方教育附加费', datafield: 'LocalAdditionalEducationTax', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '服务费金额', datafield: 'ServiceFee', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '拖运费', datafield: 'ConsignFee', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '不含税收入', datafield: 'TaxFreeIncome', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '税金', datafield: 'Taxes', width: 150, align: 'center', cellsAlign: 'center' },
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