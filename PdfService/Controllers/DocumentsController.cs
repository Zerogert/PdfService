using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PdfService.Controllers {
	[ApiController]
	[Route("v2/[controller]")]
	public class DocumentsController: ControllerBase {
		private readonly LaunchOptions _browserOptions = new LaunchOptions { Headless = true, ExecutablePath = @"C:\Users\Zeroget\Downloads\chrome-win\chrome-win\chrome.exe", Args = new string[] { "--no-sandbox" } };
		//private readonly LaunchOptions _browserOptions = new LaunchOptions { Headless = true, ExecutablePath = @"/usr/bin/chromium-browser", Args = new string[] { "--no-sandbox" } };
		private static Browser s_browser = null;

		private readonly ILogger<DocumentsController> _logger;

		public DocumentsController(ILogger<DocumentsController> logger) {
			_logger = logger;
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
			s_browser ??= await Puppeteer.LaunchAsync(_browserOptions);
			var tasks = streams.Select(async x => {
				using (StreamReader stringReader = new StreamReader(x.OpenReadStream()))
				using (var page = await s_browser.NewPageAsync()) {
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
			s_browser ??= await Puppeteer.LaunchAsync(_browserOptions);
			var tasks = url.Select(async x => {
				using (var page = await s_browser.NewPageAsync()) {
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
