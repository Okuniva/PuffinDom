using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.UI;
using PuffinDom.UI.Asserts;
using PuffinDom.UI.Scroll;
using PuffinDom.UI.Views;

namespace PuffinDom.UnitTests;

[TestFixture]
public class ScrollIndexViewsValidationTests
{
    [Theory]
    public void AllViewsMarkedByScrollOrderAttributeShouldHaveIndexFrom1ToNumberOfViewsOnly(Platform platform)
    {
        UIContext.Platform = platform;

        var viewsWithScrollOrderAttribute = typeof(UIContext).Assembly
            .GetTypes()
            .Where(x => x.GetProperties().Any(p => p.GetCustomAttribute<ScrollOrderAttribute>() != null));

        foreach (var scrollContainerView in viewsWithScrollOrderAttribute)
        {
            var propertiesIndexes = scrollContainerView
                .GetProperties()
                .Where(x => x.GetCustomAttribute<ScrollOrderAttribute>() != null)
                .Select(x => x.GetCustomAttribute<ScrollOrderAttribute>())
                .Select(x => x.Index)
                .ToList();

            var methodsIndexes = scrollContainerView
                .GetMethods()
                .Where(x => x.GetCustomAttribute<ScrollOrderAttribute>() != null)
                .Select(x => x.GetCustomAttribute<ScrollOrderAttribute>())
                .Select(x => x.Index)
                .ToList();

            var indexes = propertiesIndexes.Concat(methodsIndexes).ToList();

            var validationTransformationIndexesList = indexes
                .Distinct()
                .Order()
                .ToList();

            TypedAssert.AreEqual(
                1,
                validationTransformationIndexesList[0],
                $"Wrong ScrollOrderAttribute index somewhere in {scrollContainerView}");

            TypedAssert.AreEqual(
                indexes.Count,
                validationTransformationIndexesList.Last(),
                $"Wrong ScrollOrderAttribute index somewhere in {scrollContainerView}; Check all the properties with {nameof(ScrollOrderAttribute)} are public");
        }
    }

    [Test]
    public void IfAnyViewMarkedByScrollOrderAttributeThenAllItsNeighborViewsShouldAlsoBeMarked()
    {
        UIContext.Platform = Platform.Android;
        var viewsWithScrollOrderAttribute = typeof(UIContext).Assembly
            .GetTypes()
            .Where(x => x.GetProperties().Any(p => p.GetCustomAttribute<ScrollOrderAttribute>() != null));

        var badProperties = new List<PropertyInfo>();
        foreach (var scrollContainerView in viewsWithScrollOrderAttribute)
            badProperties.AddRange(
                scrollContainerView
                    .GetProperties()
                    .Where(x => x.PropertyType.IsAssignableTo(typeof(View))
                                && x.GetCustomAttribute<ScrollOrderAttribute>() == null
                                && x.Name != nameof(View.Parent)
                                && x.DeclaringType == scrollContainerView)
                    .ToList());

        if (badProperties.Any())
            Assert.Fail(
                $"There are unmarked Views among marked by {nameof(ScrollOrderAttribute)}. " +
                $"You should mark them also if you'd like to proceed.\nViews to mark:\n" +
                string.Join("\n", badProperties));
    }
}