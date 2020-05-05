using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using System.DirectoryServices.AccountManagement;

namespace WebApi.Helpers
{
    public static class ExtensionMethods
    {
        public static IEnumerable<User> WithoutPasswords(this IEnumerable<User> users) 
        {
            if (users == null) return null;

            return users.Select(x => x.WithoutPassword());
        }

        public static User WithoutPassword(this User user) 
        {
            if (user == null) return null;

            user.Password = null;
            return user;
        }
        public static User WithoutPassword(this User user, UserPrincipal userPrincipal)
        {
            if (userPrincipal == null) return null;
            if (user == null) return null;
            user.FirstName = userPrincipal.DisplayName;
            user.LastName = "";
            user.Username = userPrincipal.Name;
            user.Id = 1; 
            user.Password = null;
            return user;
        }

    }
}