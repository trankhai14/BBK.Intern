(function ($) {
    var _importSlipService = abp.services.app.importSlip,
        _productService = abp.services.app.product,
        _supplierService = abp.services.app.supplier,
        l = abp.localization.getSource('MyProject'),
        _$createModal = $('#ImportSlipCreateModal'),
        _$createForm = _$createModal.find('form'),
        _$editModal = $('#ImportSlipEditModal'),
        _$table = $('#ImportSlipsTable'),
        _productRowIndex = 0;

    // Khởi tạo DataTable
    var _$importSlipTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _importSlipService.getAllImportSlips,
            inputFilter: function () {
                return {
                    importCode: $('#SearchImportCode').val(),
                    status: $('#SearchStatus').val() ? parseInt($('#SearchStatus').val()) : null,
                    type: $('#SearchType').val() ? parseInt($('#SearchType').val()) : null,
                    fromDate: $('#SearchFromDate').val() ? new Date($('#SearchFromDate').val()) : null,
                    toDate: $('#SearchToDate').val() ? new Date($('#SearchToDate').val()) : null,
                    keyword: $('#SearchImportCode').val()
                };
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                titleAttr: 'Làm mới danh sách',
                action: () => _$importSlipTable.draw(false)
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
                data: 'importCode',
                sortable: false,
                width: '12%',
                responsivePriority: 1
            },
            {
                targets: 1,
                data: 'importDate',
                sortable: false,
                width: '12%',
                responsivePriority: 2,
                render: function (data) {
                    if (!data) return '-';
                    var date = new Date(data);
                    return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
                }
            },
            {
                targets: 2,
                data: 'supplierName',
                sortable: false,
                width: '15%',
                responsivePriority: 3,
                render: function (data) {
                    return data || '<span class="text-muted">-</span>';
                }
            },
            {
                targets: 3,
                data: 'typeName',
                sortable: false,
                width: '15%',
                responsivePriority: 4
            },
            {
                targets: 4,
                data: 'status',
                sortable: false,
                width: '12%',
                responsivePriority: 2,
                render: function (data) {
                    var statusMap = {
                        0: '<span class="badge badge-warning">Nháp</span>',
                        1: '<span class="badge badge-success">Đã hoàn thành</span>',
                        2: '<span class="badge badge-danger">Đã hủy</span>'
                    };
                    return statusMap[data] || '<span class="badge badge-secondary">-</span>';
                }
            },
            {
                targets: 5,
                data: 'totalAmount',
                sortable: false,
                width: '12%',
                responsivePriority: 3,
                render: function (data) {
                    return Number(data).toLocaleString('vi-VN') + ' đ';
                }
            },
            {
                targets: 6,
                data: 'details',
                sortable: false,
                width: '10%',
                responsivePriority: 5,
                render: function (data) {
                    return data ? data.length : 0;
                }
            },
            {
                targets: 7,
                data: 'creatorUserName',
                sortable: false,
                width: '12%',
                responsivePriority: 4
            },
            {
                targets: 8,
                data: null,
                sortable: false,
                width: '15%',
                responsivePriority: 1,
                defaultContent: '',
                render: function (data, type, row, meta) {
                    var buttons = [];
                    buttons.push(`<a href="/ImportSlips/Detail?importSlipId=${row.id}" class="btn btn-sm bg-info" title="Xem chi tiết">`);
                    buttons.push('<i class="fa fa-eye"></i>');
                    buttons.push('</a>');

                    if (row.status === 0) { // Draft
                        buttons.push(`<button type="button" class="btn btn-sm bg-warning edit-import-slip" data-id="${row.id}" title="Sửa">`);
                        buttons.push('<i class="fa fa-edit"></i>');
                        buttons.push('</button>');
                        buttons.push(`<button type="button" class="btn btn-sm bg-success complete-import-slip" data-id="${row.id}" title="Hoàn thành">`);
                        buttons.push('<i class="fa fa-check"></i>');
                        buttons.push('</button>');
                        buttons.push(`<button type="button" class="btn btn-sm bg-danger cancel-import-slip" data-id="${row.id}" title="Hủy">`);
                        buttons.push('<i class="fa fa-times"></i>');
                        buttons.push('</button>');
                    }

                    return buttons.join('');
                }
            }
        ]
    });

    // Search button
    $('#SearchButton').click(function () {
        _$importSlipTable.draw();
    });

    // Load suppliers vào dropdown
    function loadSuppliers() {
        _supplierService.getAll({ maxResultCount: 1000, skipCount: 0 })
            .done(function (result) {
                var $supplierSelect = $('#SupplierId, #EditSupplierId');
                $supplierSelect.find('option:not(:first)').remove();
                $.each(result.items, function (index, supplier) {
                    $supplierSelect.append($('<option></option>')
                        .attr('value', supplier.id)
                        .text(supplier.name));
                });
            });
    }

    // Thêm sản phẩm vào bảng
    function addProductRow(productId, productName, quantity, unitPrice, notes, detailId) {
        var rowIndex = _productRowIndex++;
        var row = `
			<tr data-row-index="${rowIndex}">
				<td>
					<input type="hidden" name="Details[${rowIndex}].ProductId" class="product-id" value="${productId || ''}" />
					<input type="hidden" name="Details[${rowIndex}].Id" value="${detailId || ''}" />
					<input type="text" class="form-control product-name-input" value="${productName || ''}" placeholder="-- Chọn sản phẩm --" readonly onclick="showProductSelectModal(${rowIndex})" />
				</td>
				<td>
					<input type="number" name="Details[${rowIndex}].Quantity" class="form-control quantity-input" value="${quantity || 1}" min="1" required />
				</td>
				<td>
					<input type="number" name="Details[${rowIndex}].UnitPrice" class="form-control unit-price-input" value="${unitPrice || 0}" min="0.01" step="0.01" required />
				</td>
				<td>
					<span class="total-amount">0</span>
				</td>
				<td>
					<input type="text" name="Details[${rowIndex}].Notes" class="form-control" value="${notes || ''}" maxlength="500" />
				</td>
				<td>
					<button type="button" class="btn btn-sm btn-danger remove-product-row">
						<i class="fa fa-trash"></i>
					</button>
				</td>
			</tr>
		`;
        $('#ProductDetailsBody, #EditProductDetailsBody').append(row);
        calculateTotal();
    }

    // Tính tổng tiền
    function calculateTotal() {
        var total = 0;
        $('.quantity-input').each(function () {
            var $row = $(this).closest('tr');
            var quantity = parseFloat($(this).val()) || 0;
            var unitPrice = parseFloat($row.find('.unit-price-input').val()) || 0;
            var rowTotal = quantity * unitPrice;
            $row.find('.total-amount').text(rowTotal.toLocaleString('vi-VN'));
            total += rowTotal;
        });
        $('#TotalAmount, #EditTotalAmount').text(total.toLocaleString('vi-VN'));
    }

    // Hiển thị modal chọn sản phẩm
    window.showProductSelectModal = function (rowIndex) {
        _currentRowIndex = rowIndex;
        var $productModal = $('#ProductSelectModal');
        var $createModal = $('#ImportSlipCreateModal');

        // Đảm bảo modal chọn sản phẩm được reset về trạng thái ban đầu
        $productModal.removeClass('show');
        $productModal.attr('aria-hidden', 'true');
        $productModal.removeAttr('aria-modal');
        $productModal.css('display', 'none');
        $productModal.css('z-index', '');

        // Mở modal chọn sản phẩm với z-index cao hơn modal tạo nhập kho
        $productModal.css('z-index', '1060');
        $productModal.modal('show');

        // Đảm bảo modal tạo nhập kho vẫn ở dưới và tạo backdrop thứ 2
        setTimeout(function () {
            if ($createModal.hasClass('show')) {
                $createModal.css('z-index', '1050');
            }
            // Tạo backdrop thứ 2 cho modal chọn sản phẩm nếu chưa có
            var backdrops = $('.modal-backdrop');
            if (backdrops.length === 1) {
                $('body').append('<div class="modal-backdrop fade show" style="z-index: 1055;"></div>');
            }
        }, 200);
    };

    // Xử lý khi modal chọn sản phẩm được mở
    $('#ProductSelectModal').on('show.bs.modal', function () {
        var $modal = $(this);
        // Đảm bảo modal chọn sản phẩm có z-index cao hơn modal tạo nhập kho
        $modal.css('z-index', '1060');
    });

    // Khởi tạo DataTable cho modal chọn sản phẩm
    $('#ProductSelectModal').on('shown.bs.modal', function () {
        if (!$.fn.DataTable.isDataTable('#ProductSelectTable')) {
            var productSelectTable = $('#ProductSelectTable').DataTable({
                paging: true,
                serverSide: true,
                listAction: {
                    ajaxFunction: _productService.search,
                    inputFilter: function () {
                        // Lấy dữ liệu từ form search để gửi lên server
                        var formData = $('#ProductSelectSearchForm').serializeFormToObject(true);
                        // Lấy SupplierId từ dropdown trong modal tạo nhập kho
                        var supplierId = $('#SupplierId').val();
                        return {
                            keyword: formData.Keyword || formData.keyword || '',
                            name: formData.Keyword || formData.keyword || '',
                            supplierId: supplierId ? parseInt(supplierId) : null,
                            maxResultCount: formData.maxResultCount,
                            skipCount: formData.skipCount,
                            sorting: formData.sorting
                        };
                    }
                },
                buttons: [
                    {
                        name: 'refresh',
                        text: '<i class="fas fa-redo-alt"></i>',
                        action: function (e, dt) {
                            dt.draw(false);
                        }
                    }
                ],
                columnDefs: [
                    {
                        targets: 0,
                        data: 'name',
                        sortable: false,
                        width: '40%'
                    },
                    {
                        targets: 1,
                        data: 'price',
                        sortable: false,
                        width: '20%',
                        render: function (data) {
                            return Number(data).toLocaleString('vi-VN') + ' đ';
                        }
                    },
                    {
                        targets: 2,
                        data: 'categoryName',
                        sortable: false,
                        width: '25%',
                        render: function (data) {
                            return data || '<span class="text-muted">-</span>';
                        }
                    },
                    {
                        targets: 3,
                        data: null,
                        sortable: false,
                        width: '15%',
                        defaultContent: '',
                        render: function (data, type, row) {
                            return '<button class="btn btn-sm btn-primary select-product-btn" ' +
                                'data-id="' + row.id + '" ' +
                                'data-name="' + (row.name || '').replace(/"/g, '&quot;') + '" ' +
                                'data-price="' + row.price + '">' +
                                '<i class="fas fa-check"></i> Chọn' +
                                '</button>';
                        }
                    }
                ]
            });
        } else {
            // Nếu modal được mở nhiều lần, chỉ cần reload lại
            $('#ProductSelectTable').DataTable().ajax.reload();
        }
    });

    // Chọn sản phẩm
    $(document).on('click', '.select-product-btn', function () {
        var productId = $(this).data('id');
        var productName = $(this).data('name');
        var productPrice = $(this).data('price');

        if (typeof _currentRowIndex !== 'undefined') {
            var $row = $(`tr[data-row-index="${_currentRowIndex}"]`);
            $row.find('.product-id').val(productId);
            $row.find('.product-name-input').val(productName);
            $row.find('.unit-price-input').val(productPrice);
            calculateTotal();
        } else {
            addProductRow(productId, productName, 1, productPrice, '');
        }

        // Đóng modal chọn sản phẩm một cách an toàn
        closeProductSelectModal();

        abp.notify.success('Đã chọn sản phẩm: ' + productName);
    });

    /**
     * Xử lý tìm kiếm sản phẩm
     * Reload DataTable với từ khóa tìm kiếm
     */
    $(document).on('click', '#ProductSelectSearchBtn', function () {
        if ($.fn.DataTable.isDataTable('#ProductSelectTable')) {
            $('#ProductSelectTable').DataTable().draw(false);
        }
    });

    /**
     * Xử lý reset form tìm kiếm
     * Clear input và reload DataTable
     */
    $(document).on('click', '#ProductSelectResetBtn', function () {
        $('#ProductSelectSearchForm')[0].reset();
        if ($.fn.DataTable.isDataTable('#ProductSelectTable')) {
            $('#ProductSelectTable').DataTable().draw(false);
        }
    });

    /**
     * Xử lý Enter key trong input search
     * Tự động trigger search khi nhấn Enter
     */
    $(document).on('keypress', '#ProductSelectKeyword', function (e) {
        if (e.which === 13) {  // Enter key
            e.preventDefault();
            if ($.fn.DataTable.isDataTable('#ProductSelectTable')) {
                $('#ProductSelectTable').DataTable().draw(false);
            }
        }
    });

    /**
     * Xử lý đóng modal chọn sản phẩm một cách an toàn
     * Đảm bảo không ảnh hưởng đến modal tạo nhập kho và có thể mở lại được
     */
    function closeProductSelectModal() {
        var $productModal = $('#ProductSelectModal');
        var $createModal = $('#ImportSlipCreateModal');

        // Đóng modal chọn sản phẩm bằng Bootstrap modal method
        $productModal.modal('hide');

        // Xóa backdrop của modal chọn sản phẩm (backdrop thứ 2 nếu có)
        setTimeout(function () {
            var backdrops = $('.modal-backdrop');
            if (backdrops.length > 1) {
                // Xóa backdrop cuối cùng (của modal chọn sản phẩm)
                backdrops.last().remove();
            }

            // Đảm bảo modal tạo nhập kho vẫn hiển thị và hoạt động
            if ($createModal.length && $createModal.hasClass('show')) {
                // Đảm bảo modal tạo nhập kho có z-index đúng
                $createModal.css('z-index', '1050');
                $createModal.css('display', 'block');

                // Đảm bảo có backdrop cho modal tạo nhập kho
                if ($('.modal-backdrop').length === 0) {
                    $('body').append('<div class="modal-backdrop fade show"></div>');
                }

                // Đảm bảo body có class modal-open
                if (!$('body').hasClass('modal-open')) {
                    $('body').addClass('modal-open');
                }
            }
        }, 300);
    }

    // Xử lý khi modal chọn sản phẩm bị đóng (event của Bootstrap)
    $('#ProductSelectModal').on('hidden.bs.modal', function () {
        var $modal = $(this);
        // Đảm bảo modal được reset về trạng thái ban đầu để có thể mở lại
        $modal.removeClass('show');
        $modal.attr('aria-hidden', 'true');
        $modal.removeAttr('aria-modal');
        $modal.css('display', 'none');

        // Đảm bảo modal tạo nhập kho vẫn hoạt động
        var $createModal = $('#ImportSlipCreateModal');
        setTimeout(function () {
            if ($createModal.length && $createModal.hasClass('show')) {
                $createModal.css('z-index', '1050');
                $createModal.css('display', 'block');
                if ($('.modal-backdrop').length === 0) {
                    $('body').append('<div class="modal-backdrop fade show"></div>');
                }
                if (!$('body').hasClass('modal-open')) {
                    $('body').addClass('modal-open');
                }
            }
        }, 100);
    });

    // Xử lý nút đóng modal chọn sản phẩm
    $(document).on('click', '#CloseProductSelectModal, #CloseProductSelectModalX', function (e) {
        e.preventDefault();
        closeProductSelectModal();
    });

    // Thêm sản phẩm mới
    $('#AddProductBtn, #EditAddProductBtn').click(function () {
        addProductRow();
    });

    // Xóa dòng sản phẩm
    $(document).on('click', '.remove-product-row, .remove-product-btn', function () {
        $(this).closest('tr').remove();
        calculateTotal();
    });

    // Tính lại tổng khi thay đổi số lượng hoặc giá
    $(document).on('input', '.quantity-input, .unit-price-input', function () {
        calculateTotal();
    });

    // Set ngày mặc định
    $('#ImportDate').val(new Date().toISOString().slice(0, 16));

    // Tạo phiếu nhập
    _$createForm.submit(function (e) {
        e.preventDefault();

        if (!_$createForm.valid()) {
            return false;
        }

        var productCount = $('#ProductDetailsBody tr').length;
        if (productCount === 0) {
            abp.notify.error('Vui lòng thêm ít nhất 1 sản phẩm!');
            return false;
        }

        var formData = _$createForm.serializeFormToObject();
        var details = [];

        $('#ProductDetailsBody tr').each(function () {
            var $row = $(this);
            var productId = $row.find('.product-id').val();
            if (!productId) {
                return true; // Skip if no product selected
            }
            details.push({
                productId: parseInt(productId),
                quantity: parseInt($row.find('.quantity-input').val()),
                unitPrice: parseFloat($row.find('.unit-price-input').val()),
                notes: $row.find('input[name*="Notes"]').val() || ''
            });
        });

        if (details.length === 0) {
            abp.notify.error('Vui lòng chọn ít nhất 1 sản phẩm!');
            return false;
        }

        var input = {
            importDate: new Date(formData.ImportDate),
            supplierId: formData.SupplierId ? parseInt(formData.SupplierId) : null,
            type: parseInt(formData.Type),
            notes: formData.Notes || '',
            details: details
        };

        abp.ui.setBusy(_$createModal);
        _importSlipService.createImportSlip(input)
            .done(function () {
                _$createModal.modal('hide');
                _$createForm[0].reset();
                $('#ProductDetailsBody').empty();
                _productRowIndex = 0;
                _$importSlipTable.draw();
                abp.notify.success('Tạo phiếu nhập kho thành công!');
            })
            .always(function () {
                abp.ui.clearBusy(_$createModal);
            });
    });

    // Sửa phiếu nhập
    $(document).on('click', '.edit-import-slip', function () {
        var id = $(this).data('id');
        abp.ajax({
            url: abp.appPath + 'ImportSlips/EditModal',
            type: 'GET',
            data: { importSlipId: id },
            success: function (content) {
                _$editModal.html(content).modal('show');
                loadSuppliers();

                // Bind events cho form edit
                var $editForm = _$editModal.find('form');
                $editForm.off('submit').on('submit', function (e) {
                    e.preventDefault();

                    if (!$(this).valid()) {
                        return false;
                    }

                    var formData = $(this).serializeFormToObject();
                    var details = [];

                    $('#EditProductDetailsBody tr').each(function () {
                        var $row = $(this);
                        var detailId = $row.find('input[name*="Id"]').val();
                        var productId = $row.find('.product-id').val();
                        if (!productId) return true;

                        details.push({
                            id: detailId ? parseInt(detailId) : null,
                            productId: parseInt(productId),
                            quantity: parseInt($row.find('.quantity-input').val()),
                            unitPrice: parseFloat($row.find('.unit-price-input').val()),
                            notes: $row.find('input[name*="Notes"]').val() || ''
                        });
                    });

                    var input = {
                        id: parseInt(formData.Id),
                        importDate: new Date(formData.ImportDate),
                        supplierId: formData.SupplierId ? parseInt(formData.SupplierId) : null,
                        type: parseInt(formData.Type),
                        notes: formData.Notes || '',
                        details: details
                    };

                    abp.ui.setBusy(_$editModal);
                    _importSlipService.updateImportSlip(input)
                        .done(function () {
                            _$editModal.modal('hide');
                            _$importSlipTable.draw();
                            abp.notify.success('Cập nhật phiếu nhập thành công!');
                        })
                        .always(function () {
                            abp.ui.clearBusy(_$editModal);
                        });
                });

                // Bind events cho các nút trong edit form
                $('#EditAddProductBtn').off('click').on('click', function () {
                    addProductRow();
                });

                $(document).off('click', '.remove-product-btn').on('click', '.remove-product-btn', function () {
                    $(this).closest('tr').remove();
                    calculateTotal();
                });

                // Recalculate total for edit form
                calculateTotal();
            }
        });
    });

    // Hoàn thành phiếu nhập
    $(document).on('click', '.complete-import-slip', function () {
        var id = $(this).data('id');
        abp.message.confirm('Bạn có chắc chắn muốn hoàn thành phiếu nhập này? Hệ thống sẽ cập nhật tồn kho và tạo lịch sử giao dịch.', 'Xác nhận', function (isConfirmed) {
            if (isConfirmed) {
                abp.ui.setBusy();
                _importSlipService.completeImportSlip(id)
                    .done(function () {
                        _$importSlipTable.draw();
                        abp.notify.success('Hoàn thành phiếu nhập thành công!');
                    })
                    .always(function () {
                        abp.ui.clearBusy();
                    });
            }
        });
    });

    // Hủy phiếu nhập
    $(document).on('click', '.cancel-import-slip', function () {
        var id = $(this).data('id');
        abp.message.confirm('Bạn có chắc chắn muốn hủy phiếu nhập này?', 'Xác nhận', function (isConfirmed) {
            if (isConfirmed) {
                abp.ui.setBusy();
                _importSlipService.cancelImportSlip(id)
                    .done(function () {
                        _$importSlipTable.draw();
                        abp.notify.success('Hủy phiếu nhập thành công!');
                    })
                    .always(function () {
                        abp.ui.clearBusy();
                    });
            }
        });
    });

    // Load suppliers khi mở modal tạo mới
    _$createModal.on('shown.bs.modal', function () {
        loadSuppliers();
        _productRowIndex = 0;
        $('#ProductDetailsBody').empty();
    });

    // Reload danh sách sản phẩm khi thay đổi nhà cung cấp
    $(document).on('change', '#SupplierId', function () {
        // Nếu modal chọn sản phẩm đang mở, reload lại danh sách
        if ($('#ProductSelectModal').hasClass('show')) {
            if ($.fn.DataTable.isDataTable('#ProductSelectTable')) {
                $('#ProductSelectTable').DataTable().ajax.reload();
            }
        }
    });

    // Khởi tạo
    loadSuppliers();

})(jQuery);

