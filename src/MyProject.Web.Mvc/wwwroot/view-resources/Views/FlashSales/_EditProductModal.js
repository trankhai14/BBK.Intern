(function ($) {
    var _flashSaleService = abp.services.app.flashSale,
        l = abp.localization.getSource('MyProject'),
        _$modal = $('#EditProductModal'),
        _$form = _$modal.find('form');

    function save() {
        if (!_$form.valid()) {
            return;
        }

        var formData = _$form.serializeFormToObject();
        var flashSaleProductId = formData.FlashSaleProductId;

        // Remove FlashSaleProductId from the data object
        delete formData.FlashSaleProductId;

        abp.ui.setBusy(_$form);
        _flashSaleService.updateProduct(flashSaleProductId, formData).done(function () {
            _$modal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
            location.reload();
        }).always(function () {
            abp.ui.clearBusy(_$form);
        });
    }

    _$form.closest('div.modal-content').find(".save-button").click(function (e) {
        e.preventDefault();
        save();
    });

    _$form.find('input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            save();
        }
    });

    _$modal.on('shown.bs.modal', function () {
        _$form.find('input[type=number]:first').focus();
    });
})(jQuery);

