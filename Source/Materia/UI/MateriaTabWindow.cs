using System.Collections.Generic;
using Materia.Models;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Materia.UI
{
    public class MateriaTabWindow : MainTabWindow
    {
        private string _selected;
        private static readonly Color _titleColor = new Color(0.49f, 0.89f, 0.49f);

        public override Vector2 RequestedTabSize => new Vector2(700f, 700f);

        public override void DoWindowContents(Rect fillRect)
        {
            if (MateriaMod.Instance == null) { return; }

            Text.Font = GameFont.Small;
            GUI.BeginGroup(fillRect);

            var leftRect = new Rect(0, 0, 250f, fillRect.height);
            var rightRect = new Rect(260f, 0, fillRect.width - 260f, fillRect.height);

            var current = MateriaMod.Instance.GetCurrent();
            if (current == null)
            {
                DrawLeftRectAsOptions(leftRect, MateriaMod.Instance.GetCurrentOptions());
                if (_selected != null) { DrawRightRect(rightRect); }
            }
            else
            {
                DrawLeftRectAsProgress(leftRect, current);
                _selected = current.Label;
                DrawRightRect(rightRect);
            }

            // fillRect
            GUI.EndGroup();
        }

        private void DrawLeftRectAsOptions(Rect rect, IEnumerable<RecipeSpec> options)
        {
            Widgets.DrawMenuSection(rect, false);

            GUI.BeginGroup(rect);

            float y = 0;
            int i = 0;

            foreach (var s in options)
            {
                var row = new Rect(0f, y, rect.width, 50f);
                Widgets.DrawHighlightIfMouseover(row);

                var labelRec = new Rect(row.x + 10f, row.y, row.width - 10f, row.height);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(labelRec, s.ProductLabel);

                if (_selected == s.Label) { Widgets.DrawHighlightSelected(row); }

                if (i++ % 2 == 1) { Widgets.DrawAltRect(row); }
                y += 50f;

                if (Widgets.ButtonInvisible(row))
                {
                    _selected = s.Label;
                    SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
                }
            }

            var buttonRect = new Rect(35f, rect.height - 80f, rect.width - 70f, 50f);

            if (Widgets.ButtonText(buttonRect, "Choose"))
            {
                MateriaMod.Instance.SetChoice(_selected);
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

            Text.Anchor = TextAnchor.UpperLeft;

            GUI.EndGroup();
        }

        private static void DrawLeftRectAsProgress(Rect rect, RecipeSpec current)
        {
            Widgets.DrawMenuSection(rect, false);

            GUI.BeginGroup(rect);

            var progressRect = new Rect(35f, rect.height - 80f, rect.width - 70f, 50f);
            float percent = (current.Progress / current.MaxProgress);

            var nameRect = new Rect(15f, 15, rect.width - 20f, 100f);
            Text.Anchor = TextAnchor.UpperCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(nameRect, current.ProductLabel);

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;
            Widgets.FillableBar(progressRect, percent);
            Widgets.Label(progressRect, $"{current.Progress} / {current.MaxProgress}");

            Text.Anchor = TextAnchor.UpperLeft;

            GUI.EndGroup();
        }

        private void DrawRightRect(Rect rect)
        {
            Widgets.DrawMenuSection(rect, false);

            var spec = MateriaMod.Instance.GetByLabel(_selected);
            if (spec == null) { return; }

            var prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.BeginGroup(rect);

            var textRect = new Rect(10f, 10f, rect.width - 10f, 45f);
            Widgets.Label(textRect, spec.Description);

            var ingTitle = new Rect(10f, 50f, rect.width - 10f, 25f);
            GUI.color = _titleColor;
            Widgets.Label(ingTitle, "Ingredients");

            GUI.color = Color.white;
            var ingRect = new Rect(10f, 80f, rect.width - 10f, 125f);
            Widgets.Label(ingRect, spec.GetIngredientText());

            var statTitle = new Rect(10f, 210f, rect.width - 10f, 25f);
            GUI.color = _titleColor;
            Widgets.Label(statTitle, "Stats");

            GUI.color = Color.white;
            var statsRect = new Rect(10f, 240f, rect.width - 10f, 200f);
            Widgets.Label(statsRect, spec.GetStatsText());

            var effectsTitle = new Rect(10f, 445f, rect.width - 10f, 25f);
            GUI.color = _titleColor;
            Widgets.Label(effectsTitle, "Effects");

            GUI.color = Color.white;
            for (int i = 0; i < spec.Hediffs.Count; i++)
            {
                var h = spec.Hediffs[i];
                var r = new Rect(10f, 475f + i * 25f, rect.width - 10f, 25f);
                Widgets.Label(r, h.GetDescription());
            }

            GUI.EndGroup();
            Text.Anchor = prevAnchor;
        }
    }
}
