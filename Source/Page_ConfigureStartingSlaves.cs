using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ElderThingFaction
{
    public class Page_ConfigureStartingSlaves : Page
    {
        private const float TabAreaHeight = 30f;

        private const float RectAreaWidth = 100f;

        private const float RightRectLeftPadding = 5f;

        private static readonly Vector2 PawnPortraitSize = new Vector2(100f, 140f);

        private Pawn curPawn;

        public override string PageTitle => "CreateCharacters".Translate();

        private static List<Pawn> StartingPawns => Find.GameInitData.startingAndOptionalPawns;

        public override void PreOpen()
        {
            base.PreOpen();
            if (Find.GameInitData.startingPawnCount > 0)
            {
                curPawn = Find.GameInitData.startingAndOptionalPawns[0];
            }
        }

        public override void DoWindowContents(Rect rect)
        {
            DrawPageTitle(rect);
            var mainRect = GetMainRect(rect, 30f);
            Widgets.DrawMenuSection(mainRect);
            TabDrawer.DrawTabs(mainRect, (from c in Find.GameInitData.startingAndOptionalPawns
                select new TabRecord(c.LabelCap, delegate { SelectPawn(c); }, c == curPawn)).ToList());
            var rect2 = mainRect.ContractedBy(17f);
            var rect3 = rect2;
            rect3.width = 100f;
            GUI.DrawTexture(
                new Rect(rect3.xMin + ((rect3.width - PawnPortraitSize.x) / 2f) - 10f, rect3.yMin + 20f,
                    PawnPortraitSize.x, PawnPortraitSize.y), PortraitsCache.Get(curPawn, PawnPortraitSize, Rot4.South));
            var rect4 = rect2;
            rect4.xMin = rect3.xMax;
            var rect5 = rect4;
            rect5.width = 475f;
            CharacterCardUtility.DrawCharacterCard(rect5, curPawn, RandomizeCurPawn);
            var rect6 = new Rect(rect5.xMax + 5f, rect4.y + 100f, rect4.width - rect5.width - 5f, 200f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect6, "Health".Translate());
            Text.Font = GameFont.Small;
            rect6.yMin += 35f;
            HealthCardUtility.DrawHediffListing(rect6, curPawn, true);
            var rect7 = new Rect(rect6.x, rect6.yMax, rect6.width, 200f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect7, "Relations".Translate());
            Text.Font = GameFont.Small;
            rect7.yMin += 35f;
            SocialCardUtility.DrawRelationsAndOpinions(rect7, curPawn);
            DoBottomButtons(rect, "Next".Translate());
        }

        private void RandomizeCurPawn()
        {
            if (!TutorSystem.AllowAction("RandomizePawn"))
            {
                return;
            }

            var num = 0;
            while (true)
            {
                curPawn = RandomizeInPlace(curPawn);
                num++;
                if (num > 15)
                {
                    break;
                }

                if (StartingPawnUtility.WorkTypeRequirementsSatisfied())
                {
                    goto Block_3;
                }
            }

            return;
            Block_3:
            TutorSystem.Notify_Event("RandomizePawn");
        }

        protected override bool CanDoNext()
        {
            if (!base.CanDoNext())
            {
                return false;
            }

            foreach (var current in Find.GameInitData.startingAndOptionalPawns)
            {
                if (current.Name.IsValid)
                {
                    continue;
                }

                Messages.Message("EveryoneNeedsValidName".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            PortraitsCache.Clear();
            return true;
        }

        public void SelectPawn(Pawn c)
        {
            if (c != null && c != curPawn)
            {
                curPawn = c;
            }
        }

        public static Pawn RandomizeInPlace(Pawn p)
        {
            var index = StartingPawns.IndexOf(p);
            var pawn = RegenerateStartingPawnInPlace(index);
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.ManualDumb) ||
                pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent))
            {
                pawn = RegenerateStartingPawnInPlace(index);
            }

            return pawn;
        }

        private static Pawn RegenerateStartingPawnInPlace(int index)
        {
            var pawn = StartingPawns[index];
            PawnUtility.TryDestroyStartingColonistFamily(pawn);
            pawn.relations.ClearAllRelations();
            Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            StartingPawns[index] = null;
            for (var i = 0; i < Find.GameInitData.startingPawnCount; i++)
            {
                if (StartingPawns[i] != null)
                {
                    PawnUtility.TryDestroyStartingColonistFamily(Find.GameInitData.startingAndOptionalPawns[i]);
                }
            }

            var pawn2 = ScenPart_ConfigPage_ConfigureStartingSlaves.NewGeneratedStartingSlave();
            Find.GameInitData.startingAndOptionalPawns[index] = pawn2;
            return pawn2;
        }
    }
}