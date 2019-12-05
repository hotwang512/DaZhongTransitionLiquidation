var vguid = "";
var mydate = new Date();
var ManagementCompany = [];
var dataArr = [];
var $page = function () {
    this.init = function () {
        var date = new Date();
        $("#DateOfYear").val(date.getFullYear());
        GetVehicleAmountCompany();
        addEvent();
        if ($("#EditPermission").val() == "True" || $("#NewPermission").val() == "True") {
            $("#CommentDialog_OKBtn").show();
        } else {
            $("#CommentDialog_OKBtn").hide();
        }
        var winHeight = document.body.scrollWidth;
        $("#table").css("width", winHeight - 242);
    }
    //所有事件
    function addEvent() {
        $(".menuRight").on("click",
            function () {
                $("#Comment").val("");
                $("#CommentModalDialog").modal("show");
                $("#CommentDialog_OKBtn").attr("tid", this.id.replace("menu_", ""));
                $("#Comment").val($("#" + this.id.replace("menu_", "")).attr("title"));
            });

        $("#CommentDialog_OKBtn").on("click",
            function () {
                $("#" + $("#CommentDialog_OKBtn").attr("tid")).attr("title", $("#Comment").val());
                $("#CommentModalDialog").modal("hide");
            });
        $("#CommentDialog_CancelBtn").on("click",
            function () {
                $("#CommentModalDialog").modal("hide");
            });
        $("#tableInput").on("blur",
            function () {
                var id = $("#tableInput").attr("VGUID");
                var oldData = $("#" + id).text();
                $("#" + id).text($("#tableInput").val());
                if (oldData != $("#tableInput").val()) {
                    var leftID = id.split("_")[0];
                    var rightID = id.split("_")[1];
                    for (var i = rightID; i <= 12; i++) {
                        $("#" + leftID + "_" + i).text($("#tableInput").val());
                    }
                }
                $("#tableInput").val("");
                $("#inputHolder").hide();
                computeSum();
            });
        $("#btnSave").on("click", function () {
            //验证
            var validateError = 0;
            for (var i = 1; i <= 12; i++) {
                if ($("#operation_manage_" + i).text() != $("#operation_asset_" + i).text()) {
                    validateError++;
                }
            }
            if (validateError == 0) {
                var rows = $(".tdvalue");
                var tbValues = [];
                for (var j = 0; j < rows.length; j++) {
                    tbValues.push({
                        "VGUID": rows[j].getAttribute("VGUID"),
                        "DateOfYear": $("#DateOfYear").val(),
                        "YearMonth": rows[j].id.split("_")[1],
                        "CompanyGuid": rows[j].id.split("_")[0],
                        "CompanyName": rows[j].getAttribute("companyname"),
                        "Comment": rows[j].title,
                        "LicenseAmount": rows[j].innerText
                    });
                };
                $.ajax({
                    url: "/AnalysisManagementCenter/VehicleAmount/SaveVehicleAmountList",
                    data: {
                        "tbValues": tbValues
                    },
                    type: "post",
                    success: function(msg) {
                        switch (msg.Status) {
                        case "0":
                            jqxNotification("保存失败", null, "error");
                            break;
                        case "1":
                            jqxNotification("保存成功！", null, "success");
                            break;
                        }
                    }
                });
            } else {
                jqxNotification("管理公司与资产公司小计不一致", null, "error");
            }
        });
        window.oncontextmenu = function (e) {
            e.preventDefault();
        }
        window.onclick = function (e) {
            $('#menuRight').css("width", 0);
        }
        $("#tableInput").on("keypress",
            function (e) {
                if (event.keyCode == 13) {
                    $("#tableInput").blur();
                }
            });
    }; //addEvent end
};
function GetVehicleAmountCompany() {
    layer.load();
    $.ajax({
        url: "/AnalysisManagementCenter/VehicleAmount/GetVehicleAmountCompanyList",
        type: "post",
        success: function (data) {
            var operationCompanyData = data.Rows.filter(function (e) { return e.Param1 == "营运公司"; });
            var chargeUnitData = data.Rows.filter(function (e) { return e.Param1 == "分管单位"; });
            var tripData = data.Rows.filter(function (e) { return e.Param1 == "大众出行"; });
            var chainData = data.Rows.filter(function (e) { return e.Param1 == "连锁企业"; });
            var theaders = ["公司名称", "1月", "2月", "3月", "4月", "5月", "6月", "7月", "8月", "9月", "10月", "11月", "12月"];
            var table = document.createElement("table");
            table.border = "1px";
            table.cellSpacing = 0;
            table.cellpadding = 0;
            var thead = document.createElement("thead");
            table.appendChild(thead);
            for (var k = 0; k < 2; k++) {
                var tr = document.createElement("tr");
                thead.appendChild(tr);
                for (var i = 0; i < theaders.length; i++) {
                    var theader = "牌照张数";
                    if (k == 0) {
                        theader = theaders[i];
                    } else if (k == 1 && i == 0) {
                        i++;
                    }
                    var th = document.createElement("th");
                    th.style.width = "130px";
                    if (i == 0 && k == 0) {
                        th.colSpan = 3;
                        th.rowSpan = 2;
                        th.style.width = "180px";
                    }
                    th.style.textAlign = "center";
                    tr.appendChild(th);
                    th.innerText = theader;
                }
            }
            var tbody = document.createElement("tbody");
            tbody = getTbodyOperationCompany(tbody, table, operationCompanyData, "管理公司", 0, theaders.length + 2);
            tbody = getTbodyOperationCompany(tbody, table, operationCompanyData, "资产公司", 1, theaders.length + 2);
            tbody = getTbodyChargeUnit(tbody, table, chargeUnitData, theaders.length + 1);
            tbody = getTbodyTrip(tbody, table, tripData, theaders.length + 1);
            tbody = getTbodyTaxiShanghaiSum(tbody, table, theaders.length);
            tbody = getTbodyChain(tbody, table, chainData, theaders.length + 1);
            tbody = getTbodyTaxiAllSum(tbody, table, theaders.length);
            table.appendChild(tbody);
            $("#table").html(table);
            
            GetVehicleAmountValueList();
        }
    });
}
function format_input_num(obj) {
    // 清除"数字"和"."以外的字符
    obj.value = obj.value.replace(/[^\d.]/g, "");
    // 验证第一个字符是数字
    obj.value = obj.value.replace(/^\./g, "");
    // 只保留第一个, 清除多余的
    obj.value = obj.value.replace(/\.{2,}/g, ".");
    obj.value = obj.value.replace(".", "$#$").replace(/\./g, "").replace("$#$", ".");
    // 只能输入两个小数
    obj.value = obj.value.replace(/^(\-)*(\d+)\.(\d\d).*$/, '$1$2.$3');
}
function GetVehicleAmountValueList() {
    $.ajax({
        url: "/AnalysisManagementCenter/VehicleAmount/GetVehicleAmountValueList",
        data: {
            "DateOfYear": $("#DateOfYear").val()
        },
        type: "post",
        success: function (data) {
            if (data.Rows != null) {
                for (var i = 0; i < data.Rows.length; i++) {
                    $("#" + data.Rows[i].CompanyGuid + "_" + data.Rows[i].YearMonth).text(data.Rows[i].LicenseAmount);
                    $("#" + data.Rows[i].CompanyGuid + "_" + data.Rows[i].YearMonth).attr("VGUID", data.Rows[i].VGUID);
                    $("#" + data.Rows[i].CompanyGuid + "_" + data.Rows[i].YearMonth).attr("title", data.Rows[i].Comment);
                }
                computeSum();
            }
            layer.closeAll('loading');
        }
    });
}
function computeSum() {
    var companyArr = ["operation_manage", "operation_asset", "chargeunit", "trip", "chain"];
    for (var k = 1; k <= 12; k++) {
        for (var n = 0; n < companyArr.length; n++) {
            var sumID = companyArr[n] + "_" + k;
            var objList = $("td[parentgroup$='" + companyArr[n] + "'][id$='_" + k + "']");
            var sum = 0;
            for (var i = 0; i < objList.length; i++) {
                if (objList[i].innerText != "") {
                    sum += parseFloat(objList[i].innerText);
                }
            }
            $("#" + sumID).text(sum);
            var month = k;
            var shanghaiSumID = "taxiShanghaiSum_" + month;
            var shanghaiSum = 0;
            if (companyArr[n] == "operation_manage" ||
                companyArr[n] == "trip" ||
                companyArr[n] == "chargeunit") {
                if ($("#operation_manage_" + month)[0].innerText != "") {
                    shanghaiSum += parseFloat($("#operation_manage_" + month)[0].innerText);
                }
                if ($("#chargeunit_" + month)[0].innerText != "") {
                    shanghaiSum += parseFloat($("#chargeunit_" + month)[0].innerText);
                }
                if ($("#trip_" + month)[0].innerText != "") {
                    shanghaiSum += parseFloat($("#trip_" + month)[0].innerText);
                }
                $("#" + shanghaiSumID).text(shanghaiSum);
            }
        }
        var allSumID = "taxiAllSum_" + k;
        $("#" + allSumID).text(parseFloat($("#chain_" + k)[0].innerText) + parseFloat($("#taxiShanghaiSum_" + k)[0].innerText));
    }
}
function getTbodyOperationCompany(tbody, table, datas,companytype, startnum, collength) {
    var manageCompanyData = datas.filter(function (e) { return e.Param2 == companytype });
    for (var i = 0; i < manageCompanyData.length + 1; i++) {
        var tr = document.createElement("tr");
        tbody.appendChild(tr);
        tr.style.textAlign = "center";
        for (var k = startnum; k < collength; k++) {
            if (i > 0 && k == startnum) {
                k = k + 2 - startnum;
            }
            var td = document.createElement("td");
            tr.appendChild(td);
            if (i == 0 && k == 0) {
                td.rowSpan = datas.length + 2;
                td.innerText = "营运公司";
            } else if (i == 0 && k == 1) {
                td.rowSpan = manageCompanyData.length + 1;
                td.innerText = companytype;
            } else if (k == 2 && i < manageCompanyData.length) {
                td.innerText = manageCompanyData[i].DESC0;
            } else if (k == 2) {
                td.innerText = "小计";
            } else if (i < manageCompanyData.length) {
                td.id = manageCompanyData[i].LGUID + "_" + (k - 2);
                $(td).attr("companyname", manageCompanyData[i].DESC0);
                if (companytype == "管理公司") {
                    $(td).attr("parentGroup", "operation_manage");
                } else if (companytype == "资产公司") {
                    $(td).attr("parentGroup", "operation_asset");
                }
                $(td).addClass("tdvalue");
                $(td).attr("VGUID", "");
                td.ondblclick = function () {
                    $("#inputHolder").show();
                    $("#inputHolder").css("top", this.offsetTop);
                    $("#inputHolder").css("left", this.offsetLeft + 1);
                    $("#tableInput").css("height", this.clientHeight);
                    $("#tableInput").css("width", this.clientWidth);
                    $("#tableInput").attr("VGUID", this.id);
                    $("#tableInput").val(this.innerText);
                    $("#tableInput").focus();
                    $("#tableInput").select();
                }
                td.oncontextmenu = function (e) {
                    e.preventDefault();
                    var menu = document.querySelector("#menuRight");
                    menu.style.left = e.pageX + 'px';
                    menu.style.top = e.pageY + 'px';
                    menu.style.width = '125px';
                    menu.children[0].id = "menu_" + this.id;
                }
            } else if (i == manageCompanyData.length) {
                if (companytype == "管理公司") {
                    $(td).attr("id", "operation_manage" + "_" + (k - 2));
                } else if (companytype == "资产公司") {
                    $(td).attr("id", "operation_asset" + "_" + (k - 2));
                }
            }
        }
    }
    return tbody;
}
function getTbodyChargeUnit(tbody, table, datas, collength) {
    for (var i = 0; i < datas.length + 1; i++) {
        var tr = document.createElement("tr");
        tbody.appendChild(tr);
        tr.style.textAlign = "center";
        for (var k = 0; k < collength; k++) {
            if (i > 0 && k == 0) {
                k = k + 1;
            }
            var td = document.createElement("td");
            tr.appendChild(td);
            if (i == 0 && k == 0) {
                td.rowSpan = datas.length + 1;
                td.innerText = "分管单位";
            } else if (k == 1 && i < datas.length) {
                td.innerText = datas[i].DESC0;
                td.colSpan = 2;
            } else if (k == 1) {
                td.innerText = "小计";
                td.colSpan = 2;
            } else if (i < datas.length) {
                td.id = datas[i].LGUID + "_" + (k - 1);
                $(td).attr("companyname", datas[i].DESC0);
                $(td).attr("parentGroup", "chargeunit");
                $(td).addClass("tdvalue");
                $(td).attr("VGUID", "");
                td.ondblclick = function () {
                    $("#inputHolder").show();
                    $("#inputHolder").css("top", this.offsetTop);
                    $("#inputHolder").css("left", this.offsetLeft + 1);
                    $("#tableInput").css("height", this.clientHeight);
                    $("#tableInput").css("width", this.clientWidth);
                    $("#tableInput").attr("VGUID", this.id);
                    $("#tableInput").val(this.innerText);
                    $("#tableInput").focus();
                }
                td.oncontextmenu = function (e) {
                    e.preventDefault();
                    var menu = document.querySelector("#menuRight");
                    menu.style.left = e.pageX + 'px';
                    menu.style.top = e.pageY + 'px';
                    menu.style.width = '125px';
                    menu.children[0].id = "menu_" + this.id;
                }
            } else if (i == datas.length) {
                $(td).attr("id", "chargeunit" + "_" + (k - 1));
            }
        }
    }
    return tbody;
}
function getTbodyTrip(tbody, table, datas, collength) {
    for (var i = 0; i < datas.length + 1; i++) {
        var tr = document.createElement("tr");
        tbody.appendChild(tr);
        tr.style.textAlign = "center";
        for (var k = 0; k < collength; k++) {
            if (i > 0 && k == 0) {
                k = k + 1;
            }
            var td = document.createElement("td");
            tr.appendChild(td);
            if (i == 0 && k == 0) {
                td.rowSpan = datas.length + 1;
                td.innerText = "大众出行";
            } else if (k == 1 && i < datas.length) {
                td.innerText = datas[i].DESC0;
                td.colSpan = 2;
            } else if (k == 1) {
                td.innerText = "小计";
                td.colSpan = 2;
            } else if (i < datas.length) {
                td.id = datas[i].LGUID + "_" + (k - 1);
                $(td).attr("companyname", datas[i].DESC0);
                $(td).attr("parentGroup", "trip");
                $(td).addClass("tdvalue");
                $(td).attr("VGUID", "");
                td.ondblclick = function () {
                    $("#inputHolder").show();
                    $("#inputHolder").css("top", this.offsetTop);
                    $("#inputHolder").css("left", this.offsetLeft + 1);
                    $("#tableInput").css("height", this.clientHeight);
                    $("#tableInput").css("width", this.clientWidth);
                    $("#tableInput").attr("VGUID", this.id);
                    $("#tableInput").val(this.innerText);
                    $("#tableInput").focus();
                }
                td.oncontextmenu = function (e) {
                    e.preventDefault();
                    var menu = document.querySelector("#menuRight");
                    menu.style.left = e.pageX + 'px';
                    menu.style.top = e.pageY + 'px';
                    menu.style.width = '125px';
                    menu.children[0].id = "menu_" + this.id;
                }
            } else if (i == datas.length) {
                $(td).attr("id", "trip" + "_" + (k - 1));
            }
        }
    }
    return tbody;
}
function getTbodyChain(tbody, table, datas, collength) {
    for (var i = 0; i < datas.length + 1; i++) {
        var tr = document.createElement("tr");
        tbody.appendChild(tr);
        tr.style.textAlign = "center";
        for (var k = 0; k < collength; k++) {
            if (i > 0 && k == 0) {
                k = k + 1;
            }
            var td = document.createElement("td");
            tr.appendChild(td);
            if (i == 0 && k == 0) {
                td.rowSpan = datas.length + 1;
                td.innerText = "连锁企业";
            } else if (k == 1 && i < datas.length) {
                td.innerText = datas[i].DESC0;
                td.colSpan = 2;
            } else if (k == 1) {
                td.innerText = "连锁合计";
                td.style.backgroundColor = "yellow";
                td.colSpan = 2;
            } else if (i < datas.length) {
                td.id = datas[i].LGUID + "_" + (k - 1);
                $(td).attr("companyname", datas[i].DESC0);
                $(td).attr("parentGroup", "chain");
                $(td).addClass("tdvalue");$(td).attr("VGUID", "");
                td.ondblclick = function () {
                    $("#inputHolder").show();
                    $("#inputHolder").css("top", this.offsetTop);
                    $("#inputHolder").css("left", this.offsetLeft + 1);
                    $("#tableInput").css("height", this.clientHeight);
                    $("#tableInput").css("width", this.clientWidth);
                    $("#tableInput").attr("VGUID", this.id);
                    $("#tableInput").val(this.innerText);
                    $("#tableInput").focus();
                }
                td.oncontextmenu = function (e) {
                    e.preventDefault();
                    var menu = document.querySelector("#menuRight");
                    menu.style.left = e.pageX + 'px';
                    menu.style.top = e.pageY + 'px';
                    menu.style.width = '125px';
                    menu.children[0].id = "menu_" + this.id;
                }
            } else if (i == datas.length) {
                td.style.backgroundColor = "yellow";
                $(td).attr("id", "chain" + "_" + (k - 1));
            }
        }
    }
    return tbody;
}
function getTbodyTaxiShanghaiSum(tbody, table, collength) {
    var tr = document.createElement("tr");
    tbody.appendChild(tr);
    tr.style.textAlign = "center";
    for (var k = 0; k < collength; k++) {
        var td = document.createElement("td");
        td.style.backgroundColor = "yellow";
        tr.appendChild(td);
        if (k == 0) {
            td.colSpan = 3;
            td.innerText = "上海出租车合计";
        } else{
            $(td).attr("id", "taxiShanghaiSum" + "_" + k);
        }
    }
    return tbody;
}
function getTbodyTaxiAllSum(tbody, table, collength) {
    var tr = document.createElement("tr");
    tbody.appendChild(tr);
    tr.style.textAlign = "center";
    for (var k = 0; k < collength; k++) {
        var td = document.createElement("td");
        td.style.backgroundColor = "rgb(179, 193, 230)";
        tr.appendChild(td);
        if (k == 0) {
            td.colSpan = 3;
            td.innerText = "出租板块总计";
        } else {
            $(td).attr("id", "taxiAllSum" + "_" + k);
        }
    }
    return tbody;
}
function pickedFunc() {
    GetVehicleAmountCompany();
}
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}
$(function () {
    var page = new $page();
    page.init();
});
function formatDate(NewDtime) {
    var d = NewDtime;
    var datetime = d.getFullYear() + '-' + (d.getMonth() + 1);//  + '-' + d.getDate() + ' ' + d.getHours() + ':' + d.getMinutes() + ':' + d.getSeconds()
    return datetime;
}