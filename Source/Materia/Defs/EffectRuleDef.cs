using System.Collections.Generic;
using Verse;

namespace Materia.Defs
{
    public class EffectRuleDef : Def
    {
        public List<string> ExcludeStats = new List<string>();
        public List<string> ExcludeCaps = new List<string>();

        public string Stat;
        public bool InvertEffect;
        public List<float> BuffPotencyPerTier = new List<float>();
        public List<float> DebuffPotencyPerTier = new List<float>();
    }
}
