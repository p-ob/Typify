﻿namespace Typify.NET
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;
    using Microsoft.Extensions.DependencyModel;
    using Typify.NET.Utils;

    public static class Typifier
    {
        public static readonly string DefaultTypeScriptDefinitionFilename = "typified.d.ts";

        public static void Typify(TypifyOptions options)
        {
            options = options ?? new TypifyOptions();
            if (string.IsNullOrEmpty(options.AssemblyFile))
            {
                throw new TypifyInvalidOptionException(nameof(options.AssemblyFile), options.AssemblyFile, "Need assembly file");
            }
            var typesToTypify = GetTypesToTypify(options.AssemblyFile);
            var typeScriptDefinitions = new List<ITypeScriptDefinition>();

            foreach (var type in typesToTypify)
            {
                var definition = GenerateTypeScriptDefinition(type, options);
                if (
                    !typeScriptDefinitions.Any(
                        td => td.Name == definition.Namespace && td.Namespace == definition.Namespace))
                {
                    typeScriptDefinitions.Add(definition);
                }
            }

            var namespacedDefinitions = typeScriptDefinitions.GroupBy(td => td.Namespace);
            WriteDefinitions(namespacedDefinitions, options.Destination);
        }

        private static IEnumerable<Type> GetTypesToTypify(string assemblyFile)
        {
            var typesToTypify = new List<Type>();
            Assembly assembly;

            try
            {
                assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
            }
            catch (FileLoadException)
            {
                // check if the assembly is already loaded before throwing
                var loadedAssemblyNames = DependencyContext.Default.GetDefaultAssemblyNames();
                var typifyOptionAssemblyName = AssemblyLoadContext.GetAssemblyName(assemblyFile);
                var matchingAssemblyName =
                    loadedAssemblyNames.FirstOrDefault(
                        an => string.Equals(an.Name, typifyOptionAssemblyName.Name));
                if (matchingAssemblyName != null)
                {
                    assembly = Assembly.Load(matchingAssemblyName);
                }
                else
                {
                    throw;
                }
            }

            var typeInfos = assembly.GetTypes().Select(t => t.GetTypeInfo());
            foreach (var t in typeInfos)
            {
                var attributes = t.GetCustomAttributes<TypifyAttribute>();
                if (attributes != null && attributes.Any())
                {
                    var attributeTypes = attributes.Select(a => a.Type);
                    typesToTypify.AddRange(attributeTypes.Where(at => !typesToTypify.Contains(at)));
                }
            }

            var propertyTypes = typesToTypify.SelectMany(GetPropertyTypesToTypify).Distinct().ToList();
            typesToTypify.AddRange(propertyTypes.Where(pt => !typesToTypify.Contains(pt)));

            return typesToTypify;
        }

        private static ITypeScriptDefinition GenerateTypeScriptDefinition(Type type, TypifyOptions options)
        {
            ITypeScriptDefinition typeScriptDefinition;
            if (type.GetTypeInfo().IsEnum)
            {
                typeScriptDefinition =
                    Activator.CreateInstance(typeof(TypeScriptEnumDefinition<>).MakeGenericType(type)) as
                        ITypeScriptDefinition;
            }
            else
            {
                if (type.GetTypeInfo().IsGenericType)
                {
                    typeScriptDefinition =
                        Activator.CreateInstance(typeof(TypeScriptGenericInterfaceDefinition<>).MakeGenericType(type), options)
                            as ITypeScriptDefinition;
                }
                else
                {
                    typeScriptDefinition =
                        Activator.CreateInstance(typeof(TypeScriptInterfaceDefinition<>).MakeGenericType(type), options)
                            as ITypeScriptDefinition;
                }
            }

            return typeScriptDefinition;
        }

        private static IEnumerable<Type> GetPropertyTypesToTypify(Type type)
        {
            var propertyTypes =
                type.GetTypeInfo().GetProperties(TypeUtils.PropertyBindingFlags)
                    .Where(
                        p =>
                            !(TypeScriptUtils.DotNetTypeToTypeScriptTypeLookup.Contains(p.PropertyType) ||
                              p.PropertyType.IsSystemType()))
                    .Select(t => t.PropertyType)
                    .ToList();
            var subPropertyTypes = propertyTypes.SelectMany(GetPropertyTypesToTypify).Distinct().ToList();
            propertyTypes.AddRange(subPropertyTypes);
            return propertyTypes;
        }

        private static void WriteDefinitions(IEnumerable<IGrouping<string, ITypeScriptDefinition>> namespacedDefinitions, string destination)
        {
            var typeScriptFileContents = "/*\n\tAutogenerated using Typify. Do not modify.\n*/\n";
            foreach (var namespacedDefinition in namespacedDefinitions)
            {
                var imports = GenerateTypeScriptImports(namespacedDefinition.OfType<TypeScriptInterfaceDefinition>());
                typeScriptFileContents +=
                    $"declare module '{namespacedDefinition.Key}' {{\n{imports}{string.Join("\n", namespacedDefinition.Select(d => d.ToTypeScriptString(1)))}\n}}\n";
            }

            var file = GetDestination(destination);
            var stream = File.Create(file);
            using (var tw = new StreamWriter(stream))
            {
                tw.Write(typeScriptFileContents);
            }
        }

        private static string GetDestination(string destination)
        {
            if (string.IsNullOrEmpty(destination))
            {
                return DefaultTypeScriptDefinitionFilename;
            }
            var extension = Path.GetExtension(destination);
            if (string.Equals(extension, ".ts"))
            {
                return destination;
            }
            if (!string.IsNullOrEmpty(extension))
            {
                throw new TypifyInvalidOptionException(nameof(TypifyOptions.Destination), destination,
                    "Not a valid file path. Needs to be a TypeScript or TypeScript definition file (e.g. *.ts or *.d.ts");
            }

            return $"{(Path.IsPathRooted(destination) ? string.Empty : Directory.GetCurrentDirectory())}/{destination}/{DefaultTypeScriptDefinitionFilename}";
        }

        private static string GenerateTypeScriptImports(IEnumerable<TypeScriptInterfaceDefinition> namespacedDefinitions)
        {
            var importString = string.Empty;
            if (!namespacedDefinitions.Any()) return importString;

            var distinctImports = namespacedDefinitions.SelectMany(d => d.Properties)
                .Where(p => p.IsImport)
                .DistinctBy(p => new { p.Name, p.Namespace });
            var groupedByNamespaceImportedProperties = distinctImports.GroupBy(p => p.Namespace);

            return groupedByNamespaceImportedProperties.Aggregate(importString,
                (current, groupedByNamespaceImport) =>
                    current +
                    $"\timport {{ {string.Join(", ", groupedByNamespaceImport.Select(p => p.Type))} }} from '{groupedByNamespaceImport.Key}';\n");
        }
    }
}
