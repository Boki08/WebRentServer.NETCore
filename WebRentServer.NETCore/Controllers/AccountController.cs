using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Owin.Security;
using WebRentServer.NETCore.Authentication;
using WebRentServer.NETCore.JwtHelpers;
using WebRentServer.NETCore.Models;
using WebRentServer.NETCore.Models.Entities;
using WebRentServer.NETCore.Persistance.UnitOfWork;


namespace WebRentServer.NETCore.Controllers
{
    [ApiController]
    [Route("api/Account")]
    public class AccountController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly JwtSettings jwtSettings;

        private UserManager<RAIdentityUser> userManager { get; set; }
        private RoleManager<IdentityRole> roleManager { get; set; }
        public AccountController(IUnitOfWork unitOfWork, UserManager<RAIdentityUser> userManager, RoleManager<IdentityRole> roleManager,
           JwtSettings jwtSettings)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.roleManager = roleManager; 
            this.jwtSettings = jwtSettings;
        }

        // POST api/Account/Logout
        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return Ok();
        }

        [HttpPost]
        [Route("logIn")]
        public async Task<IActionResult> GetTokenAsync([FromForm]LoginModel loginModel)
        {
            try
            {
                var token = new UserTokens();
                var user = await userManager.FindByNameAsync(loginModel.Username);
                if (user != null && await userManager.CheckPasswordAsync(user, loginModel.Password))
                {
                    var userRoles = await userManager.GetRolesAsync(user);

                    token = JwtHelpers.JwtHelpers.GenTokenKey(new UserTokens()
                    {
                        EmailId = user.Email,
                        GuidId = Guid.NewGuid(),
                        UserName = user.UserName,
                        Id = Guid.Parse(user.Id),
                        Claims = new List<Claim>() { new Claim(ClaimTypes.Role, userRoles.First()) }
                    }, jwtSettings);
                }
                else
                {
                    return BadRequest($"wrong password");
                }
                return Ok(token);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // POST api/Account/ChangePassword
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RAIdentityUser user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null)
            {
                return null;
            }
            IdentityResult result = await userManager.ChangePasswordAsync(user, model.OldPassword,
                model.NewPassword);
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [HttpPost]
        [Route("SetPassword")]
        public async Task<IActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            RAIdentityUser user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null)
            {
                return null;
            }
            IdentityResult result = await userManager.AddPasswordAsync(user, model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/Register
        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await userManager.FindByEmailAsync(model.Email) != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            AppUser appUser = new AppUser() { FullName = model.FullName, Email = model.Email, BirthDate = model.BirthDate};
            var user = new RAIdentityUser() { UserName = model.Email, Email = model.Email, AppUser = appUser, PasswordHash = RAIdentityUser.HashPassword(model.Password) };

            if (await roleManager.RoleExistsAsync(UserRoles.Manager))
                await userManager.AddToRoleAsync(user, UserRoles.User);

            IdentityResult result = await userManager.CreateAsync(user, model.Password);
           
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
      
            //NotificationsHub.NotifyAdmin("New User was registered");
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && userManager != null)
            {
                userManager.Dispose();
                userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirst(ClaimTypes.Name).Value
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpUtility.UrlEncode(data);
            }
        }
        #endregion
    }
}