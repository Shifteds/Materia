namespace Materia
{
    internal static class Settings
    {
        public static int[] ProgressPerTurn =
        {
            100,
            200,
            300,
            500,
            600,
            700,
            1000,
            1100,
            1200,
            1300,
            1600,
            1700,
            2400,
            2500,
            2600
        };

        // Minimum per-ingredient amount needed per tier.
        public static int[] MinIngredientAmounts =
        {
            8, 8, 8, 8
        };

        // Maximum per-ingredient amount needed per tier.
        public static int[] MaxIngredientAmounts =
        {
            12, 12, 12, 12
        };

        // Number of unique ingredients per tier.
        public static int[] IngredientCount =
        {
            1, 2, 4, 6
        };

        // Number of recipes per tier.
        public static int[] RecipeAmount =
        {
            3, 9, 9, 9
        };
    }
}
