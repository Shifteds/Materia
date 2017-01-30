namespace Materia
{
    internal static class Settings
    {
        public static int TierAmount = 3;
        public static int RecipeAmount = 27;

        public static int[] BuffsPerTier =
        {
            1, 2, 3
        };

        public static int[] DebuffsPerTier =
        {
            1, 1, 1
        };

        public static float[] BuffPotencyPerTier =
        {
            0.15f, 0.25f, 0.35f
        };

        public static float[] DebuffPotencyPerTier =
        {
            -0.10f, -0.15f, -0.20f
        };

        public static int[] ProgressPerTurn =
        {
            50,
            1000,
            1400,
            1600,
            7400,
            8000,
            8300,
            30000,
            50000
        };

        public static int[] ProgressGainPerTier =
        {
            20, 
            80,
            160
        };

        // Minimum per-ingredient amount needed per tier.
        public static int[] MinIngredientAmounts =
        {
            8, 8, 8
        };

        // Maximum per-ingredient amount needed per tier.
        public static int[] MaxIngredientAmounts =
        {
            13, 13, 13
        };

        // Number of unique ingredients per tier.
        public static int[] IngredientCount =
        {
            2, 4, 5
        };

        public static int[] DaysToRotMin =
        {
            2, 2, 2
        };

        public static int[] DaysToRotMax =
        {
            8, 12, 18
        };

        public static int[] MarketValueMin =
        {
            2, 5, 10
        };

        public static int[] MarketValueMax =
        {
            30, 30, 60
        };

        public static int[] YieldMin =
        {
            1, 1, 2
        };

        public static int[] YieldMax =
        {
            2, 4, 6
        };

        public static int[] WorkToMakeMin =
        {
            300, 400, 600
        };

        public static int[] WorkToMakeMax =
        {
            400, 500, 700
        };

        public static int[] CookingSkillMin =
        {
            0, 6, 13
        };

        public static int[] CookingSkillMax =
        {
            3, 10, 20
        };
    }
}
