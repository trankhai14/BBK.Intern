$(document).ready(function () {


	// Khi nhấn nút tìm kiếm
	$('#searchForm').on('submit', function (e) {
		e.preventDefault(); // Ngăn chặn reload trang

		var keyword = $('#searchInput').val().trim(); // Lấy từ khóa tìm kiếm

		$.ajax({
			url: '/Product/SearchProductsWeb', // API endpoint
			type: 'GET',
			data: { keyword: keyword }, // Truyền tham số tìm kiếm
			success: function (response) {
				displayProducts(response.products); // Gọi hàm hiển thị sản phẩm
			},
			error: function () {
				alert('Có lỗi xảy ra khi tìm kiếm sản phẩm.');
			}
		});
	});

	// Hàm hiển thị danh sách sản phẩm
	function displayProducts(products) {
		var productList = $('#productList'); // Phần tử chứa danh sách sản phẩm
		productList.empty(); // Xóa danh sách cũ

		if (products.length === 0) {
			productList.html('<p class="text-muted">Không tìm thấy sản phẩm nào.</p>');
			return;
		}

		// Duyệt qua danh sách sản phẩm và hiển thị
		$.each(products, function (index, product) {
			var productHtml = `
                <div class="col-md-4">
                    <div class="card mb-4">
                        <img src="${product.image}" class="card-img-top" alt="${product.name}" style="height: 200px; object-fit: cover;">
                        <div class="card-body">
                            <h5 class="card-title">${product.name}</h5>
                            <p class="card-text">${product.description}</p>
                            <p class="text-danger">${Number(product.price).toLocaleString('vi-VN')} ₫</p>
                            <a href="/products/detail/${product.id}" class="btn btn-primary">Xem chi tiết</a>
                        </div>
                    </div>
                </div>`;
			productList.append(productHtml);
		});
	}
});
