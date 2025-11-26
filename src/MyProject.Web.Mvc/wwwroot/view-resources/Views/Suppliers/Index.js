(function ($) {
    var _supplierService = abp.services.app.supplier,
        l = abp.localization.getSource('MyProject'),
        _$modal = $('#SupplierCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#SuppliersTable');

    var _$supplierTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _supplierService.getAll,
            inputFilter: function () {
                return {};
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                titleAttr: 'Làm mới danh sách',
                action: () => _$supplierTable.draw(false)
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
                data: 'code',
                sortable: false,
                width: '10%',
                responsivePriority: 3,
                render: function (data) {
                    return data || '<span class="text-muted">-</span>';
                }
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
                data: 'phone',
                sortable: false,
                width: '12%',
                responsivePriority: 4,
                render: function (data) {
                    return data || '<span class="text-muted">-</span>';
                }
            },
            {
                targets: 3,
                data: 'email',
                sortable: false,
                width: '18%',
                responsivePriority: 4,
                render: function (data) {
                    return data || '<span class="text-muted">-</span>';
                }
            },
            {
                targets: 4,
                data: 'address',
                sortable: false,
                width: '20%',
                responsivePriority: 3,
                render: function (data) {
                    if (!data) return '<span class="text-muted">-</span>';
                    return data.length > 50 ? data.substring(0, 50) + '...' : data;
                }
            },
            {
                targets: 5,
                data: 'contactPerson',
                sortable: false,
                width: '12%',
                responsivePriority: 5,
                render: function (data) {
                    return data || '<span class="text-muted">-</span>';
                }
            },
            {
                targets: 6,
                data: 'isActive',
                sortable: false,
                width: '10%',
                responsivePriority: 3,
                render: function (data) {
                    return data
                        ? '<span class="badge bg-success">Đang hoạt động</span>'
                        : '<span class="badge bg-secondary">Tạm ngưng</span>';
                }
            },
            {
                targets: 7,
                data: null,
                sortable: false,
                width: '8%',
                responsivePriority: 1,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    return [
                        `<div class="btn-group" role="group">`,
                        `   <button type="button" class="btn btn-sm bg-secondary edit-supplier" data-supplier-id="${row.id}" data-toggle="modal" data-target="#SupplierEditModal" title="Chỉnh sửa nhà cung cấp">`,
                        `       <i class="fas fa-pencil-alt"></i>`,
                        '   </button>',
                        `   <button type="button" class="btn btn-sm bg-danger delete-supplier" data-supplier-id="${row.id}" data-supplier-name="${row.name}" title="Xóa nhà cung cấp">`,
                        `       <i class="fas fa-trash"></i>`,
                        '   </button>',
                        '</div>'
                    ].join('');
                }
            }
        ]
    });

    $.validator.addMethod("validNameSupplier", function (value, element) {
        return this.optional(element) || /^(?!\d+$)(?!\s+$)[\p{L}\d\s]+$/u.test(value);
    }, "Tên nhà cung cấp không hợp lệ. Không được chỉ chứa số hoặc dấu cách.");

    $.validator.addMethod("validEmail", function (value, element) {
        if (this.optional(element)) {
            return true;
        }
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
    }, "Email không hợp lệ.");

    $.validator.addMethod("validPhone", function (value, element) {
        if (this.optional(element)) {
            return true;
        }
        return /^[0-9\s\-\+\(\)]+$/.test(value);
    }, "Số điện thoại không hợp lệ.");

    _$form.validate({
        rules: {
            Name: {
                required: true,
                validNameSupplier: true
            },
            Phone: {
                validPhone: true
            },
            Email: {
                validEmail: true
            }
        },
        messages: {
            Name: {
                required: "Vui lòng nhập tên nhà cung cấp.",
                validNameSupplier: "Tên nhà cung cấp không hợp lệ."
            },
            Phone: {
                validPhone: "Số điện thoại không hợp lệ."
            },
            Email: {
                validEmail: "Email không hợp lệ."
            }
        }
    });

    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var supplier = _$form.serializeFormToObject();
        // Convert checkbox value from "on" to boolean
        // serializeFormToObject() trả về "on" cho checkbox checked, cần convert thành boolean
        supplier.IsActive = _$form.find('input[name="IsActive"]').is(':checked');

        abp.ui.setBusy(_$modal);

        _supplierService.create(supplier).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            _$supplierTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-supplier', function () {
        var supplierId = $(this).attr("data-supplier-id");
        var supplierName = $(this).attr('data-supplier-name');
        deleteSupplier(supplierId, supplierName);
    });

    function deleteSupplier(supplierId, supplierName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('Bạn có chắc chắn muốn xóa nhà cung cấp {0}?'),
                supplierName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _supplierService.delete(supplierId).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$supplierTable.ajax.reload();
                    }).fail((error) => {
                        if (error && error.message) {
                            abp.notify.error(error.message);
                        } else {
                            abp.notify.error('Không thể xóa nhà cung cấp. Vui lòng thử lại.');
                        }
                    });
                }
            }
        );
    }

    $(document).on('click', '.edit-supplier', function (e) {
        var supplierId = $(this).attr("data-supplier-id");
        e.preventDefault();

        abp.ui.setBusy();
        abp.ajax({
            url: abp.appPath + 'Suppliers/EditModalSupplier?supplierId=' + supplierId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#SupplierEditModal div.modal-content').html(content);
                // Mở modal sau khi load content thành công
                $('#SupplierEditModal').modal('show');
            },
            error: function (e) {
                abp.notify.error('Không thể tải thông tin nhà cung cấp.');
            },
            complete: function () {
                abp.ui.clearBusy();
            }
        });
    });

    abp.event.on('supplier.edited', (data) => {
        _$supplierTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$supplierTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$supplierTable.ajax.reload();
            return false;
        }
    });

})(jQuery);

