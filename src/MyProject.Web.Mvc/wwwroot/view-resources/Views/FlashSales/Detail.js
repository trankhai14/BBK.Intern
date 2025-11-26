/**
 * FlashSale Detail Page - Xử lý thêm/sửa/xóa sản phẩm trong FlashSale
 */
(function ($) {
    // Khai báo các service và biến toàn cục
    var _flashSaleService = abp.services.app.flashSale,      // Service xử lý FlashSale
        _productService = abp.services.app.product,          // Service xử lý sản phẩm
        _inventoryService = abp.services.app.inventory,       // Service xử lý kho hàng
        l = abp.localization.getSource('MyProject'),         // Localization
        _$addProductForm = $('#AddProductForm'),              // Form thêm sản phẩm
        _$addProductModal = $('#AddProductModal'),            // Modal thêm sản phẩm
        _$productSelect = $('#ProductId'),                     // Hidden input chứa ProductId
        _$tableSelectProduct = $('#ProductSelectTable');       // DataTable chọn sản phẩm

    /**
     * Xử lý khi modal chọn sản phẩm được mở
     * Khởi tạo DataTable nếu chưa có, reload nếu đã có
     */
    $('#ProductSelectModal').on('shown.bs.modal', function () {
        // Kiểm tra xem DataTable đã được khởi tạo chưa
        if (!$.fn.DataTable.isDataTable('#ProductSelectTable')) {
            // Khởi tạo DataTable để hiển thị danh sách sản phẩm
            var productSelectTable = $('#ProductSelectTable').DataTable({
                paging: true,           // Bật phân trang
                serverSide: true,       // Xử lý phía server
                listAction: {
                    // Chỉ lấy các sản phẩm đã có trong kho (Inventory)
                    ajaxFunction: _inventoryService.getAllInventories,
                    inputFilter: function () {
                        // Lấy dữ liệu từ form search để gửi lên server (sử dụng Keyword cho Inventory)
                        var input = $('#ProductSelectSearchForm').serializeFormToObject(true);
                        return {
                            keyword: input.keyword || input.Keyword || $('#ProductSelectKeyword').val(),
                            maxResultCount: input.maxResultCount,
                            skipCount: input.skipCount,
                            sorting: input.sorting
                        };
                    }
                },
                buttons: [
                    {
                        name: 'refresh',
                        text: '<i class="fas fa-redo-alt"></i>',
                        action: function (e, dt) {
                            dt.draw(false);  // Reload lại dữ liệu
                        }
                    }
                ],
                columnDefs: [
                    // Cột 0: Tên sản phẩm (từ Inventory -> productName)
                    {
                        targets: 0,
                        data: 'productName',
                        sortable: false
                    },
                    // Cột 1: Số lượng khả dụng trong kho
                    {
                        targets: 1,
                        data: 'availableQuantity',
                        sortable: false,
                        render: function (data) {
                            var val = Number(data || 0);
                            return 'Khả dụng: ' + val.toLocaleString('vi-VN');
                        }
                    },
                    // Cột 2: Nút chọn sản phẩm
                    {
                        targets: 2,
                        data: null,
                        sortable: false,
                        defaultContent: '',
                        render: function (data, type, row) {
                            // Tạo nút chọn với các data attributes: productId, productName
                            return '<button class="btn btn-sm btn-primary select-product-btn" ' +
                                'data-product-id="' + row.productId + '" ' +
                                'data-product-name="' + (row.productName || '') + '">' +
                                '<i class="fas fa-check"></i> Chọn' +
                                '</button>';
                        }
                    }
                ]
            });

            /**
             * Xử lý sự kiện click nút "Chọn" sản phẩm trong DataTable
             * Lấy thông tin sản phẩm và điền vào form AddProduct
             */
            $('#ProductSelectTable tbody').on('click', '.select-product-btn', function () {
                // Lấy thông tin sản phẩm từ data attributes
                var productId = $(this).data('product-id');
                var productName = $(this).data('product-name');

                // Điền thông tin vào form: ID (hidden) và tên + giá (readonly input)
                $('#ProductId').val(productId);
                $('#ProductName').val(productName);

                // Đóng modal chọn sản phẩm
                $('#ProductSelectModal').modal('hide');

                // Load thông tin inventory để hiển thị số lượng khả dụng
                loadInventoryInfo(productId);

                // Thử lấy giá sản phẩm để hiển thị kèm tên (không bắt buộc)
                // Tìm nhanh theo tên, sau đó ưu tiên khớp đúng productId nếu có trong kết quả
                try {
                    _productService.search({
                        keyword: productName,
                        maxResultCount: 1,
                        skipCount: 0
                    }).done(function (res) {
                        if (res && res.items && res.items.length > 0) {
                            var found = res.items.find(function (x) { return x.id === productId; }) || res.items[0];
                            if (found && typeof found.price !== 'undefined') {
                                var formattedPrice = Number(found.price).toLocaleString('vi-VN') + ' ₫';
                                $('#ProductName').val(productName + ' - ' + formattedPrice);
                            }
                        }
                    });
                } catch (e) { /* ignore */ }

                // Hiển thị thông báo thành công
                abp.notify.success('Đã chọn sản phẩm: ' + productName);
            });

        } else {
            // Nếu DataTable đã được khởi tạo, chỉ cần reload lại dữ liệu
            $('#ProductSelectTable').DataTable().ajax.reload();
        }
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
     * Load thông tin inventory của sản phẩm
     * Hiển thị số lượng khả dụng và set max cho input FlashSaleQuantity
     * @param {number} productId - ID của sản phẩm
     */
    function loadInventoryInfo(productId) {
        // Nếu không có productId thì clear thông tin
        if (!productId) {
            $('#AvailableQuantityText').text('');
            return;
        }

        // Hiển thị loading indicator
        abp.ui.setBusy(_$addProductModal);

        // Gọi API lấy thông tin inventory
        _inventoryService.getInventoryByProductId(productId).done(function (inventory) {
            // Tính số lượng khả dụng = tổng số lượng - số lượng đã đặt
            var availableQuantity = inventory.quantity - inventory.reservedQuantity;

            // Hiển thị số lượng khả dụng
            $('#AvailableQuantityText').text('Số lượng khả dụng: ' + availableQuantity);

            // Set max cho input số lượng FlashSale
            $('#FlashSaleQuantity').attr('max', availableQuantity);
        }).fail(function () {
            // Nếu không tìm thấy inventory, hiển thị thông báo
            $('#AvailableQuantityText').text('Sản phẩm chưa có trong kho');
            $('#FlashSaleQuantity').attr('max', 0);
        }).always(function () {
            // Ẩn loading indicator
            abp.ui.clearBusy(_$addProductModal);
        });
    }

    /**
     * Xử lý submit form thêm sản phẩm vào FlashSale
     * Gọi API addProduct và reload trang sau khi thành công
     */
    _$addProductForm.on('submit', function (e) {
        e.preventDefault();  // Ngăn submit form mặc định

        // Serialize form data thành object
        var formData = _$addProductForm.serializeFormToObject();

        // Hiển thị loading indicator
        abp.ui.setBusy(_$addProductModal);

        // Gọi API thêm sản phẩm vào FlashSale
        _flashSaleService.addProduct(formData).done(function () {
            // Đóng modal
            _$addProductModal.modal('hide');

            // Reset form
            _$addProductForm[0].reset();
            $('#ProductId').val('');
            $('#ProductName').val('');
            $('#AvailableQuantityText').text('');

            // Hiển thị thông báo thành công
            abp.notify.info(l('SavedSuccessfully'));

            // Reload trang để cập nhật danh sách sản phẩm
            location.reload();
        }).always(function () {
            // Ẩn loading indicator
            abp.ui.clearBusy(_$addProductModal);
        });
    });

    /**
     * Xử lý xóa sản phẩm khỏi FlashSale
     * Hiển thị confirm dialog trước khi xóa
     */
    $(document).on('click', '.delete-product', function () {
        // Lấy thông tin sản phẩm từ data attributes
        var productId = $(this).attr("data-product-id");
        var productName = $(this).attr('data-product-name');

        // Hiển thị dialog xác nhận xóa
        abp.message.confirm(
            abp.utils.formatString(
                'Bạn có chắc chắn muốn xóa sản phẩm {0} khỏi FlashSale?',
                productName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    // Hiển thị loading indicator
                    abp.ui.setBusy();

                    // Gọi API xóa sản phẩm
                    _flashSaleService.removeProduct({
                        id: productId
                    }).done(() => {
                        // Hiển thị thông báo thành công
                        abp.notify.info(l('SuccessfullyDeleted'));

                        // Reload trang để cập nhật danh sách
                        location.reload();
                    }).always(function () {
                        // Ẩn loading indicator
                        abp.ui.clearBusy();
                    });
                }
            }
        );
    });

    /**
     * Xử lý sửa sản phẩm trong FlashSale
     * Load modal edit product và hiển thị
     */
    $(document).on('click', '.edit-product', function (e) {
        e.preventDefault();

        // Lấy ID của FlashSaleProduct từ data attribute
        var productId = $(this).attr("data-product-id");

        // Gọi AJAX để load nội dung modal edit
        abp.ajax({
            url: abp.appPath + 'FlashSales/EditProductModal?flashSaleProductId=' + productId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                // Điền nội dung vào modal và hiển thị
                $('#EditProductModal div.modal-content').html(content);
                $('#EditProductModal').modal('show');
            },
            error: function (e) {
                // Xử lý lỗi nếu cần
            }
        });
    });

    /**
     * Reset form khi modal AddProduct được mở
     * Clear tất cả các field về trạng thái ban đầu
     */
    _$addProductModal.on('shown.bs.modal', function () {
        // Reset các field về giá trị rỗng
        $('#ProductId').val('');
        $('#ProductName').val('');
        $('#AvailableQuantityText').text('');
    });
})(jQuery);

