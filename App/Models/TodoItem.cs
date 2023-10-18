using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Models;

public class TodoItem
{
    public int Id { get; set; }
    public User? User { get; set; }

    [StringLength(500, MinimumLength = 4, ErrorMessage = ""), Required]
    public string Text { get; set; } = default!;
    [DefaultValue(0)]
    public int Priority { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public DateTime? ExecutionTime { get; set; } = DateTime.Now;
}