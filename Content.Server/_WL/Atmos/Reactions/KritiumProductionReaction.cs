using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class KritiumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialHypernoblium = mixture.GetMoles(Gas.HyperNoblium);
        if (initialHypernoblium >= 2.5f && mixture.Temperature > 20f)
        {
            mixture.AdjustMoles(Gas.HyperNoblium, -0.1f);
            return ReactionResult.NoReaction;
        }
        var initialOxygen = mixture.GetMoles(Gas.Oxygen);
        var initialPlasma = mixture.GetMoles(Gas.Plasma);

        var oxyRatio = initialPlasma / (initialPlasma + initialOxygen);

        var productionRate = Math.Max(0, (float)(-100 * Math.Pow(oxyRatio - 0.9, 2) + 1));

        var producedAmount = ((initialPlasma / 2 + initialOxygen) * productionRate) / 3;

        if (producedAmount <= 0)
            return ReactionResult.NoReaction;

        producedAmount = Math.Min(initialPlasma, producedAmount * 2);
        producedAmount = Math.Min(initialOxygen, producedAmount);

        var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);

        mixture.AdjustMoles(Gas.Oxygen, producedAmount * -1);
        mixture.AdjustMoles(Gas.Plasma, producedAmount * -2);
        mixture.AdjustMoles(Gas.Kritium, producedAmount * 3);

        var energyReleased = producedAmount * (Atmospherics.KritiumFormationEnergy / Math.Max(initialPlasma, 1));

        var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * oldHeatCapacity + energyReleased) / newHeatCapacity, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
