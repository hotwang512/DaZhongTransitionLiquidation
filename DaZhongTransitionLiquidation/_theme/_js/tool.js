//////////////////////////////////////////////////通用函数只针对本系统//////////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////工具代码(可分离到其他系统)//////////////////////////////////////////////////////

//防止回车跳转
function NoSubmit(event) {
    return (ev.keyCode != 13);
}

//获取文件名
function getFileName(url) {
    var pos = url.lastIndexOf("/");
    if (pos == -1) {
        pos = url.lastIndexOf("\\")
    }
    var filename = url.substr(pos + 1)
    return filename;
}
//ajax异步错误处理
function AjaxError(msg) {
    //if (window.location.href.lastIndexOf("51hiring") != -1) {
    alert("Server is busy, Please hode on ^_^");
    //    } else {
    //        $("html").html(msg.responseText);
    //    }
    /*window.location.href = "/Account/Index";
    console.log(msg)*/
}
//面页后退
function urlBack() {
    history.go(-1);
}
//页面跳转
function _url(url) {
    window.location.href = url;
}
//打开新页面跳转
function _open(url) {
    window.open(url);
}
function _errorImg(obj) {
    obj.onerror = null; obj.src = '/Content/Image/defaultHead.gif';
}
//JS判段图片是否存在
function CheckImgExists(imgurl) {
    var ImgObj = new Image(); //判断图片是否存在  
    ImgObj.src = "http://" + imgurl;
    //没有图片，则返回-1  
    if (ImgObj.fileSize > 0 || (ImgObj.width > 0 && ImgObj.height > 0)) {
        return true;
    } else {
        return false;
    }
}

function OpenOutlook(mails, subject, body) {
    location.href = 'mailto:' + mails + '?subject=' + subject + '&body=' + body;
}

// 设置为主页 
function SetHome(obj, vrl) {
    try {
        obj.style.behavior = 'url(#default#homepage)'; obj.setHomePage(vrl);
    }
    catch (e) {
        if (window.netscape) {
            try {
                netscape.security.PrivilegeManager.enablePrivilege("UniversalXPConnect");
            }
            catch (e) {
                alert("此操作被浏览器拒绝！\n请在浏览器地址栏输入“about:config”并回车\n然后将 [signed.applets.codebase_principal_support]的值设置为'true',双击即可。");
            }
            var prefs = Components.classes['@mozilla.org/preferences-service;1'].getService(Components.interfaces.nsIPrefBranch);
            prefs.setCharPref('browser.startup.homepage', vrl);
        } else {
            alert("您的浏览器不支持，请按照下面步骤操作：\n1.打开浏览器设置。\n2.点击设置网页。\n3.输入：" + vrl + "点击确定。");
        }
    }
}

// 加入收藏 兼容360和IE6 
function shoucang(sTitle, sURL) {
    try {
        window.external.addFavorite(sURL, sTitle);
    }
    catch (e) {
        try {
            window.sidebar.addPanel(sTitle, sURL, "");
        }
        catch (e) {
            alert("加入收藏失败，请使用Ctrl+D进行添加");
        }
    }
}

$.fn.extend({
    //表格行的移入移出事件
    hoverTable: function () {
        var th = this;
        th.find("tr").live("mouseover", function () {
            $(this).addClass("hover");
        })
        th.find("tr").live("mouseout", function () {
            $(this).removeClass("hover");
        })

    }
})
$.extend({
    //限制checkbox选中个数
    checkboxMaxNum: function (selector, maxNum, Message) {
        $(selector).unbind();
        $(selector).live("click", function () {
            var sizeNum = $(selector).filter(":checked").size();
            if (sizeNum > maxNum) {
                alert(Message);
                $(this).removeAttr("checked");
            }
        })
    }
})


//////只能输入数字
function reginput(obj, reg, inputstr) {
    if (event.srcelement.getattribute("readonly") || event.srcelement.getattribute("disabled")) return false;
    if (event.keycode < 46 || event.keycode > 57) return false;
    var docsel = document.selection.createrange();
    if (docsel.parentelement().tagname != "input") return false;
    osel = docsel.duplicate()
    osel.text = ""
    var srcrange = obj.createtextrange()
    osel.setendpoint("starttostart", srcrange)
    var str = osel.text + inputstr + srcrange.text.substr(osel.text.length)
    return reg.test(str);
}

