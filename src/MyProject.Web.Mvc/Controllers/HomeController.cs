using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using MyProject.Controllers;
using MyProject.Products;

namespace MyProject.Web.Controllers
{
	[AbpMvcAuthorize]
	public class HomeController : MyProjectControllerBase
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}
