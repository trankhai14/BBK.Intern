//using Microsoft.AspNetCore.Mvc;
//using System.IO;

//namespace MyProject.Web.Controllers
//{
//	public class ImageController: Controller
//	{
//		private readonly string externalPath = @"D:\Uploads"; // Đường dẫn ngoài thư mục dự án
//		private readonly string defaultImage = "/img/default.jpg"; // Ảnh mặc định trong wwwroot

//		[HttpGet("/get-image/{*imagePath}")]
//		public IActionResult GetImage(string imagePath)
//		{
//			if (string.IsNullOrEmpty(imagePath))
//			{
//				return Redirect(defaultImage); // Trả về ảnh mặc định nếu ảnh không tồn tại
//			}

//			string fullPath = Path.Combine(externalPath, imagePath);

//			if (!System.IO.File.Exists(fullPath))
//			{
//				return Redirect(defaultImage); // Nếu ảnh không tồn tại, trả về ảnh mặc định
//			}

//			var imageFileStream = System.IO.File.OpenRead(fullPath);
//			return File(imageFileStream, "image/jpeg"); // Trả về ảnh từ thư mục ngoài
//		}
//	}
//}
