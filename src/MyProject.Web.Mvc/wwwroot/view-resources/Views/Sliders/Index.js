(function ($) {
	var _sliderService = abp.services.app.slider,

		l = abp.localization.getSource('MyProject'),
		_$modal = $('#SliderCreateModal'),
		_$form = _$modal.find('form'),
		_$table = $('#SlidersTable');

	var _$sliderTable = _$table.DataTable({
		paging: true,
		serverSide: true,
		listAction: {
			ajaxFunction: _sliderService.getAllSlider,
			inputFilter: function () {
				return $('#SliderSearchForm').serializeFormToObject(true);
			}
		},
		buttons: [
			{
				name: 'refresh',
				text: '<i class="fas fa-redo-alt"></i>',
				action: () => _$sliderTable.draw(false)
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
				data: 'title',
				sortable: false
			},
			{
				targets: 1,
				data: 'description',
				sortable: false
			},
			{
				targets: 2,
				data: 'creationTime',
				sortable: false
			},
			{
				targets: 3,
				data: 'image',
				sortable: false,
				render: function (data, type, row) {
					if (data) {
						return `<img src="${data}" alt="Ảnh sản phẩm" class="img-thumbnail d-block mx-auto" width="120" height="120" style="object-fit: cover;">`;
					}
					return '<span class="text-muted">Không có ảnh</span>';
				}
			},
			{
				targets: 4,
				data: 'isActive',
				sortable: false,
				className: "text-center align-middle active-checkbox",
				render: data => `<input type="checkbox" ${data ? 'checked' : ''}>`
			},
			{
				targets: 5,
				data: null,
				sortable: false,
				autoWidth: true,
				defaultContent: '',
				render: (data, type, row, meta) => {
					return [
						`   <button type="button" class="btn btn-sm bg-secondary edit-slider" data-slider-id="${row.id}" data-toggle="modal" data-target="#SliderEditModal">`,
						`       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
						'   </button>',
						`   <button type="button" class="btn btn-sm bg-danger delete-slider" data-slider-id="${row.id}" data-slider-title="${row.title}">`,
						`       <i class="fas fa-trash"></i> ${l('Delete')}`,
						'   </button>',
						`   <button type="button" class="btn btn-sm bg-info detail-slider" data-slider-id="${row.id}" data-toggle="modal" >`,
						`       <i class="fas fa-eye"></i> ${l('Details')}`,
						'   </button>'
					].join('');
				}
			}
		]
	});

	$.validator.addMethod("validTitle", function (value, element) {
		return this.optional(element) || /^(?!\d+$)(?!\s+$)[A-Za-zÀ-ỹ0-9\s]+$/.test(value);
	}, "Chủ đề không hợp lệ. Vui lòng nhập ít nhất một chữ cái và không chỉ chứa số hoặc dấu cách.");


	// ✅ Thêm phương thức kiểm tra file ảnh
	$.validator.addMethod("validImage", function (value, element) {
		if (element.files.length === 0) {
			return false; // Không có file nào được chọn
		}
		let file = element.files[0];
		let validExtensions = ["image/jpeg", "image/png", "image/gif", "image/jpg"];
		return validExtensions.includes(file.type); // Kiểm tra định dạng file
	}, "Vui lòng chọn một file ảnh hợp lệ (JPG, JPEG, PNG, GIF).");




	_$form.validate({
		rules: {
			Title: {
				required: true,
				validTitle: true
			},
			Description: {
				required: true
			},
			Image: {
				required: true,
				validImage: true
			}
		},
		messages: {
			Title: {
				required: "Vui lòng nhập tên chủ đề.",
				validName: "Tên chủ đề không hợp lệ. Không được chỉ chứa số hoặc dấu cách."
			},
			Description: {
				required: "Vui lòng nhập mô tả chủ đề."
			},
			Image: {
				required: "Vui lòng chọn ảnh"
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

		var slider = _$form.serializeFormToObject(); // Lấy dữ liệu từ form
		var formData = new FormData(_$form[0]);
		formData.delete("IsActive");
		var isActive = $("#Active").is(":checked");
		formData.append("IsActive", isActive);
		abp.ui.setBusy(_$modal);
		$.ajax({

			url: abp.appPath + 'Sliders/Create', // Đường dẫn đến phương thức trong controller
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
			_$sliderTable.ajax.reload();

		}).always(function () {

			abp.ui.clearBusy(_$modal);

		});
	});

	$(document).on('click', '.delete-slider', function () {
		var sliderId = $(this).attr("data-slider-id");
		var sliderTitle = $(this).attr('data-slider-title');

		deleteSlider(sliderId, sliderTitle);
	});

	function deleteSlider(sliderId, sliderTitle) {
		abp.message.confirm(
			abp.utils.formatString(
				l('Bạn có chắc chắn muốn xóa slider {0}'),
				sliderTitle),
			null,
			(isConfirmed) => {
				if (isConfirmed) {
					_sliderService.deleteSlider({
						id: sliderId
					}).done(() => {
						abp.notify.info(l('SuccessfullyDeleted'));
						_$sliderTable.ajax.reload();
					});
				}
			}
		);
	}

	$(document).on('click', '.edit-slider', function (e) {
		var sliderId = $(this).attr("data-slider-id");

		e.preventDefault();
		abp.ajax({
			url: abp.appPath + 'Sliders/EditModal?sliderId=' + sliderId,
			type: 'POST',
			dataType: 'html',
			success: function (content) {
				$('#SliderEditModal div.modal-content').html(content);
			},
			error: function (e) {
			}
		});
	});

	$(document).on('click', 'a[data-target="#SliderCreateModal"]', (e) => {
		$('.nav-tabs a[href="#Slider-details"]').tab('show')
	});

	abp.event.on('slider.edited', (data) => {
		_$sliderTable.ajax.reload();
	});

	_$modal.on('shown.bs.modal', () => {
		_$modal.find('input:not([type=hidden]):first').focus();
	}).on('hidden.bs.modal', () => {
		_$form.clearForm(); // Xóa toàn bộ dữ liệu trong form
		$('#previewImage').attr('src', ''); // Đặt lại ảnh về mặc định
	});


	$('.btn-search').on('click', (e) => {
		_$sliderTable.ajax.reload();
	});

	$('.txt-search').on('keypress', (e) => {
		if (e.which == 13) {
			_$sliderTable.ajax.reload();
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


	$(document).on("click", ".detail-slider", function () {
		var sliderId = $(this).attr("data-slider-id");
		window.location.href = "/Sliders/Detail?sliderId=" + sliderId;
	});

	//// Thay đổi IsActive nếu click vào checkbox trong table
	//$(document).on('change', '.toggle-status', function () {
	//	let sliderId = $(this).data('id'); // Lấy ID của slider
	//	let isActive = $(this).is(':checked'); // Lấy trạng thái mới

	//	abp.message.confirm(
	//		"Bạn có chắc muốn thay đổi hoạt động?", "Xác nhận",
	//		function (isConfirmed) {
	//			if (isConfirmed) {
	//				updateSliderStatus(sliderId, isActive);
	//			} else {
	//				$(this).prop('checked', !isActive); // Hoàn tác nếu hủy
	//			}
	//		}
	//	);
	//});

	$(document).on('change', '.active-checkbox', function () {
		var checkbox = $(this); // Lưu trạng thái checkbox hiện tại
		var sliderId = checkbox.data('id'); // Lấy ID của slider

		abp.message.confirm(
			"Bạn có chắc chắn muốn thay đổi trạng thái?", // Nội dung xác nhận
			"Xác nhận", // Tiêu đề hộp thoại
			function (isConfirmed) {
				if (isConfirmed) {
					_sliderService.updateActive({ id: sliderId })
						.done(function () {
							abp.notify.success("Cập nhật trạng thái thành công!");
						})
						.fail(function () {
							abp.notify.error("Cập nhật trạng thái thất bại!");
							checkbox.prop('checked', !checkbox.prop('checked')); // Khôi phục trạng thái nếu lỗi
						});
				} else {
					checkbox.prop('checked', !checkbox.prop('checked')); // Hoàn tác nếu người dùng từ chối
				}
			}
		);
	});



})(jQuery);


