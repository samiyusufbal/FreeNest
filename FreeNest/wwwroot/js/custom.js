function CreateDatatableList(data, col) {
    $(".dataTable").DataTable({
        language: {
            url: '/lib/datatables/data-table-turkish.json'
        },
        "bStateSave": true,
        "fnStateSave": function (oSettings, oData) {
            localStorage.setItem('offersDataTables', JSON.stringify(oData));
        },
        dom: 'fBrtipl',

        buttons: [
            { extend: 'excel', className: 'btn btn-sm btn-primary ' },
            { extend: 'pdf', className: 'btn btn-sm btn-primary ' },
            { extend: 'print', className: 'btn btn-sm btn-primary ' }
        ],
        "fnStateLoad": function (oSettings) {
            return JSON.parse(localStorage.getItem('offersDataTables'));
        },
        scrollX: true,
        data: data,
        columns: col,
    });
}

function CreateExtraDatatableList(data, col) {
    $(".dataTable thead tr")
        .clone(true)
        .addClass('filters')
        .appendTo(".dataTable thead");

    var table = $(".dataTable").DataTable({
        language: {
            url: '/lib/datatables/data-table-turkish.json'
        },
        orderCellsTop: true,
        fixedHeader: true,
        "bStateSave": true,
        "fnStateSave": function (oSettings, oData) {
            localStorage.setItem('offersDataTables', JSON.stringify(oData));
        },
        dom: 'fBrtipl',
        buttons: [
            { extend: 'excel', className: 'btn btn-sm btn-primary' },
            { extend: 'pdf', className: 'btn btn-sm btn-primary' },
            { extend: 'print', className: 'btn btn-sm btn-primary' }
        ],
        "fnStateLoad": function (oSettings) {
            return JSON.parse(localStorage.getItem('offersDataTables'));
        },
        scrollX: true,
        data: data,
        columns: col,
        initComplete: function () {
            var api = this.api();

            api.columns().eq(0).each(function (colIdx) {
                var cell = $('.filters th').eq($(api.column(colIdx).header()).index());
                var title = $(cell).text();

                $(cell).html(`<input class="form-control" type="text" placeholder="${title}" />`);
                if (title == 'Eylemler')
                    $(cell).html('');
                if (title == 'Kapak Fotoğrafı')
                    $(cell).html('');

                $('input', $('.filters th').eq($(api.column(colIdx).header()).index()))
                    .off('keyup change')
                    .on('change', function (e) {
                        $(this).attr('title', $(this).val());
                        var regexr = '({search})';
                        var cursorPosition = this.selectionStart;

                        api.column(colIdx)
                            .search(this.value != '' ? regexr.replace('{search}', `(((${this.value})))`) : '',
                                this.value != '',
                                this.value == '')
                            .draw();
                    })
                    .on('keyup', function (e) {
                        e.stopPropagation();
                        $(this).trigger('change');
                        $(this).focus()[0].setSelectionRange(cursorPosition, cursorPosition);
                    });
            });
        }
    });
}
function Delete(urlStr, id) {
    Swal.fire({
        title: 'Bu kaydı silmek üzeresin!',
        text: "Bu kayıtla ilgili tüm kayıtlar da silinecektir!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#333',
        confirmButtonText: 'Sil',
        cancelButtonText: 'Vazgeç'
    }).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: urlStr + id,
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                dataType: "json",
                success: function (data, textStatus, jQxhr) {

                    if (data == "success") {
                        window.location.reload();
                    }
                    else {
                        swal("Error", data.message, "error");
                    }
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    console.log(errorThrown);
                }
            });
        }
    });
}