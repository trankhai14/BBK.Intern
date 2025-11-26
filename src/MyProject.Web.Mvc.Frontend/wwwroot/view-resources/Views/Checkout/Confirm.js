(function () {
    function loadCart() {
        var $summary = $('#OrderSummary');
        $summary.html('<div class="text-muted">Đang tải giỏ hàng...</div>');

        $.getJSON('/Checkout/Summary')
            .done(function (response) {
                var payload = normalizeResponse(response);
                if (!payload || payload.success === false) {
                    renderEmptySummary(payload && payload.message ? payload.message : 'Không thể tải giỏ hàng.');
                    return;
                }

                if (!payload.items || !payload.items.length) {
                    renderEmptySummary('Giỏ hàng trống.');
                    return;
                }

                renderSummary(payload.items, payload.total);
            })
            .fail(function () {
                renderEmptySummary('Không thể tải giỏ hàng. Vui lòng thử lại.');
            });
    }

    function renderEmptySummary(message) {
        var $summary = $('#OrderSummary');
        $summary.html('<div class="text-muted">' + escapeHtml(message) + '</div>');
        $('#OrderTotalText').text(formatCurrency(0));
        $('#OrderAmountInput').val(0);
    }

    function renderSummary(items, total) {
        var $summary = $('#OrderSummary');
        $summary.empty();

        items.forEach(function (it) {
            var productName = it.productName || 'Sản phẩm';
            var quantity = it.quantity || 0;
            var unitPrice = it.unitPrice || 0;
            var lineTotal = it.lineTotal || (unitPrice * quantity);

            var imageHtml = '';
            if (it.image) {
                imageHtml = '<img src="' + encodeURI(it.image) + '" class="order-summary-thumb-img" alt="' + escapeHtml(productName) + '" />';
            } else {
                imageHtml = '<div class="order-summary-thumb-placeholder"><i class="fas fa-box-open"></i></div>';
            }

            var rowHtml =
                '<div class="order-summary-item media mb-3">' +
                '<div class="mr-3 order-summary-thumb">' + imageHtml + '</div>' +
                '<div class="media-body">' +
                '<div class="d-flex justify-content-between align-items-center mb-1">' +
                '<span class="font-weight-bold">' + escapeHtml(productName) + '</span>' +
                '<span class="text-danger font-weight-bold">' + formatCurrency(lineTotal) + '</span>' +
                '</div>' +
                '<div class="d-flex justify-content-between text-muted small">' +
                '<span>Số lượng: ' + quantity + '</span>' +
                '<span>Đơn giá: ' + formatCurrency(unitPrice) + '</span>' +
                '</div>' +
                '</div>' +
                '</div>';

            $summary.append(rowHtml);
        });

        $('#OrderTotalText').text(formatCurrency(total));
        $('#OrderAmountInput').val(Number(total || 0));
    }

    function normalizeResponse(response) {
        if (!response) {
            return null;
        }

        if (response.__abp && response.result) {
            return response.result;
        }

        return response;
    }

    function formatCurrency(value) {
        var number = Number(value || 0);
        if (isNaN(number)) {
            number = 0;
        }
        return number.toLocaleString('vi-VN') + ' ₫';
    }

    function escapeHtml(value) {
        if (value === null || value === undefined) {
            return '';
        }

        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    function initProfileSelector() {
        var $select = $('#CustomerProfileSelect');
        if (!$select.length) {
            return;
        }

        $select.on('change', function () {
            var selectedValue = $(this).val();
            if (!selectedValue) {
                return;
            }

            var $option = $(this).find('option:selected');
            applyProfile({
                fullName: $option.data('fullname') || '',
                phone: $option.data('phone') || '',
                address: $option.data('address') || ''
            });
        });

        if ($select.val()) {
            $select.trigger('change');
        }
    }

    function applyProfile(profile) {
        if (!profile) {
            return;
        }

        if (profile.fullName) {
            $('#FullNameInput').val(profile.fullName);
        }
        if (profile.phone) {
            $('#PhoneInput').val(profile.phone);
        }
        if (profile.address) {
            $('#AddressInput').val(profile.address);
        }
    }

    $(function () {
        loadCart();
        initProfileSelector();
    });
})();

