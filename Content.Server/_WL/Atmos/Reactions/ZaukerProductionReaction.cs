using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class ZaukerProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialHyperNoblium = mixture.GetMoles(Gas.HyperNoblium);
        if (initialHyperNoblium >= 2.5f && mixture.Temperature > 20f)
        {
            mixture.AdjustMoles(Gas.HyperNoblium, initialHyperNoblium - 0.1f);
            return ReactionResult.NoReaction;
        }

        var initialHypernoblium = mixture.GetMoles(Gas.HyperNoblium);
        var initialNitrium = mixture.GetMoles(Gas.Nitrium);

        var temperature = mixture.Temperature;
        var producedAmount = Math.Min(temperature * Atmospherics.ZaukerFormationTemperatureScale, Math.Min(initialHypernoblium * 0.01f, initialNitrium * 0.5f));

        if (producedAmount <= 0)
            return ReactionResult.NoReaction;

        producedAmount = Math.Min(initialNitrium / 0.5f, producedAmount);
        producedAmount = Math.Min(initialHypernoblium / 0.01f, producedAmount);

        var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);

        mixture.AdjustMoles(Gas.HyperNoblium, -producedAmount * 0.01f);
        mixture.AdjustMoles(Gas.Nitrium, -producedAmount * 0.5f);
        mixture.AdjustMoles(Gas.Zauker, producedAmount * 0.5f);

        var energyUsed = producedAmount * Atmospherics.ZaukerFormationEnergy;

        var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * oldHeatCapacity - energyUsed) / newHeatCapacity, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
