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
    debugger;
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
        ready: function () {
        },
        columns: [
            { text: '主分类段', editable: false, dataField: 'MainCategory', width: 150, cellsAlign: 'center', align: 'center' },
            {
                text: '年初数',
                editable: false,
                dataField: 'StartPeriod',
                width: 100,
                cellsAlign: 'center',
                align: 'center'
            },
            {
                text: '本期增加额',
                editable: true,
                dataField: 'AddedPeriod',
                width: 100,
                cellsAlign: 'center',
                align: 'center'
            },
            {
                text: '本期减少额',
                editable: true,
                dataField: 'ReducePeriod',
                width: 100,
                cellsAlign: 'center',
                align: 'center'
            },
            {
                text: '期末数',
                editable: false,
                dataField: 'EndPeriod',
                width: 100,
                cellsAlign: 'center',
                align: 'center'
            },
            { text: 'VGUID', datafield: 'VGUID', hidden: true }
        ]
    });
}
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