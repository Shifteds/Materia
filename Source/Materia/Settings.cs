namespace Materia
{
    internal static class Settings
    {
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

        // Number of recipes per tier.
        public static int[] RecipeAmount =
        {
            9, 9, 9
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
