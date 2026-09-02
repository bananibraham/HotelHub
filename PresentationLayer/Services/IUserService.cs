using System.Security.Claims;
using System.Threading.Tasks;

namespace PresentationLayer.Services
{
    public interface IUserService
    {
        Task<string> GetDisplayNameAsync(ClaimsPrincipal user);
    }
}
