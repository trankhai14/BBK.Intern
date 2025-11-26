(function ($) {
    var _$modal = $('#ProductSpecificationModal'),
        _$form = _$modal.find('form'),
        l = abp.localization.getSource('MyProject');

    _$form.find('.save-button').on('click', function (e) {
        e.preventDefault();

        var formData = _$form.serializeFormToObject();
        var productId = formData.ProductId;

        // Tạo object specification từ form data
        var specification = {
            Id: formData.Id ? parseInt(formData.Id) : 0,
            ProductId: parseInt(productId),
            Sku: (formData.Sku || '').trim(),
            ModelNumber: (formData.ModelNumber || '').trim(),
            Chipset: (formData.Chipset || '').trim(),
            Ram: (formData.Ram || '').trim(),
            Storage: (formData.Storage || '').trim(),
            Screen: (formData.Screen || '').trim(),
            OperatingSystem: (formData.OperatingSystem || '').trim(),
            Battery: (formData.Battery || '').trim(),
            Camera: (formData.Camera || '').trim(),
            FrontCamera: (formData.FrontCamera || '').trim(),
            Sim: (formData.Sim || '').trim(),
            Connectivity: (formData.Connectivity || '').trim(),
            Security: (formData.Security || '').trim(),
            Charging: (formData.Charging || '').trim(),
            ChargingPort: (formData.ChargingPort || '').trim(),
            Color: (formData.Color || '').trim(),
            Warranty: (formData.Warranty || '').trim(),
            TechnicalSpecifications: (formData.TechnicalSpecifications || '').trim()
        };

        console.log("Specification to send:", specification);
        console.log("ProductId:", productId);

        abp.ui.setBusy(_$modal);

        $.ajax({
            url: abp.appPath + 'Products/SaveSpecification?productId=' + productId,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(specification),
            success: function (response) {
                if (response.success) {
                    abp.notify.success(response.message || 'Lưu thông tin kỹ thuật thành công');
                    _$modal.modal('hide');
                    // Reload datatable
                    var productTable = $('#ProductsTable').DataTable();
                    if (productTable) {
                        productTable.ajax.reload();
                    }
                } else {
                    abp.notify.error(response.message || 'Có lỗi xảy ra');
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                var errorMessage = 'Có lỗi xảy ra khi lưu thông tin kỹ thuật';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                abp.notify.error(errorMessage);
            }
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    _$modal.on('hidden.bs.modal', function () {
        _$form[0].reset();
    });

})(jQuery);

