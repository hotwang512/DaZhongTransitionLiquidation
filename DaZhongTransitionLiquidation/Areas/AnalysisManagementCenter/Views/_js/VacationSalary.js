var vguid = "";
var mydate = new Date();
var ManagementCompany = [];
var dataArr = [];
var $page = function () {
    this.init = function () {
        //$("#DateOfYear").attr("disabled",true);
        if ($("#EditPermission").val() == "True" || $("#NewPermission").val() == "True") {
            $("#VacationSalaryDialog_OKBtn").show();
        } else {
            $("#VacationSalaryDialog_OKBtn").hide();
        }
        addEvent();
    }
    //所有事件
    function addEvent() {
        //保存
        $("#btnNew").on("click",
            function () {
                var date = new Date();
                $("#DateOfYear").val(date.getFullYear());
                //$("#VacationTypeNew").val("");
                //GetVacationSalary();
                $("#VacationSalaryModalDialog").modal("show");
            });
        $("#VacationType").on("change",
            function () {
                GetVacationSalaryYear();
            });
        $("#VacationTypeNew").on("change",
            function () {
                GetVacationSalary();
            });
        $("#DateOfYear").on("blur",
            function () {
                GetVacationSalary();
            });
        $("#VacationSalaryDialog_OKBtn").on("click", function () {
            var validateError = 0;
            if (!Validate($("#VacationTypeNew"))) {
                validateError++;
            }
            if (!Validate($("#DateOfYear"))) {
                validateError++;
            }
            if (validateError <= 0) {
                var rows = $('#table').jqxGrid('getboundrows');
                var SettingList = [];
                for (var j = 0; j < rows.length; j++) {
                    SettingList.push({
                        "VGUID": rows[j].VGUID, "VacationType": rows[j].VacationType,
                        "Salary": rows[j].Salary, "Zorder": rows[j].Zorder,
                        "ProjectName": rows[j].ProjectName, "DateOfYear": rows[j].DateOfYear
                    });
                };
                $.ajax({
                    url: "/AnalysisManagementCenter/VacationSalary/SaveVacationSalary",
                    data: {
                        "VacationSalaryList": SettingList
                    },
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                $("#VacationSalaryModalDialog").modal("hide");
                                GetVacationSalaryYear();
                                //window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                                break;
                        }
                    }
                });
            }
        });
        $("#VacationSalaryDialog_CancelBtn").on("click", function () {
            $("#VacationSalaryModalDialog").modal("hide");
        });
    }; //addEvent end
    function GetVacationSalary() {
        if ($("#VacationTypeNew").val() != "") {
            $.ajax({
                url: "/AnalysisManagementCenter/VacationSalary/GetVacationSalaryDetail",
                data: {
                    "VacationTypeID": $("#VacationTypeNew").val(),
                    "VacationType": $("#VacationTypeNew option:selected").text(),
                    "DateOfYear": $("#DateOfYear").val()
                },
                type: "GET",
                dataType: "json",
                async: false,
                success: function (data) {
                    GetVacationSalaryDetail(data);
                }
            });
        }
    }
   
    function GetVacationSalaryYear() {
        $.ajax({
            url: "/AnalysisManagementCenter/VacationSalary/GetVacationSalaryYear",
            data: { "VacationType": $("#VacationType").val() },
            type: "GET",
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.length > 0) {
                    $("#tableList").show();
                    dataArr = data;
                    GetVacationSalaryList(data);
                } else {
                    $("#tableList").hide();
                }
            }
        });
    }
    function GetVacationSalaryDetail(data) {
        var ordersSource =
        {
            dataFields: [
                { name: 'VacationType', type: 'string' },
                { name: 'ProjectName', type: 'string' },
                { name: 'Salary', type: 'string' },
                { name: 'DateOfYear', type: 'string' },
                { name: 'Zorder', type: 'string' },
                { name: 'VGUID', type: 'string' }
            ],
            dataType: "json",
            id: 'VGUID',
            localdata: data,
            updateRow: function (rowID, rowData, commit) {
                commit(true);
            }
        };
        var dataAdapter = new $.jqx.dataAdapter(ordersSource, {
            loadComplete: function () {
            }
        });
        $("#table").jqxGrid(
        {
            width: "100%",
            height: "300px",
            source: dataAdapter,
            selectionmode: 'singlerow',
            editable: true,
            ready: function () {
            },
            toolbarHeight: 35,
            columns: [
                { text: '工资类型', editable: false, dataField: 'VacationType', width: 100, cellsAlign: 'center', align: 'center' },
                { text: '项目', editable: false, dataField: 'ProjectName', width: 157, cellsAlign: 'center', align: 'center' },
                {
                    text: '工资',
                    editable: true,
                    dataField: 'Salary',
                    width: 430,
                    cellsAlign: 'center',
                    align: 'center',
                    cellsformat: 'F2',
                    createeditor: function (row, cellvalue, editor) {
                        editor.jqxNumberInput({ digits: 3 });
                    }
                },
                { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ]
        });
    }
    function GetVacationSalaryList(data) {
        debugger;
        var datafields = [
            { name: 'ProjectName', type: 'string' }
        ];
        var columns = [
            {
                text: '项目',
                editable: false,
                dataField: 'ProjectName',
                width: 200,
                cellsAlign: 'center',
                align: 'center'
            }
        ];
        for (var i = 0; i < data.length; i++) {
            datafields.push({ name: data[i], type: 'stirng' });
            columns.push({ text: data[i] + "年", datafield: data[i], editable: false, width: 100, align: 'center', cellsAlign: 'center' });
        }
        debugger;
        var paras = "";
        for (var k = 0; k < data.length; k++) {
            paras += "[" + data[k] + "],";
        }
        paras = paras.substring(0, paras.length - 1);
        var url = "/AnalysisManagementCenter/VacationSalary/GetVacationSalaryList";
        var ordersSource =
        {
            dataFields: datafields,
            dataType: "json",
            id: 'VGUID',
            url: url,
            data: { "VacationType": $("#VacationType").val(), paras: paras},
            updateRow: function (rowID, rowData, commit) {
                commit(true);
            }
        };
        var dataAdapter = new $.jqx.dataAdapter(ordersSource, {
            loadComplete: function () {
                
            }
        });
        $("#tableList").jqxGrid(
            {
                width: "100%",
                height: "100%",
                source: dataAdapter,
                selectionmode: 'singlerow',
                editable: true,
                editmode: 'selectedcell',
                ready: function () {
                },
                columns: columns
            });
    }
};
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