using RimWorld;
using UnityEngine;
using Verse;

namespace Materia.UI
{
    public class MateriaTabWindow : MainTabWindow
    {
        private string _selected;

        public override Vector2 RequestedTabSize => new Vector2(700f, 400f);

        public override void DoWindowContents(Rect fillRect)
        {
            if (MateriaMod.Instance == null) { return; }

            Text.Font = GameFont.Small;
            GUI.BeginGroup(fillRect);

            var leftRect = new Rect(0, 0, 250f, fillRect.height);
            var rightRect = new Rect(260f, 0, fillRect.width - 260f, fillRect.height);

            DrawLeftRect(leftRect);
            DrawRightRect(rightRect);

            // fillRect
            GUI.EndGroup();
        }

        private void DrawLeftRect(Rect rect)
        {
            Widgets.DrawMenuSection(rect, false);

            GUI.BeginGroup(rect);

            float y = 0;
            int i = 0;

            foreach (var s in MateriaMod.Instance.GetCurrentOptions())
            {
                var row = new Rect(0f, y, rect.width, 50f);
                Widgets.DrawHighlightIfMouseover(row);

                var labelRec = new Rect(row.x + 10f, row.y, row.width - 5f, row.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRec, s.Label);

                if (_selected == s.Label) { Widgets.DrawHighlightSelected(row); }

                if (i++ % 2 == 1) { Widgets.DrawAltRect(row); }
                y += 50f;

                if (Widgets.ButtonInvisible(row)) { _selected = s.Label; }
            }

            var buttonRect = new Rect(35f, rect.height - 80f, rect.width - 70f, 50f);

            Text.Anchor = TextAnchor.MiddleCenter;
            if (Widgets.ButtonText(buttonRect, "Choose", true, true))
            {
                MateriaMod.Instance.SetChoice(_selected);
            }

            Text.Anchor = TextAnchor.UpperLeft;

            GUI.EndGroup();
        }

        private void DrawRightRect(Rect rect)
        {
            Widgets.DrawMenuSection(rect, false);

            var prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.BeginGroup(rect);

            var textRect = new Rect(10f, 10f, rect.width - 10f, rect.height - 10f);
            Widgets.Label(textRect, "Testing \n Enter!");

            GUI.EndGroup();
            Text.Anchor = prevAnchor;
        }
    }
}
