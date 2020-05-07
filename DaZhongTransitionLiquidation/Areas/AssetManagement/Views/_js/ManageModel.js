//所有元素选择器
var selector = {
    $grid: function () { return $("#moduletree") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
    $AddNewManageModelDialog: function () { return $("#AddNewManageModelDialog") },
    $AddManageModelData_OKButton: function () { return $("#AddManageModelData_OKButton") },
    $AddManageModelData_CancelBtn: function () { return $("#AddManageModelData_CancelBtn") },
    $AddAssetsModel_OKButton: function () { return $("#AddAssetsModel_OKButton") },
    $AddAssetsModel_CancelBtn: function () { return $("#AddAssetsModel_CancelBtn") },
    $AddAssetsModelDialog: function () { return $("#AddAssetsModelDialog") },
    $AddAssetsModelButton: function () { return $("#AddAssetsModelButton") },
    $AddAssetsModelTable_CancelBtn: function () { return $("#AddAssetsModelTable_CancelBtn") },
    $AddAssetsModelTableDialog: function () { return $("#AddAssetsModelTableDialog") },
    $AssetsCategoryTable: function () { return $("#AssetsCategoryTable") }
}

var parentVguid = "";
var hideParentMenu = "";
var vguid = "";
var isEditAssetsCategory = 0;
var isEdit;
var currentlevel;
var $page = function () {
    this.init = function () {
        pageload();
        initSelectAssetMajor();
        initSelectGoodsModel();
        addEvent();
        if ($("#EditPermission").val() == "True" || $("#NewPermission").val() == "True") {
            $("#AddAssetsModelButton").show();
        } else {
            $("#AddAssetsModelButton").hide();
        }
    }
    function pageload() {
        getModules(function (modules) {
            loadGridTree(modules);
        });
    }
    //所有事件
    function addEvent() {
        //新增
        selector.$btnAdd().click(function () {
            var checkrow = selector.$grid().jqxTreeGrid('getSelection');
            debugger;
            if (checkrow.length != 1) {
                jqxNotification("请选择一个节点！", null, "error");
                return;
            } else {
                debugger;
                parentVguid = checkrow[0].VGUID;
                hideParentMenu = checkrow[0].BusinessName;
                currentlevel = checkrow[0].level;
                var level = checkrow[0].level;
                if (level == 1) {
                    //显示车龄
                    $("#VehicleAge").val("");
                    $("#SubjectVehicleAge").show();
                } else {
                    $("#SubjectVehicleAge").hide();
                }
                var code = checkrow[0].Code;
                $("#SubjectCode").show();
                $("#hideParentMenu").val(parentVguid);
                $("#ParentMenu").val(hideParentMenu);
                $("#ModuleCode").val(code);
            }

            $("#ModuleName").val("");

            $("#FirstMenu").val("0");
            //$("#txtParentCode").val("");
            //$("#txtRemark").val("");
            isEdit = false;
            vguid = "";
            $("#myModalLabel_title").text("新增数据");
            //$("#AddNewManageModelDialog table tr").eq(1).show();
            $(".msg").remove();
            $("#ModuleName").removeClass("input_Validate");


            selector.$AddNewManageModelDialog().modal({ backdrop: "static", keyboard: false });
            selector.$AddNewManageModelDialog().modal("show");

        });
        //弹出框中的取消按钮
        selector.$AddManageModelData_CancelBtn().on("click", function () {
            selector.$AddNewManageModelDialog().modal("hide");
        });
        //弹出框中的取消按钮
        selector.$AddAssetsModel_CancelBtn().on("click", function () {
            //selector.AddAssetsModelDialog().modal("hide");
            $("#AddAssetsModelDialog").modal("hide");
        });
        //弹出框中的取消按钮
        selector.$AddAssetsModelTable_CancelBtn().on("click", function () {
            selector.$AddAssetsModelTableDialog().modal("hide");
        });
        //删除
        selector.$btnDelete().click(function () {
            var selection = [];
            var checkrow = selector.$grid().jqxTreeGrid('getSelection');
            for (var i = 0; i < checkrow.length; i++) {
                var rowdata = checkrow[i];
                selection.push(rowdata.VGUID);
            }
            if (selection.length < 1) {
                jqxNotification("请选择您要删除的数据！", null, "error");
            } else {
                WindowConfirmDialog(dele, "您确定要删除选中的数据？", "确认框", "确定", "取消", selection);
            }

        });
        //保存
        selector.$AddManageModelData_OKButton().on("click", function () {
            var validateError = 0;//未通过验证的数量
            if (!Validate($("#ModuleName"))) {
                validateError++;
            }
            if (!Validate($("#hideParentMenu")) && $("#FirstMenu").val() == "0") {
                validateError++;
            }
            var url = "/AssetManagement/ManageModel/SaveBusiness?isEdit=";
            if (validateError <= 0) {
                if ($("#FirstMenu").val() == 1) {
                    currentlevel = 0;
                } else if (!isEdit) {
                    currentlevel = parseInt(currentlevel) + 1;
                }
                $.ajax({
                    url: url + isEdit,
                    data: {
                        "Code": $("#ModuleCode").val(),
                        "BusinessName": $("#ModuleName").val(),
                        "VehicleAge": $("#VehicleAge").val(),
                        "VGUID": vguid,
                        "LevelNum": currentlevel,
                        "ParentVGUID": $("#hideParentMenu").val()
                    },
                    type: "post",
                    dataType: "json",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("保存失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("保存成功！", null, "success");
                                pageload();
                                selector.$AddNewManageModelDialog().modal("hide");
                                break;
                        }

                    }
                });
            }
        });
        //改变一级节点值
        $('#FirstMenu').on('change',
            function(event) {
                if ($("#FirstMenu").val() == "1") {
                    $("#SubjectCode").hide(); //父级菜单
                    $("#ParentMenu").val("");
                    $("#hideParentMenu").val("");
                } else {
                    $("#SubjectCode").show();
                    $("#hideParentMenu").val(parentVguid);
                    $("#ParentMenu").val(hideParentMenu);
                }
            });
        $("#CategoryMajor").on('select', function (event) {
            if (event.args) {
                initSelectMinor();
            }
        });
        selector.$AddAssetsModel_OKButton().on("click", function () {
            var url = "/AssetManagement/ManageModel/SaveAssetsModel";
            $.ajax({
                url: url,
                data: {
                    "VGUID": $("#EditVguid").val(),
                    "GoodsModelCode": $("#GoodsModel").val(),
                    "GoodsModel": $("#GoodsModel option:selected").text(),
                    "CategoryMajor": $("#CategoryMajor").val(),
                    "AssetsCategoryVGUID": $("#CategoryMinor").val(),
                    "CategoryMinor": $("#CategoryMinor").text(),
                    "ManageModelVGUID": $("#AddAssetsModel_OKButton").attr("VGUID"),
                    "isEditAssetsCategory": isEditAssetsCategory
                },
                type: "post",
                dataType: "json",
                success: function (msg) {
                    switch (msg.Status) {
                    case "0":
                        jqxNotification("保存失败！", null, "error");
                        break;
                    case "2":
                        jqxNotification(msg.ResultInfo, null, "error");
                        break;
                    case "1":
                        jqxNotification("保存成功！", null, "success");
                        initAssetsCategoryTable($("#AddAssetsModel_OKButton").attr("VGUID"));
                        selector.$AddAssetsModelDialog().modal("hide");
                        break;
                    }
                }
            });
        });
        selector.$AddAssetsModelButton().on("click", function () {
            isEditAssetsCategory = 0;
            $("#GoodsModel").attr("disabled",false);
            $("#GoodsModel").val("");
            initSelectAssetMajor();
            selector.$AddAssetsModelDialog().modal("show");
        });
    }

    //删除
    function dele(selection) {
        $.ajax({
            url: "/AssetManagement/ManageModel/DeleteBusiness",
            data: { vguids: selection },
            traditional: true,
            type: "post",
            success: function (msg) {
                switch (msg.Status) {
                    case "0":
                        jqxNotification("删除失败！", null, "error");
                        break;
                    case "1":
                        jqxNotification("删除成功！", null, "success");
                        pageload();
                        break;
                }
            }
        });
    }
}

