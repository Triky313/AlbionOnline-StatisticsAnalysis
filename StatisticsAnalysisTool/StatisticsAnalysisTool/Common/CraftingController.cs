﻿namespace StatisticsAnalysisTool.Common
{
    public static class CraftingController
    {
        // https://docs.google.com/spreadsheets/d/1Vw9ucaSHmuqvQjgfVGYYcyKDv1yW24wyAf0p6iCZJII/edit#gid=303664656

        // (Base Fame * Ressourcen Menge) = Total Base Fame
        // Total Base Fame * Item Menge = Total Book Fame
        // Total Book Fame / Fame Buch Max Menge (zB. T8 19200) = Anzahl benötigtiger Bücher zum Craften
        public static double GetBaseFame(int numberOfMaterials, ItemTier tier, ItemLevel level)
        {
            return (tier, level) switch
            {
                (ItemTier.T2, ItemLevel.Level0) => numberOfMaterials*1.5,
                (ItemTier.T3, ItemLevel.Level0) => numberOfMaterials * 7.5,
                (ItemTier.T4, ItemLevel.Level0) => numberOfMaterials * 22.5,
                (ItemTier.T4, ItemLevel.Level1) => numberOfMaterials * 37.5,
                (ItemTier.T4, ItemLevel.Level2) => numberOfMaterials * 52.5,
                (ItemTier.T4, ItemLevel.Level3) => numberOfMaterials * 67.5,
                (ItemTier.T5, ItemLevel.Level0) => numberOfMaterials * 90,
                (ItemTier.T5, ItemLevel.Level1) => numberOfMaterials * 172.5,
                (ItemTier.T5, ItemLevel.Level2) => numberOfMaterials * 255,
                (ItemTier.T5, ItemLevel.Level3) => numberOfMaterials * 337.5,
                (ItemTier.T6, ItemLevel.Level0) => numberOfMaterials * 270,
                (ItemTier.T6, ItemLevel.Level1) => numberOfMaterials * 532.5,
                (ItemTier.T6, ItemLevel.Level2) => numberOfMaterials * 795,
                (ItemTier.T6, ItemLevel.Level3) => numberOfMaterials * 1057.5,
                (ItemTier.T7, ItemLevel.Level0) => numberOfMaterials * 645,
                (ItemTier.T7, ItemLevel.Level1) => numberOfMaterials * 1282.5,
                (ItemTier.T7, ItemLevel.Level2) => numberOfMaterials * 1920,
                (ItemTier.T7, ItemLevel.Level3) => numberOfMaterials * 2557.5,
                (ItemTier.T8, ItemLevel.Level0) => numberOfMaterials * 1395,
                (ItemTier.T8, ItemLevel.Level1) => numberOfMaterials * 2782.5,
                (ItemTier.T8, ItemLevel.Level2) => numberOfMaterials * 4170,
                (ItemTier.T8, ItemLevel.Level3) => numberOfMaterials * 5557.5,
                _ => 0
            };
        }
    }
}