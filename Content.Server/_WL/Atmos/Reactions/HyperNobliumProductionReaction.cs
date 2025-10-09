using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class HyperNobliumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialHypernoblium = mixture.GetMoles(Gas.HyperNoblium);
        if (initialHypernoblium >= 2.5f && mixture.Temperature > 20f)
        {
            mixture.AdjustMoles(Gas.HyperNoblium, -0.1f);
            return ReactionResult.NoReaction;
        }
        var initialNitrogen = mixture.GetMoles(Gas.Nitrogen);
        var initialTritium = mixture.GetMoles(Gas.Tritium);
        var initialBZ = mixture.GetMoles(Gas.BZ);

        var producedAmount = Math.Min((initialNitrogen + initialTritium) * 0.01f, Math.Min(initialTritium * 5f, initialNitrogen * 10f));
        if (producedAmount <= 0 || (initialTritium - 5f) * producedAmount < 0 || (initialNitrogen - 10f) * producedAmount < 0)
            return ReactionResult.NoReaction;

        var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);

        var reductionFactor = Math.Clamp(initialTritium / (initialTritium + initialBZ), 0.001f, 1f);

        mixture.AdjustMoles(Gas.Tritium, -5f * producedAmount * reductionFactor);
        mixture.AdjustMoles(Gas.Nitrogen, -10f * producedAmount);
        mixture.AdjustMoles(Gas.HyperNoblium, producedAmount);

        var energyReleased = producedAmount * (Atmospherics.NobliumFormationEnergy / Math.Max(initialBZ, 1));

        var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * oldHeatCapacity + energyReleased) / newHeatCapacity, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
