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

    $txtDatedAmount_Dialog: function () { return $("#txtDatedAmount_Dialog") },
    $txtDatedTime_Dialog: function () { return $("#txtDatedTime_Dialog") },

    $txtBankAccountName_Dialog: function () { return $("#txtBankAccountName_Dialog") },
    $txtBankAccount_Dialog: function () { return $("#txtBankAccount_Dialog") },
    $txtBank_Dialog: function () { return $("#txtBank_Dialog") },


    $txtChannel_Dialog: function () { return $("#txtChannel_Dialog") },
    $txtRemark_Dialog: function () { return $("#txtRemark_Dialog") },
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
        getChannelInfos();
        //加载列表数据
        initTable();

        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            selector.$txtDatedTime().val("");
        });

        selector.$txtBankAccountName_Dialog().on("change", function () {
            var backAccount = $(this).val();
            var option = $("#" + backAccount);
            selector.$txtBankAccount_Dialog().val(backAccount);
            selector.$txtBank_Dialog().val(option.attr("bank"));
            selector.$txtChannel_Dialog().val(option.attr("channel"));
        });

        //新增
        selector.$btnAdd().on("click", function () {
            selector.$txtDatedAmount_Dialog().val("");
            selector.$txtDatedTime_Dialog().val("");
            selector.$txtBankAccountName_Dialog().val("");
            selector.$txtBankAccount_Dialog().val("");
            selector.$txtBank_Dialog().val("");
            selector.$txtChannel_Dialog().val("");
            selector.$txtRemark_Dialog().val("");
            isEdit = false;
            vguid = "";
            $("#myModalLabel_title").text("新增银行数据");
            $("#AddNewBankDataDialog table tr").eq(1).show();
            $(".msg").remove();
            selector.$txtDatedAmount_Dialog().removeClass("input_Validate");
            selector.$txtDatedTime_Dialog().removeClass("input_Validate");
            selector.$txtBankAccountName_Dialog().removeClass("input_Validate");
            selector.$txtBankAccount_Dialog().removeClass("input_Validate");
            selector.$txtBank_Dialog().removeClass("input_Validate");
            selector.$txtChannel_Dialog().removeClass("input_Validate");
            selector.$txtRemark_Dialog().removeClass("input_Validate");
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
            if (!Validate(selector.$txtDatedAmount_Dialog())) {
                validateError++;
            }
            if (!Validate(selector.$txtBankAccountName_Dialog())) {
                validateError++;
            }
            if (!isEdit) {
                if (!Validate(selector.$txtDatedTime_Dialog())) {
                    validateError++;
                }
            }
            if (validateError <= 0) {
                $.ajax({
                    url: "/PaymentManagement/BankData/SaveBankData?isEdit=" + isEdit,
                    data: {
                        "ArrivedTime": selector.$txtDatedTime_Dialog().val(),
                        "ArrivedTotal": selector.$txtDatedAmount_Dialog().val(),
                        "ExpendBankAccountName": $("#" + selector.$txtBankAccountName_Dialog().val()).attr("bankaccountname"),
                        "ExpendBankAccount": selector.$txtBankAccount_Dialog().val(),
                        "ExpendBank": selector.$txtBank_Dialog().val(),
                        "Channel_Id": selector.$txtChannel_Dialog().val(),
                        "remark": selector.$txtRemark_Dialog().val(),
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
                                selector.$AddNewBankDataDialog().modal("hide");
                                break;
                            case "2":
                                jqxNotification("当日银行数据已经存在！", null, "error");
                                break;
                            case "3":
                                jqxNotification("当日数据已对账完成，禁止修改！", null, "error");
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
                    { name: 'ArrivedTotal', type: 'number' },
                    { name: 'ArrivedTime', type: 'date' },
                    { name: 'ExpendBankAccountName', type: 'string' },
                    { name: 'ExpendBankAccount', type: 'string' },
                    { name: 'ExpendBank', type: 'string' },
                    { name: 'ReceiveBankAccountName', type: 'string' },
                    { name: 'ReceiveBankAccount', type: 'string' },
                    { name: 'ReceiveBank', type: 'string' },
                    { name: 'Channel_Id', type: 'string' },
                    { name: 'Name', type: 'string' },
                    { name: 'remark', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "Channel_Id": $("#txtChannel").val(), "ArrivedTime": selector.$txtDatedTime().val() },
                url: "/PaymentManagement/BankData/GetBankDatas"   //获取数据源的路径
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
                    { text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '到账金额', datafield: 'ArrivedTotal', cellsFormat: "d2", width: 100, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '到账时间', datafield: 'ArrivedTime', width: 130, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '支付银行户名', datafield: 'ExpendBankAccountName', width: 350, align: 'center', cellsAlign: 'center' },
                    { text: '支付银行账号', datafield: 'ExpendBankAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '支付银行', datafield: 'ExpendBank', width: 350, align: 'center', cellsAlign: 'center' },
                    { text: '收款银行户名', datafield: 'ReceiveBankAccountName', width: 350, align: 'center', cellsAlign: 'center' },
                    { text: '收款银行账号', datafield: 'ReceiveBankAccount', width: 200, align: 'center', cellsAlign: 'center' },
                    { text: '收款银行', datafield: 'ReceiveBank', width: 350, align: 'center', cellsAlign: 'center' },
                    { text: '渠道ID', datafield: 'Channel_Id', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '渠道名称', datafield: 'Name', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '摘要', datafield: 'remark', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });

    }

    function detailFunc(row, column, value, rowData) {
        var container = "";
        if (selector.$EditPermission().val() == "1") {

            container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.ArrivedTotal + "','" + rowData.ExpendBankAccountName + "','" + rowData.ExpendBankAccount + "','" + rowData.ExpendBank + "','" + rowData.Channel_Id + "','" + (rowData.remark == null ? "" : rowData.remark) + "') style=\"text-decoration: underline;color: #333;\">" + rowData.ArrivedTotal + "</a>";
        } else {
            container = "<span>" + rowData.ArrivedTotal + "</span>";
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

    function getChannelInfos() {
        $.ajax({
            url: "/PaymentManagement/NextDayData/GetChannelInfor",
            type: "post",
            dataType: "json",
            success: function (msg) {
                uiEngineHelper.bindSelect('#txtChannel', msg, "Id", "Name");
                $("#txtChannel").prepend("<option value=\"\" selected='true'>请选择</>");
            }

        });
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
            url: "/PaymentManagement/BankData/DeleteBankDatas",
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

                    case "3":
                        jqxNotification("当日数据已对账完成，禁止删除！", null, "error");
                        break;
                }
            }
        });
    }
};

function edit(guid, total, ExpendBankAccountName, ExpendBankAccount, ExpendBank, Channel_Id, remark) {
    selector.$txtDatedAmount_Dialog().val("");
    selector.$txtDatedTime_Dialog().val("");
    selector.$txtBankAccountName_Dialog().val("");
    selector.$txtBankAccount_Dialog().val("");
    selector.$txtBank_Dialog().val("");
    selector.$txtChannel_Dialog().val("");
    selector.$txtRemark_Dialog().val("");
    isEdit = true;
    vguid = guid;
    selector.$txtDatedAmount_Dialog().val(total);
    selector.$txtBankAccountName_Dialog().val(ExpendBankAccount);
    selector.$txtBankAccount_Dialog().val(ExpendBankAccount);
    selector.$txtBank_Dialog().val(ExpendBank);
    selector.$txtChannel_Dialog().val(Channel_Id);
    if (remark != null) {
        selector.$txtRemark_Dialog().val(remark);
    }
    $("#myModalLabel_title").text("编辑银行数据");
    $("#AddNewBankDataDialog table tr").eq(1).hide();
    $(".msg").remove();
    selector.$txtDatedAmount_Dialog().removeClass("input_Validate");
    selector.$txtDatedTime_Dialog().removeClass("input_Validate");
    selector.$txtBankAccountName_Dialog().removeClass("input_Validate");
    selector.$txtBankAccount_Dialog().removeClass("input_Validate");
    selector.$txtBank_Dialog().removeClass("input_Validate");
    selector.$txtChannel_Dialog().removeClass("input_Validate");
    selector.$txtRemark_Dialog().removeClass("input_Validate");
    selector.$AddNewBankDataDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewBankDataDialog().modal("show");
}

$(function () {
    var page = new $page();
    page.init();
});
