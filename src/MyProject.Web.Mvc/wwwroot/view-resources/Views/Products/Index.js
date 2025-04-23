(function ($) {
	var _productService = abp.services.app.product,

		l = abp.localization.getSource('MyProject'),
		_$modal = $('#ProductCreateModal'),
		_$form = _$modal.find('form'),
		_$table = $('#ProductsTable');

	var _$productTable = _$table.DataTable({
		paging: true,
		serverSide: true,
		listAction: {
			ajaxFunction: _productService.search,
			inputFilter: function () {
				return $('#ProductSearchForm').serializeFormToObject(true);
			}
		},
		buttons: [
			{
				name: 'refresh',
				text: '<i class="fas fa-redo-alt"></i>',
				action: () => _$productTable.draw(false)
			}
		],
		responsive: {
			details: {
				type: 'column'
			}
		},
		columnDefs: [
			{
				targets: 0,
				data: 'name',
				sortable: false
			},
			{
				targets: 1,
				data: 'description',
				sortable: false
			},
			{
				targets: 2,
				data: 'price',
				sortable: false,
				render: data => Number(data).toLocaleString('vi-VN') + ' ₫'
			},
			{
				targets: 3,
				data: 'creationTime',
				sortable: false,
				render: data => new Date(data).toLocaleDateString('vi-VN')
			},
			{
				targets: 4,
				data: 'image',
				sortable: false,
				render: function (data, type, row) {
					if (data) {
						return `<img src="${data}" alt="Ảnh sản phẩm" class="img-thumbnail d-block mx-auto" width="80" height="80" style="object-fit: cover;">`;
					}
					return '<span class="text-muted">Không có ảnh</span>';
				}
			},
			{
				targets: 5,
				data: 'state',
				sortable: false,
				render: function (data, type, row) {
					switch (data) {
						case 0: return '<span class="badge bg-success">Còn hàng</span>';
						case 1: return '<span class="badge bg-warning">Hết hàng</span>';
						case 2: return '<span class="badge bg-danger">Ngừng kinh doanh</span>';
					}
				}
			},

			{
				targets: 6,
				data: null,
				sortable: false,
				autoWidth: true,
				defaultContent: '',
				render: (data, type, row, meta) => {
					return [
						`   <button type="button" class="btn btn-sm bg-secondary edit-product" data-product-id="${row.id}" data-toggle="modal" data-target="#ProductEditModal">`,
						`       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
						'   </button>',
						`   <button type="button" class="btn btn-sm bg-danger delete-product" data-product-id="${row.id}" data-product-name="${row.name}">`,
						`       <i class="fas fa-trash"></i> ${l('Delete')}`,
						'   </button>',
						`   <button type="button" class="btn btn-sm bg-info detail-product" data-product-id="${row.id}" data-toggle="modal" >`,
						`       <i class="fas fa-eye"></i> ${l('Details')}`,
						'   </button>'
					].join('');
				}
			}
		]
	});
	$.validator.addMethod("validName", function (value, element) {
		return this.optional(element) || /^(?!\d+$)(?!\s+$)[A-Za-zÀ-ỹ0-9\s]+$/.test(value);
	}, "Tên sản phẩm không hợp lệ. Vui lòng nhập ít nhất một chữ cái và không chỉ chứa số hoặc dấu cách.");


	$.validator.addMethod("validPrice", function (value, element) {
		return this.optional(element) || /^\d+(\.\d{1,2})?$/.test(value);
	}, "Giá phải là số hợp lệ và có tối đa 2 chữ số thập phân.");


	$.validator.addMethod("validPrice", function (value, element) {
		return this.optional(element) || /^\d+(\.\d{1,2})?$/.test(value);
	});




	_$form.validate({
		rules: {
			Name: {
				required: true,
				validName: true
			},
			Description: {
				required: true
			},
			Price: {
				required: true,
				validPrice: true,
				min: 1000
			},
			State: {
				required: true
			},
			Image: {
				required: true,
				validImage: true
			},
			CategoryId: {
				required: true
			}
		},
		messages: {
			Name: {
				required: "Vui lòng nhập tên sản phẩm.",
				validName: "Tên sản phẩm không hợp lệ. Không được chỉ chứa số hoặc dấu cách."
			},
			Description: {
				required: "Vui lòng nhập mô tả sản phẩm."
			},
			State: {
				required: "Vui lòng chọn trạng thái sản phẩm."
			},
			Price: {
				required: "Vui lòng nhập giá sản phẩm.",
				validPrice: "Giá không hợp lệ",
				number: "Giá phải là số.",
				min: "Giá phải lớn hơn 1000."
			},
			Image: {
				required: "Vui lòng chọn ảnh"
			},
			CategoryId: {
				required: "Vui lòng chọn danh mục sản phẩm."
			},
			errorPlacement: function (error, element) {
				error.insertAfter(element); // Hiển thị lỗi ngay bên dưới ô input
			},
			highlight: function (element) {
				$(element).addClass("is-invalid"); // Thêm viền đỏ khi có lỗi
			},
			unhighlight: function (element) {
				$(element).removeClass("is-invalid"); // Xóa viền đỏ khi nhập đúng
			}
		}
	});


	_$form.find('.save-button').on('click', (e) => {
		e.preventDefault();

		if (!_$form.valid()) {
			return;
		}

		var product = _$form.serializeFormToObject(); // Lấy dữ liệu từ form
		var formData = new FormData(_$form[0]);
		abp.ui.setBusy(_$modal);
		$.ajax({

			url: abp.appPath + 'Products/Create', // Đường dẫn đến phương thức trong controller
			type: 'POST',
			processData: false, // Important! Không xử lý dữ liệu
			contentType: false, // Important!  Không đặt kiểu dữ liệu
			data: formData,
			error: function (xhr, textStatus, errorThrown) {
				var errorMessage;
				if (xhr.responseJSON && xhr.responseJSON.errors && xhr.responseJSON.errors.length > 0) {
					errorMessage = xhr.responseJSON.errors.join("<br/>");
				}
				else {
					errorMessage = "Có lỗi xảy ra khi tạo mới khách hàng (Có thể do upload ảnh không đúng định dạng (.jpg, .jpeg, .png, .gif)";
				}
				$("#error-message").html(errorMessage).show();
			}
		}).done(function () {
			/*resetDefaultImage();*/
			_$modal.modal('hide');
			_$form[0].reset();
			abp.notify.info(l('Lưu thành công'));
			_$productTable.ajax.reload();

		}).always(function () {

			abp.ui.clearBusy(_$modal);

		});
	});

	$(document).on('click', '.delete-product', function () {
		var productId = $(this).attr("data-product-id");
		var productName = $(this).attr('data-product-name');

		deleteProduct(productId, productName);
	});

	function deleteProduct(productId, productName) {
		abp.message.confirm(
			abp.utils.formatString(
				l('Bạn có chắc chắn muốn xóa sản phẩm {0}'),
				productName),
			null,
			(isConfirmed) => {
				if (isConfirmed) {
					_productService.delete({
						id: productId
					}).done(() => {
						abp.notify.info(l('SuccessfullyDeleted'));
						_$productTable.ajax.reload();
					});
				}
			}
		);
	}

	$(document).on('click', '.edit-product', function (e) {
		var productId = $(this).attr("data-product-id");

		e.preventDefault();
		abp.ajax({
			url: abp.appPath + 'Products/EditModal?productId=' + productId,
			type: 'POST',
			dataType: 'html',
			success: function (content) {
				$('#ProductEditModal div.modal-content').html(content);
			},
			error: function (e) {
			}
		});
	});

	//$(document).on('click', 'a[data-target="#ProductCreateModal"]', (e) => {
	//	$('.nav-tabs a[href="#product-details"]').tab('show')
	//});

	abp.event.on('product.edited', (data) => {
		_$productTable.ajax.reload();
	});

	_$modal.on('shown.bs.modal', () => {
		_$modal.find('input:not([type=hidden]):first').focus();
	}).on('hidden.bs.modal', () => {
		_$form.clearForm(); // Xóa toàn bộ dữ liệu trong form
		$('#previewImage').attr('src', '/img/products/default_product.png'); // Đặt lại ảnh về mặc định
	});


	$('.btn-search').on('click', (e) => {
		_$productTable.ajax.reload();
	});

	$('.txt-search').on('keypress', (e) => {
		if (e.which == 13) {
			_$productTable.ajax.reload();
			return false;
		}
	});


	//hiện thị ảnh để xem trước trong create modal
	document.getElementById('imageUpload').addEventListener('change', function (event) {
		const file = event.target.files[0]; // Lấy file ảnh
		const previewImage = document.getElementById('previewImage');

		if (file) {
			const reader = new FileReader();
			reader.onload = function (e) {
				previewImage.src = e.target.result; // Gán ảnh vào thẻ <img>
				previewImage.style.display = 'block'; // Hiển thị ảnh
			};
			reader.readAsDataURL(file); // Đọc file ảnh dưới dạng URL
		} else {
			previewImage.src = ''; // Xóa ảnh nếu không có file
			previewImage.style.display = 'none';
		}
	});


	$(document).on("click", ".detail-product", function () {
		var productId = $(this).attr("data-product-id");
		window.location.href = "/Products/Detail?productId=" + productId;
	});






})(jQuery);


