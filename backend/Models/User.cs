namespace TMS.Models
{
    public class User{
        User( string name, string email)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
        }
        public Guid Id;
        public string Name { get; }
        public string Email { get; }
    }
}