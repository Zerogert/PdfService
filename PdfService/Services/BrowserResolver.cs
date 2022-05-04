using PdfService.Interfaces;
using PuppeteerSharp;
using System.Threading.Tasks;

namespace PdfService.Services {
	public class BrowserResolver : IBrowserResolver {
		private readonly LaunchOptions _browserOptions = new LaunchOptions { Headless = true, ExecutablePath = @"/usr/bin/chromium-browser", Args = new string[] { "--no-sandbox" } };
		//private readonly LaunchOptions _browserOptions = new LaunchOptions { Headless = true, ExecutablePath = @"C:\Users\Zeroget\Desktop\chrome-win\chrome.exe", Args = new string[] { "--no-sandbox" } };
		private readonly Task<Browser> _browser;

		public BrowserResolver() {
			_browser = Puppeteer.LaunchAsync(_browserOptions);
		}

		public async ValueTask DisposeAsync() {
			var browser = await _browser;
			await browser.CloseAsync();
			await browser.DisposeAsync();
		}

		public Task<Browser> Get() {
			return _browser;
		}
	}
}
