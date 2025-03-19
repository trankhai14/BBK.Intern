


(function ($) {
	var _sliderService = abp.services.app.slider,
		l = abp.localization.getSource('MyProject'),
		_$modal = $('#SliderEditModal'),
		_$form = _$modal.find('form');


	//$.validator.addMethod("validName", function (value, element) {
	//  return this.optional(element) || /^(?!\d+$)(?!\s+$)[\p{L}\d\s]+$/u.test(value);
	//}, "Tên sản phẩm không hợp lệ. Vui lòng nhập ít nhất một chữ cái và không chỉ chứa số hoặc dấu cách.");

	//$.validator.addMethod("validPrice", function (value, element) {
	//  return this.optional(element) || /^\d+(\.\d{1,2})?$/.test(value);
	//}, "Giá phải là số hợp lệ và có tối đa 2 chữ số thập phân.");

	//_$form.validate({
	//  rules: {
	//    Name: {
	//      required: true,
	//      validName: true
	//    },
	//    Description: {
	//      required: true
	//    },
	//    Price: {
	//      required: true,
	//      validPrice: true
	//    }
	//  },
	//  messages: {
	//    Name: {
	//      required: "Vui lòng nhập tên sản phẩm.",
	//      validName: "Tên sản phẩm không hợp lệ. Không được chỉ chứa số hoặc dấu cách."
	//    },
	//    Description: {
	//      required: "Vui lòng nhập mô tả sản phẩm."
	//    },
	//    Price: {
	//      required: "Vui lòng nhập giá sản phẩm.",
	//      validPrice: "Giá không hợp lệ"
	//    }
	//  }
	//});



	function save() {
		if (!_$form.valid()) {
			return;
		}
		var formData = new FormData(_$form[0]);

		// Lấy thông tin về file ảnh
		var imageInput = document.getElementById('newImage');
		var imageFile = imageInput.files[0];

		// Thêm file ảnh vào FormData nếu đã chọn
		if (imageFile) {
			formData.append('ImageFile', imageFile);
		}

		formData.delete("IsActive"); // Xóa dữ liệu cũ để tránh lỗi trùng lặp
		var isActive = $("#active").is(":checked") ? "true" : "false";
		formData.append("IsActive", isActive); // Luôn gửi giá trị "true" hoặc "false"

		console.log("IsActive gửi lên:", isActive); 


		abp.ui.setBusy(_$modal);
		$.ajax({
			url: abp.appPath + 'Sliders/EditAndUploadDeleteImage', // Đường dẫn đến phương thức trong controller
			type: 'POST',
			processData: false, // Quan trọng!
			contentType: false, // Quan trọng!
			data: formData,
			error: function (xhr, textStatus, errorThrown) {
				var errorMessage;
				if (xhr.responseJSON && xhr.responseJSON.errors && xhr.responseJSON.errors.length > 0) {
					errorMessage = xhr.responseJSON.errors.join("<br/>");
				} else {
					errorMessage = "Có lỗi xảy ra khi cập nhật sản phẩm (Có thể do upload ảnh không đúng định dạng .jpg, .jpeg, .png, .gif)";
				}
				$("#error-message").html(errorMessage).show();
			}
		}).done(function (slider) {
			_$modal.modal('hide');
			_$form[0].reset();
			abp.notify.info(l('Lưu thành công'));
			abp.event.trigger('slider.edited', slider);
		}).always(function () {
			abp.ui.clearBusy(_$modal);

		});
	}

	// Xử lý sự kiện khi nhấn nút "Lưu"
	_$form.closest('div.modal-content').find(".save-button").click(function (e) {
		e.preventDefault(); // Ngăn chặn reload trang
		save(); // Gọi hàm save() để cập nhật sản phẩm
	});

	// Xử lý sự kiện khi nhấn Enter trong form
	_$form.find('input').on('keypress', function (e) {
		if (e.which === 13) { // Mã 13 là Enter
			e.preventDefault();
			save();// cap nhat khi an enter
		}
	});

	// Khi mở modal, tự động focus vào ô input đầu tiên
	_$modal.on('shown.bs.modal', function () {
		_$form.find('input[type=text]:first').focus();
	});


	// Xử lý sự kiện xóa ảnh sản phẩm

	$('#deleteSliderBtn').on('click', function () {
		var sliderId = $('input[name="Id"]').val();
		console.log("Slider Id:", sliderId);
		abp.message.confirm(
			"Bạn có chắc chắn muốn xóa ảnh này?",
			"Xác nhận",
			function (isConfirmed) {
				if (isConfirmed) {
					abp.ui.setBusy();
					$.ajax({
						url: abp.appPath + 'Sliders/DeleteImage',
						type: 'POST',
						data: { sliderId: $('input[name="Id"]').val() },
						success: function (response) {
							if (response.success) {
								$('#sliderImage').attr('src', '/img/sliders/default_image.png'); // Hiển thị ảnh mặc định
								abp.notify.info("Ảnh đã được xóa thành công."); // Cập nhật thông báo
								abp.event.trigger('slider.edited', response);
							} else {
								abp.notify.error(response.message);
							}
						},
						error: function () {
							abp.notify.error("Đã có lỗi xảy ra, vui lòng thử lại.");
						},
						complete: function () {
							abp.ui.clearBusy();
						}
					});
				}
			}
		);
	});



	// Xem trước ảnh mới khi chọn file
	$('#newImage').on('change', function (event) {
		var file = event.target.files[0];
		if (file) {
			var reader = new FileReader();
			reader.onload = function (e) {
				$('#sliderImage').attr('src', e.target.result);
			};
			reader.readAsDataURL(file);
		}
	});

})(jQuery);
