using System;
using System.ComponentModel.DataAnnotations;

namespace TodoList.Api.Models
{
    public class CreateTodoItemRequestDto
    {
        [Required(ErrorMessage = "Description field can not be empty")]
        [MaxLength(255, ErrorMessage = "Description field can not be greater than 255 characters")]
        public string Description { get; set; }
    }
}
