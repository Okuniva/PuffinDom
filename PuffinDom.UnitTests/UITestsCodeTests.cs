using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using PuffinDom.Tools;
using PuffinDom.UI;
using PuffinDom.UI.Exceptions;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace PuffinDom.UnitTests;

[TestFixture]
public class UITestsCodeTests
{
    [Test]
    public void ThereAreNoStaticViewsPropertiesDefinedWithinAnotherView()
    {
        var badProperties = new List<PropertyInfo>();

        var views = Assembly.GetAssembly(typeof(ViewExtensions))!
            .GetTypes()
            .Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(View)));

        foreach (var type in views)
            badProperties.AddRange(
                type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                    .Where(info => info.PropertyType.IsAssignableFrom(typeof(View)))
                    .ToList());

        var result = badProperties.Aggregate(
            string.Empty,
            (current, propertyInfo) =>
                current
                + "Class: "
                + propertyInfo.DeclaringType!.Name
                + "; Property: "
                + propertyInfo.Name
                + Environment.NewLine);

        if (!string.IsNullOrEmpty(result))
            throw new FailTestException(result);
    }

    [Test]
    public void PropertiesInPagesListDoNotHavePageWordInTitle()
    {
        var badNaming = typeof(UIContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => property.Name.EndsWith("Page"))
            .ToList();

        if (badNaming.Any())
            throw new FailTestException(
                badNaming.Aggregate(
                    string.Empty,
                    (current, property) =>
                        $"{current}Rename property '{property.Name}' " +
                        $"to '{property.Name[..^4]}'{Environment.NewLine}"));
    }

    [Test]
    public void CheckIfConstructorsFromViewClassesDoNotContainCallerMemberNameAttribute()
    {
        var badTypesList = new List<string>();

        var viewTypes = Assembly.GetAssembly(typeof(ViewExtensions))!
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.IsSubclassOf(typeof(View)))
            .ToList();

        ClassicAssert.Greater(viewTypes.Count, 5, "There are not enough view classes");

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var viewType in viewTypes)
            // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var constructorInfo in viewType.GetConstructors())
        {
            var parameters = constructorInfo.GetParameters();

            var isViewWithParent = parameters.Any(x => x.Name == "parent");

            if (!isViewWithParent)
                continue;

            var needsParameter = parameters
                .FirstOrDefault(x =>
                    x.Name == "viewName"
                    && x.Attributes.HasFlag(ParameterAttributes.Optional)
                    && x.CustomAttributes.Any(a => a.AttributeType == typeof(CallerMemberNameAttribute)));

            if (needsParameter == null)
                badTypesList.Add(viewType.Name);
        }

        if (badTypesList.Count > 0)
            throw new FailTestException(
                $"{nameof(CallerMemberNameAttribute)} is absence in next classes: " +
                badTypesList.Aggregate((i, j) => i + "," + j).ToString());
    }

    [Test]
    public void UseUITestsIgnoreExceptionInsteadOfAssertIgnoreToIgnore()
    {
        foreach (var (content, fileName) in TestFilesReaderHelper.GetAllFilesContent("PuffinDom.UI.Tests"))
        {
            const string assertIgnore = "Assert.Ignore";
            if (content.Contains(assertIgnore))
                throw new FailTestException(
                    $"{fileName} contains {assertIgnore} but shouldn't. " +
                    "Please use UITestsIgnoreException instead");
        }
    }

    [Test]
    public void UseUITestsExceptionInsteadOfAssertFailToFailTest()
    {
        foreach (var (content, fileName) in TestFilesReaderHelper.GetAllFilesContent("PuffinDom.UI.Tests"))
        {
            const string assertFail = "Assert.Fail";
            if (content.Contains(assertFail))
                throw new FailTestException(
                    $"{fileName} contains {assertFail} but shouldn't. " +
                    "Please use UITestsException instead");
        }
    }

    [Test]
    public void NoThreadSleepDirectUsagesToHaveThemAllLogged()
    {
        foreach (var (content, fileName) in TestFilesReaderHelper.GetAllFilesContent(""))
        {
            if (fileName is nameof(ThreadSleep) or nameof(UITestsCodeTests))
                continue;

            const string threadSleep = "Thread.Sleep(";
            if (content.Contains(threadSleep))
                throw new FailTestException(
                    $"{fileName} contains {threadSleep} but shouldn't. " +
                    $"Please use {nameof(ThreadSleep)}.{nameof(ThreadSleep.For)} instead");
        }
    }

    [Test]
    public void UseSimpleDevicePropertyInTestsInsteadOfDeviceManagerInstance()
    {
        foreach (var (testContent, fileName) in TestFilesReaderHelper.GetAllFilesContent(
                     Path.Combine("PuffinDom.UI.Tests", "Tests")))
        {
            const string deviceManagerInstance = "DeviceManager.Instance";
            if (testContent.Contains(deviceManagerInstance))
                throw new FailTestException(
                    $"{fileName} contains {deviceManagerInstance} but shouldn't. " +
                    "Please use 'Device' instead as simple and shorter version");
        }
    }

    [Test]
    public void UseSimpleDevicePropertyInViewsInsteadOfDeviceManagerInstance()
    {
        foreach (var (testContent, fileName) in TestFilesReaderHelper.GetAllFilesContent(
                     Path.Combine("PuffinDom.UI.Tests", "AppModel", "Views")))
        {
            const string deviceManagerInstance = "DeviceManager.Instance";
            if (testContent.Contains(deviceManagerInstance))
                throw new FailTestException(
                    $"{fileName} contains {deviceManagerInstance} but shouldn't. " +
                    "Please use 'Device' instead as simple and shorter version");
        }
    }

    [Test]
    public void DoNotUseIgnoreAttributeButUseRunAsIgnoreEnumValueInUITestsAttribute()
    {
        foreach (var (testContent, fileName) in TestFilesReaderHelper.GetAllFilesContent("PuffinDom.UI.Tests"))
        {
            const string ignoreAttribute = "[Ignore";
            if (testContent.Contains(ignoreAttribute))
                throw new FailTestException(
                    $"{fileName} contains {ignoreAttribute} but shouldn't. " +
                    "Please use RunAsIgnore enum value instead");
        }
    }

    [Test]
    public void DoNotUseDynamicEver()
    {
        foreach (var (testContent, fileName) in TestFilesReaderHelper.GetAllFilesContent(""))
        {
            const string ignoreAttribute = "dynamic ";
            if (fileName != nameof(UITestsCodeTests) && testContent.Contains(ignoreAttribute))
                throw new FailTestException(
                    $"{fileName} contains {ignoreAttribute} but shouldn't. " +
                    "Please use RunAsIgnore enum value instead");
        }
    }
}