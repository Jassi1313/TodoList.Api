using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TodoList.Api.Models
{
    public class UpdateTodoItemRequestDto
    {
        [Required(ErrorMessage = "Id field can not be empty")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Description field can not be empty")]
        [MaxLength(255, ErrorMessage = "Description field can not be greater than 255 characters")]
        public string Description { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}
