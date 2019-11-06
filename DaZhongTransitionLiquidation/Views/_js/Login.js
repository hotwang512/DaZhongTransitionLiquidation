var selector = {
    //表单元素
    $txtName: function () { return $("#txtUserAccount") },
    $txtPwd: function () { return $("#txtUserPwd") },
    $bgiconline: function () { return $(".bg-icon-line"); },
    $bgicon: function () { return $(".bg-icon") },
    $loginValidate: function () { return $("#loginValidate"); },
    $loginValidateMessageText: function () { return $("#loginValidateMessageText"); },
    //按钮
    $btnLogin: function () { return $("#btnLogin") }
};
var iCount = undefined;

var $page = function () {

    this.init = function () {
        SetDate();
        bgCarousel(); 
        addEvent();
        selector.$txtName().focus(); //页面加载让用户名文本框获得焦点
    }
    function bgCarousel() {
        iCount = setInterval("bgCarouselShow()", 5000);
    }
    function SetDate() {
        var date = new Date();
        var month = date.getMonth() + 1;
        if (month < 10) {
            month = "0" + month;
        }
        var day = date.getDate();
        if (day < 10) {
            day = "0" + day;
        }
        var year = date.getFullYear();
        $("#lbDateDay").text(day);
        $("#lbYearMonth").text(year + "/" + month);
    }
    function addEvent() {
        selector.$bgiconline().on("click", function () {
            clearInterval(iCount);
            var element = $(this);
            var imgUrl = element.attr("bgurl");
            selector.$bgiconline().removeClass("active");
            element.addClass("active");
            selector.$bgicon().css("background", "url(" + imgUrl + ")");
            selector.$bgicon().css("background-size", "cover");
            selector.$bgicon().css("background-repeat", "no-repeat");
            selector.$bgicon().css("background-position", "center center");
            selector.$bgicon().css("background-color", "#031529");
            iCount = setInterval("bgCarouselShow()", 5000);
        })
        //点击登录按钮
        selector.$btnLogin().on("click", function () {
            selector.$loginValidate().hide(200);
            var reg = /^[0-9a-zA-Z@@]+$/;
            var userName = selector.$txtName().val();
            var pwd = selector.$txtPwd().val();
            if (userName.length == 0) {
                selector.$loginValidateMessageText().text("请输入用户名！");
                selector.$loginValidate().show(200);
                //jqxNotification("请输入用户名！", null, "error");
                selector.$txtName().focus();
            }
            else if (pwd.length === 0) {
                selector.$loginValidateMessageText().text("请输入密码！");
                selector.$loginValidate().show(200);
                selector.$txtPwd().focus();
            } else {
                $.ajax({
                    url: "/Login/ProcessLogin",
                    data: { LoginName: userName, Password: pwd },
                    type: "POST",
                    dataType: "json",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                selector.$loginValidateMessageText().text(msg.ResultInfo);
                                selector.$loginValidate().show(200);
                                //jqxNotification(msg.ResultInfo, null, "error");
                                break;
                            case "1":
                                window.location.href = "/HomePage/CompanyHomePage/Index";
                                //window.location.href = "/HomePage/HomePage/Index";
                                break;
                        }
                    }
                });
            }

        });

        //按回车键也可登录
        $(document).keyup(function (event) {
            if (event.keyCode === 13) {
                selector.$btnLogin().click();
            }
        });

    }//addEvent end



};

function bgCarouselShow() {
    var bg_icon_line = selector.$bgiconline();
    for (var i = 0; i < bg_icon_line.length; i++) {
        var element = $(bg_icon_line[i]);
        if (element.attr("class").search("active") != -1) {
            if (i == 0) {
                $(bg_icon_line[1]).click();
                break;
            }
            if (i == 1) {
                $(bg_icon_line[2]).click();
                break;
            }
            if (i == 2) {
                $(bg_icon_line[0]).click();
                break;
            }
        }
    }
}

$(function () {
    var page = new $page();
    page.init();
});