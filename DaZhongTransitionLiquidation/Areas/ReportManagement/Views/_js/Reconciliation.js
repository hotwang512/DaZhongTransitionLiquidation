var selector = {
    //表格
    $grid: function () { return $("#datatable") },
    $gridDetail: function () { return $("#jqxTable") },
    //查询条件
    $txtName: function () { return $("#txtName") },
    $txtPaymentForm: function () { return $("#txtPaymentForm") },
    $txtPaymentTo: function () { return $("#txtPaymentTo") },
    //$drdLevel: function () { return $("#drdLevel") },
    //$pushPeopleDropDownButton: function () { return $("#pushPeopleDropDownButton") },
    //$pushTree: function () { return $("#pushTree") },
    $txtChannel: function () { return $("#txtChannel") },
    //$drdReasonStatus: function () { return $("#drdReasonStatus") },
    $txtStatus: function () { return $("#txtStatus") },
    //按钮
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $btnAutomaticReconciliation: function () { return $("#btnAutomaticReconciliation") },
    $btnExport: function () { return $("#btnExport") },
    $departmentVguid: function () { return $("#DepartmentVguid") },
    $currentUserDepartment: function () { return $("#currentUserDepartment") },

    $detailDialog: function () { return $("#detailDialog") },
    //$detail_OKButton: function () { return $("#detail_OKButton") },
    $detail_CancelBtn: function () { return $("#detail_CancelBtn") },

    $DetailedDate: function () { return $("#DetailedDate") },

    $ChannelDate: function () { return $("#ChannelDate") },
    $BankDate: function () { return $("#BankDate") },
    $RevenueDate: function () { return $("#RevenueDate") },
    $DepositDate: function () { return $("#DepositDate") },

    $ChannelTotal: function () { return $("#ChannelTotal") },
    $BankTotal: function () { return $("#BankTotal") },
    $RevenueSystemTotal: function () { return $("#RevenueSystemTotal") },
    $RevenueTotal: function () { return $("#RevenueTotal") },
    $ArrearsRevenueTotal: function () { return $("#ArrearsRevenueTotal") },
    $ArrearsChannelTotal: function () { return $("#ArrearsChannelTotal") },
    $ArrearsDepositTotal: function () { return $("#ArrearsDepositTotal") },
    $DepositTotal: function () { return $("#DepositTotal") },

    $ChanneljqxTable: function () { return $("#ChanneljqxTable") },
    $DepositjqxTable: function () { return $("#DepositjqxTable") },
    $BankjqxTable: function () { return $("#BankjqxTable") },
    $RevenuejqxTable: function () { return $("#RevenuejqxTable") },
    $detail_BeforeDate: function () { return $("#detail_BeforeDate") },
    $detail_AfterDate: function () { return $("#detail_AfterDate") },
    $detail_ReconciliationsBtn: function () { return $("#detail_ReconciliationsBtn") },
    $RevenueDetailed: function () { return $("#RevenueDetailed") },
    $ChannelDetailed: function () { return $("#ChannelDetailed") },
    $DepositDetailed: function () { return $("#DepositDetailed") },
    $BankDetailed: function () { return $("#BankDetailed") },
    $hidestatus: function () { return $("#hidestatus") },
    $ChannelData: function () { return $("#ChannelData") },
    $hideChannelid: function () { return $("#hideChannelid") },
};
var status = "";
var currentDate = "";
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        //加载部门下拉框
        //initOrganization();
        //getChannelInfos();
        //层级下拉框改变事件
        //selector.$drdLevel().on("change", function () {
        //    selector.$txtChannel().val("");
        //    selector.$departmentVguid().val("");
        //    selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', "");
        //    switch (selector.$drdLevel().val()) {
        //        case "1":
        //            $(".cover").hide();
        //            $(".cover1").hide();
        //            break;
        //        case "2":
        //            $(".cover1").show();
        //            $(".cover").hide();
        //            break;
        //        case "3":
        //            $(".cover").show();
        //            $(".cover1").hide();
        //            break;
        //    }
        //});
        initTable();
        //查询（原）
        //selector.$btnSearch().on("click", function () {
        //    var start = selector.$txtPaymentForm().val();
        //    var end = selector.$txtPaymentTo().val();
        //    if (start == "" && end == "") {
        //        jqxNotification("请选择对账条件！", null, "error");
        //        return false;
        //    } else {
        //        if (selector.$drdLevel().val() == "2") {
        //            if (selector.$txtChannel().val() == "") {
        //                jqxNotification("请选择对账条件！", null, "error");
        //                return false;
        //            }
        //        }
        //        if (selector.$drdLevel().val() == "3") {
        //            if (selector.$departmentVguid().val() == "") {
        //                jqxNotification("请选择对账条件！", null, "error");
        //                return false;
        //            }
        //        }
        //    }
        //    $.ajax({
        //        url: "/ReportManagement/ReconciliationReport/IsExistReconciliationInfo",
        //        data: {
        //            "Level": selector.$drdLevel().val(),
        //            "Channel": selector.$txtChannel().val(),
        //            "Department": selector.$departmentVguid().val(),
        //            "PayDateFrom": selector.$txtPaymentForm().val(),
        //            "PayDateTo": selector.$txtPaymentTo().val()
        //        },
        //        datatype: "json",
        //        type: "post",
        //        success: function (msg) {
        //            if (msg.IsSuccess) {
        //                WindowConfirmDialog(del, "对账信息已存在，是否重新对账？", "确认框", "确认", "取消");
        //            } else {
        //                initTable();
        //            }
        //        }
        //    });
        //});

        //查询（新）bing.cheng 2018/4/16
        selector.$btnSearch().on("click", function () {
            var start = selector.$txtPaymentForm().val();
            var end = selector.$txtPaymentTo().val();
            if (start == "" && end == "") {
                jqxNotification("请选择对账条件！", null, "error");
                return false;
            }
            initTable();
        });

        selector.$RevenueDetailed().on("click", function () {
            if (selector.$RevenueDetailed().attr("load") == undefined) {
                getRevenuejqxTable();
                selector.$RevenueDetailed().attr("load", "true");
            }
            selector.$RevenuejqxTable().toggle();
            //selector.$RevenuejqxTable().css('display', 'block')
        });
        selector.$ChannelDetailed().on("click", function () {
            if (selector.$ChannelDetailed().attr("load") == undefined) {
                getChanneljqxTable();
                selector.$ChannelDetailed().attr("load", "true");
            }
            selector.$ChanneljqxTable().toggle();
            //selector.$ChanneljqxTable().css('display', 'block')
        });
        selector.$DepositDetailed().on("click", function () {
            if (selector.$DepositDetailed().attr("load") == undefined) {
                getDepositjqxTable();
                selector.$DepositDetailed().attr("load", "true");
            }
            selector.$DepositjqxTable().toggle();
            //selector.$ChanneljqxTable().css('display', 'block')
        });
        selector.$BankDetailed().on("click", function () {
            if (selector.$BankDetailed().attr("load") == undefined) {
                getBankjqxTable();
                selector.$BankDetailed().attr("load", "true");
            }
            selector.$BankjqxTable().toggle();
            //selector.$BankjqxTable().css('display', 'block')
        });

        selector.$detail_BeforeDate().on("click", function () {
            var revenueDateString = selector.$RevenueDate().text();
            var revenueDateArray = revenueDateString.split(",");
            var date = new Date(revenueDateArray[revenueDateArray.length - 1]);
            var newDate = new Date(date.getTime() - (1000 * 60 * 60 * 24));
            var newDateString = newDate.Format("yyyy-MM-dd");
            //验证营收日期是否已经对账
            ValidateReconciliation(newDateString, function () {
                revenueDateString = revenueDateString + "," + newDateString;
                selector.$RevenueDate().text(revenueDateString);
                selector.$ChannelDate().text(revenueDateString);
                selector.$DepositDate().text(revenueDateString);
                GetTotalAmount();
                if (selector.$RevenueDetailed().attr("load") == "true") {
                    getRevenuejqxTable();
                }
                if (selector.$ChannelDetailed().attr("load") == "true") {
                    getChanneljqxTable();
                }
                if (selector.$DepositDetailed().attr("load") == "true") {
                    getDepositjqxTable();
                }
            })

        });
        selector.$detail_AfterDate().on("click", function () {
            var revenueDateString = selector.$RevenueDate().text();
            var revenueDateArray = revenueDateString.split(",");
            var date = new Date(revenueDateArray[revenueDateArray.length - 1]);
            var revenueDateString = revenueDateString.replace("," + date.Format("yyyy-MM-dd"), "");
            selector.$RevenueDate().text(revenueDateString);
            selector.$ChannelDate().text(revenueDateString);
            selector.$DepositDate().text(revenueDateString);
            GetTotalAmount();
            if (selector.$RevenueDetailed().attr("load") == "true") {
                getRevenuejqxTable();
            }
            if (selector.$ChannelDetailed().attr("load") == "true") {
                getChanneljqxTable();
            }
            if (selector.$DepositDetailed().attr("load") == "true") {
                getDepositjqxTable();
            }
        });

        selector.$detail_ReconciliationsBtn().on("click", function () {

            if (selector.$hidestatus().val() != "已对账") {
                RevenuepaymentReconciliation();

            } else {
                WindowConfirmDialog(RevenuepaymentReconciliation, "对账信息已存在，是否重新对账？", "确认框", "确认", "取消");
            }

        })



        //function del() {
        //    initTable();
        //    //$.ajax({
        //    //    url: "/ReportManagement/ReconciliationReport/DeleteReconciliationInfos",
        //    //    datatype: "json",
        //    //    type: "post",
        //    //    success: function (msg) {
        //    //        if (msg.Status == "1") {
        //    //            initTable();
        //    //        } else {
        //    //            jqxNotification("删除失败！", null, "error");
        //    //        }
        //    //    }
        //    //});
        //}
        //重置
        selector.$btnReset().on("click", function () {
            selector.$txtPaymentForm().val("");
            selector.$txtPaymentTo().val("");
            //selector.$drdLevel().val("1");
            //selector.$drdLevel().change();
            selector.$txtChannel().val("");
            //selector.$departmentVguid().val("");
            ////selector.$pushPeopleDropDownButton().jqxDropDownButton('setContent', "");
        });

        //弹出框中的取消按钮
        selector.$detail_CancelBtn().on("click", function () {
            selector.$detailDialog().modal("hide");
        });
        //弹出框中的导出按钮
        //selector.$detail_OKButton().on("click", function () {
        //    var data = {
        //        "PayDateFrom": currentDate,
        //        "Level": selector.$drdLevel().val(),
        //        "Channel": selector.$txtChannel().val(),
        //        "Department": selector.$departmentVguid().val(),
        //        "ReasonStatus": selector.$drdReasonStatus().val()
        //    };
        //    window.location.href = "/ReportManagement/ReconciliationReport/ExportDetail?paras=" + JSON.stringify(data);
        //});

        //selector.$drdReasonStatus().on("change", function () {
        //    status = selector.$drdReasonStatus().val();
        //    initDetailTable(currentDate, status);
        //});
    }; //addEvent end

    //初始化表格（现）bing.cheng2018-4-16
    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'BankBillDate', type: 'date' },
                    { name: 'BankBillTotalAmount', type: 'number' },
                    { name: 'Channel_Id', type: 'string' },
                    { name: 'Channel_Name', type: 'string' },
                    //{ name: 'PaymentAmount', type: 'string' },
                    { name: 'BatchBillNo', type: 'string' },
                    { name: 'AbnormalReason', type: 'string' },
                    { name: 'RevenueDate', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'StatusName', type: 'string' },
                    { name: 'ReconciliationUser', type: 'string' },
                    { name: 'ReconciliationDate', type: 'date' },
                    { name: 'VGUID', type: 'string' }
                ],
                datatype: "json",
                id: "RoleID",
                async: true,
                data: {
                    "PayDateFrom": selector.$txtPaymentForm().val(),
                    "PayDateTo": selector.$txtPaymentTo().val(),
                    //"Level": selector.$drdLevel().val(),
                    "Channel": selector.$txtChannel().val(),
                    //"Department": selector.$departmentVguid().val()
                    "Status": selector.$txtStatus().val(),
                },
                url: "/ReportManagement/ReconciliationReport/GetReconciliations"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid().jqxDataTable(
            {
                //pageable: true,
                width: "100%",
                height: 510,
                //pageSize: 10,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                columns: [
                    //{ width: 35, text: "", datafield: "checkbox", align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '银行日期', datafield: 'BankBillDate', width: 120, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd", cellsRenderer: redcolcorFunc },
                    { text: '银行总额', datafield: 'BankBillTotalAmount', cellsFormat: "d2", width: 100, align: 'center', cellsAlign: 'center', cellsRenderer: redcolcorFunc },
                    { text: '对账营收日期', datafield: 'RevenueDate', minwidth: 120, align: 'center', cellsAlign: 'center', cellsRenderer: redcolcorFunc },
                    { text: '渠道', datafield: 'Channel_Name', minwidth: 120, align: 'center', cellsAlign: 'center', cellsRenderer: redcolcorFunc },
                    { text: '状态', datafield: 'StatusName', width: 120, align: 'center', cellsAlign: 'center', cellsRenderer: redcolcorFunc },
                    { text: '对账人', datafield: 'ReconciliationUser', width: 120, align: 'center', cellsAlign: 'center', cellsRenderer: redcolcorFunc },
                    { text: '对账时间', datafield: 'ReconciliationDate', width: 120, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd", cellsRenderer: redcolcorFunc },
                    { text: '入账批次号', datafield: 'BatchBillNo', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: redcolcorFunc },
                    { text: '备注', datafield: 'AbnormalReason', width: 200, align: 'center', cellsAlign: 'center', cellsRenderer: redcolcorFunc },
                    { text: '操作', align: 'center', cellsAlign: 'center', width: 80, cellsRenderer: cellsRendererFunc },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                    { text: 'Channel_Id', datafield: 'Channel_Id', hidden: true },
                ]
            });

    }
    function cellsRendererFunc(row, column, value, rowData) {
        var container = "";
        if (rowData.Status != "2") {
            container = "<a href='#' onclick=\"edit('" + rowData.BankBillDate + "','" + rowData.RevenueDate + "','" + rowData.status + "','" + rowData.Channel_Id + "','" + rowData.Channel_Name + "')\">对账</a>";
        }
        return container;
    }

    function redcolcorFunc(row, column, value, rowData) {
        var container = "";
        if (rowData.Status == "2") {
            container = "<font style='color:green'>" + value + "</font>";
        }
        else if (rowData.Status == "3") {
            container = "<font style='color:red'>" + value + "</font>";
        }
        else {
            container = value;
        }
        return container;
    }
};


