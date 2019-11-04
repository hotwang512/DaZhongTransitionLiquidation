//资产基础信息维护列表
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var $page = function () {

    this.init = function () {
        GetVehicleModelDropDown();
        GetVehicleExtrasFeeSettingColumns();
        //加载列表数据
        addEvent();
    }
    //所有事件
    function addEvent() {
        //InitVehicleModelSelect();
        selector.$btnSearch().unbind("click").on("click", function () {
            GetVehicleExtrasFeeSettingColumns();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#VehicleModel").val("");
        });
        //新增
        $("#btnAdd").on("click", function () {
            window.location.href = "/Systemmanagement/VehicleExtrasFeeSettingDetail/Index";
            //window.open("/CapitalCenterManagement/OrderListDetail/Index");
        });
        //删除
        $("#btnDelete").on("click", function () {
            var selection = [];
            var grid = $("#jqxTable");
            var checedBoxs = grid.find("#tablejqxTable .jqx_datatable_checkbox:checked");//grid.find(".jqx_datatable_checkbox:checked");
            checedBoxs.each(function () {
                var th = $(this);
                if (th.is(":checked")) {
                    var index = th.attr("index");
                    var data = grid.jqxDataTable('getRows')[index];
                    selection.push(data.VGUID);
                }
            });
            if (selection.length < 1) {
                jqxNotification("请选择您要删除的数据！", null, "error");
            } else {
                WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
    }; //addEvent end

    //删除
    function dele(selection) {
        $.ajax({
            url: "/Systemmanagement/VehicleExtrasFeeSetting/DeleteVehicleExtrasFeeSetting",
            data: { vguids: selection },
            traditional: true,
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                    case "0":
                        jqxNotification("删除失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("删除成功！", null, "success");
                        $("#jqxTable").jqxDataTable('updateBoundData');
                        break;
                }
            }
        });
    }

    function initTable(data, para) {
        var datafields = [{ name: 'BusinessProject', type: 'string' }];
        var columns = [
            { text: '业务项目', datafield: 'BusinessProject', width: 100, align: 'center', cellsAlign: 'center' }
        ];
        for (var i = 0; i < data.length; i++) {
            datafields.push({ name: data[i], type: 'float' });
            columns.push({ text: data[i], datafield: data[i], width: 100, align: 'center', cellsAlign: 'center' });
        }
        $.ajax({
            url: "/Systemmanagement/VehicleExtrasFeeSetting/GetVehicleExtrasFeeSettingListDatas",
            type: "GET",
            dataType: "json",
            async: false,
            success: function (msg) {
                var source =
                {
                    datafields: datafields,
                    datatype: "json",
                    data: { "VehicleModel": para },
                    localdata: msg   //获取数据源的路径
                };
                var typeAdapter = new $.jqx.dataAdapter(source, {
                    downloadComplete: function (data) {
                        source.totalrecords = data.TotalRows;
                    }
                });
                //创建卡信息列表（主表）
                selector.$grid().jqxDataTable(
                    {
                        pageable: true,
                        width: "100%",
                        height: 400,
                        pageSize: 10,
                        serverProcessing: true,
                        pagerButtonsCount: 10,
                        source: typeAdapter,
                        theme: "office",
                        columnsHeight: 40,
                        columnsResize: true,
                        columns: columns
                    });
                //selector.$grid().on('rowDoubleClick', function (event) {
                //    // event args.
                //    var args = event.args;
                //    // row data.
                //    var row = args.row;
                //    // row index.
                //    window.location.href = "/Systemmanagement/VehicleExtrasFeeSettingDetail/Index?VGUID=" + row.VGUID + "&Code=" + row.VehicleModelCode;
                //});
            }
        });
    }
    function statusFunc(row, column, value, rowData) {
        var container = '<div style="color:#30dc32">启用</div>';
        if (value == "0") {
            container = '<div style="color:#F00">禁用</div>';
        }
        return container;
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
                    grid.find(".jqx_datatable_checkbox").attr("checked", "checked")
                }
            } else {
                grid.jqxDataTable('clearSelection');
                grid.find(".jqx_datatable_checkbox").removeAttr("checked", "checked")
            }
        });
        return true;
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
            }
        });
    }
    function GetVehicleExtrasFeeSettingColumns() {
        $.ajax({
            url: "/Systemmanagement/VehicleExtrasFeeSetting/GetVehicleExtrasFeeSettingColumns",
            type: "GET",
            dataType: "json",
            async: false,
            success: function (msg) {
                var para = "";
                for (var i = 0; i < msg.ResultInfo.length; i++) {
                    para += "[" + msg.ResultInfo[i] + "],";
                }
                para = para.substring(0, para.length - 1);
                initTable(msg.ResultInfo,para);
            }
        });
    }
};
$(function () {
    var page = new $page();
    page.init();
});
