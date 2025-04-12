using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InnerBlend.API.Models;
using InnerBlend.API.Models.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InnerBlend.API.Controllers.AuthController
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserAccountController(UserManager<User> userManager) : ControllerBase
    {
        
        [HttpPut("edit")]
        [Authorize]
        public async Task<IActionResult> EditUser([FromBody] EditUserModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("You are not logged in");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.FirstName = model.FirstName ?? user.FirstName;
            user.LastName = model.LastName ?? user.LastName;
            user.Email = model.Email ?? user.Email;
            user.UserName = model.Email ?? user.UserName;
            user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;

            user.DateModified = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            // Update user successfully
            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return BadRequest("Failed to update user");
            }


            return Ok(new { user.FirstName, user.LastName, user.Email, user.UserName, user.PhoneNumber, user.DateCreated, user.DateModified, message = "User updated successfully" });
        }
        
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("You are not logged in");
            }
            
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            
           // Check if new password is provided, but old password is missing
			if (!string.IsNullOrEmpty(model.NewPassword) && string.IsNullOrEmpty(model.OldPassword))
			{
				return BadRequest("Please enter your old password first before trying to change to a new password.");
			}
			
			// Validate Old password
			if (!string.IsNullOrEmpty(model.OldPassword)) 
			{
				var passwordCheck = await userManager.CheckPasswordAsync(user, model.OldPassword);
				if (!passwordCheck) 
				{
					return BadRequest("Old password is incorrect");
				}
				
				// Ensure the new password is not the same as the old password
				if (model.OldPassword == model.NewPassword)
				{
					return BadRequest("New password cannot be the same as the old password");
				}
			}
			
			// Check if NewPassword and ConfirmNewPassword match
			if (!string.IsNullOrEmpty(model.NewPassword) && model.NewPassword != model.ConfirmNewPassword)
			{
				return BadRequest("New password and confirm password do not match.");
			}
			
			if (!string.IsNullOrEmpty(model.OldPassword) && !string.IsNullOrEmpty(model.NewPassword))
			{
				var token = await userManager.GeneratePasswordResetTokenAsync(user);
				var resetResult = await userManager.ResetPasswordAsync(user, token, model.NewPassword);

				if (!resetResult.Succeeded)
				{
					return BadRequest("Failed to update password.");
				}
			}
			
            user.DateModified = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
			
			var updateResult = await userManager.UpdateAsync(user);

			if (!updateResult.Succeeded) 
			{
				return BadRequest("Failed to update password");
			}

			return Ok(new {user.DateModified, message = "Password updated successfully"});
        }

        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("You are not logged in");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            
            var deleteResult = await userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                return BadRequest("Failed to delete user");
            }
            
            return Ok(new {message = "User deleted successfully"});
        }
    }
}