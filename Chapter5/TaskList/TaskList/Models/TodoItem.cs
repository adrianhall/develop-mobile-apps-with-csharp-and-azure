using TaskList.Abstractions;

namespace TaskList.Models
{
    public class TodoItem : TableData
    {
        public string Text { get; set; }

        public bool Complete { get; set; }
    }
}