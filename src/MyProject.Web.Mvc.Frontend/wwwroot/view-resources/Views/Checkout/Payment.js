(function () {
    var checkInterval = null;
    var orderId = null;
    var paymentReference = null;
    var isPaid = false;

    $(function () {
        // Lấy orderId và paymentReference từ view
        orderId = $('#btnPaid').data('order-id');
        paymentReference = $('#paymentReference').text() || $('#paymentReference').data('reference');
        isPaid = $('#paymentStatus').data('is-paid') === true || $('#paymentStatus').data('is-paid') === 'true';

        // Auto-check mỗi 10 giây (nếu chưa thanh toán)
        if (orderId && !isPaid) {
            startAutoCheck();
        }

        // Nút "Đã thanh toán" thủ công
        $('#btnPaid').on('click', function (e) {
            e.preventDefault();
            confirmPayment();
        });

        // Dừng auto-check khi rời trang
        $(window).on('beforeunload', function () {
            stopAutoCheck();
        });
    });

    function startAutoCheck() {
        // Kiểm tra mỗi 10 giây
        checkInterval = setInterval(function () {
            checkPaymentStatus();
        }, 10000); // 10 giây

        // Kiểm tra ngay lần đầu
        checkPaymentStatus();
    }

    function stopAutoCheck() {
        if (checkInterval) {
            clearInterval(checkInterval);
            checkInterval = null;
        }
    }

    function checkPaymentStatus() {
        if (!orderId || isPaid) {
            stopAutoCheck();
            return;
        }

        abp.ajax({
            url: '/Checkout/CheckPaymentStatus',
            type: 'GET',
            data: { orderId: orderId },
            dataType: 'json'
        }).done(function (response) {
            if (response && response.isPaid) {
                // Đã thanh toán, dừng auto-check và redirect
                stopAutoCheck();
                isPaid = true;
                showSuccessMessage('Thanh toán đã được xác nhận!');
                updatePaymentStatus(true);
                setTimeout(function () {
                    window.location.href = response.redirectUrl || '/Checkout/Success?orderCode=' + paymentReference;
                }, 2000);
            } else if (response && response.hasTransaction) {
                // Có giao dịch nhưng chưa xác nhận, hiển thị thông báo
                showInfoMessage('Đã phát hiện giao dịch. Đang xác nhận...');
                // Tự động xác nhận
                confirmPayment();
            }
        }).fail(function () {
            // Lỗi khi check, không làm gì (sẽ thử lại lần sau)
        });
    }

    function confirmPayment() {
        if (!orderId) {
            abp.notify.error('Không xác định được đơn hàng.');
            return;
        }

        if (isPaid) {
            window.location.href = '/Checkout/Success?orderCode=' + paymentReference;
            return;
        }

        abp.ui.setBusy($('body'));

        abp.ajax({
            url: '/Checkout/ConfirmPaid',
            type: 'POST',
            data: JSON.stringify({ orderId: orderId }),
            contentType: 'application/json',
            dataType: 'json'
        }).done(function (response) {
            if (response && response.success) {
                stopAutoCheck();
                isPaid = true;
                showSuccessMessage(response.message || 'Xác nhận thanh toán thành công!');
                updatePaymentStatus(true);

                if (response.redirectUrl) {
                    setTimeout(function () {
                        window.location.href = response.redirectUrl;
                    }, 1500);
                } else {
                    setTimeout(function () {
                        window.location.href = '/Checkout/Success?orderCode=' + paymentReference;
                    }, 1500);
                }
            } else {
                var message = response && response.message
                    ? response.message
                    : 'Chưa tìm thấy giao dịch thanh toán. Vui lòng kiểm tra lại hoặc liên hệ hỗ trợ.';

                if (response && response.canRetry) {
                    showWarningMessage(message + ' Hệ thống sẽ tiếp tục kiểm tra tự động...');
                    // Tiếp tục auto-check
                } else {
                    abp.notify.error(message);
                }
            }
        }).always(function () {
            abp.ui.clearBusy($('body'));
        });
    }

    function showSuccessMessage(message) {
        abp.notify.success(message, 'Thành công');
    }

    function showInfoMessage(message) {
        abp.notify.info(message, 'Thông tin');
    }

    function showWarningMessage(message) {
        abp.notify.warn(message, 'Cảnh báo');
    }

    function updatePaymentStatus(paid) {
        var statusAlert = $('#paymentStatusAlert');
        if (statusAlert.length) {
            if (paid) {
                statusAlert.removeClass('alert-info').addClass('alert-success');
                statusAlert.html('<i class="fas fa-check-circle"></i> Đơn hàng đã được thanh toán thành công!');
            }
        }
    }
})();
