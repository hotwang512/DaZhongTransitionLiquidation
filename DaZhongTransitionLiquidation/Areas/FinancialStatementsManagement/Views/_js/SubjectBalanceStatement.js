//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $txtBankAccountName_Dialog: function () { return $("#txtBankAccountName_Dialog") },
    $txtBankAccount_Dialog: function () { return $("#txtBankAccount_Dialog") },
    $txtBank_Dialog: function () { return $("#txtBank_Dialog") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end

var myDate = new Date();//获取系统当前时间
var isEdit = false;
var vguid = "";
var companyCode = $("#CompanyCode").val();
var accountModeCode = $("#AccountModeCode").val();
var accountModeName = $("#AccountModeCode  option:selected").text();
var month = myDate.getMonth() + 1;
var year = $("#Year").val();
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        getCompanyCode();
        $("#Month").val(month);

        //切换公司
        $('#CompanyCode').on('change', function (event) {
            companyCode = $("#CompanyCode").val();
            console.log(companyCode);
            initTable();
        })
        //切换账套
        $('#AccountModeCode').on('change', function (event) {
            accountModeCode = $("#AccountModeCode").val();
            console.log(accountModeCode);
            initTable();
        })
        //切换年份
        $('#Year').on('change', function (event) {
            year = $("#Year").val();
            initTable();
        })
        //切换月份
        $('#Month').on('change', function (event) {
            month = $("#Month").val();
            initTable();
        })
        $('#btnCheck').on("click", function () {
            //验证当前账期下是否存在待审核凭证
            var isAnyVoucher = checkVoucher();
            if (isAnyVoucher) {
                jqxNotification("当前账期下存在待审核凭证！", null, "error");
            } else {
                var check = "T";
                initTable(check);
            } 
        })
        $("#AddSubject_CancelBtn").on("click", function () {
            $("#AddSubjectTable").modal("hide");
        });
    }; //addEvent end
};


