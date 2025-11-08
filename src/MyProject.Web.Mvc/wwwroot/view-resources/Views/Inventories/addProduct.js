(function ($) {
  var _productService = abp.services.app.product,
    l = abp.localization.getSource('MyProject'),
    _$tableSelectProduct = $('#ProductSelectTable');

  // Xử lý khi modal được mở
  $('#ProductSelectModal').on('shown.bs.modal', function () {
    if (!$.fn.DataTable.isDataTable('#ProductSelectTable')) {
      var productSelectTable = $('#ProductSelectTable').DataTable({
        paging: true,
        serverSide: true,
        listAction: {
          ajaxFunction: _productService.search
        },
        buttons: [
          {
            name: 'refresh',
            text: '<i class="fas fa-redo-alt"></i>',
            action: function (e, dt) {
              dt.draw(false);
            }
          }
        ],
        columnDefs: [
          {
            targets: 0,
            data: 'name',
            sortable: false
          },
          {
            targets: 1,
            data: 'price',
            sortable: false,
            render: function (data) {
              return Number(data).toLocaleString('vi-VN') + ' ₫';
            }
          },
          {
            targets: 2,
            data: null,
            sortable: false,
            defaultContent: '',
            render: function (data, type, row) {
              return '<button class="btn btn-sm btn-primary select-product-btn" data-product-id="' + row.id + '" data-product-name="' + (row.name || '') + '">' +
                '<i class="fas fa-check"></i> Chọn' +
                '</button>';
            }
          }
        ]
      });

      // Xử lý khi click nút chọn sản phẩm
      $('#ProductSelectTable tbody').on('click', '.select-product-btn', function () {
        var productId = $(this).data('product-id');
        var productName = $(this).data('product-name');

        // Điền thông tin vào form tạo inventory
        $('#ProductId').val(productId);
        $('#ProductName').val(productName);
        // Thông báo tới form để chuyển chế độ (tạo/nhập kho)
        $('#ProductId').trigger('change');

        // Đóng modal
        $('#ProductSelectModal').modal('hide');

        // Thông báo
        abp.notify.success('Đã chọn sản phẩm: ' + productName);
      });
    } else {
      // Nếu modal được mở nhiều lần, chỉ cần reload lại
      $('#ProductSelectTable').DataTable().ajax.reload();
    }
  });

  // Xử lý khi modal được đóng, xóa instance DataTable nếu cần
  $('#ProductSelectModal').on('hidden.bs.modal', function () {
    // Không cần xóa DataTable, chỉ cần giữ lại để dùng lần sau
  });

})(jQuery);