Date.prototype.Format = function (fmt) { //author: meizz 
    var o = {
        "M+": this.getMonth() + 1, //月份 
        "d+": this.getDate(), //日 
        "H+": this.getHours(), //小时 
        "m+": this.getMinutes(), //分 
        "s+": this.getSeconds(), //秒 
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
        "S": this.getMilliseconds() //毫秒 
    };
    if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}


function edit(date, revenueDateString, status, Channel_Id, Channel_name) {
    selector.$RevenuejqxTable().css('display', 'none');
    selector.$ChanneljqxTable().css('display', 'none');
    selector.$BankjqxTable().css('display', 'none');
    selector.$RevenueDetailed().removeAttr("load");
    selector.$ChannelDetailed().removeAttr("load");
    selector.$BankDetailed().removeAttr("load");
    selector.$RevenueSystemTotal().text("");
    selector.$RevenueTotal().text("");
    selector.$BankTotal().text("");
    selector.$ChannelTotal().text("");

    selector.$RevenueDate().text("");
    selector.$BankDate().text("");
    selector.$ChannelDate().text("");

    var bankDate = new Date(date);
    var revenueDate = new Date(bankDate.getTime() - (1000 * 60 * 60 * 24));
    var bankDateString = bankDate.Format("yyyy-MM-dd")
    //var dateString = d.getFullYear() + '-' + (d.getMonth() + 1) + '-' + d.getDate();
    selector.$DetailedDate().text(bankDateString);
    selector.$BankDate().text(bankDateString);
    if (revenueDateString == undefined || revenueDateString == null || revenueDateString == "") {
        revenueDateString = revenueDate.Format("yyyy-MM-dd");
    }
    selector.$RevenueDate().text(revenueDateString);
    selector.$ChannelDate().text(revenueDateString);
    selector.$DepositDate().text(revenueDateString);

    selector.$hidestatus().val(status);
    if (Channel_name != "null") {
        selector.$ChannelData().text(Channel_name);
        selector.$hideChannelid().val(Channel_Id);
    }
    GetTotalAmount();
    selector.$detailDialog().modal({ backdrop: "static", keyboard: false });
    selector.$detailDialog().modal("show");
}
//获取对账金额
function GetTotalAmount() {
    $.ajax({
        url: "/ReportManagement/ReconciliationReport/GetTotalAmount",
        data: { "BankDate": selector.$DetailedDate().text(), "RevenueDate": selector.$RevenueDate().text(), "Channel_Id": selector.$hideChannelid().val() },
        traditional: true,
        type: "post",
        success: function (msg) {
            selector.$RevenueSystemTotal().text(msg.ResultInfo.RevenueSystemTotalAccount);
            selector.$ArrearsRevenueTotal().text(msg.ResultInfo.RevenueArrearsTotalAccount);
            selector.$RevenueTotal().text(msg.ResultInfo.RevenuePaymentTotalAccount);
            selector.$ArrearsChannelTotal().text(msg.ResultInfo.T1DataArrearsTotalAccount);
            selector.$ChannelTotal().text(msg.ResultInfo.T1DataPaymentTotalAccount);

            selector.$ArrearsDepositTotal().text(msg.ResultInfo.DepositArrearsTotalAccount);
            selector.$DepositTotal().text(msg.ResultInfo.DepositPaymentTotalAccount);

            selector.$BankTotal().text(msg.ResultInfo.BankTotalAccount);
        }
    });
}

