using AutoMapper;
using TodoList.Business.Models;

namespace TodoList.Business.Profiles
{
    public class TodoItemsProfile : Profile
    {
        public TodoItemsProfile()
        {
            CreateMap<Data.Entities.TodoItem, TodoItem>().ReverseMap();
        }
    }
}
