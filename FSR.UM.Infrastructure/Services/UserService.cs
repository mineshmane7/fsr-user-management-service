using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSR.UM.Infrastructure.Services
{
    public class UserService : IUserService
    {
        public IEnumerable<User> GetUsers()
        {
            return new List<User>
            {
                new User { Id = 1, Email = "john@test.com" },
                new User { Id = 2, Email = "admin@test.com" }
            };
        }
    }
}
