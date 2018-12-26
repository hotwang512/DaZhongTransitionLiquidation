var $page = function () {

    this.init = function () {
        addEvent();
    }
    var guid = $.request.queryString().VGUID;
    //所有事件
    function addEvent() {
        var myDate = new Date();
        var date = myDate.toLocaleDateString();     //获取当前日期

        if (guid != "" && guid != null) {
            getOrderListDraftPrint();
        } 
        //打印
        $("#btnPrint").on("click", function () {
            $(".body_mk").printArea();
        })

    }; //addEvent end

    function getOrderListDraftPrint() {
        $.ajax({
            url: "/CapitalCenterManagement/OrderListDraftPrint/GetOrderListDraftPrint",
            data: {
                "vguid": guid,
            },
            type: "post",
            dataType: "json",
            success: function (msg) {
                //$("#BusinessType").text(msg.BusinessType);
                //$("#BusinessProject").text(msg.BusinessProject);
                //$("#BusinessSubItem1").text(msg.BusinessSubItem1);
                //$("#BusinessSubItem2").text(msg.BusinessSubItem2);
                //$("#BusinessSubItem3").text(msg.BusinessSubItem3);
                //$("#OrderDate").text(msg.OrderDate);
                //$("#OrderTime").text(msg.OrderTime);
                $("#PaymentCompany").text(msg.PaymentCompany);
                //$("#CollectionCompany").text(msg.CollectionCompany);
                //$("#VisitorsNumber").text(msg.VisitorsNumber);
                //$("#EscortNumber").text(msg.EscortNumber);
                //$("#NumberCount").text(msg.NumberCount);
                
                $("#Money").text(msg.Money);
                $("#CapitalizationMoney").text(msg.CapitalizationMoney);
                $("#EnterpriseLeader").text(msg.EnterpriseLeader);
                $("#ResponsibleLeader").text(msg.ResponsibleLeader);
                $("#JiCaiBuExamine").text(msg.JiCaiBuExamine);
                $("#DepartmentHead").text(msg.DepartmentHead);
                $("#Cashier").text(msg.Cashier);
                $("#Payee").text(msg.Payee);
                $("#InvoiceNumber").text(msg.InvoiceNumber);
                $("#AttachmentNumber").text(msg.AttachmentNumber);
                $("#PaymentContents").text(msg.PaymentContents);
                //$("#FillingDate").text(msg.FillingDate);
                var fillingDate = parseInt(msg.FillingDate.replace(/[^0-9]/ig, ""));//转时间戳
                var date = $.convert.toDate(new Date(fillingDate), "yyyy-MM-dd");
                var d = date.split("-")[0] + " 年 " + date.split("-")[1] + " 月 " + date.split("-")[2] +" 日";
                $("#FillingDate").text(d);

                switch (msg.PaymentMethod) {
                    case "现金": $("#Cash").text("√");
                        break;
                    case "银行": $("#Bank").text("√");
                        break;
                    case "其他": $("#Other").text("√");
                        break;
                    default:
                }
                //$("#CapitalizationMoney").attr("title", $("#CapitalizationMoney").text())
                //loadAttachments(msg.Attachment);
            }
        });
    }
};


$(function () {
    var page = new $page();
    page.init();
});