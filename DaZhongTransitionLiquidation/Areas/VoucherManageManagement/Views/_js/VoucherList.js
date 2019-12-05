//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $grid1: function () { return $("#jqxTable1") },
    $grid2: function () { return $("#jqxTable2") },
    $grid3: function () { return $("#jqxTable3") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") },
    $AddBankChannelDialog: function () { return $("#AddBankChannelDialog") },
    $AddBankChannel_OKButton: function () { return $("#AddBankChannel_OKButton") },
    $AddBankChannel_CancelBtn: function () { return $("#AddBankChannel_CancelBtn") },
}; //selector end
var isEdit = false;
var vguid = "";
var type = $.request.queryString().Type;
var tableIndex = 0;
var status = $.request.queryString().Status;
var createType = "";
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        getBankInfo();
        if (status == "1") {
            $("#buttonList").show();
            if (type == "转账类" || type == "2") {
                $("#btnCreate").show();
                $("#btnCreateModel").show();
            }
        }
        if (status == "2") {
            $("#buttonList2").show();
            $("#Oracle").show();
            $("#btnCheck").show();
            $("#OracleCheck").hide();
            $("#btnCheckStatus").hide();
            //$("#AddSubject_OKButton").show();
        }
        if (status == "3") {
            $("#buttonList2").show();
            $("#Oracle").show();
            $("#OracleCheck").hide();
            $("#btnCheckStatus").hide();
            $("#btnCheck").hide();
            $("#AddSubject_OKButton").hide();
        }
        if (status == "4") {
            $("#buttonList2").show();
            $("#Oracle").hide();
            $("#Oracle0").hide();
            $("#Oracle1").hide();
            $("#AddSubject_OKButton").hide();
            $("#OracleCheck").show();
            $("#btnCheckStatus").show();
            $('#jqxTabs').jqxTabs({ selectedItem: 0 })
        }
        
        //加载列表数据
        selector.$btnSearch().unbind("click").on("click", function () {
            if (type == "银行类") {
                //initTable1();
            }
            if (status == "4") {
                initTable3();
            } else {
                switch (tableIndex) {
                    case 0: initTable(); break;
                    case 1: initTable1(); break;
                    case 2: initTable2(); break;
                    default: break;
                }
            }
        });

        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#ReceivingUnit").val("");
            $("#TradingBank").val("");
            $("#TransactionDateS").val("");
            $("#TransactionDateE").val("");
        });
        //打印
        $("#btnPrint").on("click", function () {
            print();
        });
        $("#btnPrint2").on("click", function () {
            print();
        });
        //新增
        $("#btnAdd").on("click", function () {
            if (type == "现金类") {
                window.location.href = "/VoucherManageManagement/VoucherListDetail/Index?Type=0";
            }else if (type == "银行类") {
                window.location.href = "/VoucherManageManagement/VoucherListDetail/Index?Type=1";
            } else if (type == "转账类") {
                window.location.href = "/VoucherManageManagement/VoucherListDetail/Index?Type=2";
            }
           
            //window.open("/VoucherManageManagement/VoucherListDetail/Index");
        });
        //删除
        $("#btnDelete").on("click", function () {
            var selection = [];
            var grid = $("#jqxTable");
            if (tableIndex == 1) {
                grid = $("#jqxTable1");
            }
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
        //提交
        $("#btnUp").on("click", function () {
            var selection = [];
            var grid = "";
            if (tableIndex == 1) {
                grid = $("#jqxTable1");
            } else {
                grid = $("#jqxTable");
            }
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
        //审核
        $("#btnCheck").on("click", function () {
            var selection = [];
            var grid = "";
            if (tableIndex == 1) {
                grid = $("#jqxTable1");
            } else if (tableIndex == 0) {
                grid = $("#jqxTable");
            } else if (tableIndex == 2) {
                grid = $("#jqxTable2");
            }
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
                WindowConfirmDialog(check, "您确定要提交选中的数据？", "确认框", "确定", "取消", selection);
            }
        });
        $('#jqxTabs').on('tabclick', function (event) {
            tableIndex = event.args.item;
        });
        //同步
        $("#AddSubject_OKButton").on("click", function () {
            syncAssetsData();
        });
        //校验
        $("#btnCheckStatus").on("click", function () {
            checkOracleData();
        });
        //生成--根据结算项目或者模板
        $("#btnCreate").on("click", function () {
            createType = "1";
            selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
            selector.$AddBankChannelDialog().modal("show");
        });
        $("#btnCreateModel").on("click", function () {
            createType = "2";
            getVoucherModelVGUID();
            selector.$AddBankChannelDialog().modal({ backdrop: "static", keyboard: false });
            selector.$AddBankChannelDialog().modal("show");
        });
        //弹出框中的取消按钮
        selector.$AddBankChannel_CancelBtn().on("click", function () {
            selector.$AddBankChannelDialog().modal("hide");
        });
        //弹出框中的保存按钮
        selector.$AddBankChannel_OKButton().on("click", function () {
            var year = $("#Year").val();
            var month = $("#Month").val();
            var guid = modelVGUID[0].VGUID;
            switch (createType) {
                case "1": createVoucher(year, month); break;
                case "2": getVoucherModel(year, month, guid); break;
            }
        });
        //上一页
        $("#btnPre").on("click", function () {
            voucherIndex--;
            pageIndex--;
            if (pageIndex >= 1) {
                var year = $("#Year").val();
                var month = $("#Month").val();
                var guid = modelVGUID[voucherIndex].VGUID;
                getVoucherModel(year, month, guid);
                //previewVoucher(voucherList[voucherIndex]);
            } else {
                voucherIndex = 0;
                pageIndex = 1;
            }
            $("#btnNext").show();
            $("#btnFinish").hide();
        });
        //下一页
        $("#btnNext").on("click", function () {
            voucherIndex++;
            pageIndex++;
            if (pageIndex <= modelVGUID.length) {
                var borrowCount = $("#BorrowCount").val();
                var loanCount = $("#LoanCount").val();
                if (borrowCount == loanCount) {
                    var year = $("#Year").val();
                    var month = $("#Month").val();
                    var guid = modelVGUID[voucherIndex].VGUID;
                    saveVoucherModel();
                    getVoucherModel(year, month, guid);
                    //previewVoucher(voucherList[voucherIndex]);
                } else {
                    jqxNotification("借贷金额不平！", null, "error");
                }
            }
            if (pageIndex == modelVGUID.length) {
                $("#btnNext").hide();
                $("#btnFinish").show();
            }
        });
        //完成
        $("#btnFinish").on("click", function () {
            voucherIndex = 0;
            pageIndex = 1;
            saveVoucherModel();
            $("#btnNext").show();
            $("#btnFinish").hide();
            $("#ShowDialog").modal({ backdrop: "static", keyboard: false });
            $("#ShowDialog").modal("hide");
        });
        //取消
        $("#AddNewBankData_CancelBtn").on("click", function () {
            voucherIndex = 0;
            pageIndex = 1;
            $("#ShowDialog").modal({ backdrop: "static", keyboard: false });
            $("#ShowDialog").modal("hide");
        })
    }; //addEvent end
    //打印
    function print() {
        var selection = [];
        var grid = $("#jqxTable");
        if (tableIndex == 1) {
            grid = $("#jqxTable1");
        }
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
            jqxNotification("请选择您要打印的数据！", null, "error");
        } else {
            layer.load();
            $.ajax({
                url: "/VoucherManageManagement/VoucherListDetail/PrintVoucherList",
                data: { vguids: selection },
                //async: false,
                type: "post",
                success: function (msg) {
                    layer.closeAll('loading');
                    if (msg.ResultInfo != null) {
                        window.open(msg.ResultInfo);
                    }
                }
            });
        }
    }
    //删除
    function dele(selection) {
        $.ajax({
            url: "/VoucherManageManagement/VoucherList/DeleteVoucherListInfo",
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
                        if (tableIndex == 1) {
                            $("#jqxTable1").jqxDataTable('updateBoundData');
                        } else {
                            $("#jqxTable").jqxDataTable('updateBoundData');
                        }
                        break;
                }
            }
        });
    }
    //提交
    function submit(selection) {
        $.ajax({
            url: "/VoucherManageManagement/VoucherList/UpdataVoucherListInfo",
            data: { vguids: selection, status:"2" },
            traditional: true,
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                    case "0":
                        jqxNotification("提交失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("提交成功！", null, "success");
                        if (tableIndex == 1) {
                            $("#jqxTable1").jqxDataTable('updateBoundData');
                        } else {
                            $("#jqxTable").jqxDataTable('updateBoundData');
                        }
                        break;
                    case "2":
                        jqxNotification(msg.ResultInfo + "条凭证借贷不相平,提交失败！", null, "error");
                        break;
                }
            }
        });
    }
    //审核
    function check(selection) {
        $.ajax({
            url: "/VoucherManageManagement/VoucherList/UpdataVoucherListInfo",
            data: { vguids: selection, status: "3", index: tableIndex },
            traditional: true,
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                    case "0":
                        jqxNotification("提交失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("提交成功！", null, "success");
                        if (tableIndex == 1) {
                            $("#jqxTable1").jqxDataTable('updateBoundData');
                        } else if(tableIndex == 0)  {
                            $("#jqxTable").jqxDataTable('updateBoundData');
                        } else if (tableIndex == 2) {
                            $("#jqxTable2").jqxDataTable('updateBoundData');
                        }
                        break;
                }
            }
        });
    }

    function initTable() {
        var DateEnd = $("#TransactionDateE").val();
        var DateStar = $("#TransactionDateS").val();
        var status = $.request.queryString().Status; 
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'CompanyCode', type: 'string' },
                    { name: 'CompanyName', type: 'string' },
                    { name: 'AccountingPeriod', type: 'date' },
                    { name: 'Currency', type: 'string' },
                    { name: 'BatchName', type: 'string' },
                    { name: 'VoucherNo', type: 'string' },
                    { name: 'VoucherDate', type: 'date' },
                    { name: 'VoucherType', type: 'string' },
                    { name: 'DebitAmountTotal', type: 'number' },
                    { name: 'CreditAmountTotal', type: 'number' },
                    { name: 'FinanceDirector', type: 'string' },
                    { name: 'Bookkeeping', type: 'string' },
                    { name: 'Auditor', type: 'string' },
                    { name: 'DocumentMaker', type: 'string' },
                    { name: 'Cashier', type: 'string' },
                    { name: 'Attachment1', type: 'string' },
                    { name: 'Attachment2', type: 'string' },
                    { name: 'Attachment3', type: 'string' },
                    { name: 'Attachment4', type: 'string' },
                    { name: 'Attachment5', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'TradingBank', type: 'string' },
                    { name: 'ReceivingUnit', type: 'string' },
                    { name: 'TransactionDate', type: 'date' },
                    { name: 'Batch', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                type: "post",
                data: {
                    "Status": status,
                    "AccountingPeriod": $("#AccountingPeriod").val(),
                    "Automatic": "0",
                    "VoucherType": type,
                    "TradingBank": $('#TradingBank option:selected').text(),
                    "TransactionDate": DateStar
                },
                url: "/VoucherManageManagement/VoucherList/GetVoucherListDatas?dateEnd=" + DateEnd   //获取数据源的路径
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
                columnsHeight: 25,
                columns: [
                    { text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                    { text: '批名', datafield: 'BatchName', width: 250, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '交易银行', datafield: 'TradingBank', width: 120, hidden: true, pinned: true, align: 'center', cellsAlign: 'center' },
                    { text: '交易日期', datafield: 'TransactionDate', width: 120, hidden: true, pinned: true, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '对方公司', datafield: 'ReceivingUnit', width: 380, hidden: true, align: 'center', cellsAlign: 'center', },
                    { text: '借方金额合计', datafield: 'DebitAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '贷方金额合计', datafield: 'CreditAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '凭证号码', datafield: 'VoucherNo', width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '我方公司', datafield: 'CompanyName', width: 380, align: 'center', cellsAlign: 'center', },
                    { text: '会计期', datafield: 'AccountingPeriod', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM" },
                    { text: '币种', datafield: 'Currency', hidden: true, width: 150, align: 'center', cellsAlign: 'center', },
                    { text: '凭证日期', datafield: 'VoucherDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '凭证类型', datafield: 'VoucherType', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '财务主管', datafield: 'FinanceDirector', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '记账', datafield: 'Bookkeeping', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '审核', datafield: 'Auditor', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '制单', datafield: 'DocumentMaker', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '交易流水号', datafield: 'Batch', hidden: true, width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '出纳', datafield: 'Cashier', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });
        selector.$grid().on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row index.
            //window.location.href = "/VoucherManageManagement/VoucherListDetail/Index?VGUID=" + row.VGUID;
        });
    }
    function initTable1() {
        var DateEnd = $("#TransactionDateE").val();
        var DateStar = $("#TransactionDateS").val();
        var status = $.request.queryString().Status;
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'CompanyCode', type: 'string' },
                    { name: 'CompanyName', type: 'string' },
                    { name: 'AccountingPeriod', type: 'date' },
                    { name: 'Currency', type: 'string' },
                    { name: 'BatchName', type: 'string' },
                    { name: 'VoucherNo', type: 'string' },
                    { name: 'VoucherDate', type: 'date' },
                    { name: 'VoucherType', type: 'string' },
                    { name: 'DebitAmountTotal', type: 'number' },
                    { name: 'CreditAmountTotal', type: 'number' },
                    { name: 'FinanceDirector', type: 'string' },
                    { name: 'Bookkeeping', type: 'string' },
                    { name: 'Auditor', type: 'string' },
                    { name: 'DocumentMaker', type: 'string' },
                    { name: 'Cashier', type: 'string' },
                    { name: 'Attachment1', type: 'string' },
                    { name: 'Attachment2', type: 'string' },
                    { name: 'Attachment3', type: 'string' },
                    { name: 'Attachment4', type: 'string' },
                    { name: 'Attachment5', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'TradingBank', type: 'string' },
                    { name: 'ReceivingUnit', type: 'string' },
                    { name: 'TransactionDate', type: 'date' },
                    { name: 'Batch', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                type:"post",
                data: {
                    "Status": status,
                    "ReceivingUnit": $("#ReceivingUnit").val(),
                    "Automatic": "1",
                    "VoucherType": type,
                    "TradingBank": $('#TradingBank option:selected').text(),
                    "TransactionDate": DateStar
                },
                url: "/VoucherManageManagement/VoucherList/GetVoucherListDatas?dateEnd=" + DateEnd   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid1().jqxDataTable(
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
                    { text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                    { text: '批名', datafield: 'BatchName', width: 250, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '交易银行', datafield: 'TradingBank', width: 120, pinned: true, align: 'center', cellsAlign: 'center' },
                    { text: '交易日期', datafield: 'TransactionDate', width: 120, pinned: true, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '对方公司', datafield: 'ReceivingUnit', width: 380, align: 'center', cellsAlign: 'center', },
                    { text: '借方金额合计', datafield: 'DebitAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '贷方金额合计', datafield: 'CreditAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '凭证号码', datafield: 'VoucherNo', width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '我方公司', datafield: 'CompanyName', width: 380, align: 'center', cellsAlign: 'center', },
                    { text: '会计期', datafield: 'AccountingPeriod', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM" },
                    { text: '币种', datafield: 'Currency', hidden: true, width: 150, align: 'center', cellsAlign: 'center', },
                    { text: '凭证日期', datafield: 'VoucherDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },    
                    { text: '凭证类型', datafield: 'VoucherType', width: 150, align: 'center', cellsAlign: 'center' },         
                    //{ text: '财务主管', datafield: 'FinanceDirector', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '记账', datafield: 'Bookkeeping', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '审核', datafield: 'Auditor', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '制单', datafield: 'DocumentMaker', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '交易流水号', datafield: 'Batch', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '出纳', datafield: 'Cashier', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });
        selector.$grid1().on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row index.
            //window.location.href = "/VoucherManageManagement/VoucherListDetail/Index?VGUID=" + row.VGUID;
        });
    }
    function initTable2() {
        var DateEnd = $("#TransactionDateE").val();
        var DateStar = $("#TransactionDateS").val();
        var status = $.request.queryString().Status;
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'CompanyCode', type: 'string' },
                    { name: 'CompanyName', type: 'string' },
                    { name: 'AccountingPeriod', type: 'date' },
                    { name: 'Currency', type: 'string' },
                    { name: 'BatchName', type: 'string' },
                    { name: 'VoucherNo', type: 'string' },
                    { name: 'VoucherDate', type: 'date' },
                    { name: 'VoucherType', type: 'string' },
                    { name: 'DebitAmountTotal', type: 'number' },
                    { name: 'CreditAmountTotal', type: 'number' },
                    { name: 'FinanceDirector', type: 'string' },
                    { name: 'Bookkeeping', type: 'string' },
                    { name: 'Auditor', type: 'string' },
                    { name: 'DocumentMaker', type: 'string' },
                    { name: 'Cashier', type: 'string' },
                    { name: 'Attachment1', type: 'string' },
                    { name: 'Attachment2', type: 'string' },
                    { name: 'Attachment3', type: 'string' },
                    { name: 'Attachment4', type: 'string' },
                    { name: 'Attachment5', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'TradingBank', type: 'string' },
                    { name: 'ReceivingUnit', type: 'string' },
                    { name: 'TransactionDate', type: 'date' },
                    { name: 'Batch', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: {
                    "Status": status,
                    "Automatic": "3",
                    "VoucherType": type,
                    "ReceivingUnit": $("#ReceivingUnit").val(),
                    "TradingBank": $('#TradingBank option:selected').text(),
                    "TransactionDate": DateStar
                },
                url: "/VoucherManageManagement/VoucherList/GetVoucherListDatas?dateEnd=" + DateEnd   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid2().jqxDataTable(
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
                    { text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                    { text: '批名', datafield: 'BatchName', width: 250, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '交易银行', datafield: 'TradingBank', width: 120, pinned: true, align: 'center', cellsAlign: 'center' },
                    { text: '交易日期', datafield: 'TransactionDate', width: 120, pinned: true, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '对方公司', datafield: 'ReceivingUnit', width: 380, align: 'center', cellsAlign: 'center', },
                    { text: '借方金额合计', datafield: 'DebitAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '贷方金额合计', datafield: 'CreditAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '凭证号码', datafield: 'VoucherNo', width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '我方公司', datafield: 'CompanyName', width: 380, align: 'center', cellsAlign: 'center', },
                    { text: '会计期', datafield: 'AccountingPeriod', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM" },
                    { text: '币种', datafield: 'Currency', hidden: true, width: 150, align: 'center', cellsAlign: 'center', },
                    { text: '凭证日期', datafield: 'VoucherDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '凭证类型', datafield: 'VoucherType', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '财务主管', datafield: 'FinanceDirector', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '记账', datafield: 'Bookkeeping', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '审核', datafield: 'Auditor', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '制单', datafield: 'DocumentMaker', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '交易流水号', datafield: 'Batch', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '出纳', datafield: 'Cashier', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });
        selector.$grid2().on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row index.
            //window.location.href = "/VoucherManageManagement/VoucherListDetail/Index?VGUID=" + row.VGUID;
        });
    }
    function initTable3() {
        var DateEnd = $("#TransactionDateE").val();
        var DateStar = $("#TransactionDateS").val();
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'CompanyCode', type: 'string' },
                    { name: 'CompanyName', type: 'string' },
                    { name: 'AccountingPeriod', type: 'date' },
                    { name: 'Currency', type: 'string' },
                    { name: 'BatchName', type: 'string' },
                    { name: 'VoucherNo', type: 'string' },
                    { name: 'VoucherDate', type: 'date' },
                    { name: 'VoucherType', type: 'string' },
                    { name: 'DebitAmountTotal', type: 'number' },
                    { name: 'CreditAmountTotal', type: 'number' },
                    { name: 'FinanceDirector', type: 'string' },
                    { name: 'Bookkeeping', type: 'string' },
                    { name: 'Auditor', type: 'string' },
                    { name: 'DocumentMaker', type: 'string' },
                    { name: 'Cashier', type: 'string' },
                    { name: 'Attachment1', type: 'string' },
                    { name: 'Attachment2', type: 'string' },
                    { name: 'Attachment3', type: 'string' },
                    { name: 'Attachment4', type: 'string' },
                    { name: 'Attachment5', type: 'string' },
                    { name: 'VGUID', type: 'string' },
                    { name: 'Status', type: 'string' },
                    { name: 'TradingBank', type: 'string' },
                    { name: 'ReceivingUnit', type: 'string' },
                    { name: 'TransactionDate', type: 'date' },
                    { name: 'Batch', type: 'string' },
                    { name: 'OracleStatus', type: 'string' },
                    { name: 'OracleMessage', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: {
                    "Status": status,
                    "Automatic": "4",
                    "VoucherType": type,
                    "ReceivingUnit": $("#ReceivingUnit").val(),
                    "TradingBank": $('#TradingBank option:selected').text(),
                    "TransactionDate": DateStar
                },
                url: "/VoucherManageManagement/VoucherList/GetVoucherListDatas?dateEnd=" + DateEnd   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid3().jqxDataTable(
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
                    { text: "", datafield: "checkbox", width: 35, hidden: true,pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                   { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                    { text: '批名', datafield: 'BatchName', width: 250, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '交易银行', datafield: 'TradingBank', width: 120, pinned: true, align: 'center', cellsAlign: 'center' },
                    { text: '交易日期', datafield: 'TransactionDate', width: 120, pinned: true, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '对方公司', datafield: 'ReceivingUnit', width: 380, align: 'center', cellsAlign: 'center', },
                    { text: '借方金额合计', datafield: 'DebitAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '贷方金额合计', datafield: 'CreditAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '凭证号码', datafield: 'VoucherNo', width: 250, align: 'center', cellsAlign: 'center' },
                    { text: '我方公司', datafield: 'CompanyName', width: 380, align: 'center', cellsAlign: 'center', },
                    { text: '会计期', datafield: 'AccountingPeriod', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM" },
                    { text: '币种', datafield: 'Currency', hidden: true, width: 150, align: 'center', cellsAlign: 'center', },
                    { text: '凭证日期', datafield: 'VoucherDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '凭证类型', datafield: 'VoucherType', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '财务主管', datafield: 'FinanceDirector', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '记账', datafield: 'Bookkeeping', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '审核', datafield: 'Auditor', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '制单', datafield: 'DocumentMaker', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '交易流水号', datafield: 'Batch', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '出纳', datafield: 'Cashier', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '状态', datafield: 'Status', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true },
                ]
            });
        selector.$grid3().on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row index.
            //window.location.href = "/VoucherManageManagement/VoucherListDetail/Index?VGUID=" + row.VGUID;
        });
    }
    if (status == "4") {
        var initWidgets = function (tab) {
            switch (tab) {
                case 0:
                    initTable3();
                    break;
            }
        }
        $('#jqxTabs').jqxTabs({ width: "99%", height: 450, initTabContent: initWidgets });
    } else {
        if (type == null) {
            type = "";
        } else {
            switch (type) {
                case "0": type = "现金类"; break;
                case "1": type = "银行类"; break;
                case "2": type = "转账类"; break;
                default:

            }
        }
        var initWidgets1 = function (tab) {
            switch (tab) {
                case 0:
                    initTable();
                    break;
                case 1:
                    initTable1();
                    break;
                case 2:
                    initTable2();
                    break;
            }
        }
        $('#jqxTabs').jqxTabs({ width: "99%", height: 450, initTabContent: initWidgets1, selectedItem: 1 });
        tableIndex = 1;
    }
   
    function detailFunc(row, column, value, rowData) {
        var container = "";
        var type = "";
        switch (rowData.VoucherType) {
            case "现金类": type = "0"; break;
            case "银行类": type = "1"; break;
            case "转账类": type = "2"; break;
        }
        switch (rowData.Status) {
            case "2": type = "3"; break;
            case "3": type = "4"; break;
        }
        if (selector.$EditPermission().val() == "1") {
            container = "<a href='/VoucherManageManagement/VoucherListDetail/Index?Type=" + type + "&VGUID=" + rowData.uid + "' style=\"text-decoration: underline;color: #333;font-family: Calibri;\">" + rowData.BatchName + "</a>";
        } else {
            container = "<span>" + rowData.BatchName + "</span>";
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
        var grid = "";
        if (tableIndex == 1) {
            grid = $("#jqxTable1");
        } else if (tableIndex == 0) {
            grid = $("#jqxTable");
        } else if (tableIndex == 2) {
            grid = $("#jqxTable2");
        }
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
var voucherList = null;
var modelVGUID = null;
var voucherIndex = 0;
var pageIndex = 1;
function syncAssetsData() {
    //var tableData = $('#jqxSubjectTable').jqxGrid('getboundrows')
    layer.load();
    $.ajax({
        url: "/VoucherManageManagement/VoucherList/SyncAssetsData",
        data: { },
        type: "POST",
        dataType: "json",
        success: function (msg) {
            layer.closeAll('loading');
            if (msg.IsSuccess == true) {
                jqxNotification("同步成功！", null, "success");
                if (tableIndex == 2) {
                    $("#jqxTable2").jqxDataTable('updateBoundData');
                }
            }
            if (msg.Status == "1") {
                jqxNotification("这批数据已经同步！", null, "success");
            }
            if (msg.Status == "2") {
                jqxNotification("暂无差异数据！", null, "success");
            }
        }
    })
}
function checkOracleData() {
    //var tableData = $('#jqxSubjectTable').jqxGrid('getboundrows')
    layer.load();
    $.ajax({
        url: "/VoucherManageManagement/VoucherList/CheckOracleData",
        data: {},
        type: "POST",
        dataType: "json",
        success: function (msg) {
            layer.closeAll('loading');
            if (msg.IsSuccess == true) {
                jqxNotification("校验完成！", null, "success");
                if (tableIndex == 3) {
                    $("#jqxTable3").jqxDataTable('updateBoundData');
                }
            }
            //if (msg.Status == "1") {
            //    jqxNotification("这批数据已经同步！", null, "success");
            //}
            //if (msg.Status == "2") {
            //    jqxNotification("暂无差异数据！", null, "success");
            //}
        }
    })
}
function getBankInfo() {
    $.ajax({
        url: "/CapitalCenterManagement/BankFlowTemplate/GetBankInfo",
        async: false,
        data: {},
        type: "post",
        success: function (result) {
            uiEngineHelper.bindSelect('#TradingBank', result, "BankAccount", "BankName");
            $("#TradingBank").prepend("<option value=\"\" selected='true'></>");
        }
    });
}
function createVoucher(year, month) {
    layer.load();
    $.ajax({
        url: "/VoucherManageManagement/VoucherList/CreateVoucher",
        data: { year: year, month: month },
        type: "POST",
        dataType: "json",
        success: function (msg) {
            layer.closeAll('loading');
            if (msg.IsSuccess == true) {
                jqxNotification("同步完成！", null, "success");
                if (tableIndex == 1) {
                    $("#jqxTable1").jqxDataTable('updateBoundData');
                    selector.$AddBankChannelDialog().modal("hide");
                }
            } else {
                if (msg.Status == "2") {
                    jqxNotification("我方公司借贷配置无数据！", null, "error");
                } else if (msg.Status == "3") {
                    jqxNotification("当前月份结算汇总无数据！", null, "error");
                }
            }
        }
    })
}
function getVoucherModelVGUID() {
    $.ajax({
        url: "/VoucherManageManagement/VoucherList/GetVoucherModelVGUID",
        data: { },
        type: "POST",
        dataType: "json",
        success: function (msg) {
            if (msg.length > 0) {
                modelVGUID = msg;
            } else {
                jqxNotification("当前账套公司无模板数据！", null, "error");
            }
        }
    })
}
function getVoucherModel(year, month, guid) {
    layer.load();
    $.ajax({
        url: "/VoucherManageManagement/VoucherList/GetVoucherModel",
        data: { year: year, month: month, vguid: guid },
        type: "POST",
        dataType: "json",
        success: function (msg) {
            layer.closeAll('loading');
            if (msg.length > 0) {
                voucherList = msg;
                previewVoucher(msg[0]);
            } else {
                jqxNotification("当前模板无借贷数据！", null, "error");
            }
        }
    })
}
function createVoucherModel(year, month) {
    layer.load();
    $.ajax({
        url: "/VoucherManageManagement/VoucherList/CreateVoucherModel",
        data: { year: year, month: month },
        type: "POST",
        dataType: "json",
        success: function (msg) {
            layer.closeAll('loading');
            if (msg.IsSuccess == true) {
                jqxNotification("同步完成！", null, "success");
                if (tableIndex == 1) {
                    $("#jqxTable1").jqxDataTable('updateBoundData');
                    selector.$AddBankChannelDialog().modal("hide");
                }
            } else {
                if (msg.Status == "2") {
                    jqxNotification("当前账套公司无模板数据！", null, "error");
                } else if (msg.Status == "3") {
                    jqxNotification("当前月份无模板数据！", null, "error");
                }
            }
        }
    })
}
function saveVoucherModel() {
    var keyList = [];
    var valueList = [];
    for (var i = 0; i < voucherList[0].VoucherData.length; i++) {
        var borrowMoney = $("#Borrow" + i).val();
        var loanMoney = $("#Loan" + i).val();
        var key = $("#Borrow" + i).attr("name");
        if (borrowMoney > 0) {
            keyList.push(key);
            valueList.push(borrowMoney);
        }
        if (loanMoney > 0) {
            keyList.push(key);
            valueList.push(loanMoney);
        }
    }
    if (keyList.length > 0 && valueList.length > 0) {
        $.ajax({
            url: "/VoucherManageManagement/VoucherList/SaveVoucherModel",
            data: { key: keyList, value: valueList },
            type: "POST",
            dataType: "json",
            success: function (msg) {
            }
        })
    }
}
function previewVoucher(data) {
    $("#SubjectTable").remove();
    $("#lblModelName").text(data.ModelName);
    $("#lblAccountingPeriods").text(data.YearMonth);
    $("#lblCurrency").text("人民币");
    var voucherDate = parseInt(data.VoucherDate.replace(/[^0-9]/ig, ""));//转时间戳
    $("#lblVoucherDate").text($.convert.toDate(new Date(voucherDate), "yyyy-MM-dd"));
    var htmls = "";
    var list1 = "";
    var subjectName = "";
    var voucher = data.VoucherData;
    $("#lblPageIndex").text(pageIndex);
    $("#lblPageCount").text(voucher.length);
    for (var j = 0; j < voucher.length; j++) {
        var borrow = 0;
        var loan = 0;
        if (voucher[j].Borrow != "" && voucher[j].Borrow != null) {
            subjectName = voucher[j].Borrow;
            borrow = voucher[j].Money;
        }
        if (voucher[j].Loan != "" && voucher[j].Loan != null) {
            subjectName = voucher[j].Loan;
            loan = voucher[j].Money;
        }
        var subjectNameList = subjectName.split(".");
        var companyName = subjectNameList[6].split(/[\s\n]/)[1];
        if (subjectNameList[6].split(/[\s\n]/).length < 2) {
            companyName = subjectNameList[6].substring(1, subjectNameList[6].length);
        }
        $("#lblCompany").text(companyName);
        list1 += "<tr style='height:40px'>" +
                      "<td style='text-align: left;'>" + "  " + voucher[j].Remark + "</td>" +
                      "<td style='text-align: left;'>" + "  " + subjectName + "</td>" +
                      "<td style='text-align: right;'><input id='Borrow" + j + "' value='" + borrow + "' name='" + voucher[j].VGUID + "' type='text' style='width: 150px;text-align: right' class='input_text form-control money Borrow'/></td>" +
                      "<td style='text-align: right;'><input id='Loan" + j + "' value='" + loan + "' name='" + voucher[j].VGUID + "' type='text' style='width: 150px;text-align: right' class='input_text form-control money Loan'/></td>" +
                "</tr>";
    }
    htmls = "<table id='SubjectTable' style='width:100%;white-space:pre' border='1' cellspacing='0'>" +
                 "<tr style='height:40px'>" +
                      "<td style='width: 250px;text-align: center;font-size: 18px;'>摘要</td>" +
                      "<td style='text-align: center;font-size: 18px;'>科目及描述</td>" +
                      "<td style='width: 150px;text-align: center;font-size: 18px;'>借方金额</td>" +
                      "<td style='width: 150px;text-align: center;font-size: 18px;'>贷方金额</td>" +
                "</tr>"
                   + list1 +
                "<tr style='height:40px'>" +
                      "<td style='text-align: center;'>合计</td>" +
                      "<td style='text-align: center;'></td>" +
                      "<td style='text-align: right;'><input id='BorrowCount' type='text' style='width: 150px;text-align: right' class='input_text form-control' readonly /></td>" +
                      "<td style='text-align: right;'><input id='LoanCount' type='text' style='width: 150px;text-align: right' class='input_text form-control' readonly/></td>" +
               "</tr>"
    "</table>";
    $("#VoucherDetail").append(htmls);
    $("#ShowDialog").modal({ backdrop: "static", keyboard: false });
    $("#ShowDialog").modal("show");
    tdClick();
}
function tdClick() {
    $(".money").blur(function (event) {
        var id = event.target.id;
        var trIndexs = 0;
        var idNmae = "";
        if (id.length == 8 || id.length == 6) {
            trIndexs = id.substr(id.length - 2, 2);//获取下标
            idNmae = id.substr(0, id.length - 2);
        } else {
            trIndexs = id.substr(id.length - 1, 1);//获取下标
            idNmae = id.substr(0, id.length - 1);
        }
        var value = $("#" + id).val();
        if (value != "" && value != 0) {
            if (idNmae == "Borrow") {
                $("#Loan" + trIndexs).attr("readonly", "readonly");
                value = value.replace(/,/g, '');
                $("#Borrow" + trIndexs).val(parseFloat(value).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,'));
            } else {
                $("#Borrow" + trIndexs).attr("readonly", "readonly");
                value = value.replace(/,/g, '');
                $("#Loan" + trIndexs).val(parseFloat(value).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,'));
            }
        } else {
            if (idNmae == "Borrow") {
                if ($("#Loan" + trIndexs).val() != "") {
                    $("#Borrow" + trIndexs).attr("readonly", "readonly");
                    $("#Loan" + trIndexs).removeAttr("readonly");
                } else {
                    $("#Borrow" + trIndexs).removeAttr("readonly");
                    $("#Loan" + trIndexs).removeAttr("readonly");
                }
            } else {
                if ($("#Borrow" + trIndexs).val() != "") {
                    $("#Loan" + trIndexs).attr("readonly", "readonly");
                    $("#Borrow" + trIndexs).removeAttr("readonly");
                } else {
                    $("#Borrow" + trIndexs).removeAttr("readonly");
                    $("#Loan" + trIndexs).removeAttr("readonly");
                }
            }
        }
        countMoney();
    });
    countMoney();
}
function countMoney() {
    var borrowCount = 0;
    var loanCount = 0;
    for (var i = 0; i < $(".Borrow").length; i++) {
        if ($(".Borrow")[i].value != "") {
            var valB = $(".Borrow")[i].value.replace(/,/g, '');
            borrowCount += parseFloat(valB);
        }
    }
    for (var i = 0; i < $(".Loan").length; i++) {
        if ($(".Loan")[i].value != "") {
            var valL = $(".Loan")[i].value.replace(/,/g, '');
            loanCount += parseFloat(valL);
        }
    }
    $("#BorrowCount").val(parseFloat(borrowCount).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,'));
    $("#LoanCount").val(parseFloat(loanCount).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,'));
}