var selector = {
    //表格
    $grid: function () { return $("#datatable") },
    //查询条件
    $txtName: function () { return $("#txtName") },
    $txtJobNumber: function () { return $("#txtJobNumber") },
    $txtUserIDNo: function () { return $("#txtUserIDNo") },
    $selPaymentStatus: function () { return $("#selPaymentStatus") },
    $txtTransactionId: function () { return $("#txtTransactionId") },
    $txtPaymentForm: function () { return $("#txtPaymentForm") },
    $txtPaymentTo: function () { return $("#txtPaymentTo") },

    //按钮
    $btnReconciliation: function () { return $("#btnReconciliation") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $txtTransactionId_T: function () { return $("#txtTransactionId_T") },
    //弹出框
    $exceptionDataDialog: function () { return $("#exceptionDataDialog") },
    $exceptionDataTable: function () { return $("#exceptionDataTable") },
    $btnDownLoad: function () { return $("#btnDownLoad") },
    $btnCancel: function () { return $("#btnCancel") }

};

var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        initTable();
        getChannelInfos();
        //查询
        selector.$btnSearch().on("click", function () {
            initTable();
        });

        //重置
        selector.$btnReset().on("click", function () {
            $("#Name").val("");
            $("#Code").val("");
            $("#txtChannel").val("");
            $("#txtChannel2").val("");
            selector.$txtUserIDNo().val("");
            selector.$selPaymentStatus().val("1");
            selector.$txtTransactionId().val("");
            selector.$txtPaymentForm().val("");
            selector.$txtPaymentTo().val("");
        });
        //支付状态下拉框改变
        selector.$selPaymentStatus().on("change", function () {
            initTable();
        });
        //点击金额对账按钮
        selector.$btnReconciliation().on("click", function () {
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
            //if (selection.length <= 0) {
            //    jqxNotification("请选择需要对账的数据！", null, "error");
            //    return false;
            //}
            if (selector.$selPaymentStatus().val() !== "1") {
                jqxNotification("请选择支付成功的数据！", null, "error");
                return false;
            }
            showLoading();
            $.ajax({
                url: "/PaymentManagement/RevenuePayment/Reconciliation",
                data: {
                    "Name": selector.$txtName().val(),
                    "JobNumber": selector.$txtJobNumber().val(),
                    "IDNumber": selector.$txtUserIDNo().val(),
                    "PaymentStatus": selector.$selPaymentStatus().val(),
                    "TransactionID": selector.$txtTransactionId().val(),
                    "PayDateFrom": selector.$txtPaymentForm().val(),
                    "PayDateTo": selector.$txtPaymentTo().val(),
                    transactionIds: selection
                },
                traditional: true,
                type: "post",
                dataType: "json",
                success: function (msg) {
                    closeLoading();
                    if (msg.length > 0) {
                        initExceptionTable(msg);
                        selector.$exceptionDataDialog().modal({ backdrop: 'static', keyboard: false });
                        selector.$exceptionDataDialog().modal('show');
                    } else {
                        jqxNotification("金额无异常!", null, "success");
                    }
                }
            });
        });
        //取消
        selector.$btnCancel().on("click", function () {
            selector.$exceptionDataDialog().modal('hide');
        });
        //下载Excel
        selector.$btnDownLoad().on("click", function () {

        });
        //初始化表格
        function initTable() {
            var transactionId = selector.$txtTransactionId().val();
            if (selector.$txtTransactionId_T().val()) {
                transactionId = selector.$txtTransactionId_T().val();
            }
            var source =
                {
                    datafields:
                    [
                        { name: "checkbox", type: null },
                        { name: 'Name', type: 'string' },
                        { name: 'JobNumber', type: 'string' },
                        { name: 'PhoneNumber', type: 'string' },
                        { name: 'Department', type: 'string' },
                        { name: 'PaymentAmount', type: 'number' },
                        { name: 'DriverBearFees', type: 'number' },
                        { name: 'copeFee', type: 'number' },
                        { name: 'ActualAmount', type: 'number' },
                        { name: 'copeFee', type: 'number' },
                        { name: 'CompanyBearsFees', type: 'number' },
                        { name: 'ChannelPayableAmount', type: 'number' },
                        { name: 'PaymentBrokers', type: 'string' },
                        { name: 'TransactionID', type: 'string' },
                        { name: 'PayDate', type: 'date' },
                        { name: 'PaymentStatus', type: 'string' },
                        { name: 'RevenueType', type: 'string' },
                        { name: 'RevenueStatus', type: 'string' },
                        { name: 'Channel_Id', type: 'string' },
                        { name: 'Channel_IdDESC', type: 'string' },
                        { name: 'SubjectId', type: 'string' },
                        { name: 'Subject_IdDESC', type: 'string' },
                        { name: 'CreateUser', type: 'string' },
                        { name: 'CreateDate', type: 'date' },
                        { name: 'VGUID', type: 'string' }
                    ],
                    datatype: "json",
                    id: "VGUID",//主键
                    data: {
                        "Channel_Id": $("#txtChannel").val(),
                        "SubjectId": $("#txtChannel2").val(),
                        "IDNumber": selector.$txtUserIDNo().val(),
                        "TransactionID": transactionId,
                        "PayDateFrom": selector.$txtPaymentForm().val(),
                        "PayDateTo": selector.$txtPaymentTo().val(),
                        "Name": $("#Name").val(),
                        "JobNumber": $("#Code").val()
                    },
                    url: "/PaymentManagement/RevenuePayment/GetRevenuePaymentInfos"    //获取数据源的路径
                };
            var typeAdapter = new $.jqx.dataAdapter(source, {
                downloadComplete: function (data) {
                    source.totalrecords = data.TotalRows;
                }
            });
            selector.$grid().jqxDataTable(
                {
                    pageable: true,
                    width: "100%",
                    height: 480,
                    pageSize: 10,
                    serverProcessing: true,
                    pagerButtonsCount: 10,
                    source: typeAdapter,
                    theme: "office",
                    columns: [
                        //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                        { text: '人员姓名', datafield: 'Name', width: 110, align: 'center', cellsAlign: 'center' },
                        { text: '工号', datafield: 'JobNumber', width: 110, align: 'center', cellsAlign: 'center' },
                        { text: '流水号', datafield: 'TransactionID', minwidth: 200, align: 'center', cellsAlign: 'center' },
                        { text: '驾驶员欠款金额', cellsFormat: "d2", width: 150, datafield: 'PaymentAmount', align: 'center', cellsAlign: 'center' },
                        { text: '驾驶员承担手续费', cellsFormat: "d2", width: 150, datafield: 'DriverBearFees', align: 'center', cellsAlign: 'center' },
                        { text: '驾驶员实付金额', cellsFormat: "d2", width: 150, datafield: 'ActualAmount', align: 'center', cellsAlign: 'center' },
                        { text: '渠道实收手续费', cellsFormat: "d2", width: 150, datafield: 'copeFee', align: 'center', cellsAlign: 'center' },
                        { text: '公司承担手续费', cellsFormat: "d2", width: 150, datafield: 'CompanyBearsFees', align: 'center', cellsAlign: 'center' },
                        { text: '渠道应付金额', cellsFormat: "d2", width: 150, datafield: 'ChannelPayableAmount', align: 'center', cellsAlign: 'center' },
                        { text: '付款时间', datafield: 'PayDate', width: 130, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                        { text: '渠道ID', datafield: 'Channel_Id', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '渠道名称', datafield: 'Channel_IdDESC', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '二级渠道ID', datafield: 'SubjectId', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '二级渠道名称', datafield: 'Subject_IdDESC', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '创建人', datafield: 'CreateUser', width: 150, align: 'center', cellsAlign: 'center' },
                        { text: '创建时间', datafield: 'CreateDate', width: 130, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                        { text: 'VGUID', datafield: 'VGUID', hidden: true }
                    ]
                });
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
        //翻译营收类型字段
        function showRevenue(row, column, value, rowData) {
            var container = "";
            switch (rowData.RevenueType) {
                case 1:
                    container = "<span>固定金额</span>";
                    break;
                case 2:
                    container = "<span>营收金额</span>";
                    break;
            }
            return container;
        }

        //翻译营收类型字段
        function showPaymentStatus(row, column, value, rowData) {
            var container = "";
            switch (rowData.PaymentStatus) {
                case "1":
                    container = "<span>支付成功</span>";
                    break;
                case "2":
                    container = "<span>支付失败</span>";
                    break;
                case "3":
                    container = "<span>待支付</span>";
                    break;
                default:
                    container = "<span>已退款</span>";
                    break;
            }
            return container;
        }

        //加载金额异常数据
        function initExceptionTable(data) {
            var source =
                {
                    localData: data,
                    dataType: "array",
                    dataFields:
                    [
                        { name: 'Name', type: 'string' },
                        { name: 'ActualAmount', type: 'number' },
                        { name: 'Remitamount', type: 'number' },
                        { name: 'PayDate', type: 'date' },
                        //{ name: 'lastname', type: 'string' },
                        { name: 'Reason', type: 'string' },
                    ]
                };
            var dataAdapter = new $.jqx.dataAdapter(source);
            selector.$exceptionDataTable().jqxDataTable(
                {
                    width: 810,
                    height: 430,
                    pageable: true,
                    pagerButtonsCount: 10,
                    source: dataAdapter,
                    theme: "office",
                    columns: [
                        { text: '姓名', align: 'center', cellsAlign: 'center', dataField: 'Name' },
                        { text: '实际付款', align: 'center', cellsAlign: 'center', dataField: 'ActualAmount', cellsFormat: "d2" },
                        { text: '到账金额', dataField: 'Remitamount', align: 'center', cellsAlign: 'center', cellsFormat: "d2" },
                        { text: '付款时间', dataField: 'PayDate', align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                        { text: '异常原因', dataField: 'Reason', align: 'center', cellsAlign: 'center', cellsFormat: 'c2' }
                    ]
                });
        }

    }; //addEvent end

};

$(function () {
    var page = new $page();
    page.init();

});

function getChannelInfos() {
    $.ajax({
        url: "/PaymentManagement/NextDayData/GetChannelInfor",
        type: "post",
        dataType: "json",
        success: function (msg) {
            var option = "<option value=''></option>";
            for (var i = 0; i < msg.length; i++) {
                option += "<option value=" + msg[i].Id + ">" + msg[i].Name + "</option>";
            }
            $("#txtChannel").append(option);
        }

    });
}

function changeChannel() {
    var channel = $("#txtChannel").val();
    $.ajax({
        url: "/PaymentManagement/NextDayData/GetSubject",
        type: "post",
        dataType: "json",
        data: { "Channel": channel },
        success: function (msg) {
            uiEngineHelper.bindSelect('#txtChannel2', msg, "SubjectId", "SubjectNmae");
            $("#txtChannel2").prepend("<option value=\"\" selected='true'></>");
        }

    });
}