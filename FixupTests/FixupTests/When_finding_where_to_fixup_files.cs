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
        private Dictionary<string, List<Diff>> _ShasToFiles;

        protected override void SetUp()
        {
            base.SetUp();
            ChangeFile("A");
            ChangeFile("C");

            _Fixup = new FixupRepo(_WorkingDirectoryPath);

            _ShasToFiles = _Fixup.GetShasToFiles(_Repository.Commits.QueryBy(new CommitFilter { SortBy = CommitSortStrategies.Topological }).Last().Id);
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
}
