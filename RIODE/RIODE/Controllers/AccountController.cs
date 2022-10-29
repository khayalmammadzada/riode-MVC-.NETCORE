using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RIODE.Models;
using RIODE.Utilities.Enums;
using RIODE.ViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RIODE.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Register(RegisterVM register, string email)
        {
            if (!ModelState.IsValid) return View();
            if (string.IsNullOrWhiteSpace(email)) return BadRequest();
            if (!register.Terms)
            {
                ModelState.AddModelError("Terms", "Please check our terms and conditions");
                return View();
            }
            AppUser user = new AppUser
            {
                FirstName = register.Firstname,
                LastName=register.Lastname,
                UserName=register.Username,
                Email=register.Email
            };

            IdentityResult result = await _userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
             
                return View();
            }
            var newUser = await _userManager.FindByEmailAsync(email);
            SendMail(email, "Welcome to Riode Store", "Congragulations, thank you for registering");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM login)
        {
            if (!ModelState.IsValid) return View();

            AppUser user = await _userManager.FindByNameAsync(login.Username);
            if (user is null) return View();


            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user, login.Password, login.RememberMe, true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "Due to overtrying you have been blocked about 10 minutes");
                    return View();
                }

                ModelState.AddModelError("", "Username or password is incorrect");
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        //public async Task CreateRoles()
        //{
        //    foreach (var item in Enum.GetValues(typeof(Roles)))
        //    {
        //        if (!await _roleManager.RoleExistsAsync(item.ToString()))
        //        {
        //            await _roleManager.CreateAsync(new IdentityRole { Name = item.ToString() });
        //        }
        //    }
        //}

        //public async Task<IActionResult> CreateSA()
        //{
        //    AppUser admin = new AppUser { FirstName = "Khayal",LastName="Mammadzada", UserName = "khayal", Email = "khayalkm@code.edu.az", IsAdmin = true };
        //    var result = await _userManager.CreateAsync(admin, "Xeyal123");
        //    if (!result.Succeeded)
        //    {
        //        var err = "";
        //        foreach (var item in result.Errors)
        //        {
        //            err += item;
        //        }
        //        return Content(err);
        //    }
        //    await _userManager.AddToRoleAsync(await _userManager.FindByNameAsync(admin.UserName), Roles.SuperAdmin.ToString());
        //    return Ok(admin);
        //}

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


        [Authorize]
        public async Task<IActionResult> UpdateUser()
        {
            ViewBag.ActiveTab = "Dashboard";
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            return View(new UpdateUserVM { Fullname = user.FirstName, Email = user.Email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateUser(UpdateUserVM user)
        {
            ViewBag.ActiveTab = "Account";
            if (!ModelState.IsValid) return View();
            AppUser currentUser = _userManager.FindByNameAsync(HttpContext.User.Identity.Name).Result;
            if (currentUser == null) RedirectToAction(nameof(Login));
            AppUser existUser = _userManager.FindByEmailAsync(user.Email).Result;
            if (existUser != null && existUser.Email != currentUser.Email)
            {
                ModelState.AddModelError("Email", "This email already exist");
                return View();
            }
            if (user.CurrentPassword != user.NewPassword)
            {
                var result = _userManager.ChangePasswordAsync(currentUser, user.CurrentPassword, user.NewPassword);
                if (!result.IsCompletedSuccessfully)
                {
                    if (!_userManager.CheckPasswordAsync(currentUser, user.CurrentPassword).Result)
                    {
                        ModelState.AddModelError("CurrentPassword", "Current password is not correct");
                        return View();
                    }
                }
            }
            currentUser.Email = user.Email;
            currentUser.FirstName = user.Fullname;
            _userManager.UpdateAsync(currentUser);
            ViewBag.ActiveTab = "Dashboard";
            ViewBag.ToastrMessage = "Changes saved successfully";
            return View();


            //string a = _userManager.GenerateEmailConfirmationTokenAsync();
            //_userManager.Verif
            //ConfirmEmail/UserId?token=a
            //Url.Action("ConfirmEmail", "Account", new { username = "", token = "" }, HttpContext.Request.Scheme);
        }



      

        //Sending email method
        //Registrationda bu metodu cagirarag register eden customere welcome mesaji gonderilir
        private void SendMail(string email, string subject, string body)
        {
            string myEmail = "riodestore@gmail.com";
            string pass = "vkpnyijbilytijks";

            var from = new MailAddress(myEmail, "Riode support");
            var to = new MailAddress(email);

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(myEmail, pass)
            };
            using (var message = new MailMessage(from, to) { Subject = subject, Body = body, IsBodyHtml = true })
            {
                smtp.Send(message);
            }

        }



    }
}