function initTable(check) {
    var source =
        {
            datafields:
            [
                { name: 'BusinessCode', type: 'string' },
                { name: 'Company', type: 'string' },
                { name: 'Balance', type: 'number' },//期初余额
                { name: 'ENTERED_DR', type: 'number' },//本期借方
                { name: 'ENTERED_CR', type: 'number' },//本期贷方
                { name: 'END_BALANCE', type: 'number' },//期末余额
                { name: 'Checked', type: 'string' }
            ],
            datatype: "json",
            //cache: false,
            async: false,
            //id: "VGUID",
            data: { companyCode: companyCode, accountModeCode: accountModeCode, accountModeName: accountModeName, month: month,year:year, check: check },
            url: "/FinancialStatementsManagement/SubjectBalanceStatement/GetSubjectBalance"    //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    //创建卡信息列表（主表）
    $("#jqxTable").jqxDataTable({
        pageable: true,
        width: "100%",
        height: 450,
        pageSize: 15,
        serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        columnsHeight: 40,
        columns: [
            //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
            { text: '编码', datafield: 'BusinessCode', width: 200, align: 'center', cellsAlign: 'center', },
            { text: '组合', datafield: 'Company', width: 450, align: 'center', cellsAlign: 'center', },
            { text: '期初余额', datafield: 'Balance', cellsFormat: "d2", align: 'center', cellsAlign: 'center' },
            { text: '本期借方', datafield: 'ENTERED_DR', cellsFormat: "d2", align: 'center', cellsAlign: 'center' },
            { text: '本期贷方', datafield: 'ENTERED_CR', cellsFormat: "d2", align: 'center', cellsAlign: 'center' },
            { text: '期末余额', datafield: 'END_BALANCE', cellsFormat: "d2", align: 'center', cellsAlign: 'center' },
            {
                text: '校验状态', datafield: 'Checked', width: 250, align: 'center', cellsAlign: 'center', cellsrenderer: function (row, column, value, rowData) {
                    if (value == "Y") {
                        return "<div style='margin:4px;text-align: center'>校验成功</div>";
                    }
                    else if (value == "N") {
                        //return "<div style='margin:4px;text-align: center'>校验失败</div>";
                        return "<a href='#' onclick=find('" + rowData.BusinessCode + "') style=\"text-decoration: underline;color: #f00;\">校验失败</a>";
                    }
                    else {
                        return "";
                    }
                }
            }
        ]
    });
}

$(function () {
    var page = new $page();
    page.init();
});

function getCompanyCode() {
    var accountMode = $("#AccountModeCode").val();
    $.ajax({
        url: "/HomePage/CompanyHomePage/GetCompanyCode",
        data: { accountMode: accountMode },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            uiEngineHelper.bindSelect('#CompanyCode', msg, "CompanyCode", "CompanyName");
            //$("#CompanyCode").prepend("<option value=\"\" selected='true'>请选择</>");
        }
    });
    companyCode = $("#CompanyCode").val();
    initTable();
}
function find(businessCode) {
    initSubjectTable(businessCode);
    $("#AddSubjectTable").modal({ backdrop: "static", keyboard: false });
    $("#AddSubjectTable").modal("show");
}
function initSubjectTable(businessCode) {
    var source =
        {
            datafields:
            [
                { name: 'SubjectCount', type: 'string' },
                { name: 'LEDGER_NAME', type: 'string' },
                { name: 'JE_BATCH_NAME', type: 'string' },
                { name: 'JE_HEADER_NAME', type: 'string' },
                { name: 'JE_CATEGORY_NAME', type: 'string' },
                { name: 'ACCOUNTING_DATE', type: 'date' },
                { name: 'ENTERED_DR', type: 'number' },//本期借方
                { name: 'ENTERED_CR', type: 'number' },//本期贷方
                { name: 'STATUS', type: 'string' },
                { name: 'MESSAGE', type: 'string' }
            ],
            datatype: "json",
            //cache: false,
            //async: false,
            data: { businessCode: businessCode, companyCode: companyCode, accountModeCode: accountModeCode, accountModeName: accountModeName, month: month, year:year},
            url: "/FinancialStatementsManagement/SubjectBalanceStatement/CheckSubjectBalance"    //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    //创建卡信息列表（主表）
    $("#jqxSubjectTable").jqxGrid({
        pageable: true,
        width: "100%",
        height: 400,
        pageSize: 15,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        theme: "office",
        columnsHeight: 40,
        pagermode: 'simple',
        columns: [
            //{ text: "", datafield: "checkbox", width: 35, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
            { text: '编码', datafield: 'SubjectCount', width: 180, align: 'center', cellsAlign: 'center', hidden: true },
            { text: '账簿', datafield: 'LEDGER_NAME', width: 180, align: 'center', cellsAlign: 'center', },
            { text: '批名', datafield: 'JE_BATCH_NAME', width: 450, align: 'center', cellsAlign: 'center', },
            { text: '凭证号', datafield: 'JE_HEADER_NAME', width: 250, align: 'center', cellsAlign: 'center', },
            { text: '记账日期', datafield: 'ACCOUNTING_DATE', width: 120, align: 'center', cellsAlign: 'center', cellsformat: "yyyy-MM-dd" },
            { text: '本期借方', datafield: 'ENTERED_DR', width: 120, cellsFormat: "d2", align: 'center', cellsAlign: 'center' },
            { text: '本期贷方', datafield: 'ENTERED_CR', cellsFormat: "d2", align: 'center', cellsAlign: 'center' },
            { text: '状态', datafield: 'STATUS', width: 50, align: 'center', cellsAlign: 'center',hidden:true },
            { text: '描述', datafield: 'MESSAGE', align: 'center', cellsAlign: 'center', hidden: true },
            { text: '', datafield: 'JE_CATEGORY_NAME', align: 'center', cellsAlign: 'center', hidden: true }
        ]
    });
}
function checkVoucher() {
    var isAny = false;
    $.ajax({
        url: "/FinancialStatementsManagement/SubjectBalanceStatement/CheckVoucher",
        data: { companyCode: companyCode, accountModeCode: accountModeCode, accountModeName: accountModeName, month: month, year: year },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            if (msg.Status == "1") {
                isAny = true;
            }
        }
    });
    return isAny;
}