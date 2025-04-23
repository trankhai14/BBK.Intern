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


	// Kiểm tra nếu đã load thì không trigger lại
	if (!$("#mainContent").data("loaded")) {
		var $orderListTab = $(".load-content[data-view='orderlist']");
		if ($orderListTab.length) {
			$orderListTab.trigger("click");
		}
		$("#mainContent").data("loaded", true);
	}

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

		$.ajax({
			url: "/Home/FilterStatus",
			type: "GET",
			data: { orderStatus: status },
			success: function (response) {
				$("#orderList").html(response);
				$this.data("loading", false); // Reset trạng thái loading
			},
			error: function () {
				alert("Lỗi tải dữ liệu!");
				$this.data("loading", false);
			}
		});
	});

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
				console.log(response);
				$this.prop("disabled", false); // Cho phép click lại sau khi hoàn thành
				// Nếu view là _OrderList => Gọi tiếp FilterStatus để lấy danh sách đơn hàng
				if (viewName === "_OrderList") {
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
		$.ajax({
			url: "/Home/FilterStatus",
			type: "GET",
			data: { orderStatus: 5 }, // 5 nghĩa là lấy tất cả trạng thái
			cache: false,
			success: function (response) {
				$("#orderListContainer").html(response);
			},
			error: function () {
				alert("Lỗi tải danh sách đơn hàng!");
			}
		});
	}
})(jQuery);


