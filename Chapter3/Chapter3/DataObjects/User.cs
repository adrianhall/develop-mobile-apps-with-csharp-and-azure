using System.ComponentModel.DataAnnotations;

namespace Chapter3.DataObjects
{
    public class User
    {
        [Key]
        public string Id { get; set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
    }
}