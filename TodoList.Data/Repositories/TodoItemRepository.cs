using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Data.Context;
using TodoList.Data.Entities;
using TodoList.Data.Interfaces;

namespace TodoList.Data.Repositories
{
    public class TodoItemRepository : ITodoItemRepository
    {
        private readonly TodoContext _todoContext;
        private readonly ILogger<TodoItemRepository> _logger;

        public TodoItemRepository(TodoContext todoContext, ILogger<TodoItemRepository> logger)
        {
            _todoContext = todoContext ?? throw new ArgumentNullException(nameof(todoContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<TodoItem>> GetTodoItemsAsync(CancellationToken cancellationToken)
        {
            var todoItems = new List<TodoItem>();

            // Just an example for how we will handle the exception thrown when a user cancels the request
            try
            {
                todoItems = await _todoContext.TodoItems.Where(x => !x.IsCompleted).ToListAsync(cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation($"User cancelled the get todo items request : {ex}");
            }

            return todoItems;
        }

        public async Task<TodoItem> GetTodoItemAsync(Guid id) =>
            await _todoContext.TodoItems.FirstOrDefaultAsync(item => item.Id == id);

        public async Task<Guid> CreateTodoItemAsync(TodoItem todoItem)
        {
            // Here we are throwing an exception but this can also be better managed by using a Global Error Handling middleware
            if (todoItem == null)
            {
                _logger.LogError($"A todo item create request was sent with empty payload");
                throw new ArgumentNullException(nameof(todoItem));
            }

            _todoContext.Add(todoItem);

            await _todoContext.SaveChangesAsync();

            return todoItem.Id;
        }

        public async Task UpdateTodoItemAsync(TodoItem todoItem)
        {
            _todoContext.Update(todoItem);
            await _todoContext.SaveChangesAsync();
        }

        public bool CheckIfTodoItemExists(string description)
        {
            return _todoContext.TodoItems
                                .Any(x => x.Description.ToLowerInvariant() == description.ToLowerInvariant() && !x.IsCompleted);
        }        
    }
}
