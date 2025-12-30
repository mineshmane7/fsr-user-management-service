using FSR.UM.Core.Models;

namespace FSR.UM.Core.Interfaces.Auth
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
