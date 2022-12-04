using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Business.Interfaces;
using TodoList.Business.Models;
using TodoList.Data.Interfaces;

namespace TodoList.Business.Services
{
    /* 
     * For this application we don't need a service/business layer as there is no logic
     * however, I did it just to showcase how I will structure the API for more complex scenarios
    */
    public class TodoItemService : ITodoItemService
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IMapper _mapper;

        public TodoItemService(ITodoItemRepository todoItemRepository, IMapper mapper)
        {
            _todoItemRepository = todoItemRepository ?? throw new ArgumentNullException(nameof(todoItemRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<TodoItem>> GetTodoItemsAsync(CancellationToken cancellationToken)
        {
            var todoItemsFromRepository = await _todoItemRepository.GetTodoItemsAsync(cancellationToken);
            return _mapper.Map<IEnumerable<TodoItem>>(todoItemsFromRepository);
        }

        public async Task<TodoItem> GetTodoItemAsync(Guid id)
        {
            var todoItemFromRepository = await _todoItemRepository.GetTodoItemAsync(id);
            return _mapper.Map<TodoItem>(todoItemFromRepository);
        }

        public async Task<Guid> CreateTodoItemAsync(TodoItem todoItem)
        {
            var todoItemToCreate = _mapper.Map<Data.Entities.TodoItem>(todoItem);
            var createdTodoItem = await _todoItemRepository.CreateTodoItemAsync(todoItemToCreate);

            return createdTodoItem;
        }

        public async Task UpdateTodoItemAsync(TodoItem todoItem)
        {
            var itemToBeUpdated = await _todoItemRepository.GetTodoItemAsync(todoItem.Id);
            _mapper.Map(todoItem, itemToBeUpdated);

            await _todoItemRepository.UpdateTodoItemAsync(itemToBeUpdated);
        }

        public bool CheckIfTodoItemExists(string description) => _todoItemRepository.CheckIfTodoItemExists(description);
    }
}
