using System;
using System.ComponentModel.DataAnnotations;

namespace BulkyBookWeb.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public required string Name { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public DateTime CreatedDateTime { get; set; }

        // Constructor to set default values
        public Category()
        {
            CreatedDateTime = DateTime.Now;
        }
    }
}
