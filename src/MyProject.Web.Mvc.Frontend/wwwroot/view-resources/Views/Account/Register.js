(function () {
    var _$form = $('#RegisterForm');

    // Email validation regex
    function isValidEmail(email) {
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    // Custom validation function
    function validateForm() {
        var isValid = true;

        _$form.find('.form-control-modern').each(function () {
            var $input = $(this);
            var $errorMsg = $input.siblings('.error-message');
            var value = $input.val().trim();
            var isRequired = $input.data('required') === true;
            var isEmail = $input.data('email') === true;
            var name = $input.attr('name');

            // Clear previous error
            $input.removeClass('error');
            $errorMsg.text('').hide();

            // Validate required
            if (isRequired && !value) {
                isValid = false;
                $input.addClass('error');
                $errorMsg.text('Vui lòng điền thông tin này').fadeIn();
                return;
            }

            // Validate email format
            if (isEmail && value && !isValidEmail(value)) {
                isValid = false;
                $input.addClass('error');
                $errorMsg.text('Email không hợp lệ').fadeIn();
                return;
            }

            // Validate username (cannot be email except if it matches email address)
            if (name === 'UserName' && value) {
                var emailValue = _$form.find('input[name="EmailAddress"]').val();
                if (value !== emailValue && isValidEmail(value)) {
                    isValid = false;
                    $input.addClass('error');
                    $errorMsg.text('Tên đăng nhập không được là email (trừ email đã nhập)').fadeIn();
                    return;
                }
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
        var isEmail = $input.data('email') === true;
        var name = $input.attr('name');

        $input.removeClass('error');
        $errorMsg.text('').hide();

        if (isRequired && !value) {
            $input.addClass('error');
            $errorMsg.text('Vui lòng điền thông tin này').fadeIn();
            return;
        }

        if (isEmail && value && !isValidEmail(value)) {
            $input.addClass('error');
            $errorMsg.text('Email không hợp lệ').fadeIn();
            return;
        }

        if (name === 'UserName' && value) {
            var emailValue = _$form.find('input[name="EmailAddress"]').val();
            if (value !== emailValue && isValidEmail(value)) {
                $input.addClass('error');
                $errorMsg.text('Tên đăng nhập không được là email (trừ email đã nhập)').fadeIn();
            }
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

    // Handle form submit
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
