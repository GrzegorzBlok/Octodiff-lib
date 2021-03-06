﻿using System;
using System.IO;
using FastRsync.Delta;
using FastRsync.Diagnostics;
using OctodiffAsync.CommandLine.Support;

namespace OctodiffAsync.CommandLine
{
    [Command("patch", Description = "Given a basis file, and a delta, produces the new file", Usage = "<basis-file> <delta-file> <new-file>")]
    class PatchCommand : ICommand
    {
        private readonly OptionSet options;
        private IProgress<ProgressReport> progressReport;
        private string basisFilePath;
        private string deltaFilePath;
        private string newFilePath;
        private bool skipHashCheck;

        public PatchCommand()
        {
            options = new OptionSet();
            options.Positional("basis-file", "The file that the delta was created for.", v => basisFilePath = v);
            options.Positional("delta-file", "The delta to apply to the basis file", v => deltaFilePath = v);
            options.Positional("new-file", "The file to write the result to.", v => newFilePath = v);
            options.Add("progress", "Whether progress should be written to stdout", v => progressReport = new ConsoleProgressReporter());
            options.Add("skip-verification", "Skip checking whether the basis file is the same as the file used to produce the signature that created the delta.", v => skipHashCheck = true);
        }

        public void GetHelp(TextWriter writer)
        {
            options.WriteOptionDescriptions(writer);
        }

        public int Execute(string[] commandLineArguments)
        {
            options.Parse(commandLineArguments);

            if (string.IsNullOrWhiteSpace(basisFilePath))
                throw new OptionException("No basis file was specified", "basis-file");
            if (string.IsNullOrWhiteSpace(deltaFilePath))
                throw new OptionException("No delta file was specified", "delta-file");
            if (string.IsNullOrWhiteSpace(newFilePath))
                throw new OptionException("No new file was specified", "new-file");

            basisFilePath = Path.GetFullPath(basisFilePath);
            deltaFilePath = Path.GetFullPath(deltaFilePath);
            newFilePath = Path.GetFullPath(newFilePath);

            if (!File.Exists(basisFilePath)) throw new FileNotFoundException("File not found: " + basisFilePath, basisFilePath);
            if (!File.Exists(deltaFilePath)) throw new FileNotFoundException("File not found: " + deltaFilePath, deltaFilePath);

            var directory = Path.GetDirectoryName(newFilePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var delta = new DeltaApplier
            {
                SkipHashCheck = skipHashCheck
            };

            using (var basisStream = new FileStream(basisFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var deltaStream = new FileStream(deltaFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var newFileStream = new FileStream(newFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                delta.ApplyAsync(basisStream, new BinaryDeltaReader(deltaStream, progressReport), newFileStream).GetAwaiter().GetResult();
            }

            return 0;
        }
    }
}
