var selector = {
    //表格
    $grid: function () { return $("#datatable") },
    $NextDayDataFrm: function () { return $("#NextDayDataFrm") },
    //查询条件
    $txtName: function () { return $("#txtName") },
    $txtMobilePhone: function () { return $("#txtMobilePhone") },
    $txtUserIDNo: function () { return $("#txtUserIDNo") },
    $txtJobNumber: function () { return $("#txtJobNumber") },
    $txtTransactionId: function () { return $("#txtTransactionId") },
    $txtPaymentForm: function () { return $("#txtPaymentForm") },
    $txtPaymentTo: function () { return $("#txtPaymentTo") },
    $txtChannel: function () { return $("#txtChannel") },
    $txtChannel2: function () { return $("#txtChannel2") },
    //按钮
    $btnAdd: function () { return $("#btnAdd") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $txtTransactionId_T: function () { return $("#txtTransactionId_T") },
    //弹出框
    $AddDialog: function () { return $("#AddDialog") },
    $AddDialog_Title: function () { return $("#AddDialog_Title") },
    $add_OKButton: function () { return $("#add_OKButton") },
    $add_CancelBtn: function () { return $("#add_CancelBtn") },
    $txtRemitamount: function () { return $("#txtRemitamount") },
    $txtWechatNo: function () { return $("#txtWechatNo") },
    //$txtTransactionId_Dialog: function () { return $("#txtTransactionId_Dialog") },
    $txtPaymentTime: function () { return $("#txtPaymentTime") },
    $txtChannel_Dialog: function () { return $("#txtChannel_Dialog") },
    $txtSubject_Dialog: function () { return $("#txtSubject_Dialog") },

    $EditPermission: function () { return $("#EditPermission") },
    $uploadFileHuiDouQuan: function () { return $("#uploadFileHuiDouQuan") },
    $btnImportHuiDouQuan: function () { return $("#btnImportHuiDouQuan") },
    $btnImportSelfServicePaymentMachine: function () { return $("#btnImportSelfServicePaymentMachine") },
    $uploadSelfServicePaymentMachine: function () { return $("#uploadSelfServicePaymentMachine") }
};
var isEdit = false;
var vguid = "";
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        initTable();
        getChannelInfos();
        //查询
        selector.$btnSearch().on("click", function () {
            initTable();
        });

        //重置
        selector.$btnReset().on("click", function () {
            selector.$txtTransactionId().val("");
            selector.$txtPaymentForm().val("");
            selector.$txtPaymentTo().val("");
            selector.$txtChannel().val("");
            selector.$txtChannel2().val("");
        });
        //点击新增
        selector.$btnAdd().on("click", function () {
            $(".msg").remove();
            selector.$txtRemitamount().removeClass("input_Validate");
            //selector.$txtTransactionId_Dialog().removeClass("input_Validate");
            selector.$txtPaymentTime().removeClass("input_Validate");
            selector.$txtChannel_Dialog().removeClass("input_Validate");
            selector.$txtWechatNo().removeClass("input_Validate");
            selector.$txtRemitamount().val("");
            //selector.$txtTransactionId_Dialog().val("");
            selector.$txtWechatNo().val("");
            selector.$txtPaymentTime().val("");
            selector.$txtChannel_Dialog().val("");
            selector.$AddDialog().modal({ backdrop: "static", keyboard: false });
            selector.$AddDialog().modal("show");
        });

        selector.$btnImportHuiDouQuan().on("click", function () {
            selector.$uploadFileHuiDouQuan().val("");
            selector.$uploadFileHuiDouQuan().click();
        });
        //上传文件变更时间
        selector.$uploadFileHuiDouQuan().on('change', function () {
            layer.load();
            uploadFile(this.files[0], function (fileName) {
                runImportDataHuiDouQuan(fileName, function (result) {
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
        selector.$btnImportSelfServicePaymentMachine().on("click", function () {
            selector.$uploadSelfServicePaymentMachine().val("");
            selector.$uploadSelfServicePaymentMachine().click();
        });
        //上传文件变更时间
        selector.$uploadSelfServicePaymentMachine().on('change', function () {
            layer.load();
            uploadFile(this.files[0], function (fileName) {
                runImportSelfServicePaymentMachine(fileName, function (result) {
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
        selector.$txtChannel_Dialog().on("change", function () {

            selector.$txtSubject_Dialog().html('');

            var Channel = selector.$txtChannel_Dialog().val();
            loadSubject(Channel, "");
        });

        //拉取邮件
        $("#btnEmail").on("click", function () {
            $.ajax({
                url: "/PaymentManagement/NextDayData/GetEmailInfo",
                type: "post",
                dataType: "json",
                success: function (msg) {
                    if (msg.Status == "0") {
                        jqxNotification("未拉取到邮件！", null, "error");
                    } else {
                        jqxNotification("邮件拉取成功！", null, "success");
                        initTable();
                    }
                }

            });
        });

        //弹出框中取消按钮
        selector.$add_CancelBtn().on("click", function () {
            selector.$AddDialog().modal("hide");
        });
        //弹出框中保存按钮
        selector.$add_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate(selector.$txtRemitamount())) {
                validateError++;
            }
            //if (!Validate(selector.$txtTransactionId_Dialog())) {
            //    validateError++;
            //}
            if (!Validate(selector.$txtWechatNo())) {
                validateError++;
            }
            if (!Validate(selector.$txtPaymentTime())) {
                validateError++;
            }
            if (!Validate(selector.$txtChannel_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtSubject_Dialog())) {
                validateError++;
            }
            if (validateError <= 0) {
                selector.$NextDayDataFrm().ajaxSubmit({
                    url: "/PaymentManagement/NextDayData/SaveNextDayData",
                    data: {
                        isEdit: isEdit,
                        Vguid: vguid
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
                                selector.$grid().jqxDataTable('updateBoundData');
                                selector.$AddDialog().modal("hide");
                                break;
                            case "2":
                                jqxNotification("流水号已经存在！", null, "error");
                                break;
                        }

                    }
                });

            }

        });
        //初始化表格
        function initTable() {
            var transactionId = selector.$txtTransactionId().val();
            if (selector.$txtTransactionId_T().val()) {
                transactionId = selector.$txtTransactionId_T().val();
            }
            var source =
                {
                    datafields:
                    [
                        { name: "checkbox", type: null },
                        { name: 'Remitamount', type: 'number' },
                        { name: 'DriverBearFees', type: 'number' },
                        { name: 'PaidAmount', type: 'number' },
                        { name: 'RevenueFee', type: 'number' },
                        { name: 'CompanyBearsFees', type: 'number' },
                        { name: 'ChannelPayableAmount', type: 'number' },
                        { name: 'WechatNo', type: 'string' },
                        { name: 'Channel_Id', type: 'string' },
                        { name: 'ChannelName', type: 'string' },
                        { name: 'Revenuetime', type: 'date' },
                        { name: 'serialnumber', type: 'string' },
                        { name: 'CreatedDate', type: 'date' },
                        { name: 'CreatedUser', type: 'string' },
                        { name: 'SubjectId', type: 'string' },
                        { name: 'SubjectNmae', type: 'string' },
                        { name: 'Vguid', type: 'string' }

                    ],
                    datatype: "json",
                    id: "Vguid",//主键
                    data: {
                        "Channel_Id2": $("#txtChannel2").val(),
                        "PayDateFrom": selector.$txtPaymentForm().val(),
                        "PayDateTo": selector.$txtPaymentTo().val(),
                        "Channel_Id": selector.$txtChannel().val()
                    },
                    url: "/PaymentManagement/NextDayData/GetNextDayDatas"    //获取数据源的路径
                };
            var typeAdapter = new $.jqx.dataAdapter(source, {
                downloadComplete: function (data) {
                    source.totalrecords = data.TotalRows;
                }
            });
            selector.$grid().jqxDataTable(
                {
                    pageable: true,
                    width: "100%",
                    height: 480,
                    pageSize: 10,
                    serverProcessing: true,
                    pagerButtonsCount: 10,
                    source: typeAdapter,
                    theme: "office",
                    columns: [
                        //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                        { text: '订单号', datafield: 'WechatNo', minwidth: 200, align: 'center', cellsAlign: 'center', hidden: true },
                        { text: '流水号', datafield: 'serialnumber', minwidth: 200, align: 'center', cellsAlign: 'center' },
                        { text: '驾驶员欠款金额', cellsFormat: "d2", width: 150, datafield: 'Remitamount', align: 'center', cellsAlign: 'center' },// cellsRenderer: detailFunc 
                        { text: '驾驶员承担手续费', cellsFormat: "d2", width: 150, datafield: 'DriverBearFees', align: 'center', cellsAlign: 'center' },
                        { text: '驾驶员实付金额', cellsFormat: "d2", width: 150, datafield: 'PaidAmount', align: 'center', cellsAlign: 'center' },
                        { text: '渠道实收手续费', cellsFormat: "d2", width: 150, datafield: 'RevenueFee', align: 'center', cellsAlign: 'center' },
                        { text: '公司承担手续费', cellsFormat: "d2", width: 150, datafield: 'CompanyBearsFees', align: 'center', cellsAlign: 'center' },
                        { text: '渠道应付金额', cellsFormat: "d2", width: 150, datafield: 'ChannelPayableAmount', align: 'center', cellsAlign: 'center' },
                        { text: '到账时间', datafield: 'Revenuetime', minwidth: 130, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                        { text: '渠道ID', datafield: 'Channel_Id', width: 120, align: 'center', cellsAlign: 'center' },
                        { text: '渠道名称', datafield: 'ChannelName', minwidth: 150, align: 'center', cellsAlign: 'center' },
                        { text: '二级渠道ID', datafield: 'SubjectId', width: 120, align: 'center', cellsAlign: 'center' },
                        { text: '二级渠道名称', datafield: 'SubjectNmae', minwidth: 150, align: 'center', cellsAlign: 'center' },
                        { text: '创建人', datafield: 'CreatedUser', minwidth: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                        { text: '创建时间', datafield: 'CreatedDate', minwidth: 130, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                        { text: 'Vguid', datafield: 'Vguid', hidden: true }
                    ]
                });
        }

        function detailFunc(row, column, value, rowData) {
            var container = "";
            //if (selector.$EditPermission().val() == "1") {
            container = "<a href='#' onclick=edit('" + rowData.Vguid + "','" + rowData.Remitamount + "','" + rowData.WechatNo + "','" + getLocalTime(rowData.Revenuetime) + "','" + rowData.Channel_Id + "','" + rowData.SubjectId + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Remitamount + "</a>";
            //} else {
            //    container = "<span>" + rowData.Remitamount + "</span>";
            //}
            return container;
        }
        //转换时间戳
        function getLocalTime(val) {
            if (val != null) {
                var date = new Date(val);
                var y = date.getFullYear();
                var m = date.getMonth() + 1;
                if (m < 10) {
                    m = "0" + m;
                }
                var d = date.getDate();
                if (d < 10) {
                    d = "0" + d;
                }
                return y + '-' + m + '-' + d;
            }
        }

        function cellsRendererFunc(row, column, value, rowData) {
            return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
        }

        function rendererFunc() {
            var checkBox = "<div id='jqx_datatable_checkbox_all' class='jqx_datatable_checkbox_all' style='z-index: 999; margin-left:7px ;margin-top: 7px;'>";
            checkBox += "</div>";
            return checkBox;
        }

        function renderedFunc(element) {
            var grid = selector.$grid();
            element.jqxCheckBox();
            element.on('change', function (event) {
                var checked = element.jqxCheckBox('checked');
                if (checked) {
                    var rows = grid.jqxDataTable('getRows');
                    for (var i = 0; i < rows.length; i++) {
                        grid.jqxDataTable('selectRow', i);
                        grid.find(".jqx_datatable_checkbox").attr("checked", "checked");
                    }
                } else {
                    grid.jqxDataTable('clearSelection');
                    grid.find(".jqx_datatable_checkbox").removeAttr("checked", "checked");
                }
            });
            return true;
        }

        function getChannelInfos() {
            $.ajax({
                url: "/PaymentManagement/NextDayData/GetChannelInfor",
                type: "post",
                dataType: "json",
                success: function (msg) {
                    var option = "<option value=''></option>";
                    for (var i = 0; i < msg.length; i++) {
                        option += "<option value=" + msg[i].Id + ">" + msg[i].Name + "</option>";
                    }
                    selector.$txtChannel().append(option);
                }

            });
        }
    }; //addEvent end


};
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
//执行导入慧兜圈
function runImportDataHuiDouQuan(fileName, callback) {
    $.ajax({
        url: '/PaymentManagement/NextDayData/ImportDataHuiDouQuan',
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

//执行导入自助缴费机
function runImportSelfServicePaymentMachine(fileName, callback) {
    $.ajax({
        url: '/PaymentManagement/NextDayData/ImportSelfServicePaymentMachine',
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

function edit(guid, money, no, time, channel, SubjectId) {
    isEdit = true;
    vguid = guid;
    selector.$txtRemitamount().val(money);
    //selector.$txtTransactionId_Dialog().val(no);
    selector.$txtPaymentTime().val(time);
    selector.$txtPaymentTime().blur();
    selector.$txtWechatNo().val(no);
    selector.$txtChannel_Dialog().val(channel);
    selector.$txtChannel_Dialog().blur();
    loadSubject(channel, SubjectId);

    //selector.$AddDialog_Title().text("编辑T+1数据");
    selector.$AddDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddDialog().modal("show");
}

function loadSubject(Channel, SubjectId) {

    selector.$txtSubject_Dialog().html('');
    selector.$txtSubject_Dialog().append("<option value=\"\"></option>");
    $.post("/PaymentManagement/NextDayData/GetSubject", { "Channel": Channel }, function (data) {
        if (data != null) {

            for (var i = 0; i < data.length; i++) {
                var value = data[i].SubjectId;
                var text = data[i].SubjectNmae;
                selector.$txtSubject_Dialog().append("<option value='" + value + "'>" + text + "</option>")
            }
        }
        selector.$txtSubject_Dialog().val("");
        selector.$txtSubject_Dialog().val(SubjectId);
        selector.$txtSubject_Dialog().blur();
    }, "json")
}

$(function () {
    var page = new $page();
    page.init();

});

function changeChannel() {
    var channel = $("#txtChannel").val();
    $.ajax({
        url: "/PaymentManagement/NextDayData/GetSubject",
        type: "post",
        dataType: "json",
        data: { "Channel": channel },
        success: function (msg) {
            uiEngineHelper.bindSelect('#txtChannel2', msg, "SubjectId", "SubjectNmae");
            $("#txtChannel2").prepend("<option value=\"\" selected='true'></>");
        }

    });
}