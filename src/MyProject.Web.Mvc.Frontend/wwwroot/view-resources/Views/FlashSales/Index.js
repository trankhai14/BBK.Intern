/**
 * FlashSale Index Page - Countdown timer và auto-refresh
 */
(function ($) {
    /**
     * Khởi tạo countdown timer cho tất cả FlashSale
     */
    function initCountdownTimers() {
        $('.countdown-timer').each(function () {
            var $timer = $(this);
            var endTimeStr = $timer.data('end-time');

            if (!endTimeStr) return;

            // Parse end time
            var endTime = new Date(endTimeStr).getTime();

            // Update countdown every second
            var countdownInterval = setInterval(function () {
                var now = new Date().getTime();
                var distance = endTime - now;

                if (distance < 0) {
                    // FlashSale đã kết thúc
                    $timer.find('.countdown-display').html('<span class="text-danger">Đã kết thúc</span>');
                    clearInterval(countdownInterval);
                    // Reload trang sau 5 giây
                    setTimeout(function () {
                        location.reload();
                    }, 5000);
                    return;
                }

                // Calculate time units
                var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
                var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
                var seconds = Math.floor((distance % (1000 * 60)) / 1000);

                // Format với số 0 phía trước
                hours = hours < 10 ? '0' + hours : hours;
                minutes = minutes < 10 ? '0' + minutes : minutes;
                seconds = seconds < 10 ? '0' + seconds : seconds;

                // Update display
                $timer.find('.countdown-hours').text(hours);
                $timer.find('.countdown-minutes').text(minutes);
                $timer.find('.countdown-seconds').text(seconds);
            }, 1000);
        });
    }

    // Initialize khi page load
    $(document).ready(function () {
        initCountdownTimers();
    });

})(jQuery);

