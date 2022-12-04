using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Business.Models;

namespace TodoList.Business.Interfaces
{
    public interface ITodoItemService
    {
        Task<IEnumerable<TodoItem>> GetTodoItemsAsync(CancellationToken cancellationToken);
        Task<TodoItem> GetTodoItemAsync(Guid id);
        Task<Guid> CreateTodoItemAsync(TodoItem todoItem);
        Task UpdateTodoItemAsync(TodoItem todoItem);
        bool CheckIfTodoItemExists(string description);
    }
}