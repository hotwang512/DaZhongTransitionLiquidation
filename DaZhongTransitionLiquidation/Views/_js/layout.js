﻿$("#body").height($(window).height() - 60);


//修改密码
$("#aChangePwd").on("click", function () {
    $("#txtOldPwd").val("");
    $("#txtNewPwd").val("");
    $("#txtNewPwdAgain").val("");
    $("#changePwdDialog").modal({ backdrop: "static", keyboard: false });
    $("#changePwdDialog").modal("show");
});

for (var i = 0; i < $(".accountMode").length; i++) {
    $(".accountMode").eq(i).on("click", { key: i }, function (event) {
        var code = $(".accountMode")[event.data.key].getAttribute("vguid");
        WindowConfirmDialog(changeAccountMode, "是否切换账套？", "确认框", "确定", "取消", code);
    });
};

function changeAccountMode(accountModeCode) {
    $.ajax({
        url: "/HomePage/CompanyHomePage/SaveUserInfoChange",
        data: {
            AccountModeCode: accountModeCode,
        },
        type: "POST",
        dataType: "json",
        success: function (msg) {
            switch (msg.Status) {
                case "0":
                    jqxNotification("切换失败", null, "error");
                    break;
                case "1":
                    jqxNotification("切换成功", null, "success");
                    window.location.reload();
                    break;
            }
        }
    });
}

//点击弹出框中的取消按钮
$("#changePwd_CancelBtn").on("click", function () {
    $("#changePwdDialog").modal("hide");
});
//点击弹出框中的保存按钮
$("#changePwd_OKButton").on("click", function () {
    var validateError = 0;
    if (!Validate($("#txtOldPwd"))) {
        validateError++;
    }
    if (!Validate($("#txtNewPwd"))) {
        validateError++;
    }
    if (!Validate($("#txtNewPwdAgain"))) {
        validateError++;
    }
    if (validateError == 0) {
        if ($("#txtNewPwd").val() == $("#txtOldPwd").val()) {
            jqxNotification("新密码与旧密码不能一致！", null, "error");
            return;
        }
        if ($("#txtNewPwd").val() != $("#txtNewPwdAgain").val()) {
            jqxNotification("两次输入的密码不一致！", null, "error");
        } else {
            $.ajax({
                url: "/SystemManagement/UserManagement/ChangePassword",
                data: {
                    oldPassword: $("#txtOldPwd").val(),
                    Password: $("#txtNewPwdAgain").val(),
                    Vguid: $("#aChangePwd").attr("vguid")
                },
                type: "post",
                async:false,
                dataType: "json",
                success: function (msg) {
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("修改失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("修改成功！", null, "success");
                            $("#changePwdDialog").modal("hide");
                            break;
                        case "2":
                            jqxNotification("旧密码不正确！", null, "error");
                            break;
                    }
                }
            });
        }
    }
});

//退出系统
$("#aLogout").on("click", function () {
    $.ajax({
        url: "/Login/Logout",
        type: "post",
        async: false,
        success: function () {
            window.location.href = "/Login/Index";
        }
    });
});


$("#shrinkMenue").on("click", function () {
    var menueDisplay = $(".left_content").css("display");
    var width = $(document).width() - 40;
    $(".left_content").toggle(200);
    if (menueDisplay == "block") {
        $(".jqxTable").jqxDataTable({ width: width });
    }
    else {
        width = width - 210;
        $(".jqxTable").jqxDataTable({ width: width });
    }

});


$(".menue_header").on('click', function () {
    var displayVal = $(this).next(".menue_body").css("display");
    switch (displayVal) {
        case "block":
            $(this).next(".menue_body").hide(300);
            break;
        case "none":
            $(this).next(".menue_body").show(300);
            $(".menue_body").not($(this).next(".menue_body")).hide(300);
            break;
    }
});
//$(".menue_body").on('click', function () {
//    var displayVal = $(this).find('.menue_bodychild').css("display");
//    switch (displayVal) {
//        case "block":
//            $(this).find('.menue_bodychild').hide(300);
//            break;
//        case "none":
//            $(this).find('.menue_bodychild').show(300);
//            $(".menue_bodychild").not($(this).find('.menue_bodychild')).hide(300);
//            break;
//    }
//});
$(".bodyTable").on('click', function () {
    var displayVal = $(this).next(".menue_bodychild").css("display");
    switch (displayVal) {
        case "block":
            $(this).next(".menue_bodychild").hide(300);
            break;
        case "none":
            $(this).next(".menue_bodychild").show(300);
            $(".menue_bodychild").not($(this).next(".menue_bodychild")).hide(300);
            break;
    }
});

$(".BaseData").on('click', function () {
    var displayVal = $(this).next(".menue_bodychildchild").css("display");
    switch (displayVal) {
        case "block":
            $(this).next(".menue_bodychildchild").hide(300);
            break;
        case "none":
            $(this).next(".menue_bodychildchild").show(300);
            $(".menue_bodychildchild").not($(this).next(".menue_bodychildchild")).hide(300);
            break;
    }
});

$(".menue_item").on('click', function () {
    CookieHelper.SaveCookie(this.id);
    var url = $(this).attr("pageurl");
    window.location.href = url;
});

$("#gotoHome").on('click', function () {
    window.location.href = "/HomePage/HomePage/Index";;
});
$("#homeModel").on('click', function () {
    window.location.href = "/HomePage/HomePage/Index";;
});

