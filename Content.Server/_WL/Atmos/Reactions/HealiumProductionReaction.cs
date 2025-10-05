using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class HealiumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialHyperNoblium = mixture.GetMoles(Gas.HyperNoblium);
        if (initialHyperNoblium >= 2.5f && mixture.Temperature > 20f)
        {
            mixture.AdjustMoles(Gas.HyperNoblium, initialHyperNoblium - 0.1f);
            return ReactionResult.NoReaction;
        }

        var initialBZ = mixture.GetMoles(Gas.BZ);
        var initialFrezon = mixture.GetMoles(Gas.Frezon);

        var temperature = mixture.Temperature;
        var producedAmount = Math.Min(temperature * 0.3f, Math.Min(initialFrezon * 2.75f, initialBZ * 0.25f));

        if (producedAmount <= 0)
            return ReactionResult.NoReaction;

        producedAmount = Math.Min(initialFrezon / 2.75f, producedAmount);
        producedAmount = Math.Min(initialBZ / 0.25f, producedAmount);

        var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);

        mixture.AdjustMoles(Gas.Frezon, -producedAmount * 2.75f);
        mixture.AdjustMoles(Gas.BZ, -producedAmount * 0.25f);
        mixture.AdjustMoles(Gas.Healium, producedAmount * 3);

        var energyReleased = producedAmount * Atmospherics.HealiumFormationEnergy;

        var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * oldHeatCapacity + energyReleased) / newHeatCapacity, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
