using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoList.Data.Entities
{
    [Table("TodoItems")]
    public class TodoItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Description { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}