function valNum(ev) {
    var e = ev.keyCode;
    //允许的有大、小键盘的数字，左右键，backspace, delete, Control + C, Control + V
    if (e != 48 && e != 49 && e != 50 && e != 51 && e != 52 && e != 53 && e != 54 && e != 55 && e != 56 && e != 57 && e != 96 && e != 97 && e != 98 && e != 99 && e != 100 && e != 101 && e != 102 && e != 103 && e != 104 && e != 105 && e != 37 && e != 39 && e != 13 && e != 8 && e != 46) {
        if (ev.ctrlKey == false) {
            //不允许的就清空!
            ev.returnValue = "";
        }
        else {
            //验证剪贴板里的内容是否为数字!
            valClip(ev);
        }
    }
}
//验证剪贴板里的内容是否为数字!
function valClip(ev) {
    //查看剪贴板的内容!
    var content = clipboardData.getData("Text");
    if (content != null) {
        try {
            var test = parseInt(content);
            var str = "" + test;
            if (isNaN(test) == true) {
                //如果不是数字将内容清空!
                clipboardData.setData("Text", "");
            }
            else {
                if (str != content)
                    clipboardData.setData("Text", str);
            }
        }
        catch (e) {
            //清空出现错误的提示!
            alert("粘贴出现错误!");
        }
    }
}

//checked全选非选
function CheckAll(obj, selector) {
    var th = $(obj);
    var cbs = $(selector);
    var thisChecked = th.attr("checked");
    if (thisChecked) {
        cbs.attr("checked", "checked");
    } else {
        cbs.removeAttr("checked");
    }
}

//给Date 类型绑定一个format函数
Date.prototype.format = function (format) {
    var o = {
        "M+": this.getMonth() + 1,
        "d+": this.getDate(),
        "h+": this.getHours(),
        "m+": this.getMinutes(),
        "s+": this.getSeconds(),
        "q+": Math.floor((this.getMonth() + 3) / 3),
        "S": this.getMilliseconds()
    }
    if (/(y+)/.test(format)) {
        format = format.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(format)) {
            format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
        }
    }
    return format;
}
//格式化日期字符串
function getFormatDate(date, pattern) {
    if (date == undefined) {
        date = new Date();
    }
    if (pattern == undefined) {
        pattern = "yyyy-MM-dd hh:mm:ss";
    }
    return date.format(pattern);
}
//把JSON格式的时间转换成普通类型时间
function ChangeDateFormat(date, isFull) {
    if (date == null || date == undefined) {
        return "";
    }
    var pattern = "";
    if (isFull == true || isFull == undefined) {
        pattern = "yyyy-MM-dd hh:mm:ss";
    } else {
        pattern = "yyyy-MM-dd";
    }
    return getFormatDate(new Date(parseInt(date.replace("/Date(", "").replace(")/", ""), 10)), pattern);
}

//把JSON格式的时间转换成普通类型时间
function ChangeDateFormatHour(date, isFull, isHour) {
    var pattern = "";
    if (isHour == true && isFull != undefined) {
        pattern = "yyyy-MM-dd hh:mm";
    } else if ((isFull == true || isFull != undefined) && (isHour == false || isFull == undefined)) {
        pattern = "yyyy-MM-dd hh:mm:ss";
    } else if ((isFull == false || isFull == undefined) && (isHour == false || isFull == undefined)) {
        pattern = "yyyy-MM-dd";
    }
    var dd = new Date(date);
    return getFormatDate(new Date(date), pattern);
}

