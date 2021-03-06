﻿var selector = {
    //表格
    $grid: function () { return $("#datatable") },
    $txtMonth: function () { return $("#txtMonth") },
    $txtChannel: function () { return $("#txtChannel") },
    //按钮
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $AddBankChannelDialog: function () { return $("#AddBankChannelDialog") },
    $AddBankChannel_OKButton: function () { return $("#AddBankChannel_OKButton") },
    $AddBankChannel_CancelBtn: function () { return $("#AddBankChannel_CancelBtn") },
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
        selector.$AddBankChannel_CancelBtn().on("click", function () {
            selector.$AddBankChannelDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddBankChannel_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (validateError <= 0) {
                $.ajax({
                    url: "/VoucherManageManagement/SettlementImport/SaveSettlementImport",
                    data: {
                        Model: $("#txtModel").val(),
                        ClassType: $("#txtClassType").val(),
                        CarType: $("#txtCarType").val(),
                        Business: $("#txtBusiness").val(),
                        BusinessType: $("#txtBusinessType").val(),
                        Money: $("#txtMoney").val(),
                        NewMoney: $("#newMoney").val()
                    },
                    type: "post",
                    dataType: "json",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                initTable();
                                selector.$AddBankChannelDialog().modal("hide");
                                break;
                        }

                    }
                });
            }
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
        //给table外面的div滚动事件绑定一个函数
        $("#pivotTable").scroll(function () {
            //scrollFunc();
        });

        function initTable() {
            //layer.load();
            $.ajax({
                url: "/VoucherManageManagement/SettlementImport/GetSettlementData",
                data: {},
                async: false,
                type: "post",
                success: function (mps) {
                    //layer.closeAll('loading');
                    if (mps != null) {
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
                                        $("#txtModel").val(filters.Model),
                                        $("#txtClassType").val(filters.ClassType),
                                        $("#txtCarType").val(filters.CarType),
                                        $("#txtBusiness").val(filters.Business),
                                        $("#txtBusinessType").val(filters.BusinessType),
                                        $("#txtMoney").val(value),
                                        selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
                                        selector.$AddBankChannelDialog().modal("show");
                                    }
                                }
                            }
                        });

                        $(".pvtAxisLabel").eq(0).text("模式");
                        $(".pvtAxisLabel").eq(0).css("text-align", "center")
                        $(".pvtAxisLabel").eq(1).text("班型");
                        $(".pvtAxisLabel").eq(1).css("text-align", "center")
                        $(".pvtAxisLabel").eq(2).text("车型");
                        $(".pvtAxisLabel").eq(2).css("text-align", "center")
                        $(".pvtAxisLabel").eq(3).text("营收主类");
                        $(".pvtAxisLabel").eq(3).css("text-align", "center")
                        $(".pvtAxisLabel").eq(4).text("营收子类");
                        $(".pvtAxisLabel").eq(4).css("text-align", "center")
                        $(".pvtRowTotalLabel").css("text-align", "center")
                        $("#SettlementImportTable").show();
                    }
                }
            });
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

function scrollFunc() {
    var left = $("#pivotTable").scrollLeft();//获取滚动的距离
    var top = $("#pivotTable").scrollTop();//获取滚动的距离
    var trs = $("#pivotTable table tr");//获取表格的所有tr
    trs.each(function (i) {//对每一个tr（每一行）进行处理
        //获得每一行下面的所有的td，然后选中下标为0的，即第一列，设置position为相对定位
        //相对于父div左边的距离为滑动的距离，然后设置个背景颜色，覆盖住后面几列数据滑动到第一列下面的情况
        //如果有必要也可以设置一个z-index属性
        if (i < 4) {
            $(this).children().css({ "position": "relative", "top": top, "background-color": "#F0F0F0" });
            if (i < 3) {
                if (i == 0) {
                    $(this).children().eq(1).css({ "position": "relative", "left": left, "background-color": "#F0F0F0" });
                }
                $(this).children().eq(0).css({ "position": "relative", "left": left, "background-color": "#F0F0F0" });
            } else {
                $(this).children().eq(0).css({ "position": "relative", "left": left, "background-color": "#F0F0F0" });
                $(this).children().eq(1).css({ "position": "relative", "left": left, "background-color": "#F0F0F0" });
                $(this).children().eq(2).css({ "position": "relative", "left": left, "background-color": "#F0F0F0" });
            }
        } else {
            //$(this).children().eq(0).css({ "position": "relative", "left": left, "background-color": "#F0F0F0" });
        }
    });
}