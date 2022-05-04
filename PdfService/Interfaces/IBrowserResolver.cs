using PuppeteerSharp;
using System;
using System.Threading.Tasks;

namespace PdfService.Interfaces {
	public interface IBrowserResolver : IAsyncDisposable {
		public Task<Browser> Get();
	}
}
