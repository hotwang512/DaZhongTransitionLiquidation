
function subString_Str(val, strCount) {
    var result = "";
    if (val != null && val != "") {
        var val_sub = val;
        if (val_sub.length > strCount) {
            val_sub = val.substring(0, strCount) + "...";
        }
        result = "<span title='" + val + "'>" + val_sub + "</span>";

    }
    return result;
}



function clearNoNum(obj) {
    obj.value = obj.value.replace(/[^\d.]/g, ""); //清除"数字"和"."以外的字符
    obj.value = obj.value.replace(/^\./g, ""); //验证第一个字符是数字而不是
    obj.value = obj.value.replace(/\.{2,}/g, "."); //只保留第一个. 清除多余的
    obj.value = obj.value.replace(".", "$#$").replace(/\./g, "").replace("$#$", ".");
    obj.value = obj.value.replace(/^(\-)*(\d+)\.(\d\d).*$/, '$1$2.$3'); //只能输入两个小数
}



function WindowConfirmDialog(okFun, msg, title, okBtnText, cancleBtnText, parameter,status) {

    $("#confirmWindow_OKBtn").unbind("click"); //移除click

    $("#confirmWindow_title").text(title);
    $("#confirmWindow_msg").text(msg);
    $("#confirmWindow_OKBtn").text(okBtnText);
    $("#confirmWindow_CancelBtn").text(cancleBtnText);

    $("#confirmWindowDialog").modal({ backdrop: 'static', keyboard: false });
    $("#confirmWindowDialog").modal('show');

    $("#confirmWindow_OKBtn").on('click', function () {
        if (okFun != null) {
            okFun(parameter, status);
            $("#confirmWindowDialog").modal('hide');
        }
    });

    $("#confirmWindow_CancelBtn").on('click', function () {
        $("#confirmWindowDialog").modal('hide');
    });
}

//显示加载框
function showLoading() {
    $("#loadingDialog").modal({ backdrop: 'static', keyboard: false });
    $("#loadingDialog").modal('show');

}

//html导出Excel
var tableToExcel = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,'
        , template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]-->' +
    ' <style type="text/css">' +
    '.excelTable  {' +
    'border-collapse:collapse;' +
    ' border:thin solid #999; ' +
    '}' +
    '   .excelTable  th {' +
    '   border: thin solid #999;' +
    '  padding:20px;' +
    '  text-align: center;' +
    '  border-top: thin solid #999;' +
    ' ' +
    '  }' +
    ' .excelTable  td{' +
    ' border:thin solid #999;' +
    ' padding:2px 5px;' +
    ' text-align: center;' +
    ' }</style>' + '</head><body><table border="1">{table}</table></body></html>'
        , base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))) }
        , format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) }
    return function (table, name) {
        if (!table.nodeType) table = $("." + table)[0]
        var ctx = { worksheet: name || 'Worksheet', table: table.innerHTML };
        var downloadLink = document.createElement("a");
        downloadLink.href = uri + base64(format(template, ctx));
        downloadLink.download = name + ".xls";
        document.body.appendChild(downloadLink);
        downloadLink.click();
        document.body.removeChild(downloadLink);
    }
})();
//月份加减
function addMonth(date, num) {
    num = parseInt(num);
    var sDate = dateToDate(date);

    var sYear = sDate.getFullYear();
    var sMonth = sDate.getMonth() + 1;
    var sDay = sDate.getDate();

    var eYear = sYear;
    var eMonth = sMonth + num;
    var eDay = sDay;
    while (eMonth > 12) {
        eYear++;
        eMonth -= 12;
    }

    var eDate = new Date(eYear, eMonth - 1, eDay);

    while (eDate.getMonth() != eMonth - 1) {
        eDay--;
        eDate = new Date(eYear, eMonth - 1, eDay);
    }

    return eDate;
};
function dateToDate(date) {
    var sDate = new Date();
    if (typeof date == 'object'
      && typeof new Date().getMonth == "function"
      ) {
        sDate = date;
    }
    else if (typeof date == "string") {
        var arr = date.split('-')
        if (arr.length == 3) {
            sDate = new Date(arr[0] + '-' + arr[1] + '-' + arr[2]);
        }
    }
    return sDate;
};
//关闭加载框
function closeLoading() {
    $("#loadingDialog").modal('hide');
}

$(function() {
    $(".input_text").attr("autocomplete", "new-password");
});
