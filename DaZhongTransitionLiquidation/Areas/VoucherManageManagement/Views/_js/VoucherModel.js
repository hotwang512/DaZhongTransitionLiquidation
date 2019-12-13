var $page = function () {

    this.init = function () {
        addEvent();
    };
    var selector = this.selector =
        {
            $grid: function () { return $("#jqxTable") },
            $btnSearch: function () { return $("#btnSearch") },
        }

    function addEvent() {
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });
        $("#btnAdd").on("click", function () {
            window.location.href = "/VoucherManageManagement/VoucherModelDetail/Index";
        })
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
        //启用
        $('#btnEditDo').on("click", function () {
            var status = '1';
            var selection = [];
            var grid = selector.$grid();
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
                jqxNotification("请选择您要操作的数据！", null, "error");
            } else {
                WindowConfirmDialog(edits, "您确定要启用选中的数据？", "确认框", "确定", "取消", selection, status);
            }
        })
        //禁用
        $('#btnEditDis').on("click", function () {
            var status = '0';
            var selection = [];
            var grid = selector.$grid();
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
                jqxNotification("请选择您要操作的数据！", null, "error");
            } else {
                WindowConfirmDialog(edits, "您确定要禁用选中的数据？", "确认框", "确定", "取消", selection, status);
            }
        })
    }

    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'ModelName', type: 'string' },
                    { name: 'AccountModeCode', type: 'string' },
                    { name: 'CompanyCode', type: 'string' },
                    { name: 'Remark', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'Creater', type: 'string' },
                    { name: 'CreateTime', type: 'date' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "ModelName": $("#ModelName").val(), "AccountingPeriod": $("#AccountingPeriod").val() },
                url: "/VoucherManageManagement/VoucherModel/GetVoucherModelData"   //获取数据源的路径
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
                columnsHeight: 30,
                columns: [
                    { text: "", datafield: "checkbox", pinned: true, width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '模板名称', datafield: 'ModelName', align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '备注', datafield: 'Remark', align: 'center', cellsAlign: 'center', },
                    { text: '状态', datafield: 'Status', align: 'center', cellsAlign: 'center', cellsRenderer: statusFunc },
                    { text: '创建人', datafield: 'Creater', align: 'center', cellsAlign: 'center', },
                    { text: '创建时间', datafield: 'CreateTime', align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:dd", hidden: true },
                    { text: 'AccountModeCode', datafield: 'AccountModeCode', hidden: true },
                    { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });
    }
    function detailFunc(row, column, value, rowData) {
        var container = "<a href='#' onclick=link('" + rowData.VGUID + "') style=\"text-decoration: underline;color: #333;\">" + rowData.ModelName + "</a>";
        return container;
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
};

$(function () {
    var page = new $page();
    page.init();
});

function link(VGUID) {
    window.location.href = "/VoucherManageManagement/VoucherModelDetail/Index?VGUID=" + VGUID;
}

//删除
function dele(selection) {
    $.ajax({
        url: "/VoucherManageManagement/VoucherModel/DeleteVoucherModel",
        data: { vguids: selection },
        traditional: true,
        type: "post",
        success: function (msg) {
            if (msg.IsSuccess) {
                jqxNotification("删除成功！", null, "success");
                $("#jqxTable").jqxDataTable('updateBoundData');
            } else {
                jqxNotification("删除失败！", null, "error");
            }
        }
    });
}
//编辑状态
function edits(selection, status) {
    layer.load();
    $.ajax({
        url: "/VoucherManageManagement/VoucherModel/EditStatus",
        //data: { vguids: selection },
        data: { vguids: selection, status: status },
        traditional: true,
        type: "post",
        success: function (msg) {
            layer.closeAll('loading');
            switch (msg.Status) {
                case "0":
                    jqxNotification("操作失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("操作成功！", null, "success");
                    $("#jqxTable").jqxDataTable('updateBoundData');
                    break;
            }
        }
    });
}