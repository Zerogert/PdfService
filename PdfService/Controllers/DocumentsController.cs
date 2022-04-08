using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System.IO;
using System.Threading.Tasks;

namespace PdfService.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class DocumentsController: ControllerBase {

		private readonly ILogger<DocumentsController> _logger;

		public DocumentsController(ILogger<DocumentsController> logger) {
			_logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> Post(string url, bool landscape) {
			return File(await GeneratePdfAsync(url, landscape), "application/pdf");
		}

		public static async Task<Stream> GeneratePdfAsync(string url, bool landscape) {
			var options = new LaunchOptions { Headless = true, ExecutablePath = @"/usr/bin/chromium-browser", Args = new string[] { "--no-sandbox" } };
			using (var browser = await Puppeteer.LaunchAsync(options))
			using (var page = await browser.NewPageAsync()) {
				await page.GoToAsync(url);
				return await page.PdfStreamAsync(new PdfOptions() { Format = PuppeteerSharp.Media.PaperFormat.A4, Landscape = landscape, OmitBackground = true, PrintBackground = true });
			}
		}
	}
}
