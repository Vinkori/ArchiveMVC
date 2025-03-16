using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


namespace ArchiveDomain.Model
{
    public class User : IdentityUser
    {
        public int Year { get; set; }
    }
}

