using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using TodoList.Api.Models;
using TodoList.Business.Interfaces;
using TodoList.Business.Models;
using TodoList.Api.Constants;

namespace TodoList.Api.Controllers
{
    // I like to explicity mention the route so that if we change the Controller name our API routes used by clients doesn't need to change
    [Route("api/todoItems")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoItemService _todoItemService;
        private readonly ILogger<TodoItemsController> _logger;
        private readonly IMapper _mapper;

        public TodoItemsController(ITodoItemService todoItemService, IMapper mapper, ILogger<TodoItemsController> logger)
        {
            _todoItemService = todoItemService ?? throw new ArgumentNullException(nameof(todoItemService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // For bigger datasets, I will add pagination and filtering support in this API however, not needed for such small app
        public async Task<IActionResult> GetTodoItems(CancellationToken cancellationToken)
        {
            // Cancellation token for this app is an overkill. However, I added it so as to show that I am aware of what they are            
            var todoItems = await _todoItemService.GetTodoItemsAsync(cancellationToken);

            // Automapper in this API is an overkill too as there are not many properties. However, this is just to demonstrate my familiarity with it
            var mappedItems = _mapper.Map<IEnumerable<TodoItemDto>>(todoItems);

            return Ok(mappedItems);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTodoItem(Guid id)
        {
            var todoItem = await _todoItemService.GetTodoItemAsync(id);

            if (todoItem == null)
            {
                /*
                 * This logger can also be extended to tie into something like a New Relic logger to fire custom New Relic logs
                 * 
                 * To take this one step ahead we can create a middleware that is fired after response is generated and send logs to New Relic from there
                 * for errors, exceptions and audit logs scenarios
                */
                 
                _logger.LogError(ApiResponseMessage.TodoItemWithIdNotFoundLogMessage(id));
                return NotFound(ApiResponseMessage.TodoItemWithIdNotFound(id));
            }

            var mappedItem = _mapper.Map<TodoItemDto>(todoItem);
            return Ok(mappedItem);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateTodoItem(CreateTodoItemRequestDto todoItemCreateRequestDto)
        {
            if (_todoItemService.CheckIfTodoItemExists(todoItemCreateRequestDto.Description))
                return Conflict(ApiResponseMessage.TodoItemWithDescriptionExists);

            var todoItemToCreate = _mapper.Map<TodoItem>(todoItemCreateRequestDto);
            var todoItemId = await _todoItemService.CreateTodoItemAsync(todoItemToCreate);

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItemId }, todoItemId);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTodoItem(Guid id, UpdateTodoItemRequestDto updateTodoItemRequestDto)
        {
            if (id != updateTodoItemRequestDto.Id)
                return BadRequest(ApiResponseMessage.UpdatingWrongTodoItem);
            
            var getTodoItem = await _todoItemService.GetTodoItemAsync(id);

            if (getTodoItem == null)
                return NotFound(ApiResponseMessage.TodoItemWithIdNotFound(id));

            var mappedTodoItem = _mapper.Map<TodoItem>(updateTodoItemRequestDto);
            await _todoItemService.UpdateTodoItemAsync(mappedTodoItem);

            return NoContent();
        }
    }
}
