using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels
{
    public class UserDetailsViewModel
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public long ClientId { get; set; }
        public string Role { get; set; }

    }
}
