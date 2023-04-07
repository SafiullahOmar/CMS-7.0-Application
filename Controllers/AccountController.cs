using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Helpers;
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
        private readonly AppSettings _appSettings; 
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,IOptions< AppSettings> appSettings)
        {
            _userManager = userManager;
            _signManager = signInManager;
            _appSettings = appSettings.Value;
        }


        [HttpPost("[action]")]
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

        [HttpPost("[action]")]
        public async Task<IActionResult> login([FromBody]LoginViewModel formdate) {
            
            var user =await _userManager.FindByNameAsync(formdate.UserName);
            var roles = await _userManager.GetRolesAsync(user);
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));
            double tokenExpiryTime = Convert.ToDouble(_appSettings.ExpireTime);
            if (user != null &&await _userManager.CheckPasswordAsync(user, formdate.Password)) {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor { 
                    Subject=new System.Security.Claims.ClaimsIdentity(new Claim[] { 
                        new Claim(JwtRegisteredClaimNames.Sub,formdate.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier,user.Id),
                        new Claim(ClaimTypes.Role,roles.FirstOrDefault()),
                        new Claim("LogOn",DateTime.Now.ToString())
                    }),

                    SigningCredentials=new SigningCredentials(key,SecurityAlgorithms.HmacSha256Signature),
                    
                    Issuer=_appSettings.Site,
                    Audience=_appSettings.Audience,
                        Expires=DateTime.UtcNow.AddMinutes(tokenExpiryTime)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return Ok(new { token=tokenHandler.WriteToken(token),expiration=token.ValidTo,  userName=user.UserName,userRole=roles.FirstOrDefault()});
            }
            ModelState.AddModelError("","user Name & passwarod was not Found");
            return Unauthorized(new { loginError = "please check the login credentails" });
        }


    }
}
