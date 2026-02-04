// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ExpenseManager.Areas.Identity.Data;
using ExpenseManager.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ExpenseManager.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UrlHelper _urlHelper;

        public ChangePasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ChangePasswordModel> logger,
            IEmailSender emailSender,
            UrlHelper urlHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _urlHelper = urlHelper;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            // Build email body
            var messageBody = ChangePasswordMsgBody(user);

            // send email
            await _emailSender.SendEmailAsync(
                user.Email,
                "🔒 Your Expense Manager Password Was Changed",
                messageBody
            );

            return RedirectToPage();
        }

        // Build HTML email body
        private string ChangePasswordMsgBody(ApplicationUser user)
        {
            // Use UrlHelper instead of Url.Page
            var resetPath = "/Identity/Account/ForgotPassword";
            var resetUrl = _urlHelper.BuildUrl(resetPath);

            return $@"
                <div style='font-family: Arial, sans-serif; background-color:#f4f6f9; padding:20px;'>
                    <div style='max-width:600px; margin:0 auto; background:white; border-radius:10px; padding:30px; box-shadow:0 4px 12px rgba(0,0,0,0.1);'>
    
                        <h1 style='color:#2563eb; text-align:center;'>🔒 Password Changed Successfully</h1>
    
                        <p style='font-size:16px; color:#333; line-height:1.6;'>
                            Hello <strong>{user.FirstName}</strong>,
                        </p>
    
                        <p style='font-size:16px; color:#333; line-height:1.6;'>
                            This is a confirmation that your <span style='color:#2563eb; font-weight:bold;'>Expense Manager</span> 
                            password was changed on <strong>{DateTime.UtcNow:dddd, dd MMM yyyy HH:mm} (UTC)</strong>.
                        </p>

                        <p style='font-size:16px; color:#333; line-height:1.6;'>
                            ✅ If this was <strong>you</strong>, no further action is required.
                        </p>

                        <p style='font-size:16px; color:#d32f2f; line-height:1.6;'>
                            ⚠️ If this was <strong>NOT you</strong>, please reset your password immediately:
                        </p>

                        <div style='text-align:center; margin:30px 0;'>
                            <a href='{resetUrl}' 
                               style='background:#d32f2f; color:white; text-decoration:none; padding:12px 24px; border-radius:6px; font-size:16px;'>
                                🔑 Reset Your Password
                            </a>
                        </div>

                        <p style='font-size:14px; color:#666; text-align:center;'>
                            Stay safe 💙<br/>
                            — The Expense Manager Team
                        </p>

                    </div>
                </div>
            ";
        }
    }
}
