(function ($) {
    'use strict';

    // Hàm mở form tạo/sửa customer profile
    window.openCustomerProfileForm = function (id) {
        var url = id ? '/Home/GetCustomerProfileForm?id=' + id : '/Home/GetCustomerProfileForm';
        $.ajax({
            url: url,
            type: 'GET',
            success: function (response) {
                $('#customerProfileModalContainer').html(response);
                $('#customerProfileModal').modal('show');

                // Xử lý submit form
                $('#customerProfileForm').off('submit').on('submit', function (e) {
                    e.preventDefault();
                    var formData = new FormData(this);
                    var isEdit = id != null && id > 0;

                    // Xử lý checkbox IsDefault
                    if ($('#IsDefault').is(':checked')) {
                        formData.set('IsDefault', 'true');
                    } else {
                        formData.set('IsDefault', 'false');
                    }

                    var submitUrl = isEdit ? '/Home/UpdateCustomerProfile' : '/Home/CreateCustomerProfile';

                    $.ajax({
                        url: submitUrl,
                        type: 'POST',
                        data: formData,
                        processData: false,
                        contentType: false,
                        success: function (response) {
                            if (response.success) {
                                abp.notify.success(response.message);
                                $('#customerProfileModal').modal('hide');
                                // Reload lại view
                                $('.load-content[data-view="_UserInfos"]').trigger('click');
                            } else {
                                abp.notify.error(response.message);
                            }
                        },
                        error: function (xhr) {
                            var errorMsg = xhr.responseJSON?.message || 'Có lỗi xảy ra';
                            abp.notify.error(errorMsg);
                        }
                    });
                });
            },
            error: function () {
                abp.notify.error('Không thể tải form');
            }
        });
    };

    // Hàm xóa customer profile
    window.deleteCustomerProfile = function (id) {
        abp.message.confirm('Bạn có chắc chắn muốn xóa thông tin này?', 'Xác nhận xóa', function (isConfirmed) {
            if (isConfirmed) {
                $.ajax({
                    url: '/Home/DeleteCustomerProfile',
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (response.success) {
                            abp.notify.success(response.message);
                            // Reload lại view
                            $('.load-content[data-view="_UserInfos"]').trigger('click');
                        } else {
                            abp.notify.error(response.message);
                        }
                    },
                    error: function (xhr) {
                        var errorMsg = xhr.responseJSON?.message || 'Có lỗi xảy ra';
                        abp.notify.error(errorMsg);
                    }
                });
            }
        });
    };

    // Hàm đặt làm mặc định
    window.setAsDefault = function (id) {
        $.ajax({
            url: '/Home/SetDefaultCustomerProfile',
            type: 'POST',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    abp.notify.success(response.message);
                    // Reload lại view
                    $('.load-content[data-view="_UserInfos"]').trigger('click');
                } else {
                    abp.notify.error(response.message);
                }
            },
            error: function (xhr) {
                var errorMsg = xhr.responseJSON?.message || 'Có lỗi xảy ra';
                abp.notify.error(errorMsg);
            }
        });
    };

    // Xử lý lỗi ảnh avatar - tránh vòng lặp vô hạn
    $(document).on('error', 'img[data-avatar]', function () {
        var $img = $(this);
        // Kiểm tra xem đã thử load default chưa
        if (!$img.data('default-tried')) {
            $img.data('default-tried', true);
            var defaultSrc = '/img/default.jpg'; // Sử dụng file default có sẵn
            if ($img.attr('src') !== defaultSrc) {
                $img.attr('src', defaultSrc);
            } else {
                // Nếu default.jpg cũng không tồn tại, ẩn ảnh
                $img.hide();
            }
        } else {
            // Nếu đã thử load default rồi mà vẫn lỗi, ẩn ảnh
            $img.hide();
        }
    });

})(jQuery);

