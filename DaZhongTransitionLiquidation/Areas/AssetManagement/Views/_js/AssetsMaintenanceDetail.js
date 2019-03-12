//资产基础信息维护明细
var vguid = "";
var mydate = new Date();

//拍照数据（base64）
var baseUrl;
var socket;
var $page = function () {
    this.init = function () {
        addEvent();
    }
    //所有事件
    function addEvent() {
        var guid = $.request.queryString().VGUID;
        $("#VGUID").val(guid);
        if (guid != "" && guid != null) {
            getAssetInfoListDetail();
        } else {
            $("#hideButton").show();
            $(".status").hide();
        }
        //取消
        $("#btnCancel").on("click", function () {
            history.go(-1);
        })
        //保存
        $("#btnSave").on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate($("#GROUP_ID"))) { validateError++; }
            if (!Validate($("#ORGANIZATION_NUM"))) { validateError++; }
            if (!Validate($("#ENGINE_NUMBER"))) { validateError++; }
            if (!Validate($("#CHASSIS_NUMBER"))) { validateError++; }
            //if (!Validate($("#BOOK_TYPE_CODE"))) { validateError++; }
            if (!Validate($("#TAG_NUMBER"))) { validateError++; }
            if (!Validate($("#DESCRIPTION"))) { validateError++; }
            if (!Validate($("#QUANTITY"))) { validateError++; }
            //if (!Validate($("#ASSET_CATEGORY_MAJOR"))) { validateError++; }
            //if (!Validate($("#ASSET_CATEGORY_MINOR"))) { validateError++; }
            if (!Validate($("#ASSET_CREATION_DATE"))) { validateError++; }
            if (!Validate($("#ASSET_COST"))) { validateError++; }
            if (!Validate($("#SALVAGE_TYPE"))) { validateError++; }
            //if (!Validate($("#METHOD"))) { validateError++; }
            //if (!Validate($("#LIFE_MONTHS"))) { validateError++; }
            //if (!Validate($("#EXP_ACCOUNT_SEGMENT1"))) { validateError++; }
            //if (!Validate($("#EXP_ACCOUNT_SEGMENT2"))) { validateError++; }
            //if (!Validate($("#EXP_ACCOUNT_SEGMENT3"))) { validateError++; }
            //if (!Validate($("#EXP_ACCOUNT_SEGMENT4"))) { validateError++; }
            //if (!Validate($("#EXP_ACCOUNT_SEGMENT5"))) { validateError++; }
            //if (!Validate($("#EXP_ACCOUNT_SEGMENT6"))) { validateError++; }
            //if (!Validate($("#EXP_ACCOUNT_SEGMENT7"))) { validateError++; }
            if (!Validate($("#FA_LOC_1"))) { validateError++; }
            if (!Validate($("#FA_LOC_2"))) { validateError++; }
            if (!Validate($("#FA_LOC_3"))) { validateError++; }
            //if (!Validate($("#TRANSACTION_ID"))) { validateError++; }
            //if (!Validate($("#LAST_UPDATE_DATE"))) { validateError++; }
            //if (!Validate($("#PURCHASE_TAX"))) { validateError++; }
            if (!Validate($("#PLATE_NUMBER"))) { validateError++; }
            if (validateError <= 0) {
                $.ajax({
                    url: "/AssetManagement/AssetMaintenanceInfoDetail/SaveAssetMaintenanceInfo",
                    data: {
                        "VGUID": $("#VGUID").val(),
                        "GROUP_ID": $("#GROUP_ID").val(),
                        "ORGANIZATION_NUM": $("#ORGANIZATION_NUM").val(),
                        "ENGINE_NUMBER": $("#ENGINE_NUMBER").val(),
                        "CHASSIS_NUMBER": $("#CHASSIS_NUMBER").val(),
                        "BOOK_TYPE_CODE": $("#BOOK_TYPE_CODE").val(),
                        "TAG_NUMBER": $("#TAG_NUMBER").val(),
                        "DESCRIPTION": $("#DESCRIPTION").val(),
                        "QUANTITY": $("#QUANTITY").val(),
                        "ASSET_CATEGORY_MAJOR": $("#ASSET_CATEGORY_MAJOR").val(),
                        "ASSET_CATEGORY_MINOR": $("#ASSET_CATEGORY_MINOR").val(),
                        "ASSET_CREATION_DATE": $("#ASSET_CREATION_DATE").val(),
                        "ASSET_COST": $("#ASSET_COST").val(),
                        "SALVAGE_TYPE": $("#SALVAGE_TYPE").val(),
                        "SALVAGE_PERCENT": $("#SALVAGE_PERCENT").val().replace("%", ""),
                        "SALVAGE_VALUE": $("#SALVAGE_VALUE").val(),
                        "YTD_DEPRECIATION": $("#YTD_DEPRECIATION").val(),
                        "ACCT_DEPRECIATION": $("#ACCT_DEPRECIATION").val(),
                        "METHOD": $("#METHOD").val(),
                        "LIFE_MONTHS": $("#LIFE_MONTHS").val(),
                        "AMORTIZATION_FLAG": $("#AMORTIZATION_FLAG").val(),
                        "EXP_ACCOUNT_SEGMENT1": $("#EXP_ACCOUNT_SEGMENT1").val(),
                        "EXP_ACCOUNT_SEGMENT2": $("#EXP_ACCOUNT_SEGMENT2").val(),
                        "EXP_ACCOUNT_SEGMENT3": $("#EXP_ACCOUNT_SEGMENT3").val(),
                        "EXP_ACCOUNT_SEGMENT4": $("#EXP_ACCOUNT_SEGMENT4").val(),
                        "EXP_ACCOUNT_SEGMENT5": $("#EXP_ACCOUNT_SEGMENT5").val(),
                        "EXP_ACCOUNT_SEGMENT6": $("#EXP_ACCOUNT_SEGMENT6").val(),
                        "EXP_ACCOUNT_SEGMENT7": $("#EXP_ACCOUNT_SEGMENT7").val(),
                        "FA_LOC_1": $("#FA_LOC_1").val(),
                        "FA_LOC_2": $("#FA_LOC_2").val(),
                        "FA_LOC_3": $("#FA_LOC_3").val(),
                        "RETIRE_FLAG": $("#RETIRE_FLAG").val(),
                        "RETIRE_QUANTITY": $("#RETIRE_QUANTITY").val(),
                        "RETIRE_COST": $("#RETIRE_COST").val(),
                        "RETIRE_DATE": $("#RETIRE_DATE").val(),
                        "TRANSACTION_ID": $("#TRANSACTION_ID").val(),
                        "LAST_UPDATE_DATE": $("#LAST_UPDATE_DATE").val(),
                        "LISENSING_FEE": $("#LISENSING_FEE").val(),
                        "OUT_WAREHOUSE_FEE": $("#OUT_WAREHOUSE_FEE").val(),
                        "DOME_LIGHT_FEE": $("#DOME_LIGHT_FEE").val(),
                        "ANTI_ROBBERY_FEE": $("#ANTI_ROBBERY_FEE").val(),
                        "LOADING_FEE": $("#LOADING_FEE").val(),
                        "INNER_ROOF_FEE": $("#INNER_ROOF_FEE").val(),
                        "TAXIMETER_FEE": $("#TAXIMETER_FEE").val(),
                        "PLATE_NUMBER": $("#PLATE_NUMBER").val(),
                        "PURCHASE_TAX": $("#PURCHASE_TAX").val(),
                        "OBD_FEE": $("#OBD_FEE").val()
                    },
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                history.go(-1);
                                window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                                break;
                            case "2":
                                jqxNotification("车牌号已存在！", null, "error");
                                break;
                        }
                    }
                });
            }
        })

        //验收
        $("#btnAccept").on("click", function () {
            //验证当前表所有非空
            var validateError = 0;//未通过验证的数量
            if (!ValidateEmpty($("#GROUP_ID"))) { validateError++; }
            if (!ValidateEmpty($("#ORGANIZATION_NUM"))) { validateError++; }
            if (!ValidateEmpty($("#ENGINE_NUMBER"))) { validateError++; }
            if (!ValidateEmpty($("#CHASSIS_NUMBER"))) { validateError++; }
            if (!ValidateEmpty($("#BOOK_TYPE_CODE"))) { validateError++; }
            if (!ValidateEmpty($("#TAG_NUMBER"))) { validateError++; }
            if (!ValidateEmpty($("#DESCRIPTION"))) { validateError++; }
            if (!ValidateEmpty($("#QUANTITY"))) { validateError++; }
            if (!ValidateEmpty($("#ASSET_CATEGORY_MAJOR"))) { validateError++; }
            if (!ValidateEmpty($("#ASSET_CATEGORY_MINOR"))) { validateError++; }
            if (!ValidateEmpty($("#ASSET_CREATION_DATE"))) { validateError++; }
            if (!ValidateEmpty($("#ASSET_COST"))) { validateError++; }
            if (!ValidateEmpty($("#SALVAGE_TYPE"))) { validateError++; }
            if (!ValidateEmpty($("#METHOD"))) { validateError++; }
            if (!ValidateEmpty($("#LIFE_MONTHS"))) { validateError++; }
            if (!ValidateEmpty($("#EXP_ACCOUNT_SEGMENT1"))) { validateError++; }
            if (!ValidateEmpty($("#EXP_ACCOUNT_SEGMENT2"))) { validateError++; }
            if (!ValidateEmpty($("#EXP_ACCOUNT_SEGMENT3"))) { validateError++; }
            if (!ValidateEmpty($("#EXP_ACCOUNT_SEGMENT4"))) { validateError++; }
            if (!ValidateEmpty($("#EXP_ACCOUNT_SEGMENT5"))) { validateError++; }
            if (!ValidateEmpty($("#EXP_ACCOUNT_SEGMENT6"))) { validateError++; }
            if (!ValidateEmpty($("#EXP_ACCOUNT_SEGMENT7"))) { validateError++; }
            if (!ValidateEmpty($("#FA_LOC_1"))) { validateError++; }
            if (!ValidateEmpty($("#FA_LOC_2"))) { validateError++; }
            if (!ValidateEmpty($("#FA_LOC_3"))) { validateError++; }
            if (!ValidateEmpty($("#TRANSACTION_ID"))) { validateError++; }
            if (!ValidateEmpty($("#LAST_UPDATE_DATE"))) { validateError++; }
            if (!ValidateEmpty($("#PURCHASE_TAX"))) { validateError++; }
            if (!ValidateEmpty($("#PLATE_NUMBER"))) { validateError++; }
            if (!ValidateEmpty($("#LISENSING_FEE"))) { validateError++; }
            if (!ValidateEmpty($("#OUT_WAREHOUSE_FEE"))) { validateError++; }
            if (!ValidateEmpty($("#DOME_LIGHT_FEE"))) { validateError++; }
            if (!ValidateEmpty($("#ANTI_ROBBERY_FEE"))) { validateError++; }
            if (!ValidateEmpty($("#LOADING_FEE"))) { validateError++; }
            if (!ValidateEmpty($("#INNER_ROOF_FEE"))) { validateError++; }
            if (!ValidateEmpty($("#TAXIMETER_FEE"))) { validateError++; }
            if (!ValidateEmpty($("#OBD_FEE"))) { validateError++; }
            if (validateError > 0) {
                jqxNotification("还有数据未同步！", null, "error");
            } else {
                $("#Accept_OKBtn").show();
                $("#ResultDiv").hide();
                $("#AcceptTable").show();
                $("#AcceptDialog").modal({ backdrop: "static", keyboard: false });
                $("#AcceptDialog").modal("show");
            }
        })
        $("#UploadFile").on("click", function () {
            baseUrl = "ws://127.0.0.1:12345";
            openSocket();
            $("#devPhoto").hide();
            $("#Upload_OKBtn").hide();
            $("#photographPri").show();
            //$("#AcceptDialog").modal("hide");
            $("#UploadPictureDialog").modal({ backdrop: "static", keyboard: false });
            $("#UploadPictureDialog").modal("show");
        })
        $("#photographPri").on("click", function () {
            $("#Upload_OKBtn").show();
            $("#photographPri").hide();
            $("#devPhoto").show();
        })
        $("#Upload_OKBtn").on("click", function () {
            $('#jqxLoader').jqxLoader('open');
            $.ajax({
                url: "/AssetManagement/AssetMaintenanceInfoDetail/UploadImg",
                data: {
                    "Vguid": $("#VGUID").val(),
                    "ImageBase64Str": $("#devPhoto").attr("src"),
                },
                type: "post",
                success: function (msg) {
                    $('#jqxLoader').jqxLoader('close');
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("上传失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("上传成功！", null, "success");
                            $("#Attachment").show();
                            $("#Attachment").attr("href", msg.ResultInfo);
                            $("#Attachment").html(msg.ResultInfo2);
                            $("#UploadPictureDialog").modal("hide");
                            break;
                    }
                }
            });
        });
        $("#Accept_OKBtn").on("click", function () {
            $("#AcceptTable").hide();
            $("#ResultDiv").empty();
            $.ajax({
                url: "/AssetManagement/AssetMaintenanceInfoDetail/InserIntoSwapTable",
                data: {
                    "Vguid": $("#VGUID").val()
                },
                type: "post",
                success: function (msg) {
                    $("#ResultDiv").show();
                    $("#Accept_OKBtn").hide();;
                    var templateResult = "";
                    switch (msg.Status) {
                        case "0":
                            templateResult = "error";
                            $("#notificationContent").html("写入中间表失败");
                            break;
                        case "1":
                            templateResult = "success";
                            $("#notificationContent").html("写入中间表成功");
                            SendAssetInfo();
                            break;
                    }
                    $("#jqxNotification").jqxNotification({
                        width: "100%", position: "top-left", opacity: 1,
                        autoOpen: false, animationOpenDelay: 800, appendContainer: "#ResultDiv", autoClose: false, template: templateResult, closeOnClick: false
                    });
                    $("#jqxNotification").jqxNotification("open");
                }
            });
        });
        $("#btnGenerateAcceptance").on("click", function () {
            $("#GenerateAcceptanceDialog").modal({ backdrop: "static", keyboard: false });
            $("#GenerateAcceptanceDialog").modal("show");
        })
        //弹出框中的取消按钮
        $("#Accept_CancelBtn").on("click", function () {
            $("#AcceptDialog").modal("hide");
        });
        $("#Print_CancelBtn").on("click", function () {
            $("#GenerateAcceptanceDialog").modal("hide");
        });
        $("#Upload_CancelBtn").on("click", function () {
            $("#UploadPictureDialog").modal("hide");
        });
        $("#UploadLocalFile").on("click", function () {
            $("#FileInput").click();
        })
        $("#FileInput").on("change", function () {
            var filePath = this.value;
            var fileExt = filePath.substring(filePath.lastIndexOf("."))
                .toLowerCase();
            if (!checkFileExt(fileExt)) {
                jqxNotification("您上传的文件类型不允许,请重新上传！！", null, "error");
                this.value = "";
                return;
            } else {
                $('#jqxLoader').jqxLoader('open');
                $("#formFile").ajaxSubmit({
                    url: "/AssetManagement/AssetMaintenanceInfoDetail/UploadLocalFile",
                    type: "post",
                    data: {
                        'VGUID': $("#VGUID").val()
                    },
                    success: function (msg) {
                        $('#jqxLoader').jqxLoader('close');
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("上传失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("上传成功！", null, "success");
                                $("#Attachment").show();
                                $("#Attachment").attr("href", msg.ResultInfo);
                                $("#Attachment").html(msg.ResultInfo2);
                                break;
                        }
                    }
                });
            }
        })
        function checkFileExt(ext) {
            if (!ext.match(/.jpg|.png|.doc|.docx|.xls|.xlsx|.pdf|.bmp/i)) {
                return false;
            }
            return true;
        }
        $("#jqxLoader").jqxLoader({ isModal: true, width: 100, height: 60, imagePosition: 'top' });
        
    }; //addEvent end

    function getAssetInfoListDetail() {
        $.ajax({
            url: "/AssetManagement/AssetMaintenanceInfoDetail/GetAssetInfoDetail",
            data: {
                "vguid": $("#VGUID").val(),
            },
            type: "post",
            dataType: "json",
            success: function (msg) {
                //$("#Status").val(msg.Status);
                //if ($("#Status").val() == "1") {
                //    $("#hideButton").show();
                //}
                $("#VGUID").val(msg.VGUID);
                $("#GROUP_ID").val(msg.GROUP_ID);
                $("#ORGANIZATION_NUM").val(msg.ORGANIZATION_NUM);
                $("#ENGINE_NUMBER").val(msg.ENGINE_NUMBER);
                $("#CHASSIS_NUMBER").val(msg.CHASSIS_NUMBER);
                $("#BOOK_TYPE_CODE").val(msg.BOOK_TYPE_CODE);
                $("#TAG_NUMBER").val(msg.TAG_NUMBER);
                $("#DESCRIPTION").val(msg.DESCRIPTION);
                $("#QUANTITY").val(msg.QUANTITY);
                $("#ASSET_CATEGORY_MAJOR").val(msg.ASSET_CATEGORY_MAJOR);
                $("#ASSET_CATEGORY_MINOR").val(msg.ASSET_CATEGORY_MINOR);
                if (msg.ASSET_CREATION_DATE != "" && msg.ASSET_CREATION_DATE != null) {
                    $("#ASSET_CREATION_DATE").val(formatDate(msg.ASSET_CREATION_DATE));
                }
                $("#ASSET_COST").val(msg.ASSET_COST);
                $("#SALVAGE_TYPE").val(msg.SALVAGE_TYPE);
                msg.SALVAGE_PERCENT == null ? "" : $("#SALVAGE_PERCENT").val(msg.SALVAGE_PERCENT + "%");
                $("#SALVAGE_VALUE").val(msg.SALVAGE_VALUE);
                $("#YTD_DEPRECIATION").val(msg.YTD_DEPRECIATION);
                $("#ACCT_DEPRECIATION").val(msg.ACCT_DEPRECIATION);
                $("#METHOD").val(msg.METHOD);
                $("#LIFE_MONTHS").val(msg.LIFE_MONTHS);
                $("#AMORTIZATION_FLAG").val(msg.AMORTIZATION_FLAG);
                $("#EXP_ACCOUNT_SEGMENT1").val(msg.EXP_ACCOUNT_SEGMENT1);
                $("#EXP_ACCOUNT_SEGMENT2").val(msg.EXP_ACCOUNT_SEGMENT2);
                $("#EXP_ACCOUNT_SEGMENT3").val(msg.EXP_ACCOUNT_SEGMENT3);
                $("#EXP_ACCOUNT_SEGMENT4").val(msg.EXP_ACCOUNT_SEGMENT4);
                $("#EXP_ACCOUNT_SEGMENT5").val(msg.EXP_ACCOUNT_SEGMENT5);
                $("#EXP_ACCOUNT_SEGMENT6").val(msg.EXP_ACCOUNT_SEGMENT6);
                $("#EXP_ACCOUNT_SEGMENT7").val(msg.EXP_ACCOUNT_SEGMENT7);
                $("#FA_LOC_1").val(msg.FA_LOC_1);
                $("#FA_LOC_2").val(msg.FA_LOC_2);
                $("#FA_LOC_3").val(msg.FA_LOC_3);
                $("#RETIRE_FLAG").val(msg.RETIRE_FLAG);
                $("#RETIRE_QUANTITY").val(msg.RETIRE_QUANTITY);
                $("#RETIRE_COST").val(msg.RETIRE_COST);
                if (msg.RETIRE_DATE != "" && msg.RETIRE_DATE != null) {
                    $("#RETIRE_DATE").val(formatDate(msg.RETIRE_DATE));
                }
                $("#TRANSACTION_ID").val(msg.TRANSACTION_ID);
                if (msg.LAST_UPDATE_DATE != "" && msg.LAST_UPDATE_DATE != null) {
                    $("#LAST_UPDATE_DATE").val(formatDate(msg.LAST_UPDATE_DATE));
                }
                debugger;
                $("#LISENSING_FEE").val(msg.LISENSING_FEE);
                $("#OUT_WAREHOUSE_FEE").val(msg.OUT_WAREHOUSE_FEE);
                $("#DOME_LIGHT_FEE").val(msg.DOME_LIGHT_FEE);
                $("#ANTI_ROBBERY_FEE").val(msg.ANTI_ROBBERY_FEE);
                $("#LOADING_FEE").val(msg.LOADING_FEE);
                $("#INNER_ROOF_FEE").val(msg.INNER_ROOF_FEE);
                $("#TAXIMETER_FEE").val(msg.TAXIMETER_FEE);
                $("#PURCHASE_TAX").val(msg.PURCHASE_TAX);
                $("#OBD_FEE").val(msg.OBD_FEE);

                $("#LISENSING_FEE_M").val(msg.LISENSING_FEE);
                $("#OUT_WAREHOUSE_FEE_M").val(msg.OUT_WAREHOUSE_FEE);
                $("#DOME_LIGHT_FEE_M").val(msg.DOME_LIGHT_FEE);
                $("#ANTI_ROBBERY_FEE_M").val(msg.ANTI_ROBBERY_FEE);
                $("#LOADING_FEE_M").val(msg.LOADING_FEE);
                $("#INNER_ROOF_FEE_M").val(msg.INNER_ROOF_FEE);
                $("#TAXIMETER_FEE_M").val(msg.TAXIMETER_FEE);
                $("#PURCHASE_TAX_M").val(msg.PURCHASE_TAX);
                $("#OBD_FEE_M").val(msg.OBD_FEE);

                if (msg.CHANGE_DATE != "" && msg.CHANGE_DATE != null) {
                    $("#CHANGE_DATE").val(formatDate(msg.CHANGE_DATE));
                }
                if (msg.CREATE_DATE != "" && msg.CREATE_DATE != null) {
                    $("#CREATE_DATE").val(formatDate(msg.CREATE_DATE));
                }
                $("#CREATE_USER").val(msg.CREATE_USER);
                $("#CHANGE_USER").val(msg.CHANGE_USER);
                $("#PLATE_NUMBER").val(msg.PLATE_NUMBER);
                $("#STATUS").val(msg.STATUS);
                debugger;
                if (msg.ACCEPTANCE_CERTIFICATE != "" && msg.ACCEPTANCE_CERTIFICATE != null) {
                    $("#Attachment").show();
                    $("#Attachment").attr("href", msg.ACCEPTANCE_CERTIFICATE);
                    var fileName = msg.ACCEPTANCE_CERTIFICATE.substring(msg.ACCEPTANCE_CERTIFICATE.lastIndexOf("\\") + 1, msg.ACCEPTANCE_CERTIFICATE.length)
                    $("#Attachment").html(fileName);
                }
                $("#btnAccept").show();
            }
        });
    }
    //拍照
    function openSocket() {
        socket = new WebSocket(baseUrl);
        socket.onclose = function () {
            console.error("web channel closed");
        };
        socket.onerror = function (error) {
            console.error("web channel error: " + error);
        };
        socket.onopen = function () {
            new QWebChannel(socket, function (channel) {
                // make dialog object accessible globally
                window.dialog = channel.objects.dialog;
                //dialog.set_configValue("set_savePath:D:\\img");
                //网页关闭函数
                window.onbeforeunload = function () {
                    dialog.get_actionType("closeSignal");
                }
                window.onunload = function () {
                    dialog.get_actionType("closeSignal");
                }
                //拍照按钮点击
                document.getElementById("photographPri").onclick = function () {
                    dialog.photoBtnClicked("primaryDev_");
                    dialog.get_actionType("savePhotoPriDev");
                };
                //纠偏裁边
                document.getElementById("setdeskew").onclick = function () {
                    dialog.get_actionType("setdeskew");
                };

                //服务器返回消息
                dialog.sendPrintInfo.connect(function (message) {
                    //设备分辨率
                    message = message.substr(14);
                    //图片保存后返回路径关键字savePhoto_success:
                    if (message.indexOf("savePhoto_success:") >= 0) {
                        imgPath = message.substr(18);
                    }
                });
                //接收图片流用来展示，多个，较小的base64数据
                dialog.send_priImgData.connect(function (message) {
                    var element = document.getElementById("bigPriDev");
                    element.src = "data:image/jpg;base64," + message;
                });
                //接收拍照base64
                dialog.send_priPhotoData.connect(function (message) {
                    var element = document.getElementById("devPhoto");
                    element.src = "data:image/jpg;base64," + message;
                });
                //网页加载完成信号
                dialog.html_loaded("one");
            });
        }
    }
    function SendAssetInfo() {
        $.ajax({
            url: "/AssetManagement/AssetMaintenanceInfoDetail/SendAssetInfo",
            data: {
                "Vguid": $("#VGUID").val()
            },
            type: "post",
            success: function (msg) {
                var templateResult = "";
                switch (msg.Status) {
                    case "0":
                        templateResult = "error";
                        $("#notificationContentSendAPI").html("调用接口失败");
                        break;
                    case "1":
                        templateResult = "success";
                        $("#notificationContentSendAPI").html("调用接口成功");
                        $("#STATUS").val("已验收");
                        break;
                }
                $("#jqxNotificationSendAPT").jqxNotification({
                    width: "100%", position: "top-left", opacity: 1,
                    autoOpen: false, animationOpenDelay: 800, appendContainer: "#ResultDiv", autoClose: false, template: templateResult, closeOnClick: false
                });
                $("#jqxNotificationSendAPT").jqxNotification("open");
            }
        });
    }
};
function formatDate(NewDtime) {
    var dt = new Date(parseInt(NewDtime.slice(6, 19)));
    var year = dt.getFullYear();
    var month = dt.getMonth() + 1;
    var date = dt.getDate();
    var hour = dt.getHours();
    var minute = dt.getMinutes();
    var second = dt.getSeconds();
    return year + "-" + month + "-" + date;
    //+ " " + hour + ":" + minute + ":" + second;
}
function doPrint() {
    $("#GenerateAcceptanceDialog").hide();
    $(".gridtable")[0].style.margin = "60px"
    bdhtml = window.document.body.innerHTML;
    sprnstr = "<!--startprint-->";
    eprnstr = "<!--endprint-->";
    prnhtml = bdhtml.substr(bdhtml.indexOf(sprnstr) + 17);
    prnhtml = prnhtml.substring(0, prnhtml.indexOf(eprnstr));
    window.document.body.innerHTML = prnhtml;
    window.print();
    window.location.reload()
}
function ValidateEmpty(obj) {
    if ($(obj).val() == "") {
        return false;
    } else {
        return true;
    }
}
$(function () {
    var page = new $page();
    page.init();
});
