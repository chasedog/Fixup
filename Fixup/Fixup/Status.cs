using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace Fixup
{
    public class Status {
        public IEnumerable<StatusEntry> Unstaged { get; set; } = new List<StatusEntry>();
        public IEnumerable<StatusEntry> Staged { get; set; } = new List<StatusEntry>();
        public IEnumerable<StatusEntry> Untracked { get; set; } = new List<StatusEntry>();

        public static Status FromRepo(Repository repository) {
            var repoStatus = repository.RetrieveStatus();

            var status = new Status {
                Unstaged = repoStatus.Modified,
                Untracked = repoStatus.Untracked,
                Staged = repoStatus.Staged,
            };

            return status;
        }

        public IEnumerable<StatusEntry> AllTracked => Unstaged.Concat(Staged);

        public bool IsEmpty()
        {
            return !Unstaged.Any() && !Staged.Any() && !Untracked.Any();
        }
    }
}