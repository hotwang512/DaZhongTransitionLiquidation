//资产基础信息维护明细
var vguid = "";
var mydate = new Date();
var $page = function () {
    this.init = function () {
        addEvent();
    }
    //所有事件
    function addEvent() {
        
        initSelect();
        initSelectMinor();
        var guid = $.request.queryString().VGUID;
        $("#VGUID").val(guid);
        if (guid != "" && guid != null) {
            getOrderSettingInfoListDetail();
        } else {
           // $("#ASSET_CATEGORY_MAJOR").jqxComboBox('clearSelection');
            $("#hideButton").show();
        }
        //取消
        $("#btnCancel").on("click", function () {
            history.go(-1);
        })
        //保存
        $("#btnSave").on("click",
            function() {
                var validateError = 0; //未通过验证的数量
                if (!Validate($("#PurchaseGoods"))) {
                    validateError++;
                }
                if (!Validate($("#ASSET_CATEGORY_MAJOR"))) {
                    validateError++;
                }
                if (!Validate($("#ASSET_CATEGORY_MINOR"))) {
                    validateError++;
                }
                if (validateError <= 0) {
                    $.ajax({
                        url: "/Systemmanagement/PurchaseOrderSettingDetail/SavePurchaseOrderSetting",
                        data: {
                            "VGUID": $("#VGUID").val(),
                            "AssetCategoryMajor": $("#AssetCategoryMajor").val(),
                            "AssetCategoryMinor": $("#AssetCategoryMinor").val(),
                            "OrderCategory": $("#OrderCategory").val(),
                            "AssetCategoryMinorVguid": $("#AssetCategoryMinorVguid").val(),
                            "PurchaseGoods": $("#PurchaseGoods").val()
                        },
                        type: "post",
                        success: function(msg) {
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
            });
        $("#ASSET_CATEGORY_MAJOR").on('select', function (event) {
            if (event.args) {
                var item = event.args.item;
                $("#AssetCategoryMajor").val(item.label);
                initSelectMinor();
            }
        });
        $("#ASSET_CATEGORY_MINOR").on('select', function (event) {
            if (event.args) {
                var item = event.args.item;
                $("#AssetCategoryMinor").val(item.label);
                $("#AssetCategoryMinorVguid").val(item.value);
            }
        });
    }; //addEvent end

    function getOrderSettingInfoListDetail() {
        
        $.ajax({
            url: "/Systemmanagement/PurchaseOrderSettingDetail/GetPurchaseOrderSettingDetail",
            data: {
                "vguid": $("#VGUID").val()
            },
            type: "post",
            async: false,
            dataType: "json",
            success: function (msg) {
                $("#PurchaseGoods").val(msg.PurchaseGoods);
                $("#OrderCategory").val(msg.OrderCategory);
                $("#AssetCategoryMinor").val(msg.AssetCategoryMinor);
                $("#AssetCategoryMajor").val(msg.AssetCategoryMajor);
                $("#AssetCategoryMinorVguid").val(msg.AssetCategoryMinorVguid);
                $("#ASSET_CATEGORY_MAJOR").val(msg.AssetCategoryMajor);
                initSelectMinor(msg.AssetCategoryMinorVguid);
            }
        });
    }
    function initSelect() {
        $.ajax({
            url: "/AssetManagement/AssetBasicInfoMaintenance/GetMajorListDatas",
            type: "post",
            async: false,
            success: function (data) {
                var arr = [];
                for (var i = 0; i < data.length; i++) {
                    arr.push(data[i].AssetMajor);
                }
                var dataAdapter = new $.jqx.dataAdapter(arr);
                $("#ASSET_CATEGORY_MAJOR").jqxDropDownList({ selectedIndex: 0, source: dataAdapter, width: 198, height: 33 });
                $("#ASSET_CATEGORY_MAJOR").jqxDropDownList({ itemHeight: 33 });
                $("#AssetCategoryMajor").val($("#ASSET_CATEGORY_MAJOR").val());
                initSelectMinor();
            }
        });

    }
    function initSelectMinor() {
        var source =
        {
            data: {
                "MAJOR": $("#AssetCategoryMajor").val()
            },
            datatype: "json",
            type: "post",
            datafields: [
                { name: 'AssetMinor' },
                { name: 'AssetMinorVguid' }
            ],
            url: "/Systemmanagement/PurchaseOrderSettingDetail/GetMinorListDatas",
            async: false
        };
        var dataAdapter = new $.jqx.dataAdapter(source);
        $("#ASSET_CATEGORY_MINOR").jqxDropDownList({ selectedIndex: 0, source: dataAdapter, displayMember: "AssetMinor", valueMember: "AssetMinorVguid", width: 198, height: 33 });
        $("#ASSET_CATEGORY_MINOR").jqxDropDownList({ itemHeight: 33 });
        if ($("#AssetCategoryMinorVguid").val() != "") {
            debugger;
            $("#ASSET_CATEGORY_MINOR").val($("#AssetCategoryMinorVguid").val());
        } else {
            $("#AssetCategoryMinorVguid").val($("#ASSET_CATEGORY_MINOR").val());
            debugger;
            $("#AssetCategoryMinor").val($("#ASSET_CATEGORY_MINOR").jqxDropDownList('getSelectedItem').label);
        }
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
