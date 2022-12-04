using System;

namespace TodoList.Api.Constants
{
    public static class ApiResponseMessage
    {
        public static string TodoItemWithIdNotFound(Guid id) => $"Todo item with id {id} not found";
        public static string TodoItemWithIdNotFoundLogMessage(Guid id) => $"Client tried to search for a non-existing todo item with id {id}";
        public const string TodoItemWithDescriptionExists = "A todo item with description already exists";
        public const string UpdatingWrongTodoItem = "You are trying to update the wrong todo item";
    }
}