/*********************HTML转义 Start*************************/
function htmlEscape(html) {
    var elem = document.createElement('div')
    var txt = document.createTextNode(html)
    elem.appendChild(txt)
    return elem.innerHTML;
}
// 将实体转回为HTML
function htmlUnescape(str) {
    var elem = document.createElement('div')
    elem.innerHTML = str
    return elem.innerText || elem.textContent
}
/*********************HTML转义 End*************************/
// 格式化列表字符串 obj需要转换类型，long截取长度，isDateTime 是否时间类型，isFull是否为长日期
function FormatTableLongString(obj, long, isDateTime, isFull, isTip, tipContent) {
    if (obj == null || obj == undefined || obj == "") {
        return "<span></span>";
    }
    else {
        if (isDateTime != true) {
            // 字符截取截取
            //var reg = new RegExp('"', "g");
            //obj = obj.replace(reg, "\\\"");
            var temp = htmlUnescape(obj).toString();
            if (temp.length > long) {
                if (isTip) {
                    if (tipContent) {
                        return "<span  title=\"" + obj + "\" tip=\"" + tipContent + "\">" + obj.substring(0, long) + "... " + "</span>";
                    } else {
                        return "<span  title=\"" + obj + "\" tip=\"" + obj + "\">" + obj.substring(0, long) + "... " + "</span>";
                    }
                } else {
                    if (tipContent) {
                        return "<span  title=\"" + obj + "\">" + obj.substring(0, long) + "... " + "</span>";
                    } else {
                        return "<span  title=\"" + obj + "\">" + obj.substring(0, long) + "... " + "</span>";
                    }
                }
                //return isTip ? "<span  title=\"" + obj + "\" tip=\"" + obj + "\">" + obj.substring(0, long) + "... " + "</span>" : "<span  title=\"" + obj + "\">" + obj.substring(0, long) + "... " + "</span>";
            }
            else {
                if (isTip) {
                    if (tipContent) {
                        return "<span  title=\"" + obj + "\" tip=\"" + tipContent + "\">" + obj + "</span>";
                    } else {
                        return "<span  title=\"" + obj + "\" tip=\"" + obj + "\">" + obj + "</span>";
                    }
                } else {
                    if (tipContent) {
                        return "<span  title=\"" + obj + "\">" + obj + "</span>";
                    } else {
                        return "<span  title=\"" + obj + "\">" + obj + "</span>";
                    }
                }
                //return isTip ? "<span  title=\"" + obj + "\" tip=\"" + obj + "\">" + obj + "</span>" : "<span  title=\"" + obj + "\">" + obj + "</span>";
            }
        }
        else {
            //长时间和短时间
            if (isFull == true) {
                return "<span >" + ChangeDateFormat(obj, true); +"</span>";
            }
            else {
                return "<span >" + ChangeDateFormat(obj, false); +"</span>";
            }
        }
    }
}


//对内容进行HTML编码
function HtmlEncode(text) {
    if (isNaN(text)) {
        return text.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    } else {
        return text;
    }
}
//对内容进行HTML解码
function HtmlDecode(text) {
    if (isNaN(text)) {
        return text.replace(/&amp;/g, '&').replace(/&quot;/g, '/"').replace(/&lt;/g, '<').replace(/&gt;/g, '>');
    } else {
        return text;
    }
}
// 格式化列表字符串 obj需要转换类型，long截取长度，isDateTime 是否时间类型，isFull是否为长日期
function FormatTableLongStringHtmlEncode(obj, long, isDateTime, isFull) {
    if (obj == null || obj == undefined) {
        return "<span></span>";
    }
    else {
        if (isDateTime != true) {
            // 字符截取截取
            if (obj.toString().length > long) {
                return "<span  title='" + obj + "'>" + HtmlEncode(obj).toString().substring(0, long) + "... " + "</span>";
            }
            else {
                return "<span  title='" + obj + "'>" + HtmlEncode(obj).toString() + "</span>";
            }
        }
        else {
            //长时间和短时间
            if (isFull == true) {
                return "<span >" + ChangeDateFormat(obj, true); +"</span>";
            }
            else {
                return "<span >" + ChangeDateFormat(obj, false); +"</span>";
            }
        }
    }
}

