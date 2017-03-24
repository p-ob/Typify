namespace Typify.NET.Tests
{
    using System.IO;
    using System.Reflection;
    using Xunit;

    public class TypifierTests
    {
        private static readonly string AssemblyFile;

        static TypifierTests()
        {
            var thisType = typeof(Typify.NET.Tests.Library.Entity).GetTypeInfo();
            AssemblyFile = thisType.Assembly.Location;
        }

        [Fact]
        public void Typify_GenerateDefaultFile_Test()
        {
            var typifyOptions = new TypifyOptions
            {
                AssemblyFile = AssemblyFile
            };

            Typifier.Typify(typifyOptions);

            Assert.True(File.Exists(Typifier.DefaultTypeScriptDefinitionFilename), "Did not generate expected file.");
        }

        [Fact]
        public void Typify_GenerateFileDestination_Test()
        {
            var destination = "TestDestination/";
            var typifyOptions = new TypifyOptions
            {
                AssemblyFile = AssemblyFile,
                Destination = destination
            };

            Typifier.Typify(typifyOptions);

            Assert.True(File.Exists($"{destination}{Typifier.DefaultTypeScriptDefinitionFilename}"), "Did not generate expected file.");
        }

        [Fact]
        public void Typify_InvalidFileType_ThrowsTypifyInvalidOptionException_Test()
        {
            var destination = "TestDestination/notatypescriptfile.cs";
            var typifyOptions = new TypifyOptions
            {
                AssemblyFile = AssemblyFile,
                Destination = destination
            };

            try
            {
                Typifier.Typify(typifyOptions);
                Assert.False(true, $"Did not throw {nameof(TypifyInvalidOptionException)}");
            }
            catch (TypifyInvalidOptionException e)
            {
                Assert.True(string.Equals(e.OptionName, nameof(TypifyOptions.Destination)),
                    $"Threw unexpected {nameof(TypifyInvalidOptionException)}");
            }
        }

        [Fact]
        public void Typify_NoAssemblyFile_ThrowsTypifyInvalidOptionException_Test()
        {
            var typifyOptions = new TypifyOptions();

            try
            {
                Typifier.Typify(typifyOptions);
                Assert.False(true, $"Did not throw {nameof(TypifyInvalidOptionException)}");
            }
            catch (TypifyInvalidOptionException e)
            {
                Assert.True(string.Equals(e.OptionName, nameof(TypifyOptions.AssemblyFile)),
                    $"Threw unexpected {nameof(TypifyInvalidOptionException)}");
            }
        }
    }
}
