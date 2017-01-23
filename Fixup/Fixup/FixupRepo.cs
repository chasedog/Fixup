using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
// ReSharper disable InconsistentNaming

namespace Fixup {
    public class FixupRepo {
        private readonly Dictionary<string, List<Diff>> _ShasToFiles = new Dictionary<string, List<Diff>>();
        private readonly Repository _Repo;

        public FixupRepo(string repoPath)
        {
            _Repo = new Repository(repoPath);
        }

        public  Dictionary<string, List<Diff>> GetShasToFiles(ObjectId givenSha)
        {
            var sha = _Repo.Lookup<Commit>(givenSha)?.Sha;
            //if (sha == null)
            //{
            //    throw new Exception($"Invalid sha {givenSha}");
            //}

            var status = Status.FromRepo(_Repo);
            var result = new Dictionary<string, List<Diff>>();
            _IterateCommits(_Repo, status, sha);
            foreach (var file in status.AllTracked)
            {
                var diffs = _ShasToFiles[file.FilePath];
                if (diffs.Any())
                    Console.WriteLine($"{file.FilePath} was changed in:");
                foreach (var diff in diffs)
                {
                    Console.WriteLine($" {diff.Commit.Message}");
                }
            }
            foreach (var file in status.AllTracked)
            {
                result[file.FilePath] = _ShasToFiles.ContainsKey(file.FilePath) ? _ShasToFiles[file.FilePath] : new List<Diff>();
            }
            return result;
        }

        private void _IterateCommits(Repository repo, Status status, string beginSha)
        {

            var afterCommit = repo.Head.Tip;
            var commits = repo.Commits.QueryBy(new CommitFilter {SortBy = CommitSortStrategies.Topological, /*IncludeReachableFrom = beginSha*/});
            foreach (var commit in commits)
            {
                var result = repo.Diff.Compare<TreeChanges>(commit.Tree, afterCommit.Tree);
                foreach (var entry in result) {
                    if (_ShasToFiles.ContainsKey(entry.Path))
                        _ShasToFiles[entry.Path].Add(new Diff(entry, afterCommit));
                    else
                        _ShasToFiles[entry.Path] = new List<Diff> { new Diff(entry, afterCommit) };
                }
                afterCommit = commit;
            }
        }
    }

    public class Diff
    {
        public TreeEntryChanges Delta { get; set; }
        public Commit Commit { get; set; }

        public Diff(TreeEntryChanges delta, Commit commit)
        {
            Delta = delta;
            Commit = commit;
        }
    }
}
