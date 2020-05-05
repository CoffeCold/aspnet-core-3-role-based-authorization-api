using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Entities;
using WebApi.Helpers;
using System.DirectoryServices.AccountManagement;


namespace WebApi.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
    }


    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private static string[] allowedroles = { "HeroesReader", "HeroesWriter" };
        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;

         
        }

        public static User ValidateCredentials(string userName, string password)
        {
            try
            {
                //new PrincipalContext(ContextType.Domain, "ESTAGIOIT", "CN=Users,DC=estagioit,DC=local");
                //optionally a container (as an LDAP path - a "distinguished" name, full path but without any LDAP:// prefix)
                using (var adContext = new PrincipalContext(ContextType.Machine, null))
                {
                    if (adContext.ValidateCredentials(userName, password))
                    {
                        //user
                        UserPrincipal usr = new UserPrincipal(adContext);
                        usr.SamAccountName = userName;
                        var searcher = new PrincipalSearcher(usr);
                        usr = searcher.FindOne() as UserPrincipal;
                        User user = new User();
                        user.WithoutPassword(usr);


                        //roles
                         PrincipalSearchResult<Principal> groups = usr.GetAuthorizationGroups();
                        user.Roles = GetRoles(groups);
                        return user;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;

            }
        }

        private static Role[] GetRoles(PrincipalSearchResult<Principal> roles)
        {
            List<Role> roleslist = new List<Role>();
            foreach (Principal roleprincipal in roles)
            {
                if (!String.IsNullOrEmpty(allowedroles.Where(role => role == roleprincipal.Name).FirstOrDefault()))
                {
                    Role role = new Role();
                    role.roletype = roleprincipal.Name;
                    roleslist.Add(role);
                }
            }
            return roleslist.ToArray();
        }


        public User Authenticate(string username, string password)
        {
            //User user = _users.SingleOrDefault(x => x.Username == username && x.Password == password);
            // hook on to AD here...
            User user = ValidateCredentials(username, password);
            // return null if user not found
            if (user == null)
                return null;
            List<Claim> claimlist = new List<Claim>();
            foreach (Role role in user.Roles)
            {
                claimlist.Add(new Claim(ClaimTypes.Role, role.roletype));
            }
            claimlist.Add(new Claim(ClaimTypes.Name, user.Id.ToString()));
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimlist.ToArray()),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            return user.WithoutPassword();
        }

        public IEnumerable<User> GetAll()
        {
            // just a stub to get some data here..
            List<User> _users = new List<User>();

            List<Role> rolesdoe = new List<Role>();
            rolesdoe.Add(new Role() { roletype = "def" });
            User doe = new User { Id = 1, FirstName = "Does", LastName = "not matter", Username = "doesnotmatter", Password = "doe", Roles = rolesdoe.ToArray() };

            List<Role> roleswoe = new List<Role>();
            roleswoe.Add(new Role() { roletype = "abc" });
            User woe = new User { Id = 1, FirstName = "Does", LastName = "not matter as well", Username = "doenotmatter", Password = "woe", Roles = roleswoe.ToArray() };

            _users.Add(doe);
            _users.Add(woe);
            return _users.WithoutPasswords();
        }

        public User GetById(int id)
        {
            // just a stub to get some data here..

            List<Role> roleswoe = new List<Role>();
            roleswoe.Add(new Role() { roletype = "fgh" });
            roleswoe.Add(new Role() { roletype = "klm" });
            User woe = new User { Id = 1, FirstName = "Does", LastName = "not matter", Username = "doenotmatter", Password = "woe", Roles = roleswoe.ToArray() };
            return woe;
        }
    }
}