function getRevenuejqxTable() {
    var source =
        {
            datafields:
            [
                { name: "Name", type: "string" },
                { name: "PhoneNumber", type: "string" },
                { name: 'PaymentAmount', type: 'number' },
                { name: 'DriverBearFees', type: 'number' },
                { name: 'copeFee', type: 'number' },
                { name: 'ActualAmount', type: 'number' },
                { name: 'copeFee', type: 'number' },
                { name: 'CompanyBearsFees', type: 'number' },
                { name: 'ChannelPayableAmount', type: 'number' },
                { name: "PaymentPersonnel", type: "string" },
                { name: "TransactionID", type: "string" },
                { name: "PayDate", type: "date" },
                { name: "Channel_IdDESC", type: "string" }
            ],
            datatype: "json",
            //id: "RoleID",
            async: true,
            data: {
                "RevenueDate": selector.$RevenueDate().text(),
                "Channel_Id": selector.$hideChannelid().val(),
            },
            url: "/ReportManagement/ReconciliationReport/GetRevenueDetail"   //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    //创建卡信息列表（主表）
    selector.$RevenuejqxTable().jqxDataTable(
        {
            pageable: true,
            width: 860,
            height: 300,
            pageSize: 10,
            serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 40,
            columns: [
                { text: '姓名', datafield: 'Name', width: 100, align: 'center', cellsAlign: 'center' },
                { text: '流水号', datafield: 'TransactionID', width: '30%', align: 'center', cellsAlign: 'center' },
                { text: '驾驶员欠款金额', cellsFormat: "d2", width: 150, datafield: 'PaymentAmount', align: 'center', cellsAlign: 'center' },
                { text: '驾驶员承担手续费', cellsFormat: "d2", width: 150, datafield: 'DriverBearFees', align: 'center', cellsAlign: 'center' },
                { text: '驾驶员实付金额', cellsFormat: "d2", width: 150, datafield: 'ActualAmount', align: 'center', cellsAlign: 'center' },
                { text: '渠道实收手续费', cellsFormat: "d2", width: 150, datafield: 'copeFee', align: 'center', cellsAlign: 'center' },
                { text: '公司承担手续费', cellsFormat: "d2", width: 150, datafield: 'CompanyBearsFees', align: 'center', cellsAlign: 'center' },
                { text: '渠道应付金额', cellsFormat: "d2", width: 150, datafield: 'ChannelPayableAmount', align: 'center', cellsAlign: 'center' },
                { text: '付款时间', datafield: 'PayDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                { text: '渠道名称', datafield: 'Channel_IdDESC', width: 150, align: 'center', cellsAlign: 'center' },

            ]
        });
}

function getChanneljqxTable() {

    var source =
        {
            datafields:
            [
                { name: 'Remitamount', type: 'number' },
                { name: 'DriverBearFees', type: 'number' },
                { name: 'PaidAmount', type: 'number' },
                { name: 'RevenueFee', type: 'number' },
                { name: 'CompanyBearsFees', type: 'number' },
                { name: 'ChannelPayableAmount', type: 'number' },
                { name: "WechatNo", type: "string" },
                { name: "Revenuetime", type: "date" },
                { name: "serialnumber", type: "string" },
                { name: "ChannelName", type: "string" },

            ],
            datatype: "json",
            //id: "RoleID",
            async: true,
            data: {
                "RevenueDate": selector.$RevenueDate().text(),
                "Channel_Id": selector.$hideChannelid().val(),
            },
            url: "/ReportManagement/ReconciliationReport/GetChannelDetail"   //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    //创建卡信息列表（主表）
    selector.$ChanneljqxTable().jqxDataTable(
        {
            pageable: true,
            width: 860,
            height: 300,
            pageSize: 10,
            serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 40,
            columns: [
                { text: '流水号', datafield: 'serialnumber', width: '30%', align: 'center', cellsAlign: 'center' },
                { text: '驾驶员欠款金额', cellsFormat: "d2", width: 150, datafield: 'Remitamount', align: 'center', cellsAlign: 'center' },// cellsRenderer: detailFunc 
                { text: '驾驶员承担手续费', cellsFormat: "d2", width: 150, datafield: 'DriverBearFees', align: 'center', cellsAlign: 'center' },
                { text: '驾驶员实付金额', cellsFormat: "d2", width: 150, datafield: 'PaidAmount', align: 'center', cellsAlign: 'center' },
                { text: '渠道实收手续费', cellsFormat: "d2", width: 150, datafield: 'RevenueFee', align: 'center', cellsAlign: 'center' },
                { text: '公司承担手续费', cellsFormat: "d2", width: 150, datafield: 'CompanyBearsFees', align: 'center', cellsAlign: 'center' },
                { text: '渠道应付金额', cellsFormat: "d2", width: 150, datafield: 'ChannelPayableAmount', align: 'center', cellsAlign: 'center' },
                { text: '支付时间', datafield: 'Revenuetime', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                { text: '渠道名称', datafield: 'ChannelName', width: 150, align: 'center', cellsAlign: 'center' },

            ]
        });
}

function getDepositjqxTable() {

    var source =
        {
            datafields:
            [
                { name: 'Remitamount', type: 'number' },
                { name: 'DriverBearFees', type: 'number' },
                { name: 'PaidAmount', type: 'number' },
                { name: 'RevenueFee', type: 'number' },
                { name: 'CompanyBearsFees', type: 'number' },
                { name: 'ChannelPayableAmount', type: 'number' },
                { name: "WechatNo", type: "string" },
                { name: "Revenuetime", type: "date" },
                { name: "serialnumber", type: "string" },
                { name: "ChannelName", type: "string" },

            ],
            datatype: "json",
            //id: "RoleID",
            async: true,
            data: {
                "RevenueDate": selector.$RevenueDate().text(),
                "Channel_Id": selector.$hideChannelid().val(),
            },
            url: "/ReportManagement/ReconciliationReport/GetDepositDetail"   //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    //创建卡信息列表（主表）
    selector.$DepositjqxTable().jqxDataTable(
        {
            pageable: true,
            width: 860,
            height: 300,
            pageSize: 10,
            serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 40,
            columns: [
                { text: '流水号', datafield: 'serialnumber', width: '30%', align: 'center', cellsAlign: 'center' },
                { text: '驾驶员欠款金额', cellsFormat: "d2", width: 150, datafield: 'Remitamount', align: 'center', cellsAlign: 'center' },// cellsRenderer: detailFunc 
                { text: '驾驶员承担手续费', cellsFormat: "d2", width: 150, datafield: 'DriverBearFees', align: 'center', cellsAlign: 'center' },
                { text: '驾驶员实付金额', cellsFormat: "d2", width: 150, datafield: 'PaidAmount', align: 'center', cellsAlign: 'center' },
                { text: '渠道实收手续费', cellsFormat: "d2", width: 150, datafield: 'RevenueFee', align: 'center', cellsAlign: 'center' },
                { text: '公司承担手续费', cellsFormat: "d2", width: 150, datafield: 'CompanyBearsFees', align: 'center', cellsAlign: 'center' },
                { text: '渠道应付金额', cellsFormat: "d2", width: 150, datafield: 'ChannelPayableAmount', align: 'center', cellsAlign: 'center' },
                { text: '支付时间', datafield: 'Revenuetime', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                { text: '渠道名称', datafield: 'ChannelName', width: 150, align: 'center', cellsAlign: 'center' },

            ]
        });
}

function getBankjqxTable() {
    var source =
        {
            datafields:
            [
                { name: "ArrivedTime", type: "date" },
                { name: "ArrivedTotal", type: "string" },
                { name: "Name", type: "string" },
                { name: "remark", type: "string" },

            ],
            datatype: "json",
            //id: "RoleID",
            async: true,
            data: {
                "BankDate": selector.$DetailedDate().text(),
                "Channel_Id": selector.$hideChannelid().val(),
            },
            url: "/ReportManagement/ReconciliationReport/GetBankDetail"   //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    //创建卡信息列表（主表）
    selector.$BankjqxTable().jqxDataTable(
        {
            pageable: true,
            width: 860,
            height: 300,
            pageSize: 10,
            serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 40,
            columns: [
                //{ width: 35, text: "", datafield: "checkbox", align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                { text: '到账日期', datafield: 'ArrivedTime', width: '10%', align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                { text: '到账金额', datafield: 'ArrivedTotal', width: '10%', align: 'center', cellsAlign: 'center' },
                { text: '渠道', datafield: 'Name', width: '30%', align: 'center', cellsAlign: 'center' },
                { text: '摘要', datafield: 'remark', width: '50%', align: 'center', cellsAlign: 'center' },

            ]
        });
}

function RevenuepaymentReconciliation() {
    $.ajax({
        url: "/ReportManagement/ReconciliationReport/RevenuepaymentReconciliation",
        data: {
            "BankDate": selector.$DetailedDate().text(),
            "RevenueDate": selector.$RevenueDate().text(),
            "Channel_Id": selector.$hideChannelid().val(),
            "RevenueSystemTotal": selector.$RevenueSystemTotal().text(),
            "ArrearsRevenueTotal": selector.$ArrearsRevenueTotal().text(),
            "RevenueTotal": selector.$RevenueTotal().text(),
            "ArrearsChannelTotal": selector.$ArrearsChannelTotal().text(),
            "ChannelTotal": selector.$ChannelTotal().text(),

            "ArrearsDepositTotal": selector.$ArrearsDepositTotal().text(),
            "DepositTotal": selector.$DepositTotal().text(),

            "BankTotal": selector.$BankTotal().text(),

        },
        datatype: "json",
        type: "post",
        success: function (msg) {
            if (msg.IsSuccess) {
                jqxNotification("对账完成！", null, "success");
                selector.$detailDialog().modal("hide");
            } else {
                jqxNotification("对账异常！ " + msg.ResultInfo, null, "error");
                //selector.$detailDialog().modal("hide");
            }
            selector.$grid().jqxDataTable('updateBoundData');
        }
    });
}

function ValidateReconciliation(RevenueDate, callback) {
    $.ajax({
        url: "/ReportManagement/ReconciliationReport/ValidateReconciliation",
        data: {
            "RevenueDate": RevenueDate,
            "Channel_Id": selector.$hideChannelid().val()
        },
        datatype: "json",
        type: "post",
        success: function (msg) {
            if (msg.IsSuccess == true) {
                jqxNotification("营收日期已对账或对账异常！", null, "error");
            }
            else {
                callback();
            }
        }
    });
}



$(function () {
    var page = new $page();
    page.init();

});
