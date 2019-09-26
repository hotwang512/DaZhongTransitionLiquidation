//资产报废审核
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnVerify: function () { return $("#btnVerify") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var $page = function () {
    this.init = function () {
        var date = new Date;
        var year = date.getFullYear();
        var month = date.getMonth() + 1;
        month = (month < 10 ? "0" + month : month);
        var currentDate = (year.toString() + "-" + month.toString());
        $("#YearMonth").val(currentDate);
        addEvent();
    }
    //var status = $.request.queryString().Status;
    //所有事件
    function addEvent() {
        //加载列表数据
        initTable(false);
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable(false);
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#YearMonth").val("");
        });
        //提交
        $("#btnSubmit").on("click", function () {
            if ($("#YearMonth").val() == "") {
                jqxNotification("请选择您要提交的月份！", null, "error");
            } else {
                WindowConfirmDialog(submit, "您确定要提交的" + $("#YearMonth").val() + "月份的数据？", "确认框", "确定", "取消", selection);
            }
        });
        //关闭
        $("#AssetReviewDialog_CancelBtn").on("click",
            function () {
                $("#AssetReviewDialog").modal("hide");
            }
        );
    }; //addEvent end
    $("#btnSubmit").on("click", function () {
        var array = $("#jqxTable").jqxGrid('getselectedrowindexes');
        var pars = [];
        $(array).each(function (i, v) {
            try {
                var value = $("#jqxTable").jqxGrid('getcell', v, "VGUID");
                pars.push(value.value);
            } catch (e) {
            }
        });
        if (array.length < 1) {
            jqxNotification("请选择一条数据！", null, "error");
        } else {
            WindowConfirmDialog(submit, "您确定要提交选中的数据？", "确认框", "确定", "取消", pars);
        }
    });
    //提交
    function submit(selection) {
        layer.load();
        $.ajax({
            url: "/AssetManagement/AssetsRetirementReview/SubmitRetirementVehicleReview",
            data: { guids: selection },
            //traditional: true,
            type: "post",
            success: function (msg) {
                layer.closeAll('loading');
                switch (msg.Status) {
                    case "0":
                        jqxNotification("审核失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("审核成功！", null, "success");
                        $("#jqxTable").jqxDataTable('updateBoundData');
                        break;
                }
            }
        });
    }
    $("#btnGetScrap").on("click", function () {
        $.ajax({
            url: "/AssetManagement/AssetsRetirementReview/GetScrapVehicleReview",
            //traditional: true,
            type: "post",
            data: { YearMonth: $("#YearMonth").val() },
            success: function (msg) {
                switch (msg.Status) {
                case "0":
                    jqxNotification("获取失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("获取成功！", null, "success");
                    $("#jqxTable").jqxDataTable('updateBoundData');
                    break;
                }
            }
        });
    });
    function initTable(isVerify) {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'QUANTITY', type: 'number' },
                    { name: 'ASSET_ID', type: 'string' },
                    { name: 'PLATE_NUMBER', type: 'string' },
                    { name: 'TAG_NUMBER', type: 'string' },
                    { name: 'ORGANIZATION_NUM', type: 'string' },
                    { name: 'MANAGEMENT_COMPANY', type: 'string' },
                    { name: 'BELONGTO_COMPANY', type: 'string' },
                    { name: 'VEHICLE_SHORTNAME', type: 'string' },
                    { name: 'VEHICLE_STATE', type: 'string' },
                    { name: 'OPERATING_STATE', type: 'string' },
                    { name: 'BACK_CAR_DATE', type: 'date' },
                    { name: 'VEHICLE_STATE', type: 'string' },
                    { name: 'OPERATING_STATE', type: 'string' },
                    { name: 'MODEL_MAJOR', type: 'string' },
                    { name: 'MODEL_MINOR', type: 'string' },
                    { name: 'VEHICLE_AGE', type: 'number' },
                    { name: 'LISENSING_DATE', type: 'date' },
                    { name: 'CREATE_DATE', type: 'date' },
                    { name: 'CREATE_USER', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { ISVerify: isVerify  },
                url: "/AssetManagement/AssetsRetirementReview/GetReviewAssetListDatas"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid().jqxGrid(
            {
                pageable: false,
                width: "100%",
                height: 400,
                //pageSize: 5,
                //serverProcessing: true,
                //pagerButtonsCount: 10,
                selectionmode: 'checkbox',
                source: typeAdapter,
                showstatusbar: true,
                statusbarheight: 22,
                showaggregates: true,
                theme: "office",
                columnsHeight: 40,
                columns: [
                    //{ text: "", datafield: "checkbox", width: 35, pinned: true, hidden: false, align: 'center',  cellsAlign: 'center', cellclassname: cellclass,, cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    {
                        text: '资产编号', datafield: 'ASSET_ID', width: 100, align: 'center', cellsAlign: 'center', cellclassname: cellclass,
                        aggregates: ['count',
                            {
                                function (aggregatedValue, currentValue) {
                                    return aggregatedValue + 1;
                                }
                            }
                        ]
                    },
                    { text: '车管车牌号', datafield: 'PLATE_NUMBER', width: 100, align: 'center',  cellsAlign: 'center', cellclassname: cellclass },
                    { text: '资产标签号', datafield: 'TAG_NUMBER', width: 100, align: 'center',  cellsAlign: 'center', cellclassname: cellclass },
                    { text: '车辆简称', datafield: 'VEHICLE_SHORTNAME', width: 100, align: 'center',  cellsAlign: 'center', cellclassname: cellclass },
                    { text: '管理公司', datafield: 'MANAGEMENT_COMPANY', width: 150, align: 'center',  cellsAlign: 'center', cellclassname: cellclass },
                    { text: '资产所属公司', datafield: 'BELONGTO_COMPANY', width: 150, align: 'center',  cellsAlign: 'center', cellclassname: cellclass },
                    { text: '上牌日期', datafield: 'LISENSING_DATE', width: 100, align: 'center',  cellsAlign: 'center', cellclassname: cellclass, datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '退车日期', datafield: 'BACK_CAR_DATE', width: 100, align: 'center',  cellsAlign: 'center', cellclassname: cellclass, datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '退车车龄', datafield: 'VEHICLE_AGE', width: 100, align: 'center',  cellsAlign: 'center', cellclassname: cellclass },
                    { text: '车辆状态', datafield: 'VEHICLE_STATE', width: 100, align: 'center',  cellsAlign: 'center', cellclassname: cellclass },
                    { text: '营运状态', datafield: 'OPERATING_STATE', width: 100, align: 'center',  cellsAlign: 'center', cellclassname: cellclass },
                    { text: '经营模式主类', datafield: 'MODEL_MAJOR', width: 150, align: 'center',  cellsAlign: 'center', cellclassname: cellclass },
                    { text: '经营模式子类', datafield: 'MODEL_MINOR', width: 150, align: 'center',  cellsAlign: 'center', cellclassname: cellclass },
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 150, align: 'center',  cellsAlign: 'center', cellclassname: cellclass, datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
    }
    function cellclass(rowData, columnfield, value) {
        debugger;
        var rowid = selector.$grid().jqxGrid('getrowid', rowData);
        var data = selector.$grid().jqxGrid('getrowdatabyid', rowid);
        if (data.GROUP_ID != "null" && data.GROUP_ID == "1") {
            return 'red';
        }
    };
    selector.$btnVerify().on("click",
        function () {
            initTable(true);
        });
};
$(function () {
    var page = new $page();
    page.init();
});
function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) {
        return unescape(r[2]);
    } else {
        return null;
    }
}