﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.ComponentDetection.Detectors.Pip;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ComponentDetection.Detectors.Tests
{
    [TestClass]
    [TestCategory("Governance/All")]
    [TestCategory("Governance/ComponentDetection")]
    public class PythonVersionTests
    {
        [TestMethod]
        public void TestBasicVersionConstruction()
        {
            PythonVersion pythonVersion = new PythonVersion("4!3.2.1.1rc2.post99.dev2");

            Assert.AreEqual(pythonVersion.Epoch, 4);
            Assert.AreEqual(pythonVersion.Release, "3.2.1.1");
            Assert.AreEqual(pythonVersion.PreReleaseLabel, "rc");
            Assert.AreEqual(pythonVersion.PostNumber, 99);
            Assert.AreEqual(pythonVersion.DevNumber, 2);

            var newPythonVersion = new PythonVersion("0.3m1");
        }

        [TestMethod]
        public void TestPythonVersionComplexComparisons()
        {
            // This is a list of versions supplied by PEP440 for testing (minus local versions)
            var versions = new List<string>
            {
                "1.0.dev456",
                "1.0a1",
                "1.0a2.dev456",
                "1.0a12.dev456",
                "1.0a12",
                "1.0b1.dev456",
                "1.0b2",
                "1.0b2.post345.dev456",
                "1.0b2.post345",
                "1.0rc1.dev456",
                "1.0rc1",
                "1.0",
                "1.0.post456.dev34",
                "1.0.post456",
                "1.1.dev1",
            }.Select(x => new PythonVersion(x)).ToList();

            for (int i = 1; i < versions.Count; i++)
            {
                Assert.IsTrue(versions[i - 1] < versions[i]);
            }
        }

        [TestMethod]
        public void TestVersionValidForSpec()
        {
            IList<(IList<string>, IList<string>, IList<string>)> testCases = new List<(IList<string>, IList<string>, IList<string>)>
            {
                (new List<string> { "==1.0" }, new List<string> { "1.0" }, new List<string> { "1.0.1", "2.0", "0.1" }),
                (new List<string> { "==1.4.*" }, new List<string> { "1.4", "1.4.1", "1.4.2", "1.4.3" }, new List<string> { "1.0.1", "2.0", "0.1", "1.5", "1.5.0" }),
                (new List<string> { ">=1.0" }, new List<string> { "1.0", "1.1", "1.5" }, new List<string> { "0.9" }),
                (new List<string> { ">=1.0", "<=1.4" }, new List<string> { "1.0", "1.1", "1.4" }, new List<string> { "0.9", "1.5" }),
                (new List<string> { ">1.0", "<1.4" }, new List<string> { "1.1", "1.3" }, new List<string> { "0.9", "1.5", "1.0", "1.4" }),
                (new List<string> { ">1.0", "<1.4", "!=1.2" }, new List<string> { "1.1", "1.3" }, new List<string> { "0.9", "1.5", "1.0", "1.4", "1.2" }),
            };

            foreach (var (specs, validVersions, invalidVersions) in testCases)
            {
                Assert.IsTrue(validVersions.All(x => PythonVersionUtilities.VersionValidForSpec(x, specs)));
                Assert.IsTrue(invalidVersions.All(x => !PythonVersionUtilities.VersionValidForSpec(x, specs)));
            }
        }

        [TestMethod]
        public void TestVersionValidForSpec_VersionIsNotValid_ArgumentExpcetionIsThrown()
        {
            Action action = () => PythonVersionUtilities.VersionValidForSpec("notvalid", new List<string> { "==1.0" });
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void TestVersionValidForSpec_SomeSpecIsNotValid_ArgumentExpcetionIsThrown()
        {
            Action action = () => PythonVersionUtilities.VersionValidForSpec("1.0.0", new List<string> { "==notvalid" });
            action.Should().Throw<ArgumentException>();
        }
    }
}
