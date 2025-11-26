(function ($) {
    var _flashSaleService = abp.services.app.flashSale,
        l = abp.localization.getSource('MyProject'),
        _$modal = $('#FlashSaleCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#FlashSalesTable');

    var _$flashSalesTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _flashSaleService.getAll,
            inputFilter: function () {
                return $('#FlashSaleSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                titleAttr: 'Làm mới danh sách',
                action: () => _$flashSalesTable.draw(false)
            }
        ],
        responsive: {
            details: {
                type: 'column',
                target: 'tr'
            }
        },
        columnDefs: [
            {
                targets: 0,
                className: 'control',
                defaultContent: '',
            },
            {
                targets: 1,
                data: 'name',
                sortable: false,
                width: '20%',
                responsivePriority: 1
            },
            {
                targets: 2,
                data: 'startTime',
                sortable: false,
                width: '12%',
                responsivePriority: 3,
                render: function (data) {
                    return data ? new Date(data).toLocaleString('vi-VN') : '';
                }
            },
            {
                targets: 3,
                data: 'endTime',
                sortable: false,
                width: '12%',
                responsivePriority: 3,
                render: function (data) {
                    return data ? new Date(data).toLocaleString('vi-VN') : '';
                }
            },
            {
                targets: 4,
                data: 'statusText',
                sortable: false,
                width: '10%',
                responsivePriority: 2
            },
            {
                targets: 5,
                data: 'totalProducts',
                sortable: false,
                width: '8%',
                responsivePriority: 4
            },
            {
                targets: 6,
                data: 'totalSold',
                sortable: false,
                width: '8%',
                responsivePriority: 4
            },
            {
                targets: 7,
                data: 'isActive',
                sortable: false,
                width: '8%',
                responsivePriority: 4,
                render: data => `<input type="checkbox" disabled ${data ? 'checked' : ''}>`
            },
            {
                targets: 8,
                data: 'isHidden',
                sortable: false,
                width: '8%',
                responsivePriority: 4,
                render: data => `<input type="checkbox" disabled ${data ? 'checked' : ''}>`
            },
            {
                targets: 9,
                data: null,
                sortable: false,
                width: '14%',
                responsivePriority: 1,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    const hideButtonTitle = row.isHidden ? 'Hiển thị flash sale' : 'Ẩn flash sale';
                    return [
                        `<div class="btn-group" role="group">`,
                        `   <button type="button" class="btn btn-sm bg-secondary edit-flash-sale" data-flash-sale-id="${row.id}" data-toggle="modal" data-target="#FlashSaleEditModal" title="Chỉnh sửa flash sale">`,
                        `       <i class="fas fa-pencil-alt"></i>`,
                        '   </button>',
                        `   <button type="button" class="btn btn-sm ${row.isHidden ? 'bg-success' : 'bg-warning'} toggle-hide-flash-sale" data-flash-sale-id="${row.id}" title="${hideButtonTitle}">`,
                        `       <i class="fas fa-eye${row.isHidden ? '' : '-slash'}"></i>`,
                        '   </button>',
                        `   <button type="button" class="btn btn-sm bg-info detail-flash-sale" data-flash-sale-id="${row.id}" title="Xem chi tiết flash sale">`,
                        `       <i class="fas fa-eye"></i>`,
                        '   </button>',
                        `   <button type="button" class="btn btn-sm bg-danger delete-flash-sale" data-flash-sale-id="${row.id}" data-flash-sale-name="${row.name}" title="Xóa flash sale">`,
                        `       <i class="fas fa-trash"></i>`,
                        '   </button>',
                        '</div>'
                    ].join('');
                }
            }
        ]
    });

    _$form.validate({
        rules: {
            Name: "required",
            StartTime: "required",
            EndTime: "required"
        }
    });

    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var flashSale = _$form.serializeFormToObject();

        // Convert datetime-local to ISO string
        if (flashSale.StartTime) {
            flashSale.StartTime = new Date(flashSale.StartTime).toISOString();
        }
        if (flashSale.EndTime) {
            flashSale.EndTime = new Date(flashSale.EndTime).toISOString();
        }

        abp.ui.setBusy(_$modal);
        _flashSaleService.create(flashSale).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            _$flashSalesTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-flash-sale', function () {
        var flashSaleId = $(this).attr("data-flash-sale-id");
        var flashSaleName = $(this).attr('data-flash-sale-name');

        deleteFlashSale(flashSaleId, flashSaleName);
    });

    function deleteFlashSale(flashSaleId, flashSaleName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                flashSaleName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _flashSaleService.delete({
                        id: flashSaleId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$flashSalesTable.ajax.reload();
                    });
                }
            }
        );
    }

    $(document).on('click', '.edit-flash-sale', function (e) {
        var flashSaleId = $(this).attr("data-flash-sale-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'FlashSales/EditModal?flashSaleId=' + flashSaleId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#FlashSaleEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', '.toggle-hide-flash-sale', function () {
        var flashSaleId = $(this).attr("data-flash-sale-id");

        _flashSaleService.toggleHide({
            id: flashSaleId
        }).done(() => {
            abp.notify.info(l('SavedSuccessfully'));
            _$flashSalesTable.ajax.reload();
        });
    });

    $(document).on('click', '.detail-flash-sale', function () {
        var flashSaleId = $(this).attr("data-flash-sale-id");
        window.location.href = "/FlashSales/Detail?flashSaleId=" + flashSaleId;
    });

    abp.event.on('flashSale.edited', (data) => {
        _$flashSalesTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$flashSalesTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$flashSalesTable.ajax.reload();
            return false;
        }
    });
})(jQuery);

