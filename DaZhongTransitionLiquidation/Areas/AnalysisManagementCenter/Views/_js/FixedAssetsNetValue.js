var vguid = "";
var mydate = new Date();
var ManagementCompany = [];
var dataArr = [];
var $page = function () {
    this.init = function () {
        var date = new Date();
        $("#DateOfYear").val(date.getFullYear());
        GetFixedAssetsNetValueDetail(1,12);
        //$("#DateOfYear").attr("disabled",true);
        //if ($("#EditPermission").val() == "True" || $("#NewPermission").val() == "True") {
        //    $("#FilialeStatisticsDialog_OKBtn").show();
        //} else {
        //    $("#FilialeStatisticsDialog_OKBtn").hide();
        //}
        addEvent();
    }
    //所有事件
    function addEvent() {
        $("#btnSearch").on("click",
            function () {
                var minMonth = 1;
                var maxMonth = 12;
                var arr = $("input[name='Month']:checked").map(function () {
                    return parseInt($(this).val());
                }).get().sort();
                if (arr.length > 0) {
                    minMonth = Math.min.apply(null, arr);
                    maxMonth = Math.max.apply(null, arr);
                }
                GetFixedAssetsNetValueDetail(minMonth,maxMonth);
            });
    }; //addEvent end
};

function parseToInt(str) {
    if (str != null) {
        return parseInt(str);
    } else {
        return 0;
    }
}
function GetFixedAssetsNetValueDetail(minMonth,maxMonth) {
    var ordersSource =
    {
        dataFields: [
            { name: 'MainCategory', type: 'string' },
            { name: 'StartPeriod', type: 'number' },
            { name: 'AddedPeriod', type: 'number' },
            { name: 'ReducePeriod', type: 'number' },
            { name: 'EndPeriod', type: 'number' },
            { name: 'Zorder', type: 'number' }
        ],
        url: "/AnalysisManagementCenter/FixedAssetsNetValue/GetFixedAssetsNetValueDetail",
        data: {
            "DateOfYear": $("#DateOfYear").val(), "minMonth": minMonth, "maxMonth": maxMonth
        },
        dataType: "json"
    };
    var dataAdapter = new $.jqx.dataAdapter(ordersSource, {
        loadComplete: function () {
            $("#row0table td:first").css("text-align", "left");
            $("#row4table td:first").css("text-align", "left");
            $("#row8table td:first").css("text-align", "left");
            $("#row12table td:first").css("text-align", "left");
        }
    });
    $("#table").jqxDataTable(
    {
        width: "100%",
        height: "500px",
        source: dataAdapter,
        enableHover: false,
        ready: function () {
        },
        columns: [
            { text: '主分类段', editable: false, dataField: 'MainCategory', width: 120, cellsAlign: 'center', align: 'center', cellclassname: cellclass },
            {
                text: '年初数',
                editable: false,
                dataField: 'StartPeriod',
                width: 100,
                cellsAlign: 'center',
                cellclassname: cellclass,
                align: 'center'
            },
            {
                text: '本期增加额',
                editable: true,
                dataField: 'AddedPeriod',
                width: 100,
                cellsAlign: 'center', cellclassname: cellclass,
                align: 'center'
            },
            {
                text: '本期减少额',
                editable: true,
                dataField: 'ReducePeriod',
                width: 100,
                cellsAlign: 'center', cellclassname: cellclass,
                align: 'center'
            },
            {
                text: '期末数',
                editable: false,
                dataField: 'EndPeriod',
                width: 100,
                cellsAlign: 'center', cellclassname: cellclass,
                align: 'center'
            },
            { text: 'VGUID', datafield: 'VGUID', hidden: true }
        ]
    });
}
function cellclass(rowData, columnfield, value) {
    if (rowData >= 0 && rowData < 4) {
        return 'yellowlight';
    } else if (rowData >= 4 && rowData < 8) {
        return 'blue';
    } else if (rowData >= 8 && rowData < 12) {
        return 'gray';
    } else if (rowData >= 12 && rowData < 16) {
        return 'yellow';
    }
};
function pickedFunc() {
    GetFilialeStatisticsDetail();
}

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