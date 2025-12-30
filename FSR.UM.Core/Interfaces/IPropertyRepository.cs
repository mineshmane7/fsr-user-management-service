using FSR.UM.Core.Models;

namespace FSR.UM.Core.Interfaces
{
    public interface IPropertyRepository
    {
        Task<List<Property>> GetAllAsync();
        Task<Property?> GetByIdAsync(int id);
        Task<Property> AddAsync(Property property);
        Task<Property> UpdateAsync(Property property);
        Task DeleteAsync(int id);
    }
}
