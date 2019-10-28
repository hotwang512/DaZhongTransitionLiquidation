//所有元素选择器
var selector = {
    $grid: function () { return $("#jqxTable") },
    $btnSearch: function () { return $("#btnSearch") },
    $btnSubmit: function () { return $("#btnSubmit") },
    $btnReset: function () { return $("#btnReset") },
    $EditPermission: function () { return $("#EditPermission") }
}; //selector end
var isEdit = false;
var vguid = "";
var $page = function () {
    this.init = function () {
        initSelectPurchaseGoods();
        initSelectCompany();
        addEvent();
    }
    //所有事件
    function addEvent() {
        //加载列表数据
        initTable();
        selector.$btnSearch().unbind("click").on("click", function () {
            initTable();
        });
        //重置按钮事件
        selector.$btnReset().on("click", function () {
            $("#PurchaseGoods").val("");
            $("#SubmitStatus").val("");
        });
        $("#OrderBelongToDialog_OKBtn").on("click",
            function () {
                $.ajax({
                    url: "/AssetPurchase/FundClearing/AddAssign",
                    data: {
                        "FundClearingVguid": $("#AssignVguid").val(),
                        "CompanyVguid": $("#CompanyName").val(),
                        "Company": $("#CompanyName option:selected").text(),
                        "AssetNum": $("#AssetNum").val()
                    },
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                            case "0":
                                jqxNotification("分配失败", null, "error");
                                break;
                            case "1":
                                jqxNotification("分配成功！", null, "success");
                                $("#AssignDialog").modal("hide");
                                initTable();
                                break;
                        }
                    }
                });
            }
        );
        $("#OrderBelongToDialog_CancelBtn").on("click",
            function () {
                $("#AssignDialog").modal("hide");
            }
        );
        $("#btnAssign").on("click", function () {
            var selection = [];
            var grid = $("#jqxTable");
            var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
            checedBoxs.each(function () {
                var th = $(this);
                if (th.is(":checked")) {
                    var index = th.attr("index");
                    var data = grid.jqxDataTable('getRows')[index];
                    selection.push(data.VGUID);
                }
            });
            if (selection.length != 1) {
                jqxNotification("请选择一条数据！", null, "error");
            } else {
                $("#AssignVguid").val(selection[0]);
                $("#AssetNum").val("");
                $("#AssignDialog").modal("show");
            }
        });
        selector.$btnSubmit().on("click", function() {
            var selection = [];
            var grid = $("#jqxTable");
            var checedBoxs = grid.find(".jqx_datatable_checkbox:checked");
            checedBoxs.each(function () {
                var th = $(this);
                if (th.is(":checked")) {
                    var index = th.attr("index");
                    var data = grid.jqxDataTable('getRows')[index];
                    selection.push(data.VGUID);
                }
            });
            if (selection.length != 1) {
                jqxNotification("请选择一条数据！", null, "error");
            } else {
                $.ajax({
                    url: "/AssetPurchase/FundClearing/SubmitAssign",
                    data: { FundClearingVguid: selection[0] },
                    type: "post",
                    success: function (msg) {
                        switch (msg.Status) {
                        case "0":
                            jqxNotification("提交失败！", null, "error");
                            break;
                        case "2":
                            jqxNotification(msg.ResultInfo, null, "error");
                            break;
                        case "1":
                            jqxNotification("提交成功！", null, "success");
                            $("#jqxTable").jqxDataTable('updateBoundData');
                            break;
                        }
                    }
                });
            }
        });
        $("#jqxTable").on('rowExpand',
            function (event) {
                debugger;
                // event args.
                var args = event.args;
                // row data.
                var row = args.row;
                $("#OrderQuantity").val(row.OrderQuantity);
            });
    }; //addEvent end
    function initTable() {
        var source =
            {
                datafields:
                [
                    { name: "checkbox", type: null },
                    { name: 'VGUID', type: 'string' },
                    { name: 'FixedAssetsOrderVguid', type: 'string' },
                    { name: 'PurchaseGoods', type: 'string' },
                    { name: 'OrderQuantity', type: 'number' },
                    { name: 'PurchasePrices', type: 'float' },
                    { name: 'ContractAmount', type: 'float' },
                    { name: 'AssetDescription', type: 'string' },
                    { name: 'SubmitStatus', type: 'number' },
                    { name: 'CreateDate', type: 'date' },
                    { name: 'ChangeDate', type: 'date' },
                    { name: 'CreateUser', type: 'string' },
                    { name: 'ChangeUser', type: 'string' }
                ],
                datatype: "json",
                id: "VGUID",
                data: { "PurchaseGoodsVguid": $("#PurchaseGoods").val(), "SubmitStatus": $("#SubmitStatus").val() },
                url: "/AssetPurchase/FundClearing/GetListDatas"   //获取数据源的路径
            };
        var typeAdapter = new $.jqx.dataAdapter(source, {
            downloadComplete: function (data) {
                source.totalrecords = data.TotalRows;
            }
        });
        var nestedTables = new Array();
        var initRowDetails = function (id, row, element, rowinfo) {
            debugger;
            element.append($("<div style='margin: 10px;'></div>"));
            var nestedDataTable = $(element.children()[0]);
            var ordersSource = {
                dataFields: [
                    { name: 'VGUID', type: 'string' },
                    { name: 'FundClearingVguid', type: 'string' },
                    { name: 'Company', type: 'string' },
                    { name: 'PurchasePrices', type: 'number' },
                    { name: 'AssetNum', type: 'number' },
                    { name: 'ContractAmount', type: 'number' }
                ],
                datatype: "json",
                id: 'VGUID',
                data: { "Vguid": id },
                url: "/AssetPurchase/FundClearing/GetAssignCompany",
                updateRow: function (rowID, rowData, commit) {
                    debugger;
                    var count = 0;
                    var orderQuantity = parseInt($("#OrderQuantity").val());
                    var rows = $(nestedDataTable).jqxDataTable('getView');
                    for (var i = 0; i < rows.length; i++) {
                        if (rowData.VGUID == rows[i].VGUID) {
                            count += parseInt(rowData.AssetNum);
                        } else {
                            count += parseInt(rows[i].AssetNum);
                        }
                    }
                    rowData.ContractAmount = rowData.PurchasePrices * rowData.AssetNum;
                    if (orderQuantity >= count) {
                        rowData.ContractAmount = rowData.PurchasePrices * rowData.AssetNum;
                        $.ajax({
                            url: "/AssetPurchase/FundClearing/UpdateAssignCompany",
                            data: { Vguid: rowID, AssetNum: rowData.AssetNum },
                            traditional: true,
                            type: "post",
                            success: function (msg) {
                                switch (msg.Status) {
                                case "0":
                                    break;
                                case "1":
                                    commit(true);
                                    break;
                                }
                            }
                        });
                    } else {
                        jqxNotification("分配数量不能大于订单总数量！", null, "error");
                        return;
                    }
                }
            }
            if (nestedDataTable != null) {
                var nestedDataTableAdapter = new $.jqx.dataAdapter(ordersSource);
                nestedDataTable.jqxDataTable({
                    source: nestedDataTableAdapter,
                    editable: rowinfo.row.SubmitStatus == 1 ? false : true,
                    altRows: true,
                    editSettings: { saveOnPageChange: true, saveOnBlur: true, saveOnSelectionChange: true, cancelOnEsc: true, saveOnEnter: true, editSingleCell: true, editOnDoubleClick: true, editOnF2: true },
                    width: '100%', height: 180,
                    pageable: false,
                    showToolbar: true,
                    renderToolbar: function (toolBar) {
                        var toTheme = function (className) {
                            if (theme == "") return className;
                            return className + " " + className + "-" + theme;
                        }
                        var container = $("<div style='overflow: hidden; position: relative; height: 100%; width: 100%;'></div>");
                        var buttonDelTemplate = "<div style='float: left; padding: 3px; margin: 2px;'><div style='margin: 4px; width: 16px; height: 16px;'></div></div>";
                        var buttonAddTemplate = "<div style='float: left; padding: 3px; margin: 2px;'><div style='margin: 4px; width: 16px; height: 16px;'></div></div>";
                        var addButton = $(buttonAddTemplate);
                        var deleteButton = $(buttonDelTemplate);
                        container.append(addButton);
                        container.append(deleteButton);
                        toolBar.append(container);
                        addButton.jqxButton({ cursor: "pointer", enableDefault: false, height: 25, width: 25 });
                        addButton.find('div:first').addClass(toTheme('jqx-icon-plus'));
                        addButton.jqxTooltip({ position: 'bottom', content: "分配资产" });
                        deleteButton.jqxButton({ cursor: "pointer", disabled: false, enableDefault: false, height: 25, width: 25 });
                        deleteButton.find('div:first').addClass(toTheme('jqx-icon-delete'));
                        deleteButton.jqxTooltip({ position: 'bottom', content: "删除" });
                        var rowIndex = null;
                        $(nestedDataTable).on('rowSelect', function (event) {
                            var args = event.args;
                            rowIndex = args.index;
                        });
                        addButton.click(function (event) {
                            $("#AssignVguid").val(id);
                            $("#AssetNum").val("");
                            $("#AssignDialog").modal("show");
                        });
                        deleteButton.click(function () {
                            var selection = $(nestedDataTable).jqxDataTable('getSelection');
                            $.ajax({
                                url: "/AssetPurchase/FundClearing/DeleteAssignCompany",
                                data: { Vguid: selection[0].VGUID },
                                traditional: true,
                                type: "post",
                                success: function (msg) {
                                    switch (msg.Status) {
                                    case "0":
                                        jqxNotification("删除失败！", null, "error");
                                        break;
                                    case "1":
                                        jqxNotification("删除成功！", null, "success");
                                        break;
                                    }
                                }
                            });
                            $(nestedDataTable).jqxDataTable('deleteRow', rowIndex);
                        });
                    },
                    columns: [
                      { text: '公司', dataField: 'Company', editable:false, width: '45%', align: 'center', cellsAlign: 'center' },
                      { text: '单价', dataField: 'PurchasePrices', editable: false, width: '20%', align: 'center', cellsAlign: 'center' },
                      { text: '数量', dataField: 'AssetNum', width: '15%', align: 'center', cellsAlign: 'center' },
                      { text: '金额', dataField: 'ContractAmount', editable: false, width: '20%', align: 'center', cellsAlign: 'center' },
                      { text: '删除', dataField: 'VGUID', hidden:true, align: 'center', cellsAlign: 'center' }
                    ]
                });
                nestedTables[id] = nestedDataTable;
            }
        }
        //创建卡信息列表（主表）
        selector.$grid().jqxDataTable(
            {
                pageable: true,
                width: "100%",
                height: 400,
                pageSize: 10,
                serverProcessing: true,
                pagerButtonsCount: 10,
                source: typeAdapter,
                theme: "office",
                columnsHeight: 40,
                rowDetails: true,
                initRowDetails: initRowDetails,
                ready: function () {
                    //elector.$grid().jqxDataTable('showDetails', 0);
                },
                columns: [
                    { text: "", datafield: "checkbox", width: 35, pinned: true, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererFunc, renderer: rendererFunc, rendered: renderedFunc, autoRowHeight: false },
                    { text: '提交状态', datafield: 'SubmitStatus', width: 150, align: 'center', cellsAlign: 'center', cellsRenderer: cellsRendererSubmit },
                    { text: 'FixedAssetsOrderVguid', datafield: 'FixedAssetsOrderVguid', width: 150, align: 'center', cellsAlign: 'center', hidden: true },
                    { text: '采购物品', datafield: 'PurchaseGoods', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购数量', datafield: 'OrderQuantity', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购单价', datafield: 'PurchasePrices', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '合同金额', datafield: 'ContractAmount', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '采购说明', datafield: 'AssetDescription', width: 150, align: 'center', cellsAlign: 'center' },
                    { text: '创建时间', datafield: 'CreateDate', width: 150, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: '创建人', datafield: 'CreateUser', width: 100, align: 'center', cellsAlign: 'center' },
                    { text: '修改时间', datafield: 'ChangeDate', width: 100, align: 'center', cellsAlign: 'center', datatype: 'date', cellsformat: "yyyy-MM-dd HH:mm:ss" },
                    { text: 'VGUID', datafield: 'VGUID', hidden: true }
                ]
            });
    }

    function cellsRendererFunc(row, column, value, rowData) {
        return "<input class=\"jqx_datatable_checkbox\" index=\"" + row + "\" type=\"checkbox\"  style=\"margin:auto;width: 17px;height: 17px;\" />";
    }
    function rendererFunc() {
        var checkBox = "<div id='jqx_datatable_checkbox_all' class='jqx_datatable_checkbox_all' style='z-index: 999; margin-left:7px ;margin-top: 7px;'>";
        checkBox += "</div>";
        return checkBox;
    }
    function renderedFunc(element) {
        var grid = selector.$grid();
        element.jqxCheckBox();
        element.on('change', function (event) {
            var checked = element.jqxCheckBox('checked');
            if (checked) {
                var rows = grid.jqxDataTable('getRows');
                for (var i = 0; i < rows.length; i++) {
                    grid.jqxDataTable('selectRow', i);
                    grid.find(".jqx_datatable_checkbox").attr("checked", "checked")
                }
            } else {
                grid.jqxDataTable('clearSelection');
                grid.find(".jqx_datatable_checkbox").removeAttr("checked", "checked")
            }
        });
        return true;
    }
    function cellsRendererSubmit(row, column, value, rowData) {
        if (value == 1) {
            return '<span style="margin: 4px; margin-top:8px;">已提交</span>';
        } else if (value == 0) {
            return '<span style="margin: 4px; margin-top:8px;">待提交</span>';
        } else if (value == 2) {
            return '<span style="margin: 4px; margin-top:8px;">退回待提交</span>';
        }
    }
    function initSelectPurchaseGoods() {
        //使用部门
        $.ajax({
            url: "/AssetPurchase/FixedAssetsOrderDetail/GetPurchaseGoods",
            data: { "OrderCategory": 0 },
            type: "POST",
            dataType: "json",
            async: false,
            success: function (msg) {
                uiEngineHelper.bindSelect('#PurchaseGoods', msg, "VGUID", "PurchaseGoods");
                $("#PurchaseGoods").prepend("<option value=\"\" selected='true'>请选择</>");
            }
        });
    }
    function initSelectCompany() {
        //使用部门
        $.ajax({
            url: "/AssetPurchase/FundClearing/GetCompanyData",
            type: "POST",
            dataType: "json",
            async: false,
            success: function (msg) {
                uiEngineHelper.bindSelect('#CompanyName', msg, "VGUID", "Descrption");
                $("#Company").prepend("<option value=\"\" selected='true'>请选择</>");
            }
        });
    }
};
$(function () {
    var page = new $page();
    page.init();
});
