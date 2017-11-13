﻿using System;
using System.IO;
using System.Threading.Tasks;
using FastRsync.Diagnostics;
using FastRsync.Hash;
using FastRsync.Signature;
using NSubstitute;
using NUnit.Framework;

namespace FastRsync.Tests
{
    [TestFixture]
    public class SignatureBuilderTests
    {
        private const int RandomSeed = 123;

        private readonly byte[] xxhash1037TestSignature = {
            0x4F, 0x43, 0x54, 0x4F, 0x53, 0x49, 0x47, 0x01, 0x05, 0x58, 0x58, 0x48, 0x36, 0x34, 0x07, 0x41, 0x64, 0x6C, 0x65, 0x72, 0x33, 0x32, 0x3E, 0x3E, 0x3E, 0x0D, 0x04, 0x2F, 0xFC, 0xF4, 0x6C, 0x7B, 0x52, 0x06, 0x17, 0x0A, 0x90, 0x3D, 0x70
        };

        [Test]
        public void SignatureBuilderXXHash_BuildsSignature()
        {
            // Arrange
            const int dataLength = 1037;
            var data = new byte[dataLength];
            new Random(RandomSeed).NextBytes(data);
            var dataStream = new MemoryStream(data);
            var signatureStream = new MemoryStream();

            var progressReporter = Substitute.For<IProgress<ProgressReport>>();

            // Act
            var target = new SignatureBuilder
            {
                ProgressReport = progressReporter
            };
            target.Build(dataStream, new SignatureWriter(signatureStream));

            // Assert
            CollectionAssert.AreEqual(xxhash1037TestSignature, signatureStream.ToArray());

            signatureStream.Seek(0, SeekOrigin.Begin);
            var sig = new SignatureReader(signatureStream, null).ReadSignature();
            Assert.AreEqual(new XxHashAlgorithm().Name, sig.HashAlgorithm.Name);
            Assert.AreEqual(new XxHashAlgorithm().HashLength, sig.HashAlgorithm.HashLength);
            Assert.AreEqual(new Adler32RollingChecksum().Name, sig.RollingChecksumAlgorithm.Name);

            progressReporter.Received().Report(Arg.Any<ProgressReport>());
        }

        [Test]
        public async Task SignatureBuilderAsyncXXHash_BuildsSignature()
        {
            // Arrange
            const int dataLength = 1037;
            var data = new byte[dataLength];
            new Random(RandomSeed).NextBytes(data);
            var dataStream = new MemoryStream(data);
            var signatureStream = new MemoryStream();

            var progressReporter = Substitute.For<IProgress<ProgressReport>>();

            // Act
            var target = new SignatureBuilder
            {
                ProgressReport = progressReporter
            };
            await target.BuildAsync(dataStream, new SignatureWriter(signatureStream)).ConfigureAwait(false);

            // Assert
            CollectionAssert.AreEqual(xxhash1037TestSignature, signatureStream.ToArray());

            signatureStream.Seek(0, SeekOrigin.Begin);
            var sig = new SignatureReader(signatureStream, null).ReadSignature();
            Assert.AreEqual(new XxHashAlgorithm().Name, sig.HashAlgorithm.Name);
            Assert.AreEqual(new XxHashAlgorithm().HashLength, sig.HashAlgorithm.HashLength);
            Assert.AreEqual(new Adler32RollingChecksum().Name, sig.RollingChecksumAlgorithm.Name);

            progressReporter.Received().Report(Arg.Any<ProgressReport>());
        }
    }
}