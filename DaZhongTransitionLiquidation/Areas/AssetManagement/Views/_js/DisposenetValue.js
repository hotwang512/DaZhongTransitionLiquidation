//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") },
    $btnImport: function () { return $("#btnImport") }
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
                        url: "/AssetManagement/DisposeNetValue/ImportDisposeNetValueFile",
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

        //获取数据
        $("#btnGetData").on("click", function () {
            layer.load();
            $.ajax({
                url: "/AssetManagement/DisposeNetValue/GetAssetsRetirement",
                //traditional: true,
                type: "post",
                success: function (msg) {
                    layer.closeAll('loading');
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("获取失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("获取成功！", null, "success");
                            initTable(false);
                            break;
                        case "2":
                            jqxNotification(msg.ResultInfo, null, "error");
                            $("#myModalLabel_title2").html(msg.ResultInfo);
                            ViewReview(msg.ResultInfo2);
                            $("#jqxTable").jqxDataTable('updateBoundData');
                            break;
                        case "3":
                            jqxNotification(msg.ResultInfo, null, "error");
                            $("#myModalLabel_title2").html(msg.ResultInfo);
                            break;
                    }
                }
            });
        });
        //提交
        $("#btnSubmit").on("click", function () {
            var selection = [];
            var array = $("#jqxTable").jqxGrid('getselectedrowindexes');
            var pars = [];
            $(array).each(function (i, v) {
                try {
                    var value = $("#jqxTable").jqxGrid('getcell', v, "VGUID");
                    pars.push(value.value);
                } catch (e) {
                }
            });
            if (array.length < 1) {
                jqxNotification("请选择一条数据！", null, "error");
            } else {
                layer.load();
                $.ajax({
                    url: "/AssetManagement/DisposeNetValue/SubmitDisposeNetValue",
                    data: { guids: pars },
                    //traditional: true,
                    type: "post",
                    success: function (msg) {
                        layer.closeAll('loading');
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("提交失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("提交成功！", null, "success");
                                $("#jqxTable").jqxGrid('updateBoundData');
                                $('#jqxTable').jqxGrid('clearselection');
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
                    { name: 'AssetID', type: 'string' },
                    { name: 'ManageCompany', type: 'string' },
                    { name: 'BelongToCompany', type: 'string' },
                    { name: 'DepartmentVehiclePlateNumber', type: 'string' },
                    { name: 'OraclePlateNumber', type: 'string' },
                    { name: 'VehicleOwner', type: 'string' },
                    { name: 'OriginalValue', type: 'float' },
                    { name: 'AcctDepreciation', type: 'float' },
                    { name: 'NetValue', type: 'float' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'ChangeDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'ChangeUser', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "PlateNumber": $("#PlateNumber").val() },
                url: "/AssetManagement/DisposeNetValue/GetAssetsDisposeNetValueListDatas"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid().jqxGrid(
            {
                pageable: false,
                width: "100%",
                height: 400,
                //pageSize: 5,
                //serverProcessing: true,
                //pagerButtonsCount: 10,
                source: typeAdapter,
                rowsheight: 40,
                selectionmode: 'checkbox',
                theme: "office",
                columnsHeight: 40,
                columns: [
                    //{ text: '车主', datafield: 'VehicleOwner', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产编号', datafield: 'AssetID', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '管理公司', datafield: 'ManageCompany', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '所属公司', datafield: 'BelongToCompany', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '车管车牌号', datafield: 'ImportPlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'Oracle车牌号', datafield: 'OraclePlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '处置车牌号', datafield: 'DepartmentVehiclePlateNumber', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '原值', datafield: 'OriginalValue', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '累计折旧', datafield: 'AcctDepreciation', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '净值', datafield: 'NetValue', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '创建日期', datafield: 'CreateDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CreateUser', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '修改日期', datafield: 'ChangeDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'ChangeUser', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
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