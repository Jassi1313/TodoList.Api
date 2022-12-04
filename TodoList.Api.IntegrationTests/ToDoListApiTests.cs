using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TodoList.Api.Constants;
using TodoList.Api.IntegrationTests.Helper;
using TodoList.Api.Models;
using TodoList.Data.Context;
using Xunit;

namespace TodoList.Api.IntegrationTests
{
	public class ToDoListApiTests : IClassFixture<ApiClientFixture>
	{		
		private ApiClientFixture _fixture;

		private HttpClient _client;

		public ToDoListApiTests(ApiClientFixture fixture)
		{
			_fixture = fixture;
			_client = fixture.httpClient;

			var builder = new DbContextOptionsBuilder<TodoContext>();
			builder.UseInMemoryDatabase("TodoDb");

			var dbOptions = builder.Options;
			using var context = new TodoContext(dbOptions);

			if (context.TodoItems.Any())
			{

				foreach (var item in context.TodoItems)
				{
					context.Remove(item);
					context.SaveChanges();
				}
			}

		}


		[Fact]
		[Trait("Category", "Success")]
		public async Task BeAbleToReturnListOfTodoItems()
		{

			var body = new { description = "Some description" };
			var body2 = new { description = "Some description 2" };

			var guid1 = await createToDoItemAsync(_client, body);
			var guid2 = await createToDoItemAsync(_client, body2);


			var req = HttpClientHelper.CreateApiHttpRequestMessage(HttpMethod.Get, "/api/todoItems");
			var response = await _client.SendAsync(req);
			response.Should().NotBeNull();
			response?.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

			var content = await response.Content.ReadAsStringAsync();
			var items = (JsonConvert.DeserializeObject<List<TodoItemDto>>(content));
			Assert.NotEmpty(items);

			// Assert


			items.Should().BeEquivalentTo(new List<TodoItemDto>()
			{
				new TodoItemDto() { Description = "Some description", Id = guid1, IsCompleted = false },
				new TodoItemDto() { Description = "Some description 2", Id = guid2, IsCompleted = false }
			});

		}


		[Fact]
		[Trait("Category", "Success")]
		public async Task BeAbleToReturnASingleTodoItem()
		{
			// Arrange
			

			var body = new { description = "Some description" };

			var guid1 = await createToDoItemAsync(_client, body);


			var req = HttpClientHelper.CreateApiHttpRequestMessage(HttpMethod.Get, $"/api/todoItems/{guid1}");
			var response = await _client.SendAsync(req);

			// Assert
			response.Should().NotBeNull();
			response?.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
			var content = await response.Content.ReadAsStringAsync();
			var item = (JsonConvert.DeserializeObject<TodoItemDto>(content));
			item.Should().BeEquivalentTo(new TodoItemDto()
			{
				Description = "Some description",
				Id = guid1,
				IsCompleted = false
			});
		}

		[Fact]
		[Trait("Category", "Error")]
		public async Task ReturnNotFoundWhenTodoItemDoesNotExist()
		{

			var todoId = Guid.NewGuid();
			

			var req = HttpClientHelper.CreateApiHttpRequestMessage(HttpMethod.Get, $"/api/todoItems/{todoId}");
			var response = await _client.SendAsync(req);

			// Assert
			response.Should().NotBeNull();
			response?.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
			var respponseMessage = await response.Content.ReadAsStringAsync();

			respponseMessage?.Should().Be(ApiResponseMessage.TodoItemWithIdNotFound(todoId));
		}



		[Fact]
		[Trait("Category", "Success")]
		public async Task BeAbleToReturnIdOfNewlyCreatedTodoItem()
		{
			// Arrange
			var body = new { description = "Some description 1" };
			
			var req = HttpClientHelper.CreateApiHttpRequestMessage(HttpMethod.Post, "/api/todoItems", body);
			var response = await _client.SendAsync(req);
			var content = await response?.Content.ReadAsStringAsync();
			var guid = JsonConvert.DeserializeObject<Guid>(content);

			// Assert
			response?.Should().NotBeNull();
			response?.StatusCode.Should().Be(HttpStatusCode.Created);
		}

