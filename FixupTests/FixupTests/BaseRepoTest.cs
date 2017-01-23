using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using NUnit.Framework;

namespace FixupTests
{
    [TestFixture]
    public class BaseRepoTest
    {
        protected readonly Signature _DefaultSignature = new Signature("testuser", "test@test.com", DateTimeOffset.UtcNow);
        protected Repository _Repository;
        protected string _WorkingDirectoryPath;

        [SetUp]
        protected virtual void SetUp()
        {
            _WorkingDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N").Substring(0, 8));
            Repository.Init(_WorkingDirectoryPath);
            _Repository = new Repository(_WorkingDirectoryPath);
        }

        protected void CreateFile(string name)
        {
            var filePath = GetFullPath(name);
            using (var stream = new StreamWriter(filePath))
            {
                stream.Write(Guid.NewGuid());
            }
        }

        protected void ChangeFile(string name)
        {
            var filePath = GetFullPath(name);
            if (!File.Exists(filePath))
                throw new InvalidOperationException($"File must exist to change it {filePath}");

            using (var stream = new StreamWriter(filePath))
            {
                stream.Write(Guid.NewGuid());
            }
        }

        protected string GetFullPath(string name)
        {
            return Path.Combine(_WorkingDirectoryPath, name);
        }

        [TearDown]
        public void TearDown()
        {
            _Repository.Dispose();
            DirectoryHelper.DeleteDirectory(_WorkingDirectoryPath);
        }
    }

    public class CommitRepoTest : BaseRepoTest
    {
        protected override void SetUp()
        {
            base.SetUp();
            CreateFile("A");
            CreateFile("B");
            CreateFile("C");
            CreateFile("D");
            Commands.Stage(_Repository, new []{ "A", "B", "C", "D" });
            _Repository.Commit("Initial commit", _DefaultSignature, _DefaultSignature);

            ChangeFile("A");
        }

        [Test]
        public void there_should_be_one_commit()
        {
            Assert.That(_Repository.Commits.Count(), Is.EqualTo(1));
        }

        [Test]
        public void there_should_be_one_modified_file()
        {
            Assert.That(_Repository.RetrieveStatus().Modified.First().State, Is.EqualTo(FileStatus.ModifiedInWorkdir));
        }
    }

    public class MultipleCommitRepoTest : BaseRepoTest
    {
        protected override void SetUp()
        {
            base.SetUp();
            CreateFile("A");
            CreateFile("B");
            CreateFile("C");
            CreateFile("D");

            Commands.Stage(_Repository, new[] { "A", "B", "C", "D" });
            _Repository.Commit("Initial commit", _DefaultSignature, _DefaultSignature);

            ChangeFile("A");
            ChangeFile("B");
            
            Commands.Stage(_Repository, new[] { "A", "B" });
            _Repository.Commit("Changed A and B", _DefaultSignature, _DefaultSignature);

            ChangeFile("C");
            ChangeFile("D");

            Commands.Stage(_Repository, new[] { "C", "D" });
            _Repository.Commit("Changed C and D", _DefaultSignature, _DefaultSignature);

        }

        [Test]
        public void there_should_be_three_commits()
        {
            Assert.That(_Repository.Commits.Count(), Is.EqualTo(3));
        }
    }
}