using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signManager;
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signManager = signInManager;
        }


        [HttpPost("action")]
        public async Task<ActionResult> Register([FromBody] RegistrationViewModel formdata)
        {
            List<string> Errorlist = new List<string>();
            var user = new IdentityUser
            {
                Email = formdata.Email,
                UserName = formdata.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, formdata.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                return Ok(new { username = user.UserName, email = user.Email, status = 1, message = "Registration Successfull" });
            }
            else {

                foreach (var error in result.Errors) {
                    ModelState.AddModelError("", error.Description);
                    Errorlist.Add(error.Description);
                }
            }

            return BadRequest(new JsonResult(Errorlist));

        }


    }
}
