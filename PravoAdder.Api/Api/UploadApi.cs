using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class UploadApi
	{
		public async Task<FileResponse> Upload(HttpAuthenticator httpAuthenticator, FileInfo file)
		{
			return await ApiHelper.SendFileAsync<FileResponse>(httpAuthenticator, "upload", HttpMethod.Post, file);
		}
	}
}
