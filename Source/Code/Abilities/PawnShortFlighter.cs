using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ElderThingFaction
{
	
public class PawnShortFlighter : PawnFlyer
{
	private static readonly Func<float, float> FlightSpeed;

	private static readonly Func<float, float> FlightCurveHeight;

	private Material cachedShadowMaterial;

	private Effecter flightEffecter;

	private int positionLastComputedTick = -1;

	private Vector3 groundPos;

	private Vector3 effectivePos;

	private float effectiveHeight;

	private Material ShadowMaterial
	{
		get
		{
			if (cachedShadowMaterial == null && !def.pawnFlyer.shadow.NullOrEmpty())
			{
				cachedShadowMaterial = MaterialPool.MatFrom(def.pawnFlyer.shadow, ShaderDatabase.Transparent);
			}
			return cachedShadowMaterial;
		}
	}

	public override Vector3 DrawPos
	{
		get
		{
			RecomputePosition();
			return effectivePos;
		}
	}

	static PawnShortFlighter()
	{
		FlightCurveHeight = GenMath.InverseParabola;
		AnimationCurve animationCurve = new AnimationCurve();
		animationCurve.AddKey(0f, 0f);
		animationCurve.AddKey(0.1f, 0.15f);
		animationCurve.AddKey(1f, 1f);
		FlightSpeed = animationCurve.Evaluate;
	}

	protected override bool ValidateFlyer()
	{
		return true;
	}

	private void RecomputePosition()
	{
		if (positionLastComputedTick != ticksFlying)
		{
			positionLastComputedTick = ticksFlying;
			float arg = (float)ticksFlying / (float)ticksFlightTime;
			float num = FlightSpeed(arg);
			effectiveHeight = FlightCurveHeight(num);
			groundPos = Vector3.Lerp(startVec, base.DestinationPos, num);
			Vector3 vector = new Vector3(0f, 0f, 2f);
			Vector3 vector2 = Altitudes.AltIncVect * effectiveHeight;
			Vector3 vector3 = vector * effectiveHeight;
			effectivePos = groundPos + vector2 + vector3;
		}
	}

	public override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		RecomputePosition();
		DrawShadow(groundPos, effectiveHeight);
		base.FlyingPawn.DrawAt(effectivePos, flip);
		if (base.CarriedThing != null)
		{
			PawnRenderer.DrawCarriedThing(base.FlyingPawn, effectivePos, base.CarriedThing);
		}
	}

	private void DrawShadow(Vector3 drawLoc, float height)
	{
		Material shadowMaterial = ShadowMaterial;
		if (!(shadowMaterial == null))
		{
			float num = Mathf.Lerp(1f, 0.6f, height);
			Vector3 s = new Vector3(num, 1f, num);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(drawLoc, Quaternion.identity, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
		}
	}

	protected override void RespawnPawn()
	{
		LandingEffects();
		base.RespawnPawn();
	}

	public override void Tick()
	{
		if (flightEffecter == null && flightEffecterDef != null)
		{
			flightEffecter = flightEffecterDef.Spawn();
			flightEffecter.Trigger(this, TargetInfo.Invalid);
		}
		else
		{
			flightEffecter?.EffectTick(this, TargetInfo.Invalid);
		}
		base.Tick();
	}

	private void LandingEffects()
	{
		if (soundLanding != null)
		{
			soundLanding.PlayOneShot(new TargetInfo(base.Position, base.Map));
		}
		FleckMaker.ThrowDustPuff(base.DestinationPos + Gen.RandomHorizontalVector(0.5f), base.Map, 2f);
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		flightEffecter?.Cleanup();
		base.Destroy(mode);
	}
}

}
