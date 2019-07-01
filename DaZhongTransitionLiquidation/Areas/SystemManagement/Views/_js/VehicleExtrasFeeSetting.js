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
        addEvent();
    }
    //所有事件
    function addEvent() {
        //加载列表数据
        initTable();
        //InitVehicleModelSelect();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
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
            var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
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

    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: 'VehicleModel', type: 'string' },
                    { name: 'BusinessProject', type: 'string' },
                    { name: 'Fee', type: 'float' },
                    { name: 'Status', type: 'number' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'ChangeDate', type: 'date' },
                    { name: 'ChangeUser', type: 'string' },
                    { name: 'VGUID', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "VehicleModel": $("#VehicleModel").val() },
                url: "/Systemmanagement/VehicleExtrasFeeSetting/GetVehicleExtrasFeeSettingListDatas"   //获取数据源的路径
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
                columns: [
                    { text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '车型', datafield: 'VehicleModel', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '业务项目', datafield: 'BusinessProject', width: 300, align: 'center', cellsAlign: 'center' },
                    { text: '费用', datafield: 'Fee', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 200, align: 'center', cellsAlign: 'center', cellsRenderer:statusFunc},
                    { text: '创建时间', datafield: 'CreateDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CreateUser', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '修改时间', datafield: 'ChangeDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'ChangeUser', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
        selector.$grid().on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row index.
            window.location.href = "/Systemmanagement/VehicleExtrasFeeSettingDetail/Index?VGUID=" + row.VGUID;
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
                debugger;
                uiEngineHelper.bindSelect('#VehicleModel', msg, "Code", "Descrption");
                $("#VehicleModel").prepend("<option value=\"\" selected='true'>请选择</>");
                debugger;
            }
        });
    }
};
$(function () {
    var page = new $page();
    page.init();
});
