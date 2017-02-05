using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fixup;
using LibGit2Sharp;
using NUnit.Framework;
using Diff = Fixup.Diff;

namespace FixupTests
{
    public class When_creating_files : BaseRepoTest
    {
        protected override void SetUp()
        {
            base.SetUp();
            CreateFile("A");
            CreateFile("B");
            CreateFile("C");
            CreateFile("D");
        }

        [Test]
        public void they_should_be_found_in_the_untracked_list()
        {
            Assert.That(_Repository.RetrieveStatus().Untracked.Count(), Is.EqualTo(4));
        }
    }

    public class When_creating_files_and_staging_them : BaseRepoTest
    {
        protected override void SetUp()
        {
            base.SetUp();
            CreateFile("A");
            CreateFile("B");
            CreateFile("C");
            CreateFile("D");

            Commands.Stage(_Repository, new []{"A", "B", "C", "D"});
        }

        [Test]
        public void they_should_be_found_in_the_untracked_list()
        {
            Assert.That(_Repository.RetrieveStatus().Added.Count(), Is.EqualTo(4));
        }
    }

    public class When_finding_modified_files_to_fixup : MultipleCommitRepoTest
    {
        private FixupRepo _Fixup;
        private DictionaryList<string, Diff> _ShasToFiles;

        protected override void SetUp()
        {
            base.SetUp();
            ChangeFile("A");
            ChangeFile("C");

            _Fixup = new FixupRepo(_WorkingDirectoryPath);

            _ShasToFiles = _Fixup.GetMappings(_Repository.Commits.QueryBy(new CommitFilter { SortBy = CommitSortStrategies.Topological }).Last().Id).FileToDiffs;
        }

        [Test]
        public void file_A_should_be_found_in_the_second_commit()
        {
            Assert.That(_ShasToFiles["A"].Count(x => x.Commit.Message.Contains("Changed A")), Is.EqualTo(1));
        }

        [Test]
        public void file_C_should_be_found_in_the_second_commit()
        {
            Assert.That(_ShasToFiles["C"].Count(x => x.Commit.Message.Contains("Changed C")), Is.EqualTo(1));
        }
    }

    public class When_grouping_files_together_to_fixup : MultipleCommitRepoTest
    {
        private FixupRepo _Fixup;
        private DictionaryList<string, Diff> _ShasToFiles;
        private ShaAndFileMappings Result;

        protected override void SetUp()
        {
            base.SetUp();
            ChangeFile("A");
            ChangeFile("B");
            ChangeFile("C");
            ChangeFile("D");

            _Fixup = new FixupRepo(_WorkingDirectoryPath);

            var lastCommit = _Repository.Commits.QueryBy(new CommitFilter { SortBy = CommitSortStrategies.Topological }).Last().Id;
            Result = _Fixup.GetMappings(lastCommit);
        }

        [Test]
        public void A_and_B_should_be_together()
        {
            var commitAB = _Repository.Commits.Single(x => x.MessageShort == "Changed A and B");

            Assert.That(Result.ShasToFiles[commitAB.Sha].Count(x => x == "A"), Is.EqualTo(1));
            Assert.That(Result.ShasToFiles[commitAB.Sha].Count(x => x == "B"), Is.EqualTo(1));
        }

        [Test]
        public void C_and_D_should_be_together()
        {
            var commitCD = _Repository.Commits.Single(x => x.MessageShort == "Changed C and D");

            Assert.That(Result.ShasToFiles[commitCD.Sha].Count(x => x == "C"), Is.EqualTo(1));
            Assert.That(Result.ShasToFiles[commitCD.Sha].Count(x => x == "D"), Is.EqualTo(1));
        }
    }
}
