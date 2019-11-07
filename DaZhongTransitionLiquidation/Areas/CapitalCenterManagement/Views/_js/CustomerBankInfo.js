//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },

    $txtBankName: function () { return $("#txtBankName") },
    $txtChannelName: function () { return $("#txtChannelName") },

    $AddBankChannelDialog: function () { return $("#AddBankChannelDialog") },
    $AddBankChannel_OKButton: function () { return $("#AddBankChannel_OKButton") },
    $AddBankChannel_CancelBtn: function () { return $("#AddBankChannel_CancelBtn") },
    $txtBankAccount_Dialog: function () { return $("#txtBankAccount_Dialog") },
    $txtBankAccountName_Dialog: function () { return $("#txtBankAccountName_Dialog") },
    $txtBankNo: function () { return $("#txtBankNo") },
    $txtBank_Dialog: function () { return $("#txtBank_Dialog") },
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
            selector.$txtChannelName().val("");
            selector.$txtBankName().val("");
        });
        selector.$btnAdd().on("click", function () {
            add();
        });

        //弹出框中的取消按钮
        selector.$AddBankChannel_CancelBtn().on("click", function () {
            selector.$AddBankChannelDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddBankChannel_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate(selector.$txtBankAccount_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtBankAccountName_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtBank_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtBankNo())) {
                validateError++;
            }
            if (validateError <= 0) {
                $.ajax({
                    url: "/CapitalCenterManagement/CustomerBankInfo/SaveCustomerBankInfo?isEdit=" + isEdit,
                    data: {
                        BankAccount: selector.$txtBankAccount_Dialog().val(),
                        BankAccountName: selector.$txtBankAccountName_Dialog().val(),
                        Bank: selector.$txtBank_Dialog().val(),
                        BankNo: selector.$txtBankNo().val(),
                        CompanyOrPerson: $("#CompanyOrPerson").val(),
                        VGUID: vguid
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
                                selector.$AddBankChannelDialog().modal("hide");
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
                WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消", selection);
            }
        });

    }; //addEvent end


    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'CompanyOrPerson', type: 'string' },
                    { name: 'BankAccountName', type: 'string' },
                    { name: 'Bank', type: 'string' },
                    { name: 'BankAccount', type: 'string' },
                    { name: 'BankNo', type: 'string' },
                    { name: 'VGUID', type: 'string' }
                ],
                datatype: "json",
                id: "Vguid",
                data: { "BankAccount": selector.$txtBankName().val() },
                url: "/CapitalCenterManagement/CustomerBankInfo/GetCustomerBankInfo"   //获取数据源的路径
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
                columns: [
                    { width: 35, text: "", datafield: "checkbox", align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '供应商类别', datafield: 'CompanyOrPerson', width: '250px', align: 'center', cellsAlign: 'center', cellsRenderer: channelDetailFunc },
                    { text: '账号', datafield: 'BankAccount', align: 'center', width: '250px', cellsAlign: 'center', },
                    { text: '户名', datafield: 'BankAccountName', align: 'center', width: '350px', cellsAlign: 'center' },
                    { text: '开户行', datafield: 'Bank', align: 'center', width: '350px', cellsAlign: 'center' },
                    { text: '行号', datafield: 'BankNo', align: 'center', cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });

    }

    function channelDetailFunc(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "true") {
            container = "<a href='#' onclick=edit('" + rowData.VGUID + "','"
                + rowData.BankAccount + "','"
                + rowData.BankAccountName + "','"
                + rowData.Bank + "','"
                + rowData.BankNo + "','"
                + rowData.CompanyOrPerson + "') style=\"text-decoration: underline;color: #333;\">" + rowData.CompanyOrPerson + "</a>";
        } else {
            container = "<span>" + rowData.CompanyOrPerson + "</span>";
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
    function dele(selection) {

        $.ajax({
            url: "/CapitalCenterManagement/CustomerBankInfo/DeleteCustomerBankInfo",
            data: { vguids: selection },
            //traditional: true,
            type: "post",
            success: function (msg) {
                if (msg.IsSuccess) {
                    jqxNotification("删除成功！", null, "success");
                    selector.$grid().jqxDataTable('updateBoundData');
                } else {
                    jqxNotification("删除失败！", null, "error");
                }
            }
        });
    }
};

function add() {
    selector.$txtBankAccount_Dialog().val("");
    selector.$txtBankAccountName_Dialog().val("");
    selector.$txtBank_Dialog().val("");
    selector.$txtBankNo().val("");
    isEdit = false;
    vguid = "";
    $("#myModalLabel_title").text("新增客户银行信息");
    selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddBankChannelDialog().modal("show");
}

function edit(guid, BankAccount, BankAccountName, Bank, BankNo, CompanyOrPerson) {
    selector.$txtBankAccount_Dialog().val("");
    selector.$txtBankAccountName_Dialog().val("");
    selector.$txtBank_Dialog().val("");
    selector.$txtBankNo().val("");
    isEdit = true;
    vguid = guid;
    $("#myModalLabel_title").text("编辑客户银行信息");
    selector.$txtBankAccount_Dialog().val(BankAccount);
    selector.$txtBankAccountName_Dialog().val(BankAccountName);
    selector.$txtBank_Dialog().val(Bank);
    selector.$txtBankNo().val(BankNo);
    $("#CompanyOrPerson").val(CompanyOrPerson)



    $(".msg").remove();
    selector.$txtBankAccount_Dialog().removeClass("input_Validate");
    selector.$txtBankAccountName_Dialog().removeClass("input_Validate");
    selector.$txtBank_Dialog().removeClass("input_Validate");
    selector.$txtBankNo().removeClass("input_Validate");

    selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddBankChannelDialog().modal("show");
}



$(function () {
    var page = new $page();
    page.init();
});