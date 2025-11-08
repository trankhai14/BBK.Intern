(function ($) {
    var _customerProfileService = abp.services.app.customerProfile,
        l = abp.localization.getSource('MyProject'),
        _$table = $('#CustomerProfilesTable');

    var _$customerProfilesTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _customerProfileService.getAll,
            inputFilter: function () {
                return $('#CustomerProfileSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$customerProfilesTable.draw(false)
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
                className: 'control',
                defaultContent: '',
            },
            {
                targets: 1,
                data: 'fullName',
                sortable: false
            },
            {
                targets: 2,
                data: 'phoneNumber',
                sortable: false
            },
            {
                targets: 3,
                data: 'address',
                sortable: false
            },
            {
                targets: 4,
                data: 'city',
                sortable: false
            },
            {
                targets: 5,
                data: 'isDefault',
                sortable: false,
                render: data => `<input type="checkbox" disabled ${data ? 'checked' : ''}>`
            },
            {
                targets: 6,
                data: null,
                sortable: false,
                autoWidth: false,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    return [
                        `   <button type="button" class="btn btn-sm bg-secondary edit-customer-profile" data-customer-profile-id="${row.id}" data-toggle="modal" data-target="#CustomerProfileEditModal">`,
                        `       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                        '   </button>',
                        `   <button type="button" class="btn btn-sm bg-danger delete-customer-profile" data-customer-profile-id="${row.id}" data-customer-profile-name="${row.fullName}">`,
                        `       <i class="fas fa-trash"></i> ${l('Delete')}`,
                        '   </button>'
                    ].join('');
                }
            }
        ]
    });

    $(document).on('click', '.delete-customer-profile', function () {
        var customerProfileId = $(this).attr("data-customer-profile-id");
        var customerProfileName = $(this).attr('data-customer-profile-name');

        deleteCustomerProfile(customerProfileId, customerProfileName);
    });

    function deleteCustomerProfile(customerProfileId, customerProfileName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                customerProfileName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _customerProfileService.deleteForAdmin({
                        id: customerProfileId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$customerProfilesTable.ajax.reload();
                    });
                }
            }
        );
    }

    $(document).on('click', '.edit-customer-profile', function (e) {
        var customerProfileId = $(this).attr("data-customer-profile-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'CustomerProfiles/EditModal?customerProfileId=' + customerProfileId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#CustomerProfileEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    abp.event.on('customerProfile.edited', (data) => {
        _$customerProfilesTable.ajax.reload();
    });

    $('.btn-search').on('click', (e) => {
        _$customerProfilesTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$customerProfilesTable.ajax.reload();
            return false;
        }
    });
})(jQuery);

