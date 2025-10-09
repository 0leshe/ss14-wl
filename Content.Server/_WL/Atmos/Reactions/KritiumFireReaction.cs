using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
[DataDefinition]
public sealed partial class KritiumFireReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialHypernoblium = mixture.GetMoles(Gas.HyperNoblium);
        if (initialHypernoblium >= 2.5f && mixture.Temperature > 20f)
        {
            mixture.AdjustMoles(Gas.HyperNoblium, -0.1f);
            return ReactionResult.NoReaction;
        }
        mixture.ReactionResults[(byte)GasReaction.Fire] = 0;
        var initialKritium = mixture.GetMoles(Gas.Kritium);
        var initialNO = mixture.GetMoles(Gas.NitrousOxide);

        var explosionPower = (float)((Math.Pow(initialKritium, 1.08f) + 3.1f * Math.Pow(initialKritium, 0.59)) * (-0.69f * Math.Pow(50, -40 * (initialNO / initialKritium)) + 1.75f));

        if (holder is TileAtmosphere && explosionPower >= 1)
        {
            var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
            mixture.AdjustMoles(Gas.Kritium, -initialKritium);
            mixture.AdjustMoles(Gas.NitrousOxide, -initialNO);
            var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
            mixture.Temperature = Math.Max((mixture.Temperature * oldHeatCapacity) / newHeatCapacity, Atmospherics.TCMB);
            atmosphereSystem.Explosion(holder, explosionPower);
        }

        return ReactionResult.Reacting;
    }
}
