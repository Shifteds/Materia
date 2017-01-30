using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Materia.Models;
using RimWorld;
using UnityEngine;
using Verse;
using S = Materia.Settings;

namespace Materia.Gen
{
    internal static class EffectsGen
    {
        public const string BuffName = "MateriaBuff", DebuffName = "MateriaDebuff";

        private static readonly TextInfo TextInfo = new CultureInfo("en-US", false).TextInfo;
        private static readonly Color BuffColor = new Color(0.27f, 0.92f, 0.39f);
        private static readonly Color DebuffColor = new Color(0.27f, 0.92f, 0.39f);

        public static void CreateEmptyHediffs()
        {
            CreateEmptyBuffHediffs();
            CreateEmptyDebuffHediffs();
        }

        public static void PopulateEffects(List<RecipeSpec> recipes)
        {
            var effects = CreateAllStatModifiers()
                .OrderBy(e => Guid.NewGuid())
                .ToList();

            var hediffs = DefDatabase<HediffDef>.AllDefsListForReading
                .Where(h => h.defName.StartsWith("MateriaBuff") || h.defName.StartsWith("MateriaDebuff"))
                .ToList();

            var used = new HashSet<EffectSpec>();
            var usedStats = new HashSet<string>();

            foreach (var recipe in recipes)
            {
                int tier = recipe.Tier;

                // Add buffs.
                for (int i = 0; i < S.BuffsPerTier[tier]; i++)
                {
                    var next = effects
                        .Where(e => e.Tier == recipe.Tier && e.Category == EffectCategory.Buff)
                        .OrderByDescending(e => !used.Contains(e))
                        .ThenByDescending(e => !usedStats.Contains(e.StatName))
                        .First();

                    used.Add(next);
                    usedStats.Add(next.StatName);

                    var hediff = hediffs.First();
                    hediffs.Remove(hediff);
                    hediff.defaultLabelColor = BuffColor;

                    var nextClone = next.Clone();
                    nextClone.HediffName = hediff.defName;

                    recipe.Effects.Add(nextClone);
                }

                // Add debuffs.
                for (int i = 0; i < S.DebuffsPerTier[tier]; i++)
                {
                    var next = effects
                        .Where(e => e.Tier == recipe.Tier && e.Category == EffectCategory.Debuff)
                        .OrderByDescending(e => !used.Contains(e))
                        .ThenByDescending(e => !usedStats.Contains(e.StatName))
                        .First();

                    used.Add(next);
                    usedStats.Add(next.StatName);

                    var hediff = hediffs.First();
                    hediffs.Remove(hediff);
                    hediff.defaultLabelColor = DebuffColor;

                    var nextClone = next.Clone();
                    nextClone.HediffName = hediff.defName;

                    recipe.Effects.Add(nextClone);
                }
            }
        }

        private static void CreateEmptyBuffHediffs()
        {
            int amount = 0;
            int recipesPerTier = S.RecipeAmount / S.TierAmount;
            for (int tier = 0; tier < S.TierAmount; tier++) { amount += S.BuffsPerTier[tier] * recipesPerTier; }

            for (int i = 0; i < amount; i++)
            {
                var hediff = new HediffDef
                {
                    defName = $"{BuffName}{i}",
                    label = string.Empty,
                    description = string.Empty,
                    maxSeverity = 1.0f,
                    scenarioCanAdd = false
                };

                DefDatabase<HediffDef>.Add(hediff);
            }
        }

        private static void CreateEmptyDebuffHediffs()
        {
            int amount = 0;
            int recipesPerTier = S.RecipeAmount / S.TierAmount;
            for (int tier = 0; tier < S.TierAmount; tier++) { amount += S.DebuffsPerTier[tier] * recipesPerTier; }

            for (int i = 0; i < amount; i++)
            {
                var hediff = new HediffDef
                {
                    defName = $"{DebuffName}{i}",
                    label = string.Empty,
                    description = string.Empty,
                    maxSeverity = 1.0f,
                    scenarioCanAdd = false
                };

                DefDatabase<HediffDef>.Add(hediff);
            }
        }

        private static IEnumerable<EffectSpec> CreateAllStatModifiers()
        {
            var statDefs = DefDatabase<StatDef>.AllDefsListForReading
                .Where(s => s.category.defName == "BasicsPawn")
                .ToList();

            var specs = new List<EffectSpec>();

            foreach (var statDef in statDefs)
            {
                for (int tier = 0; tier < S.TierAmount; tier++)
                {
                    string label = TextInfo.ToTitleCase(statDef.label);
                    float buffValue = S.BuffPotencyPerTier[tier];

                    var buff = new EffectSpec
                    {
                        Tier = tier,
                        StatName = statDef.defName,
                        Type = EffectType.Stat,
                        Category = EffectCategory.Buff,
                        Value = buffValue,
                        Label = label,
                        Description = $"{label}: +{buffValue * 100:00000}%"
                    };

                    float debuffValue = S.DebuffPotencyPerTier[tier];

                    var debuff = new EffectSpec
                    {
                        Tier = tier,
                        StatName = statDef.defName,
                        Type = EffectType.Stat,
                        Category = EffectCategory.Debuff,
                        Value = debuffValue,
                        Label = label,
                        Description = $"{label}: -{debuffValue * 100:00000}%"
                    };

                    specs.Add(buff);
                    specs.Add(debuff);
                }
            }

            return specs;
        }
    }
}
