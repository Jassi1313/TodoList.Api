using AutoMapper;
using TodoList.Api.Models;

namespace TodoList.Api.Profiles
{
    public class TodoItemsProfile : Profile
    {
        public TodoItemsProfile()
        {
            CreateMap<Business.Models.TodoItem, TodoItemDto>();
            CreateMap<CreateTodoItemRequestDto, Business.Models.TodoItem>();
            CreateMap<UpdateTodoItemRequestDto, Business.Models.TodoItem>();
        }
    }
}
