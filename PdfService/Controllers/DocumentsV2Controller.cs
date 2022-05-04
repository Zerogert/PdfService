using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PdfService.Interfaces;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PdfService.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class DocumentsV2Controller: ControllerBase {
		private readonly ILogger<DocumentsV2Controller> _logger;
		private readonly IBrowserResolver _browser;
		public DocumentsV2Controller(IBrowserResolver browser, ILogger<DocumentsV2Controller> logger) {
			_logger = logger;
			_browser = browser;
		}

		[HttpPost("byUrl")]
		public async Task<IActionResult> Get(List<string> url, bool landscape) {
			Stopwatch stopwatch = Stopwatch.StartNew();
			stopwatch.Start();
			var files = await GeneratePdfAsync(url, landscape);
			return Ok(new { Paths = files.Select(x => "/pdf/" + x), Elapsed = stopwatch.ElapsedMilliseconds });
		}

		[HttpPost("byFile")]
		public async Task<IActionResult> Post(IFormFileCollection fileStream, bool landscape) {
			Stopwatch stopwatch = Stopwatch.StartNew();
			stopwatch.Start();
			var files = await GeneratePdfAsync(fileStream.ToList(), landscape);
			return Ok(new { Paths = files.Select(x => "/pdf/" + x), Elapsed = stopwatch.ElapsedMilliseconds });
		}

		[NonAction]
		public async Task<List<string>> GeneratePdfAsync(List<IFormFile> streams, bool landscape) {
			var browser = await _browser.Get();
			var tasks = streams.Select(async x => {
				using (StreamReader stringReader = new StreamReader(x.OpenReadStream()))
				using (var page = await browser.NewPageAsync()) {
					await page.SetContentAsync(await stringReader.ReadToEndAsync());
					var fileName = $"{Guid.NewGuid()}.pdf";
					await page.PdfAsync($"{Directory.GetCurrentDirectory()}/files/{fileName}", new PdfOptions() { Format = PuppeteerSharp.Media.PaperFormat.A4, Landscape = landscape, OmitBackground = true, PrintBackground = true });
					return fileName;
				}
			});

			var files = await Task.WhenAll(tasks);
			return files.ToList();
		}

		[NonAction]
		public async Task<List<string>> GeneratePdfAsync(List<string> url, bool landscape) {
			var browser = await _browser.Get();
			var tasks = url.Select(async x => {
				using (var page = await browser.NewPageAsync()) {
					await page.GoToAsync(x);
					var fileName = $"{Guid.NewGuid()}.pdf";
					await page.PdfAsync($"{Directory.GetCurrentDirectory()}/files/{fileName}", new PdfOptions() { Format = PuppeteerSharp.Media.PaperFormat.A4, Landscape = landscape, OmitBackground = true, PrintBackground = true });
					return fileName;
				}
			});

			var files = await Task.WhenAll(tasks);
			return files.ToList();
		}
	}
}
