﻿//资产变更审核
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var $page = function () {
    this.init = function () {
        addEvent();
        var arr =[];
        var d = new Date;
        d.setMonth(d.getMonth() +1);
        for (var i = 0; i < 3; i++) {
            debugger;
            var m = d.getMonth() - i;
            var y = d.getFullYear();
            if (m <= 0) {
                m = m + 12;
                y = y - 1;
            }
            m = (m < 10 ? "0" + m : m);
            arr.push(y.toString() + "-" + m.toString());
        }
        debugger;
        if (arr.length == 0) {
            arr.push("Default");
        }
        var dataAdapter = new $.jqx.dataAdapter(arr);
        $("#SubmitYearMonth").jqxComboBox({
            selectedIndex: 0, source: dataAdapter, width : 198, height: 33
        });
        $("#SubmitYearMonth").jqxComboBox({
            itemHeight: 33
        });
        $("#SubmitYearMonth input").click(function () {
            $("#SubmitYearMonth").jqxComboBox('clearSelection');
        })
        $("#dropdownlistWrapperSubmitYearMonth Input")[0].style.paddingLeft = "10px";
    };

    //var status = $.request.queryString().Status;
    //所有事件
    function addEvent() {
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#VEHICLE_SHORTNAME").val("");
            $("#PLATE_NUMBER").val("");
            $("#ENGINE_NUMBER").val("");
            $("#CHASSIS_NUMBER").val("");
        });
        //提交
        $("#btnSubmit").on("click", function () {
            var array = $("#jqxTable").jqxGrid('getselectedrowindexes');
            var pars = [];
            $(array).each(function (i, v) {
                try {
                    debugger;
                    var value = $("#jqxTable").jqxGrid('getcell', v, "VGUID").value;
                    pars.push(value);
                } catch (e) {
                }
            });
            if (array.length < 1) {
                jqxNotification("请选择您要提交的数据！", null, "error");
            } else {
                $("#SubmitAssetReviewDialog").modal("show");
            }
        });
        $("#SubmitAssetReviewDialog_OKBtn").on("click", function () {
            if ($("#SubmitYearMonth").val() == "") {
                jqxNotification("请选择期间！", null, "error");
            } else {
                var array = $("#jqxTable").jqxGrid('getselectedrowindexes');
                var pars = [];
                $(array).each(function (i, v) {
                    try {
                        debugger;
                        var value = $("#jqxTable").jqxGrid('getcell', v, "VGUID").value;
                        pars.push(value);
                    } catch (e) {
                    }
                });
                if (array.length < 1) {
                    jqxNotification("请选择您要提交的数据！", null, "error");
                } else {
                    WindowConfirmDialog(submit, "您确定要提交的" + $("#SubmitYearMonth").val() + "月份的数据？", "确认框", "确定", "取消", pars);
                }
            }
        });
        //关闭
        $("#AssetReviewDialog_CancelBtn").on("click",
            function () {
                $("#AssetReviewDialog").modal("hide");
            }
        );
        $("#SubmitAssetReviewDialog_CancelBtn").on("click",
            function () {
                $("#SubmitAssetReviewDialog").modal("hide");
            }
        );
    }; //addEvent end
    //$("#btnSubmit").on("click", function () {
    //    //var selection = [];
    //    //var grid = $("#jqxTable");
    //    //debugger;
    //    //var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
    //    //checedBoxs.each(function () {
    //    //    var th = $(this);
    //    //    if (th.is(":checked")) {
    //    //        var index = th.attr("index");
    //    //        var data = grid.jqxDataTable('getRows')[index];
    //    //        selection.push(data.VGUID);
    //    //    }
    //    //});
        
    //});
    $("#btnGetModify").on("click", function () {
        layer.load();
        $.ajax({
            url: "/AssetManagement/AssetModifyReview/GetModifyVehicleReview",
            data: { "MODIFY_TYPE": getQueryString("MODIFY_TYPE"), "YearMonth": $("#YearMonth").val() },
            //traditional: true,
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                case "0":
                    jqxNotification("获取失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("获取成功！", null, "success");
                    $("#jqxTable").jqxGrid('updateBoundData');
                    initTable();
                    break;
                }
                layer.closeAll('loading');
            }
        });
    });
    //提交
    function submit(selection) {
        debugger;
        layer.load();
        $.ajax({
            url: "/AssetManagement/AssetModifyReview/SubmitModifyVehicleReview",
            data: { "vguids": selection, "MODIFY_TYPE": getQueryString("MODIFY_TYPE"), "YearMonth": $("#SubmitYearMonth").val() },
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
                        $("#SubmitAssetReviewDialog").modal("hide");
                        $("#jqxTable").jqxGrid('updateBoundData');
                        break;
                }
            }
        });
    }
    function initTable() {
        var mtype = getQueryString("MODIFY_TYPE");
        var columns = [];
        if (mtype == "PLATE_NUMBER") {
            columns = [
                    //{ text: "", datafield: "checkbox", width: 35, pinned: true, hidden: false, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    {
                        text: '变更前车牌号', datafield: 'OLDDATA', width: 150, align: 'center', cellsAlign: 'center',
                        aggregates: ['count',
                            {
                                function (aggregatedValue, currentValue) {
                                    return aggregatedValue + 1;
                                }
                            }
                        ]
                    },
                    { text: '变更后车牌号', datafield: 'PLATE_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '管理公司', datafield: 'MANAGEMENT_COMPANY', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产所属公司', datafield: 'BELONGTO_COMPANY', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产说明', datafield: 'DESCRIPTION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '发动机号', datafield: 'ENGINE_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '车架号', datafield: 'CHASSIS_NUMBER', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ];
        } else if (mtype == "BUSINESS_MODEL") {
            columns = [
                    //{ text: "", datafield: "checkbox", width: 35, pinned: true, hidden: false, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    {
                        text: '变更前经营模式主类', datafield: 'MODEL_MAJOR_M', width: 260, align: 'center', cellsAlign: 'center',
                        aggregates: ['count',
                            {
                                function (aggregatedValue, currentValue) {
                                    return aggregatedValue + 1;
                                }
                            }
                        ]
                    },
                    { text: '变更前经营模式子类', datafield: 'MODEL_MINOR_M', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '变更后经营模式主类', datafield: 'MODEL_MAJOR', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '变更后模式子类', datafield: 'MODEL_MINOR', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '变更前资产类型', width: 170, align: 'center', cellsAlign: 'center', cellsrenderer: cellstyperenderer },
                    { text: '变更前资产主类', datafield: 'ASSET_CATEGORY_MAJOR_M', width: 120, align: 'center', cellsAlign: 'center' },
                    { text: '变更前资产子类', datafield: 'ASSET_CATEGORY_MINOR_M', width: 120, align: 'center', cellsAlign: 'center' },
                    { text: '变更后资产主类', datafield: 'ASSET_CATEGORY_MAJOR', width: 120, align: 'center', cellsAlign: 'center' },
                    { text: '变更后资产子类', datafield: 'ASSET_CATEGORY_MINOR', width: 120, align: 'center', cellsAlign: 'center' },
                    { text: '车牌号', datafield: 'PLATE_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '管理公司', datafield: 'MANAGEMENT_COMPANY', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产所属公司', datafield: 'BELONGTO_COMPANY', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '资产说明', datafield: 'DESCRIPTION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '发动机号', datafield: 'ENGINE_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '车架号', datafield: 'CHASSIS_NUMBER', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ];
        } else if (mtype == "FA_LOC_1") {
            columns = [
                    //{ text: "", datafield: "checkbox", width: 35, pinned: true, hidden: false, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    {
                        text: '变更前所属公司', datafield: 'OLDDATA', width: 150, align: 'center', cellsAlign: 'center',
                        aggregates: ['count',
                            {
                                function (aggregatedValue, currentValue) {
                                    return aggregatedValue + 1;
                                }
                            }
                        ]
                    },
                    //{ text: '管理公司', datafield: 'MANAGEMENT_COMPANY', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '变更后所属公司', datafield: 'BELONGTO_COMPANY', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '车牌号', datafield: 'PLATE_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产说明', datafield: 'DESCRIPTION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '发动机号', datafield: 'ENGINE_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '车架号', datafield: 'CHASSIS_NUMBER', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ];
        } else if (mtype == "FA_LOC_2") {
            columns = [
                    //{ text: "", datafield: "checkbox", width: 35, pinned: true, hidden: false, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    {
                        text: '变更前管理公司', datafield: 'OLDDATA', width: 150, align: 'center', cellsAlign: 'center',
                        aggregates: ['count',
                            {
                                function (aggregatedValue, currentValue) {
                                    return aggregatedValue + 1;
                                }
                            }
                        ]
                    },
                    { text: '变更后管理公司', datafield: 'MANAGEMENT_COMPANY', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '资产所属公司', datafield: 'BELONGTO_COMPANY', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '资产说明', datafield: 'DESCRIPTION', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '车牌号', datafield: 'PLATE_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '发动机号', datafield: 'ENGINE_NUMBER', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '车架号', datafield: 'CHASSIS_NUMBER', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ];
        }
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'ORIGINALID', type: 'string' },
                    { name: 'PLATE_NUMBER', type: 'string' },
                    { name: 'ASSET_CATEGORY_MAJOR_M', type: 'string' },
                    { name: 'ASSET_CATEGORY_MINOR_M', type: 'string' },
                    { name: 'ASSET_CATEGORY_MAJOR', type: 'string' },
                    { name: 'ASSET_CATEGORY_MINOR', type: 'string' },
                    { name: 'PLATE_NUMBER_M', type: 'string' },
                    { name: 'TAG_NUMBER', type: 'string' },
                    { name: 'VEHICLE_SHORTNAME', type: 'string' },
                    { name: 'OLDDATA', type: 'string' },
                    { name: 'ORGANIZATION_NUM', type: 'string' },
                    { name: 'MANAGEMENT_COMPANY', type: 'string' },
                    { name: 'MANAGEMENT_COMPANY_M', type: 'string' },
                    { name: 'BELONGTO_COMPANY', type: 'string' },
                    { name: 'BELONGTO_COMPANY_M', type: 'string' },
                    { name: 'VEHICLE_STATE', type: 'string' },
                    { name: 'OPERATING_STATE', type: 'string' },
                    { name: 'ENGINE_NUMBER', type: 'string' },
                    { name: 'CHASSIS_NUMBER', type: 'string' },
                    { name: 'MODIFY_TYPE', type: 'string' },
                    { name: 'MODEL_MAJOR', type: 'string' },
                    { name: 'MODEL_MINOR', type: 'string' },
                    { name: 'MODEL_MAJOR_M', type: 'string' },
                    { name: 'MODEL_MINOR_M', type: 'string' },
                    { name: 'DESCRIPTION', type: 'string' },
                    { name: 'CREATE_DATE', type: 'date' },
                    { name: 'CREATE_USER', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { PLATE_NUMBER: $("#PLATE_NUMBER").val(), VEHICLE_SHORTNAME: $("#VEHICLE_SHORTNAME").val(), ENGINE_NUMBER: $("#ENGINE_NUMBER").val(), CHASSIS_NUMBER: $("#CHASSIS_NUMBER").val(), "MODIFY_TYPE": getQueryString("MODIFY_TYPE") },
                url: "/AssetManagement/AssetModifyReview/GetReviewAssetListDatas"   //获取数据源的路径
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
                columns: columns
            });
    }
    function cellsrenderer(row, column, value, rowData) {
        debugger;
        switch (rowData.MODIFY_TYPE) {
        case "PLATE_NUMBER":
            return '<span style="margin: 4px; margin-top:8px;">' + rowData.PLATE_NUMBER_M + '</span>';
            break;
        case "FA_LOC_1":
            return '<span style="margin: 4px; margin-top:8px;">' + rowData.BELONGTO_COMPANY_M + '</span>';
            break;
        case "FA_LOC_2":
            return '<span style="margin: 4px; margin-top:8px;">' + rowData.MANAGEMENT_COMPANY_M + '</span>';
            break;
        case "BUSINESS_MODEL":
            return '<span style="margin: 4px; margin-top:8px;">' + rowData.MODEL_MAJOR_M + "-" + rowData.MODEL_MINOR_M + '</span>';
            break;
        default:
            return '<span style="margin: 4px; margin-top:8px;"></span>';
        }
    }
    function cellstyperenderer(row, column, value, rowData) {
        return '<span style="margin: 4px; margin-top:8px;">' + rowData.ASSET_CATEGORY_MAJOR + '-' + rowData.ASSET_CATEGORY_MINOR + '</span>';
    }
    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
    }
    function rendererFunc() {
        var checkBox = "<div id='jqx_datatable_checkbox_all' class='jqx_datatable_checkbox_all' style='z-index: 999; margin-left:7px ;margin-top: 7px;'>";
        checkBox += "</div>";
        return checkBox;
    }
    function renderedFunc(element) {
        var grid = selector.$grid();
        element.jqxCheckBox();
        element.on('change', function (event) {
            var checked = element.jqxCheckBox('checked');
            if (checked) {
                var rows = grid.jqxDataTable('getRows');
                for (var i = 0; i < rows.length; i++) {
                    grid.jqxDataTable('selectRow', i);
                    grid.find(".jqx_datatable_checkbox").attr("checked", "checked");
                }
            } else {
                grid.jqxDataTable('clearSelection');
                grid.find(".jqx_datatable_checkbox").removeAttr("checked", "checked");
            }
        });
        return true;
    }
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