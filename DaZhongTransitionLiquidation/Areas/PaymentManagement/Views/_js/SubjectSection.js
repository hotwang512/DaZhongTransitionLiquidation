//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },

    $txtDatedTime: function () { return $("#txtDatedTime") },
    $AddNewBankDataDialog: function () { return $("#AddNewBankDataDialog") },
    $AddNewBankData_OKButton: function () { return $("#AddNewBankData_OKButton") },
    $AddNewBankData_CancelBtn: function () { return $("#AddNewBankData_CancelBtn") },

    $txtCode: function () { return $("#txtCode") },
    $txtDescrption: function () { return $("#txtDescrption") },

    $EditPermission: function () { return $("#EditPermission") }
}; //selector end

var isEdit = false;
var vguid = "";
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {

        //加载列表数据
        initTable();

        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            selector.$txtDatedTime().val("");
        });

        //新增
        selector.$btnAdd().on("click", function () {
            selector.$txtCode().val("");
            selector.$txtDescrption().val("");
            isEdit = false;
            vguid = "";
            $("#myModalLabel_title").text("新增科目段");
            $("#AddNewBankDataDialog table tr").eq(1).show();
            $(".msg").remove();
            selector.$txtCode().removeClass("input_Validate");
            selector.$txtDescrption().removeClass("input_Validate");

            selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
            selector.$AddNewBankDataDialog().modal("show");
        });
        //弹出框中的取消按钮
        selector.$AddNewBankData_CancelBtn().on("click", function () {
            selector.$AddNewBankDataDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddNewBankData_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate(selector.$txtCode())) {
                validateError++;
            }
            if (!Validate(selector.$txtDescrption())) {
                validateError++;
            }
            //if (!isEdit) {
            //    if (!Validate(selector.$txtDatedTime_Dialog())) {
            //        validateError++;
            //    }
            //}
            if (validateError <= 0) {
                $.ajax({
                    url: "/PaymentManagement/SubjectSection/SaveCompanySection?isEdit=" + isEdit,
                    data: {
                        "Code": selector.$txtCode().val(),
                        "Descrption": selector.$txtDescrption().val(),
                        "VGUID": vguid
                    },
                    type: "post",
                    dataType: "json",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                selector.$grid().jqxDataTable('updateBoundData');
                                selector.$AddNewBankDataDialog().modal("hide");
                                break;
                        }

                    }
                });
            }
        });
        //删除
        selector.$btnDelete().on("click", function () {
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
                jqxNotification("请选择您要删除的数据！", null, "error");
            } else {
                WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消");
            }
        });

    }; //addEvent end


    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'Code', type: 'string' },
                    { name: 'Descrption', type: 'string' },
                    { name: 'SectionVGUID', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: {},
                url: "/PaymentManagement/SubjectSection/GetCompanySection"   //获取数据源的路径
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
                height: 600,
                pageSize: 10,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                columns: [
                    { text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '科目编码', datafield: 'Code', width: 800, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '科目名称', datafield: 'Descrption', width: 840, align: 'center', cellsAlign: 'center' },
                    { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }

    function detailFunc(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "1") {

            container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.Code + "','" + rowData.Descrption + "') style=\"text-decoration: underline;color: #333;\">" + rowData.Code + "</a>";
        } else {
            container = "<span>" + rowData.Code + "</span>";
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


    //删除
    function dele() {
        var selection = [];
        var grid = selector.$grid();
        var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
        checedBoxs.each(function () {
            var th = $(this);
            if (th.is(":checked")) {
                var index = th.attr("index");
                var data = grid.jqxDataTable('getRows')[index];
                selection.push(data.VGUID);
                //selection.push(data.ArrivedTime);
            }
        });
        $.ajax({
            url: "/PaymentManagement/SubjectSection/DeleteCompanySection",
            //data: { vguids: selection },
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
                        selector.$grid().jqxDataTable('updateBoundData');
                        break;
                }
            }
        });
    }
};

function edit(guid, Code, Descrption) {
    isEdit = true;
    vguid = guid;
    selector.$txtCode().val(Code);
    selector.$txtDescrption().val(Descrption);
    $("#myModalLabel_title").text("编辑科目段");
    //$("#AddNewBankDataDialog table tr").eq(1).hide();
    $(".msg").remove();
    selector.$txtCode().removeClass("input_Validate");
    selector.$txtDescrption().removeClass("input_Validate");
    selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewBankDataDialog().modal("show");
}

$(function () {
    var page = new $page();
    page.init();
});
