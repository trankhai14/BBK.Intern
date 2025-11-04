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
      ajaxFunction: _inventoryService.getAllInventories, // gọi hàm AppService
      //inputFilter: function () {
      //  //return $('#InventorySearchForm').serializeFormToObject(true);
      //}
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
        data: 'productId',
        sortable: false
      },
      {
        targets: 1,
        data: 'quantity',
        sortable: false
      },
      {
        targets: 2,
        data: 'reservedQuantity',
        sortable: false
      },
      {
        targets: 3,
        data: 'lastUpdated',
        sortable: false,
        render: data => new Date(data).toLocaleString('vi-VN')
      },
      {
        targets: 4,
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
            '</button>'
          ].join(' ');
        }
      }
    ]
  });


  

   //Xử lý delete
  $(document).on('click', '.delete-inventory', function () {
    var id = $(this).attr("data-inventory-id");
    var name = $(this).attr('data-product-name');

    abp.message.confirm(
      abp.utils.formatString(l('Bạn có chắc muốn xóa tồn kho của sản phẩm {0}?'), name),
      null,
      (isConfirmed) => {
        if (isConfirmed) {
          _inventoryService.delete({ id: id }).done(() => {
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

  // Khi tạo mới inventory
  _$form.find('.save-button').on('click', (e) => {
    e.preventDefault();
    if (!_$form.valid()) {
      return;
    }

    var inventory = _$form.serializeFormToObject();
    console.log("Inventory", inventory);

    abp.ui.setBusy(_$modal);

    _inventoryService.createInventory(inventory).done(() => {
      _$modal.modal('hide');
      _$form[0].reset();
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

  //$('#SelectProductModal').on('shown.bs.modal', function () {
  //  _$productTable.ajax.reload();
  //});

})(jQuery);
