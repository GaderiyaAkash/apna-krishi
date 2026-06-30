using ApnaKrishi.Models;
using ApnaKrishi.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApnaKrishi.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var vm = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                Address = user.Address,
                City = user.City,
                State = user.State,
                PinCode = user.PinCode
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Mobile = model.Mobile;
            user.Address = model.Address;
            user.City = model.City;
            user.State = model.State;
            user.PinCode = model.PinCode;

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        public IActionResult ChangePassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }
    }
}
