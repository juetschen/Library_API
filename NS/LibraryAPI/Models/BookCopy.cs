using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Models
{
    public class BookCopy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }

        [Required]
        public string CopyId { get; set; } = Guid.NewGuid().ToString();

        public string? BarrowedTo { get; set; }

        public bool Returned { get; set; } = false;
    }
}
