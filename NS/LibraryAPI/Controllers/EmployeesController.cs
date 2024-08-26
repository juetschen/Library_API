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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public EmployeesController(ApplicationContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }
            return await _context.Employees.ToListAsync();
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(string id)
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(string id, Employee employee, string? currentPassword = null)
        {
            ApplicationUser applicationUser = _userManager.FindByIdAsync(id).Result;

            if (id != employee.Id)
            {
                return BadRequest();
            }

            applicationUser.Address = employee.ApplicationUser!.Address;
            applicationUser.BirthDate = employee.ApplicationUser!.BirthDate;
            applicationUser.Email = employee.ApplicationUser!.Email;
            //...
            _userManager.UpdateAsync(applicationUser).Wait();
            if (currentPassword != null)
            {
                _userManager.ChangePasswordAsync(applicationUser, currentPassword, applicationUser.Password).Wait();
            }
            employee.ApplicationUser = null;

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
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

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee, string? category = null)
        {
            Claim claim;

            if (_context.Employees == null)
            {
                return Problem("Entity set 'ApplicationContext.Employees'  is null.");
            }
            _userManager.CreateAsync(employee.ApplicationUser!, employee.ApplicationUser!.Password).Wait();
            _userManager.AddToRoleAsync(employee.ApplicationUser, "Worker").Wait();
            if (category != null)
            {
                claim = new Claim("Category", category);
                _userManager.AddClaimAsync(employee.ApplicationUser, claim).Wait();
            }
            //foreach(string category in categories)
            //{
            //    claim = new Claim("Category", category);
            //    _userManager.AddClaimAsync(employee.ApplicationUser, claim).Wait();
            //}
            employee.Id = employee.ApplicationUser!.Id;
            employee.ApplicationUser = null;
            _context.Employees.Add(employee);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EmployeeExists(employee.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEmployee", new { id = employee.Id }, employee);
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


                        return Ok("                                     <===>  Sayın Çalışan Sisteme Hoşgeldiniz Token Aşağıdadır.  <===>                                     " +
                            "                                                    " +
                            "                                                    " +
                            "                                                    " +
                     new
                     {
                         token = new JwtSecurityTokenHandler().WriteToken(token)
                         //expiration = token.ValidTo
                     });
                    }

                    signInResult = _signInManager.PasswordSignInAsync(applicationUser, password, false, false).Result;
                    if (signInResult.Succeeded == true)
                    {
                        return Ok();
                    }
                }
            }
            return Unauthorized("         <===>  Sayın Çalışan Bilgileriniz Hatalı. Lütfen Kontrol Ediniz!  <===>         ");
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
            System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage("abc@abc", applicationUser.Email, "Şifre sıfırlama", token);
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

        private bool EmployeeExists(string id)
        {
            return (_context.Employees?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
