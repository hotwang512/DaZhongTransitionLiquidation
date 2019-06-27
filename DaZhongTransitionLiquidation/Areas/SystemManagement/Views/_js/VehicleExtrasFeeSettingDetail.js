//车辆类资产费用标准配置明细
var vguid = "";
var mydate = new Date();
var ManagementCompany = [];
var $page = function () {
    this.init = function () {
        GetVehicleModelDropDown();
        initBusinessProject();
        //initBusinessSubItem();
        addEvent();
    }
    //所有事件
    function addEvent() {
        var guid = $.request.queryString().VGUID;
        $("#VGUID").val(guid);
        if (guid != "" && guid != null) {
            getOrderSettingInfoListDetail();
        } else {
            // $("#ASSET_CATEGORY_MAJOR").jqxComboBox('clearSelection');
            $("#hideButton").show();
        }
        //取消
        $("#btnCancel").on("click",
            function () {
                history.go(-1);
            });
        //保存
        $("#btnSave").on("click",
            function () {
                var validateError = 0; //未通过验证的数量
                if (!Validate($("#Fee"))) {
                    validateError++;
                }
                if (validateError <= 0) {
                    debugger;
                    $.ajax({
                        url: "/Systemmanagement/VehicleExtrasFeeSettingDetail/SaveVehicleExtrasFeeSetting",
                        data: {
                            "VGUID": $("#VGUID").val(),
                            "VehicleModelCode": $("#VehicleModel").val(),
                            "VehicleModel": $("#VehicleModel").find("option:selected").text(),
                            "Fee": $("#Fee").val(),
                            "Status": $("#Status").val(),
                            "BusinessSubItem": $("#BusinessSubItem").text(),
                            "BusinessProject": $("#BusinessProject").text()
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
            });
    }; //addEvent end
    function GetVehicleModelDropDown() {
        $.ajax({
            url: "/Systemmanagement/VehicleExtrasFeeSettingDetail/GetVehicleModelDropDown",
            type: "GET",
            dataType: "json",
            async: false,
            success: function (msg) {
                debugger;
                uiEngineHelper.bindSelect('#VehicleModel', msg, "Code", "Descrption");
                $("#VehicleModel").prepend("<option value=\"\" selected='true'>请选择</>");
                debugger;
            }
        });
    }
    function getOrderSettingInfoListDetail() {
        $.ajax({
            url: "/Systemmanagement/VehicleExtrasFeeSettingDetail/GetVehicleExtrasFeeSettingDetail",
            data: {
                "vguid": $("#VGUID").val()
            },
            type: "post",
            async: false,
            dataType: "json",
            success: function (msg) {
                $("#VehicleModel").val(msg.ResultInfo.VehicleModelCode);
                $("#Fee").val(msg.ResultInfo.Fee);
                $("#Status").val(msg.ResultInfo.Status);
                $("#BusinessSubItem").text(msg.ResultInfo.BusinessSubItem);
                $("#BusinessProject").val(msg.ResultInfo.BusinessSubItem);
            }
        });
    }
    function initBusinessProject() {
        var url = "/Systemmanagement/VehicleExtrasFeeSettingDetail/GetBusinessProject";
        // prepare the data
        var source =
        {
            datatype: "json",
            data: {
                "BusinessProject": ""
            },
            datafields: [
                { name: 'BusinessProject' },
                { name: 'BusinessSubItem1' }
            ],
            url: url,
            async: false
        };
        var dataAdapter = new $.jqx.dataAdapter(source);
        $("#BusinessProject").jqxDropDownList({
            selectedIndex: 0,
            filterable: true,
            source: dataAdapter,
            displayMember: "BusinessProject",
            valueMember: "BusinessSubItem1",
            searchMode: 'contains',
            width: 200,
            height: 30
        });
        $("#BusinessProject").on('select', function (event) {
            if (event.args) {
                var item = event.args.item;
                debugger;
                if (item) {
                    $("#BusinessSubItem").text(event.args.item.value);
                }
            }
        });
    }
};
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}
$(function () {
    var page = new $page();
    page.init();
});
