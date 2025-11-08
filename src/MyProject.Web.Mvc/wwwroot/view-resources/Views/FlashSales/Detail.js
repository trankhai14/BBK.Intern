(function ($) {
    var _flashSaleService = abp.services.app.flashSale,
        _productService = abp.services.app.product,
        _inventoryService = abp.services.app.inventory,
        l = abp.localization.getSource('MyProject'),
        _$addProductForm = $('#AddProductForm'),
        _$addProductModal = $('#AddProductModal'),
        _$productSelect = $('#ProductId');

    // Load danh sách sản phẩm vào select
    function loadProducts() {
        abp.ui.setBusy(_$addProductModal);
        _productService.getAllProducts().done(function (products) {
            _$productSelect.empty();
            _$productSelect.append('<option value="">-- Chọn sản phẩm --</option>');

            products.forEach(function (product) {
                _$productSelect.append(
                    $('<option></option>')
                        .attr('value', product.id)
                        .text(product.name + ' - ' + product.price.toLocaleString('vi-VN') + ' đ')
                        .data('price', product.price)
                );
            });
        }).always(function () {
            abp.ui.clearBusy(_$addProductModal);
        });
    }

    // Khi chọn sản phẩm, load thông tin inventory
    _$productSelect.on('change', function () {
        var productId = $(this).val();
        if (!productId) {
            $('#AvailableQuantityText').text('');
            return;
        }

        abp.ui.setBusy(_$addProductModal);
        _inventoryService.getInventoryByProductId(productId).done(function (inventory) {
            var availableQuantity = inventory.quantity - inventory.reservedQuantity;
            $('#AvailableQuantityText').text('Số lượng khả dụng: ' + availableQuantity);
            $('#FlashSaleQuantity').attr('max', availableQuantity);
        }).fail(function () {
            $('#AvailableQuantityText').text('Sản phẩm chưa có trong kho');
            $('#FlashSaleQuantity').attr('max', 0);
        }).always(function () {
            abp.ui.clearBusy(_$addProductModal);
        });
    });

    // Xử lý thêm sản phẩm vào FlashSale
    _$addProductForm.on('submit', function (e) {
        e.preventDefault();

        var formData = _$addProductForm.serializeFormToObject();

        abp.ui.setBusy(_$addProductModal);
        _flashSaleService.addProduct(formData).done(function () {
            _$addProductModal.modal('hide');
            _$addProductForm[0].reset();
            $('#AvailableQuantityText').text('');
            abp.notify.info(l('SavedSuccessfully'));
            location.reload(); // Reload để cập nhật danh sách
        }).always(function () {
            abp.ui.clearBusy(_$addProductModal);
        });
    });

    // Xử lý xóa sản phẩm khỏi FlashSale
    $(document).on('click', '.delete-product', function () {
        var productId = $(this).attr("data-product-id");
        var productName = $(this).attr('data-product-name');

        abp.message.confirm(
            abp.utils.formatString(
                'Bạn có chắc chắn muốn xóa sản phẩm {0} khỏi FlashSale?',
                productName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    abp.ui.setBusy();
                    _flashSaleService.removeProduct({
                        id: productId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        location.reload();
                    }).always(function () {
                        abp.ui.clearBusy();
                    });
                }
            }
        );
    });

    // Xử lý sửa sản phẩm trong FlashSale
    $(document).on('click', '.edit-product', function (e) {
        var productId = $(this).attr("data-product-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'FlashSales/EditProductModal?flashSaleProductId=' + productId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#EditProductModal div.modal-content').html(content);
                $('#EditProductModal').modal('show');
            },
            error: function (e) {
            }
        });
    });

    // Load products khi modal được mở
    _$addProductModal.on('shown.bs.modal', function () {
        loadProducts();
    });
})(jQuery);

