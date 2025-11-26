(function ($) {
    var _supplierService = abp.services.app.supplier,
        l = abp.localization.getSource('MyProject'),
        _$modal = $('#SupplierEditModal');

    // Function để lấy form hiện tại (sẽ được gọi lại mỗi lần modal mở)
    function getForm() {
        return _$modal.find('form[name="supplierEditForm"]');
    }

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

    // Khởi tạo validation và event handlers khi modal được mở
    function initForm() {
        var $form = getForm();

        if ($form.length === 0) {
            return;
        }

        // Remove old validator if exists
        var validator = $form.data('validator');
        if (validator) {
            validator.destroy();
        }

        // Setup validation
        $form.validate({
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

        // Remove old event handlers to prevent duplicate
        $form.off('keypress', 'input');
        $('.save-button').off('click');

        // Setup save button handler
        $form.closest('div.modal-content').find(".save-button").on('click', function (e) {
            e.preventDefault();
            save();
        });

        // Setup Enter key handler
        $form.find('input').on('keypress', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                save();
            }
        });
    }

    function save() {
        var $form = getForm();

        if ($form.length === 0) {
            abp.notify.error('Không tìm thấy form.');
            return;
        }

        if (!$form.valid()) {
            return;
        }

        var supplier = $form.serializeFormToObject();
        // Convert checkbox value from "on" to boolean
        // serializeFormToObject() trả về "on" cho checkbox checked, cần convert thành boolean
        supplier.IsActive = $form.find('input[name="IsActive"]').is(':checked');

        abp.ui.setBusy($form);
        _supplierService.update(supplier).done(function () {
            _$modal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('supplier.edited', supplier);
        }).always(function () {
            abp.ui.clearBusy($form);
        });
    }

    // Initialize form when modal is shown
    _$modal.on('shown.bs.modal', function () {
        initForm();
        var $form = getForm();
        if ($form.length > 0) {
            $form.find('input[type=text]:first').focus();
        }
    });
})(jQuery);

