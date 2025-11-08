(function ($) {
    var _flashSaleService = abp.services.app.flashSale,
        l = abp.localization.getSource('MyProject'),
        _$modal = $('#FlashSaleEditModal'),
        _$form = _$modal.find('form');

    function save() {
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

        abp.ui.setBusy(_$form);
        _flashSaleService.update(flashSale).done(function () {
            _$modal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('flashSale.edited', flashSale);
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
        _$form.find('input[type=text]:first').focus();
    });
})(jQuery);

