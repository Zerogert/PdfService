using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PdfService.Interfaces;
using PuppeteerSharp;
using System.IO;
using System.Threading.Tasks;

namespace PdfService.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class DocumentsController: ControllerBase {

		private readonly ILogger<DocumentsController> _logger;
		private readonly IBrowserResolver _browser;

		public DocumentsController(ILogger<DocumentsController> logger, IBrowserResolver browser) {
			_logger = logger;
			_browser = browser;
		}

		[HttpGet]
		public async Task<IActionResult> Post(string url, bool landscape) {
			return File(await GeneratePdfAsync(url, landscape), "application/pdf");
		}

		public async Task<Stream> GeneratePdfAsync(string url, bool landscape) {
			var browser = await _browser.Get();
			using (var page = await browser.NewPageAsync()) {
				await page.GoToAsync(url);
				return await page.PdfStreamAsync(new PdfOptions() { Format = PuppeteerSharp.Media.PaperFormat.A4, Landscape = landscape, OmitBackground = true, PrintBackground = true });
			}
		}
	}
}