		[Fact]
		[Trait("Category", "Error")]
		public async Task NotAllowMultipleTodoItemsWithSameDescription()
		{
			// Arrange
			

			var body = new { description = "Some random description" };

			await createToDoItemAsync(_client, body);
			// Act
			var req = HttpClientHelper.CreateApiHttpRequestMessage(HttpMethod.Post, "/api/todoItems", body);
			var response = await _client.SendAsync(req);

			// Assert
			response.Should().NotBeNull();
			response?.StatusCode.Should().Be(HttpStatusCode.Conflict);
			var content = await response?.Content.ReadAsStringAsync();

			content.Should().Be(ApiResponseMessage.TodoItemWithDescriptionExists);
		}

		[Fact]
		[Trait("Category", "Success")]
		public async Task BeAbleToUpdateAnItem()
		{

			var body = new
			{
				Description = "This is a test to do item for update",
			};
			

			var guid = await createToDoItemAsync(_client, body);

			var updatedBody = new
			{
				Id = guid,
				Description = "Updated Item",
			};


			var req = HttpClientHelper.CreateApiHttpRequestMessage(HttpMethod.Put, $"api/todoItems/{guid}", updatedBody);
			var response = await _client.SendAsync(req);

			// Assert
			response.Should().NotBeNull();
			response?.StatusCode.Should().Be(HttpStatusCode.NoContent);

		}

		[Fact]
		[Trait("Category", "Error")]
		public async Task NotBeAbleToUpdateAnIncorrectTodoItem()
		{
			// Arrange
			var body = new
			{
				Description = "This is a test to do item for update",
			};
			

			var guid = await createToDoItemAsync(_client, body);

			var updatedBody = new
			{
				Id = Guid.NewGuid(),
				Description = "Updated Item",
			};


			// Assert
			var req = HttpClientHelper.CreateApiHttpRequestMessage(HttpMethod.Put, $"api/todoItems/{guid}", updatedBody);
			var response = await _client.SendAsync(req);

			// Assert
			response.Should().NotBeNull();
			response?.StatusCode.Should().Be(HttpStatusCode.BadRequest);
			var content = await response?.Content.ReadAsStringAsync();

			content.Should().Be(ApiResponseMessage.UpdatingWrongTodoItem);
		}

		[Fact]
		[Trait("Category", "Error")]
		public async Task ReturnNotFoundWhenItemForUpdateNotFound()
		{
			// Arrange
			

			var guid = Guid.NewGuid();
			var updatedBody = new
			{
				Id = guid,
				Description = "Updated Item",
			};


			var req = HttpClientHelper.CreateApiHttpRequestMessage(HttpMethod.Put, $"api/todoItems/{guid}", updatedBody);
			var response = await _client.SendAsync(req);

			// Assert
			response.Should().NotBeNull();
			response?.StatusCode.Should().Be(HttpStatusCode.NotFound);
			var content = await response?.Content.ReadAsStringAsync();

			content.Should().Be(ApiResponseMessage.TodoItemWithIdNotFound(guid));
		}


		private async Task<Guid> createToDoItemAsync(HttpClient _client, object body)
		{
			var req = HttpClientHelper.CreateApiHttpRequestMessage(HttpMethod.Post, "/api/todoItems", body);
			var response = await _client.SendAsync(req);
			var content = await response.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<Guid>(content);
		}

		public void Dispose()
		{
			var builder = new DbContextOptionsBuilder<TodoContext>();
			builder.UseInMemoryDatabase("TodoDb");

			var dbOptions = builder.Options;
			using var context = new TodoContext(dbOptions);

			if (!context.TodoItems.Any())
			{
				context.Database.ExecuteSqlRaw("Truncate Table TodoItem");
			}
		}
	}
}
