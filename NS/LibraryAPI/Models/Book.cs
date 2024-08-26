using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
    public class Loan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MemberId { get; set; } = "";

        [ForeignKey(nameof(MemberId))]
        public Member? Member { get; set; }

        [Required]
        public int BookId { get; set; }

        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }

        [Required]
        public string EmployeeId { get; set; } = "";

        [ForeignKey(nameof(EmployeeId))]
        public Employee? Employee { get; set; }

        [Required]
        public DateTime LoanDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        [Required]
        public string CopyId { get; set; } = ""; // Added CopyId
    }

    public class Book
    {
        public int Id { get; set; }

        [StringLength(13, MinimumLength = 10)]
        [Column(TypeName = "varchar(13)")]
        public string? ISBN { get; set; }

        [Required]
        [StringLength(2000)]
        public string Title { get; set; } = "";

        [Range(1, short.MaxValue)]
        public short PageCount { get; set; }

        [Range(-4000, 2100)]
        public short PublishingYear { get; set; }

        [StringLength(5000)]
        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int PrintCount { get; set; }

        public bool Banned { get; set; }

        [JsonIgnore]
        public float Rating { get; set; }

        public int PublisherId { get; set; }

        [NotMapped]
        public List<long>? AuthorIds { get; set; }

        [StringLength(6, MinimumLength = 3)]
        [Column(TypeName = "varchar(6)")]
        public string LocationShelf { get; set; } = "";

        public List<AuthorBook>? AuthorBooks { get; set; }

        [ForeignKey(nameof(PublisherId))]
        public Publisher? Publisher { get; set; }

        public List<SubCategory>? SubCategories { get; set; }
        public List<Language>? Languages { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(LocationShelf))]
        public Location? Location { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }  // Stok alanı
    }
}
