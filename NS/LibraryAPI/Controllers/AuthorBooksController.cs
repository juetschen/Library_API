using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorBooksController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public AuthorBooksController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/AuthorBooks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorBook>>> GetAuthorBook()
        {
          if (_context.AuthorBook == null)
          {
              return NotFound();
          }
            return await _context.AuthorBook.ToListAsync();
        }

        // GET: api/AuthorBooks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorBook>> GetAuthorBook(long id)
        {
          if (_context.AuthorBook == null)
          {
              return NotFound();
          }
            var authorBook = await _context.AuthorBook.FindAsync(id);

            if (authorBook == null)
            {
                return NotFound();
            }

            return authorBook;
        }

        // PUT: api/AuthorBooks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAuthorBook(long id, AuthorBook authorBook)
        {
            if (id != authorBook.AuthorsId)
            {
                return BadRequest();
            }

            _context.Entry(authorBook).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorBookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/AuthorBooks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AuthorBook>> PostAuthorBook(AuthorBook authorBook)
        {
          if (_context.AuthorBook == null)
          {
              return Problem("Entity set 'ApplicationContext.AuthorBook'  is null.");
          }
            _context.AuthorBook.Add(authorBook);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (AuthorBookExists(authorBook.AuthorsId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetAuthorBook", new { id = authorBook.AuthorsId }, authorBook);
        }

        // DELETE: api/AuthorBooks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthorBook(long id)
        {
            if (_context.AuthorBook == null)
            {
                return NotFound();
            }
            var authorBook = await _context.AuthorBook.FindAsync(id);
            if (authorBook == null)
            {
                return NotFound();
            }

            _context.AuthorBook.Remove(authorBook);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AuthorBookExists(long id)
        {
            return (_context.AuthorBook?.Any(e => e.AuthorsId == id)).GetValueOrDefault();
        }
    }
}
