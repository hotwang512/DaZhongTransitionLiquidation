; (function ($, window, document, undefined) {
    $.leftNav = function (where, data) {
        var where = where;
        var menu = "";
        var html = "";
        var defaultmenu = {};
        var here = $("body").find(where);
        if (!here.length) {
            where = "body";
        }
        if (data) {
            data.level = 0;
            menu = data;
        } else {
            menu = defaultmenu;
        }
        $("#homeModel").text("--" + data.Name);
        $(where).append('<div class="nav_box"><ul class="nav"></ul></div>');
        sidebarNav(menu);
        function sidebarNav(data, level) {
            if (data.Type == 2)
            { return; }
            html += '<li class="left_nav_list">';
            var eleclass = "left_nav_name_folder";
            if (data.Type != 0) {
                var eleclass = "left_nav_name_item";
            }
            html += '<div class="' + eleclass + ' level' + data.level;
            if (data.IsOpen == true) {
                html += ' nav_open';
            }
            html += '"><em>' + data.Name + '</em></div>';
            if (data.Type == 0) {
                html += '<ul class="nav_ul"';
                if (data.IsOpen == true) {
                    html += ' style="display:block;"';
                }
                html += '>';
                for (var i = 0, l = data.ChildModuleMenu.length; i < l; i++) {
                    var children = data.ChildModuleMenu[i];
                    children.level = data.level + 1;
                    if (children.Type == 0) {
                        sidebarNav(children, children.level);
                    } else {
                        var active = "";
                        if (children.IsActive) {
                            active = "active";
                        }
                        html += '<li class="nav_li level' + children.level + " " + active;
                        if (children.IsOpen) {
                            html += ' nav_li_open';
                        }
                        var url = "";
                        if (children.Url != null && children.Url != undefined && children.Url != "") {
                            url = children.Url.split(',')[0];
                        }
                        html += '" data-url="' + url + '">';
                        html += '<em>' + children.Name + '</em>';
                        html += '</li>';
                    }
                }
                html += '</ul>';
            }
            html += '</li>';
        }
        $(where + " .nav").html(html);
        $(where + " .left_nav_name_folder").on("click", function () {
            var open = $(this).hasClass("nav_open");
            if (open) {
                $(this).removeClass("nav_open");
                $(this).next(".nav_ul").slideUp();
            } else {
                $(this).addClass("nav_open");
                $(this).next(".nav_ul").slideDown();
                $(this).parents(".left_nav_list").siblings("li").find(".nav_ul").slideUp();
                $(this).parents(".left_nav_list").siblings("li").find(".left_nav_name_folder").removeClass("nav_open");
            }

        });
        $(where + " .nav_li").on("click", function () {
            $(where + " .nav_li").removeClass("nav_li_open");
            $(this).addClass("nav_li_open");
            $(this).siblings(".left_nav_list").find(".nav_ul").slideUp();
            $(this).siblings(".left_nav_list").find(".left_nav_name").removeClass("nav_open");
            var url = $(this).attr("data-url");
            if (url != null && url != undefined && url != "") {
                location.href = url;
            }
        })
    }
})(jQuery, window, document);