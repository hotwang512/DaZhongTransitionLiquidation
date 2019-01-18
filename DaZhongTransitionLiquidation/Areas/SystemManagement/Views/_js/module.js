//所有元素选择器
var selector = {
    $grid: function () { return $("#moduletree") },
    $btnAdd: function () { return $("#btnAdd") },
    $btnDelete: function () { return $("#btnDelete") },
}

var $page = function () {

    this.init = function () {
        pageload();
        addEvent();
    }
    //所有事件
    function pageload() {
        getModules(function (modules) {
            loadGridTree(modules);
        });
    }
    //所有事件
    function addEvent() {

        selector.$btnAdd().click(function () {
          

        });
        selector.$btnDelete().click(function () {
           

        });
    }
}

function getModules(callback) {
    $.ajax({
        url: "/SystemManagement/ModuleManagement/GetModules",
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
                    { name: 'ModuleName', type: 'string' },
                    { name: 'Parent', type: 'string' },
                    { name: 'Vguid', type: 'string' }
                ],
                hierarchy:
                {
                    keyDataField: { name: 'Vguid' },
                    parentDataField: { name: 'Parent' }
                },
                id: 'Vguid',
                localData: modules
            };
    var dataAdapter = new $.jqx.dataAdapter(source);
    selector.$grid().jqxTreeGrid({
        width: selector.$grid().width(),
        showHeader: false,
        source: dataAdapter,
        //ready: function () {
        //    $("#treegrid").jqxTreeGrid('expandRow', '1');
        //},
        columns: [
          { text: '模块名称', dataField: 'ModuleName', width: "100%" },
        ]
    });
}




$(function () {
    var page = new $page();
    page.init();
});