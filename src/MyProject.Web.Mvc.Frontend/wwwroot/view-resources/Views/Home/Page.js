(function ($) {
  //var categoryId = null;
  var sortOrder = "";
  categoryId = getParameterByName("categoryId");
  console.log(categoryId);
  var page = 2; // Trang bắt đầu từ 2 vì trang 1 đã load sẵn
  var pageSize = 10; // Số sản phẩm mỗi lần tải

  $(document).on("click", "#loadMoreBtn", function () {
    console.log("Click nút Load More");

    $.ajax({
      url: '/Home/LoadMoreProducts',
      type: 'GET',
      data: { categoryId: categoryId, page: page, pageSize: pageSize, sortOrder: sortOrder },
      beforeSend: function () {
        console.log("Đang gửi request AJAX...");
      },
      success: function (data) {
        console.log("Dữ liệu nhận về:", data);
        if ($.trim(data).length > 0) {
          $("#productPage").append(data);
          page++;
        } else {
          $("#loadMoreBtn").hide();
        }
      },
      error: function (xhr, status, error) {
        console.error("Lỗi khi tải sản phẩm:", error);
      }
    });
  });

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
        pageSize: pageSize
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


  function getParameterByName(name) {
    let url = new URL(window.location.href);
    return url.searchParams.get(name);
  }

})(jQuery);


