using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
// ReSharper disable InconsistentNaming

namespace Fixup {
    public class ShaAndFileMappings
    {
        public DictionaryList<string, Diff> FileToDiffs = new DictionaryList<string, Diff>();
        public DictionaryList<string, string> ShasToFiles = new DictionaryList<string, string>();
    }

    public class FixupRepo {
        private readonly Repository _Repo;
        private readonly DictionaryList<string, Diff> _FileNamesToDiffs;

        public FixupRepo(string repoPath)
        {
            _Repo = new Repository(repoPath);
            _FileNamesToDiffs = new DictionaryList<string, Diff>();
        }

        public ShaAndFileMappings GetMappings(ObjectId givenSha)
        {
            var sha = _Repo.Lookup<Commit>(givenSha)?.Sha;
            //if (sha == null)
            //{
            //    throw new Exception($"Invalid sha {givenSha}");
            //}

            var result = new ShaAndFileMappings();
            _InitializeIfNecessary(_Repo, sha);
            //foreach (var file in status.AllTracked)
            //{
            //    var diffs = _FileNamesToDiffs[file.FilePath];

            //    if (diffs.Any())
            //        Console.WriteLine($"{file.FilePath} was changed in:");

            //    foreach (var diff in diffs)
            //    {
            //        Console.WriteLine($" {diff.Commit.Message}");
            //    }
            //}

            var status = Status.FromRepo(_Repo);
            _MapTrackedFilesToDiffs(status, result);
            _MapCommitsToFiles(result);

            return result;
        }

        private void _MapCommitsToFiles(ShaAndFileMappings result)
        {
            var r = new DictionaryList<string, string>();
            foreach (var file in result.FileToDiffs)
            {
               r.Add(file.Value.First().Commit.Sha, file.Key);
            }
            result.ShasToFiles = r;
        }

        private void _MapTrackedFilesToDiffs(Status status, ShaAndFileMappings result)
        {
            foreach (var file in status.AllTracked)
            {
                result.FileToDiffs[file.FilePath] = _FileNamesToDiffs.ValueOrEmptyList(file.FilePath);
            }
        }

        private void _InitializeIfNecessary(IRepository repo, string beginSha)
        {
            if (_FileNamesToDiffs.IsPopulated)
                return;

            var afterCommit = repo.Head.Tip;

            var commits = repo.Commits.QueryBy(new CommitFilter {SortBy = CommitSortStrategies.Topological, /*IncludeReachableFrom = beginSha*/});
            foreach (var commit in commits)
            {
                var result = repo.Diff.Compare<TreeChanges>(commit.Tree, afterCommit.Tree);
                foreach (var entry in result)
                {
                    _FileNamesToDiffs.Add(entry.Path, new Diff(entry, afterCommit));
                }
                afterCommit = commit;
            }
        }

        //public DictionaryList<string, Diff> GetCommitGroups(ObjectId lastCommit)
        //{
        //    _InitializeIfNecessary(_Repo, lastCommit.Sha);
        //    var shaAndFileMappings = new ShaAndFileMappings();
        //    _MapTrackedFilesToDiffs(Status.FromRepo(_Repo), shaAndFileMappings);
        //    foreach (var )
        //}
        public void CreateFixupCommits(ShaAndFileMappings mappings)
        {
            var author = _Repo.Head.Tip.Author;
            var committer = _Repo.Head.Tip.Committer;
            foreach (var file in mappings.ShasToFiles)
            {
                Commands.Stage(_Repo, file.Value);
                _Repo.Commit($"[Fixup into {file.Key}", author, committer);
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
