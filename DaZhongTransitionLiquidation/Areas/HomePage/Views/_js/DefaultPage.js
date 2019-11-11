var $page = function () {

    this.init = function () {
        //addEvent();
    };
    var selector = this.selector ={}

    function addEvent() {
        var type = $.request.queryString().Type;
        var html = "";
        switch (type) {
            case "1": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
            case "2": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
            case "3": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
            case "4": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
            case "5": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
            case "6": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
            case "7": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
            case "8": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
            case "9": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
            case "10": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
            case "11": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
            case "12": html = '<div class="bg-icon" style="background: url(/_theme/images/20191101135036.jpg)  center center / cover no-repeat;"></div>'; break;
        }
        $(".mk").append(html);
    }
};

$(function () {
    var page = new $page();
    page.init();
});