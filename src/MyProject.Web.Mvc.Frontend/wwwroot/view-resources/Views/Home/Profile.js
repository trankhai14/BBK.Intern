(function ($) {
	$(document).on("click", ".btn-sort", function () {
		console.log("Click nút Sắp xếp");

		sortOrder = $(this).attr("value"); // Lấy giá trị sắp xếp từ thuộc tính value
		console.log(sortOrder)
		$.ajax({
			url: "/Home/LoadMoreProducts",
			type: "GET",
			data: {
				categoryId: categoryId, // Sử dụng categoryId từ trước
				sortOrder: sortOrder,
				page: 1, // Reset lại trang về 1 khi chọn sắp xếp mới
				pageSize: 10
			},
			success: function (data) {
				console.log("Dữ liệu nhận về:", data);
				$("#productPage").html(data); // Thay thế toàn bộ danh sách sản phẩm
				page = 2; // Reset lại page về 2 để tải tiếp khi nhấn "Xem thêm"
			},
			error: function (xhr, status, error) {
				console.error("Lỗi khi tải sản phẩm:", error);
			}
		});
	});

	// Khi điều hướng tới trang UserProfile, tự động vào tab Đơn hàng đã mua và load "Tất cả"
	$(function () {
		// Phát hiện đang ở trang UserProfile thông qua URL hoặc container
		if (window.location.pathname.toLowerCase().indexOf('/home/userprofile') !== -1 || $('#mainContent').length) {
			// Đánh dấu menu Đơn hàng đã mua là active nếu chưa có
			var $orderMenu = $('.profile-menu .load-content[data-view="_OrderList"]');
			if ($orderMenu.length && !$orderMenu.hasClass('active')) {
				$('.profile-menu .load-content').removeClass('active');
				$orderMenu.addClass('active');
			}
			// Kích hoạt load danh sách Tất cả ngay lần đầu
			loadAllOrders();
		}
	});


	// Kiểm tra nếu đã load thì không trigger lại
	//if (!$("#mainContent").data("loaded")) {
	//	var $orderListTab = $(".load-content[data-view='orderlist']");
	//	if ($orderListTab.length) {
	//		$orderListTab.trigger("click");
	//	}
	//	$("#mainContent").data("loaded", true);
	//}

	// Lọc đơn hàng - Ngăn chặn việc gán nhiều lần
	$(document).off("click", ".btn-status").on("click", ".btn-status", function (e) {
		e.preventDefault(); // Ngăn chặn tải lại trang

		var $this = $(this);
		var status = $this.data("status");

		$(".btn-status").removeClass("active"); // Xóa trạng thái active của các nút khác
		$this.addClass("active"); // Đánh dấu nút hiện tại

		// Kiểm tra nếu đã gửi request trước đó, không gửi lại
		if ($this.data("loading")) return;

		$this.data("loading", true); // Đánh dấu đang load

		// Reset về trang 1 khi chọn trạng thái mới
		loadOrdersByStatus(status, 1);
		$this.data("loading", false);
	});

	// Xử lý click phân trang
	$(document).off("click", ".order-page-link").on("click", ".order-page-link", function (e) {
		e.preventDefault();

		var $this = $(this);
		var page = parseInt($this.data("page"));
		var status = $this.data("status") || 5;

		if (isNaN(page) || page < 1) return;

		loadOrdersByStatus(status, page);
	});

	// Hàm load đơn hàng theo trạng thái và trang
	function loadOrdersByStatus(status, page) {
		var $orderList = $("#orderList");

		// Hiển thị loading indicator nếu có
		if ($orderList.length) {
			$orderList.html('<div class="text-center p-4"><i class="fas fa-spinner fa-spin fa-2x"></i><p class="mt-2">Đang tải...</p></div>');
		}

		$.ajax({
			url: "/Home/FilterStatus",
			type: "GET",
			data: {
				orderStatus: status,
				page: page,
				pageSize: 10
			},
			success: function (response) {
				$orderList.html(response);
				// Scroll to top của danh sách đơn hàng
				$orderList[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
			},
			error: function () {
				abp.notify.error("Lỗi tải dữ liệu!");
				$orderList.html('<div class="alert alert-danger">Không thể tải danh sách đơn hàng. Vui lòng thử lại sau.</div>');
			}
		});
	}

	$(document).on("click", ".load-content", function (e) {
		e.preventDefault(); // Ngăn chặn tải lại trang

		var $this = $(this); // Lưu lại `this` tránh mất ngữ cảnh
		var viewName = $this.data("view"); // Lấy tên view cần load

		// Nếu đang load, không thực hiện tiếp (chống spam click)
		if ($this.prop("disabled")) return;

		$this.prop("disabled", true); // Chặn tiếp tục nhấn khi request đang xử lý

		$(".load-content").removeClass("active"); // Xóa class active cũ
		$this.addClass("active"); // Đánh dấu thẻ được chọn

		// Xóa nội dung trước khi load mới
		$("#mainContent").empty();

		$.ajax({
			url: "/Home/LoadPartialView",
			type: "GET",
			data: { nameView: viewName },
			cache: false, // Đảm bảo không dùng cache
			success: function (response) {
				$("#mainContent").html(response); // Cập nhật nội dung mới

				$this.prop("disabled", false); // Cho phép click lại sau khi hoàn thành
				// Nếu view là OrderList (dù truyền 'orderlist' hay '_OrderList') => tự load tab Tất cả
				if (viewName === "_OrderList" || viewName === "orderlist") {
					loadAllOrders();
				}
			},
			error: function () {
				alert("Lỗi tải dữ liệu!");
				$this.prop("disabled", false); // Cho phép click lại nếu có lỗi
			}
		});

	});

	$(document).on("click", ".btn-detail-order", function (e) {
		e.preventDefault(); // Ngăn chặn load lại trang
		console.log("click!!!"); // Kiểm tra sự kiện có chạy không

		var $this = $(this);
		var orderId = $this.data("orderid"); // Lấy orderId từ data-orderid (viết thường hết)

		if (!orderId) {
			alert("Lỗi: Không tìm thấy mã đơn hàng!");
			return;
		}

		if ($this.prop("disabled")) return;
		$this.prop("disabled", true);

		$("#mainContent").empty(); // Xóa nội dung trước khi load mới

		$.ajax({
			url: "/Home/GetInforDetailOrder",
			type: "GET",
			data: { orderId: orderId }, // Gửi orderId lên controller
			cache: false,
			success: function (response) {
				$("#mainContent").html(response);
				$this.prop("disabled", false);
			},
			error: function () {
				alert("Lỗi tải dữ liệu!");
				$this.prop("disabled", false);
			}
		});
	});
	function loadAllOrders() {
		// Sử dụng hàm loadOrdersByStatus với status = 5 (Tất cả) và page = 1
		loadOrdersByStatus(5, 1);
	}

	// Khi mở modal Bootstrap chứa khu vực profile, tự động load tab Đơn hàng đã mua (Tất cả)
	$(document).on('shown.bs.modal', '.modal', function () {
		var $modal = $(this);
		// Chỉ áp dụng nếu modal có vùng nội dung profile
		if ($modal.find('#mainContent').length) {
			// Ưu tiên trigger click vào menu Đơn hàng đã mua nếu có
			var $orderMenu = $modal.find('.profile-menu .load-content[data-view="_OrderList"]');
			if ($orderMenu.length) {
				$orderMenu.trigger('click');
			} else {
				// Nếu không có menu (chỉ có vùng nội dung), gọi loadAllOrders trực tiếp
				loadAllOrders();
			}
		}
	});
})(jQuery);


