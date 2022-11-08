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

        private static readonly Vector2 PawnPortraitSize = new Vector2(x: 100f, y: 140f);

        private Pawn curPawn;

        public override string PageTitle => "CreateCharacters".Translate();

        private static List<Pawn> StartingPawns => Find.GameInitData.startingAndOptionalPawns;

        public override void PreOpen()
        {
            base.PreOpen();
            if (Find.GameInitData.startingPawnCount > 0)
            {
                curPawn = Find.GameInitData.startingAndOptionalPawns[index: 0];
            }
        }

        public override void DoWindowContents(Rect rect)
        {
            DrawPageTitle(rect: rect);
            var mainRect = GetMainRect(rect: rect, extraTopSpace: 30f);
            Widgets.DrawMenuSection(rect: mainRect);
            TabDrawer.DrawTabs(baseRect: mainRect, tabs: (from c in Find.GameInitData.startingAndOptionalPawns
                select new TabRecord(label: c.LabelCap, clickedAction: delegate { SelectPawn(c: c); }, selected: c == curPawn)).ToList());
            var rect2 = mainRect.ContractedBy(margin: 17f);
            var rect3 = rect2;
            rect3.width = 100f;
            GUI.DrawTexture(
                position: new Rect(x: rect3.xMin + ((rect3.width - PawnPortraitSize.x) / 2f) - 10f, y: rect3.yMin + 20f,
                    width: PawnPortraitSize.x, height: PawnPortraitSize.y), image: PortraitsCache.Get(pawn: curPawn, size: PawnPortraitSize, rotation: Rot4.South));
            var rect4 = rect2;
            rect4.xMin = rect3.xMax;
            var rect5 = rect4;
            rect5.width = 475f;
            CharacterCardUtility.DrawCharacterCard(rect: rect5, pawn: curPawn, randomizeCallback: RandomizeCurPawn);
            var rect6 = new Rect(x: rect5.xMax + 5f, y: rect4.y + 100f, width: rect4.width - rect5.width - 5f, height: 200f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect: rect6, label: "Health".Translate());
            Text.Font = GameFont.Small;
            rect6.yMin += 35f;
            HealthCardUtility.DrawHediffListing(rect: rect6, pawn: curPawn, showBloodLoss: true);
            var rect7 = new Rect(x: rect6.x, y: rect6.yMax, width: rect6.width, height: 200f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect: rect7, label: "Relations".Translate());
            Text.Font = GameFont.Small;
            rect7.yMin += 35f;
            SocialCardUtility.DrawRelationsAndOpinions(rect: rect7, selPawnForSocialInfo: curPawn);
            DoBottomButtons(rect: rect, nextLabel: "Next".Translate());
        }

        private void RandomizeCurPawn()
        {
            if (!TutorSystem.AllowAction(ep: "RandomizePawn"))
            {
                return;
            }

            var num = 0;
            while (true)
            {
                curPawn = RandomizeInPlace(p: curPawn);
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
            TutorSystem.Notify_Event(ep: "RandomizePawn");
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

                Messages.Message(text: "EveryoneNeedsValidName".Translate(), def: MessageTypeDefOf.RejectInput);
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
            var index = StartingPawns.IndexOf(item: p);
            var pawn = RegenerateStartingPawnInPlace(index: index);
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.ManualDumb) ||
                pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Violent))
            {
                pawn = RegenerateStartingPawnInPlace(index: index);
            }

            return pawn;
        }

        private static Pawn RegenerateStartingPawnInPlace(int index)
        {
            var pawn = StartingPawns[index: index];
            PawnUtility.TryDestroyStartingColonistFamily(pawn: pawn);
            pawn.relations.ClearAllRelations();
            Find.WorldPawns.PassToWorld(pawn: pawn, discardMode: PawnDiscardDecideMode.Discard);
            StartingPawns[index: index] = null;
            for (var i = 0; i < Find.GameInitData.startingPawnCount; i++)
            {
                if (StartingPawns[index: i] != null)
                {
                    PawnUtility.TryDestroyStartingColonistFamily(pawn: Find.GameInitData.startingAndOptionalPawns[index: i]);
                }
            }

            var pawn2 = ScenPart_ConfigPage_ConfigureStartingSlaves.NewGeneratedStartingSlave();
            Find.GameInitData.startingAndOptionalPawns[index: index] = pawn2;
            return pawn2;
        }
    }
}