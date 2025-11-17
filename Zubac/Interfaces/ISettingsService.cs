using Zubac.Models;

namespace Zubac.Interfaces
{
    public interface ISettingsService
    {
        public Task<List<StaffViewModel>> GetStaff();

        public Task<string> CreateStaffLinkAsync(int staffId);

        public Task<bool> SetStaffPasswordAsync(int staffId, string newPassword, string token);

        public bool VerifyPassword(Users user, string password);

        public Task<StaffLink> GetStaffLinkByTokenAsync(string token);
    }
}
