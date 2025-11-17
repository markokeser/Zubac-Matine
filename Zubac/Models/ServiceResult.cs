namespace Zubac.Models
{
    public class ServiceResult
    {
        public bool Success { get; set; } = true; // default true
        public string ErrorTitle { get; set; }
        public string ErrorMessage { get; set; }
    }
}
