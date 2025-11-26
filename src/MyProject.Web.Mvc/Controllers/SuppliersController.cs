using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MyProject.Controllers;
using MyProject.Suppliers;
using MyProject.Suppliers.Dto;
using MyProject.Web.Models.Suppliers;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using MyProject.Authorization;
using Abp.AspNetCore.Mvc.Authorization;

namespace MyProject.Web.Controllers
{
	/// <summary>
	/// Controller quản lý Nhà cung cấp cho Admin
	/// Yêu cầu quyền Pages_Suppliers để truy cập
	/// </summary>
	[AbpMvcAuthorize(PermissionNames.Pages_Suppliers)]
	public class SuppliersController : MyProjectControllerBase
	{
		private readonly ISupplierAppService _supplierAppService;

		/// <summary>
		/// Constructor - Inject SupplierAppService
		/// </summary>
		/// <param name="supplierAppService">Service xử lý business logic cho nhà cung cấp</param>
		public SuppliersController(ISupplierAppService supplierAppService)
		{
			_supplierAppService = supplierAppService;
		}

		/// <summary>
		/// Trang danh sách nhà cung cấp
		/// </summary>
		/// <param name="input">Thông tin phân trang và tìm kiếm</param>
		/// <returns>View hiển thị danh sách nhà cung cấp</returns>
		public async Task<ActionResult> Index(GetAllSuppliersInput input)
		{
			// Khởi tạo input mặc định nếu null
			if (input == null)
			{
				input = new GetAllSuppliersInput();
			}

			// Lấy danh sách nhà cung cấp từ service
			var output = await _supplierAppService.GetAll(input);
			var model = new SupplierViewModel(output.Items);

			return View(model);
		}

		/// <summary>
		/// Modal sửa nhà cung cấp (Partial View)
		/// Load thông tin nhà cung cấp và hiển thị form chỉnh sửa
		/// </summary>
		/// <param name="supplierId">ID của nhà cung cấp cần sửa</param>
		/// <returns>Partial View chứa form chỉnh sửa</returns>
		public async Task<ActionResult> EditModalSupplier(int supplierId)
		{
			// Lấy thông tin nhà cung cấp theo ID
			var supplier = await _supplierAppService.GetById(supplierId);

			var model = new EditSupplierViewModel
			{
				Supplier = supplier
			};

			return PartialView("_EditModal", model);
		}

		/// <summary>
		/// Trang chi tiết nhà cung cấp
		/// </summary>
		/// <param name="supplierId">ID của nhà cung cấp cần xem chi tiết</param>
		/// <returns>View hiển thị thông tin chi tiết nhà cung cấp</returns>
		public async Task<ActionResult> Detail(int supplierId)
		{
			// Lấy thông tin nhà cung cấp theo ID
			var supplier = await _supplierAppService.GetById(supplierId);

			var model = new EditSupplierViewModel
			{
				Supplier = supplier
			};

			return View(model);
		}
	}
}

