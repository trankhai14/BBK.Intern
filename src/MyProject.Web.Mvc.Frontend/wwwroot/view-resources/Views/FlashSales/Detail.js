/**
 * FlashSale Detail Page - Countdown timer và xử lý mua hàng
 */
(function ($) {
    var _flashSaleService = abp.services.app.flashSale;
    var _cartService = abp.services.app.cart;

    /**
     * Khởi tạo countdown timer
     */
    function initCountdownTimer() {
        var $timer = $('.countdown-timer');
        if ($timer.length === 0) return;

        var endTimeStr = $timer.data('end-time');
        if (!endTimeStr) return;

        var endTime = new Date(endTimeStr).getTime();

        var countdownInterval = setInterval(function () {
            var now = new Date().getTime();
            var distance = endTime - now;

            if (distance < 0) {
                $timer.find('.countdown-display').html('<span class="text-danger">Đã kết thúc</span>');
                clearInterval(countdownInterval);
                setTimeout(function () {
                    location.reload();
                }, 5000);
                return;
            }

            var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
            var seconds = Math.floor((distance % (1000 * 60)) / 1000);

            hours = hours < 10 ? '0' + hours : hours;
            minutes = minutes < 10 ? '0' + minutes : minutes;
            seconds = seconds < 10 ? '0' + seconds : seconds;

            $timer.find('.countdown-hours').text(hours);
            $timer.find('.countdown-minutes').text(minutes);
            $timer.find('.countdown-seconds').text(seconds);
        }, 1000);
    }

    /**
     * Xử lý mua hàng FlashSale
     */
    $(document).on('click', '.btn-buy-now-flashsale', function () {
        var $btn = $(this);
        var productId = $btn.data('product-id');
        var flashSaleProductId = $btn.data('flashsale-product-id');
        var flashSalePrice = $btn.data('flashsale-price');
        var maxQuantity = $btn.data('max-quantity');
        var remainingQuantity = $btn.data('remaining-quantity');

        // Kiểm tra số lượng còn lại
        if (remainingQuantity <= 0) {
            abp.notify.error('Sản phẩm đã hết hàng trong FlashSale');
            return;
        }

        // Chuyển đến trang chi tiết sản phẩm để mua
        window.location.href = '/Home/GetDetailProduct?Id=' + productId;
    });

    // Initialize khi page load
    $(document).ready(function () {
        initCountdownTimer();
    });

})(jQuery);

