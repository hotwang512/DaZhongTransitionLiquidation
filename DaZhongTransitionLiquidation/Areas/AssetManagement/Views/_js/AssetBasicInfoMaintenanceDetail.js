//资产基础信息维护明细
var vguid = "";
var mydate = new Date();
var $page = function () {
    this.init = function () {
        addEvent();
    }
    //所有事件
    function addEvent() {
        debugger;
        initSelect();
        var guid = $.request.queryString().VGUID;
        $("#VGUID").val(guid);
        if (guid != "" && guid != null) {
            getAssetInfoListDetail();
        } else {
            $("#ASSET_CATEGORY_MAJOR").jqxComboBox('clearSelection');
            $("#hideButton").show();
        }
        //取消
        $("#btnCancel").on("click", function () {
            history.go(-1);
        })
        //保存
        $("#btnSave").on("click", function () {
            var validateError = 0;//未通过验证的数量
            debugger;
            if ($("#ASSET_CATEGORY_MAJOR").find("Input").val() == "") {
                $("#ASSET_CATEGORY_MAJOR").find("Input").addClass("input_Validate");
                validateError++;
            }
            //if (!Validate($("#ASSET_CATEGORY_MAJOR"))) { validateError++; }
            if (!Validate($("#ASSET_CATEGORY_MINOR"))) { validateError++; }
            if (!Validate($("#LIFE_YEARS"))) { validateError++; }
            if (!Validate($("#LIFE_MONTHS"))) { validateError++; }
            if (!Validate($("#SALVAGE_PERCENT"))) { validateError++; }
            if (!Validate($("#METHOD"))) { validateError++; }
            if (!Validate($("#BOOK_TYPE_CODE"))) { validateError++; }
            if (!Validate($("#ASSET_COST_ACCOUNT"))) { validateError++; }
            if (!Validate($("#ASSET_SETTLEMENT_ACCOUNT"))) { validateError++; }
            if (!Validate($("#DEPRECIATION_EXPENSE_SEGMENT"))) { validateError++; }
            if (!Validate($("#ACCT_DEPRECIATION_ACCOUNT"))) { validateError++; }
            if (validateError <= 0) {
                $.ajax({
                    url: "/AssetManagement/AssetBasicInfoMaintenanceDetail/SaveAssetBasicInfo",
                    data: {
                        "VGUID": $("#VGUID").val(),
                        "ASSET_CATEGORY_MAJOR": $("#ASSET_CATEGORY_MAJOR").find("Input").val(),
                        "ASSET_CATEGORY_MINOR": $("#ASSET_CATEGORY_MINOR").val(),
                        "LIFE_YEARS": $("#LIFE_YEARS").val(),
                        "LIFE_MONTHS": $("#LIFE_MONTHS").val(),
                        "SALVAGE_PERCENT": $("#SALVAGE_PERCENT").val().replace("%", ""),
                        "METHOD": $("#METHOD").val(),
                        "BOOK_TYPE_CODE": $("#BOOK_TYPE_CODE").val(),
                        "ASSET_COST_ACCOUNT": $("#ASSET_COST_ACCOUNT").val(),
                        "ASSET_SETTLEMENT_ACCOUNT": $("#ASSET_SETTLEMENT_ACCOUNT").val(),
                        "DEPRECIATION_EXPENSE_SEGMENT": $("#DEPRECIATION_EXPENSE_SEGMENT").val(),
                        "ACCT_DEPRECIATION_ACCOUNT": $("#ACCT_DEPRECIATION_ACCOUNT").val(),
                        "CREATE_TIME": $("#CREATE_TIME").val(),
                        "CREATE_USER": $("#CREATE_USER").val(),
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
    }; //addEvent end

    function getAssetInfoListDetail() {
        debugger;
        $.ajax({
            url: "/AssetManagement/AssetBasicInfoMaintenanceDetail/GetAssetBasicInfoDetail",
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
                //$("#ASSET_CATEGORY_MAJOR").val(msg.ASSET_CATEGORY_MAJOR);
                $("#ASSET_CATEGORY_MAJOR").find("Input").val(msg.ASSET_CATEGORY_MAJOR);
                $("#ASSET_CATEGORY_MINOR").val(msg.ASSET_CATEGORY_MINOR);
                $("#LIFE_YEARS").val(msg.LIFE_YEARS);
                $("#LIFE_MONTHS").val(msg.LIFE_MONTHS);
                if (msg.SALVAGE_PERCENT != "" && msg.SALVAGE_PERCENT != null) {
                    $("#SALVAGE_PERCENT").val(msg.SALVAGE_PERCENT + "%");
                } else {
                    $("#SALVAGE_PERCENT").val("0%");
                }
                $("#METHOD").val(msg.METHOD);
                $("#BOOK_TYPE_CODE").val(msg.BOOK_TYPE_CODE);
                $("#ASSET_COST_ACCOUNT").val(msg.ASSET_COST_ACCOUNT);
                $("#ASSET_SETTLEMENT_ACCOUNT").val(msg.ASSET_SETTLEMENT_ACCOUNT);
                $("#DEPRECIATION_EXPENSE_SEGMENT").val(msg.DEPRECIATION_EXPENSE_SEGMENT);
                $("#ACCT_DEPRECIATION_ACCOUNT").val(msg.ACCT_DEPRECIATION_ACCOUNT);
                //$("#CREATE_TIME").val(formatDate(msg.CREATE_TIME));
                $("#CREATE_USER").val(msg.CREATE_USER);
                //$("#CHANGE_TIME").val(formatDate(msg.CHANGE_TIME));
                $("#CHANGE_USER").val(msg.CHANGE_USER);
            }
        });
    }
    function initSelect() {
        $.ajax({
            url: "/AssetManagement/AssetBasicInfoMaintenance/GetMajorListDatas",
            type: "post",
            success: function (data) {
                debugger;
                var arr = [];
                for (var i = 0; i < data.length; i++) {
                    arr.push(data[i].AssetMajor);
                }
                if (arr.length == 0) {
                    arr.push("Default");
                }
                var dataAdapter = new $.jqx.dataAdapter(arr);
                $("#ASSET_CATEGORY_MAJOR").jqxComboBox({ selectedIndex: 0, source: dataAdapter, width: 198, height: 33 });
                $("#ASSET_CATEGORY_MAJOR").jqxComboBox({ itemHeight: 33 });
                $("#ASSET_CATEGORY_MAJOR input").click(function () {
                    $("#ASSET_CATEGORY_MAJOR").jqxComboBox('clearSelection');
                })
                $("#dropdownlistWrapperASSET_CATEGORY_MAJOR Input")[0].style.paddingLeft = "10px"
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
    return year + "-" + month + "-" + date + " " + hour + ":" + minute + ":" + second;
}
$(function () {
    var page = new $page();
    page.init();
});
