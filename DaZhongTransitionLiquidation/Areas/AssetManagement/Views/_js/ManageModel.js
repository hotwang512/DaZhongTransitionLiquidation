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
    $AddAssetsModelDialog: function () { return $("#AddAssetsModelDialog") }
}

var parentVguid = "";
var hideParentMenu = "";
var vguid = "";

var $page = function () {
    this.init = function () {
        pageload();
        addEvent();
    }
    function pageload() {
        getModules(function (modules) {
            loadGridTree(modules);
        });
        initSelectAssetMajor();
    }
    //所有事件
    function addEvent() {
        //新增
        selector.$btnAdd().click(function () {
            var checkrow = selector.$grid().jqxTreeGrid('getSelection');
            if (checkrow.length != 1) {
                jqxNotification("请选择一个节点！", null, "error");
                return;
            } else {
                debugger;
                parentVguid = checkrow[0].VGUID;
                hideParentMenu = checkrow[0].BusinessName;
                var level = checkrow[0].level;
                if (level == 1) {
                    //显示车龄
                    $("#VehicleAge").val("");
                    $("#SubjectVehicleAge").show();
                } else {
                    $("#SubjectVehicleAge").hide();
                }
                var code = checkrow[0].Code;
                if ($("#FirstMenu").val() == "1") {
                    $("#SubjectCode").hide();//父级菜单
                    $("#FirstMenu").val("");
                } else {
                    $("#SubjectCode").show();
                    $("#hideParentMenu").val(parentVguid);
                    $("#ParentMenu").val(hideParentMenu);
                    $("#ModuleCode").val(code);
                }
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
            selector.$AddAssetsModelDialog().modal("hide");
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
                $.ajax({
                    url: url + isEdit,
                    data: {
                        "Code": $("#ModuleCode").val(),
                        "BusinessName": $("#ModuleName").val(),
                        "VehicleAge": $("#VehicleAge").val(),
                        "VGUID": vguid,
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
                    "CategoryMajor": $("#CategoryMajor").val(),
                    "AssetsCategoryVGUID": $("#CategoryMinor").val(),
                    "CategoryMinor": $("#CategoryMinor").text(),
                    "VGUID": $("#AddAssetsModel_OKButton").attr("VGUID")
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
                        selector.$AddAssetsModelDialog().modal("hide");
                        break;
                    }
                }
            });
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
    selector.$grid().jqxTreeGrid({
        width: selector.$grid().width(),
        showHeader: true,
        source: dataAdapter,
        //checkboxes: true,
        ready: function () {
            selector.$grid().jqxTreeGrid('expandAll');
        },
        columns: [
          { text: '业务名称', dataField: 'BusinessName', width: "20%", cellsRenderer: detailFuncs },
          { text: '车龄', dataField: 'VehicleAge', width: "40%" },
          { text: '资产类别', width: "40%", cellsRenderer: detailsCategoryFuncs },
          { text: '', dataField: 'ParentVGUID', width: "40%", hidden: true },
          { text: '', dataField: 'VGUID', width: "100%", hidden: true }
        ]
    });
    selector.$grid().jqxTreeGrid('expandAll');
}
function detailFuncs(row, column, value, rowData) {
    var container = "";
    if (rowData.parent == null) {
        container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.level + "','" + rowData.BusinessName + "','" + rowData.VehicleAge + "','" + rowData.ParentVGUID + "') style=\"text-decoration: underline;color: #333;\">" + rowData.BusinessName + "</a>";
    } else {
        debugger;
        container = "<a href='#' onclick=edit('" + rowData.VGUID + "','" + rowData.level + "','" + rowData.BusinessName + "','" + rowData.VehicleAge + "','" + rowData.ParentVGUID + "','" + rowData.parent.BusinessName + "') style=\"text-decoration: underline;color: #333;\">" + rowData.BusinessName + "</a>";
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
function edit(guid,level, BusinessName,VehicleAge, ParentVGUID, parentBusinessName) {
    isEdit = true;
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

function editAsstsModel(VGUID, CategoryMajor, AssetsCategoryVGUID) {
    debugger;
    $("#AddAssetsModel_OKButton").attr("VGUID", VGUID);
    if (CategoryMajor != "null") {
        $("#CategoryMajor").val(CategoryMajor);
        $("#CategoryMinor").val(AssetsCategoryVGUID);
    } else {
        initSelectAssetMajor();
    }
    $("#AddAssetsModelDialog").modal("show");
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
$(function () {
    var page = new $page();
    page.init();
});