//字符串编码并去除前后空格
function EncodeAndTrim(obj) {
    var str = $.trim(obj)
    // 处理特殊字符 °¹²³½¾¼×÷±
    if (str != null && str.toString().length > 0) {
        var e = new RegExp("°", "g");
        str = str.replace(e, "%@smartlaba@%");
        e = new RegExp("¹", "g");
        str = str.replace(e, "%@smartlabb@%");
        e = new RegExp("²", "g");
        str = str.replace(e, "%@smartlabc@%");
        e = new RegExp("³", "g");
        str = str.replace(e, "%@smartlabd@%");
        e = new RegExp("½", "g");
        str = str.replace(e, "%@smartlabe@%");
        e = new RegExp("¾", "g");
        str = str.replace(e, "%@smartlabf@%");
        e = new RegExp("¼", "g");
        str = str.replace(e, "%@smartlabg@%");
        e = new RegExp("×", "g");
        str = str.replace(e, "%@smartlabh@%");
        e = new RegExp("÷", "g");
        str = str.replace(e, "%@smartlabj@%");
        e = new RegExp("±", "g");
        str = str.replace(e, "%@smartlabk@%");
    }
    return escape(str);
}

//截取字符串的长度
String.prototype.CutString = function (strLen) {
    var temp = this;
    if (temp.length <= strLen) {
        return temp;
    } else {
        return temp.substr(0, strLen - 3) + "...";
    }
}
// 将字符串转换成2位小数,无小数位数将省略
function ToParseFloat(str) {
    if (str.length == 0) {
        return 0;
    } else {
        return (parseFloat(str) * 100).toFixed(2);
    }
}

//给字符串编码
String.prototype.Encode = function () {
    var str = $.trim(this);
    if (this != null && this.toString().length > 0) {
        // 处理特殊字符 °¹²³½¾¼×÷±
        var e = new RegExp("°", "g");
        str = str.replace(e, "%@smartlaba@%");
        e = new RegExp("¹", "g");
        str = str.replace(e, "%@smartlabb@%");
        e = new RegExp("²", "g");
        str = str.replace(e, "%@smartlabc@%");
        e = new RegExp("³", "g");
        str = str.replace(e, "%@smartlabd@%");
        e = new RegExp("½", "g");
        str = str.replace(e, "%@smartlabe@%");
        e = new RegExp("¾", "g");
        str = str.replace(e, "%@smartlabf@%");
        e = new RegExp("¼", "g");
        str = str.replace(e, "%@smartlabg@%");
        e = new RegExp("×", "g");
        str = str.replace(e, "%@smartlabh@%");
        e = new RegExp("÷", "g");
        str = str.replace(e, "%@smartlabj@%");
        e = new RegExp("±", "g");
        str = str.replace(e, "%@smartlabk@%");
        return escape(str);
    } else {
        return ""
    }
}
String.prototype.Decode = function () {
    return unescape(this);
}

// 解析自定义字符串
function ToParsingStr(obj) {
    if (obj == null || obj == undefined) {
        return "";
    } else {
        var e = RegExp("%@smartLab@%", "g");
        return obj.toString().replace(e, "\n");
    }
}


//截取显示字段，以避免显示不全问题
function ForbidFormatTableLongString(obj, long, isDateTime, isFull, isTip, tipContent) {
    if (obj == null || obj == undefined) {
        return "<span></span>";
    }
    else {
        if (isDateTime != true) {
            // 字符截取截取
            //var reg = new RegExp('"', "g");
            //obj = obj.replace(reg, "\\\"");
            var temp = htmlUnescape(obj).toString();
            if (temp.length > long) {
                //UpdateUser:Burgess.qian
                //Date:2015-12-24
                //防止过长，影响显示
                if (temp.length > 170) {
                    return "<p  title=\"" + obj + "\" tip=\"" + obj + "\">" + obj.substring(0, long) + "... " + "</p>";
                }
                if (isTip) {
                    debugger;
                    if (tipContent) {
                        return "<span  title=\"" + obj + "\" tip=\"" + tipContent + "\">" + obj.substring(0, long) + "... " + "</span>";
                    } else {
                        return "<span  title=\"" + obj + "\" tip=\"" + obj + "\">" + obj.substring(0, long) + "... " + "</span>";
                    }
                } else {
                    if (tipContent) {
                        return "<span  title=\"" + obj + "\">" + obj.substring(0, long) + "... " + "</span>";
                    } else {
                        return "<span  title=\"" + obj + "\">" + obj.substring(0, long) + "... " + "</span>";
                    }
                }
                //return isTip ? "<span  title=\"" + obj + "\" tip=\"" + obj + "\">" + obj.substring(0, long) + "... " + "</span>" : "<span  title=\"" + obj + "\">" + obj.substring(0, long) + "... " + "</span>";
            }
            else {
                if (isTip) {
                    if (tipContent) {
                        return "<span  title=\"" + obj + "\" tip=\"" + tipContent + "\">" + obj + "</span>";
                    } else {
                        return "<span  title=\"" + obj + "\" tip=\"" + obj + "\">" + obj + "</span>";
                    }
                } else {
                    if (tipContent) {
                        return "<span  title=\"" + obj + "\">" + obj + "</span>";
                    } else {
                        return "<span  title=\"" + obj + "\">" + obj + "</span>";
                    }
                }
                //return isTip ? "<span  title=\"" + obj + "\" tip=\"" + obj + "\">" + obj + "</span>" : "<span  title=\"" + obj + "\">" + obj + "</span>";
            }
        }
        else {
            //长时间和短时间
            if (isFull == true) {
                return "<span >" + ChangeDateFormat(obj, true); +"</span>";
            }
            else {
                return "<span >" + ChangeDateFormat(obj, false); +"</span>";
            }
        }
    }
}




