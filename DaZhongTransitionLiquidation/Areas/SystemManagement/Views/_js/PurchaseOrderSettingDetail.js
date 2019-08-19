//资产基础信息维护明细
var vguid = "";
var mydate = new Date();
var ManagementCompany = [];
var $page = function () {
    this.init = function () {
        initSelect();
        initSelectMinor();
        initSelectPurchaseDepartment();
        initBusinessProject();
        //initBusinessSubItem();
        GetManagementCompanyData();
        addEvent();
    }
    //所有事件
    function addEvent() {
        var guid = $.request.queryString().VGUID;
        $("#VGUID").val(guid);
        if (guid != "" && guid != null) {
            getOrderSettingInfoListDetail();
        } else {
           // $("#ASSET_CATEGORY_MAJOR").jqxComboBox('clearSelection');
            $("#hideButton").show();
        }
        //取消
        $("#btnCancel").on("click",
            function() {
                history.go(-1);
            });
        //保存
        $("#btnSave").on("click",
            function() {
                var validateError = 0; //未通过验证的数量
                if (!Validate($("#PurchaseGoods"))) {
                    validateError++;
                }
                if (!Validate($("#ASSET_CATEGORY_MAJOR"))) {
                    validateError++;
                }
                if (!Validate($("#ASSET_CATEGORY_MINOR"))) {
                    validateError++;
                }
                var DepartmentModelList = [];
                var items = $("#PurchaseDepartment").jqxDropDownList('getCheckedItems');
                for (var i = 0; i < items.length; i++) {
                    DepartmentModelList.push({ "VGUID": items[i].value, "Descrption": items[i].label });
                };
                var ManagementCompanyList = [];
                var rows = $('#jqxManagementCompanyTable').jqxGrid('getboundrows');
                debugger;
                for (var j = 0; j < rows.length; j++) {
                    ManagementCompanyList.push({ "IsCheck": rows[j].IsCheck, "CompanyCode": rows[j].CompanyCode, "AccountModeCode": rows[j].AccountModeCode, "ManagementCompany": rows[j].ManagementCompany, "Descrption": rows[j].Descrption, "KeyData": rows[j].KeyData, "ManagementCompanyVguid": rows[j].ManagementCompanyVguid });
                };
                debugger;
                if (validateError <= 0) {
                    debugger;
                    $.ajax({
                        url: "/Systemmanagement/PurchaseOrderSettingDetail/SavePurchaseOrderSetting",
                        data: {
                            "VGUID": $("#VGUID").val(),
                            "AssetCategoryMajor": $("#AssetCategoryMajor").val(),
                            "AssetCategoryMinor": $("#AssetCategoryMinor").val(),
                            "OrderCategory": $("#OrderCategory").val(),
                            "AssetCategoryMinorVguid": $("#AssetCategoryMinorVguid").val(),
                            "PurchaseGoods": $("#PurchaseGoods").val(),
                            "BusinessSubItem": $("#BusinessSubItem").text(),
                            "DepartmentModelList": DepartmentModelList,
                            "ManagementCompanyList": ManagementCompanyList
                        },
                        type: "post",
                        success: function (msg) {
                            switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                history.go(-1);
                                window.opener.$("#jqxTable").jqxDataTable('updateBoundData');
                                break;
                            }
                        }
                    });
                }
            });
        $("#ASSET_CATEGORY_MAJOR").on('select', function (event) {
            if (event.args) {
                var item = event.args.item;
                $("#AssetCategoryMajor").val(item.label);
                initSelectMinor();
            }
        });
        $("#ASSET_CATEGORY_MINOR").on('select', function (event) {
            if (event.args) {
                var item = event.args.item;
                $("#AssetCategoryMinor").val(item.label);
                $("#AssetCategoryMinorVguid").val(item.value);
            }
        });
        $("#AssetManagementCompany").on('click', function (event) {
            //if (GetQueryString("VGUID") != null) {
            //    GetManagementCompanyData();
            //}
            $("#ManagementCompanyModalDialog").modal("show");
        });
        $("#btnAddManagementCompany").on('click', function (event) {
            var PurchaseOrderSettingVguid = GetQueryString("VGUID");
            if (PurchaseOrderSettingVguid != null) {
                $.ajax({
                    url: "/Systemmanagement/PurchaseOrderSettingDetail/AddManagementCompany",
                    data: {
                        "PurchaseOrderSettingVguid": GetQueryString("VGUID"),
                        "ManagementCompany": $("#ManagementCompany").val()
                    },
                    type: "post",
                    success: function(msg) {
                        switch (msg.Status) {
                        case "0":
                            jqxNotification("添加失败！", null, "error");
                            break;
                        case "1":
                            jqxNotification("添加成功！", null, "success");
                            GetManagementCompanyData();
                            break;
                        }
                    }
                });
            } else {
                ManagementCompany.push($("#ManagementCompany").val());
                bindManagementCompanyData();
            }
        });
        $("#ManagementCompanyDialog_OKBtn").on("click",
            function (event) {
                $("#ManagementCompanyModalDialog").modal("hide");
            });
        $("#ManagementCompanyDialog_CancelBtn").on("click",
            function (event) {
                $("#ManagementCompanyModalDialog").modal("hide");
            });
    }; //addEvent end
    function cellsDeleteRenderer(row, column, value, rowData) {
        var vguid = rowData.VGUID;
        return '<div style="margin: 8px; margin-top:6px;"><a style="cursor:pointer"  onclick="DeleteManagementCompany(\'' + vguid + '\')" id="' + vguid + '">删除</a></div>';
    }
    function getOrderSettingInfoListDetail() {
        $.ajax({
            url: "/Systemmanagement/PurchaseOrderSettingDetail/GetPurchaseOrderSettingDetail",
            data: {
                "vguid": $("#VGUID").val()
            },
            type: "post",
            async: false,
            dataType: "json",
            success: function (msg) {
                $("#PurchaseGoods").val(msg.ResultInfo.PurchaseGoods);
                $("#OrderCategory").val(msg.ResultInfo.OrderCategory);
                $("#AssetCategoryMinor").val(msg.ResultInfo.AssetCategoryMinor);
                $("#AssetCategoryMajor").val(msg.ResultInfo.AssetCategoryMajor);
                $("#AssetCategoryMinorVguid").val(msg.ResultInfo.AssetCategoryMinorVguid);
                $("#ASSET_CATEGORY_MAJOR").val(msg.ResultInfo.AssetCategoryMajor);
                $("#BusinessSubItem").text(msg.ResultInfo.BusinessSubItem);
                $("#BusinessProject").val(msg.ResultInfo.BusinessSubItem);
                
                initSelectMinor(msg.ResultInfo.AssetCategoryMinorVguid);
                for (var i = 0; i < msg.ResultInfo2.length; i++) {
                    var item = $("#PurchaseDepartment").jqxDropDownList('getItemByValue', msg.ResultInfo2[i].VGUID);
                    $("#PurchaseDepartment").jqxDropDownList('checkItem', item);
                }
            }
        });
    }
    function initSelect() {
        $.ajax({
            url: "/AssetManagement/AssetBasicInfoMaintenance/GetMajorListDatas",
            type: "post",
            async: false,
            success: function (data) {
                var arr = [];
                for (var i = 0; i < data.length; i++) {
                    arr.push(data[i].AssetMajor);
                }
                var dataAdapter = new $.jqx.dataAdapter(arr);
                $("#ASSET_CATEGORY_MAJOR").jqxDropDownList({ selectedIndex: 0, source: dataAdapter, width: 198, height: 33 });
                $("#ASSET_CATEGORY_MAJOR").jqxDropDownList({ itemHeight: 33 });
                $("#AssetCategoryMajor").val($("#ASSET_CATEGORY_MAJOR").val());
                initSelectMinor();
            }
        });

    }
    function initSelectMinor() {
        var source =
        {
            data: {
                "MAJOR": $("#AssetCategoryMajor").val()
            },
            datatype: "json",
            type: "post",
            datafields: [
                { name: 'AssetMinor' },
                { name: 'AssetMinorVguid' }
            ],
            url: "/Systemmanagement/PurchaseOrderSettingDetail/GetMinorListDatas",
            async: false
        };
        var dataAdapter = new $.jqx.dataAdapter(source);
        $("#ASSET_CATEGORY_MINOR").jqxDropDownList({ selectedIndex: 0, source: dataAdapter, displayMember: "AssetMinor", valueMember: "AssetMinorVguid", width: 198, height: 33 });
        $("#ASSET_CATEGORY_MINOR").jqxDropDownList({ itemHeight: 33 });
        if ($("#AssetCategoryMinorVguid").val() != "") {
            $("#ASSET_CATEGORY_MINOR").val($("#AssetCategoryMinorVguid").val());
        } else {
            $("#AssetCategoryMinorVguid").val($("#ASSET_CATEGORY_MINOR").val());
            $("#AssetCategoryMinor").val($("#ASSET_CATEGORY_MINOR").jqxDropDownList('getSelectedItem').label);
        }
    }
    function initSelectPurchaseDepartment() {
        var source =
        {
            datatype: "json",
            type: "post",
            datafields: [
                { name: 'Descrption' },
                { name: 'VGUID' }
            ],
            url: "/Systemmanagement/PurchaseOrderSettingDetail/GetPurchaseDepartmentListDatas",
            async: false
            };
            var dataAdapter = new $.jqx.dataAdapter(source);
            $("#PurchaseDepartment").jqxDropDownList({ checkboxes: true, selectedIndex: 0, source: dataAdapter, displayMember: "Descrption", valueMember: "VGUID",width: 198, height: 33, placeHolder: "请选择" });
            $("#PurchaseDepartment").jqxDropDownList({ itemHeight: 33 });
    }
    function initBusinessProject() {
        var url = "/Systemmanagement/PurchaseOrderSettingDetail/GetBusinessProject";
        // prepare the data
        var source =
        {
            datatype: "json",
            data: {
                "BusinessProject": ""
            },
            datafields: [
                { name: 'BusinessProject' },
                { name: 'BusinessSubItem1' }
            ],
            url: url,
            async: false
        };
        var dataAdapter = new $.jqx.dataAdapter(source);
        $("#BusinessProject").jqxDropDownList({
            selectedIndex: 0,
            filterable: true,
            source: dataAdapter,
            displayMember: "BusinessProject",
            valueMember: "BusinessSubItem1",
            searchMode: 'contains',
            width: 200,
            height: 30
        });
        $("#BusinessProject").on('select', function (event) {
            if (event.args) {
                var item = event.args.item;
                debugger;
                if (item) {
                    $("#BusinessSubItem").text(event.args.item.value);
                }
            }
        });
    }
};
function GetManagementCompanyData_bak() {
    var source =
    {
        datafields:
        [
            { name: 'ManagementCompany', type: 'string' },
            { name: 'VGUID', type: 'string' }
        ],
        datatype: "json",
        id: "VGUID",
        data: { "VGUID": $("#VGUID").val() },
        url: "/Systemmanagement/PurchaseOrderSettingDetail/GetBusiness_PurchaseDepartment"   //获取数据源的路径
    };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    debugger;
    $("#jqxManagementCompanyTable").jqxDataTable(
        {
            pageable: true,
            width: "100%",
            height: 300,
            pageSize: 10,
            serverProcessing: true,
            autoRowHeight: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            //columnsHeight: 30,
            columnsResize: true,
            columns: [
                { text: '资产管理公司', datafield: 'ManagementCompany', width: 338, align: 'center', cellsAlign: 'center' },
                { text: '删除', hidden: false, width: 110, align: 'center', cellsAlign: 'center', cellsRenderer: cellsDeleteRenderer },
                { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ]
        });
}
function GetManagementCompanyData() {
    var source =
        {
            datafields:
            [
                { name: 'AccountModeCode', type: 'string' },
                { name: 'ManagementCompanyVguid', type: 'string' },
                { name: 'Descrption', type: 'string' },
                { name: 'CompanyCode', type: 'string' },
                { name: 'ManagementCompany', type: 'string' },
                { name: 'IsCheck', type: 'bool' },
                { name: 'KeyData', type: 'string' }
            ],
            datatype: "json",
            id: "KeyData",
            data: { VGUID: $.request.queryString().VGUID },
            url: "/Systemmanagement/PurchaseOrderSettingDetail/GetPurchaseManagementCompanyData"   //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source);

    //创建卡信息列表（主表）
    $("#jqxManagementCompanyTable").jqxGrid(
        {
            pageable: true,
            width: "98%",
            height: 380,
            pageSize: 10,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            groupable: true,
            groupsexpandedbydefault: true,
            groups: ['Descrption'],
            showgroupsheader: false,
            columnsHeight: 30,
            editable: true,
            pagermode: 'simple',
            selectionmode: 'none',
            columns: [
                {
                    text: '选择', datafield: "IsCheck", width: 60, align: 'center', cellsAlign: 'center', columntype: 'checkbox'
                },
                { text: '账套编码', datafield: 'AccountModeCode', width: 120, align: 'center', cellsAlign: 'center', editable: false },
                { text: '账套描述', datafield: 'Descrption', width: 250, align: 'center', cellsAlign: 'center', editable: false ,hidden:true},
                { text: '公司编码', datafield: 'CompanyCode', width: 150, align: 'center', cellsAlign: 'center', editable: false },
                { text: '公司描述', datafield: 'ManagementCompany', width: 463, align: 'center', cellsAlign: 'center', editable: false },
                { text: 'ManagementCompanyVguid', datafield: 'ManagementCompanyVguid', width: 463, hidden: true },
                { text: 'KeyData', datafield: 'KeyData', hidden: true }
            ]
        });
}
function cellsDeleteRenderer(row, column, value, rowData) {
    var vguid = rowData.VGUID;
    var managementCompany = rowData.ManagementCompany;
    return '<div style="margin: 8px; margin-top:6px;"><a style="cursor:pointer"  onclick="DeleteManagementCompany(\'' + vguid + '\',\'' + managementCompany + '\')" id="' + vguid + '">删除</a></div>';
}
function DeleteManagementCompany(vguid, managementCompany) {
    var PurchaseOrderSettingVguid = GetQueryString("VGUID");
    if (PurchaseOrderSettingVguid != null) {
        $.ajax({
            url: "/Systemmanagement/PurchaseOrderSettingDetail/DeleteManagementCompany",
            data: {
                "VGUID": vguid
            },
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                case "0":
                    jqxNotification("删除失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("删除成功！", null, "success");
                    GetManagementCompanyData();
                    break;
                }
            }
        });
    } else {
        ManagementCompany.splice(managementCompany);
        bindManagementCompanyData();
    }
}
//绑定缓存数据
function bindManagementCompanyData() {
    //构造data
    var data = [];
    for (var i = 0; i < ManagementCompany.length; i++) {
        data.push({ "ManagementCompany": ManagementCompany[i] });
    }
    var source =
    {
        localData: data,
        dataType: "array"
    };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    debugger;
    $("#jqxManagementCompanyTable").jqxDataTable(
        {
            pageable: true,
            width: "100%",
            height: 300,
            pageSize: 10,
            serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 30,
            columnsResize: true,
            columns: [
                { text: '资产管理公司', datafield: 'ManagementCompany', width: 338, align: 'center', cellsAlign: 'center' },
                { text: '删除', hidden: false, width: 110, align: 'center', cellsAlign: 'center', cellsRenderer: cellsDeleteRenderer },
                { text: 'VGUID', datafield: 'VGUID', hidden: true }
            ]
        });
}
//公司配置
function initManagementCompanyData() {
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
            data: { UserVGUID: $.request.queryString().Vguid },
            url: "/Systemmanagement/UserManagement/GetUserCompanyInfo"   //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source);

    //创建卡信息列表（主表）
    $("#jqxTable1").jqxGrid(
        {
            pageable: true,
            width: "98%",
            height: 380,
            pageSize: 10,
            //serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            groupable: true,
            groupsexpandedbydefault: true,
            groups: ['Descrption'],
            showgroupsheader: false,
            columnsHeight: 30,
            editable: true,
            pagermode: 'simple',
            selectionmode: 'none',
            columns: [
                //{ text: "", datafield: "checkbox", width: 40, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                {
                    text: '选择', datafield: "IsCheck", width: 60, align: 'center', cellsAlign: 'center', columntype: 'checkbox',
                },
                { text: '账套编码', datafield: 'Code', width: 120, align: 'center', cellsAlign: 'center', editable: false, },
                { text: '账套描述', datafield: 'Descrption', width: 250, align: 'center', cellsAlign: 'center', editable: false },
                { text: '公司编码', datafield: 'CompanyCode', width: 150, align: 'center', cellsAlign: 'center', editable: false },
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
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}
function formatDate(NewDtime) {
    var dt = new Date(parseInt(NewDtime.slice(6, 19)));
    var year = dt.getFullYear();
    var month = dt.getMonth() + 1;
    var date = dt.getDate();
    var hour = dt.getHours();
    var minute = dt.getMinutes();
    var second = dt.getSeconds();
    return year + "-" + month + "-" + date + " " + hour + ":" + minute + ":" + second;
}
$(function () {
    var page = new $page();
    page.init();
});
