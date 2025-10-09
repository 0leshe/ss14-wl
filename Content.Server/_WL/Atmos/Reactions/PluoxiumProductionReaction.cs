using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class PluoxiumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialHypernoblium = mixture.GetMoles(Gas.HyperNoblium);
        if (initialHypernoblium >= 2.5f && mixture.Temperature > 20f)
        {
            mixture.AdjustMoles(Gas.HyperNoblium, initialHypernoblium - 0.1f);
            return ReactionResult.NoReaction;
        }
        var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
        var initialCarbonDioxide = mixture.GetMoles(Gas.CarbonDioxide);
        var initialOxygen = mixture.GetMoles(Gas.Oxygen);
        var initialTritium = mixture.GetMoles(Gas.Tritium);

        var producedAmount = Math.Min(Atmospherics.PluoxiumMaxRate, Math.Min(initialCarbonDioxide, Math.Min(initialOxygen * 0.5f, initialTritium * 0.01f)));

        if (producedAmount <= 0)
            return ReactionResult.NoReaction;

        producedAmount = Math.Min(producedAmount, initialCarbonDioxide);
        producedAmount = Math.Min(producedAmount, initialOxygen / 0.5f);
        producedAmount = Math.Min(producedAmount, initialTritium / 0.01f);

        mixture.AdjustMoles(Gas.CarbonDioxide, -producedAmount);
        mixture.AdjustMoles(Gas.Oxygen, -producedAmount * 0.5f);
        mixture.AdjustMoles(Gas.Tritium, -producedAmount * 0.01f);
        mixture.AdjustMoles(Gas.Pluoxium, producedAmount);

        var energyReleased = producedAmount * Atmospherics.PluoxiumFormationEnergy;

        var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * oldHeatCapacity + energyReleased) / newHeatCapacity, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
