﻿namespace Typify.NET
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;
    using Typify.NET.Utils;

    public static class Typifier
    {
        public static void Typify(TypifyOptions options)
        {
            options = options ?? new TypifyOptions();
            if (string.IsNullOrEmpty(options.AssemblyFile))
            {
                throw new Exception("Need assembly file");
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
                var assemblyName = AssemblyLoadContext.GetAssemblyName(assemblyFile);
                var entryAssembly = Assembly.GetEntryAssembly();
                var matchingAssembly =
                    entryAssembly.GetReferencedAssemblies()
                        .FirstOrDefault(a => string.Equals(a.ToString(), assemblyName.ToString()));
                if (matchingAssembly != null)
                {
                    assembly = Assembly.Load(matchingAssembly);
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

            var file =
                $"{destination}{(destination.EndsWith("/") || destination == string.Empty ? string.Empty : "/")}typified.d.ts";
            var stream = File.Create(file);
            using (var tw = new StreamWriter(stream))
            {
                tw.Write(typeScriptFileContents);
            }
        }

        private static string GenerateTypeScriptImports(IEnumerable<TypeScriptInterfaceDefinition> namespacedDefinitions)
        {
            var importString = string.Empty;
            if (!namespacedDefinitions.Any()) return importString;

            var distinctImports = namespacedDefinitions.SelectMany(d => d.Properties)
                .Where(p => p.IsImport)
                .DistinctBy(p => new { p.Name, p.Namespace });
            var groupedByNamespaceImportedProperties = distinctImports.GroupBy(p => p.Namespace);
            foreach (var groupedByNamespaceImport in groupedByNamespaceImportedProperties)
            {
                importString +=
                    $"\timport {{ {string.Join(", ", groupedByNamespaceImport.Select(p => p.Type))} }} from '{groupedByNamespaceImport.Key}';\n";
            }

            return importString;
        }
    }
}
