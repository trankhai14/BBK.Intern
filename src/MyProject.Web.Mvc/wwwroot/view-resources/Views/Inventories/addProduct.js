(function ($) {
  var _productService = abp.services.app.product,
    l = abp.localization.getSource('MyProject'),
    _$tableSelectProduct = $('#ProductSelectTable');

  $('#ProductSelectModal').on('shown.bs.modal', function () {
    if (!$.fn.DataTable.isDataTable('#ProductSelectTable')) {
      $('#ProductSelectTable').DataTable({
        paging: true,
        serverSide: true,
        listAction: {
          ajaxFunction: abp.services.app.product.search
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
          { targets: 0, data: 'name', sortable: false },
          {
            targets: 1,
            data: 'price',
            sortable: false,
            render: data => Number(data).toLocaleString('vi-VN') + ' ₫'
          },
          {
            targets: 2,
            data: null,
            sortable: false,
            render: (data, type, row) => `
                        <button class="btn btn-sm bg-secondary edit-product" data-id="${row.id}">
                            <i class="fas fa-pencil-alt"></i> Sửa
                        </button>
                        <button class="btn btn-sm bg-danger delete-product" data-id="${row.id}">
                            <i class="fas fa-trash"></i> Xóa
                        </button>
                    `
          }
        ]
      });
    } else {
      // Nếu modal được mở nhiều lần, chỉ cần reload lại
      $('#ProductSelectTable').DataTable().ajax.reload();
    }
  });

})(jQuery);