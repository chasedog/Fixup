using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Fixup {
    class Program {
        static void Main(string[] args) {
            var repo = new FixupRepo(@"C:\Projects\TestRepo");
            var mappings = repo.GetMappings((ObjectId)"367e9408902a2a2217546e7af981d9105365b9ac");

            repo.CreateFixupCommits(mappings);

            
        }
    }
}
