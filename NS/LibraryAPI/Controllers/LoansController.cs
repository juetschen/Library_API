using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Models;
using LibraryAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoansController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook(string memberId, int bookId, string employeeId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null || book.Stock <= 0)
            {
                return BadRequest("Book not available.");
            }

            var existingLoan = await _context.Loans
                .FirstOrDefaultAsync(l => l.MemberId == memberId && l.BookId == bookId && l.ReturnDate == null);
            if (existingLoan != null)
            {
                return BadRequest("Member has already borrowed this book.");
            }

            var availableCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.BookId == bookId && bc.Returned);
            if (availableCopy == null)
            {
                return BadRequest("No available copies of the book.");
            }

            var loan = new Loan
            {
                MemberId = memberId,
                BookId = bookId,
                EmployeeId = employeeId,
                LoanDate = DateTime.UtcNow,
                CopyId = availableCopy.CopyId
            };

            book.Stock--;
            availableCopy.Returned = false;


            _context.Loans.Add(loan);
            _context.Books.Update(book);
            _context.BookCopies.Update(availableCopy);

            await _context.SaveChangesAsync();

            return Ok("Book borrowed successfully.");
        }

        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook(string memberId, int bookId)
        {
            var loan = await _context.Loans
                .FirstOrDefaultAsync(l => l.MemberId == memberId && l.BookId == bookId && l.ReturnDate == null);
            if (loan == null)
            {
                return BadRequest("No active loan found for this member and book.");
            }

            loan.ReturnDate = DateTime.UtcNow;

            var book = await _context.Books.FindAsync(bookId);
            if (book != null)
            {
                book.Stock++;
                _context.Books.Update(book);
            }

            var bookCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.BookId == bookId && bc.CopyId == loan.CopyId && !bc.Returned);
            if (bookCopy != null)
            {
                bookCopy.Returned = true;
                bookCopy.BarrowedTo=null;
                _context.BookCopies.Update(bookCopy);
            }

            await _context.SaveChangesAsync();

            return Ok("Book returned successfully.");
        }

        [HttpGet("stock/{bookId}")]
        public async Task<IActionResult> GetBookStock(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound("Book not found.");
            }

            var stock = await _context.BookCopies
                .CountAsync(bc => bc.BookId == bookId && bc.Returned);

            return Ok(stock);
        }
    }
}
