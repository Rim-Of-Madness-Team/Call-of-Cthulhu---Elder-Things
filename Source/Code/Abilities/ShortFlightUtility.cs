using RimWorld;
using Verse;
using Verse.AI;

namespace ElderThingFaction;
// Token: 0x0200188A RID: 6282
	public static class ShortFlightUtility
	{
		// Token: 0x0600997E RID: 39294 RVA: 0x0037A0B0 File Offset: 0x003782B0
		public static bool DoShortFlight(Pawn pawn, LocalTargetInfo currentTarget, CompReloadable comp, VerbProperties verbProps)
		{
			if (comp != null && !comp.CanBeUsed)
			{
				return false;
			}
			if (comp != null)
			{
				comp.UsedOnce();
			}
			IntVec3 position = pawn.Position;
			IntVec3 cell = currentTarget.Cell;
			Map map = pawn.Map;
			bool flag = Find.Selector.IsSelected(pawn);
			PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(
				ThingDef.Named("ET_ShortFlighter"), pawn, cell, verbProps.flightEffecterDef, verbProps.soundLanding,
				verbProps.flyWithCarriedThing);
			if (pawnFlyer != null)
			{
				FleckMaker.ThrowDustPuff(
					position.ToVector3Shifted() + Gen.RandomHorizontalVector(0.5f), map, 2f);
				GenSpawn.Spawn(pawnFlyer, cell, map, WipeMode.Vanish);
				if (flag)
				{
					Find.Selector.Select(pawn, false, false);
				}
				return true;
			}
			return false;
		}

		public static void OrderShortFlight(Pawn pawn, LocalTargetInfo target, Verb verb, float range)
		{
			Map map = pawn.Map;
			IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(target.Cell, pawn, (IntVec3 c) => JumpUtility.ValidJumpTarget(map, c) && JumpUtility.CanHitTargetFrom(pawn, pawn.Position, c, range));
			Job job = JobMaker.MakeJob(JobDefOf.CastJump, intVec);
			job.verbToUse = verb;
			if (pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false))
			{
				FleckMaker.Static(intVec, map, FleckDefOf.FeedbackGoto, 1f);
			}
		}

		// Token: 0x06009980 RID: 39296 RVA: 0x0037A204 File Offset: 0x00378404
		public static bool CanHitTargetFrom(Pawn pawn, IntVec3 root, LocalTargetInfo targ, float range)
		{
			float num = range * range;
			IntVec3 cell = targ.Cell;
			return (float)pawn.Position.DistanceToSquared(cell) <= num && GenSight.LineOfSight(root, cell, pawn.Map, false, null, 0, 0);
		}

		// Token: 0x06009981 RID: 39297 RVA: 0x0037A240 File Offset: 0x00378440
		public static bool ValidFlightTarget(Map map, IntVec3 cell)
		{
			if (!cell.IsValid || !cell.InBounds(map))
			{
				return false;
			}
			if (cell.Impassable(map) || !cell.Walkable(map) || cell.Fogged(map))
			{
				return false;
			}
			Building edifice = cell.GetEdifice(map);
			Building_Door building_Door;
			return edifice == null || (building_Door = edifice as Building_Door) == null || building_Door.Open;
		}
	}
