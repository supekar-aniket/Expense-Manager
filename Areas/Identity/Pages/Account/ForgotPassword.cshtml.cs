// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ExpenseManager.Areas.Identity.Data;
using ExpenseManager.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ExpenseManager.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly UrlHelper _urlHelper;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender, UrlHelper urlHelper)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _urlHelper = urlHelper;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }


        public async Task<IActionResult> OnPostAsync()
        { 
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                // Use custom UrlHelper for port forwarding / tunneling
                var path = $"/Identity/Account/ResetPassword?token={token}&email={Input.Email}";
                var callbackUrl = _urlHelper.BuildUrl(path);

                // Build themed email body
                var messageBody = GetResetPasswordEmailBody(user.FirstName ?? "User", callbackUrl);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "🔑 Reset Your Expense Manager Password",
                    messageBody);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }

        private string GetResetPasswordEmailBody(string firstName, string resetUrl)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; background-color:#f4f6f9; padding:20px;'>
                    <div style='max-width:600px; margin:0 auto; background:white; border-radius:10px; padding:30px; box-shadow:0 4px 12px rgba(0,0,0,0.1);'>
    
                        <h1 style='color:#2563eb; text-align:center;'>Reset Your Password</h1>
    
                        <p style='font-size:16px; color:#333; line-height:1.6;'>
                            Hello <strong>{firstName}</strong>,
                        </p>

                        <p style='font-size:16px; color:#333; line-height:1.6;'>
                            We received a request to reset your <span style='color:#2563eb; font-weight:bold;'>Expense Manager</span> password.  
                            Click the button below to create a new one:
                        </p>
    
                        <div style='text-align:center; margin:30px 0;'>
                            <a href='{resetUrl}' 
                               style='background:#2563eb; color:white; text-decoration:none; padding:12px 24px; border-radius:6px; font-size:16px;'>
                                🔑 Reset Password
                            </a>
                        </div>

                        <p style='font-size:14px; color:#e11d48; font-weight:bold; text-align:center;'>
                            ⚠️ Note: This reset link will expire in 10 minutes for your security.
                        </p>

                        <p style='font-size:14px; color:#666;'>
                            If you did not request this, you can safely ignore this email.  
                            Your current password will remain unchanged.
                        </p>
    
                        <p style='font-size:14px; color:#666; text-align:center;'>
                            Stay secure 💙<br/>
                            — The Expense Manager Team
                        </p>
    
                    </div>
                </div>
            ";
        }
    }
}
