using System.Threading.Tasks;
using AuthPilot.Models.Auth; // Keep namespace as in DTO project

namespace Audit.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(RegisterModel model);
        Task<string?> LoginAsync(LoginModel model);
    }
}
