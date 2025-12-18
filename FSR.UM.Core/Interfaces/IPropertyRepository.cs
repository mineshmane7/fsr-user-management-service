using FSR.UM.Core.Models;

namespace FSR.UM.Core.Interfaces
{
    public interface IPropertyRepository
    {
        Task<List<Property>> GetAllAsync();
        Task<Property> AddAsync(Property property);
    }
}