function getModules(callback) {
    $.ajax({
        url: "/AssetManagement/ManageModel/GetBusiness",
        type: "get",
        dataType: "json",
        success: function (msg) {
            callback(msg);
        }
    });

}

function loadGridTree(modules) {
    var source =
            {
                dataType: "json",
                dataFields: [
                    { name: 'BusinessName', type: 'string' },
                    { name: 'ParentVGUID', type: 'string' },
                    { name: 'VehicleAge', type: 'number' },
                    { name: 'CategoryMajor', type: 'string' },
                    { name: 'CategoryMinor', type: 'string' },
                    { name: 'AssetsCategoryVGUID', type: 'string' },
                    { name: 'VGUID', type: 'string' }
                ],
                hierarchy:
                {
                    keyDataField: { name: 'VGUID' },
                    parentDataField: { name: 'ParentVGUID' }
                },
                id: 'VGUID',
                localData: modules
            };
    var dataAdapter = new $.jqx.dataAdapter(source);
    var width = selector.$grid().width() - 330;
    selector.$grid().jqxTreeGrid({
        width: selector.$grid().width() - 2,
        height:400,
        showHeader: true,
        source: dataAdapter,
        //checkboxes: true,
        ready: function () {
            selector.$grid().jqxTreeGrid('expandAll');
        },
        columns: [
          { text: '业务名称', dataField: 'BusinessName', width: "260", cellsRenderer: detailFuncs },
          { text: '车龄', dataField: 'VehicleAge', width: "50" },
          { text: '资产类别', width: width, cellsRenderer: detailsCategoryFuncs },
          { text: '', dataField: 'ParentVGUID', hidden: true },
          { text: '', dataField: 'VGUID', hidden: true }
        ]
    });
    selector.$grid().jqxTreeGrid('expandAll');
}
function detailFuncs(row, column, value, rowData) {
    var container = "";
    if ($("#EditPermission").val() == "True" || $("#NewPermission").val() == "True") {
        if (rowData.parent == null) {
            container = "<a href='#' onclick=edit('" +
                rowData.VGUID +
                "','" +
                rowData.level +
                "','" +
                rowData.BusinessName +
                "','" +
                rowData.VehicleAge +
                "','" +
                rowData.ParentVGUID +
                "') style=\"text-decoration: underline;color: #333;\">" +
                rowData.BusinessName +
                "</a>";
        } else {
            debugger;
            container = "<a href='#' onclick=edit('" +
                rowData.VGUID +
                "','" +
                rowData.level +
                "','" +
                rowData.BusinessName +
                "','" +
                rowData.VehicleAge +
                "','" +
                rowData.ParentVGUID +
                "','" +
                rowData.parent.BusinessName +
                "') style=\"text-decoration: underline;color: #333;\">" +
                rowData.BusinessName +
                "</a>";
        }
    } else {
        container = rowData.BusinessName;
    }
    
    return container;
}
function detailsCategoryFuncs(row, column, value, rowData) {
    var container = "";
    if (rowData.level == 1) {
        container = "<a href='#' onclick=editAsstsModel('" + rowData.VGUID + "','" + rowData.CategoryMajor + "','" + rowData.AssetsCategoryVGUID + "') style=\"text-decoration: underline;color: #333;\">配置</a>";
    } 
    return container;
}
function detailsEditCategoryFuncs(row, column, value, rowData) {
    var container = "";
    container = "<a href='#' onclick=editCategory('" + rowData.VGUID + "','" + rowData.CategoryMajor + "','" + rowData.AssetsCategoryVGUID + "','" + rowData.GoodsModelCode + "') style=\"text-decoration: underline;color: #333;\">" + rowData.GoodsModel + "</a>";
    return container;
}
function delCategoryFuncs(row, column, value, rowData) {
    var container = "";
    container = "<a href='#' onclick=delCategory('" + rowData.VGUID + "') style=\"text-decoration: underline;color: #333;\">删除</a>";
    return container;
}
function edit(guid,level, BusinessName,VehicleAge, ParentVGUID, parentBusinessName) {
    isEdit = true;
    currentlevel = level;
    vguid = guid;
    $("#ModuleName").val(BusinessName);
    $("#VehicleAge").val(VehicleAge);
    $("#myModalLabel_title").text("编辑数据");
    //$("#AddNewManageModelDialog table tr").eq(1).hide();
    $(".msg").remove();
    $("#ModuleName").removeClass("input_Validate");
    selector.$AddNewManageModelDialog().modal({ backdrop: "static", keyboard: false });
    selector.$AddNewManageModelDialog().modal("show");
    debugger;
    if (ParentVGUID == "null" || ParentVGUID == "" || ParentVGUID == null) {
        $("#FirstMenu").val("1");
        $("#ParentMenu").val("");
        $("#hideParentMenu").val("");
        $("#SubjectCode").hide();
    } else {
        $("#FirstMenu").val("0");
        $("#ParentMenu").val(parentBusinessName);
        $("#hideParentMenu").val(ParentVGUID);
        $("#SubjectCode").show();
    }
    if (level == 2) {
        //显示车龄
        $("#SubjectVehicleAge").show();
    } else {
        $("#SubjectVehicleAge").hide();
    }
}
function initAssetsCategoryTable(VGUID) {
    var source =
        {
            dataFields: [
                { name: 'ManageModelVGUID', type: 'string'},
                { name: 'GoodsModelCode', type: 'string'},
                { name: 'GoodsModel', type: 'number' },
                { name: 'AssetsCategoryVGUID', type: 'string'},
                { name: 'CategoryMajor', type: 'string' },
                { name: 'CategoryMinor', type: 'string' },
                { name: 'VGUID', type: 'string' }
            ],
            datatype: "json",
            id: "VGUID",
            data: { "ManageModelVGUID": VGUID },
            url: "/AssetManagement/ManageModel/GetAssetsCategoryList"   //获取数据源的路径
        };
    var typeAdapter = new $.jqx.dataAdapter(source, {
        downloadComplete: function (data) {
            source.totalrecords = data.TotalRows;
        }
    });
    //创建卡信息列表（主表）
    selector.$AssetsCategoryTable().jqxDataTable(
        {
            pageable: true,
            width: 640,
            height: 400,
            pageSize: 10,
            serverProcessing: true,
            pagerButtonsCount: 10,
            source: typeAdapter,
            theme: "office",
            columnsHeight: 40,
            columns: [
                { text: '车型', datafield: 'GoodsModel', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: detailsEditCategoryFuncs },
                { text: '资产主类', datafield: 'CategoryMajor', width: 170, align: 'center', cellsAlign: 'center' },
                { text: '资产子类', datafield: 'CategoryMinor', width: 170, align: 'center', cellsAlign: 'center' },
                //{ text: '编辑', width: 150, align: 'center',cellsRenderer: detailsEditCategoryFuncs, cellsAlign: 'center' },
                { text: '删除', width: 150, align: 'center', cellsRenderer: delCategoryFuncs, cellsAlign: 'center' }
                //{ text: '创建日期', datafield: 'CREATE_DATE', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" }
            ]
        });
}
function detailFunc(row, column, value, rowData) {
    var container = "";
    container = "<a href='#' onclick=link('" + rowData.VGUID + "') style=\"text-decoration: underline;color: #333;\">" + rowData.GoodsModel + "</a>";
    return container;
}
function editAsstsModel(VGUID, CategoryMajor, AssetsCategoryVGUID) {
    //debugger;
    $("#AddAssetsModel_OKButton").attr("VGUID", VGUID);
    //if (CategoryMajor != "null") {
    //    $("#CategoryMajor").val(CategoryMajor);
    //    $("#CategoryMinor").val(AssetsCategoryVGUID);
    //} else {
    //    initSelectAssetMajor();
    //}
    initAssetsCategoryTable($("#AddAssetsModel_OKButton").attr("VGUID"));
    $("#AddAssetsModelTableDialog").modal("show");
}
function editCategory(VGUID, CategoryMajor, AssetsCategoryVGUID, GoodsModelCode) {
    debugger;
    if ($("#EditPermission").val() == "True" || $("#NewPermission").val() == "True") {
        $("#EditVguid").val(VGUID);
        if (CategoryMajor != "null") {
            $("#CategoryMajor").val(CategoryMajor);
            $("#CategoryMinor").val(AssetsCategoryVGUID);
            $("#GoodsModel").val(GoodsModelCode);
        } else {
            initSelectAssetMajor();
        }
        $("#GoodsModel").attr("disabled", true);
        isEditAssetsCategory = 1;
        $("#AddAssetsModelDialog").modal("show");
    } else {
        jqxNotification("没有权限！", null, "error");
    }
}
function delCategory(VGUID) {
    debugger;
    if ($("#EditPermission").val() == "True" || $("#NewPermission").val() == "True") {
        $.ajax({
            url: "/AssetManagement/ManageModel/DelAssetsCategory",
            type: "post",
            async: false,
            data: { "Vguid": VGUID },
            success: function(msg) {
                switch (msg.Status) {
                case "0":
                    jqxNotification("删除失败！", null, "error");
                    break;
                case "1":
                    jqxNotification("删除成功！", null, "success");
                    initAssetsCategoryTable($("#AddAssetsModel_OKButton").attr("VGUID"));
                    break;
                }
            }
        });
    } else {
        jqxNotification("没有权限！", null, "error");
    }
}
function initSelectAssetMajor() {
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
            $("#CategoryMajor").jqxDropDownList({ selectedIndex: 0, source: dataAdapter, width: 198, height: 33 });
            $("#CategoryMajor").jqxDropDownList({ itemHeight: 33 });
            initSelectMinor();
        }
    });
}
function initSelectMinor() {
    var source =
    {
        data: {
            "MAJOR": $("#CategoryMajor").val()
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
    $("#CategoryMinor").jqxDropDownList({ selectedIndex: 0, source: dataAdapter, displayMember: "AssetMinor", valueMember: "AssetMinorVguid", width: 198, height: 33 });
    $("#CategoryMinor").jqxDropDownList({ itemHeight: 33 });
}
function initSelectGoodsModel() {
    //物品类型
    $.ajax({
        url: "/AssetPurchase/FixedAssetsOrderDetail/GetGoodsModelDropDown",
        data: { "Goods": "出租车" },
        type: "POST",
        dataType: "json",
        async: false,
        success: function (msg) {
            debugger;
            if (msg.length > 0) {
                uiEngineHelper.bindSelect('#GoodsModel', msg, "Code", "Descrption");
                $("#GoodsModel").prepend("<option value=\"\" selected='true'>请选择</>");
            }
        }
    });
}
$(function () {
    var page = new $page();
    page.init();
});