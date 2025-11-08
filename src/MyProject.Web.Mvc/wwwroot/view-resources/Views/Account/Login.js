(function () {
    $('#ReturnUrlHash').val(location.hash);

    var _$form = $('#LoginForm');

    // Custom validation function
    function validateForm() {
        var isValid = true;
        _$form.find('.form-control-modern').each(function () {
            var $input = $(this);
            var $errorMsg = $input.siblings('.error-message');
            var value = $input.val().trim();
            var isRequired = $input.data('required') === true;

            // Clear previous error
            $input.removeClass('error');
            $errorMsg.text('').hide();

            // Validate required
            if (isRequired && !value) {
                isValid = false;
                $input.addClass('error');
                $errorMsg.text('Vui lòng điền thông tin này').fadeIn();
            }
        });

        return isValid;
    }

    // Validate on input blur
    _$form.find('.form-control-modern').on('blur', function () {
        var $input = $(this);
        var $errorMsg = $input.siblings('.error-message');
        var value = $input.val().trim();
        var isRequired = $input.data('required') === true;

        $input.removeClass('error');
        $errorMsg.text('').hide();

        if (isRequired && !value) {
            $input.addClass('error');
            $errorMsg.text('Vui lòng điền thông tin này').fadeIn();
        }
    });

    // Clear error on input
    _$form.find('.form-control-modern').on('input', function () {
        var $input = $(this);
        var $errorMsg = $input.siblings('.error-message');
        var value = $input.val().trim();
        var isRequired = $input.data('required') === true;

        if (value || !isRequired) {
            $input.removeClass('error');
            $errorMsg.text('').hide();
        }
    });

    _$form.submit(function (e) {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        abp.ui.setBusy(
            $('body'),
            abp.ajax({
                contentType: 'application/x-www-form-urlencoded',
                url: _$form.attr('action'),
                data: _$form.serialize()
            })
        );
    });
})();
