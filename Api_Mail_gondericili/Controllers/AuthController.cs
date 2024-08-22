using Api_Mail_gondericili.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api_Mail_gondericili.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly TokenService _tokenService;
        private readonly IEmailSender _emailSender;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, TokenService tokenService, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Password != model.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var user = new AppUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, code = token }, Request.Scheme);

                await _emailSender.SendEmailAsync(model.Email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

                return Ok("Registration successful. Please check your email to confirm your account.");
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return BadRequest("User ID and code must be provided");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest("User not found");

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
                return Ok("Email confirmed successfully");

            return BadRequest("Email confirmation failed");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                    return BadRequest("Please confirm your email before logging in");

                var token = await _tokenService.GenerateTokenAsync(user);
                return Ok(new { Token = token });
            }

            if (result.IsLockedOut)
                return StatusCode(StatusCodes.Status423Locked, "Account is locked out");

            return Unauthorized("Invalid login attempt");
        }
    }
}
