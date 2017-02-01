using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Materia.Defs;
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

        private static readonly TextInfo _textInfo = new CultureInfo("en-US", false).TextInfo;
        public static readonly Color BuffColor = new Color(0.27f, 0.92f, 0.39f);
        public static readonly Color DebuffColor = new Color(0.90f, 0.23f, 0.29f);
        private static readonly HashSet<string> _includedStatCategories = new HashSet<string>
        {
            "PawnSocial", "BasicsPawn", "PawnWork", "PawnCombat"
        };

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
            var rules = DefDatabase<EffectRuleDef>.AllDefsListForReading
                .ToList();

            var excludedStats = rules.SelectMany(r => r.ExcludeStats).ToHashSet();
            var excludedCaps = rules.SelectMany(r => r.ExcludeCaps).ToHashSet();
            var individualRules = rules.Where(r => r.Stat != null).ToDictionary(r => r.Stat, r => r);

            var statDefs = DefDatabase<StatDef>.AllDefsListForReading
                .Where(s => _includedStatCategories.Contains(s.category.defName))
                .Where(s => !excludedStats.Contains(s.defName))
                .ToList();

            var specs = new List<EffectSpec>();

            foreach (var statDef in statDefs)
            {
                for (int tier = 0; tier < S.TierAmount; tier++)
                {
                    individualRules.TryGetValue(statDef.defName, out EffectRuleDef rule);

                    string label = _textInfo.ToTitleCase(statDef.label);

                    float buffValue = S.BuffPotencyPerTier[tier];
                    if (rule?.InvertEffect ?? false) { buffValue = buffValue * -1; }

                    if (rule != null && rule.BuffPotencyPerTier?.Count > 0)
                    {
                        buffValue = rule.BuffPotencyPerTier[tier];
                    }

                    string description = buffValue > 0
                        ? $"{label}: +{(int) (buffValue * 100)}%"
                        : $"{label}: {(int) (buffValue * 100)}%";

                    var buff = new EffectSpec
                    {
                        Tier = tier,
                        StatName = statDef.defName,
                        Type = EffectType.Stat,
                        Category = EffectCategory.Buff,
                        Value = buffValue,
                        Label = label,
                        Description = description
                    };

                    float debuffValue = S.DebuffPotencyPerTier[tier];
                    if (rule?.InvertEffect ?? false) { debuffValue = debuffValue * -1; }

                    if (rule != null && rule.DebuffPotencyPerTier?.Count > 0)
                    {
                        debuffValue = rule.DebuffPotencyPerTier[tier];
                    }

                    description = debuffValue > 0
                        ? $"{label}: +{(int)(debuffValue * 100)}%"
                        : $"{label}: {(int)(debuffValue * 100)}%";

                    var debuff = new EffectSpec
                    {
                        Tier = tier,
                        StatName = statDef.defName,
                        Type = EffectType.Stat,
                        Category = EffectCategory.Debuff,
                        Value = debuffValue,
                        Label = label,
                        Description = description
                    };

                    specs.Add(buff);
                    specs.Add(debuff);
                }
            }

            return specs;
        }
    }
}
