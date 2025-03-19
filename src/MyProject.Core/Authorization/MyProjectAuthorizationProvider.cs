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


			// Quyền cho người dùng quản lý sản phẩm, danh mục
			context.CreatePermission(PermissionNames.Pages_Categories, L("Categories"));
			context.CreatePermission(PermissionNames.Pages_Products, L("Products"));

			// Quyền quản lý tài khoản và vai trò
			context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
			context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
			context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));

			// Quyền dành cho Host (đa tenant)
			context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
		}

		private static ILocalizableString L(string name)
		{
			return new LocalizableString(name, MyProjectConsts.LocalizationSourceName);
		}
	}
}
