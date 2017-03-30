namespace Typify.NET.Tools
{
    using System.IO;
    using Microsoft.Build.Evaluation;

    internal static class MsBuildHelper
    {
        private const string MsBuildOutputPathProperty = "OutputPath";
        private const string MsBuildOutputDirProperty = "OutDir";
        private const string MsBuildAssemblyNameProperty = "AssemblyName";
        private const string MsBuildTargetFrameworkProperty = "TargetFramework";
        private const string MsBuildProjectDirectoryProperty = "MSBuildProjectDirectory";
        private const string MsBuildProjectNameProperty = "MSBuildProjectName";

        public static string GetAssemblyForProject(string projectPath)
        {
            var projectCollection = new ProjectCollection();
            var project = new Project(projectPath, null, null, projectCollection, ProjectLoadSettings.IgnoreMissingImports);
            project.ReevaluateIfNecessary();
            var outputPath = project.GetProperty(MsBuildOutputPathProperty);
            var outDir = project.GetProperty(MsBuildOutputDirProperty);
            var projectDirectory = project.GetProperty(MsBuildProjectDirectoryProperty);
            var projectName = project.GetProperty(MsBuildProjectNameProperty);
            var assemblyName = project.GetProperty(MsBuildAssemblyNameProperty);
            var targetFramework = project.GetProperty(MsBuildTargetFrameworkProperty);
            var projectOrAssemblyName = assemblyName?.EvaluatedValue ?? projectName.EvaluatedValue;
            return outputPath?.EvaluatedValue ??
                   outDir?.EvaluatedValue ??
                   GetOutputDll(projectDirectory.EvaluatedValue, targetFramework.EvaluatedValue, projectOrAssemblyName);
        }

        // this is a temporary hack until we can get around MSBuild and Microsoft.Net.Sdk magic
        private static string GetOutputDll(string projectFolder, string targetFramework, string projectOrAssemblyName)
        {
            return Path.Combine(projectFolder, "bin/Debug/", targetFramework, $"{projectOrAssemblyName}.dll");
        }
    }
}
