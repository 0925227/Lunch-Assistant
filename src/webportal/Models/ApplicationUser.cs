using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace webportal.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public List<Order> Orders { get; set; }

        public int Age { get; set; }
        public int Weigth { get; set; }
        public string Sex { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public string OrderLine { get; set; }
        public DateTime DateTime { get; set; }

        public string Email { get; set; }
        public ApplicationUser User { get; set; }
    }
}
