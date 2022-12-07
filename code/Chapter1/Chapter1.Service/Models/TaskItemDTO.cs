using Microsoft.AspNetCore.Datasync.EFCore;
using System.ComponentModel.DataAnnotations;

namespace Chapter1.Service.Models
{
    public class TaskItemDTO : EntityTableData
    {
        [Required, MinLength(1)]
        public string Title { get; set; } = string.Empty;

        public bool IsComplete { get; set; } = false;
    }
}
