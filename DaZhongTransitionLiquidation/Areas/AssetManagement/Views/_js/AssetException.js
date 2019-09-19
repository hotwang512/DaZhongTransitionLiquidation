//资产维护
//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var $page = function () {
    this.init = function () {
        addEvent();
        GetVehicleModelDropDown();
    }
    //所有事件
    function addEvent() {
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });
        //重置按钮事件
        //selector.$btnReset().on("click", function () {
        //});
        //提交
        $("#btnSubmit").on("click", function () {
            var selection = [];
            var array = $("#jqxTable").jqxGrid('getselectedrowindexes');
            var pars = [];
            $(array).each(function (i, v) {
                try {
                    var value = $("#jqxTable").jqxGrid('getcell', v, "TRANSACTION_ID");
                    pars.push(value.value);
                } catch (e) {
                }
            });
            if (array.length < 1) {
                jqxNotification("请选择一条数据！", null, "error");
            } else {
                layer.load();
                debugger;
                $.ajax({
                    url: "/AssetManagement/AssetException/SubmitExceptionAsset",
                    data: { vguids: pars },
                    //traditional: true,
                    type: "post",
                    success: function (msg) {
                        layer.closeAll('loading');
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("提交失败！", null, "error");
                                break;
                            case "1":
                                jqxNotification("提交成功！", null, "success");
                                $("#jqxTable").jqxGrid('updateBoundData');
                                $('#jqxTable').jqxGrid('clearselection');
                                break;
                            case "2":
                                jqxNotification(msg.ResultInfo, null, "success");
                                $("#myModalLabel_title2").html(msg.ResultInfo);
                                ViewReview(msg.ResultInfo2);
                                $("#jqxTable").jqxGrid('updateBoundData');
                                $('#jqxTable').jqxGrid('clearselection');
                                break;
                        }
                    }
                });
            }
        });
        //关闭
        $("#AssetReviewDialog_CancelBtn").on("click",
            function () {
                $("#AssetReviewDialog").modal("hide");
            }
        );
    }; //addEvent end

    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'TRANSACTION_ID', type: 'string' },
                    { name: 'BOOK_TYPE_CODE', type: 'string' },
                    { name: 'TAG_NUMBER', type: 'string' },
                    { name: 'DESCRIPTION', type: 'string' },
                    { name: 'QUANTITY', type: 'string' },
                    { name: 'ASSET_CATEGORY_MAJOR', type: 'string' },
                    { name: 'ASSET_CATEGORY_MINOR', type: 'string' },
                    { name: 'ASSET_CREATION_DATE', type: 'date' },
                    { name: 'PERIOD', type: 'string' },
                    { name: 'ASSET_COST', type: 'float' },
                    { name: 'SALVAGE_TYPE', type: 'string' },
                    { name: 'SALVAGE_PERCENT', type: 'float' },
                    { name: 'SALVAGE_VALUE', type: 'float' },
                    { name: 'METHOD', type: 'string' },
                    { name: 'LIFE_MONTHS', type: 'number' },
                    { name: 'RETIRE_QUANTITY', type: 'string' },
                    { name: 'RETIRE_COST', type: 'string' },
                    { name: 'RETIRE_DATE', type: 'date' },
                    { name: 'RETIRE_ACCT_DEPRECIATION', type: 'string' },
                    { name: 'RETIRE_PL', type: 'string' },
                    { name: 'CREATE_DATE', type: 'date' },
                    { name: 'LAST_UPDATE_DATE', type: 'date' },
                    { name: 'ASSET_ID', type: 'string' },
                    { name: 'DISPOSA_TYPE', type: 'string' },
                    { name: 'DISPOSA_AMOUNT', type: 'string' },
                    { name: 'DISPOSAL_TAX', type: 'string' },
                    { name: 'DISPOSAL_PL', type: 'string' },
                    { name: 'PROCESS_TYPE', type: 'string' }
                ],
                updaterow: function (rowid, rowdata, commit) {
                    debugger;
                    //更新标签号
                    $.ajax({
                        url: "/AssetManagement/AssetException/UpdateAssetSwap",
                        type: "POST",
                        data: { VGUID: rowdata.VGUID, TAG_NUMBER: rowdata.TAG_NUMBER },
                        dataType: "json",
                        async: false,
                        success: function (msg) {
                            switch (msg.Status) {
                            case "0":
                                jqxNotification("更新失败！", null, "error");
                                break;
                            case "1":
                                //jqxNotification("更新成功！", null, "success");
                                commit(true);
                                break;
                            }
                        }
                    });
                },
                datatype: "json",
                id: "VGUID",
                url: "/AssetManagement/AssetException/GetExceptionAssetListDatas"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        //创建卡信息列表（主表）
        selector.$grid().jqxGrid(
            {
                pageable: false,
                width: "100%",
                height: 400, editable: true,
                //pageSize: 5,
                //serverProcessing: true,
                //pagerButtonsCount: 10,
                source: typeAdapter,
                rowsheight: 40,
                selectionmode: 'checkbox',
                theme: "office",
                columnsHeight: 40,
                enablehover: false,
                columns: [
                    //{ text: "", datafield: "checkbox", width: 35, pinned: true, hidden:false,align: 'center',  cellsAlign: 'center',editable:false  cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: 'GroupID', datafield: 'GROUP_ID', width: 100, hidden: true, align: 'center',  cellsAlign: 'center',editable:false },
                    { text: '资产账簿', datafield: 'BOOK_TYPE_CODE', width: 100, align: 'center', cellsAlign: 'center', editable: false },
                    { text: '资产ID', datafield: 'ASSET_ID', width: 100, align: 'center', cellsAlign: 'center', editable: false },
                    { text: '标签号', datafield: 'TAG_NUMBER', width: 100, align: 'center', cellsAlign: 'center', editable: true },
                    { text: '资产说明', datafield: 'DESCRIPTION', width: 100, align: 'center', cellsAlign: 'center', editable: false },
                    { text: '数量', datafield: 'QUANTITY', width: 100, align: 'center', cellsAlign: 'center', editable: false },
                    { text: '资产主类', datafield: 'ASSET_CATEGORY_MAJOR', width: 100, align: 'center', cellsAlign: 'center', editable: true },
                    { text: '资产次类', datafield: 'ASSET_CATEGORY_MINOR', width: 100, align: 'center', cellsAlign: 'center', editable: true },
                    //{ text: '车型', datafield: 'VEHICLE_SHORTNAME', width: 100, align: 'center',  cellsAlign: 'center' },
                    { text: '启用日期', datafield: 'ASSET_CREATION_DATE', width: 100, align: 'center', cellsAlign: 'center',editable:false ,datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '期间', datafield: 'PERIOD', width: 100, align: 'center', cellsAlign: 'center',editable:true, datatype: 'date', cellsformat: "yyyy-MM-dd" },
                    { text: '当前成本', datafield: 'ASSET_COST', width: 100, align: 'center', cellsAlign: 'center',editable:true },
                    { text: '摊销标记', datafield: 'AMORTIZATION_FLAG', width: 100, align: 'center',  cellsAlign: 'center',editable:false },
                    { text: '存放地点1', datafield: 'BELONGTO_COMPANY', width: 100, align: 'center',  cellsAlign: 'center',editable:true },
                    { text: '存放地点2', datafield: 'MANAGEMENT_COMPANY', width: 100, align: 'center', cellsAlign: 'center', editable: true },
                    { text: '存放地点3', datafield: 'ORGANIZATION_NUM', width: 100, align: 'center', cellsalign: 'center', editable: true },
                    { text: '发动机号', datafield: 'ENGINE_NUMBER', width: 100, align: 'center',  cellsAlign: 'center',editable:false },
                    { text: '车架号', datafield: 'CHASSIS_NUMBER', width: 120, align: 'center',  cellsAlign: 'center' ,editable:false},
                    { text: '经营模式主类', datafield: 'MODEL_MAJOR', width: 180, align: 'center',  cellsAlign: 'center',editable:true },
                    { text: '模式子类', datafield: 'MODEL_MINOR', width: 100, align: 'center',  cellsAlign: 'center',editable:true },
                    { text: '创建人', datafield: 'CREATE_USER', width: 100, align: 'center',  cellsAlign: 'center' ,editable:false},
                    { text: '创建日期', datafield: 'CREATE_DATE', width: 150, align: 'center',  cellsAlign: 'center',editable:false , datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '修改人', datafield: 'CHANGE_USER', width: 100, align: 'center',  cellsAlign: 'center' },
                    { text: '修改日期', datafield: 'CHANGE_DATE', width: 100, align: 'center',  cellsAlign: 'center',editable:false , datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: 'VGUID', datafield: 'TRANSACTION_ID', hidden: true }
                ]
            });
    }
};
$(function () {
    var page = new $page();
    page.init();
});
