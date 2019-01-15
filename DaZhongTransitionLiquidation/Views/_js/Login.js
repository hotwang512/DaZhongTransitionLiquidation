var selector = {
    //表单元素
    $txtName: function () { return $("#txtName") },
    $txtPwd: function () { return $("#txtPwd") },

    //按钮
    $btnLogin: function () { return $("#btnLogin") }
};


var $page = function () {

    this.init = function () {
        addEvent();
        selector.$txtName().focus(); //页面加载让用户名文本框获得焦点
    }

    function addEvent() {

        //点击登录按钮
        selector.$btnLogin().on("click", function () {
            var reg = /^[0-9a-zA-Z@@]+$/;
            var userName = selector.$txtName().val();
            var pwd = selector.$txtPwd().val();
            if (userName.length == 0) {
                jqxNotification("请输入用户名！", null, "error");
                selector.$txtName().focus();
            } else if (!reg.test(userName)) {
                jqxNotification("无效的用户名！", null, "error");
                selector.$txtName().val("");
                selector.$txtName().focus();
            }
            else if (pwd.length === 0) {
                jqxNotification("请输入密码！", null, "error");
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
                                jqxNotification(msg.ResultInfo, null, "error");
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



$(function () {
    var page = new $page();
    page.init();
});