using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Data.Context;
using TodoList.Data.Entities;
using TodoList.Data.Repositories;
using Xunit;

namespace TodoList.Api.UnitTests.Repository
{
	public class TodoItemRepositoryTests
	{
		public readonly DbContextOptions<TodoContext> dbContextOptions;
		private Mock<ILogger<TodoItemRepository>> _mockLogger;

		private Guid FIRST_TO_DO_ITEM_GUID = Guid.NewGuid();

		private Guid SECOND_TO_DO_ITEM_GUID = Guid.NewGuid();

		private Guid THREE_TO_DO_ITEM_GUID= Guid.NewGuid();
		public TodoItemRepositoryTests()
		{
			_mockLogger = new Mock<ILogger<TodoItemRepository>>();
			dbContextOptions = new DbContextOptionsBuilder<TodoContext>()
			.UseInMemoryDatabase(databaseName: nameof(TodoItemRepositoryTests))
			.Options;
			
			PopulateToDoItem();
		}


		[Fact]

		public void Test_CheckIfTodoItemExists_WhenItemCompleted()
		{

			using var context = new TodoContext(dbContextOptions);
			var repo = new TodoItemRepository(context, _mockLogger.Object);
			var exist = repo.CheckIfTodoItemExists(FIRST_TO_DO_ITEM_GUID.ToString());

			Assert.False(exist);

		}


		[Fact]

		public void Test_CheckIfTodoItemExists_WhenItemNotCompleted()
		{

			using var context = new TodoContext(dbContextOptions);
			var repo = new TodoItemRepository(context, _mockLogger.Object);
			var exist = repo.CheckIfTodoItemExists(SECOND_TO_DO_ITEM_GUID.ToString());

			Assert.True(exist);

		}

		[Fact]

		public void Test_CheckIfTodoItemExists_WhentItemNotExists()
		{

			using var context = new TodoContext(dbContextOptions);
			var repo = new TodoItemRepository(context, _mockLogger.Object);
			var exist = repo.CheckIfTodoItemExists("Not exists");

			Assert.False(exist);

		}

		[Fact]

		public async Task Test_GetTodoItemAsync_WhentItemExistsAsync()
		{

			using var context = new TodoContext(dbContextOptions);
			var repo = new TodoItemRepository(context, _mockLogger.Object);
			var todoItem = await repo.GetTodoItemAsync(FIRST_TO_DO_ITEM_GUID);

			Assert.Equal(FIRST_TO_DO_ITEM_GUID, todoItem.Id);

			Assert.True(todoItem.IsCompleted);

		}

		[Fact]

		public async Task Test_GetTodoItemAsync_WhentItemNotExistsAsync()
		{

			using var context = new TodoContext(dbContextOptions);
			var repo = new TodoItemRepository(context, _mockLogger.Object);
			var todoItem = await repo.GetTodoItemAsync(Guid.NewGuid());

			Assert.Null(todoItem);


		}

		[Fact]
	
		public async Task Test_GetTodoItemsAsync_Cancellation()
		{
			CancellationTokenSource cts = new();
			CancellationToken cancellationToken = cts.Token;
			cts.Cancel();

			using var context = new TodoContext(dbContextOptions);
			var repo = new TodoItemRepository(context, _mockLogger.Object);


			// Assert
			var items = await repo.GetTodoItemsAsync(cancellationToken);

			Assert.Empty(items);

		}


		[Fact]

		public async Task Test_GetTodoItemsAsync()
		{

			using var context = new TodoContext(dbContextOptions);
			var repo = new TodoItemRepository(context, _mockLogger.Object);
			CancellationTokenSource cts = new();
			CancellationToken cancellationToken = cts.Token;

			// Assert
			var items = await repo.GetTodoItemsAsync(cancellationToken);

			Assert.Single(items);

		}


		[Fact]

		public async Task Test_UpdateTodoItemAsyn()
		{

			using var context = new TodoContext(dbContextOptions);
			var itemToUpdate = await context.TodoItems.FirstOrDefaultAsync(item => item.Id == FIRST_TO_DO_ITEM_GUID);
			itemToUpdate.Description = "Updated";
			var repo = new TodoItemRepository(context, _mockLogger.Object);
			await repo.UpdateTodoItemAsync(itemToUpdate);

			var updatedItem = await context.TodoItems.FirstOrDefaultAsync(item => item.Id == FIRST_TO_DO_ITEM_GUID);

			Assert.NotNull(updatedItem);


			Assert.Equal("Updated", updatedItem.Description);


		}

		private void PopulateToDoItem()
		{

			using (var context = new TodoContext(dbContextOptions))
			{
				
					context.TodoItems.Add(new TodoItem { Id = FIRST_TO_DO_ITEM_GUID, Description = FIRST_TO_DO_ITEM_GUID.ToString(), IsCompleted = true });
					context.TodoItems.Add(new TodoItem { Id = SECOND_TO_DO_ITEM_GUID, Description = SECOND_TO_DO_ITEM_GUID.ToString() });
					context.TodoItems.Add(new TodoItem { Id = THREE_TO_DO_ITEM_GUID, Description = THREE_TO_DO_ITEM_GUID.ToString(), IsCompleted = true });
						
				context.SaveChanges();
			}
		}
	}
}
