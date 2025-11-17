using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Web.Helpers;
using Zubac.Data;
using Zubac.Interfaces;
using Zubac.Models;

namespace Zubac.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Users> _passwordHasher;

        public SettingsService(ApplicationDbContext context, IPasswordHasher<Users> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<StaffViewModel>> GetStaff()
        {
            List<StaffViewModel> response = await _context.Users.Select(a => new StaffViewModel
            {
                Id = a.Id,
                Username = a.Username,
                UserRank = a.UserRank
            }).ToListAsync();

            return response;
        }

        public async Task<string> CreateStaffLinkAsync(int staffId)
        {
            var token = Guid.NewGuid().ToString(); // generiše jedinstveni token

            var staffLink = new StaffLink
            {
                StaffId = staffId,
                Token = token,
                IsUsed = false,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(1) // link važi 1h
            };

            _context.StaffLinks.Add(staffLink);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<StaffLink> GetStaffLinkByTokenAsync(string token)
        {
            var response = await _context.StaffLinks
                .Include(sl => sl.Staff)
                .FirstOrDefaultAsync(sl => sl.Token == token && !sl.IsUsed);
            return response;
        }

        public async Task<bool> SetStaffPasswordAsync(int staffId, string newPassword, string token)
        {
            var staff = await _context.Users.FirstOrDefaultAsync(u => u.Id == staffId);
            if (staff == null) return false;

            var staffLink = await _context.StaffLinks
                .FirstOrDefaultAsync(sl => sl.Token == token && sl.StaffId == staffId && !sl.IsUsed);

            if (staffLink == null) return false;

            // Hashujemo novu lozinku
            staff.Password = _passwordHasher.HashPassword(staff, newPassword);

            // Obeležavamo link kao iskorišćen
            staffLink.IsUsed = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public bool VerifyPassword(Users user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
