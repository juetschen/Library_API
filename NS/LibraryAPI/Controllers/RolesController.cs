using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize("Admin")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public RolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost("CreateRoles")]
        public void CreateRoles()
        {
            IdentityRole identityRole = new IdentityRole("Member");
            _roleManager.CreateAsync(identityRole).Wait();

            identityRole = new IdentityRole("Worker");
            _roleManager.CreateAsync(identityRole).Wait();
        }

        [HttpPost("GiveRole")]
        
        public async Task<IActionResult> GiveRole(string id, string role)
        {
            // Kullan�c�y� ID ile bul
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Kullan�c� bulunamad�");
            }

            // Rol var m� kontrol et
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                return BadRequest("Rol bulunamad�");
            }

            // Kullan�c�ya rol ata
            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                return Ok("Rol ba�ar�yla verildi");
            }

            // Hata durumunda mesaj d�nd�r
            return BadRequest("Rol verilemedi");
        }
    }
}
