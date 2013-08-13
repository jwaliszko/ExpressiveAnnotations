using System.ComponentModel.DataAnnotations;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Contact
    {
        [Required]
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}