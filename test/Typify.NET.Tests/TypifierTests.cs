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
            var thisLibraryType = typeof(Library.Entity).GetTypeInfo();
            AssemblyFile = thisLibraryType.Assembly.Location;
        }

        [Fact]
        public void Typify_DefaultFile_Test()
        {
            var typifyOptions = new TypifyOptions
            {
                AssemblyFile = AssemblyFile
            };

            Typifier.Typify(typifyOptions);

            Assert.True(File.Exists(Typifier.DefaultTypeScriptDefinitionFilename), "Did not generate expected file.");
        }

        [Fact]
        public void Typify_DestinationFolder_Test()
        {
            const string destination = "TestDestination/";
            var typifyOptions = new TypifyOptions
            {
                AssemblyFile = AssemblyFile,
                Destination = destination
            };

            Typifier.Typify(typifyOptions);

            Assert.True(File.Exists($"{destination}{Typifier.DefaultTypeScriptDefinitionFilename}"),
                "Did not generate expected file.");
        }

        [Fact]
        public void Typify_DestinationFile_Test()
        {
            const string destination = "TestDestination/test.ts";
            var typifyOptions = new TypifyOptions
            {
                AssemblyFile = AssemblyFile,
                Destination = destination
            };

            Typifier.Typify(typifyOptions);

            Assert.True(File.Exists(destination), "Did not generate expected file.");
        }

        [Fact]
        public void Typify_InvalidFileType_ThrowsTypifyInvalidOptionException_Test()
        {
            const string destination = "TestDestination/notatypescriptfile.cs";
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
        public void Typify_InvalidTargetTsVersion_ThrowsTypifyInvalidOptionException_Test()
        {
            var typifyOptions = new TypifyOptions
            {
                AssemblyFile = AssemblyFile,
                TargetTypeScriptVersion = "notaversionnumber"
            };

            try
            {
                Typifier.Typify(typifyOptions);
                Assert.False(true, $"Did not throw {nameof(TypifyInvalidOptionException)}");
            }
            catch (TypifyInvalidOptionException e)
            {
                Assert.True(string.Equals(e.OptionName, nameof(TypifyOptions.TargetTypeScriptVersion)),
                    $"Threw unexpected {nameof(TypifyInvalidOptionException)}");
            }
        }

        [Fact]
        public void Typify_UnsupportedTsVersion_ThrowsTypifyInvalidOptionException_Test()
        {
            var typifyOptions = new TypifyOptions
            {
                AssemblyFile = AssemblyFile,
                TargetTypeScriptVersion = "1.8"
            };

            try
            {
                Typifier.Typify(typifyOptions);
                Assert.False(true, $"Did not throw {nameof(TypifyInvalidOptionException)}");
            }
            catch (TypifyInvalidOptionException e)
            {
                Assert.True(string.Equals(e.OptionName, nameof(TypifyOptions.TargetTypeScriptVersion)),
                    $"Threw unexpected {nameof(TypifyInvalidOptionException)}");
            }
        }

        [Fact]
        public void Typify_AssemblyFileDoesntExist_ThrowsTypifyInvalidOptionException_Test()
        {
            var typifyOptions = new TypifyOptions
            {
                AssemblyFile = "somefake.dll"
            };

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
