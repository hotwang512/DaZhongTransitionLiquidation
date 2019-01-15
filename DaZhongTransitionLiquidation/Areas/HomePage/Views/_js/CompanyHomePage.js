var selector = {

}
var $page = function () {
    this.init = function () {
        $(function () {
            $("#OKButton").on("click", function () {
                $.ajax({
                    url: "/HomePage/CompanyHomePage/SaveUserInfo",
                    data: { ComapnyCode: $("#CompanyCode").val() },
                    type: "POST",
                    dataType: "json",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败", null, "error");
                                break;
                            case "1":
                                window.location.href = "/HomePage/HomePage/Index";
                                break;
                        }
                    }
                });
                
            })
        })
    }
}

$(function () {
    var page = new $page();
    page.init();
})