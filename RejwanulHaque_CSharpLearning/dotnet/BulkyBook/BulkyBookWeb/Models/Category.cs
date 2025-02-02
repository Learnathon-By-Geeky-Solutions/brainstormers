using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyBookWeb.Models
{
    public class Category
    {
        [Key]  // sets Id as primary key 
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1, 1000, ErrorMessage ="Display Order must be between 1 and 1000!")]
        public int DisplayOrder { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}
