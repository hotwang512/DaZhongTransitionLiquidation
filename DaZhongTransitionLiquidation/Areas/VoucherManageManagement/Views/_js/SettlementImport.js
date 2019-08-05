var selector = {
    //表格
    $grid: function () { return $("#datatable") },
    $txtMonth: function () { return $("#txtMonth") },
    $txtChannel: function () { return $("#txtChannel") },
    //按钮
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },

    $btnExport: function () { return $("#btnExport") }


};

var $page = function () {

    this.init = function () {
        //initControl();
        addEvent();
    }

    //所有事件
    function addEvent() {

        initTable();
        //查询
        selector.$btnSearch().on("click", function () {
            initTable();
        });

        //重置
        selector.$btnReset().on("click", function () {
            $("#YearMonth").val("");
        });

        //导入
        $("#btnImporting").on("click", function () {
            $("#uploadFile").val("");
            $("#uploadFile").click();
        });
        //上传文件变更时间
        $("#uploadFile").on('change', function () {
            layer.load();
            uploadFile(this.files[0], function (fileName) {
                runImportData(fileName, function (result) {
                    if (result.IsSuccess == true) {
                        jqxNotification("导入完成！", null, "success");
                        initTable();
                    }
                    else {
                        jqxNotification("导入失败！" + result.ResultInfo, null, "success");
                    }
                    layer.closeAll('loading');
                });
            })
        });

        function initTable() {
            var source =
           {
               datafields:
               [
                   //{ name: "checkbox", type: null },
                   { name: 'Model', type: 'string' },
                   { name: 'ClassType', type: 'string' },
                   { name: 'CarType', type: 'string' },
                   { name: 'BusinessType', type: 'string' },
                   { name: 'Money', type: 'number' },
                   { name: 'CreateTime', type: 'date' },
                   { name: 'VGUID', type: 'string' },
               ],
               datatype: "json",
               id: "VGUID",
               data: { Model: $("#Model").val(), ClassType: $("#ClassType").val() },
               url: "/VoucherManageManagement/SettlementImport/GetSettlementData" //获取数据源的路径
           };
            var typeAdapter = new $.jqx.dataAdapter(source);
            //创建卡信息列表（主表）
            selector.$grid().jqxGrid(
                {
                    pageable: false,
                    width: "100%",
                    height: 450,
                    pageSize: 10,
                    //serverProcessing: true,
                    pagerButtonsCount: 10,
                    source: typeAdapter,
                    theme: "office",
                    groupable: true,
                    groupsexpandedbydefault: true,
                    groups: ['Model', 'ClassType', 'CarType'],
                    showgroupsheader: false,
                    columnsHeight: 40,
                    pagermode: 'simple',
                    selectionmode: 'singlerow',
                    columns: [
                        //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                        { text: '模式', datafield: 'Model', width: 300, align: 'center', cellsAlign: 'center', },
                        { text: '班型', datafield: 'ClassType', width: 300, align: 'center', cellsAlign: 'center' },
                        { text: '车型', datafield: 'CarType', width: 300, align: 'center', cellsAlign: 'center' },
                        { text: '营业收入类型', datafield: 'BusinessType', width: 300, align: 'center', cellsAlign: 'center' },
                        { text: '金额', datafield: 'Money', cellsFormat: "d2", align: 'center', cellsAlign: 'center' },
                        { text: 'VGUID', datafield: 'VGUID', hidden: true },
                    ]
                });
        }

    }; //addEvent end

};

$(function () {
    var page = new $page();
    page.init();

});

//上传文件
function uploadFile(fileData, callback) {
    var formData = new FormData();
    formData.append("file", fileData);
    formData.append("filename", fileData.name);
    $.ajax({
        url: '/PaymentManagement/NextDayData/UploadImportFile',
        type: 'post',
        data: formData,//这里上传的数据使用了formData 对象
        processData: false, 	//必须false才会自动加上正确的Content-Type
        contentType: false,
        success: function (result) {
            if (callback) {
                callback(result);
            }
        },
        error: function (xmlhttprequest, textstatus, errorthrow) {
            jqxNotification("上传错误！", null, "error");
            layer.closeAll('loading');
        }

    });
}
//执行导入
function runImportData(fileName, callback) {
    $.ajax({
        url: '/VoucherManageManagement/SettlementImport/ImportSettlementData',
        type: 'post',
        data: { fileName: fileName },//这里上传的数据使用了formData 对象
        success: function (result) {
            if (callback) {
                callback(result);
            }
        },
        error: function (xmlhttprequest, textstatus, errorthrow) {
            jqxNotification("导入错误！", null, "error");
            layer.closeAll('loading');
        }

    });
}