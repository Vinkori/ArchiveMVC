using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveDomain.Model
{
    public interface IAppUser
    {
        string Id { get; }
        string UserName { get; }
    }
}
