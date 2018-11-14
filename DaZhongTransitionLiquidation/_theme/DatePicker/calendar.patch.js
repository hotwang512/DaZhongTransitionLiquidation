/*
Robin
2014-09-23
My97Date 在IE弹出层中无法下拉选择年月的补丁
*/
var WdatePickerPatch = function () {
    if ($(".modal-backdrop").length != 0) {
        $(".yminput").click(function () {
            var popLay = $(this).prev();
            if (popLay.hasClass("MMenu")) {
                popLay.show();
            } else if (popLay.hasClass("YMenu")) {
                popLay.show();
            }
        })
    }
}