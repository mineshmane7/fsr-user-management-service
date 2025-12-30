using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Models;
using FSR.UM.Infrastructure.SqlServer.Db.PropertyDb;
using Microsoft.EntityFrameworkCore;

namespace FSR.UM.Infrastructure.SqlServer.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly PropertyDbContext _db;

        public PropertyRepository(PropertyDbContext db)
        {
            _db = db;
        }

        public async Task<List<Property>> GetAllAsync()
        {
            return await _db.Properties.ToListAsync();
        }

        public async Task<Property?> GetByIdAsync(int id)
        {
            return await _db.Properties.FindAsync(id);
        }

        public async Task<Property> AddAsync(Property property)
        {
            _db.Properties.Add(property);
            await _db.SaveChangesAsync();
            return property;
        }

        public async Task<Property> UpdateAsync(Property property)
        {
            _db.Properties.Update(property);
            await _db.SaveChangesAsync();
            return property;
        }

        public async Task DeleteAsync(int id)
        {
            var property = await _db.Properties.FindAsync(id);
            if (property != null)
            {
                _db.Properties.Remove(property);
                await _db.SaveChangesAsync();
            }
        }
    }
}
