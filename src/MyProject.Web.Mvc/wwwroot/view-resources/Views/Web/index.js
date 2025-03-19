
(function ($) {
	$('#searchForm').on('submit', function (e) {
		e.preventDefault(); // Ngăn reload trang

		var keyword = $('#searchInput').val().trim(); // Lấy từ khóa tìm kiếm

		if (keyword) {
			window.location.href = "/Web/SearchProductsWeb?keyword=" + encodeURIComponent(keyword);
		} else {
			window.location.href = "/Web";
		}
	});

	$('.btn-view-detail').on('click', function (e) {
		e.preventDefault(); // Ngăn chặn hành vi mặc định của thẻ <a>

		var productId = $(this).data('id'); // Lấy ID sản phẩm từ thuộc tính data-id

		if (productId) {
			window.location.href = "/Web/DetailProductWeb?productId=" + productId; // Chuyển hướng đến trang chi tiết sản phẩm
		} else {
			console.log("Không tìm thấy sản phẩm!"); // Báo lỗi nếu không có ID
		}
	});
})(jQuery);

