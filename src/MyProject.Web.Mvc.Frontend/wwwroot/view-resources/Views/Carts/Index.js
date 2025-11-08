//const { each } = require("jquery");

(function ($) {
	// Kiểm tra nếu đã được khởi tạo rồi thì không khởi tạo lại
	if (window.cartHandlersInitialized) {
		console.log("Cart handlers đã được khởi tạo, bỏ qua");
		return;
	}
	window.cartHandlersInitialized = true;

	var _cartService = abp.services.app.cart;
	var processingButtons = {}; // Track các button đang được xử lý

	// Hàm helper để mở modal bằng jQuery (Bootstrap 4)
	function showDeleteModal(productId, isFromReduce, quantityInput, priceElement, unitPrice) {
		var $modal = $('#confirmDeleteModal');
		if (!$modal.length) {
			console.error("Không tìm thấy modal!");
			return;
		}

		// Xóa các event handler cũ để tránh duplicate
		$('#confirmDeleteBtn').off('click');
		$modal.off('hidden.bs.modal');

		// Khi người dùng ấn nút "Xóa" trong modal
		$('#confirmDeleteBtn').on('click', function () {
			if (isFromReduce) {
				_cartService.deleteCart(productId).done(function () {
					abp.notify.success("Xóa sản phẩm thành công");
					location.reload();
				});
			} else {
				_cartService.clearProduct(productId).done(function () {
					abp.notify.success("Xóa sản phẩm thành công!");
					location.reload();
				});
			}
		});

		// Nếu modal đóng (từ nút Cancel, nút X, hoặc click outside)
		$modal.on('hidden.bs.modal', function () {
			// Chỉ reset số lượng nếu là từ nút reduce
			if (isFromReduce && quantityInput && priceElement && unitPrice) {
				quantityInput.val(1);
				var newPrice = unitPrice * 1;
				priceElement.text(newPrice.toLocaleString('vi-VN') + " đ");
				updateTotalPrice();
			}
			// Cleanup event handlers
			$('#confirmDeleteBtn').off('click');
			$modal.off('hidden.bs.modal');
		});

		// Hiển thị modal bằng jQuery (Bootstrap 4)
		$modal.modal('show');
	}

	// Sử dụng event delegation để tránh duplicate handlers
	$(document).off('click', '.btn-reduce').on('click', '.btn-reduce', function (e) {
		e.preventDefault();
		e.stopImmediatePropagation(); // Ngăn tất cả các handler khác

		var $btn = $(this);
		var productId = $btn.data('id');
		var buttonKey = 'reduce_' + productId;

		// Kiểm tra nếu button này đang được xử lý
		if (processingButtons[buttonKey]) {
			console.log("Button đang được xử lý, bỏ qua click");
			return false;
		}

		var cartItem = $btn.closest('.cart-item');
		var quantityInput = cartItem.find('.quantity-input');
		var priceElement = cartItem.find('.product-price');

		// Lấy giá gốc từ thuộc tính data-unit-price
		var unitPriceText = priceElement.attr('data-unit-price');
		if (!unitPriceText) {
			console.error("Không tìm thấy data-unit-price!");
			return false;
		}
		// Parse giá - xử lý cả số và chuỗi
		var unitPrice = 0;
		if (typeof unitPriceText === 'number') {
			unitPrice = unitPriceText;
		} else {
			// Loại bỏ tất cả ký tự không phải số và dấu chấm
			var cleanPrice = unitPriceText.toString().replace(/[^\d.]/g, "");
			unitPrice = parseFloat(cleanPrice) || 0;
		}
		var currentQuantity = parseInt(quantityInput.val()) || 0;

		// Nếu số lượng lớn hơn 1, giảm số lượng ngay
		if (currentQuantity > 1) {
			// Đánh dấu button đang được xử lý
			processingButtons[buttonKey] = true;
			// Disable button để tránh click nhiều lần
			$btn.prop('disabled', true);

			var bool = false;
			_cartService.addToCart(productId, 1, bool).done(function () {
				abp.notify.success("Giảm số lượng sản phẩm thành công");
				var newQuantity = currentQuantity - 1;
				quantityInput.val(newQuantity);
				var newPrice = unitPrice * newQuantity;
				priceElement.text(newPrice.toLocaleString('vi-VN') + " đ");
				updateTotalPrice();
				$btn.prop('disabled', false);
				delete processingButtons[buttonKey];
			}).fail(function () {
				$btn.prop('disabled', false);
				delete processingButtons[buttonKey];
			});
		} else {
			// Nếu số lượng là 1, hiển thị modal xác nhận xóa
			showDeleteModal(productId, true, quantityInput, priceElement, unitPrice);
		}

		return false; // Ngăn event propagation
	});

	$(document).off('click', '.btn-increase').on('click', '.btn-increase', function (e) {
		e.preventDefault();
		e.stopImmediatePropagation(); // Ngăn tất cả các handler khác

		var $btn = $(this);
		var productId = $btn.data('id');
		var buttonKey = 'increase_' + productId;

		// Kiểm tra nếu button này đang được xử lý
		if (processingButtons[buttonKey]) {
			console.log("Button đang được xử lý, bỏ qua click");
			return false;
		}

		// Đánh dấu button đang được xử lý
		processingButtons[buttonKey] = true;

		var cartItem = $btn.closest('.cart-item');
		var quantityInput = cartItem.find('.quantity-input');
		var priceElement = cartItem.find('.product-price');

		// Lấy giá gốc từ thuộc tính data-unit-price
		var unitPriceText = priceElement.attr('data-unit-price');
		if (!unitPriceText) {
			console.error("Không tìm thấy data-unit-price!");
			delete processingButtons[buttonKey];
			return false;
		}
		// Parse giá - xử lý cả số và chuỗi
		var unitPrice = 0;
		if (typeof unitPriceText === 'number') {
			unitPrice = unitPriceText;
		} else {
			// Loại bỏ tất cả ký tự không phải số và dấu chấm
			var cleanPrice = unitPriceText.toString().replace(/[^\d.]/g, "");
			unitPrice = parseFloat(cleanPrice) || 0;
		}
		var currentQuantity = parseInt(quantityInput.val()) || 0;

		// Kiểm tra số lượng tối đa
		if (currentQuantity >= 10) {
			abp.notify.error("Số lượng sản phẩm không được vượt quá 10");
			delete processingButtons[buttonKey];
			return false;
		}

		// Disable button để tránh click nhiều lần
		$btn.prop('disabled', true);

		// Thêm vào giỏ hàng
		var bool = true;
		_cartService.addToCart(productId, 1, bool).done(function () {
			abp.notify.success("Thêm vào giỏ hàng thành công");
			var newQuantity = currentQuantity + 1;
			quantityInput.val(newQuantity);
			var newPrice = unitPrice * newQuantity;
			priceElement.text(newPrice.toLocaleString('vi-VN') + " đ");
			updateTotalPrice();
			$btn.prop('disabled', false);
			delete processingButtons[buttonKey];
		}).fail(function () {
			$btn.prop('disabled', false);
			delete processingButtons[buttonKey];
		});

		return false; // Ngăn event propagation
	});

	function updateTotalPrice() {
		var total = 0;

		$(".cart-item").each(function () {
			var $cartItem = $(this);
			var priceElement = $cartItem.find(".product-price");
			var quantityInput = $cartItem.find('.quantity-input');

			// Lấy giá từ data-unit-price và số lượng từ input
			var unitPriceText = priceElement.attr('data-unit-price');
			var quantity = parseInt(quantityInput.val()) || 0;

			// Bỏ qua item nếu không có giá hoặc số lượng không hợp lệ
			if (!unitPriceText || quantity <= 0) {
				return true; // Continue to next item
			}

			// Parse giá - xử lý cả số và chuỗi
			var unitPrice = 0;
			if (typeof unitPriceText === 'number') {
				unitPrice = unitPriceText;
			} else if (typeof unitPriceText === 'string') {
				// Loại bỏ tất cả ký tự không phải số và dấu chấm
				var cleanPrice = unitPriceText.replace(/[^\d.]/g, "");
				unitPrice = parseFloat(cleanPrice) || 0;
			} else {
				unitPrice = parseFloat(unitPriceText) || 0;
			}

			// Tính tổng cho item này: giá đơn vị × số lượng
			if (unitPrice > 0 && quantity > 0) {
				var itemTotal = unitPrice * quantity;
				total += itemTotal;
			}
		});

		// Làm tròn đến 2 chữ số thập phân để tránh lỗi floating point
		total = Math.round(total * 100) / 100;

		// Cập nhật tổng tạm tính và tổng cộng
		$("#subTotalPrice").text(total.toLocaleString('vi-VN') + " đ");
		$("#totalPrice").text(total.toLocaleString('vi-VN') + " đ");
	}

	// Gọi khi trang load xong
	$(document).ready(function () {
		updateTotalPrice();
	});


	// thêm giỏ hàng trong trang detail - chỉ tăng số lượng nếu đã có trong giỏ, không thêm mới
	$(document).off('click', '.btn-add-detail').on('click', '.btn-add-detail', function (e) {
		e.preventDefault();
		e.stopImmediatePropagation(); // Ngăn tất cả các handler khác
		var $btn = $(this);
		var productId = $btn.data('id');
		var quantityInput = parseInt($('#quantity-add-detail').val()) || 1;

		// Disable button để tránh click nhiều lần
		$btn.prop('disabled', true);

		// Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa (dựa vào DOM hiện tại)
		var $cartItem = $('.cart-item').filter(function () {
			return $(this).find('.btn-delete').data('id') == productId;
		});

		if ($cartItem.length > 0) {
			// Sản phẩm đã có trong giỏ, chỉ cần tăng số lượng cho nó
			var currentQty = parseInt($cartItem.find('.quantity-input').val()) || 0;
			var newQty = currentQty + quantityInput;
			if (newQty > 10) newQty = 10;

			_cartService.updateCart(productId, newQty).done(function () {
				abp.notify.success("Đã cập nhật số lượng sản phẩm trong giỏ hàng");
				$btn.prop('disabled', false);
				location.reload();
			}).fail(function () {
				$btn.prop('disabled', false);
			});
		} else {
			// Sản phẩm chưa có trong giỏ, thêm mới
			var bool = true;
			_cartService.addToCart(productId, quantityInput, bool).done(function () {
				abp.notify.success("Thêm vào giỏ hàng thành công");
				$btn.prop('disabled', false);
				location.reload();
			}).fail(function () {
				$btn.prop('disabled', false);
			});
		}
	});

	// xóa sản phẩm trong giỏ hàng
	$(document).off('click', '.btn-delete').on('click', '.btn-delete', function (e) {
		e.preventDefault();
		e.stopPropagation();
		var productId = $(this).data("id");
		// Hiển thị modal xác nhận xóa (không phải từ reduce, nên không cần reset quantity)
		showDeleteModal(productId, false, null, null, null);
	});



	// cập nhật số lượng sản phẩm trong giỏ hàng qua input
	$(document).off('change', '.quantity-input').on('change', '.quantity-input', function (e) {
		var $input = $(this);
		var productId = $input.data('id');
		var quantity = parseInt($input.val()) || 1;

		if (quantity < 1) {
			quantity = 1;
			$input.val(1);
		}
		if (quantity > 10) {
			quantity = 10;
			$input.val(10);
			abp.notify.error("Số lượng sản phẩm không được vượt quá 10");
			return;
		}

		$input.prop('disabled', true);
		_cartService.updateCart(productId, quantity).done(function () {
			location.reload();
		}).fail(function () {
			$input.prop('disabled', false);
		});
	});
	//đặt hàng
	$("#btnCheckout").on("click", function (e) {
		e.preventDefault();

		const userId = $(this).data("userid");
		const nameUser = $(this).data("nameuser");

		let orderItems = [];

		$(".cart-item").each(function () {
			const item = {
				ProductId: $(this).find(".btn-delete").data("id"),
				Quantity: $(this).find(".quantity-input").val(),
				UnitPrice: parseFloat($(this).find(".product-price").data("unit-price")),
				DiscountPrice: 0 // Nếu có giảm giá thì thay đổi giá trị này
			};
			orderItems.push(item);
		});

		const orderData = {
			UserId: userId,
			NameUser: nameUser,
			TotalAmount: orderItems.reduce((sum, item) => sum + item.UnitPrice * item.Quantity, 0),
			DiscountAmount: 0, // Nếu có giảm giá tổng thể thì tính toán
			PaymentMethod: 0,
			Items: orderItems
		};

		console.log(orderData);

		$.ajax({
			url: "/Orders/CreateOrder",
			type: "POST",
			contentType: "application/json",
			data: JSON.stringify(orderData),
			success: function (response) {
				console.log("Response từ server:", response);
				if (response.success && response.result && response.result.orderId) {
					const orderId = response.result.orderId;
					console.log("Order ID:", orderId);
					window.location.href = "/Orders/Success?orderId=" + orderId;
				} else {
					alert("Đặt hàng thành công nhưng không lấy được mã đơn hàng.");
				}
			},
			error: function (error) {
				console.error("Lỗi khi đặt hàng:", error);
				alert("Đặt hàng thất bại, vui lòng thử lại.");
			}
		});
	});

})(jQuery);