//////////////////////////////项目扩展函数/////////////////////////////////////////////////
$.extend({
    //是否为IE7浏览器
    IsIE7: function () {
        return $.browser.msie && ($.browser.version == "7.0");
    }
})


//////////////////////////////jquery扩展库///////////////////////////////////////
$.fn.extend({
    attrToStr: function (attr) { //连接属性以逗号格开
        var reval = "";
        this.each(function () {
            reval += $(this).attr(attr) + ","
        })
        reval = $.trimEnd(reval, ",");
        return reval;
    },
    valIsNull: function () {
        var th = $(this);
        return (th == null || th.val() == null || th.val() == "");
    },
    //浮动居中
    elementsMiddle: function () {
        var th = this;
        var htmlObjH = th.height();
        var htmlObjW = th.width();
        var w = $(window).width();
        var scrollTop = $(document).scrollTop();
        var h = $(window).height() + scrollTop;
        var left = (w - htmlObjW) / 2;
        var top = (h - htmlObjH) / 2;
        th.css({ position: "absolute", "z-index": 1000000, left: left, top: top });
    }
})
$.extend({
    //移除最后一个字符
    trimEnd: function (str, c) {
        var reg = new RegExp(c + "([^" + c + "]*?)$");
        return str.replace(reg, function (w) { if (w.length > 1) { return w.substring(1); } else { return ""; } });
    },
    /*****************转换*************************************************/
    //将对像转为json字符串
    obj2json: function (o) {
        var arr = [];
        var fmt = function (s) {
            if (typeof s == 'object' && s != null) return $.obj2json(s);
            return /^(string|number)$/.test(typeof s) ? "'" + s + "'" : s;
        }
        for (var i in o) arr.push("'" + i + "':" + fmt(o[i]));
        return '{' + arr.join(',') + '}';
    },
    htmlEncode: function (str) {
        return str.replace(/&/g, '&amp').replace(/\"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    },
    htmlDecode: function (str) {
        return str.replace(/&amp;/g, '&').replace(/&quot;/g, '\"').replace(/&lt;/g, '<').replace(/&gt;/g, '>');
    },
    textEncode: function (str) {
        str = str.replace(/&amp;/gi, '&');
        str = str.replace(/</g, '&lt;');
        str = str.replace(/>/g, '&gt;');
        return str;
    },
    textDecode: function (str) {
        str = str.replace(/&amp;/gi, '&');
        str = str.replace(/&lt;/gi, '<');
        str = str.replace(/&gt;/gi, '>');
        return str;
    },
    /*****************判段*************************************************/

    //判断是否为数字
    isNumber: function (val) {
        return (/^\d+$/.test(val)) ? true : false;
    },
    //是否是邮箱
    isMail: function (val) {
        return (/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(val)) ? true : false;
    },
    //是否是手机
    isMobilePhone: function (val) {
        return (/\d{11}$/.test(val)) ? true : false;
    },
    //判断是否为负数和整数
    isNumberOrNegative: function (val) {
        return (/^\d+|\-\d+$/.test(val)) ? true : false;
    },
    //金额验证
    isMoney: function (val) {
        return (/^[1-9]d*.d*|0.d*[1-9]d*|\d+$/.test(val)) ? true : false;
    },
    //去除字符串中的PX,返回Int值
    subPx: function (val) {
        var _val = $.trim(val);
        _val = _val.substring(0, _val.length - 2);
        if (/[^\d]{n,}/.test(_val)) { return 0; } else { return parseInt(_val); }
    },
    //获取当前元素在数组中的索引
    jIndex: function (jsonarr, cId) {
        var index = -1;
        if (typeof jsonarr != "undefined") {
            for (var i = 0; i < jsonarr.length; i++) {
                if ($.trim(jsonarr[i].cId) == cId) {
                    index = i;
                    break;
                }
            }
        }
        return index;
    },

    scollPostion: function () {
        var t, l, w, h;
        if (document.documentElement && document.documentElement.scrollTop) {
            t = document.documentElement.scrollTop;
            l = document.documentElement.scrollLeft;
            w = document.documentElement.scrollWidth;
            h = document.documentElement.scrollHeight;
        } else if (document.body) {
            t = document.body.scrollTop;
            l = document.body.scrollLeft;
            w = document.body.scrollWidth;
            h = document.body.scrollHeight;
        }
        return { top: t, left: l, width: w, height: h };
    },
    getPageNames: function () {
        //获取当期页面名称
        var pageNames = window.location.pathname, urlpath;
        if (pageNames.indexOf('/') > -1) {
            urlpath = pageNames.split('/');
        }
        return urlpath;
    },
    //获取URL字符串
    getQueryString: function (name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
        var r = window.location.search.substr(1).match(reg);
        var context = "";
        if (r != null)
            context = r[2];
        reg = null;
        r = null;
        return context == null || context == "" || context == "undefined" ? "" : context;

    },

    addCookie: function (objName, objValue, objHours) {
        var str = objName + "=" + escape(objValue);
        if (objHours > 0) {//为0时不设定过期时间，浏览器关闭时cookie自动消失
            var date = new Date();
            var ms = objHours * 3600 * 1000;
            date.setTime(date.getTime() + ms);
            str += "; expires=" + date.toGMTString();
        }
        document.cookie = str;
    },
    getCookie: function (objName) {//获取指定名称的cookie的值
        var arrStr = document.cookie.split("; ");
        for (var i = 0; i < arrStr.length; i++) {
            var temp = arrStr[i].split("=");
            if (temp[0] == objName) return unescape(temp[1]);
        }
    },
    exIE6: function () {
        var flag = false;
        if ($.browser.msie && $.browser.version == "6.0")
            flag = true;
        return flag;
    },
    exIE7: function () {
        var flag = false;
        if ($.browser.msie && $.browser.version == "7.0")
            flag = true;
        return flag;
    },
    exIE8: function () {
        var flag = false;
        if ($.browser.msie && $.browser.version == "8.0")
            flag = true;
        return flag;
    },
    exIE9: function () {
        var flag = false;
        if ($.browser.msie && $.browser.version == "9.0")
            flag = true;
        return flag;
    },
    exIE10: function () {
        var flag = false;
        if ($.browser.msie && $.browser.version == "10.0")
            flag = true;
        return flag;
    },
    exMozilla: function () {
        var flag = false;
        if ($.browser.mozilla)
            flag = true;
        return flag;
    },
    exOpera: function () {
        var flag = false;
        if ($.browser.opera)
            flag = true;
        return flag;
    },
    exSafri: function () {
        var flag = false;
        if ($.browser.safari)
            flag = true;
        return flag;
    },
    //以逗号格开 验证所有元素value是否为空
    exAreNull: function (selectors) {
        var s = selectors.split(',');
        if (s == null) return false;
        var reval = true;;
        $(s).each(function (i, v) {
            var val = $(v).val();
            if (val == null || val == "" || $.trim(val) == "") {
                reval = false;
                return reval;
            }
        })
        return reval;
    }

})