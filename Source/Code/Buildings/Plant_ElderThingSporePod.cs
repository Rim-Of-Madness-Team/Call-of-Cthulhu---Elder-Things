using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.AI;

namespace ElderThingFaction
{
    public class Plant_ElderThingSporePod : Plant
    {
        private Pawn parent = null;
        private bool skinColorCheck = false;
        private Color skinColor = Color.white;

        public Pawn Parent
        {
            get => parent;
            set => parent = value;
        }

        public override Color DrawColor
        {
            get
            {
                if (!skinColorCheck)
                {
                    skinColorCheck = true;
                    if (Parent != null && Parent.story.SkinColor is Color tempSkin)
                    {
                        skinColor = tempSkin;
                    }
                }
                return skinColor;
            }
        }

        public override Graphic Graphic
        {
            get => base.Graphic.GetColoredVersion(base.Graphic.Shader, skinColor, skinColor);
        }

        public override void TickLong()
        {
            base.TickLong();
            if (Growth >= 0.99f)
            {
                var newElderThing = PawnGenerator.GeneratePawn(
                    new PawnGenerationRequest(
                        kind: PawnKindDef.Named("ElderThing_Kind"),
                        faction: parent.Faction,
                        colonistRelationChanceFactor: 0f,
                        canGeneratePawnRelations: false,
                        allowDowned: true,
                        forcedXenotype: XenotypeDefOf.Baseliner,
                        forcedEndogenes: new List<GeneDef>
                        {
                            DefDatabase<GeneDef>.GetNamed("")
                        },
                        developmentalStages: DevelopmentalStage.Newborn)
                    );
                GenSpawn.Spawn(newElderThing, this.Position, this.Map, WipeMode.Vanish);
                if (newElderThing != null && parent != null)
                    newElderThing.relations.AddDirectRelation(PawnRelationDefOf.ParentBirth, parent);
                if (this.Destroyed != true)
                    this.Destroy();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(refee: ref parent, label: "parent", false);
        }
    }
}