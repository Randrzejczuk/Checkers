using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class User : IdentityUser
    {
        public User()
        {
            Messages = new HashSet<Message>();
        }
        public ICollection<Message> Messages { get; set; }
        /*
         * Returns user name not as an email
         */
        public string ShortName()
        {
            return this.UserName.Split('@')[0];
        }
    }
}
