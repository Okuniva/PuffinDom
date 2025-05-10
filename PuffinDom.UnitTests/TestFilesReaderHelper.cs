using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework.Legacy;
using PuffinDom.Infrastructure;

namespace PuffinDom.UnitTests;

public static class TestFilesReaderHelper
{
    public static IEnumerable<Tuple<string, string>> GetAllFilesContent(string directoryPath)
    {
        var combinedPath = Path.Combine(
            CoreConstants.GetBaseProjectPath(),
            directoryPath);

        ClassicAssert.True(Directory.Exists(combinedPath));

        var files = Directory.GetFiles(combinedPath, "*.cs", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var className = Path.GetFileNameWithoutExtension(file);
            yield return new Tuple<string, string>(content, className);
        }
    }
}