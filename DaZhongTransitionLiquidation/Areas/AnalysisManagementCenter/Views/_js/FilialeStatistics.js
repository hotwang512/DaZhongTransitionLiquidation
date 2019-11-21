var vguid = "";
var mydate = new Date();
var ManagementCompany = [];
var dataArr = [];
var $page = function () {
    this.init = function () {
        var date = new Date();
        $("#DateOfYear").val(date.getFullYear());
        GetFilialeStatisticsDetail();
        //$("#DateOfYear").attr("disabled",true);
        if ($("#EditPermission").val() == "True" || $("#NewPermission").val() == "True") {
            $("#FilialeStatisticsDialog_OKBtn").show();
        } else {
            $("#FilialeStatisticsDialog_OKBtn").hide();
        }
        addEvent();
    }
    //所有事件
    function addEvent() {
        //保存
        $("#DateOfYear").on("blur",
            function () {
                GetFilialeStatisticsDetail();
            });
        $("#btnSave").on("click", function () {
            var rows = $('#table').jqxDataTable('getRows');
            var SettingList = [];
            for (var j = 0; j < rows.length; j++) {
                SettingList.push({
                    "VGUID": rows[j].VGUID, "YearMonth": rows[j].YearMonth,
                    "BeginningPeriod": rows[j].BeginningPeriod, "CurrentPeriodAdded": rows[j].CurrentPeriodAdded,
                    "Clear": rows[j].Clear, "Retirement": rows[j].Retirement,
                    "Zorder": rows[j].Zorder,
                    "EndPeriod": rows[j].EndPeriod, "DateOfYear": rows[j].DateOfYear
                });
            };
            $.ajax({
                url: "/AnalysisManagementCenter/FilialeStatistics/SaveFilialeStatistics",
                data: {
                    "FilialeStatisticsList": SettingList
                },
                type: "post",
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("保存失败", null, "error");
                            break;
                        case "1":
                            jqxNotification("保存成功！", null, "success");
                            break;
                    }
                }
            });
        });
    }; //addEvent end
    function parseToInt(str) {
        if (str != null) {
            return parseInt(str);
        } else {
            return 0;
        }
    }
    function GetFilialeStatisticsDetail() {
        debugger;
        var ordersSource =
        {
            dataFields: [
                { name: 'YearMonth', type: 'string' },
                { name: 'BeginningPeriod', type: 'number' },
                { name: 'CurrentPeriodAdded', type: 'number' },
                { name: 'Clear', type: 'number' },
                { name: 'Retirement', type: 'number' },
                { name: 'EndPeriod', type: 'number' },
                { name: 'DateOfYear', type: 'string' },
                { name: 'Zorder', type: 'string' },
                { name: 'VGUID', type: 'string' }
            ],
            url: "/AnalysisManagementCenter/FilialeStatistics/GetFilialeStatisticsDetail",
            data: {
                "DateOfYear": $("#DateOfYear").val()
            },
            dataType: "json",
            id: 'VGUID',
            //localdata: data
        };
        debugger;
        var dataAdapter = new $.jqx.dataAdapter(ordersSource, {
            loadComplete: function () {
                debugger;
            }
        });
        $("#table").jqxDataTable(
        {
            width: "100%",
            height: "500px",
            source: dataAdapter,
            editSettings: { saveOnBlur: true, saveOnSelectionChange: true, cancelOnEsc: true, saveOnEnter: true, editSingleCell: true, editOnDoubleClick: true, editOnF2: true },
            editable: true,
            ready: function () {
            },
            columns: [
                { text: '年份', editable: false, hidden:true, dataField: 'DateOfYear', width: 100, cellsAlign: 'center', align: 'center' },
                { text: '月份', editable: false, dataField: 'YearMonth', width: 100, cellsAlign: 'center', align: 'center' },
                {
                    text: '期初数',
                    editable: false,
                    dataField: 'BeginningPeriod',
                    width: 100,
                    cellsAlign: 'center',
                    align: 'center',
                    columngroup: 'ParentGroup'
                },
                {
                    text: '本期增加',
                    editable: true,
                    dataField: 'CurrentPeriodAdded',
                    width: 100,
                    cellsAlign: 'center',
                    align: 'center',
                    columngroup: 'ParentGroup'
                },
                {
                    text: '清理',
                    editable: true,
                    dataField: 'Clear',
                    width: 100,
                    cellsAlign: 'center',
                    align: 'center',
                    columngroup: 'ChildGroup'
                },
                {
                    text: '退休',
                    editable: true,
                    dataField: 'Retirement',
                    width: 100,
                    cellsAlign: 'center',
                    align: 'center',
                    columngroup: 'ChildGroup'
                },
                {
                    text: '期末数',
                    editable: false,
                    dataField: 'EndPeriod',
                    width: 100,
                    cellsAlign: 'center',
                    align: 'center',
                    columngroup: 'ParentGroup'
                },
                { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ],
            columnGroups: [
                { text: '离岗驾驶员', align: 'center', name: 'ParentGroup' },
                { text: '本期减少', parentGroup: 'ParentGroup', align: 'center', name: 'ChildGroup' }
            ]
        });
        $("#table").on('cellEndEdit', function (event) {
            var args = event.args;
            var rows = $('#table').jqxDataTable('getRows');
            window.setTimeout(function () {
                debugger;
                if (args.boundIndex < rows.length) {
                    var EndPeriod = parseToInt(rows[args.boundIndex].BeginningPeriod) + parseToInt(rows[args.boundIndex].CurrentPeriodAdded) - parseToInt(rows[args.boundIndex].Clear) - parseToInt(rows[args.boundIndex].Retirement);
                    $("#table").jqxDataTable('setCellValue', args.boundIndex, "EndPeriod", EndPeriod);
                    $("#table").jqxDataTable('setCellValue', args.boundIndex + 1, "BeginningPeriod", EndPeriod);
                    }
                }, 200);
        });
        $("#table").on('rowSelect', function (event) {
            var rowBoundIndex = event.args.boundIndex;
            if (rowBoundIndex == 0) {
                if (event.args.row.BeginningPeriod == null || event.args.row.BeginningPeriod == "") {
                    $("#table").jqxDataTable('setColumnProperty', 'BeginningPeriod', 'editable', true);
                } else {
                    $("#table").jqxDataTable('setColumnProperty', 'BeginningPeriod', 'editable', false);
                }
            } else {
                $("#table").jqxDataTable('setColumnProperty', 'BeginningPeriod', 'editable', false);
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
function formatDate(NewDtime) {
    var d = NewDtime;
    var datetime = d.getFullYear() + '-' + (d.getMonth() + 1);//  + '-' + d.getDate() + ' ' + d.getHours() + ':' + d.getMinutes() + ':' + d.getSeconds()
    return datetime;
}