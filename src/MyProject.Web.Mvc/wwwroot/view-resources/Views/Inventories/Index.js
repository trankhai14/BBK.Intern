(function ($) {
  var _inventoryService = abp.services.app.inventory,
    l = abp.localization.getSource('MyProject'),
    _$modal = $('#InventoryCreateModal'),
    _$form = _$modal.find('form'),
    _$tableInventory = $('#InventoriesTable');

  var _$inventoryTable = _$tableInventory.DataTable({
    paging: true,
    serverSide: true,
    listAction: {
      ajaxFunction: _inventoryService.getAllInventories,
      inputFilter: function () {
        var formData = $('#InventorySearchForm').serializeFormToObject(true);
        // Convert checkbox values
        if (formData.IsLowStock === 'true') {
          formData.IsLowStock = true;
        }
        if (formData.NeedReorder === 'true') {
          formData.NeedReorder = true;
        }
        // Convert Status string to number
        if (formData.Status) {
          formData.Status = parseInt(formData.Status);
        }
        return formData;
      }
    },
    buttons: [
      {
        name: 'refresh',
        text: '<i class="fas fa-redo-alt"></i>',
        action: () => _$inventoryTable.draw(false)
      }
    ],
    responsive: {
      details: { type: 'column' }
    },
    columnDefs: [
      {
        targets: 0,
        data: 'productName',
        sortable: false
      },
      {
        targets: 1,
        data: 'quantity',
        sortable: false,
        render: function (data) {
          return '<span class="font-weight-bold">' + Number(data).toLocaleString('vi-VN') + '</span>';
        }
      },
      {
        targets: 2,
        data: 'reservedQuantity',
        sortable: false,
        render: function (data) {
          return Number(data).toLocaleString('vi-VN');
        }
      },
      {
        targets: 3,
        data: 'availableQuantity',
        sortable: false,
        render: function (data) {
          return '<span class="text-success font-weight-bold">' + Number(data).toLocaleString('vi-VN') + '</span>';
        }
      },
      {
        targets: 4,
        data: 'unit',
        sortable: false
      },
      {
        targets: 5,
        data: 'statusName',
        sortable: false,
        render: function (data, type, row) {
          var badgeClass = 'bg-secondary';
          if (row.status === 1) badgeClass = 'bg-success';
          else if (row.status === 2) badgeClass = 'bg-warning';
          else if (row.status === 3) badgeClass = 'bg-danger';
          return '<span class="badge ' + badgeClass + '">' + (data || 'N/A') + '</span>';
        }
      },
      {
        targets: 6,
        data: 'minQuantity',
        sortable: false,
        render: function (data) {
          return Number(data).toLocaleString('vi-VN');
        }
      },
      {
        targets: 7,
        data: 'reorderLevel',
        sortable: false,
        render: function (data) {
          return Number(data).toLocaleString('vi-VN');
        }
      },
      {
        targets: 8,
        data: null,
        sortable: false,
        render: function (data, type, row) {
          var badges = [];
          if (row.isLowStock) {
            badges.push('<span class="badge bg-warning">Sắp hết</span>');
          }
          if (row.needReorder) {
            badges.push('<span class="badge bg-danger">Cần đặt lại</span>');
          }
          return badges.length > 0 ? badges.join(' ') : '<span class="text-muted">-</span>';
        }
      },
      {
        targets: 9,
        data: 'lastUpdateTime',
        sortable: false,
        render: data => data ? new Date(data).toLocaleString('vi-VN') : '-'
      },
      {
        targets: 10,
        data: null,
        sortable: false,
        autoWidth: true,
        defaultContent: '',
        render: (data, type, row, meta) => {
          return [
            `<button type="button" class="btn btn-sm bg-secondary edit-inventory" data-inventory-id="${row.id}" data-toggle="modal" data-target="#InventoryEditModal">`,
            `   <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
            '</button>',
            `<button type="button" class="btn btn-sm bg-danger delete-inventory" data-inventory-id="${row.id}" data-product-name="${row.productName}">`,
            `   <i class="fas fa-trash"></i> ${l('Delete')}`,
            '</button>',
            `<button type="button" class="btn btn-sm bg-info detail-inventory" data-inventory-id="${row.id}">`,
            `   <i class="fas fa-eye"></i> ${l('Details')}`,
            '</button>'
          ].join(' ');
        }
      }
    ]
  });

  // Validation form
  _$form.validate({
    rules: {
      ProductId: {
        required: true
      },
      Quantity: {
        required: true,
        min: 0,
        number: true
      },
      ReservedQuantity: {
        min: 0,
        number: true
      },
      Status: {
        required: true
      }
    },
    messages: {
      ProductId: {
        required: "Vui lòng chọn sản phẩm."
      },
      Quantity: {
        required: "Vui lòng nhập số lượng.",
        min: "Số lượng không được âm.",
        number: "Số lượng phải là số."
      },
      ReservedQuantity: {
        min: "Số lượng giữ không được âm.",
        number: "Số lượng giữ phải là số."
      },
      Status: {
        required: "Vui lòng chọn trạng thái."
      }
    }
  });

  // Xử lý delete
  $(document).on('click', '.delete-inventory', function () {
    var id = $(this).attr("data-inventory-id");
    var name = $(this).attr('data-product-name');

    abp.message.confirm(
      abp.utils.formatString(l('Bạn có chắc muốn xóa tồn kho của sản phẩm {0}?'), name),
      null,
      (isConfirmed) => {
        if (isConfirmed) {
          _inventoryService.deleteInventory(id).done(() => {
            abp.notify.info(l('SuccessfullyDeleted'));
            _$inventoryTable.ajax.reload();
          });
        }
      }
    );
  });

  // Xử lý edit
  $(document).on('click', '.edit-inventory', function (e) {
    var id = $(this).attr("data-inventory-id");
    e.preventDefault();
    abp.ajax({
      url: abp.appPath + 'Inventories/EditModal?inventoryId=' + id,
      type: 'POST',
      dataType: 'html',
      success: function (content) {
        $('#InventoryEditModal div.modal-content').html(content);
      }
    });
  });

  // Xử lý detail
  $(document).on('click', '.detail-inventory', function () {
    var inventoryId = $(this).attr("data-inventory-id");
    window.location.href = "/Inventories/Detail?inventoryId=" + inventoryId;
  });

  // Khi tạo mới inventory
  _$form.find('.save-button').on('click', (e) => {
    e.preventDefault();
    if (!_$form.valid()) {
      return;
    }

    var inventory = _$form.serializeFormToObject();

    // Validate ReservedQuantity không được lớn hơn Quantity
    if (parseInt(inventory.ReservedQuantity) > parseInt(inventory.Quantity)) {
      abp.notify.error("Số lượng giữ không được lớn hơn số lượng trong kho");
      return;
    }

    // Convert Status to number
    if (inventory.Status) {
      inventory.Status = parseInt(inventory.Status);
    }

    abp.ui.setBusy(_$modal);

    _inventoryService.createInventory(inventory).done(() => {
      _$modal.modal('hide');
      _$form[0].reset();
      $('#ProductName').val('');
      $('#ProductId').val('');
      abp.notify.info(l('SavedSuccessfully'));
      _$inventoryTable.ajax.reload();
    }).always(() => {
      abp.ui.clearBusy(_$modal);
    });
  });

  // Search
  $('.btn-search').on('click', (e) => {
    _$inventoryTable.ajax.reload();
  });

  $('.txt-search').on('keypress', (e) => {
    if (e.which == 13) {
      _$inventoryTable.ajax.reload();
      return false;
    }
  });

  // Event khi inventory được edit
  abp.event.on('inventory.edited', (data) => {
    _$inventoryTable.ajax.reload();
  });

  _$modal.on('shown.bs.modal', () => {
    _$modal.find('input:not([type=hidden]):first').focus();
  }).on('hidden.bs.modal', () => {
    _$form.clearForm();
    $('#ProductName').val('');
    $('#ProductId').val('');
  });

})(jQuery);
