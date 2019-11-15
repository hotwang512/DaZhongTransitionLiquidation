//车辆类资产费用标准配置明细
var vguid = "";
var mydate = new Date();
var ManagementCompany = [];
var dataArr = [];
var $page = function () {
    this.init = function () {
        //$("#DateOfYear").attr("disabled",true);
        if ($("#EditPermission").val() == "True" || $("#NewPermission").val() == "True") {
            $("#FreightAnalysisDialog_OKBtn").show();
        } else {
            $("#FreightAnalysisDialog_OKBtn").hide();
        }
        GetVehicleModelDropDown();
        addEvent();
    }
    //所有事件
    function addEvent() {
        //保存
        $("#btnNew").on("click",
            function () {
                var date = new Date();
                $("#DateOfYear").val(date.getFullYear());
                //$("#VehicleModelNew").val("");
                //GetFreightAnalysis();
                $("#FreightAnalysisModalDialog").modal("show");
            });
        $("#VehicleModel").on("change",
            function () {
                GetFreightAnalysisYear();
            });
        $("#VehicleModelNew").on("change",
            function () {
                GetFreightAnalysis();
            });
        $("#DateOfYear").on("blur",
            function () {
                GetFreightAnalysis();
            });
        $("#FreightAnalysisDialog_OKBtn").on("click", function () {
            var validateError = 0;
            if (!Validate($("#VehicleModelNew"))) {
                validateError++;
            }
            if (!Validate($("#DateOfYear"))) {
                validateError++;
            }
            if (validateError <= 0) {
                var rows = $('#table').jqxGrid('getboundrows');
                var SettingList = [];
                for (var j = 0; j < rows.length; j++) {
                    SettingList.push({
                        "VGUID": rows[j].VGUID, "VehicleModel": rows[j].VehicleModel, "VehicleModelName": rows[j].VehicleModelName,
                        "DayDetail": rows[j].DayDetail, "NightDetail": rows[j].NightDetail, "Zorder": rows[j].Zorder,
                        "ProjectName": rows[j].ProjectName, "DateOfYear": rows[j].DateOfYear
                    });
                };
                $.ajax({
                    url: "/AnalysisManagementCenter/FreightAnalysis/SaveFreightAnalysis",
                    data: {
                        "FreightAnalysisList": SettingList
                    },
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                $("#FreightAnalysisModalDialog").modal("hide");
                                GetFreightAnalysisYear();
                                //window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                                break;
                        }
                    }
                });
            }
        });
        $("#FreightAnalysisDialog_CancelBtn").on("click", function () {
            $("#FreightAnalysisModalDialog").modal("hide");
        });
    }; //addEvent end
    function GetFreightAnalysis() {
        if ($("#VehicleModelNew").val() != "") {
            $.ajax({
                url: "/AnalysisManagementCenter/FreightAnalysis/GetFreightAnalysisDetail",
                data: {
                    "VehicleModel": $("#VehicleModelNew").val(),
                    "VehicleModelName": $("#VehicleModelNew option:selected").text(),
                    "DateOfYear": $("#DateOfYear").val()
                },
                type: "GET",
                dataType: "json",
                async: false,
                success: function (data) {
                    GetFreightAnalysisDetail(data);
                }
            });
        }
    }
    function GetVehicleModelDropDown() {
        $.ajax({
            url: "/Systemmanagement/VehicleExtrasFeeSettingDetail/GetVehicleModelDropDown",
            type: "GET",
            dataType: "json",
            async: false,
            success: function (msg) {
                uiEngineHelper.bindSelect('#VehicleModel', msg, "Code", "Descrption");
                $("#VehicleModel").prepend("<option value=\"\" selected='true'>请选择</>");
                uiEngineHelper.bindSelect('#VehicleModelNew', msg, "Code", "Descrption");
                $("#VehicleModelNew").prepend("<option value=\"\" selected='true'>请选择</>");
            }
        });
    }
    function GetFreightAnalysisYear() {
        $.ajax({
            url: "/AnalysisManagementCenter/FreightAnalysis/GetFreightAnalysisYear",
            data: { "VehicleModel": $("#VehicleModel").val() },
            type: "GET",
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.length > 0) {
                    $("#tableList").show();
                    $("#tableList1").show();
                    dataArr = data;
                    GetFreightAnalysisList(data, "DayDetail");
                } else {
                    $("#tableList").hide();
                    $("#tableList1").hide();
                }
            }
        });
    }
    function GetFreightAnalysisDetail(data) {
        var ordersSource =
        {
            dataFields: [
                { name: 'VehicleModel', type: 'string' },
                { name: 'VehicleModelName', type: 'string' },
                { name: 'ProjectName', type: 'string' },
                { name: 'DayDetail', type: 'string' },
                { name: 'NightDetail', type: 'string' },
                { name: 'DateOfYear', type: 'string' },
                { name: 'Zorder', type: 'string' },
                { name: 'VGUID', type: 'string' }
            ],
            dataType: "json",
            id: 'VGUID',
            localdata: data,
            updateRow: function (rowID, rowData, commit) {
                commit(true);
            }
        };
        var dataAdapter = new $.jqx.dataAdapter(ordersSource, {
            loadComplete: function () {
            }
        });
        var editrow = -1;
        $("#table").jqxGrid(
        {
            width: "100%",
            height: "300px",
            source: dataAdapter,
            selectionmode: 'singlerow',
            editable: true,
            ready: function () {
            },
            toolbarHeight: 35,
            columns: [
                { text: '车型', editable: false, dataField: 'VehicleModelName', width: 100, cellsAlign: 'center', align: 'center' },
                { text: '项目', editable: false, dataField: 'ProjectName', width: 157, cellsAlign: 'center', align: 'center' },
                { text: '白天', editable: true, dataField: 'DayDetail', width: 200, cellsAlign: 'center', align: 'center' },
                { text: '夜间', editable: true, dataField: 'NightDetail', width: 200, cellsAlign: 'center', align: 'center' },
                { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ]
        });
    }
    function GetFreightAnalysisList(data, DayOrNight) {
        debugger;
        var datafields = [
            { name: 'ProjectName', type: 'string' }
        ];
        var columns = [
            {
                text: '项目',
                editable: false,
                dataField: 'ProjectName',
                width: 200,
                cellsAlign: 'center',
                align: 'center'
            }
        ];
        for (var i = 0; i < data.length; i++) {
            datafields.push({ name: data[i], type: 'stirng' });
            columns.push({ text: data[i] + "年", datafield: data[i], editable: false, width: 100, align: 'center', cellsAlign: 'center' });
        }
        debugger;
        var paras = "";
        for (var k = 0; k < data.length; k++) {
            paras += "[" + data[k] + "],";
        }
        paras = paras.substring(0, paras.length - 1);
        var url = "/AnalysisManagementCenter/FreightAnalysis/GetFreightAnalysisList";
        var ordersSource =
        {
            dataFields: datafields,
            dataType: "json",
            id: 'VGUID',
            url: url,
            data: { "VehicleModel": $("#VehicleModel").val(), paras: paras, DayOrNight: DayOrNight },
            updateRow: function (rowID, rowData, commit) {
                commit(true);
            }
        };
        var dataAdapter = new $.jqx.dataAdapter(ordersSource, {
            loadComplete: function () {
                GetFreightAnalysisList1(dataArr, "NightDetail");
            }
        });
        $("#tableList").jqxGrid(
            {
                width: "100%",
                height: "100%",
                source: dataAdapter,
                selectionmode: 'singlerow',
                editable: true,
                editmode: 'selectedcell',
                ready: function () {
                },
                columns: columns
            });
    }
    function GetFreightAnalysisList1(data, DayOrNight) {
        debugger;
        var datafields = [
            { name: 'ProjectName', type: 'string' }
        ];
        var columns = [
            {
                text: '项目',
                editable: false,
                dataField: 'ProjectName',
                width: 200,
                cellsAlign: 'center',
                align: 'center'
            }
        ];
        for (var i = 0; i < data.length; i++) {
            datafields.push({ name: data[i], type: 'stirng' });
            columns.push({ text: data[i] + "年", datafield: data[i], editable: false, width: 100, align: 'center', cellsAlign: 'center' });
        }
        debugger;
        var paras = "";
        for (var k = 0; k < data.length; k++) {
            paras += "[" + data[k] + "],";
        }
        paras = paras.substring(0, paras.length - 1);
        var url = "/AnalysisManagementCenter/FreightAnalysis/GetFreightAnalysisList";
        var ordersSource =
        {
            dataFields: datafields,
            dataType: "json",
            id: 'VGUID',
            url: url,
            data: { "VehicleModel": $("#VehicleModel").val(), paras: paras, DayOrNight: DayOrNight },
            updateRow: function (rowID, rowData, commit) {
                commit(true);
            }
        };
        var dataAdapter = new $.jqx.dataAdapter(ordersSource, {
            loadComplete: function () {
            }
        });
        $("#tableList1").jqxGrid(
            {
                width: "100%",
                height: "100%",
                source: dataAdapter,
                selectionmode: 'singlerow',
                editable: true,
                editmode: 'selectedcell',
                ready: function () {
                },
                columns: columns
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
function formatDate(NewDtime) {
    var d = NewDtime;
    var datetime = d.getFullYear() + '-' + (d.getMonth() + 1);//  + '-' + d.getDate() + ' ' + d.getHours() + ':' + d.getMinutes() + ':' + d.getSeconds()
    return datetime;
}