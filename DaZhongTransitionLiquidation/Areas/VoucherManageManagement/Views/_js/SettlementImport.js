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
            //initTable();
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
            //layer.load();
            $.getJSON("/VoucherManageManagement/SettlementImport/GetSettlementData", function (mps) {
                var utils = $.pivotUtilities;
                var heatmap = utils.renderers["Heatmap"];
                var sumOverSum = utils.aggregators["Sum"];
                var model = [];
                var classType = [];
                var carType = [];
                var business = [];
                var businessType = [];
                for (var i = 0; i < mps.length; i++) {
                    if (model.indexOf(mps[i].Model) < 0) {
                        model.push(mps[i].Model);
                    }
                    if (classType.indexOf(mps[i].ClassType) < 0) {
                        classType.push(mps[i].ClassType);
                    }
                    if (carType.indexOf(mps[i].CarType) < 0) {
                        carType.push(mps[i].CarType);
                    }
                    if (business.indexOf(mps[i].Business) < 0) {
                        business.push(mps[i].Business);
                    }
                    if (businessType.indexOf(mps[i].BusinessType) < 0) {
                        businessType.push(mps[i].BusinessType);
                    }
                }
                $("#SettlementImportTable").pivot(mps, {
                    rows: ["Business", "BusinessType"],
                    cols: ["Model", "ClassType", "CarType"],
                    //aggregatorName: "Sum",
                    aggregator: sumOverSum(["Money"]),
                    //vals: ["Money"],
                    sorters: {
                        Business: $.pivotUtilities.sortAs(business),
                        BusinessType: $.pivotUtilities.sortAs(businessType),
                        Model: $.pivotUtilities.sortAs(model),
                        ClassType: $.pivotUtilities.sortAs(classType),
                        CarType: $.pivotUtilities.sortAs(carType),
                    },
                    rendererOptions: {
                        table: {
                            clickCallback: function (e, value, filters, pivotData) {
                                //var names = [];
                                //pivotData.forEachMatchingRecord(filters,
                                //    function (record) { names.push(record.Name); });
                                //alert(names.join("\n"));
                            }
                        }
                    }
                });
            });
            setTimeout(function () {
                $(".pvtAxisLabel").eq(0).text("模式");
                $(".pvtAxisLabel").eq(0).css("width", "200px")
                $(".pvtAxisLabel").eq(0).css("text-align", "center")
                $(".pvtAxisLabel").eq(1).text("班型");
                $(".pvtAxisLabel").eq(1).css("width", "200px")
                $(".pvtAxisLabel").eq(1).css("text-align", "center")
                $(".pvtAxisLabel").eq(2).text("车型");
                $(".pvtAxisLabel").eq(2).css("width", "200px")
                $(".pvtAxisLabel").eq(2).css("text-align", "center")
                $(".pvtAxisLabel").eq(3).text("营收主类");
                $(".pvtAxisLabel").eq(3).css("width", "200px")
                $(".pvtAxisLabel").eq(3).css("text-align", "center")
                $(".pvtAxisLabel").eq(4).text("营收子类");
                $(".pvtAxisLabel").eq(4).css("width", "200px")
                $(".pvtAxisLabel").eq(4).css("text-align", "center")
                //layer.closeAll('loading');
                $("#SettlementImportTable").show();
            }, 2000)
        }
    };
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