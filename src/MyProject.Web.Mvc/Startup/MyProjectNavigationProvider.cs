using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Localization;
using MyProject.Authorization;

namespace MyProject.Web.Startup
{
    /// <summary>
    /// This class defines menus for the application.
    /// </summary>
    public class MyProjectNavigationProvider : NavigationProvider
    {
        public override void SetNavigation(INavigationProviderContext context)
        {
            context.Manager.MainMenu
                .AddItem(
                    new MenuItemDefinition(
                        PageNames.About,
                        L("About"),
                        url: "About",
                        icon: "fas fa-info-circle"
                    )
                )
                .AddItem(
                    new MenuItemDefinition(
                        PageNames.Home,
                        L("HomePage"),
                        url: "",
                        icon: "fas fa-home",
                        requiresAuthentication: true
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        PageNames.Tenants,
                        L("Tenants"),
                        url: "Tenants",
                        icon: "fas fa-building",
                        permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        PageNames.Users,
                        L("Users"),
                        url: "Users",
                        icon: "fas fa-users",
                        permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Users)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        PageNames.Roles,
                        L("Roles"),
                        url: "Roles",
                        icon: "fas fa-theater-masks",
                        permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
                    )
                )
                .AddItem(
                    new MenuItemDefinition(
                    PageNames.Products,
                    L("ProductList"),
                    url: "Products",
                    icon: "fa fa-box",
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Products)
                    )
                )
                .AddItem(
										new MenuItemDefinition(
										PageNames.Categories,
										L("CategoryList"),
										url: "Categories",
										icon: "fa fa-th-large",
										permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Categories)
                    )
								).AddItem(
										new MenuItemDefinition(
										PageNames.Sliders,
										L("SliderList"),
										url: "Sliders",
										icon: "fa fa-th-large",
										permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Sliders)
										)
								)
                .AddItem(
										new MenuItemDefinition(
										PageNames.Orders,
										L("OrderList"),
										url: "Orders",
										icon: "fa fa-shopping-cart",
										permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Orders)
										)
								)
								// Menu Quản lý kho (nhiều cấp)
								.AddItem(
										new MenuItemDefinition(
										"WarehouseManagement",
										L("WarehouseManagement"),
										icon: "fa fa-warehouse",
										permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Inventories)
										).AddItem(
											new MenuItemDefinition(
											"Inventories",
											L("Inventories"),
											url: "Inventories",
											icon: "fa fa-boxes",
											permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Inventories)
											)
										).AddItem(
											new MenuItemDefinition(
											PageNames.ImportSlips,
											L("ImportSlips"),
											url: "ImportSlips",
											icon: "fa fa-arrow-down",
											permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Inventories)
											)
										).AddItem(
											new MenuItemDefinition(
											PageNames.ExportSlips,
											L("ExportSlips"),
											url: "ExportSlips",
											icon: "fa fa-arrow-up",
											permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Inventories)
											)
										).AddItem(
											new MenuItemDefinition(
											PageNames.Stocktakings,
											L("Stocktakings"),
											url: "Stocktakings",
											icon: "fa fa-clipboard-check",
											permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Inventories)
											)
										).AddItem(
											new MenuItemDefinition(
											PageNames.InventoryTransactions,
											L("InventoryTransactions"),
											url: "InventoryTransactions",
											icon: "fa fa-history",
											permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Inventories)
											)
										)
								).AddItem(
										new MenuItemDefinition(
										PageNames.CustomerProfiles,
										L("CustomerProfiles"),
										url: "CustomerProfiles",
										icon: "fa fa-user-circle",
										permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_CustomerProfiles)
										)
								).AddItem(
										new MenuItemDefinition(
										PageNames.FlashSales,
										L("FlashSales"),
										url: "FlashSales",
										icon: "fa fa-bolt",
										permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_FlashSales)
										)
								).AddItem(
										new MenuItemDefinition(
										PageNames.Suppliers,
										L("Suppliers"),
										url: "Suppliers",
										icon: "fa fa-truck",
										permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Suppliers)
										)
								);

		}

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, MyProjectConsts.LocalizationSourceName);
        }
    }
}