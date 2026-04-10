using System.ComponentModel.DataAnnotations;

namespace EventFlow.MVC.Models
{
    public class EventCreateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(100, ErrorMessage = "Maximum 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Number of tickets is required")]
        [Range(1, 100000, ErrorMessage = "Must be greater than 0")]
        public int TotalTickets { get; set; }

        [Required]
        public DateTime StartTime { get; set; } = DateTime.Now.AddDays(1);

        [Required]
        public DateTime EndTime { get; set; } = DateTime.Now.AddDays(1).AddHours(2);
    }
}