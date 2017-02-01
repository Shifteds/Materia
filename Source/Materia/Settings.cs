namespace Materia
{
    internal static class Settings
    {
        public static int TierAmount = 4;
        public static int RecipeAmount = 20;

        public static int[] BuffsPerTier =
        {
            1, 1, 1, 1
        };

        public static int[] DebuffsPerTier =
        {
            1, 1, 1, 1
        };

        public static float[] BuffPotencyPerTier =
        {
            0.15f, 0.20f, 0.25f, 0.30f
        };

        public static float[] DebuffPotencyPerTier =
        {
            -0.10f, -0.13f, -0.15f, -0.15f
        };

        public static int[] ProgressPerTurn =
        {
            120000,
            900000,
            1800000,
            2700000
        };

        public static int[] ProgressGainPerTier =
        {
            20, 
            80,
            160,
            160
        };

        // Minimum per-ingredient amount needed per tier.
        public static int[] MinIngredientAmounts =
        {
            8, 8, 8, 8
        };

        // Maximum per-ingredient amount needed per tier.
        public static int[] MaxIngredientAmounts =
        {
            13, 13, 13, 13
        };

        // Number of unique ingredients per tier.
        public static int[] IngredientCount =
        {
            1, 2, 3, 4
        };

        public static int[] DaysToRotMin =
        {
            2, 2, 2, 2
        };

        public static int[] DaysToRotMax =
        {
            8, 12, 18, 18
        };

        public static int[] MarketValueMin =
        {
            2, 5, 10, 12
        };

        public static int[] MarketValueMax =
        {
            30, 30, 40, 50
        };

        public static int[] YieldMin =
        {
            1, 1, 2, 2
        };

        public static int[] YieldMax =
        {
            2, 4, 6, 6
        };

        public static int[] WorkToMakeMin =
        {
            300, 400, 600, 700
        };

        public static int[] WorkToMakeMax =
        {
            400, 500, 700, 800
        };

        public static int[] CookingSkillMin =
        {
            0, 6, 13, 16
        };

        public static int[] CookingSkillMax =
        {
            3, 10, 17, 20
        };
    }
}
