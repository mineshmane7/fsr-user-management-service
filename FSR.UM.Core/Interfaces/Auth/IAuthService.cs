using FSR.UM.Core.Models.Auth;

namespace FSR.UM.Core.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
