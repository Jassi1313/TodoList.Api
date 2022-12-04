using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Api.Constants;
using TodoList.Api.Controllers;
using TodoList.Api.Models;
using TodoList.Api.Profiles;
using TodoList.Business.Interfaces;
using Xunit;
using TodoItem = TodoList.Business.Models.TodoItem;

namespace TodoList.Api.UnitTests
{
    /*
     * The tests below are just for the Controller layer as there is no business logic in the service layer
     * Even though we are using In Memory Database and there is no need to Mock responses; I personally feel that results in Integration test rather than a Unit test
     * So in my experience, I will club these unit tests along with Specflow for Integration Tests
     * 
     * 
     * Also, followed, Failed -> Fix -> Refactor approach to write these tests
    */
    public class TodoListApiShould
    {
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<TodoItemsController>> _logger;

        public TodoListApiShould()
        {
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new TodoItemsProfile());
            });

            _mapper = mockMapper.CreateMapper();
            _logger = new Mock<ILogger<TodoItemsController>>();
        }

        [Fact]
        [Trait("Category", "Success")]
        public async Task BeAbleToReturnListOfTodoItems()
        {

            // Arrange
            var mockTodoItemService = new Mock<ITodoItemService>();

            mockTodoItemService.Setup(svc => svc.GetTodoItemsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<TodoItem>()
                            {
                                new TodoItem() { Description = "Some description", Id = Guid.Parse("a3181895-3207-44d8-825c-b5572303a2a0"), IsCompleted = true },
                                new TodoItem() { Description = "Some description 2", Id = Guid.Parse("31316291-4da0-424a-9ebe-2322e56e59be"), IsCompleted = true }
                            });


            var todoItemsController = new TodoItemsController(mockTodoItemService.Object, _mapper, _logger.Object);

            // Act
            var response = await todoItemsController.GetTodoItems(It.IsAny<CancellationToken>()) as OkObjectResult;

            // Assert
            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(200);

            response?.Value.Should().BeEquivalentTo(new List<TodoItemDto>()
            {
                new TodoItemDto() { Description = "Some description", Id = Guid.Parse("a3181895-3207-44d8-825c-b5572303a2a0"), IsCompleted = true },
                new TodoItemDto() { Description = "Some description 2", Id = Guid.Parse("31316291-4da0-424a-9ebe-2322e56e59be"), IsCompleted = true }
            });
        }

        [Fact]
        [Trait("Category", "Success")]
        public async Task BeAbleToReturnASingleTodoItem()
        {
            // Arrange
            var mockTodoItemService = new Mock<ITodoItemService>();

            mockTodoItemService.Setup(svc => svc.GetTodoItemAsync(It.IsAny<Guid>())).ReturnsAsync(new TodoItem()
            {
                Description = "Some description",
                Id = Guid.Parse("a3181895-3207-44d8-825c-b5572303a2a0"),
                IsCompleted = true
            });

            var todoItemsController = new TodoItemsController(mockTodoItemService.Object, _mapper, _logger.Object);

            // Act
            var response = await todoItemsController.GetTodoItem(It.IsAny<Guid>()) as OkObjectResult;

            // Assert
            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(new TodoItemDto()
            {
                Description = "Some description",
                Id = Guid.Parse("a3181895-3207-44d8-825c-b5572303a2a0"),
                IsCompleted = true
            });
        }

        [Fact]
        [Trait("Category", "Error")]
        public async Task ReturnNotFoundWhenTodoItemDoesNotExist()
        {
            // Arrange
            var mockTodoItemService = new Mock<ITodoItemService>();

            var todoId = Guid.NewGuid();

            mockTodoItemService.Setup(svc => svc.GetTodoItemAsync(todoId)).ReturnsAsync((TodoItem)null);

            var todoItemsController = new TodoItemsController(mockTodoItemService.Object, _mapper, _logger.Object);

            // Act
            var response = await todoItemsController.GetTodoItem(todoId) as NotFoundObjectResult;

            // Assert
            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be(ApiResponseMessage.TodoItemWithIdNotFound(todoId));
        }

        [Fact]
        [Trait("Category", "Success")]
        public async Task BeAbleToReturnIdOfNewlyCreatedTodoItem()
        {
            // Arrange
            var mockTodoItemService = new Mock<ITodoItemService>();
            mockTodoItemService.Setup(svc => svc.CreateTodoItemAsync(It.IsAny<TodoItem>()))
                        .ReturnsAsync(Guid.Parse("c3430cd7-ba6d-44b1-a8c1-0810697f87ac"));

            var todoItemsController = new TodoItemsController(mockTodoItemService.Object, _mapper, _logger.Object);

            // Act
            var response = await todoItemsController.CreateTodoItem(new CreateTodoItemRequestDto()
            {
                Description = "Description for todo item"
            }) as CreatedAtActionResult;

            // Assert
            response?.Should().NotBeNull();
            response?.StatusCode.Should().Be(201);
            response?.Value.Should().Be(Guid.Parse("c3430cd7-ba6d-44b1-a8c1-0810697f87ac"));
        }

        [Fact]
        [Trait("Category", "Error")]
        public async Task NotAllowMultipleTodoItemsWithSameDescription()
        {
            // Arrange
            var mockTodoItemService = new Mock<ITodoItemService>();
            mockTodoItemService.Setup(svc => svc.CheckIfTodoItemExists(It.IsAny<string>()))
                                .Returns(true);

            var todoItemsController = new TodoItemsController(mockTodoItemService.Object, _mapper, _logger.Object);

            // Act
            var response = await todoItemsController.CreateTodoItem(new CreateTodoItemRequestDto()
            {
                Description = "Some random description"
            }) as ConflictObjectResult;

            // Assert
            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(409);
            response?.Value.Should().Be(ApiResponseMessage.TodoItemWithDescriptionExists);
        }

        [Fact]
        [Trait("Category", "Success")]
        public async Task BeAbleToUpdateAnItem()
        {
            // Arrange
            var mockTodoItemDto = new UpdateTodoItemRequestDto
            {
                Id = Guid.Parse("c3430cd7-ba6d-44b1-a8c1-0810697f87ac"),
                Description = "This is a test to do item for update",
                IsCompleted = false
            };

            var mockTodoItemService = new Mock<ITodoItemService>();
            mockTodoItemService.Setup(svc => svc.UpdateTodoItemAsync(It.IsAny<TodoItem>()));

            var todoItemsController = new TodoItemsController(mockTodoItemService.Object, _mapper, _logger.Object);

            // Act
            var result = await todoItemsController.UpdateTodoItem(Guid.Parse("c3430cd7-ba6d-44b1-a8c1-0810697f87ac"), mockTodoItemDto) as NoContentResult;

            // Assert
            result?.StatusCode.Should().Be(201);
        }

        [Fact]
        [Trait("Category", "Error")]
        public async Task NotBeAbleToUpdateAnIncorrectTodoItem()
        {
            // Arrange
            var mockTodoItemDto = new UpdateTodoItemRequestDto
            {
                Id = Guid.Parse("c3430cd7-ba6d-44b1-a8c1-0810697f87ac"),
                Description = "This is a test to not allow updating an item with no matching id in route",
                IsCompleted = false
            };

            var mockTodoItemService = new Mock<ITodoItemService>();
            var todoItemsController = new TodoItemsController(mockTodoItemService.Object, _mapper, _logger.Object);

            // Act
            var result = await todoItemsController.UpdateTodoItem(It.IsAny<Guid>(), mockTodoItemDto) as BadRequestObjectResult;

            // Assert
            result.Should().NotBe(null);
            result?.StatusCode.Should().Be(400);
            result?.Value.Should().Be(ApiResponseMessage.UpdatingWrongTodoItem);
        }

        [Fact]
        [Trait("Category", "Error")]
        public async Task ReturnNotFoundWhenItemForUpdateNotFound()
        {
            // Arrange
            var mockTestTodoItemId = Guid.Parse("c3430cd7-ba6d-44b1-a8c1-0810697f87ac");

            var mockTodoItemDto = new UpdateTodoItemRequestDto
            {
                Id = mockTestTodoItemId,
                Description = "This is a test to return Not found when To do item does not exist",
                IsCompleted = false
            };

            var mockTodoItemService = new Mock<ITodoItemService>();
            mockTodoItemService.Setup(svc => svc.GetTodoItemAsync(mockTestTodoItemId)).ReturnsAsync((TodoItem) null);

            var todoItemsController = new TodoItemsController(mockTodoItemService.Object, _mapper, _logger.Object);

            // Act
            var result = await todoItemsController.UpdateTodoItem(mockTestTodoItemId, mockTodoItemDto) as NotFoundObjectResult;

            // Assert
            result?.StatusCode.Should().Be(404);
            result?.Value.Should().Be(ApiResponseMessage.TodoItemWithIdNotFound(mockTestTodoItemId));
        }
    }
}
