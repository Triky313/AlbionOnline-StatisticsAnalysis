using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Crafting;
using StatisticsAnalysisTool.Models.ItemsJsonModel;

namespace StatisticsAnalysisTool.UnitTests.Crafting;

[TestFixture]
public class CraftingRecipeResolverTests
{
    [Test]
    public void GetReturnPolicy_WithoutMaximumReturnAmount_IsReturnableWithoutLimit()
    {
        var resources = new[]
        {
            new CraftResource
            {
                UniqueName = "RESOURCE_WITH_RETURN",
                Count = 10
            }
        };

        var result = CraftingRecipeResolver.GetReturnPolicy(resources);

        result.IsReturnable.Should().BeTrue();
        result.MaxReturnQuantityPerRun.Should().BeNull();
    }

    [Test]
    public void GetReturnPolicy_WithZeroMaximumReturnAmount_IsNotReturnable()
    {
        var resources = new[]
        {
            new CraftResource
            {
                UniqueName = "RESOURCE_WITHOUT_RETURN",
                Count = 1,
                MaxReturnAmount = "0"
            }
        };

        var result = CraftingRecipeResolver.GetReturnPolicy(resources);

        result.IsReturnable.Should().BeFalse();
        result.MaxReturnQuantityPerRun.Should().BeNull();
    }

    [Test]
    public void GetReturnPolicy_WithPositiveMaximumReturnAmounts_AggregatesLimit()
    {
        var resources = new[]
        {
            new CraftResource
            {
                UniqueName = "LIMITED_RESOURCE",
                Count = 3,
                MaxReturnAmount = "1.5"
            },
            new CraftResource
            {
                UniqueName = "LIMITED_RESOURCE",
                Count = 4,
                MaxReturnAmount = "2.5"
            }
        };

        var result = CraftingRecipeResolver.GetReturnPolicy(resources);

        result.IsReturnable.Should().BeTrue();
        result.MaxReturnQuantityPerRun.Should().Be(4m);
    }

    [Test]
    public void Calculate_WithReturnableAlchemyResource_CalculatesExpectedReturn()
    {
        var resources = new[]
        {
            new CraftResource
            {
                UniqueName = "T1_ALCHEMY_EXTRACT_LEVEL1",
                Count = 10
            }
        };

        var returnPolicy = CraftingRecipeResolver.GetReturnPolicy(resources);
        var calculator = new CraftingCalculator();
        var input = new CraftingCalculationInput
        {
            CraftingRuns = 2,
            ReturnRatePercent = 25m,
            Resources =
            [
                new CraftingResourceInput
                {
                    UniqueName = "T1_ALCHEMY_EXTRACT_LEVEL1",
                    QuantityPerRun = 10m,
                    ResourceKind = CraftingResourceKind.Alchemy,
                    IsReturnable = returnPolicy.IsReturnable,
                    MaxReturnQuantityPerRun = returnPolicy.MaxReturnQuantityPerRun
                }
            ]
        };

        var result = calculator.Calculate(input);

        result.Resources[0].ExpectedReturnQuantity.Should().Be(5m);
    }
}
