using System;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace LibraryAPI.Data
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public DbSet<Location>? Locations { get; set; }
        public DbSet<Language>? Languages { get; set; }
        public DbSet<Category>? Categories { get; set; }
        public DbSet<SubCategory>? SubCategories { get; set; }
        public DbSet<Publisher>? Publishers { get; set; }
        public DbSet<Author>? Authors { get; set; }
        public DbSet<Book>? Books { get; set; }
        public DbSet<AuthorBook>? AuthorBook { get; set; }
        public DbSet<Member>? Members { get; set; }
        public DbSet<Employee>? Employees { get; set; }
        public DbSet<Loan>? Loans { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }  // Yeni DbSet
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AuthorBook>().HasKey(a => new { a.AuthorsId, a.BooksId });


            //modelBuilder.Entity<AuthorBook>()
            //.HasOne(ab => ab.Author)
            //.WithMany(a => a.AuthorBooks)
            //.HasForeignKey(ab => ab.AuthorsId);

            //modelBuilder.Entity<AuthorBook>()
            //.HasOne(ab => ab.Book)
            //.WithMany(b => b.AuthorBooks)
            //.HasForeignKey(ab => ab.BooksId);
        }

        
    }
}

