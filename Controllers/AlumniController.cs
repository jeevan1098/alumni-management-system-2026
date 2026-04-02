using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Identity;

namespace Alumni_Management_System.Controllers
{
    [Authorize]
    public class AlumniController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<Alumni> alumniQuery = _context.Alumni.Include(a => a.User);

            if (roles.Contains(Constants.AlumniRole))
            {

                alumniQuery = alumniQuery.Where(a => a.Privacy == false);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                alumniQuery = alumniQuery.Where(a =>
                    a.FirstName.Contains(searchString) ||
                    a.LastName.Contains(searchString) ||
                    a.JagId.Contains(searchString) ||
                    a.PermanentEmail.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["UserRole"] = roles.FirstOrDefault();

            var currentAlumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id || a.JagId == currentUser.JagId);
            ViewData["CurrentAlumniId"] = currentAlumni?.AlumniId;

            return View(await alumniQuery.ToListAsync());
        }

        public async Task<IActionResult> MyProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id || a.JagId == currentUser.JagId);
            if (alumni == null)
            {
                TempData["ErrorMessage"] = "Alumni profile not found.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Edit", new { id = alumni.AlumniId });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            return View(alumni);
        }

        [Authorize(Roles = "Admin")] // Only Admin can create alumni manually
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can create alumni manually
        public async Task<IActionResult> Create([Bind("AlumniId,JagId,Prefix,FirstName,PreferredFirstName,LastName,Gender,AgeAtGraduation,StudentEmail,PermanentEmail,Phone,Address,City,State,Postcode,Country,GraduationYear,SolicitationCode,SocialMediaAccount,Privacy,IsActive,LastUpdated")] Alumni alumni)
        {

            if (ModelState.IsValid)
            {
                alumni.LastUpdated = DateTime.Now;
                alumni.IsActive = false;
                _context.Add(alumni);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alumni created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(alumni);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumni = await _context.Alumni.Include(a => a.User).FirstOrDefaultAsync(a => a.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(alumni.JagId) && string.IsNullOrEmpty(alumni.UserId))
            {
                var linkedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.JagId == alumni.JagId);
                if (linkedUser != null)
                {
                    alumni.UserId = linkedUser.Id;
                    _context.Update(alumni);
                    await _context.SaveChangesAsync();
                }
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {

                if (alumni.JagId != currentUser.JagId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own profile.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(alumni);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniId,JagId,Prefix,FirstName,PreferredFirstName,LastName,Gender,AgeAtGraduation,StudentEmail,PermanentEmail,Phone,Address,City,State,Postcode,Country,GraduationYear,SolicitationCode,SocialMediaAccount,Privacy,IsActive,LastUpdated")] Alumni alumni)
        {
            if (id != alumni.AlumniId)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {

                if (alumni.JagId != currentUser.JagId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own profile.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    alumni.LastUpdated = DateTime.Now;
                    _context.Update(alumni);

                    if (roles.Contains(Constants.AlumniRole) && currentUser.IsFirstLogin)
                    {
                        currentUser.IsFirstLogin = false;
                        await _userManager.UpdateAsync(currentUser);
                        TempData["SuccessMessage"] = "Profile updated successfully! Welcome to the Alumni Management System.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Profile updated successfully!";
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniExists(alumni.AlumniId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(alumni);
        }

        [Authorize(Roles = "Admin")] // Only Admin can delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            return View(alumni);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);

            if (alumni == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {

                    var user = alumni.User;

                    _context.Alumni.Remove(alumni);

                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                        {
                            throw new Exception("Failed to delete associated user account.");
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Alumni and associated user account deleted successfully!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Error during deletion: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AlumniExists(int id)
        {
            return _context.Alumni.Any(e => e.AlumniId == id);
        }
    }
}
