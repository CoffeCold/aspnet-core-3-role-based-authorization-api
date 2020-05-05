namespace WebApi.Entities
{
    public static class RoleTypes
    {
        public const string HeroesReader = "HeroesReader";
        public const string HeroesWriter = "HeroesWriter";
    }

    public class Role
    {

        public string roletype { get; set; }
     }
}