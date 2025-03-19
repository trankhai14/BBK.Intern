(function ($) {
	var _tourService = abp.services.app.tour,

		l = abp.localization.getSource('MyProject'),
		_$modal = $('#TourCreateModal'),
		_$form = _$modal.find('form'),
		_$table = $('#ToursTable');

	var _$tourTable = _$table.DataTable({
		paging: true,
		serverSide: true,
		listAction: {
			ajaxFunction: _tourService.getAllTours,
			inputFilter: function () {
				return $('#TourSearchForm').serializeFormToObject(true);
			}
		},
		buttons: [
			{
				name: 'refresh',
				text: '<i class="fas fa-redo-alt"></i>',
				action: () => _$tourTable.draw(false)
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
				data: 'tourName',
				sortable: false
			},
			{
				targets: 1,
				data: 'minGroupSize',
				sortable: false
			},
			{
				targets: 2,
				data: 'maxGroupSize',
				sortable: false
			},
			{
				targets: 3,
				data: 'tourTypeId',
				sortable: false,
				class: 'text-center',
				render: function (data, type, row) {
					switch (data) {
						case 1:
							return `<span class="badge badge-primary">Tour du lịch nội địa</span>`
						case 2:
							return `<span class="badge badge-success">Tour du lịch liên tỉnh</span>`
						case 3:
							return `<span class="badge badge-danger">Tour du lịch quốc tế</span>`
					}
				}
			},
			{
				targets: 4,
				data: 'startDate',
				sortable: false,
				render: data => new Date(data).toLocaleDateString('vi-VN')
			},
			{
				targets: 5,
				data: 'endDate',
				sortable: false,
				render: data => new Date(data).toLocaleDateString('vi-VN')
			},
			{
				targets: 6,
				data: 'transportation',
				sortable: false
			},
			{
				targets: 7,
				data: 'tourPrice',
				sortable: false, 
				render: data => Number(data).toLocaleString('vi-VN') + ' ₫'
			},
			{
				targets: 8,
				data: 'phoneNumber',
				sortable: false
			},
			{
				targets: 9,
				data: 'description',
				sortable: false
			},
			{
				targets: 10,
				data: 'attachment',
				sortable: false,
				render: function (data, type, row) {
					if (data) {
						return `<img src="${data}" alt="Ảnh tour" class="img-thumbnail d-block mx-auto" width="80" height="80" style="object-fit: cover;">`;
					}
					return '<span class="text-muted">Không có ảnh</span>';
				}
			},
			{
				targets: 11,
				data: null,
				sortable: false,
				autoWidth: true,
				defaultContent: '',
				render: (data, type, row, meta) => {
					return [
						`   <button type="button" class="btn btn-sm bg-secondary edit-tour" data-tour-id="${row.id}" data-toggle="modal" data-target="#TourEditModal">`,
						`       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
						'   </button>',
						`   <button type="button" class="btn btn-sm bg-danger delete-tour" data-tour-id="${row.id}" data-tour-name="${row.tourName}">`,
						`       <i class="fas fa-trash"></i> ${l('Delete')}`,
						'   </button>'
					].join('');
				}
			}
		]
	});

	$.validator.addMethod("validTourName", function (value, element) {
		return this.optional(element) || /^(?!\d+$)(?!\s+$)[A-Za-zÀ-ỹ0-9\s]+$/.test(value);
	}, "Tên tour không hợp lệ. Vui lòng nhập ít nhất một chữ cái và không chỉ chứa số hoặc dấu cách.");


	$.validator.addMethod("validTourPrice", function (value, element) {
		return this.optional(element) || /^\d+(\.\d{1,2})?$/.test(value);
	}, "Giá phải là số hợp lệ và có tối đa 2 chữ số thập phân.");

	$.validator.addMethod("validEndDate", function (value, element) {
		var startDate = $('#StartDate').val();
		return this.optional(element) || new Date(value).getTime() >= new Date(startDate).getTime();
	});

	$.validator.addMethod("validMinGroupSize", function (value, element) {
		var minGroupSize = $('#MinGroupSize').val();
		var maxGroupSize = $('#MaxGroupSize').val();
		return this.optional(element) || parseInt(minGroupSize) <= parseInt(maxGroupSize);
	});

	$.validator.addMethod("validMaxGroupSize", function (value, element) {
		var minGroupSize = $('#MinGroupSize').val();
		var maxGroupSize = $('#MaxGroupSize').val();
		return this.optional(element) || parseInt(maxGroupSize) >= parseInt(minGroupSize);
	});

	$.validator.addMethod("validPhoneNumber", function (value, element) {
		return this.optional(element) || /^\d{10,11}$/.test(value);
	});

	_$form.validate({
		rules: {
			TourName: {
				required: true,
				validTourName: true
			},
			Description: {
				required: true
			},
			TourPrice: {
				required: true,
				validTourPrice: true
			},
			Transportation: {
				required: true
			},
			TourTypeId: {
				required: true
			},
			MaxGroupSize: {
				required: true,
				validMaxGroupSize: true
			},
			MinGroupSize: {
				required: true,
				validMinGroupSize: true
			},
			StartDate: {
				required: true
			},
			EndDate: {
				required: true,
				validEndDate: true
			},
			PhoneNumber: {
				required: true,
				validPhoneNumber: true
			}
		},
		messages: {
			TourName: {
				required: "Vui lòng nhập tên tour",
				validTourName: "Tên tour không hợp lệ. Không được chỉ chứa số hoặc dấu cách."
			},
			Description: {
				required: "Vui lòng nhập mô tả."
			},
			TourPrice: {
				required: "Vui lòng nhập giá .",
				validTourPrice: "Giá không hợp lệ",
				number: "Giá phải là số.",
				min: "Giá không thể nhỏ hơn 0.",
				max: "Giá không thể lớn hơn 100 tỷ."
			},
			Transportation: {
				required: "Vui lòng chọn phương tiện"
			},
			TourTypeId: {
				required: "Vui lòng chọn loại tour"
			},
			MaxGroupSize: {
				required: "Vui lòng nhập số lượng khách tối đa",
				validMaxGroupSize: "Số lượng tối đa phải lớn hơn hoặc bằng số lượng tối thiểu",
				min: "Số lượng tối đa không thể nhỏ hơn 1"
			},
			MinGroupSize: {
				required: "Vui lòng nhập số lượng khách tối thiểu",
				validMinGroupSize: "Số lượng tối thiểu phải nhỏ hơn hoặc bằng số lượng tối đa",
				min: "Số lượng tối thiểu không thể nhỏ hơn 1"
			},
			StartDate: {
				required: "Vui lòng chọn ngày bắt đầu"
			},
			EndDate: {
				required: "Vui lòng chọn ngày kết thúc",
				validEndDate: "Ngày kết thúc phải sau ngày bắt đầu"
			},
			PhoneNumber: {
				required: "Vui lòng nhập số điện thoại",
				validPhoneNumber: "Số điện thoại không hợp lệ"
			}
		}
	});



	_$form.find('.save-button').on('click', (e) => {
		e.preventDefault();

		if (!_$form.valid()) {
			return;
		}

		var tour = _$form.serializeFormToObject(); // Lấy dữ liệu từ form
		var formData = new FormData(_$form[0]);
		abp.ui.setBusy(_$modal);
		$.ajax({

			url: abp.appPath + 'Tours/Create', // Đường dẫn đến phương thức trong controller
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
			_$tourTable.ajax.reload();

		}).always(function () {

			abp.ui.clearBusy(_$modal);

		});
	});


	$(document).on('click', 'a[data-target="#TourCreateModal"]', (e) => {
		$('.nav-tabs a[href="#tours-details"]').tab('show')
	});

	$(document).on('click', '.edit-tour', function (e) {
		var tourId = $(this).attr("data-tour-id");
		console.log(tourId);

		e.preventDefault();
		abp.ajax({
			url: abp.appPath + 'Tours/EditTourModal?tourId=' + tourId,
			type: 'POST',
			dataType: 'html',
			success: function (content) {
				$('#TourEditModal div.modal-content').html(content);
			},
			error: function (e) {
			}
		});
	});

	abp.event.on('tour.edited', (data) => {
		_$tourTable.ajax.reload();
	});

	$(document).on('click', '.delete-tour', function () {
		var tourId = $(this).attr("data-tour-id");
		var tourName = $(this).attr('data-tour-name');

		deleteTour(tourId, tourName);
	});

	function deleteTour(tourId, tourName) {
		abp.message.confirm(
			abp.utils.formatString(
				l('Bạn có chắc chắn muốn xóa tour {0}?'),
				tourName
			),
			null,
			function (isConfirmed) {
				if (isConfirmed) {
					$.ajax({
						url: abp.appPath + 'Tours/DeleteTourAndImg?tourId' + tourId, // Đường dẫn API của Controller
						type: 'POST', // Hoặc 'DELETE' nếu API hỗ trợ
						data: { id: tourId },
						success: function (response) {
							if (response.success) {
								abp.notify.info('Đã xóa tour thành công!');
								_$tourTable.ajax.reload(); // Load lại bảng dữ liệu
							} else {
								abp.notify.error('Xóa tour thất bại!');
							}
						},
						error: function () {
							abp.notify.error('Đã xảy ra lỗi khi xóa tour!');
						}
					});
				}
			}
		);
	}




	_$modal.on('shown.bs.modal', () => {
		_$modal.find('input:not([type=hidden]):first').focus();
	}).on('hidden.bs.modal', () => {
		_$form.clearForm(); // Xóa toàn bộ dữ liệu trong form
		//$('#previewImage').attr('src', '/img/products/default_product.png'); // Đặt lại ảnh về mặc định
	});

	$('.btn-search').on('click', (e) => {
		_$tourTable.ajax.reload();
	});

	$('.txt-search').on('keypress', (e) => {
		if (e.which == 13) {
			_$tourTable.ajax.reload();
			return false;
		}
	});

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

})(jQuery);