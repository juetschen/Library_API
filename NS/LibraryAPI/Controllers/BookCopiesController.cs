using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Models;
using LibraryAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookCopiesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public BookCopiesController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateCopyId(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound("Book not found.");
            }

            var bookCopy = new BookCopy
            {
                BookId = bookId,
                BarrowedTo = null,
                Returned = true
            };

            _context.BookCopies.Add(bookCopy);
            await _context.SaveChangesAsync();

            await UpdateBookStock(bookId);

            return Ok(new { copyId = bookCopy.CopyId });
        }

        [HttpGet("stock/{bookId}")]
        public async Task<IActionResult> GetBookStock(int bookId)
        {
            var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
            if (!bookExists)
            {
                return NotFound("Book not found.");
            }

            var stock = await _context.BookCopies
                .CountAsync(bc => bc.BookId == bookId && bc.Returned);

            return Ok(stock);
        }

        private async Task UpdateBookStock(int bookId)
        {
            var stock = await _context.BookCopies
                .CountAsync(bc => bc.BookId == bookId && bc.Returned);

            var book = await _context.Books.FindAsync(bookId);
            if (book != null)
            {
                book.Stock = stock;
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
        }
    }
}
