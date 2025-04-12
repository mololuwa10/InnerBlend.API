using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InnerBlend.API.Models;
using InnerBlend.API.Models.Authentication;
using InnerBlend.API.Models.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InnerBlend.API.Controllers.AuthController
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly IConfiguration _configuration = configuration;
        
        
        // REGISTER FUNCTION
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email ?? string.Empty);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return BadRequest(ModelState);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    DateCreated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                };

                var result = await _userManager.CreateAsync(user, model.Password ?? string.Empty);

                if (result.Succeeded)
                {
                    var token = JwtTokenGenerator.GenerateToken(user.Id.ToString(), user.UserName ?? string.Empty, _configuration!);
                    return Ok(new { Token = token, message = "User registered successfully" });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return BadRequest(ModelState);
        }
        
        // LOGIN FUNCTION
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid) 
            {
                User? user = null;
                
                if (new EmailAddressAttribute().IsValid(model.UsernameOrEmail ?? string.Empty)) 
                {
                    user = await _userManager.FindByEmailAsync(model.UsernameOrEmail ?? string.Empty);
                } else 
                {
                    user = await _userManager.FindByNameAsync(model.UsernameOrEmail ?? string.Empty);
                }
                
                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password ?? string.Empty, false);

                    if (result.Succeeded)
                    {
                        var token = JwtTokenGenerator.GenerateToken(user.Id.ToString(), user.UserName ?? string.Empty, _configuration!);
                        return Ok(new { Token = token, message = "User logged in successfully" });
                    }
                }
                
                ModelState.AddModelError(string.Empty, "Invalid login attempt");
            }
            return BadRequest(new { message = "Invalid login attempt" });
        }
        
        // GETTING DETAILS OF CURRENT LOGGED IN USER
        [HttpGet("details")]
        public async Task<IActionResult> CurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User not authenticated");
			}

			var user = await _userManager!.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User not found");
			}

			// Return all user details
			return Ok(
				new
				{
					Id = user.Id,
					FirstName = user.FirstName,
					LastName = user.LastName,
					UserName = user.UserName,
					Email = user.Email,
					DateCreated = user.DateCreated,
					PhoneNumber = user.PhoneNumber,
					PhoneNumberConfirmed = user.PhoneNumberConfirmed,
					TwoFactorEnabled = user.TwoFactorEnabled,
					LockoutEnabled = user.LockoutEnabled,
					LockoutEnd = user.LockoutEnd,
					AccessFailedCount = user.AccessFailedCount,
					ConcurrencyStamp = user.ConcurrencyStamp,
					SecurityStamp = user.SecurityStamp,
					EmailConfirmed = user.EmailConfirmed,
				}
			);
        }
        
        // LOGOUT FUNCTION
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "User logged out successfully" });
        }
    }
}