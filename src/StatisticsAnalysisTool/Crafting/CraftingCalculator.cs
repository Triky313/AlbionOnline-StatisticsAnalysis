using System;
using System.Linq;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingCalculator
{
    public CraftingCalculationResult Calculate(CraftingCalculationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var craftingRuns = Math.Max(0, input.CraftingRuns);
        var amountCrafted = Math.Max(1, input.AmountCrafted);
        var outputQuantity = craftingRuns * amountCrafted;
        var returnRate = Math.Clamp(input.ReturnRatePercent, 0m, 100m) / 100m;

        var result = new CraftingCalculationResult
        {
            CraftingRuns = craftingRuns,
            AmountCrafted = amountCrafted,
            OutputQuantity = outputQuantity,
            ReturnRatePercent = Math.Clamp(input.ReturnRatePercent, 0m, 100m),
            UsesFocus = input.UsesFocus,
            NutritionConsumed = RoundQuantity(Math.Max(0m, input.NutritionConsumedPerRun) * craftingRuns),
            StationFee = CalculateStationFee(input.StationFee, input.NutritionConsumedPerRun, craftingRuns),
            OtherCosts = RoundSilver(input.OtherCosts)
        }
        ;

        foreach (var resource in input.Resources ?? [])
        {
            if (resource == null)
            {
                continue;
            }

            var grossQuantity = resource.QuantityPerRun * craftingRuns;
            var expectedReturnQuantity = GetExpectedReturnQuantity(resource, grossQuantity, craftingRuns, returnRate);
            var netQuantity = Math.Max(0m, grossQuantity - expectedReturnQuantity);

            var resourceResult = new CraftingResourceResult
            {
                UniqueName = resource.UniqueName,
                ResourceKind = resource.ResourceKind,
                IsReturnable = resource.IsReturnable,
                QuantityPerRun = resource.QuantityPerRun,
                GrossQuantity = RoundQuantity(grossQuantity),
                ExpectedReturnQuantity = RoundQuantity(expectedReturnQuantity),
                NetQuantity = RoundQuantity(netQuantity),
                UnitPrice = RoundSilver(resource.UnitPrice),
                GrossCost = RoundSilver(grossQuantity * resource.UnitPrice),
                NetCost = RoundSilver(netQuantity * resource.UnitPrice),
                GrossWeight = (double) grossQuantity * resource.UnitWeight,
                ExpectedReturnWeight = (double) expectedReturnQuantity * resource.UnitWeight
            }
            ;

            result.Resources.Add(resourceResult);
        }

        result.GrossMaterialCosts = RoundSilver(result.Resources.Sum(x => x.GrossCost));
        result.NetMaterialCosts = RoundSilver(result.Resources.Sum(x => x.NetCost));
        result.NonReturnableMaterialCosts = RoundSilver(result.Resources.Where(x => !x.IsReturnable).Sum(x => x.NetCost));
        result.SalesRevenueGross = RoundSilver(outputQuantity * input.OutputUnitPrice);
        result.SalesTax = RoundSilver(result.SalesRevenueGross * Math.Clamp(input.SalesTaxPercent, 0m, 100m) / 100m);
        result.SetupFee = RoundSilver(result.SalesRevenueGross * Math.Clamp(input.SetupFeePercent, 0m, 100m) / 100m);
        result.SalesRevenueNet = RoundSilver(result.SalesRevenueGross - result.SalesTax - result.SetupFee);
        result.Journal = CalculateJournal(input.Journal, craftingRuns);
        result.JournalCosts = result.Journal?.TotalEmptyJournalCosts ?? 0m;
        result.JournalRevenue = result.Journal?.TotalFullJournalRevenue ?? 0m;
        result.TotalCosts = RoundSilver(result.NetMaterialCosts + result.StationFee + result.JournalCosts + result.OtherCosts);
        result.Profit = RoundSilver(result.SalesRevenueNet + result.JournalRevenue - result.TotalCosts);
        result.RoiPercent = result.TotalCosts > 0m
            ? RoundPercent(result.Profit / result.TotalCosts * 100m)
            : 0m;
        result.BreakEvenPrice = CalculateBreakEvenPrice(result, input.SalesTaxPercent, input.SetupFeePercent);
        result.ProfitPerItem = outputQuantity > 0
            ? RoundSilver(result.Profit / outputQuantity)
            : 0m;
        result.WeightBeforeCrafting = result.Resources.Sum(x => x.GrossWeight) + (result.Journal?.TotalJournalWeight ?? 0d);
        result.WeightAfterCrafting = (outputQuantity * input.OutputUnitWeight)
                                     + result.Resources.Sum(x => x.ExpectedReturnWeight)
                                     + (result.Journal?.TotalJournalWeight ?? 0d);

        return result;
    }

    private static decimal CalculateStationFee(decimal usageFeePerHundredNutrition, decimal nutritionConsumedPerRun, int craftingRuns)
    {
        if (usageFeePerHundredNutrition <= 0m || nutritionConsumedPerRun <= 0m || craftingRuns <= 0)
        {
            return 0m;
        }

        return RoundSilver(usageFeePerHundredNutrition * nutritionConsumedPerRun * craftingRuns / 100m);
    }

    private static decimal GetExpectedReturnQuantity(CraftingResourceInput resource, decimal grossQuantity, int craftingRuns, decimal returnRate)
    {
        if (!resource.IsReturnable || grossQuantity <= 0m || returnRate <= 0m)
        {
            return 0m;
        }

        var expectedReturnQuantity = grossQuantity * returnRate;
        if (resource.MaxReturnQuantityPerRun is not > 0m)
        {
            return expectedReturnQuantity;
        }

        var maxReturnQuantity = resource.MaxReturnQuantityPerRun.Value * craftingRuns;
        return Math.Min(expectedReturnQuantity, maxReturnQuantity);
    }

    private static CraftingJournalResult CalculateJournal(CraftingJournalInput journal, int craftingRuns)
    {
        if (journal is not { MaxFamePerJournal: > 0m } || journal.FamePerRun <= 0m || craftingRuns <= 0)
        {
            return null;
        }

        var totalFame = journal.FamePerRun * craftingRuns;
        var requiredEmptyJournals = (int) Math.Ceiling(totalFame / journal.MaxFamePerJournal);
        var expectedFullJournals = (int) Math.Floor(totalFame / journal.MaxFamePerJournal);
        var partialJournalFame = totalFame % journal.MaxFamePerJournal;

        return new CraftingJournalResult
        {
            EmptyJournalUniqueName = journal.EmptyJournalUniqueName,
            FullJournalUniqueName = journal.FullJournalUniqueName,
            TotalFame = RoundQuantity(totalFame),
            MaxFamePerJournal = RoundQuantity(journal.MaxFamePerJournal),
            RequiredEmptyJournals = requiredEmptyJournals,
            ExpectedFullJournals = expectedFullJournals,
            PartialJournalFame = RoundQuantity(partialJournalFame),
            PartialJournalPercent = RoundPercent(partialJournalFame / journal.MaxFamePerJournal * 100m),
            EmptyJournalPrice = RoundSilver(journal.EmptyJournalPrice),
            FullJournalPrice = RoundSilver(journal.FullJournalPrice),
            TotalEmptyJournalCosts = RoundSilver(requiredEmptyJournals * journal.EmptyJournalPrice),
            TotalFullJournalRevenue = RoundSilver(expectedFullJournals * journal.FullJournalPrice),
            TotalJournalWeight = requiredEmptyJournals * journal.UnitWeight
        };
    }

    private static decimal CalculateBreakEvenPrice(CraftingCalculationResult result, decimal salesTaxPercent, decimal setupFeePercent)
    {
        if (result.OutputQuantity <= 0)
        {
            return 0m;
        }

        var taxMultiplier = 1m - (Math.Clamp(salesTaxPercent, 0m, 100m) + Math.Clamp(setupFeePercent, 0m, 100m)) / 100m;
        if (taxMultiplier <= 0m)
        {
            return 0m;
        }

        var requiredRevenue = result.TotalCosts - result.JournalRevenue;
        return RoundSilver(requiredRevenue / result.OutputQuantity / taxMultiplier);
    }

    private static decimal RoundSilver(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal RoundQuantity(decimal value)
    {
        return Math.Round(value, 4, MidpointRounding.AwayFromZero);
    }

    private static decimal RoundPercent(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
