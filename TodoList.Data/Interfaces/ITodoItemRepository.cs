using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Data.Entities;

namespace TodoList.Data.Interfaces
{
    public interface ITodoItemRepository
    {
        Task<IEnumerable<TodoItem>> GetTodoItemsAsync(CancellationToken cancellationToken);

        Task<TodoItem> GetTodoItemAsync(Guid id);
        Task<Guid> CreateTodoItemAsync(TodoItem todoItem);
        Task UpdateTodoItemAsync(TodoItem todoItem);

        bool CheckIfTodoItemExists(string description);
    }
}
