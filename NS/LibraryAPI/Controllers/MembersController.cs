using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public MembersController(ApplicationContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // GET: api/Members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            if (_context.Members == null)
            {
                return NotFound();
            }
            return await _context.Members.ToListAsync();
        }

        // GET: api/Members/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            if (_context.Members == null)
            {
                return NotFound();
            }
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // PUT: api/Members/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(string id, Member member, string? currentPassword = null)
        {
            ApplicationUser applicationUser = _userManager.FindByIdAsync(id).Result;

            if (id != member.Id)
            {
                return BadRequest();
            }

            applicationUser.Address = member.ApplicationUser!.Address;
            applicationUser.BirthDate = member.ApplicationUser!.BirthDate;
            applicationUser.Email = member.ApplicationUser!.Email;

            _userManager.UpdateAsync(applicationUser).Wait();
            if (currentPassword != null)
            {
                _userManager.ChangePasswordAsync(applicationUser, currentPassword, applicationUser.Password).Wait();
            }
            member.ApplicationUser = null;

            _context.Entry(member).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
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

        // POST: api/Members
        [HttpPost]
        public async Task<ActionResult<Member>> PostMember(Member member, string? category = null)
        {
            Claim claim;

            if (_context.Members == null)
            {
                return Problem("Entity set 'ApplicationContext.Members' is null.");
            }
            _userManager.CreateAsync(member.ApplicationUser!, member.ApplicationUser!.Password).Wait();
            _userManager.AddToRoleAsync(member.ApplicationUser, "Member").Wait();
            if (category != null)
            {
                claim = new Claim("Category", category);
                _userManager.AddClaimAsync(member.ApplicationUser, claim).Wait();
            }
            member.Id = member.ApplicationUser!.Id;
            member.ApplicationUser = null;
            _context.Members.Add(member);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MemberExists(member.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetMember", new { id = member.Id }, member);
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }

            ApplicationUser applicationUser = await _userManager.FindByIdAsync(id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            applicationUser.IsDeleted = true;
            _context.Entry(applicationUser).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("Login")]
        public ActionResult Login(string userName, string password)
        {
            ApplicationUser applicationUser = _userManager.FindByNameAsync(userName).Result;
            Microsoft.AspNetCore.Identity.SignInResult signInResult;

            if (applicationUser != null)
            {

                if (applicationUser.IsDeleted == false)
                {



                    if (_userManager.CheckPasswordAsync(applicationUser, password).Result == true)
                    {
                        var userRoles = _userManager.GetRolesAsync(applicationUser).Result;
                        var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, applicationUser.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                        foreach (var userRole in userRoles)
                        {
                            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                        }

                        var userClaims = _userManager.GetClaimsAsync(applicationUser).Result;

                        authClaims.AddRange(userClaims);

                        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

                        var token = new JwtSecurityToken(
                            issuer: _configuration["JWT:ValidIssuer"],
                            audience: _configuration["JWT:ValidAudience"],
                            expires: DateTime.Now.AddHours(3),
                            claims: authClaims,
                            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token)
                        });
                    }

                    signInResult = _signInManager.PasswordSignInAsync(applicationUser, password, false, false).Result;
                    if (signInResult.Succeeded == true)
                    {
                        return Ok();
                    }
                }
            }
            return Unauthorized("Invalid credentials.");
        }

        [Authorize]
        [HttpGet("Logout")]
        public ActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpPost("ForgetPassword")]
        public ActionResult<string> ForgetPassword(string userName)
        {
            ApplicationUser applicationUser = _userManager.FindByNameAsync(userName).Result;

            string token = _userManager.GeneratePasswordResetTokenAsync(applicationUser).Result;
            System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage("abc@abc", applicationUser.Email, "Password reset", token);
            System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient("http://smtp.domain.com");
            smtpClient.Send(mailMessage);
            return token;
        }

        [HttpPost("ResetPassword")]
        public ActionResult ResetPassword(string userName, string token, string newPassword)
        {
            ApplicationUser applicationUser = _userManager.FindByNameAsync(userName).Result;

            _userManager.ResetPasswordAsync(applicationUser, token, newPassword).Wait();

            return Ok();
        }

        private bool MemberExists(string id)
        {
            return (_context.Members?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
