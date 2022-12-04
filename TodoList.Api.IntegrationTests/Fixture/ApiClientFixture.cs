
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TodoList.Api.IntegrationTests
{
	public class ApiClientFixture
	{
		public readonly HttpClient httpClient;
		private TestRunConfiguration _configuration;

		public ApiClientFixture()
		{
			string text = File.ReadAllText(@"appsettings.test.json");
			_configuration = JsonSerializer.Deserialize<TestRunConfiguration>(text);
			bool.TryParse(_configuration.IsLocalRun, out bool localRun);
			if (localRun)
			{
				var factory = new WebApplicationFactory<Program>();
				httpClient = factory.CreateClient();
			}

			else
			{
				httpClient = new HttpClient();
				httpClient.BaseAddress = new Uri(_configuration.ApiUrl);
			}
			
		}
			
	}

	public class TestRunConfiguration
	{
		public string IsLocalRun { get; set; }
		public string ApiUrl { get; set; }
	}
}
