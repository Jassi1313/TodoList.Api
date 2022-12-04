using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TodoList.Api.IntegrationTests.Helper
{
	public static class HttpClientHelper
	{
		private static string JSON_MEDIA_TYPE = "application/json";
		public static HttpRequestMessage CreateApiHttpRequestMessage(HttpMethod method, string endpoint, object body = null, Dictionary<string, object> queryParams = null)
		{

			string url = endpoint;
			if (queryParams != null)
			{
				var query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
				url = $"{url}?{query}";
			}

			Console.WriteLine($"Calling api url {url}");
			var request = new HttpRequestMessage(method, url);
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

			//request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			if (body != null)
			{
				request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), Encoding.UTF8, JSON_MEDIA_TYPE);
			}

			return request;
		}
	}
}
