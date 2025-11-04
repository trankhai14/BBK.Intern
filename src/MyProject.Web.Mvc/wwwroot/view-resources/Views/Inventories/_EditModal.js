(function ($) {
    var _inventoryService = abp.services.app.inventory,
        l = abp.localization.getSource('MyProject'),
        _$modal = $('#InventoryEditModal'),
        _$form = _$modal.find('form');

    // Validation form
    _$form.validate({
        rules: {
            Quantity: {
                min: 0,
                number: true
            },
            ReservedQuantity: {
                min: 0,
                number: true
            }
        },
        messages: {
            Quantity: {
                min: "Số lượng không được âm.",
                number: "Số lượng phải là số."
            },
            ReservedQuantity: {
                min: "Số lượng giữ không được âm.",
                number: "Số lượng giữ phải là số."
            }
        }
    });

    // Xử lý save
    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var inventory = _$form.serializeFormToObject();

        // Validate ReservedQuantity không được lớn hơn Quantity
        var quantity = parseInt(inventory.Quantity || 0);
        var reservedQuantity = parseInt(inventory.ReservedQuantity || 0);

        if (reservedQuantity > quantity) {
            abp.notify.error("Số lượng giữ không được lớn hơn số lượng trong kho");
            return;
        }

        // Convert Status to number if exists
        if (inventory.Status) {
            inventory.Status = parseInt(inventory.Status);
        }

        abp.ui.setBusy(_$modal);

        _inventoryService.updateInventory(inventory).done(() => {
            _$modal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('inventory.edited');
        }).always(() => {
            abp.ui.clearBusy(_$modal);
        });
    });

    _$modal.on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

})(jQuery);
