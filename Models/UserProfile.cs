using System;
using System.ComponentModel.DataAnnotations;

namespace tasktServer.Models
{
    public class UserProfile
    {
        [Key]
        public Guid ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime LastSuccessfulLogin { get; set; }
    }
}
