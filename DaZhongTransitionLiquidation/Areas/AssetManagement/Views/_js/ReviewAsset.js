//资产维护
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $btnVerify: function () { return $("#btnVerify") },
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
        var arr = [];
        var d = new Date;
        d.setMonth(d.getMonth() + 1);
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
        $("#SubmitYearMonth").jqxComboBox({ selectedIndex: 0, source: dataAdapter, width: 198, height: 33 });
        $("#SubmitYearMonth").jqxComboBox({ itemHeight: 33 });
        $("#SubmitYearMonth input").click(function () {
            $("#SubmitYearMonth").jqxComboBox('clearSelection');
        })
        $("#dropdownlistWrapperSubmitYearMonth Input")[0].style.paddingLeft = "10px";

        addEvent();
        GetVehicleModelDropDown();
    }
    //var status = $.request.queryString().Status;
    //所有事件
    function addEvent() {
        initiSelectCompany();
        //加载列表数据
        initTable(false);
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable(false);
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#YearMonth").val("");
        });
        //获取数据
        $("#btnGetData").on("click", function () {
            layer.load();
            $.ajax({
                url: "/AssetManagement/ReviewAsset/GetReviewAsset",
                //traditional: true,
                type: "post",
                success: function (msg) {
                    layer.closeAll('loading');
                    switch (msg.Status) {
                    case "0":
                        jqxNotification("获取失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("获取成功！", null, "success");
                        initTable(false);
                        break;
                    case "2":
                        jqxNotification(msg.ResultInfo, null, "error");
                        $("#myModalLabel_title2").html(msg.ResultInfo);
                        ViewReview(msg.ResultInfo2);
                        $("#jqxTable").jqxDataTable('updateBoundData');
                        break;
                    case "3":
                        jqxNotification(msg.ResultInfo, null, "error");
                        $("#myModalLabel_title2").html(msg.ResultInfo);
                        break;
                    }
                }
            });
        });
        //提交
        $("#SubmitAssetReviewDialog_OKBtn").on("click", function () {
            var selection = [];
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
            } else if ($("#SubmitYearMonth").val() == "") {
                jqxNotification("请选择期间！", null, "error");
            } else {
                layer.load();
                $.ajax({
                    url: "/AssetManagement/ReviewAsset/SubmitReviewAsset",
                    data: { vguids: pars, SubmitYearMonth: $("#SubmitYearMonth").val() },
                    //traditional: true,
                    type: "post",
                    success: function(msg) {
                        layer.closeAll('loading');
                        switch (msg.Status) {
                        case "0":
                            jqxNotification("审核失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("审核成功！", null, "success");
                            $("#jqxTable").jqxGrid('updateBoundData');
                            $('#jqxTable').jqxGrid('clearselection');
                            break;
                        case "2":
                            jqxNotification(msg.ResultInfo, null, "success");
                            $("#myModalLabel_title2").html(msg.ResultInfo);
                            ViewReview(msg.ResultInfo2);
                            $("#jqxTable").jqxGrid('updateBoundData');
                            $('#jqxTable').jqxGrid('clearselection');
                            break;
                        }
                    }
                });
            }
        });
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
                $("#SubmitAssetReviewDialog").modal("show");
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
        selector.$btnVerify().on("click",
            function() {
                initTable(true);
            });
    }; //addEvent end

    function initTable(isVerify) {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'GROUP_ID', type: 'string' },
                    { name: 'ASSET_ID', type: 'string' },
                    { name: 'PLATE_NUMBER', type: 'string' },
                    { name: 'TAG_NUMBER', type: 'string' },
                    { name: 'VEHICLE_SHORTNAME', type: 'string' },
                    { name: 'ORGANIZATION_NUM', type: 'string' },
                    { name: 'MANAGEMENT_COMPANY', type: 'string' },
                    { name: 'BELONGTO_COMPANY', type: 'string' },
                    { name: 'ASSET_ID', type: 'string' },
                    { name: 'VEHICLE_STATE', type: 'string' },
                    { name: 'OPERATING_STATE', type: 'string' },
                    { name: 'DESCRIPTION', type: 'string' },
                    { name: 'ENGINE_NUMBER', type: 'string' },
                    { name: 'CHASSIS_NUMBER', type: 'string' },
                    { name: 'PRODUCTION_DATE', type: 'date' },
                    { name: 'PURCHASE_DATE', type: 'date' },
                    { name: 'LISENSING_DATE', type: 'date' },
                    { name: 'COMMISSIONING_DATE', type: 'date' },
                    { name: 'VEHICLE_AGE', type: 'float' },
                    { name: 'BACK_CAR_DATE', type: 'date' },
                    { name: 'FUEL_TYPE', type: 'string' },
                    { name: 'DELIVERY_INFORMATION', type: 'string' },
                    { name: 'QUANTITY', type: 'number' },
                    { name: 'ASSET_COST', type: 'float' },
                    { name: 'NUDE_CAR_FEE', type: 'float' },
                    { name: 'PURCHASE_TAX', type: 'float' },
                    { name: 'LISENSING_FEE', type: 'float' },
                    { name: 'OUT_WAREHOUSE_FEE', type: 'float' },
                    { name: 'DOME_LIGHT_FEE', type: 'float' },
                    { name: 'ANTI_ROBBERY_FEE', type: 'float' },
                    { name: 'LOADING_FEE', type: 'float' },
                    { name: 'INNER_ROOF_FEE', type: 'float' },
                    { name: 'TAXIMETER_FEE', type: 'float' },
                    { name: 'ASSET_DISPOSITION_TYPE', type: 'string' },
                    { name: 'SCRAP_INFORMATION', type: 'string' },
                    { name: 'DISPOSAL_AMOUNT', type: 'float' },
                    { name: 'DISPOSAL_TAX', type: 'float' },
                    { name: 'DISPOSAL_PROFIT_LOSS', type: 'float' },
                    { name: 'BAK_CAR_AGE', type: 'float' },
                    { name: 'ASSET_CATEGORY_MAJOR', type: 'string' },
                    { name: 'ASSET_CATEGORY_MINOR', type: 'string' },
                    { name: 'LIFE_YEARS', type: 'number' },
                    { name: 'LIFE_MONTHS', type: 'number' },
                    { name: 'SALVAGE_TYPE', type: 'string' },
                    { name: 'SALVAGE_PERCENT', type: 'float' },
                    { name: 'SALVAGE_VALUE', type: 'float' },
                    { name: 'AMORTIZATION_FLAG', type: 'string' },
                    { name: 'METHOD', type: 'string' },
                    { name: 'BOOK_TYPE_CODE', type: 'string' },
                    { name: 'ASSET_COST_ACCOUNT', type: 'string' },
                    { name: 'ASSET_SETTLEMENT_ACCOUNT', type: 'string' },
                    { name: 'DEPRECIATION_EXPENSE_SEGMENT', type: 'string' },
                    { name: 'ACCT_DEPRECIATION_ACCOUNT', type: 'string' },
                    { name: 'YTD_DEPRECIATION', type: 'float' },
                    { name: 'ACCT_DEPRECIATION', type: 'float' },
                    { name: 'EXP_ACCOUNT_SEGMENT', type: 'string' },
                    { name: 'MODEL_MAJOR', type: 'string' },
                    { name: 'MODEL_MINOR', type: 'string' },
                    { name: 'START_VEHICLE_DATE', type: 'string' },
                    { name: 'CREATE_DATE', type: 'date' },
                    { name: 'CHANGE_DATE', type: 'date' },
                    { name: 'CREATE_USER', type: 'string' },
                    { name: 'CHANGE_USER', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "YearMonth": $("#YearMonth").val(), Company: $("#Company").val(), VehicleModel: $("#VehicleModel").val(), ISVerify: isVerify },
                url: "/AssetManagement/ReviewAsset/GetReviewAssetListDatas"   //获取数据源的路径
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
                source: typeAdapter,
                rowsheight: 40,
                selectionmode: 'checkbox',
                showstatusbar: true,
                statusbarheight: 22,
                showaggregates: true,
                theme: "office",
                columnsHeight: 40,
                enablehover:false,
                columns: [
                    //{ text: "", datafield: "checkbox", width: 35, pinned: true, hidden:false,align: 'center', cellclassname: cellclass, cellsalign: 'center', cellclassname: cellclass, cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'GroupID', datafield: 'GROUP_ID', width: 100, hidden: true, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    {
                        text: '资产账簿', datafield: 'BOOK_TYPE_CODE', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center',
                        aggregates: ['count',
                            {
                                function (aggregatedValue, currentValue) {
                                    return aggregatedValue + 1;
                                }
                            }
                        ]
                    },
                    { text: '资产ID', datafield: 'ASSET_ID', width: 120, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '车牌号', datafield: 'PLATE_NUMBER', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '标签号', datafield: 'TAG_NUMBER', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '资产说明', datafield: 'DESCRIPTION', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '数量', datafield: 'QUANTITY', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '资产主类', datafield: 'ASSET_CATEGORY_MAJOR', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '资产次类', datafield: 'ASSET_CATEGORY_MINOR', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '车型', datafield: 'VEHICLE_SHORTNAME', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '启用日期', datafield: 'LISENSING_DATE', width: 100, align: 'center', cellclassname: cellclass, cellsalign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '期间', datafield: 'COMMISSIONING_DATE', width: 100, align: 'center', cellclassname: cellclass, cellsalign: 'center', datatype: 'date', cellsformat: "yyyy-MM" },
                    { text: '资产原值', datafield: 'ASSET_COST', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '摊销标记', datafield: 'AMORTIZATION_FLAG', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '存放地点1', datafield: 'BELONGTO_COMPANY', width: 150, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '存放地点2', datafield: 'MANAGEMENT_COMPANY', width: 150, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '存放地点3', datafield: 'ORGANIZATION_NUM', width: 120, align: 'center', cellclassname: cellclass, cellsalign: 'center'},
                    { text: '发动机号', datafield: 'ENGINE_NUMBER', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '车架号', datafield: 'CHASSIS_NUMBER', width: 120, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '经营模式主类', datafield: 'MODEL_MAJOR', width: 180, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '模式子类', datafield: 'MODEL_MINOR', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '创建人', datafield: 'CREATE_USER', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 150, align: 'center', cellclassname: cellclass, cellsalign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'CHANGE_USER', width: 100, align: 'center', cellclassname: cellclass, cellsAlign: 'center' },
                    { text: '修改日期', datafield: 'CHANGE_DATE', width: 100, align: 'center', cellclassname: cellclass, cellsalign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
        //selector.$grid().on('rowDoubleClick', function (event) {
        //    // event args.
        //    var args = event.args;
        //    // row data.
        //    var row = args.row;
        //    // row index.
        //    window.location.href = "/AssetManagement/AssetMaintenanceInfoDetail/Index?VGUID=" + row.VGUID;
        //});
    }
    function cellclass(rowData, columnfield, value) {
        debugger;
        var rowid = selector.$grid().jqxGrid('getrowid', rowData);
        var data = selector.$grid().jqxGrid('getrowdatabyid', rowid);
        if (data.GROUP_ID != "null" && data.GROUP_ID == "1") {
            return 'red';
        }
    };
    function initiSelectCompany() {
        $.ajax({
            url: "/AssetManagement/ReviewAsset/GetCompany",
            type: "POST",
            dataType: "json",
            async: false,
            success: function (msg) {
                uiEngineHelper.bindSelect('#Company', msg, "Descrption", "Descrption");
                $("#Company").prepend("<option value=\"\" selected='true'>请选择</>");
            }
        });
    }
    function cellsRendererValue() {
        var value = "<p  style=\"margin-top:12px;margin-left:28px;\" >000000</p>";
        return value;
    }
    function renderedFunc(element) {
        debugger;
        var grid = selector.$grid();
        element.jqxCheckBox();
        element.on('change', function (event) {
            debugger;
            var checked = element.jqxCheckBox('checked');
            if (checked) {
                var rows = grid.jqxDataTable('getRows');
                for (var i = 0; i < rows.length; i++) {
                    grid.jqxDataTable('selectRow', i);
                    grid.find("#tablejqxTable .jqx_datatable_checkbox").attr("checked", "checked")
                }
            } else {
                grid.jqxDataTable('clearSelection');
                grid.find("#tablejqxTable .jqx_datatable_checkbox").removeAttr("checked", "checked")
            }
        });
        return true;
    }
};
function ViewReview(data) {
    var source =
    {
        datatype: "json",
        datafields:
        [
            { name: 'ENGINE_NUMBER', type: 'string' },
            { name: 'CHASSIS_NUMBER', type: 'string' },
            { name: 'MANAGEMENT_COMPANY', type: 'string' },
            { name: 'BELONGTO_COMPANY', type: 'string' }
        ],
        localdata: data
    };
    var dataAdapter = new $.jqx.dataAdapter(source);
    $("#gridAssetReview").jqxGrid(
        {
            width: "630",
            autoheight: true,
            source: dataAdapter,
            statusbarheight: 25,
            enabletooltips: true,
            theme: "office",
            pageSize: 5,
            pagerButtonsCount: 10,
            pageable: true,
            columnsresize: true,
            selectionmode: 'singlerow',
            columns: [
                { text: '发动机号', datafield: 'ENGINE_NUMBER', columntype: 'textbox', width: 130, align: 'center', cellsAlign: 'center', hidden: false, editable: false },
                { text: '车架号', datafield: 'CHASSIS_NUMBER', columntype: 'textbox', width: 130, align: 'center', cellsAlign: 'center', editable: false },
                { text: '资产管理公司', datafield: 'MANAGEMENT_COMPANY', columntype: 'textbox', width: 130, align: 'center', cellsAlign: 'center', editable: false },
                { text: '资产归属公司', datafield: 'BELONGTO_COMPANY', columntype: 'textbox', width: 240, align: 'center', cellsAlign: 'center', editable: false }
            ]
        });
    $("#AssetReviewDialog").modal("show");
}
function GetVehicleModelDropDown() {
    $.ajax({
        url: "/Systemmanagement/VehicleExtrasFeeSettingDetail/GetVehicleModelDropDown",
        type: "GET",
        dataType: "json",
        async: false,
        success: function (msg) {
            debugger;
            uiEngineHelper.bindSelect('#VehicleModel', msg, "Descrption", "Descrption");
            $("#VehicleModel").prepend("<option value=\"\" selected='true'>请选择</>");
            debugger;
        }
    });
}
$(function () {
    var page = new $page();
    page.init();
});
