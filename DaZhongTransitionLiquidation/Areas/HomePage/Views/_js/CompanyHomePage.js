var selector = {

}
var $page = function () {
    this.init = function () {
        $(function () {
            //GetCompanyCode();
            initTable1();
            $("#OKButton").on("click", function () {
                if ($("#AccountModeCode").val() == "") {
                    jqxNotification("请选择账套公司!", null, "error");
                    return;
                }
                $.ajax({
                    url: "/HomePage/CompanyHomePage/SaveUserInfo",
                    data: {
                        ComapnyCode: $("#CompanyCode").val(),
                        AccountModeCode: $("#AccountModeCode").val(),
                        CompanyName: $("#CompanyName").val(),
                        AccountModeName: $("#AccountModeName").val(),
                    },
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

            $('#jqxTable1').on('rowselect', function (event) {
                var args = event.args;
                var rowData = args.row;
                $("#CompanyCode").val(rowData.CompanyCode);
                $("#AccountModeCode").val(rowData.Code);
                $("#CompanyName").val(rowData.CompanyName);
                $("#AccountModeName").val(rowData.Descrption);
            });

            $("#jqxTable1").on("bindingcomplete", function (event) {
                //setTimeout(function () { $(".jqx-grid-groups-row").children("span").eq(0).text(""); }, 100);
                //$(".jqx-grid-cell-pinned-office").css('display', 'none');

                //$(".jqx-grid-cell-pinned-office").attr('class', 'hide');
                //$(".jqx-grid-group-cell-office").css('left', '15px');
            });
        })
    }
}

$(function () {
    var page = new $page();
    page.init();
})

function GetCompanyCode() {
    var accountMode = $("#AccountModeCode").val();
    $.ajax({
        url: "/HomePage/CompanyHomePage/GetCompanyCode",
        data: { accountMode: accountMode },
        type: "POST",
        dataType: "json",
        success: function (msg) {
            uiEngineHelper.bindSelect('#CompanyCode', msg, "CompanyCode", "CompanyName");
            //$("#CompanyCode").prepend("<option value=\"\" selected='true'>请选择</>");
        }
    });
}

//公司配置
function initTable1() {
    var source =
        {
            datafields:
            [
                { name: 'Code', type: 'string' },
                { name: 'Descrption', type: 'string' },
                { name: 'CompanyCode', type: 'string' },
                { name: 'CompanyName', type: 'string' },
                { name: 'IsCheck', type: 'bool' },
                { name: 'KeyData', type: 'string' },
                //{ name: 'Block', type: 'string' },
                { name: 'UserVGUID', type: 'string' },
            ],
            datatype: "json",
            id: "KeyData",
            data: { },
            url: "/HomePage/CompanyHomePage/GetUserCompanyInfo"   //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source);

    //创建卡信息列表（主表）
    $("#jqxTable1").jqxGrid(
        {
            pageable: false,
            width: "100%",
            autoheight: true,
            //height:500,
            pageSize: 10,
            //serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            groupable: true,
            groupsexpandedbydefault: true,
            groups: ['Descrption'],
            showgroupsheader: false,
            //showgroupmenuitems: false,
            columnsHeight: 30,
            pagermode: 'simple',
            selectionmode: 'singlerow',
            showHeader: false,
            columns: [
                //{ text: "", datafield: "checkbox", width: 40, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                {
                    text: '选择', datafield: "IsCheck", width: 60, align: 'center', cellsAlign: 'center', columntype: 'checkbox',hidden:true
                },
                { text: '账套编码', datafield: 'Code', width: 120, align: 'center', cellsAlign: 'center', editable: false, hidden: true },
                { text: '账套', datafield: 'Descrption', width: 250, align: 'center', cellsAlign: 'center', editable: false, hidden: true },
                { text: '公司编码', datafield: 'CompanyCode', width: 150, align: 'center', cellsAlign: 'center', editable: false, hidden: true },
                { text: '公司描述', datafield: 'CompanyName', width: 450, align: 'center', cellsAlign: 'center', editable: false },
                //{
                //    text: '版块', datafield: 'Block', align: 'center', cellsAlign: 'center', columntype: 'dropdownlist',
                //    createeditor: function (row, value, editor) {
                //        editor.jqxDropDownList({ source: countriesAdapter, displayMember: 'label', valueMember: 'value' });
                //    }
                //},
                { text: 'KeyData', datafield: 'SectionVGUID', hidden: true },
            ]
        });
}