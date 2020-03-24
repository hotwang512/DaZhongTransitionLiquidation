//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
}; //selector end
var isEdit = false;
var vguid = "";
var index = 0;//切换借贷
var CompanyCode = loadCompanyCode("A");
var AccountSection = null;
var CostCenterSection = null;
var SpareOneSection = null;
var SpareTwoSection = null;
var IntercourseSection = null;
var selectIndex = 0;//生成块的数量
var loginCompanyCode = $("#LoginCompanyCode").val();
var loginAccountModeCode = $("#LoginAccountModeCode").val();
var voucherHtmls = "";
var subjectName0 = "";
var subjectName1 = "";
var $page = function () {

    this.init = function () {
        addEvent();
    }

    //所有事件
    function addEvent() {
        var myDate = new Date();//获取系统当前时间
        var month = (myDate.getMonth() + 1) < 10 ? "0" + (myDate.getMonth() + 1) : (myDate.getMonth() + 1);
        var day = (myDate.getDate()) < 10 ? "0" + (myDate.getDate()) : (myDate.getDate());
        var date = myDate.getFullYear().toString() + month.toString() + day;
        var voucherType = $("#VoucherType").val();
        var type = $.request.queryString().Type;
        if (type == 0 || type == "0") {
            voucherType = "现金类";
        } else if (type == "1") {
            voucherType = "银行类";
            $("#VoucherType").val(voucherType);
        } else if (type == "2") {
            voucherType = "转账类";
            $("#VoucherType").val(voucherType);
        }
        $("#BatchName").val("");
        $("#VoucherDate").val(myDate.getFullYear() + "-" + month + "-" + day);
        $("#AccountingPeriod").val(myDate.getFullYear() + "-" + month);
        uiEngineHelper.bindSelect('#CompanySection', CompanyCode, "Code", "Descrption");
        var guid = $.request.queryString().VGUID;
        $("#VGUID").val(guid)
        if (guid != "" && guid != null) {
            getVoucherDetail();
            addVoucherListTable();
            $("#VoucherType").attr("disabled", "disableds");
        } else {
            addVoucherListTable();
            $("#hideButton").show();
            $("#AddNewBankData_GoBackBtn").hide();
            $//("#DocumentMaker").val($("#LoginName").val());
            if ($("#Status").val() != "1" && $("#Status").val() != "") {
                $("#Auditor").val($("#LoginName").val());
            }
            $("#AttachmentHide").show();
            getPersonInfo();
        }
        //控件ID后缀
        var str = "";

        //新增
        $("#btnAddDetail").on("click", function () {
            var trMore = "";
            var trIndex = $("#VoucherTable tr").length - 2;
            if (trIndex > 2) {
                if (subjectName0 == "" && subjectName1 != "") {
                    var trId = $("#VoucherTable tr")[trIndex - 1].id;
                    if (trId.length == 9) {
                        trIndex = parseInt(trId.substr(trId.length - 2, 2)) + 1;
                    } else {
                        trIndex = parseInt(trId.substr(trId.length - 1, 1)) + 1;
                    }
                } else {
                    var trId = $("#VoucherTable tr")[trIndex].id;
                    if (trId.length == 9) {
                        trIndex = parseInt(trId.substr(trId.length - 2, 2)) + 1
                    } else {
                        trIndex = parseInt(trId.substr(trId.length - 1, 1)) + 1;
                    }
                }
            }
            trMore += "<tr id='closeTr" + trIndex + "' style='height:60px'>" +
                             "<td style='text-align: center;'><textarea id='Remark" + trIndex + "' type='text' style='width: 248px;' class='input_text form-control'></textarea></td>" +
                             "<td style='text-align: center;'><textarea id='SubjectName" + trIndex + "' readonly='readonly' class='subjectArea' style='width: 875px; height: 58px;text-indent: 15px;'></textarea> <button id='btnEdit" + trIndex + "' type='button' class='buttons subjectbtn' style=''>编辑</button></td>" +
                             "<td style='text-align: right;'><input id='Borrow" + trIndex + "' type='text' style='width: 150px;text-align: right' class='input_text form-control money Borrow'/></td>" +
                             "<td style='text-align: right;'><input id='Loan" + trIndex + "' type='text' style='width: 150px;text-align: right' class='input_text form-control money Loan'/></td>" +
                             "<td style='text-align: center;'><button id='Remove" + trIndex + "' type='button' onclick='removeSubjectTr(this)'>×</button></td>" +
                      "</tr>"
            if (subjectName0 == "" && subjectName1 != "") {
                $("#closeTr1").before(trMore);
            } else {
                $("#countTr").before(trMore);
            }

            tdClick();
        });
        //取消
        $("#btnCancel").on("click", function () {
            history.go(-1);
        })
        //切换标签页
        $('#jqxTabs').on('tabclick', function (event) {
            index = event.args.item;
            console.log(index);
            var length2 = $(".nav-i2").length;
            if (index == 1 && length2 == 0) {
                //addSectionDiv();
            } else {
                autoHeight(index);
            }
        });
        var initWidgets = function (tab) {
            switch (tab) {
                case 0:
                    //initTable0();
                    break;
                case 1:
                    //initTable1();
                    break;
            }
        }
        //$('#jqxTabs').jqxTabs({ width: '1560px', height: 300, initTabContent: initWidgets });
        //双击选择科目
        $("#jqxSubjectSection").on('rowDoubleClick', function (event) {
            // event args.
            var args = event.args;
            // row data.
            var row = args.row;
            // row key.
            var key = args.key;
            // data field
            //$("#SubjectSection_" + z1 + "_" + z2).val(row.Descrption);
            //$("#hidSubjectSection_" + z1 + "_" + z2).val(row.Code);
            //$("#AddCompanyDialog").modal("hide");
            //var code = $("#CompanySection_" + z1 + "_" + z2).val();
            if (row.records != null) {
                jqxNotification("请选择已配置信息节点！", null, "error");
                return;
            }
            $("#SubjectSection").val(row.Descrption);
            $("#hidSubjectSection").val(row.Code);
            $("#AddCompanyDialog").modal("hide");
            var code = $("#CompanySection").val();
            AccountSection = loadCompanyCode("C", code, row.Code);
            CostCenterSection = loadCompanyCode("D", code, row.Code);
            SpareOneSection = loadCompanyCode("E", code, row.Code);
            SpareTwoSection = loadCompanyCode("F", code, row.Code);
            IntercourseSection = loadCompanyCode("G", code, row.Code);
            //var str = "_" + z1 + "_" + z2;
            loadSelectFun(str);
        });
        //保存
        $("#btnSave").on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (validateError <= 0) {
                var detailList = [];
                var length = $("#VoucherTable tr").length;
                for (var j = 0; j < length; j++) {
                    var i = 0;
                    var idName = $("#VoucherTable tr")[j].id;
                    if (idName.search("closeTr") != -1) {
                        if (idName.length == 9) {
                            i = idName.substring(idName.length - 2, idName.length);
                        } else {
                            i = idName.substring(idName.length - 1, idName.length);
                        }
                    } else {
                        continue;
                    }
                    var remark = $("#Remark" + i).val();
                    var subjectNames = $("#SubjectName" + i).val();
                    var subjectName = $("#SubjectName" + i).val().split(".");
                    var CompanySection = subjectName[0];
                    var SubjectSection = subjectName[1];
                    var SubjectSectionName = subjectName[7];
                    var AccountSection = subjectName[2];
                    var CostCenterSection = subjectName[3];
                    var SpareOneSection = subjectName[4];
                    var SpareTwoSection = subjectName[5];
                    var IntercourseSection = subjectName[6].split(/[\s\n]/)[0];
                    var companyName = subjectName[6].split(/[\s\n]/)[1];
                    var borrowMoney = 0;
                    var loanMoney = 0;
                    borrowMoney = $("#Borrow" + i).val().replace(/,/g, '');
                    loanMoney = $("#Loan" + i).val().replace(/,/g, '');
                    var detail = {
                        "VGUID": "",
                        "Abstract": remark,
                        "CompanySection": CompanySection,
                        "SubjectSection": SubjectSection,
                        "SubjectSectionName": SubjectSectionName,
                        "AccountSection": AccountSection,
                        "CostCenterSection": CostCenterSection,
                        "SpareOneSection": SpareOneSection,
                        "SpareTwoSection": SpareTwoSection,
                        "IntercourseSection": IntercourseSection,
                        "BorrowMoney": borrowMoney,
                        "LoanMoney": loanMoney,
                        "SevenSubjectName": subjectNames,
                        "JE_LINE_NUMBER": j
                    }
                    detailList.push(detail);
                }
                //var y = JSON.stringify(detailList);
                console.log(detailList);
                $("#VoucherType").removeAttr("disabled");
                //$("#CompanyCode").removeAttr("disabled");
                $.ajax({
                    url: "/VoucherManageManagement/VoucherListDetail/SaveVoucherListDetail",
                    data: {
                        "VGUID": $("#VGUID").val(),
                        "AccountModeName": $("#AccountModeName").val(),
                        "CompanyCode": CompanySection,
                        "CompanyName": companyName,
                        "VoucherType": $("#VoucherType").val(),
                        "AccountingPeriod": $("#AccountingPeriod").val(),
                        "BatchName": $("#BatchName").val(),
                        "Currency": $("#Currency").val(),
                        "VoucherNo": $("#VoucherNo").val(),
                        "VoucherDate": $("#VoucherDate").val(),
                        "FinanceDirector": $("#FinanceDirector").val(),
                        "Bookkeeping": $("#Bookkeeping").val(),
                        "Auditor": $("#Auditor").val(),
                        "DocumentMaker": $("#DocumentMaker").val(),
                        "Cashier": $("#Cashier").val(),
                        "Attachment": $("#Attachment").val(),
                        "Detail": detailList
                    },
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败！", null, "error");
                                //$("#VoucherType").attr("disabled", "disableds");
                                //$("#CompanyCode").attr("disabled", "disableds");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                $("#VGUID").val(msg.ResultInfo);
                                //history.go(-1);
                                //window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                                break;
                        }
                    }
                });
            }
        })
        //预览
        $("#Preview").on("click", function () {
            var borrowCount = Number($("#BorrowCount").val().replace(/,/g, ''));
            var loanCount = Number($("#LoanCount").val().replace(/,/g, ''));
            //var borrowCount = $("#BorrowCount").val();
            //var loanCount = $("#LoanCount").val();
            var documentMaker = $("#DocumentMaker").val();
            var auditor = $("#Auditor").val();
            if (borrowCount != loanCount) {
                jqxNotification("借贷不相等！", null, "error");
                return;
            }
            if (documentMaker == "") {
                $("#DocumentMaker").val($("#LoginName").val());
            }
            if (auditor == "" && $("#Status").val() != "1" && $("#Status").val() != "") {
                $("#Auditor").val($("#LoginName").val());
            }
            $("#SubjectTable").remove();
            //var x = $(".nav-i")[0].id.split("_")[1];
            if ($("#SubjectName0").val() != null && $("#SubjectName0").val() != "") {
                var subjectName = $("#SubjectName0").val().split(".");
                var companyName = subjectName[6].split(/[\s\n]/)[1];
                if (subjectName[6].split(/[\s\n]/).length < 2) {
                    companyName = subjectName[6].substring(1, subjectName[6].length);
                }
                $("#lblCompany").text(companyName);
            } else {
                jqxNotification("借贷科目不完整！", null, "error");
                return;
            }
            $("#lblAccountingPeriods").text($("#AccountingPeriod").val());
            $("#lblBatchName").text($("#BatchName").val());
            $("#lblCurrency").text("人民币");
            $("#lblVoucherNo").text($("#VoucherNo").val());
            $("#lblVoucherDate").text($("#VoucherDate").val());
            if ($("#Attachment").val() != "") {
                var att = $("#Attachment").val().split(",");
                var attlength = 0;
                for (var i in att) {
                    if (att[i] != "") {
                        attlength += 1;
                    }
                }
                $("#lblAttachmentNumber").text(attlength);
            } else {
                $("#lblAttachmentNumber").text("");
            }
            $("#lblDocumentMaker").text($("#DocumentMaker").val());
            $("#lblFinanceDirector").text($("#FinanceDirector").val());
            $("#lblBookkeeping").text($("#Bookkeeping").val());
            $("#lblAuditor").text($("#Auditor").val());
            $("#lblCashier").text($("#Cashier").val());
            //var lengths = $(".nav-i").length + $(".nav-i2").length;
            var htmls = "";
            var list1 = "";

            for (var j = 0; j < $("#VoucherTable tr").length; j++) {
                var i = 0;
                var idName = $("#VoucherTable tr")[j].id;
                if (idName.search("closeTr") != -1) {
                    if (idName.length == 9) {
                        i = idName.substring(idName.length - 2, idName.length);
                    } else {
                        i = idName.substring(idName.length - 1, idName.length);
                    }
                } else {
                    continue;
                }
                var borrowMoney = 0;
                var loanMoney = 0;
                borrowMoney = $("#Borrow" + i).val();
                loanMoney = $("#Loan" + i).val();
                list1 += "<tr style='height:40px'>" +
                              "<td style='text-align: left;'>" + "  " + $("#Remark" + i).val() + "</td>" +
                              "<td style='text-align: left;'>" + "  " + $("#SubjectName" + i).val() + "</td>" +
                              "<td style='text-align: right;'>" + borrowMoney + "  " + "</td>" +
                              "<td style='text-align: right;'>" + loanMoney + "  " + "</td>" +
                        "</tr>";
            }
            htmls = "<table id='SubjectTable' style='width:100%;white-space:pre' border='1' cellspacing='0'>" +
                         "<tr style='height:40px'>" +
                              "<td style='width: 250px;text-align: center;font-size: 18px;'>摘要</td>" +
                              "<td style='text-align: center;font-size: 18px;'>科目及描述</td>" +
                              "<td style='width: 150px;text-align: center;font-size: 18px;'>借方金额</td>" +
                              "<td style='width: 150px;text-align: center;font-size: 18px;'>贷方金额</td>" +
                        "</tr>"
                           + list1 +
                        "<tr style='height:40px'>" +
                              "<td style='text-align: center;'>合计</td>" +
                              "<td style='text-align: center;'></td>" +
                              "<td style='text-align: right;'>" + $("#BorrowCount").val() + "  " + "</td>" +
                              "<td style='text-align: right;'>" + $("#LoanCount").val() + "  " + "</td>" +
                       "</tr>"
            "</table>";
            $("#VoucherDetail").append(htmls);
            $("#ShowDialog").modal({ backdrop: "static", keyboard: false });
            $("#ShowDialog").modal("show");
        })
        //取消
        $("#AddNewBankData_CancelBtn").on("click", function () {
            $("#ShowDialog").modal({ backdrop: "static", keyboard: false });
            $("#ShowDialog").modal("hide");
        })
        //退回
        $("#AddNewBankData_GoBackBtn").on("click", function () {
            if ($("#Status").val() == "2") {
                var vguid = $("#VGUID").val();
                var tableIndex = $("#Automatic").val();
                $.ajax({
                    url: "/VoucherManageManagement/VoucherList/UpdataVoucherListInfo",
                    data: { vguids: vguid, status: "1", index: tableIndex },
                    traditional: true,
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("退回失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("退回成功！", null, "success");
                                history.go(-1);
                                break;
                        }
                    }
                });
            }
        })
        //取消
        $("#SevenSubject_CancelBtn").on("click", function () {
            $("#ShowSevenSubject").modal({ backdrop: "static", keyboard: false });
            $("#ShowSevenSubject").modal("hide");
        })
        //拍照上传
        $("#UploadFile").on("click", function () {
            baseUrl = "ws://127.0.0.1:12345";
            openSocket();
            $("#devPhoto").hide();
            $("#Upload_OKBtn").hide();
            $("#photographPri").show();
            //$("#AcceptDialog").modal("hide");
            $("#UploadPictureDialog").modal({ backdrop: "static", keyboard: false });
            $("#UploadPictureDialog").modal("show");
        })
        //拍照
        $("#photographPri").on("click", function () {
            $("#Upload_OKBtn").show();
            $("#photographPri").hide();
            $("#devPhoto").show();
        })
        $("#Upload_CancelBtn").on("click", function () {
            $("#UploadPictureDialog").modal("hide");
        });
        $("#Upload_OKBtn").on("click", function () {
            $('#jqxLoader').jqxLoader('open');
            $.ajax({
                url: "/VoucherManageManagement/VoucherListDetail/UploadImg",
                data: {
                    "ImageBase64Str": $("#devPhoto").attr("src"),
                },
                type: "post",
                success: function (msg) {
                    $('#jqxLoader').jqxLoader('close');
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("上传失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("上传成功！", null, "success");
                            //$("#Attachment").show();
                            //$("#Attachment").attr("href", msg.ResultInfo);
                            //$("#Attachment").html(msg.ResultInfo2);
                            $("#UploadPictureDialog").modal("hide");
                            var attachments = $("#Attachment").val();
                            var type = $("#AttachmentType").val();
                            if (attachments == "") {
                                attachments = type + "&" + msg.ResultInfo + "&" + msg.ResultInfo2;
                            }
                            else {
                                attachments = attachments + "," + type + "&" + msg.ResultInfo + "&" + msg.ResultInfo2;
                            }

                            $("#attachments")[0].innerHTML += "<span>" + type + "&nbsp;&nbsp;<a href='" + msg.ResultInfo + "' target='_blank'>" + msg.ResultInfo2 + "</a><button class='closes' type='button' onclick='removeAttachment(this)'>×</button></br></span>"
                            $("#Attachment").val(attachments);
                            break;
                    }
                }
            });
        });
        //选择7个段
        $("#btnSaveSeven").on("click", function () {
            var trIndex1 = $("#hidbtnIndex").val();
            var companySection = $("#CompanySection").val();
            var companySectionName = $("#CompanySection  option:selected").text();
            var subjectSection = $("#hidSubjectSection").val();
            var subjectSectionName = $("#SubjectSection").val();
            var accountSection = $("#AccountSection").val();
            var accountSectionName = $("#AccountSection  option:selected").text();
            var costCenterSection = $("#CostCenterSection").val();
            var costCenterSectionName = $("#CostCenterSection  option:selected").text();
            var spareOneSection = $("#SpareOneSection").val();
            var spareOneSectionName = $("#SpareOneSection  option:selected").text();
            var spareTwoSection = $("#SpareTwoSection").val();
            var spareTwoSectionName = $("#SpareTwoSection  option:selected").text();
            var intercourseSection = $("#IntercourseSection").val();
            var intercourseSectionName = $("#IntercourseSection  option:selected").text();
            var subjectvalue = companySection + "." + subjectSection + "." + accountSection + "." + costCenterSection + "." + spareOneSection + "." + spareTwoSection + "." + intercourseSection
                               + "\n" + companySectionName + "." + subjectSectionName + "." + accountSectionName + "." + costCenterSectionName + "." + spareOneSectionName + "." + spareTwoSectionName + "." + intercourseSectionName
            $("#SubjectName" + trIndex1).val(subjectvalue);
            $("#hideCompanyName").val(companySectionName);
            $("#ShowSevenSubject").modal({ backdrop: "static", keyboard: false });
            $("#ShowSevenSubject").modal("hide");
        });
        //提交
        $("#btnUp").on("click", function () {
            var vguid = $("#VGUID").val();
            var tableIndex = $("#Automatic").val();
            var status = "2";
            if ($("#Status").val() == "2") {
                status = "3";
            }
            layer.load();
            $.ajax({
                url: "/VoucherManageManagement/VoucherList/UpdataVoucherListInfo",
                data: { vguids: vguid, status: status, index: tableIndex },
                traditional: true,
                type: "post",
                success: function (msg) {
                    layer.closeAll('loading');
                    switch (msg.Status) {
                        case "0":
                            jqxNotification("提交失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("提交成功！", null, "success");
                            history.go(-1);
                            break;
                        case "2":
                            jqxNotification(msg.ResultInfo + "条凭证借贷不相平,提交失败！", null, "error");
                            break;
                    }
                }
            });
        });
        //打印
        $("#btnPrint").on("click", function () {
            var vguid = $("#VGUID").val();
            layer.load();
            $.ajax({
                url: "/VoucherManageManagement/VoucherListDetail/PrintVoucherList",
                data: { vguids: vguid },
                async: false,
                type: "post",
                success: function (msg) {
                    layer.closeAll('loading');
                    if (msg.ResultInfo != null) {
                        window.open(msg.ResultInfo);
                    } 
                }
            });
        });
        //获取借贷配置
        $("#GetSetting").on("click", function () {
            WindowConfirmDialog(getSettingFun, "您确定要覆盖已有的借贷数据嘛？", "确认框", "确定", "取消", "");
        });
    }; //addEvent end

    function addVoucherListTable() {
        voucherHtmls = "<table id='VoucherTable' style='width:100%;white-space:pre;' border='1' cellspacing='0'>" +
                        "<tr id='headTr' style='height:40px'>" +
                             "<td style='width: 250px;text-align: center;font-size: 18px;'>摘要</td>" +
                             "<td style='text-align: center;font-size: 18px;'>科目及描述</td>" +
                             "<td style='width: 150px;text-align: center;font-size: 18px;'>借方金额</td>" +
                             "<td style='width: 150px;text-align: center;font-size: 18px;'>贷方金额</td>" +
                             "<td style='width: 50px;text-align: center;font-size: 24px;'>×</td>" +
                       "</tr>" +
                       "<tr id='closeTr0' style='height:60px'>" +
                             "<td style='text-align: center;'><textarea id='Remark0' style='width: 248px;' class='input_text form-control'></textarea></td>" +
                             "<td style='text-align: center;'><textarea id='SubjectName0' readonly='readonly' class='subjectArea' style='width: 875px; height: 58px;text-indent: 15px;'></textarea> <button id='btnEdit0' type='button' class='buttons subjectbtn' style=''>编辑</button></td>" +
                             "<td style='text-align: right;'><input id='Borrow0' type='text' style='width: 150px;text-align: right' class='input_text form-control money Borrow' /></td>" +
                             "<td style='text-align: right;'><input id='Loan0' type='text' style='width: 150px;text-align: right' class='input_text form-control money Loan' /></td>" +
                             "<td style='text-align: center;'></td>" +
                      "</tr>" +
                       "<tr id='closeTr1' style='height:60px'>" +
                             "<td style='text-align: center;'><textarea id='Remark1' style='width: 248px;' class='input_text form-control'></textarea></td>" +
                             "<td style='text-align: center;'><textarea id='SubjectName1' readonly='readonly' class='subjectArea' style='width: 875px; height: 58px;text-indent: 15px;'></textarea> <button id='btnEdit1' type='button' class='buttons subjectbtn' style=''>编辑</button></td>" +
                             "<td style='text-align: right;'><input id='Borrow1' type='text' style='width: 150px;text-align: right' class='input_text form-control money Borrow' /></td>" +
                             "<td style='text-align: right;'><input id='Loan1' type='text' style='width: 150px;text-align: right' class='input_text form-control money Loan' /></td>" +
                             "<td style='text-align: center;'></td>" +
                      "</tr>" +
                      "<tr id='countTr' style='height:40px'>" +
                              "<td style='text-align: center;'>合计</td>" +
                              "<td style='text-align: center;'></td>" +
                              "<td style='text-align: right;'><input id='BorrowCount' type='text' style='width: 150px;text-align: right' class='input_text form-control' readonly /></td></td>" +
                              "<td style='text-align: right;'><input id='LoanCount' type='text' style='width: 150px;text-align: right' class='input_text form-control' readonly/></td>" +
                             "<td style='text-align: center;'></td>" +
                       "</tr>"
        "</table>";
        $("#VoucherListTable").append(voucherHtmls);
        tdClick();
    }

    function loadSelectFun(str) {

        var id1 = "#AccountSection";
        var id2 = "#CostCenterSection";
        var id3 = "#SpareOneSection";
        var id4 = "#SpareTwoSection";
        var id5 = "#IntercourseSection";

        uiEngineHelper.bindSelect(id1, AccountSection, "Code", "Descrption");
        uiEngineHelper.bindSelect(id2, CostCenterSection, "Code", "Descrption");
        uiEngineHelper.bindSelect(id3, SpareOneSection, "Code", "Descrption");
        uiEngineHelper.bindSelect(id4, SpareTwoSection, "Code", "Descrption");
        uiEngineHelper.bindSelect(id5, IntercourseSection, "Code", "Descrption");
    }

    function getVoucherDetail() {
        $.ajax({
            url: "/VoucherManageManagement/VoucherListDetail/GetVoucherDetail",
            data: {
                "vguid": $("#VGUID").val(),
            },
            type: "post",
            dataType: "json",
            success: function (msg) {
                $("#VGUID").val(msg.VGUID);
                $("#Status").val(msg.Status);
                if ($("#Status").val() == "1") {
                    $("#hideButton").show();
                    $("#btnUp").show();
                    $("#AttachmentHide").show();
                    $("#GetSetting").show();
                    $("#AddNewBankData_GoBackBtn").hide();
                } else {
                    $("#btnSave").hide();
                    $("#btnUp").hide();
                    $("#AddNewBankData_GoBackBtn").hide();
                }
                if ($("#Status").val() == "2") {
                    $("#btnUp").show();
                    $("#btnUp").text("审核");
                    $("#AddNewBankData_GoBackBtn").show();

                }
                if ($("#Status").val() == "4") {
                    $("#hideButton").show();
                    $("#btnUp").show();
                    $("#btnSave").show();
                    $("#AddNewBankData_GoBackBtn").hide();
                }
                var voucherDate = parseInt(msg.VoucherDate.replace(/[^0-9]/ig, ""));//转时间戳
                var accountingPeriod = parseInt(msg.AccountingPeriod.replace(/[^0-9]/ig, ""));//转时间戳
                $("#VoucherDate").val($.convert.toDate(new Date(voucherDate), "yyyy-MM-dd"));
                $("#AccountingPeriod").val($.convert.toDate(new Date(accountingPeriod), "yyyy-MM"));
                $("#BatchName").val(msg.BatchName);
                $("#Currency").val(msg.Currency);
                $("#VoucherNo").val(msg.VoucherNo);
                $("#FinanceDirector").val(msg.FinanceDirector);
                $("#Bookkeeping").val(msg.Bookkeeping);
                $("#DocumentMaker").val(msg.DocumentMaker);
                $("#Auditor").val(msg.Auditor);
                $("#Cashier").val(msg.Cashier);
                $("#CompanyCode").val(msg.CompanyCode);
                $("#AccountModeName").val(msg.AccountModeName);
                $("#VoucherType").val(msg.VoucherType);
                $("#Automatic").val(msg.Automatic);
                var datas = msg.Detail;
                createTable(datas);
                subjectName0 = $("#SubjectName0").val();
                subjectName1 = $("#SubjectName1").val();
                loadAttachments(msg.Attachment);
            }
        });
    }

    function createTable(datas) {
        for (var i = 0; i < datas.length; i++) {
            if (datas[i].Abstract == null || datas[i].Abstract == "null") {
                datas[i].Abstract = "";
                $("#Remark" + i).val("");
            } else {
                $("#Remark" + i).val(datas[i].Abstract);
            }
            $("#SubjectName" + i).val(datas[i].SevenSubjectName);
            var borrowMoney = "";
            var loanMoney = "";
            //多余2条,绑定额外的tr
            if (i > 1) {
                var trMore = "";
                trIndex = i;
                trMore += "<tr id='closeTr" + trIndex + "' style='height:60px'>" +
                                 "<td style='text-align: center;'><textarea id='Remark" + trIndex + "' type='text' style='width: 248px;' class='input_text form-control'>" + datas[i].Abstract + "</textarea></td>" +
                                 "<td style='text-align: center;'><textarea id='SubjectName" + trIndex + "' readonly='readonly' class='subjectArea' style='width: 875px; height: 58px;text-indent: 15px;'>" + datas[i].SevenSubjectName + "</textarea> <button id='btnEdit" + trIndex + "' type='button' class='buttons subjectbtn' style=' '>编辑</button></td>" +
                                 "<td style='text-align: right;'><input id='Borrow" + trIndex + "' type='text' value='" + borrowMoney + "' style='width: 150px;text-align: right' class='input_text form-control money Borrow'/></td>" +
                                 "<td style='text-align: right;'><input id='Loan" + trIndex + "' type='text' value='" + loanMoney + "' style='width: 150px;text-align: right' class='input_text form-control money Loan'/></td>" +
                                 "<td style='text-align: center;'><button id='Remove" + trIndex + "' type='button' onclick='removeSubjectTr(this)'>×</button></td>" +
                          "</tr>"
                $("#countTr").before(trMore);
                if (borrowMoney == "" || borrowMoney == null) {
                    $("#Borrow" + i).attr("readonly", "readonly");
                } else {
                    $("#Loan" + i).attr("readonly", "readonly");
                }
                //tdClick();
            }
            if (datas[i].BorrowMoney === "" || datas[i].BorrowMoney === null || datas[i].BorrowMoney == 0) {
                loanMoney = datas[i].LoanMoney;
                if ((loanMoney != null && loanMoney != "") || loanMoney == 0) {
                    loanMoney = parseFloat(loanMoney).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,');
                    if (datas.length == 1) {
                        //保持借在第一行,贷在下面
                        $("#Remark" + i).val("");
                        $("#SubjectName" + i).val("");
                        $("#Loan1").val(loanMoney);
                        $("#Borrow1").val("");
                        $("#Borrow1").attr("readonly", "readonly");
                        $("#Remark1").val(datas[i].Abstract);
                        $("#SubjectName1").val(datas[i].SevenSubjectName);
                    } else {
                        $("#Loan" + i).val(loanMoney);
                        $("#Borrow" + i).val("");
                        $("#Borrow" + i).attr("readonly", "readonly");
                    }
                } else {
                    borrowMoney = parseFloat(0).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,');
                    $("#Borrow" + i).val(borrowMoney);
                    $("#Loan" + i).val("");
                    $("#Loan" + i).attr("readonly", "readonly");
                }
            } else {
                borrowMoney = datas[i].BorrowMoney;;
                if (borrowMoney != null) {
                    borrowMoney = parseFloat(borrowMoney).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,');
                    $("#Borrow" + i).val(borrowMoney);
                    $("#Loan" + i).val("");
                    $("#Loan" + i).attr("readonly", "readonly");
                }
            }
            var bCount = "";
            if (datas[i].BorrowMoneyCount != null) {
                bCount = datas[i].BorrowMoneyCount;
                $("#BorrowCount").val(parseFloat(bCount).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,'));
            }
            //bCount = bCount.toString().replace(/,/g, '');

            var lCount = "";
            if (datas[i].LoanMoneyCount != null) {
                lCount = datas[i].LoanMoneyCount;
                $("#LoanCount").val(parseFloat(lCount).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,'));
            }
            //lCount = lCount.toString().replace(/,/g, '');           
        }
        tdClick();
    }

    function getPersonInfo() {
        $.ajax({
            url: "/VoucherManageManagement/VoucherListDetail/GetPersonInfo",
            data: {

            },
            traditional: true,
            type: "post",
            success: function (msg) {
                //$("#FinanceDirector").val(msg.FinanceDirector);
                //$("#Bookkeeping").val(msg.Bookkeeping);
                //$("#Auditor").val(msg.Auditor);
                //$("#Cashier").val(msg.Cashier);
                for (var i = 0; i < msg.length; i++) {
                    switch (msg[i].Role) {
                        case "财务经理": $("#FinanceDirector").val(msg[i].LoginName); break;
                        case "财务主管": $("#Bookkeeping").val(msg[i].LoginName); break;
                        //case "审核岗": $("#Auditor").val(msg[i].LoginName); break;
                        case "出纳": $("#Cashier").val(msg[i].LoginName); break;
                        default: break;
                    }
                }
            }
        });
    }

    function getSettingFun() {
        var vguid = $("#VGUID").val();
        layer.load();
        $.ajax({
            url: "/VoucherManageManagement/VoucherListDetail/GetSettingData",
            data: { vguids: vguid },
            //async: false,
            type: "post",
            success: function (msg) {
                layer.closeAll('loading');
                if (msg.Status == "1") {
                    //jqxNotification("获取借贷配置成功！", null, "success");
                    location.reload();
                }
                else if (msg.Status == "2") {
                    jqxNotification("账号下不存在配置！", null, "error");
                }
            }
        });
    }
};

function loadCompanyCode(name, companyCode, subjectCode) {
    var url = "/VoucherManageManagement/VoucherListDetail/GetSelectSection";
    var value = null;
    $.ajax({
        url: url,
        async: false,
        data: { name: name, companyCode: companyCode, subjectCode: subjectCode },
        type: "post",
        success: function (result) {
            value = result;
        }
    });
    return value;
}

function autoHeight(index) {
    if (index == 0) {
        var length = $(".nav-i").length;
        if (length == 1) {
            $('#jqxTabs').jqxTabs({ height: 300 });//1个
        }
        if (length >= 2) {
            $('#jqxTabs').jqxTabs({ height: 500 });//2个
        }
    }
    else {
        var length2 = $(".nav-i2").length;
        if (length2 == 1) {
            $('#jqxTabs').jqxTabs({ height: 300 });//1个
        }
        if (length2 >= 2) {
            $('#jqxTabs').jqxTabs({ height: 500 });//2个
        }
    }
}

$(function () {
    var buttonText = {
        browseButton: '上传',
        uploadButton: '提交',
        cancelButton: '清空',
        uploadFileTooltip: '上传',
        cancelFileTooltip: '删除'
    };
    $('#btn_Attachment').jqxFileUpload({ width: '600px', height: '', fileInputName: 'AttachmentFile', browseTemplate: 'success', uploadTemplate: 'primary', cancelTemplate: 'danger', localization: buttonText, multipleFilesUpload: true });
    $("#btn_Attachment").on("select", function (event) {
        if (event.args.size > (1024 * 1024 * 10)) {
            jqxAlert("单文件大小不能超过10M");
            $("#btn_AttachmentCancelButton").trigger('click');
        }
    });
    $("#btn_Attachment").on("uploadStart", function (event) {
        //获取文件名
        fileName = event.args.file;
        var extStart = fileName.lastIndexOf(".");
        //判断是文件还是图片
        var ext = fileName.substring(extStart, fileName.length).toUpperCase();
        if (ext != ".BMP" && ext != ".PNG" && ext != ".GIF" && ext != ".JPG" && ext != ".JPEG") {//上传文件
            $('#btn_Attachment').jqxFileUpload({ uploadUrl: '/File/UploadFile?allowSize=' + 20 });
        }
        else {//上传图片
            $('#btn_Attachment').jqxFileUpload({ uploadUrl: '/File/UploadImage?allowSize=' + 20 });
        }
    })
    $("#btn_Attachment").on("uploadEnd", function (event) {
        var args = event.args;
        //var msg = $.convert.strToJson($(args.response).html());
        uploadFiles(event)
    })
})
var fileName = "";
function uploadFiles(event) {
    var args = event.args;
    var msg = $.convert.strToJson($(args.response).html());
    fileName = event.args.file;
    var attachments = $("#Attachment").val();
    var type = $("#AttachmentType").val();
    if (attachments == "") {
        attachments = type + "&" + msg.WebPath + "&" + fileName;
    }
    else {
        attachments = attachments + "," + type + "&" + msg.WebPath + "&" + fileName;
    }

    $("#attachments")[0].innerHTML += "<span>" + type + "&nbsp;&nbsp;<a href='" + msg.WebPath + "' target='_blank'>" + fileName + "</a><button class='closes' type='button' onclick='removeAttachment(this)'>×</button>&nbsp;&nbsp;</span>"
    $("#Attachment").val(attachments);
}
function loadAttachments(attachments) {
    $("#Attachment").val(attachments);
    if (attachments != "" && attachments != null) {
        var attachValues = attachments.split(",");
        for (var i = 0; i < attachValues.length; i++) {
            var attach = attachValues[i].split("&");
            $("#attachments")[0].innerHTML += "<span>" + attach[0] + "&nbsp;&nbsp;<a href='" + attach[1] + "' target='_blank'>" + attach[2] + "</a><button class='closes' type='button' onclick='removeAttachment(this)'>×</button>&nbsp;&nbsp;</span>"
        }
    }
}
function removeAttachment(obj) {
    var id = obj.previousSibling.attributes["href"].value;
    var type = obj.parentElement.textContent.trim().split(/\s+/)[0];//按空格拆分字符串
    var name = obj.parentElement.textContent.trim().split(/\s+/)[1].substring(0, obj.parentElement.textContent.trim().split(/\s+/)[1].length - 1);
    var replaceStr = type + "&" + id + "&" + name;
    var attachmentValues = $("#Attachment").val();
    var vall = attachmentValues.replace(replaceStr, "");
    //console.log(vall);
    //attachmen
    $("#Attachment").val(vall);
    $(obj).parent().remove();
    return false;
}

$(function () {
    var page = new $page();
    page.init();
});

//选择科目段
function searchSubject(event) {
    //var str = event.id;
    //var x = str.split("_")[1];
    //var y = str.split("_")[2];
    //var companyCode = $("#CompanySection_" + x + "_" + y).val();
    //if (companyCode == "") {
    //    jqxNotification("请选择公司段", null, "error");
    //    return;
    //}
    //z1 = "";
    //z2 = "";
    //initSubjectTable(companyCode, x, y);
    var companyCode = $("#CompanySection").val();
    if (companyCode == "") {
        jqxNotification("请选择公司段", null, "error");
        return;
    }
    initSubjectTable(companyCode);
    $("#AddCompanyDialog").modal({ backdrop: "static", keyboard: false });
    $("#AddCompanyDialog").modal("show");

    $('#jqxSubjectSection').on('bindingComplete', function (event) {
        $("#jqxSubjectSection").jqxTreeGrid('expandAll');
    });
}
let z1 = "";
let z2 = "";
function initSubjectTable(companyCode, x, y) {
    z1 = x;
    z2 = y;
    var source = {
        datafields:
        [
             { name: 'Code', type: 'string' },
             { name: 'ParentCode', type: 'string' },
             { name: 'Descrption', type: 'string' },
             { name: 'SectionVGUID', type: 'string' },
             { name: 'VGUID', type: 'string' },
             { name: 'Status', type: 'string' },
             { name: 'Remark', type: 'string' },
             { name: 'IsAccountingCode', type: 'string' },
             { name: 'IsCostCenterCode', type: 'string' },
             { name: 'IsSpareOneCode', type: 'string' },
             { name: 'IsSpareTwoCode', type: 'string' },
             { name: 'IsIntercourseCode', type: 'string' },
        ],
        hierarchy:
        {
            keyDataField: { name: 'Code' },
            parentDataField: { name: 'ParentCode' }
        },
        datatype: "json",
        id: "",
        data: { companyCode: companyCode, accountModeCode: loginAccountModeCode },
        url: "/PaymentManagement/SubjectSection/GetCompanySectionByCode"    //获取数据源的路径
    };
    var typeAdapter = new $.jqx.dataAdapter(source);
    $("#jqxSubjectSection").jqxTreeGrid({
        pageable: false,
        width: 460,
        height: 300,
        pageSize: 9999999,
        //serverProcessing: true,
        pagerButtonsCount: 10,
        source: typeAdapter,
        //theme: "energyblue",
        filterable: true,
        columnsHeight: 30,
        checkboxes: false,
        hierarchicalCheckboxes: false,
        //ready: function () {
        //    $("#jqxSubjectSection").jqxTreeGrid('expandAll');
        //},
        columns: [
            { text: '编码', datafield: 'Code', width: 150, align: 'center', cellsAlign: 'left' },
            { text: '描述', datafield: 'Descrption', align: 'center', cellsAlign: 'center' },
            { text: 'ParentCode', datafield: 'ParentCode', hidden: true, filterable: false, },
            { text: 'SectionVGUID', datafield: 'SectionVGUID', hidden: true, filterable: false },
            { text: 'VGUID', datafield: 'VGUID', hidden: true, filterable: false },
        ]
    });
}
//拍照
function openSocket() {
    socket = new WebSocket(baseUrl);
    socket.onclose = function () {
        console.error("web channel closed");
    };
    socket.onerror = function (error) {
        console.error("web channel error: " + error);
    };
    socket.onopen = function () {
        new QWebChannel(socket, function (channel) {
            // make dialog object accessible globally
            window.dialog = channel.objects.dialog;
            //dialog.set_configValue("set_savePath:D:\\img");
            //网页关闭函数
            window.onbeforeunload = function () {
                dialog.get_actionType("closeSignal");
            }
            window.onunload = function () {
                dialog.get_actionType("closeSignal");
            }
            //拍照按钮点击
            document.getElementById("photographPri").onclick = function () {
                dialog.photoBtnClicked("primaryDev_");
                dialog.get_actionType("savePhotoPriDev");
            };
            //纠偏裁边
            document.getElementById("setdeskew").onclick = function () {
                dialog.get_actionType("setdeskew");
            };
            //左转
            document.getElementById("rotateLeft").onclick = function () {
                dialog.get_actionType("rotateLeft");
            };
            //右转
            document.getElementById("rotateRight").onclick = function () {
                dialog.get_actionType("rotateRight");
            };
            //属性设置
            document.getElementById("showProperty").onclick = function () {
                dialog.get_actionType("showProperty");
            };
            //服务器返回消息
            dialog.sendPrintInfo.connect(function (message) {
                //设备分辨率
                message = message.substr(14);
                //图片保存后返回路径关键字savePhoto_success:
                if (message.indexOf("savePhoto_success:") >= 0) {
                    imgPath = message.substr(18);
                }
            });
            //接收图片流用来展示，多个，较小的base64数据
            dialog.send_priImgData.connect(function (message) {
                var element = document.getElementById("bigPriDev");
                element.src = "data:image/jpg;base64," + message;
            });
            //接收拍照base64
            dialog.send_priPhotoData.connect(function (message) {
                var element = document.getElementById("devPhoto");
                element.src = "data:image/jpg;base64," + message;
            });
            //网页加载完成信号
            dialog.html_loaded("one");
        });
    }
}

function removeSubjectTr(obj) {
    WindowConfirmDialog(deleTr, "您确定要删除当前行？", "确认框", "确定", "取消", obj);
}
function deleTr(obj) {
    var trIndexs = 0;
    if (obj.id.length == 8) {
        trIndexs = obj.id.substr(obj.id.length - 2, 2);//获取下标
    } else {
        trIndexs = obj.id.substr(obj.id.length - 1, 1);//获取下标
    }
    $("#closeTr" + trIndexs).remove();
    countMoney();
}
function tdClick() {
    $(".money").blur(function (event) {
        var id = event.target.id;
        var trIndexs = 0;
        var idNmae = "";
        if (id.length == 8 || id.length == 6) {
            trIndexs = id.substr(id.length - 2, 2);//获取下标
            idNmae = id.substr(0, id.length - 2);
        } else {
            trIndexs = id.substr(id.length - 1, 1);//获取下标
            idNmae = id.substr(0, id.length - 1);
        }
        var value = $("#" + id).val();
        if (value != "") {
            if (idNmae == "Borrow") {
                $("#Loan" + trIndexs).attr("readonly", "readonly");
                value = value.replace(/,/g, '');
                $("#Borrow" + trIndexs).val(parseFloat(value).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,'));
            } else {
                $("#Borrow" + trIndexs).attr("readonly", "readonly");
                value = value.replace(/,/g, '');
                $("#Loan" + trIndexs).val(parseFloat(value).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,'));
            }
        } else {
            if (idNmae == "Borrow") {
                if ($("#Loan" + trIndexs).val() != "") {
                    $("#Borrow" + trIndexs).attr("readonly", "readonly");
                    $("#Loan" + trIndexs).removeAttr("readonly");
                } else {
                    $("#Borrow" + trIndexs).removeAttr("readonly");
                    $("#Loan" + trIndexs).removeAttr("readonly");
                }
            } else {
                if ($("#Borrow" + trIndexs).val() != "") {
                    $("#Loan" + trIndexs).attr("readonly", "readonly");
                    $("#Borrow" + trIndexs).removeAttr("readonly");
                } else {
                    $("#Borrow" + trIndexs).removeAttr("readonly");
                    $("#Loan" + trIndexs).removeAttr("readonly");
                }
            }
        }
        countMoney();
    });
    $(".subjectbtn").click(function (event) {
        var id = event.target.id;
        var trIndexs = 0;
        $("#CompanySection").val($("#LoginCompanyCode").val());
        if (id.length == 9) {
            trIndexs = id.substr(id.length - 2, 2);//获取下标
        } else {
            trIndexs = id.substr(id.length - 1, 1);//获取下标
        }
        var subjectVal = $("#SubjectName" + trIndexs).val();
        if (subjectVal != "") {

        }
        $("#ShowSevenSubject").modal({ backdrop: "static", keyboard: false });
        $("#ShowSevenSubject").modal("show");
        $("#hidbtnIndex").val(trIndexs);
    })
    countMoney();
}
function countMoney() {
    var borrowCount = 0;
    var loanCount = 0;
    for (var i = 0; i < $(".Borrow").length; i++) {
        if ($(".Borrow")[i].value != "") {
            var valB = $(".Borrow")[i].value.replace(/,/g, '');
            borrowCount += parseFloat(valB);
        }
    }
    for (var i = 0; i < $(".Loan").length; i++) {
        if ($(".Loan")[i].value != "") {
            var valL = $(".Loan")[i].value.replace(/,/g, '');
            loanCount += parseFloat(valL);
        }
    }
    $("#BorrowCount").val(parseFloat(borrowCount).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,'));
    $("#LoanCount").val(parseFloat(loanCount).toFixed(2).replace(/\d{1,3}(?=(\d{3})+(\.\d*)?$)/g, '$&,'));
}
