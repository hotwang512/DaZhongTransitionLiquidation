var vguid = "";
var mydate = new Date();
var ManagementCompany = [];
var dataArr = [];
var $page = function () {
    this.init = function () {
        //$("#DateOfYear").attr("disabled",true);
        if ($("#EditPermission").val() == "True" || $("#NewPermission").val() == "True") {
            $("#DriversIncomeDialog_OKBtn").show();
        } else {
            $("#DriversIncomeDialog_OKBtn").hide();
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
                //GetDriversIncome();
                $("#DriversIncomeModalDialog").modal("show");
            });
        $("#VehicleModel").on("change",
            function () {
                GetDriversIncomeYear();
            });
        $("#VehicleModelNew").on("change",
            function () {
                GetDriversIncome();
            });
        $("#DateOfYear").on("blur",
            function () {
                GetDriversIncome();
            });
        $("#DriversIncomeDialog_OKBtn").on("click", function () {
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
                        "SingleBus": rows[j].SingleBus, "DoubleBus": rows[j].DoubleBus, "Zorder": rows[j].Zorder,
                        "ProjectName": rows[j].ProjectName, "DateOfYear": rows[j].DateOfYear
                    });
                };
                $.ajax({
                    url: "/AnalysisManagementCenter/DriversIncome/SaveDriversIncome",
                    data: {
                        "DriversIncomeList": SettingList
                    },
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                $("#DriversIncomeModalDialog").modal("hide");
                                GetDriversIncomeYear();
                                //window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                                break;
                        }
                    }
                });
            }
        });
        $("#DriversIncomeDialog_CancelBtn").on("click", function () {
            $("#DriversIncomeModalDialog").modal("hide");
        });
    }; //addEvent end
    function GetDriversIncome() {
        if ($("#VehicleModelNew").val() != "") {
            $.ajax({
                url: "/AnalysisManagementCenter/DriversIncome/GetDriversIncomeDetail",
                data: {
                    "VehicleModel": $("#VehicleModelNew").val(),
                    "VehicleModelName": $("#VehicleModelNew option:selected").text(),
                    "DateOfYear": $("#DateOfYear").val()
                },
                type: "GET",
                dataType: "json",
                async: false,
                success: function (data) {
                    GetDriversIncomeDetail(data);
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
    function GetDriversIncomeYear() {
        $.ajax({
            url: "/AnalysisManagementCenter/DriversIncome/GetDriversIncomeYear",
            data: { "VehicleModel": $("#VehicleModel").val() },
            type: "GET",
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.length > 0) {
                    $("#tableList").show();
                    $("#tableList1").show();
                    dataArr = data;
                    GetDriversIncomeList(data, "SingleBus");
                } else {
                    $("#tableList").hide();
                    $("#tableList1").hide();
                }
            }
        });
    }
    function GetDriversIncomeDetail(data) {
        var ordersSource =
        {
            dataFields: [
                { name: 'VehicleModel', type: 'string' },
                { name: 'VehicleModelName', type: 'string' },
                { name: 'ProjectName', type: 'string' },
                { name: 'SingleBus', type: 'string' },
                { name: 'DoubleBus', type: 'string' },
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
                {
                    text: '单班车',
                    editable: true,
                    dataField: 'SingleBus',
                    width: 220,
                    cellsAlign: 'center',
                    align: 'center',
                    cellsformat: 'F2', 
                    createeditor: function(row, cellvalue, editor) {
                        editor.jqxNumberInput({ digits: 3 });
                    }
                },
                {
                    text: '双班车', editable: true, dataField: 'DoubleBus', width: 210, cellsAlign: 'center', align: 'center', cellsformat: 'F2', //columntype: 'numberinput',
                    createeditor: function (row, cellvalue, editor) {
                        editor.jqxNumberInput({ digits: 3 });
                    }
                },
                { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ]
        });
        $("#table").on('cellendedit', function (event) {
            var args = event.args;
            if (args.datafield == "SingleBus") {
                window.setTimeout(function() {
                    var rows = $('#table').jqxGrid('getboundrows');
                    var singleBusSum = 0;
                    for (var j = 0; j < rows.length; j++) {
                        if (rows[j].SingleBus != null && rows[j].boundindex >= 2 && rows[j].boundindex <= 10) {
                            singleBusSum += rows[j].SingleBus;
                        }
                    };
                    $("#table").jqxGrid('setcellvalue', 1, "SingleBus", singleBusSum);
                    $("#table").jqxGrid('setcellvalue', 13, "SingleBus", rows[12].SingleBus / 2);
                }, 200);
            } else {
                window.setTimeout((function() {
                    var rows = $('#table').jqxGrid('getboundrows');
                    var doubleBusSum = 0;
                    for (var j = 0; j < rows.length; j++) {
                        if (rows[j].SingleBus != null && rows[j].boundindex > 2 && rows[j].boundindex < 11) {
                            doubleBusSum += rows[j].DoubleBus;
                        }
                    };
                    $("#table").jqxGrid('setcellvalue', 1, "SingleBus", doubleBusSum);
                    $("#table").jqxGrid('setcellvalue', 13, "SingleBus", rows[12].DoubleBus / 2);
                }, 200));
            }
        });
    }
    function GetDriversIncomeList(data, SingleOrDouble) {
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
                align: 'center',
                cellclassname: function (row, column, value, data) {
                    debugger;
                    if (row == 0 || row == 2 || row == 12 || row == 11) {
                        return "fontformat";
                    } else if (row === 13) {
                        return "colorformat";
                    }
                }
            }
        ];
        for (var i = 0; i < data.length; i++) {
            datafields.push({ name: data[i], type: 'stirng' });
            columns.push({
                text: data[i] + "年", datafield: data[i], editable: false, width: 100, align: 'center', cellsAlign: 'center',
                cellclassname: function (row, column, value, data) {
                    debugger;
                    if (row == 0 || row == 2 || row == 12 || row == 11) {
                        return "fontformat";
                    } else if (row === 13) {
                        return "colorformat";
                    }
                }
            });
        }
        debugger;
        var paras = "";
        for (var k = 0; k < data.length; k++) {
            paras += "[" + data[k] + "],";
        }
        paras = paras.substring(0, paras.length - 1);
        var url = "/AnalysisManagementCenter/DriversIncome/GetDriversIncomeList";
        var ordersSource =
        {
            dataFields: datafields,
            dataType: "json",
            id: 'VGUID',
            url: url,
            data: { "VehicleModel": $("#VehicleModel").val(), paras: paras, SingleOrDouble: SingleOrDouble },
            updateRow: function (rowID, rowData, commit) {
                commit(true);
            }
        };
        var dataAdapter = new $.jqx.dataAdapter(ordersSource, {
            loadComplete: function () {
                GetDriversIncomeList1(dataArr, "DoubleBus");
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
    function GetDriversIncomeList1(data, SingleOrDouble) {
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
                align: 'center',
                cellclassname: function (row, column, value, data) {
                    debugger;
                    if (row == 0 || row == 2 || row == 12 || row == 11) {
                        return "fontformat";
                    } else if (row === 13) {
                        return "colorformat";
                    }
                }
            }
        ];
        for (var i = 0; i < data.length; i++) {
            datafields.push({ name: data[i], type: 'stirng' });
            columns.push({ text: data[i] + "年", datafield: data[i], editable: false, width: 100, align: 'center', cellsAlign: 'center',
                cellclassname: function (row, column, value, data) {
                    debugger;
                    if (row == 0 || row == 2 || row == 12 || row == 11) {
                        return "fontformat";
                    } else if (row === 13) {
                        return "colorformat";
                    }
                } });
        }
        debugger;
        var paras = "";
        for (var k = 0; k < data.length; k++) {
            paras += "[" + data[k] + "],";
        }
        paras = paras.substring(0, paras.length - 1);
        var url = "/AnalysisManagementCenter/DriversIncome/GetDriversIncomeList";
        var ordersSource =
        {
            dataFields: datafields,
            dataType: "json",
            id: 'VGUID',
            url: url,
            data: { "VehicleModel": $("#VehicleModel").val(), paras: paras, SingleOrDouble: SingleOrDouble },
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