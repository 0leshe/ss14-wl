using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class NitriumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialHypernoblium = mixture.GetMoles(Gas.HyperNoblium);
        if (initialHypernoblium >= 2.5f && mixture.Temperature > 20f)
        {
            mixture.AdjustMoles(Gas.HyperNoblium, -0.1f);
            return ReactionResult.NoReaction;
        }
        var initialTritium = mixture.GetMoles(Gas.Tritium);
        var initialNitrogen = mixture.GetMoles(Gas.Nitrogen);
        var initialBZ = mixture.GetMoles(Gas.BZ);

        var temperature = mixture.Temperature;
        var producedAmount = Math.Min(temperature / Atmospherics.NitriumFormationTempDivisor, Math.Min(initialTritium, Math.Min(initialNitrogen, initialBZ * 0.05f)));

        if (producedAmount <= 0)
            return ReactionResult.NoReaction;

        producedAmount = Math.Min(initialNitrogen, producedAmount);
        producedAmount = Math.Min(initialTritium, producedAmount);
        producedAmount = Math.Min(initialBZ / 0.05f, producedAmount);

        var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
        mixture.AdjustMoles(Gas.Tritium, -producedAmount);
        mixture.AdjustMoles(Gas.Nitrogen, -producedAmount);
        mixture.AdjustMoles(Gas.BZ, -producedAmount * 0.05f);
        mixture.AdjustMoles(Gas.Nitrium, producedAmount);

        var energyUsed = producedAmount * Atmospherics.NitriumFormationEnergy;

        var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * oldHeatCapacity - energyUsed) / newHeatCapacity, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
