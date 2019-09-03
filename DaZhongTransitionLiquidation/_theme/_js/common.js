
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

//关闭加载框
function closeLoading() {
    $("#loadingDialog").modal('hide');
}

$(function() {
    $(".input_text").attr("autocomplete", "new-password");
});
