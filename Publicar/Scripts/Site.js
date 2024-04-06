$(document).ready(function () {
  
    $(document).on('click',"a[data-tool='panel-collapse']", function () {
        $(this).find('em').toggleClass("fa-plus fa-minus");
    });

    toNumber = function (number)
    {
        return Number(number.replace(',', '.'));
    }

    $('.form-group .input-group.date').datetimepicker({
        icons: {
            time: 'fa fa-clock-o',
            date: 'fa fa-calendar',
            up: 'fa fa-chevron-up',
            down: 'fa fa-chevron-down',
            previous: 'fa fa-chevron-left',
            next: 'fa fa-chevron-right',
            today: 'fa fa-crosshairs',
            clear: 'fa fa-trash'
        },
        format: 'DD/MM/YYYY',
        locale: 'es'
    });

    $('#datetimeVencimiento').datetimepicker({
        icons: {
            time: 'fa fa-clock-o',
            date: 'fa fa-calendar',
            up: 'fa fa-chevron-up',
            down: 'fa fa-chevron-down',
            previous: 'fa fa-chevron-left',
            next: 'fa fa-chevron-right',
            today: 'fa fa-crosshairs',
            clear: 'fa fa-trash'
        },
        format: 'DD/MM/YYYY',
        locale: 'es'
    });

    $('#datetimeInhibido').datetimepicker({
        icons: {
            time: 'fa fa-clock-o',
            date: 'fa fa-calendar',
            up: 'fa fa-chevron-up',
            down: 'fa fa-chevron-down',
            previous: 'fa fa-chevron-left',
            next: 'fa fa-chevron-right',
            today: 'fa fa-crosshairs',
            clear: 'fa fa-trash'
        },
        format: 'DD/MM/YYYY',
        locale: 'es'
    });
    
});

function booleanFormatter(cellvalue, options, rowObject) {
    return (cellvalue) ? 'Si' : 'No';
}

function getDefaultSearchOptionsForDate() {
    return {
        sopt: ['eq'],
        dataInit: function (elem) {
            var self = this;
            $(elem).datepicker({
                dateFormat: 'd/m/yy',
                changeYear: true,
                changeMonth: true,
                showButtonPanel: true,
                showOn: 'focus',
                onSelect: function () {
                    if (this.id.substr(0, 3) === "gs_") {
                        // in case of searching toolbar
                        setTimeout(function () {
                            self.triggerToolbar();
                        }, 50);
                    } else {
                        // refresh the filter in case of
                        // searching dialog
                        $(this).trigger('change');
                    }
                }
            });
        }
    };
}

function getDefaultSearchOptionsForBoolean(){
    return {
        sort: ['eq'],
        value: ':Todos;true:Si;false:No'        
    }
}

function clearSearchOptions(grid,clearSessionUrl) {
    var col_arr = grid.jqGrid("getGridParam", "colModel");
    var toolbar_selector;

    for (var i = 0, max = col_arr.length; i < max; i++) {
        if (col_arr[i].search && $("#gs_" + col_arr[i].name).val().length) {
            toolbar_selector = $("#gs_" + col_arr[i].name)
            toolbar_selector.val("");
        }
    }

    $.ajax({
        url: clearSessionUrl,
        data: {},
        success: function (data) {
            console.log(data);
            if (data.result == true) {
                grid.jqGrid('setGridParam', { search: false, postData: { "filters": "" } }).trigger("reloadGrid");
            }
        }
    });
}