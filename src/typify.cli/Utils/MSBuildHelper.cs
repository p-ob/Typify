namespace Typify.NET.Cli.Utils
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Microsoft.Build.Construction;
	using Microsoft.Build.Evaluation;
	using Microsoft.Build.Exceptions;

	internal static class MSBuildHelper
    {
	    private const string MSBuildAssemblyNameProperty = "AssemblyName";

		public static ProjectRootElement LoadProject(string fileOrDirectory)
	    {
			var projects = new ProjectCollection();
		    return File.Exists(fileOrDirectory)
			    ? FromFile(projects, fileOrDirectory)
			    : FromDirectory(projects, fileOrDirectory);
	    }

		public static string GetAssemblyName(ProjectRootElement projectRootElement)
		{
			var globalProperties = new Dictionary<string, string>
			{
				["MSBuildSDKsPath"] = GetMSBuildSDKsPath()
			};
			var project = new ProjectCollection().LoadProject(projectRootElement.FullPath, globalProperties, null);
			return project.GetPropertyValue(MSBuildAssemblyNameProperty);
		}

		private static ProjectRootElement FromFile(ProjectCollection projects, string projectPath)
		{
			if (!File.Exists(projectPath))
			{
				throw new Exception($"Project does not exist: {projectPath}");
			}

			var project = TryOpenProject(projects, projectPath);
			if (project == null)
			{
				throw new Exception($"Project does not exist: {projectPath}");
			}

			return project;
		}

		private static ProjectRootElement FromDirectory(ProjectCollection projects, string projectDirectory)
		{
			var projectFile = GetProjectFileFromDirectory(projectDirectory);

			var project = TryOpenProject(projects, projectFile.FullName);
			return project;
		}

		private static FileInfo GetProjectFileFromDirectory(string projectDirectory)
		{
			var dir = new DirectoryInfo(projectDirectory);

			var files = dir.GetFiles("*proj");

			return files.Single();
		}

		private static ProjectRootElement TryOpenProject(ProjectCollection projects, string filename)
		{
			try
			{
				return ProjectRootElement.Open(filename, projects, preserveFormatting: true);
			}
			catch (InvalidProjectFileException)
			{
				return null;
			}
		}

		private static string GetMSBuildSDKsPath()
		{
			// allow consumers to set use their own SDK, if necessary
			var envMSBuildSDKsPath = Environment.GetEnvironmentVariable(Context.MSBUILD_SDKS_PATH);

			if (envMSBuildSDKsPath != null)
			{
				return envMSBuildSDKsPath;
			}

			return Path.Combine(
				AppContext.BaseDirectory,
				"Sdks");
		}
	}
}
