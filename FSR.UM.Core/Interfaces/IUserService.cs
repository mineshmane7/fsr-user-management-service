using FSR.UM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSR.UM.Core.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
    }
}
