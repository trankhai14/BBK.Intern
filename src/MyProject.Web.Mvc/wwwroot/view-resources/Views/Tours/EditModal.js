(function ($) {
  var _tourService = abp.services.app.tour,
    l = abp.localization.getSource('MyProject'),
    _$modal = $('#TourEditModal'),
    _$form = _$modal.find('form');

  $.validator.addMethod("validTourName", function (value, element) {
    return this.optional(element) || /^(?!\d+$)(?!\s+$)[A-Za-zÀ-ỹ0-9\s]+$/.test(value);
  }, "Tên tour không hợp lệ. Vui lòng nhập ít nhất một chữ cái và không chỉ chứa số hoặc dấu cách.");


  $.validator.addMethod("validTourPrice", function (value, element) {
    return this.optional(element) || /^\d+(\.\d{1,2})?$/.test(value);
  }, "Giá phải là số hợp lệ và có tối đa 2 chữ số thập phân.");


  $.validator.addMethod("validPhoneNumber", function (value, element) {
		return this.optional(element) || /^\d{10,11}$/.test(value);
  });

  $.validator.addMethod("validMaxGroupSize", function (value, element) {
    var minGroupSize = $('#minGroupSize').val();
    var maxGroupSize = $('#maxGroupSize').val();
    return this.optional(element) || parseInt(maxGroupSize) >= parseInt(minGroupSize);
  });

  $.validator.addMethod("validMinGroupSize", function (value, element) {
    var minGroupSize = $('#minGroupSize').val();
    var maxGroupSize = $('#maxGroupSize').val();
    return this.optional(element) || parseInt(minGroupSize) <= parseInt(maxGroupSize);
  });

  _$form.validate({
    rules: {
      TourName: {
        required: true,
        validTourName: true
      },
      Description: {
        required: true, 
      },
      TourPrice: {
        required: true,
        validTourPrice: true
      },
      Transportation: {
        required: true
      },
      PhoneNumber: {
        required: true,
        validPhoneNumber: true
      },
      MaxGroupSize: {
        required: true,
        validMaxGroupSize: true
      },
      MinGroupSize: {
        required: true,
        validMinGroupSize: true
      },
		},
      messages: {
        TourName: {
          required: "Vui lòng nhập tên tour",
          validTourName: "Tên tour không hợp lệ. Không được chỉ chứa số hoặc dấu cách."
        },
        Description: {
          required: "Vui lòng nhập mô tả.",
					maxlength: "Mô tả không được vượt quá 300 ký tự"
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
        PhoneNumber: {
          required: "Vui lòng nhập số điện thoại",
          validPhoneNumber: "Số điện thoại không hợp lệ"
        },
        MaxGroupSize: {
          required: "Vui lòng nhập số lượng tối đa",
          validMaxGroupSize: "Số lượng tối đa phải lớn hơn hoặc bằng số lượng tối thiểu",
					min: "Số lượng tối đa không thể nhỏ hơn 1"
        },
				MinGroupSize: {
					required: "Vui lòng nhập số lượng tối thiểu",
          validMinGroupSize: "Số lượng tối thiểu phải nhỏ hơn hoặc bằng số lượng tối đa",
					min: "Số lượng tối thiểu không thể nhỏ hơn 1"
				}
      }
  });
  
  function save() {
    if (!_$form.valid()) {
      return;
    }

    var formData = new FormData(_$form[0]);

    // Lấy thông tin về file ảnh
    var imageInput = document.getElementById('newImage');
    var attachmentFile = imageInput.files[0];

    // Thêm file ảnh vào FormData nếu đã chọn
    if (attachmentFile) {
      formData.append('ImagePath', attachmentFile);
    }
    var tourId = _$form.find('input[name=Id]').val();
		console.log(tourId);

    abp.ui.setBusy(_$modal);
    $.ajax({
      url: abp.appPath + 'Tours/EditAndUpdateTour', // Đường dẫn đến phương thức trong controller
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
    }).done(function (tour) {
      _$modal.modal('hide');
      _$form[0].reset();
      abp.notify.info(l('Lưu thành công'));
      abp.event.trigger('tour.edited', tour);
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
      save();
    }
  });

  // Khi mở modal, tự động focus vào ô input đầu tiên
  _$modal.on('shown.bs.modal', function () {
    _$form.find('input[type=text]:first').focus();
  });

   //Xử lý sự kiện xóa ảnh sản phẩm
  $('#deleteImageBtn').on('click', function () {
    abp.message.confirm(
      "Bạn có chắc chắn muốn xóa ảnh này?",
      "Xác nhận",
      function (isConfirmed) {
        if (isConfirmed) {
          abp.ui.setBusy();
          $.ajax({
            url: abp.appPath + 'Tours/DeleteImage',
            type: 'POST',
            data: { tourId: $('input[name="Id"]').val() },
            success: function (response) {
              if (response.success) {
                $('#tourImage').attr('src', '/img/tours/default_image.png'); // Hiển thị ảnh mặc định
                abp.notify.info("Ảnh tour đã được xóa thành công."); // Cập nhật thông báo
                abp.event.trigger('tour.edited', response);
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


  $('#newImage').on('change', function (event) {
    var file = event.target.files[0];
    if (file) {
      var reader = new FileReader();
      reader.onload = function (e) {
        $('#tourImage').attr('src', e.target.result);
      };
      reader.readAsDataURL(file);
    }
  });


})(jQuery);
