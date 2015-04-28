﻿namespace APIComparer.Backend.Reporting
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using APIComparer.VersionComparisons;
    using HandlebarsDotNet;

    public class APIUpgradeToHtmlFormatter
    {
        static Action<TextWriter, object> template;

        static APIUpgradeToHtmlFormatter()
        {
            var partials = typeof(Templates_Html)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Select(m => new
                {
                    Name = m.Name.ToLowerInvariant(),
                    Invoke = new Func<string>(() => (string)m.Invoke(null, null))
                }).ToArray();

            foreach (var @partial in partials)
            {
                if (@partial.Name == "comparison")
                {
                    using (var templateReader = new StringReader(@partial.Invoke()))
                    {
                        template = Handlebars.Compile(templateReader);
                        templateReader.Close();
                    }
                    continue;
                }

                using (var templateReader = new StringReader(@partial.Invoke()))
                {
                    var partialTemplate = Handlebars.Compile(templateReader);
                    Handlebars.RegisterTemplate(@partial.Name, partialTemplate);
                }
            }
        }

        public void Render(TextWriter writer, PackageDescription description, DiffedCompareSet[] diffedCompareSets)
        {
            var data = ViewModelBuilder.Build(description, diffedCompareSets);

            template(writer, data);

    public class ViewModelBuilder
    {
        public static object Build(PackageDescription description, DiffedCompareSet[] diffedCompareSets)
        {
            return new
            {
                targets = BuildTargets(description, diffedCompareSets)
            };
        }

        static IEnumerable<object> BuildTargets(PackageDescription description, DiffedCompareSet[] diffedCompareSets)
        {
            return 
                from diffedSet in diffedCompareSets
                let diff = diffedSet.Diff
                let set = diffedSet.Set
                let removedPublicTypes = BuildRemovedPublicTypes(description, diff)
                let typesMadeInternal = BuildTypesMadeInternal(description, diff)
                let obsoletes = BuildTypesObsoleted(description, diff)
                select new
                {
                    set.Name,
                    set.ComparedTo,
                    noLongerSupported = diff is EmptyDiff,
                    hasRemovedPublicTypes = removedPublicTypes.Any(),
                    removedPublicTypes,
                    hasTypesMadeInternal = typesMadeInternal.Any(),
                    typesMadeInternal,
                    hasObsoletes = obsoletes.Any(),
                    obsoletes
                };
        }

        static IEnumerable<object> BuildRemovedPublicTypes(PackageDescription description, Diff diff)
        {
            foreach (TypeDefinition definition in diff.RemovedPublicTypes())
            {
                yield return new
                {
                    name = definition.GetName()
                };
            }
        }

        static IEnumerable<object> BuildTypesMadeInternal(PackageDescription description, Diff diff)
        {
            foreach (TypeDiff typeDiff in diff.TypesChangedToNonPublic())
            {
                yield return new
                {
                    name = typeDiff.RightType.GetName()
                };
            }
        }

        static IEnumerable<object> BuildTypesObsoleted(PackageDescription description, Diff diff)
        {
            foreach (TypeDefinition typeDiff in diff.RightAllTypes.TypeWithObsoletes())
            {
                yield return new
                {
                    name = typeDiff.GetName()
                };
            }
        }
    }
}