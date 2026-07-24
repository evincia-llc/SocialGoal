using System;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SocialGoal.Data.Models;

namespace SocialGoal.Tests.Data
{
    /// <summary>
    /// Pins the generated EF model DDL as the baseline schema of record for the
    /// EF Core migration (epic Sprints 6-7). The committed baseline may only
    /// change together with a recorded decision in ai-context/decisions.md.
    /// </summary>
    [TestFixture]
    public class SchemaSnapshotTests
    {
        private static string RepoPath(string relative)
        {
            // Test assembly runs from source\SocialGoal.Tests\bin\Release.
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..", relative));
        }

        private static string GenerateModelDdl()
        {
            using (var context = new SocialGoalEntities())
            {
                var script = ((IObjectContextAdapter)context).ObjectContext.CreateDatabaseScript();
                return script.Replace("\r\n", "\n").TrimEnd() + "\n";
            }
        }

        private static string Sha256Hex(string normalizedContent)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(normalizedContent));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        [Test]
        public void SchemaBaseline_MatchesGeneratedModelDdl()
        {
            var baselinePath = RepoPath(@"docs\schema\schema-baseline.sql");
            var generated = GenerateModelDdl();

            if (!File.Exists(baselinePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(baselinePath));
                File.WriteAllText(baselinePath, generated);
                Assert.Fail("Baseline was missing. Generated docs/schema/schema-baseline.sql from the current model -- review it, commit it, and re-run.");
            }

            var committed = File.ReadAllText(baselinePath).Replace("\r\n", "\n");
            Assert.AreEqual(committed, generated,
                "Generated model DDL diverges from docs/schema/schema-baseline.sql. " +
                "Schema changes require a recorded decision; do not regenerate the baseline casually.");
        }

        [Test]
        public void SchemaBaseline_ChecksumMatches()
        {
            var baselinePath = RepoPath(@"docs\schema\schema-baseline.sql");
            var checksumPath = RepoPath(@"docs\schema\schema-baseline.sha256");
            Assert.IsTrue(File.Exists(baselinePath), "docs/schema/schema-baseline.sql is missing.");
            Assert.IsTrue(File.Exists(checksumPath), "docs/schema/schema-baseline.sha256 is missing.");

            var normalized = File.ReadAllText(baselinePath).Replace("\r\n", "\n");
            var actual = Sha256Hex(normalized);
            var recorded = File.ReadAllText(checksumPath).Trim().Split(' ')[0].ToLowerInvariant();
            Assert.AreEqual(recorded, actual,
                "schema-baseline.sql content does not match its recorded SHA-256 (computed over LF-normalized UTF-8).");
        }
    }
}
