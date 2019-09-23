//OBD资产变更审核
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var $page = function () {
    this.init = function () {
        addEvent();
    };
    //var status = $.request.queryString().Status;
    //所有事件
    function addEvent() {
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
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
        debugger;
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
            jqxNotification("请选择您要提交的数据！", null, "error");
        } else {
            WindowConfirmDialog(submit, "您确定要提交选中的数据？", "确认框", "确定", "取消", selection);
        }
    });
    $("#btnImportModify").on("click", function () {
        $("#LocalOBDFileInput").click();
    });
    $("#LocalOBDFileInput").on("change",
        function () {
            var filePath = this.value;
            var fileExt = filePath.substring(filePath.lastIndexOf("."))
                .toLowerCase();
            if (!checkFileExt(fileExt)) {
                jqxNotification("您上传的文件类型不允许,请重新上传！！", null, "error");
                this.value = "";
                return;
            } else {
                layer.load();
                $("#localOBDFormFile").ajaxSubmit({
                    url: "/AssetManagement/OBDModifyReview/ImportModifyOBDReview",
                    type: "post",
                    success: function (msg) {
                        layer.closeAll('loading');
                        switch (msg.Status) {
                        case "0":
                            if (msg.ResultInfo != null || msg.ResultInfo2 != null) {
                                jqxNotification((msg.ResultInfo == null ? "" : msg.ResultInfo) + " " + (msg.ResultInfo2 == null ? "" : msg.ResultInfo2), null, "error");
                            } else {
                                jqxNotification("导入失败", null, "error");
                            }
                            $('#LocalFileInput').val('');
                            break;
                        case "1":
                            jqxNotification("导入成功！", null, "success");
                            $('#LocalFileInput').val('');
                            initTable();
                            break;
                        }
                    }
                });
            }
        });
    //提交
    function submit(selection) {
        debugger;
        $.ajax({
            url: "/AssetManagement/OBDModifyReview/SubmitModifyOBDReview",
            data: { guids: selection, "MODIFY_TYPE": getQueryString("MODIFY_TYPE") },
            //traditional: true,
            type: "post",
            success: function (msg) {
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
    function initTable() {
        var columns = [
                { text: "", datafield: "checkbox", width: 35, pinned: true, hidden: false, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
            { text: '设备号', datafield: 'EquipmentNumber', width: 100, align: 'center', cellsAlign: 'center' },
                { text: '变更前车牌号', datafield: 'OldData', width: 100, align: 'center', cellsAlign: 'center' },
                { text: '变更后车牌号', datafield: 'PlateNumber', width: 100, align: 'center', cellsAlign: 'center' },
                { text: '创建日期', datafield: 'CreateDate', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                { text: '创建人', datafield: 'CreateUser', width: 100, align: 'center', cellsAlign: 'center' },
                { text: 'VGUID', datafield: 'VGUID', hidden: true }
        ];
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'OldData', type: 'string' },
                    { name: 'PlateNumber', type: 'string' },
                    { name: 'EquipmentNumber', type: 'string' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                url: "/AssetManagement/OBDModifyReview/GetReviewOBDListDatas"   //获取数据源的路径
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
                pageSize: 5,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                columns: columns
            });
    }
    }
    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
    }
    function rendererFunc() {
        var checkBox = "<div id='jqx_datatable_checkbox_all' class='jqx_datatable_checkbox_all' style='z-index: 999; margin-left:7px ;margin-top: 7px;'>";
        checkBox += "</div>";
        return checkBox;
    }
    function checkFileExt(ext) {
        if (!ext.match(/.xls|.xlsx/i)) {
            return false;
        }
        return true;
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