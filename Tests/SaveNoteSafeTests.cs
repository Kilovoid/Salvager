using Moq;
using Salvager.Services;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tests
{
    public class SaveNoteSafeTests
    {
        private readonly string _testRoot =
            Path.Combine(Path.GetTempPath(), "SalvagerTests");
        private readonly Dictionary<string, string> _files = new();

        private readonly Mock<IFileSystem> _mockFileSystem = new();
        private readonly Mock<ILogger> _mockLogger = new();

        private static readonly DateTime TestTime = new(2026, 12, 12, 12, 12, 12);
        private static readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

        public SaveNoteSafeTests()
        {
            _mockFileSystem.Setup(fs => fs
                .DirectoryExists(It.IsAny<string>())).Returns(true);

            _mockFileSystem.Setup(fs => fs
                .CombinePath(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>(Path.Combine);

            _mockFileSystem.Setup(fs => fs
                .GetInvalidFileNameChars()).Returns(Path.GetInvalidFileNameChars());

            _mockFileSystem.Setup(fs => fs
                .GetFileNameWithoutExtension(It.IsAny<string>())).Returns<string>(Path.GetFileNameWithoutExtension);
        }
    }
}
