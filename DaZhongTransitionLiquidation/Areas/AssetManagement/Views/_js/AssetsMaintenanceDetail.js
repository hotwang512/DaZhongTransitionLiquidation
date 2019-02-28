//资产基础信息维护明细
var vguid = "";
var mydate = new Date();
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
            if (!Validate($("#BOOK_TYPE_CODE"))) { validateError++; }
            if (!Validate($("#TAG_NUMBER"))) { validateError++; }
            if (!Validate($("#DESCRIPTION"))) { validateError++; }
            if (!Validate($("#QUANTITY"))) { validateError++; }
            if (!Validate($("#ASSET_CATEGORY_MAJOR"))) { validateError++; }
            if (!Validate($("#ASSET_CATEGORY_MINOR"))) { validateError++; }
            if (!Validate($("#ASSET_CREATION_DATE"))) { validateError++; }
            if (!Validate($("#ASSET_COST"))) { validateError++; }
            if (!Validate($("#SALVAGE_TYPE"))) { validateError++; }
            if (!Validate($("#METHOD"))) { validateError++; }
            if (!Validate($("#LIFE_MONTHS"))) { validateError++; }
            if (!Validate($("#EXP_ACCOUNT_SEGMENT1"))) { validateError++; }
            if (!Validate($("#EXP_ACCOUNT_SEGMENT2"))) { validateError++; }
            if (!Validate($("#EXP_ACCOUNT_SEGMENT3"))) { validateError++; }
            if (!Validate($("#EXP_ACCOUNT_SEGMENT4"))) { validateError++; }
            if (!Validate($("#EXP_ACCOUNT_SEGMENT5"))) { validateError++; }
            if (!Validate($("#EXP_ACCOUNT_SEGMENT6"))) { validateError++; }
            if (!Validate($("#EXP_ACCOUNT_SEGMENT7"))) { validateError++; }
            if (!Validate($("#FA_LOC_1"))) { validateError++; }
            if (!Validate($("#FA_LOC_2"))) { validateError++; }
            if (!Validate($("#FA_LOC_3"))) { validateError++; }
            if (!Validate($("#TRANSACTION_ID"))) { validateError++; }
            if (!Validate($("#LAST_UPDATE_DATE"))) { validateError++; }
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
                        "OBD_FEE": $("#OBD_FEE").val(),
                        "STATUS": $("#STATUS").val(),
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
                        }
                    }
                });
            }
        })
        //弹出框中的取消按钮
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
                $("#LISENSING_FEE").val(msg.LISENSING_FEE);
                $("#OUT_WAREHOUSE_FEE").val(msg.OUT_WAREHOUSE_FEE);
                $("#DOME_LIGHT_FEE").val(msg.DOME_LIGHT_FEE);
                $("#ANTI_ROBBERY_FEE").val(msg.ANTI_ROBBERY_FEE);
                $("#LOADING_FEE").val(msg.LOADING_FEE);
                $("#INNER_ROOF_FEE").val(msg.INNER_ROOF_FEE);
                $("#TAXIMETER_FEE").val(msg.TAXIMETER_FEE);
                $("#OBD_FEE").val(msg.OBD_FEE);
                if (msg.CHANGE_DATE != "" && msg.CHANGE_DATE != null) {
                    $("#CHANGE_DATE").val(formatDate(msg.CHANGE_DATE));
                }
                if (msg.CREATE_DATE != "" && msg.CREATE_DATE != null) {
                    $("#CREATE_DATE").val(formatDate(msg.CREATE_DATE));
                }
                $("#CREATE_USER").val(msg.CREATE_USER);
                $("#CHANGE_USER").val(msg.CHANGE_USER);
                $("#STATUS").val(msg.STATUS);
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
$(function () {
    var page = new $page();
    page.init();
});
