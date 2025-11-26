using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace MyProject.Authorization
{
	public class MyProjectAuthorizationProvider : AuthorizationProvider
	{
		public override void SetPermissions(IPermissionDefinitionContext context)
		{
			context.CreatePermission(PermissionNames.Pages_UserDashboard, L("UserDashboard"));

			// Quyền quản lý tài khoản và vai trò
			context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
			context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
			
			// Quyền quản lý vai trò
			var rolesPermission = context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
			rolesPermission.CreateChildPermission(PermissionNames.Pages_Roles_Create, L("CreateRole"));
			rolesPermission.CreateChildPermission(PermissionNames.Pages_Roles_Edit, L("EditRole"));
			rolesPermission.CreateChildPermission(PermissionNames.Pages_Roles_Delete, L("DeleteRole"));

			// Quyền quản lý sản phẩm
			var productsPermission = context.CreatePermission(PermissionNames.Pages_Products, L("Products"));
			productsPermission.CreateChildPermission(PermissionNames.Pages_Products_Create, L("CreateProduct"));
			productsPermission.CreateChildPermission(PermissionNames.Pages_Products_Edit, L("EditProduct"));
			productsPermission.CreateChildPermission(PermissionNames.Pages_Products_Delete, L("DeleteProduct"));

			// Quyền quản lý danh mục
			var categoriesPermission = context.CreatePermission(PermissionNames.Pages_Categories, L("Categories"));
			categoriesPermission.CreateChildPermission(PermissionNames.Pages_Categories_Create, L("CreateCategory"));
			categoriesPermission.CreateChildPermission(PermissionNames.Pages_Categories_Edit, L("EditCategory"));
			categoriesPermission.CreateChildPermission(PermissionNames.Pages_Categories_Delete, L("DeleteCategory"));

			// Quyền quản lý đơn hàng
			var ordersPermission = context.CreatePermission(PermissionNames.Pages_Orders, L("Orders"));
			ordersPermission.CreateChildPermission(PermissionNames.Pages_Orders_Create, L("CreateOrder"));
			ordersPermission.CreateChildPermission(PermissionNames.Pages_Orders_Edit, L("EditOrder"));
			ordersPermission.CreateChildPermission(PermissionNames.Pages_Orders_Delete, L("DeleteOrder"));
			ordersPermission.CreateChildPermission(PermissionNames.Pages_Orders_UpdateStatus, L("UpdateOrderStatus"));

			// Quyền quản lý nhà cung cấp
			var suppliersPermission = context.CreatePermission(PermissionNames.Pages_Suppliers, L("Suppliers"));
			suppliersPermission.CreateChildPermission(PermissionNames.Pages_Suppliers_Create, L("CreateSupplier"));
			suppliersPermission.CreateChildPermission(PermissionNames.Pages_Suppliers_Edit, L("EditSupplier"));
			suppliersPermission.CreateChildPermission(PermissionNames.Pages_Suppliers_Delete, L("DeleteSupplier"));

			// Quyền quản lý FlashSale
			var flashSalesPermission = context.CreatePermission(PermissionNames.Pages_FlashSales, L("FlashSales"));
			flashSalesPermission.CreateChildPermission(PermissionNames.Pages_FlashSales_Create, L("CreateFlashSale"));
			flashSalesPermission.CreateChildPermission(PermissionNames.Pages_FlashSales_Edit, L("EditFlashSale"));
			flashSalesPermission.CreateChildPermission(PermissionNames.Pages_FlashSales_Delete, L("DeleteFlashSale"));

			// Quyền quản lý kho hàng
			var inventoriesPermission = context.CreatePermission(PermissionNames.Pages_Inventories, L("Inventories"));
			inventoriesPermission.CreateChildPermission(PermissionNames.Pages_Inventories_View, L("ViewInventory"));
			inventoriesPermission.CreateChildPermission(PermissionNames.Pages_Inventories_Import, L("ImportInventory"));
			inventoriesPermission.CreateChildPermission(PermissionNames.Pages_Inventories_Export, L("ExportInventory"));

			// Quyền quản lý khách hàng
			var customerProfilesPermission = context.CreatePermission(PermissionNames.Pages_CustomerProfiles, L("CustomerProfiles"));
			customerProfilesPermission.CreateChildPermission(PermissionNames.Pages_CustomerProfiles_View, L("ViewCustomerProfile"));
			customerProfilesPermission.CreateChildPermission(PermissionNames.Pages_CustomerProfiles_Edit, L("EditCustomerProfile"));

			// Quyền quản lý Slider
			var slidersPermission = context.CreatePermission(PermissionNames.Pages_Sliders, L("Sliders"));
			slidersPermission.CreateChildPermission(PermissionNames.Pages_Sliders_Create, L("CreateSlider"));
			slidersPermission.CreateChildPermission(PermissionNames.Pages_Sliders_Edit, L("EditSlider"));
			slidersPermission.CreateChildPermission(PermissionNames.Pages_Sliders_Delete, L("DeleteSlider"));

			// Quyền dành cho Host (đa tenant)
			context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
		}

		private static ILocalizableString L(string name)
		{
			return new LocalizableString(name, MyProjectConsts.LocalizationSourceName);
		}
	}
}
