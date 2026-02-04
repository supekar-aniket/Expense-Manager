// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ExpenseManager.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ExpenseManager.Helper;

namespace ExpenseManager.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UrlHelper _urlHelper;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            UrlHelper urlHelper)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _urlHelper = urlHelper;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>


            [Required]
            [StringLength(255, ErrorMessage ="First Name must be of 255 characters.")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }


            [Required]
            [StringLength(255, ErrorMessage = "Last Name must be of 255 characters.")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }


            [Required]
            [EmailAddress(ErrorMessage = "Please enter a valid email address (must contain @).")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.DateAndTime = DateTime.Now;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Assign default "User" role
                    await _userManager.AddToRoleAsync(user, "User");

                    // Mark email as confirmed immediately
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);

                    // Build login URL using custom UrlHelper (so it works on phone too)
                    var loginUrl = _urlHelper.BuildUrl("/Identity/Account/Login");
                    var getMSG = GetWelcomeEmailBody(Input.FirstName, loginUrl);

                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "🎉 Welcome to Expense Manager!",
                        getMSG
                    );


                    // Redirect user to Login page
                    return RedirectToPage("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private string GetWelcomeEmailBody(string firstName, string loginUrl)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; background-color:#f4f6f9; padding:20px;'>
                    <div style='max-width:600px; margin:0 auto; background:white; border-radius:10px; padding:30px; box-shadow:0 4px 12px rgba(0,0,0,0.1);'>
            
                        <h1 style='color:#2563eb; text-align:center;'>Welcome, {firstName}! 🎉</h1>
            
                        <p style='font-size:16px; color:#333; line-height:1.6;'>
                            Your account has been <strong>successfully created</strong> with 
                            <span style='color:#2563eb; font-weight:bold;'>Expense Manager</span>.  
                            We're excited to have you onboard 🚀.
                        </p>

                        <p style='font-size:16px; color:#333; line-height:1.6;'>
                            With ExpenseManager you can:
                        </p>
            
                        <ul style='font-size:16px; color:#333; line-height:1.6;'>
                            <li>✅ Track your daily expenses</li>
                            <li>✅ Categorize spending with ease</li>
                            <li>✅ Export & analyze reports</li>
                            <li>✅ Stay secure with Identity authentication</li>
                        </ul>

                        <div style='text-align:center; margin:30px 0;'>
                            <a href='{loginUrl}' 
                               style='background:#2563eb; color:white; text-decoration:none; padding:12px 24px; border-radius:6px; font-size:16px;'>
                                🔑 Login to Your Account
                            </a>
                        </div>

                        <p style='font-size:14px; color:#666; text-align:center;'>
                            Need help? Reply to this email anytime.<br/>
                            Thank you for choosing <strong>Expense Manager</strong> 💙
                        </p>

                    </div>
                </div>
            ";
        }



        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
