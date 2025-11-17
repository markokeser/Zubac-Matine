using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zubac.Interfaces;
using Zubac.Models;
using Zubac.Services;

namespace Zubac.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISettingsService _service;

        public SettingsController(ISettingsService service)
        {
            _service = service;
        }

        // GET: SettingsController
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Staff()
        {
          var response = await _service.GetStaff();

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePasswordLink(int id)
        {
            var token = await _service.CreateStaffLinkAsync(id); // vraća token string ili pun link
            var link = Url.Action("SetPassword", "Settings", new { token = token }, Request.Scheme);

            return Json(new { success = true, link = link });
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword(string token)
        {
            var staffLink = await _service.GetStaffLinkByTokenAsync(token);
            if (staffLink == null)
                return NotFound("Invalid or expired link.");

            var staff = staffLink.Staff;

            var model = new SetPasswordViewModel
            {
                StaffId = staff.Id,
                Token = token,
                Username = staff.Username
            };

            return View(model);
        }

        // POST: /Settings/SetPassword
        [HttpPost]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _service.SetStaffPasswordAsync(model.StaffId, model.NewPassword, model.Token);
            if (success)
                return RedirectToAction("Login", "Account");

            ModelState.AddModelError("", "Unable to set password. Link might be invalid or expired.");
            return View(model);
        }
    }
}
