(function ($) {
	var _cartService = abp.services.app.cart;
	$('.btn-reduce').on('click', function (e) {
		e.preventDefault(); // Ngăn hành vi mặc định

		var productId = $(this).data('id'); // Lấy ID sản phẩm
		var cartItem = $(this).closest('.cart-item'); // Tìm phần tử cha chứa sản phẩm
		var quantityInput = cartItem.find('.quantity-input'); // Lấy input số lượng
		var priceElement = cartItem.find('.product-price'); // Lấy phần tử hiển thị giá sản phẩm

		// Lấy giá gốc từ thuộc tính data-unit-price, xử lý dấu phẩy nếu có
		var unitPriceText = priceElement.attr('data-unit-price');
		if (!unitPriceText) {
			console.error("Không tìm thấy data-unit-price!");
			return;
		}
		var unitPrice = parseFloat(unitPriceText.replace(/,/g, "")) || 0;
		var currentQuantity = parseInt(quantityInput.val()) || 0;

		// Nếu số lượng lớn hơn 1, giảm số lượng ngay
		if (currentQuantity > 1) {
			bool = false;
			_cartService.addToCart(productId, 1, bool).done(function () {
				abp.notify.success("Giảm số lượng sản phẩm thành công");
			});
			var newQuantity = currentQuantity - 1;
			quantityInput.val(newQuantity);
			var newPrice = unitPrice * newQuantity;
			priceElement.text(newPrice.toLocaleString('vi-VN') + " đ");
			updateTotalPrice();
		} else {
			// Nếu số lượng là 1, hiển thị modal xác nhận xóa
			$('#confirmDeleteModal').modal('show');

			// Khi người dùng ấn nút "Xóa" trong modal
			$('#confirmDeleteBtn').off('click').on('click', function () {
				_cartService.deleteCart(productId).done(function () {
					abp.notify.success("Xóa sản phẩm thành công");
					location.reload();
				});
			});

			// Nếu modal đóng mà người dùng ấn "Hủy" (với nút hủy có data-bs-dismiss="modal"),
			// modal sẽ phát sinh sự kiện hidden.bs.modal. Trong sự kiện này, đảm bảo số lượng vẫn là 1.
			$('#confirmDeleteModal').off('hidden.bs.modal').on('hidden.bs.modal', function () {
				quantityInput.val(1);
				var newPrice = unitPrice * 1;
				priceElement.text(newPrice.toLocaleString('vi-VN') + " đ");
				updateTotalPrice();
			});
		}
	});


	$('.btn-increase').on('click', function () { //Tang so luong san pham
		var productId = $(this).data('id'); // Lấy ID sản phẩm
		var cartItem = $(this).closest('.cart-item'); // Tìm phần tử cha chứa sản phẩm
		var quantityInput = cartItem.find('.quantity-input'); // Lấy input số lượng
		var priceElement = cartItem.find('.product-price'); // Lấy thẻ hiển thị giá sản phẩm

		var unitPrice = parseInt(priceElement.attr('data-unit-price')) || 0; // Lấy giá gốc
		var currentQuantity = parseInt(quantityInput.val()) || 0; // Lấy số lượng hiện tại



		//if (unitPrice === 0) {
		//	console.error("Lỗi: Không lấy được giá sản phẩm!");
		//	return;
		//}

		// Thêm vào giỏ hàng
		if (quantityInput > 10) {
			abp.notify.error("Số lượng sản phẩm không được vượt quá 10");
			return;
		} else {
			bool = true;
			_cartService.addToCart(
				productId, 1, bool
			).done(function () {
				abp.notify.success("Thêm vào giỏ hàng thành công");
			});
		}

		// Tăng số lượng
		var newQuantity = currentQuantity + 1;
		quantityInput.val(newQuantity);

		// Cập nhật giá mới
		var newPrice = unitPrice * newQuantity;
		priceElement.text(newPrice.toLocaleString('vi-VN') + " đ");

		// Cập nhật tổng tiền giỏ hàng
		updateTotalPrice();
	});

	function updateTotalPrice() {
		var total = 0;

		$(".cart-item").each(function () {
			var priceText = $(this).find(".product-price").text().replace(/[^\d]/g, ""); // Loại bỏ ký tự không phải số
			var price = parseFloat(priceText) || 0;

			total += price;
		});


		$("#totalPrice").text(total.toLocaleString('vi-VN') + " đ");
	}

	function updateProductPrice() {

	}

	// Gọi khi trang load xong
	$(document).ready(function () {
		updateTotalPrice();
	});

	// Khi tăng/giảm số lượng
	$(".btn-reduce, .btn-increase").on("click", function () {
		setTimeout(updateTotalPrice, 200); // Delay để chờ cập nhật số lượng
	});


	// thêm giỏ hàng trong trang detail
	$('.btn-add-detail').on('click', function (e) {
		var productId = $(this).data('id'); // Lấy ID sản phẩm từ thuộc tính data-id
		var quantityInput = parseInt($('#quantity-add-detail').val()) || 1;

		bool = true;
		_cartService.addToCart(
			productId, quantityInput, bool
		).done(function () {
			abp.notify.success("Thêm vào giỏ hàng thành công");
		});
	});

	// xóa sản phẩm trong giỏ hàng
	$('.btn-delete').on('click', function (e) {
		var productId = $(this).data('id');
		var quantityInput = $(this).closest('.cart-item').find('.quantity-input'); // Tìm input số lượng
		var currentQuantity = parseInt(quantityInput.val());

		if (currentQuantity == 1) {
			$('#confirmDeleteModal').modal('show'); // Hiển thị modal xác nhận xóa
			$('#confirmDeleteBtn').off('click').on('click', function () {
				_cartService.deleteCart(productId).done(function () {
					abp.notify.success("Xóa sản phẩm thành công");
					location.reload();
				});
			});
		} else {
			bool = false
			_cartService.addToCart(
				productId, 1, bool
			).done(function () {
				abp.notify.success("Xóa sản phẩm thành công");
				location.reload();
			});
		}
	});

	// cập nhật số lượng sản phẩm trong giỏ hàng qua input
	$('.quantity-input').on('change', function (e) {
		var productId = $(this).data('id');
		var quantity = parseInt($(this).val());
		bool = true;
		_cartService.updateCart(
			productId, quantity
		).done(function () {
			location.reload();
			updateTotalPrice();
		});
	});

})(jQuery);


