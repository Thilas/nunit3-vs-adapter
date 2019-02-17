﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NUnit.VisualStudio.TestAdapter.Tests.Acceptance.WorkspaceTools
{
    [DebuggerDisplay("{Directory,nq}")]
    public sealed partial class IsolatedWorkspace
    {
        private readonly List<string> projectPaths = new List<string>();

        public string Directory { get; }

        public IsolatedWorkspace(string directory)
        {
            Directory = directory;
        }

        public IsolatedWorkspace AddProject(string path, string contents)
        {
            AddFile(path, contents);
            projectPaths.Add(path);
            return this;
        }

        public IsolatedWorkspace AddFile(string path, string contents)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("File path must be specified.", nameof(path));

            if (Path.IsPathRooted(path))
                throw new ArgumentException("File path must not be rooted.", nameof(path));

            File.WriteAllText(Path.Combine(Directory, path), Utils.RemoveIndent(contents));
            return this;
        }

        public void DotNetRestore()
        {
            ConfigureRun("dotnet")
                .Add("restore")
                .Run();
        }

        public void DotNetBuild(bool noRestore = false)
        {
            ConfigureRun("dotnet")
                .Add("build")
                .AddIf(noRestore, "--no-restore")
                .Run();
        }

        public void DotNetTest(bool noBuild = false)
        {
            ConfigureRun("dotnet")
                .Add("test")
                .AddIf(noBuild, "--no-build")
                .Run();
        }

        public void DotNetVSTest(IEnumerable<string> testAssemblyPaths)
        {
            ConfigureRun("dotnet")
                .Add("vstest")
                .AddRange(testAssemblyPaths)
                .Run();
        }

        public void MSBuild(string target = null, bool restore = false)
        {
            ConfigureRun(ToolLocationFacts.MSBuild)
                .AddIf(target != null, "/t:" + target)
                .AddIf(restore, "/restore")
                .Run();
        }

        public void VSTest(IEnumerable<string> testAssemblyPaths)
        {
            ConfigureRun(ToolLocationFacts.VSTest)
                .AddRange(testAssemblyPaths)
                .Run();
        }

        private RunSettings ConfigureRun(string filename) => new RunSettings(Directory, filename);
    }
}
