// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using ExpenseManager.Areas.Identity.Data;
using ExpenseManager.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ExpenseManager.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UrlHelper _urlHelper; // ✅ inject UrlHelper

        public DeletePersonalDataModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            IEmailSender emailSender,
            UrlHelper urlHelper) // ✅ add here
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _urlHelper = urlHelper;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = user.Email;
            var firstName = user.FirstName;

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            // ✅ Build absolute home URL using UrlHelper
            var homeUrl = _urlHelper.BuildUrl("/");

            // Send account deletion email
            var getDeletionMsg = GetAccountDeletionEmailBody(firstName, homeUrl);

            await _emailSender.SendEmailAsync(
                email,
                "❌ Your Expense Manager Account Has Been Deleted",
                getDeletionMsg
            );

            return Redirect("~/");
        }

        private string GetAccountDeletionEmailBody(string firstName, string homeUrl)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; background-color:#f4f6f9; padding:20px;'>
                    <div style='max-width:600px; margin:0 auto; background:white; border-radius:10px; padding:30px; box-shadow:0 4px 12px rgba(0,0,0,0.1);'>
    
                        <h1 style='color:#dc2626; text-align:center;'>Goodbye, {firstName}! 👋</h1>
    
                        <p style='font-size:16px; color:#333; line-height:1.6;'>
                            We wanted to let you know that your <strong>Expense Manager</strong> account 
                            has been <span style='color:#dc2626; font-weight:bold;'>successfully deleted</span>.
                        </p>

                        <p style='font-size:16px; color:#333; line-height:1.6;'>
                            All your personal data has been removed from our system in accordance with our data policy.  
                            We're sorry to see you go, but we respect your decision ❤️.
                        </p>
    
                        <div style='text-align:center; margin:30px 0;'>
                            <p style='font-size:14px; color:#666;'>
                                Changed your mind? You can always come back and create a new account anytime 🚀
                            </p>
                            <a href='{homeUrl}' 
                               style='background:#2563eb; color:white; text-decoration:none; padding:12px 24px; border-radius:6px; font-size:16px;'>
                                ✨ Create a New Account
                            </a>
                        </div>

                        <p style='font-size:14px; color:#666; text-align:center;'>
                            If this action was not performed by you, please contact our support immediately.<br/>
                            Thank you for having been a part of <strong>Expense Manager</strong> 💙
                        </p>

                    </div>
                </div>
            ";
        }
    }
}
