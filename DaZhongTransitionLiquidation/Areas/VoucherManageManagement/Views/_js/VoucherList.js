//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $grid1: function () { return $("#jqxTable1") },
    $grid2: function () { return $("#jqxTable2") },
    $grid3: function () { return $("#jqxTable3") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var type = "";
var tableIndex = 0;
var status = $.request.queryString().Status;
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        getBankInfo();
        if (status == "1") {
            $("#buttonList").show();
        }
        if (status == "2") {
            $("#buttonList2").show();
            $("#Oracle").show();
            $("#OracleCheck").hide();
            $("#btnCheckStatus").hide();
            //$("#AddSubject_OKButton").show();
        }
        if (status == "3") {
            $("#Oracle").show();
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
            $("#AccountingPeriod").val("");
            //$("#TransactionDate").val("");
            //$("#TransactionDateEnd").val("");
            //$("#PaymentUnit").val("");
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
    }; //addEvent end

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
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
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
                ],
                datatype: "json",
                id: "VGUID",
                type: "post",
                data: { "Status": status, "AccountingPeriod": $("#AccountingPeriod").val(), "Automatic": "0", "VoucherType": type, "TradingBank": $('#TradingBank option:selected').text(), "TransactionDate": $("#TransactionDate").val() },
                url: "/VoucherManageManagement/VoucherList/GetVoucherListDatas"   //获取数据源的路径
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
                    { text: "", datafield: "checkbox", width: 35, pinned: false, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                    { text: '批名', datafield: 'BatchName', width: 150, pinned: false, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '凭证号码', datafield: 'VoucherNo', width: 150, pinned: false, align: 'center', cellsAlign: 'center' },
                    { text: '公司', datafield: 'CompanyName', width: 350, pinned: false, align: 'center', cellsAlign: 'center', },
                    { text: '会计期', datafield: 'AccountingPeriod', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM" },
                    { text: '币种', datafield: 'Currency',hidden: true, align: 'center', cellsAlign: 'center', },
                    { text: '凭证日期', datafield: 'VoucherDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '凭证类型', datafield: 'VoucherType', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '借方金额合计', datafield: 'DebitAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '贷方金额合计', datafield: 'CreditAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '财务主管', datafield: 'FinanceDirector', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '记账', datafield: 'Bookkeeping', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '审核', datafield: 'Auditor', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '制单', datafield: 'DocumentMaker',  align: 'center', cellsAlign: 'center' },
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
            window.location.href = "/VoucherManageManagement/VoucherListDetail/Index?VGUID=" + row.VGUID;
        });
    }
    function initTable1() {
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
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
                data: { "Status": status, "ReceivingUnit": $("#ReceivingUnit").val(), "Automatic": "1", "VoucherType": type, "TradingBank": $('#TradingBank option:selected').text(), "TransactionDate": $("#TransactionDate").val() },
                url: "/VoucherManageManagement/VoucherList/GetVoucherListDatas"   //获取数据源的路径
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
                    { text: '批名', datafield: 'BatchName', width: 150, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '交易银行', datafield: 'TradingBank', width: 120, pinned: true, align: 'center', cellsAlign: 'center' },
                    { text: '交易日期', datafield: 'TransactionDate', width: 120, pinned: true, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '对方公司', datafield: 'ReceivingUnit', width: 350, align: 'center', cellsAlign: 'center', },
                    { text: '借方金额合计', datafield: 'DebitAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '贷方金额合计', datafield: 'CreditAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '凭证号码', datafield: 'VoucherNo', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '我方公司', datafield: 'CompanyName', width: 350, align: 'center', cellsAlign: 'center', },
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
            window.location.href = "/VoucherManageManagement/VoucherListDetail/Index?VGUID=" + row.VGUID;
        });
    }
    function initTable2() {
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
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
                ],
                datatype: "json",
                id: "VGUID",
                data: { "Status": status, "AccountingPeriod": $("#AccountingPeriod").val(), "Automatic": "3", "VoucherType": type },
                url: "/VoucherManageManagement/VoucherList/GetVoucherListDatas"   //获取数据源的路径
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
                    { text: "", datafield: "checkbox", width: 35, pinned: false, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'CompanyCode', datafield: 'CompanyCode', hidden: true },
                    { text: '批名', datafield: 'BatchName', width: 150, pinned: false, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '凭证号码', datafield: 'VoucherNo', width: 150, pinned: false, align: 'center', cellsAlign: 'center' },
                    { text: '营运公司', datafield: 'CompanyName', width: 350, pinned: false, align: 'center', cellsAlign: 'center', },
                    { text: '会计期', datafield: 'AccountingPeriod', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM" },
                    { text: '币种', datafield: 'Currency', hidden: true, width: 150, align: 'center', cellsAlign: 'center', },
                    { text: '凭证日期', datafield: 'VoucherDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '凭证类型', datafield: 'VoucherType', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '借方金额合计', datafield: 'DebitAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '贷方金额合计', datafield: 'CreditAmountTotal', cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '财务主管', datafield: 'FinanceDirector', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '记账', datafield: 'Bookkeeping', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '审核', datafield: 'Auditor', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '制单', datafield: 'DocumentMaker', align: 'center', cellsAlign: 'center' },
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
            window.location.href = "/VoucherManageManagement/VoucherListDetail/Index?VGUID=" + row.VGUID;
        });
    }
    function initTable3() {
        //var DateEnd = $("#TransactionDateEnd").val();  "AccountingPeriod": $("#AccountingPeriod").val("")
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
                    { name: 'OracleStatus', type: 'string' },
                    { name: 'OracleMessage', type: 'string' },
                ],
                datatype: "json",
                id: "VGUID",
                data: { "Status": status, "AccountingPeriod": $("#AccountingPeriod").val(), "Automatic": "4", "VoucherType": type },
                url: "/VoucherManageManagement/VoucherList/GetVoucherListDatas"   //获取数据源的路径
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
                    { text: '批名', datafield: 'BatchName', width: 150, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: detailFunc },
                    { text: '凭证号码', datafield: 'VoucherNo', width: 150, pinned: true, align: 'center', cellsAlign: 'center' },
                    { text: '营运公司', datafield: 'CompanyName', width: 380, pinned: false, align: 'center', cellsAlign: 'center', },
                    { text: '会计期', datafield: 'AccountingPeriod', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM" },
                    { text: '币种', datafield: 'Currency', hidden: true, width: 150, align: 'center', cellsAlign: 'center', },
                    { text: '凭证日期', datafield: 'VoucherDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '凭证类型', datafield: 'VoucherType', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '借方金额合计', datafield: 'DebitAmountTotal', width: 120, cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '贷方金额合计', datafield: 'CreditAmountTotal', width: 120, cellsFormat: "d2", width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '财务主管', datafield: 'FinanceDirector', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '记账', datafield: 'Bookkeeping', width: 150, align: 'center', cellsAlign: 'center' },
                    //{ text: '审核', datafield: 'Auditor', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '制单', datafield: 'DocumentMaker', width: 120, align: 'center', cellsAlign: 'center' },
                    { text: 'Oracle返回状态', datafield: 'OracleStatus', width: 120, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: 'Oracle返回错误', datafield: 'OracleMessage', width: 200, align: 'center', cellsAlign: 'center' },
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
            window.location.href = "/VoucherManageManagement/VoucherListDetail/Index?VGUID=" + row.VGUID;
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
        type = $.request.queryString().Type;
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
        if (selector.$EditPermission().val() == "1") {
            container = "<a href='/VoucherManageManagement/VoucherListDetail/Index?VGUID="+rowData.uid+"' style=\"text-decoration: underline;color: #333;\">" + rowData.BatchName + "</a>";
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