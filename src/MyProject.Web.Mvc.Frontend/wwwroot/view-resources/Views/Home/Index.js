(function ($) {
	//var _productService = abp.services.app.product;
	//var _cartService = abp.services.app.cart;

	$('#searchForm').on('submit', function (e) {
		e.preventDefault(); // Ngăn reload trang

		var keyword = $('#searchInput').val().trim(); // Lấy từ khóa tìm kiếm

		if (keyword) {
			window.location.href = "/Home/SearchProductsWeb?keyword=" + encodeURIComponent(keyword);
		} else {
			window.location.href = "/";
		}
	});


	$('.btn-view-detail').on('click', function (e) {
		e.preventDefault(); // Ngăn chặn hành vi mặc định của thẻ <a>

		var productId = $(this).data('id'); // Lấy ID sản phẩm từ thuộc tính data-id

		if (productId) {
			window.location.href = "/Home/GetDetailProduct?Id=" + productId; // Chuyển hướng đến trang chi tiết sản phẩm
		} else {
			console.log("Không tìm thấy sản phẩm!"); // Báo lỗi nếu không có ID
		}
	});


	$('.cart').on('click', function (e) {
		e.preventDefault(); // Ngăn chặn hành vi mặc định của thẻ <a>

		var userId = $(this).data('id'); // Lấy ID sản phẩm từ thuộc tính data-id
		window.location.href = "/Carts/Index"
	});


	$('.btn-add-cart').on('click', function (e) {
		var _cartService = abp.services.app.cart;
		e.preventDefault(); // Ngăn chặn hành vi mặc định của thẻ <a>
			
		var productId = $(this).data('id'); // Lấy ID sản phẩm từ thuộc tính data-id
		bool = true;
		_cartService.addToCart(
			productId, 1, bool
		).done(function () {
			abp.notify.info("Thêm sản phẩm vào giỏ hàng thành công");
			window.location.href = "/Carts/Index"
		});
  });

  $('.btn-all-product').on('click', function (e) {
    // Mặc định là null
    window.location.href = "/Home/PageAllProduct"
  });

  $('.btn-all-product-byId').on('click', function (e) {
    e.preventDefault(); 
    var categoryId = $(this).data('id');

    window.location.href = "/Home/PageAllProduct?categoryId=" + categoryId;
  });


  $(document).ready(function () {
    let currentIndex = 0;
    const pageSize = 5;
    const totalProducts = $(".product-card").length;
    let animating = false; // Ngăn chặn spam click

    function updateView(direction) {
      if (animating) return; // Ngăn chặn spam click
      animating = true;

      let start = currentIndex;
      let end = currentIndex + pageSize;

      let oldProducts = $(".product-card.active");
      let newProducts = $(".product-card").slice(start, end);

      // Gán vị trí ban đầu cho nhóm sản phẩm mới
      newProducts.css({
        display: "block",
        opacity: 0,
        position: "relative",
        left: direction === "next" ? "200px" : "-200px"
      });

      // Di chuyển nhóm sản phẩm cũ ra ngoài màn hình
      oldProducts.animate(
        {
          left: direction === "next" ? "-200px" : "200px",
          opacity: 0.6
        },
        500,
        function () {
          $(this).removeClass("active").hide();
        }
      );

      // Hiển thị nhóm sản phẩm mới với hiệu ứng trượt vào
      setTimeout(() => {
        newProducts.addClass("active").animate(
          {
            left: "0",
            opacity: 1
          },
          500,
          function () {
            animating = false; // Cho phép click tiếp
          }
        );

        // Cập nhật trạng thái nút điều hướng
        $("#prevBtn").prop("disabled", currentIndex === 0);
        $("#nextBtn").prop("disabled", currentIndex + pageSize >= totalProducts);
      }, 500); // Chờ nhóm cũ trượt xong mới hiển thị nhóm mới
    }

    $("#nextBtn").click(function () {
      if (currentIndex + pageSize < totalProducts) {
        currentIndex += pageSize;
        updateView("next");
      }
    });

    $("#prevBtn").click(function () {
      if (currentIndex - pageSize >= 0) {
        currentIndex -= pageSize;
        updateView("prev");
      }
    });

    // Hiển thị sản phẩm đầu tiên khi load
    updateView("next");
  });

 


})(jQuery);


