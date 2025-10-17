using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class BZProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialHypernoblium = mixture.GetMoles(Gas.HyperNoblium);
        if (initialHypernoblium >= 2.5f && mixture.Temperature > 20f)
        {
            mixture.AdjustMoles(Gas.HyperNoblium, -0.1f);
            return ReactionResult.NoReaction;
        }
        var initN2O = mixture.GetMoles(Gas.NitrousOxide);
        var initPlasma = mixture.GetMoles(Gas.Plasma);

        var environmentEfficiency = mixture.Volume / mixture.Pressure;
        var ratioEfficiency = Math.Min(initN2O / initPlasma, 1);

        var producedAmount = Math.Min(0.09f * ratioEfficiency * environmentEfficiency, Math.Min(initN2O * 0.4f, initPlasma * 0.8f));

        if (producedAmount <= 0)
            return ReactionResult.NoReaction;
        producedAmount = Math.Min(initPlasma / 0.8f, producedAmount);
        producedAmount = Math.Min(initN2O / 0.4f, producedAmount);

        var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);

        var nitrousOxideDecomposed = Math.Max(4.0f * (initPlasma / (initN2O + initPlasma) - 0.75f), 0);
        var nitrogenAdded = 0f;
        var oxygenAdded = 0f;
        if (nitrousOxideDecomposed > 0)
        {
            var amountDecomposed = 0.4f * producedAmount * nitrousOxideDecomposed;
            nitrogenAdded = amountDecomposed;
            oxygenAdded = 0.5f * amountDecomposed;
        }
        var bzFormed = producedAmount * (1f - nitrousOxideDecomposed);
        var n2oRemoved = Math.Min(initN2O, 0.4f * producedAmount);
        var plasmaRemoved = Math.Min(initPlasma, 0.8f * bzFormed);

        mixture.AdjustMoles(Gas.NitrousOxide, -n2oRemoved);
        mixture.AdjustMoles(Gas.Plasma, -plasmaRemoved);
        mixture.AdjustMoles(Gas.Nitrogen, nitrogenAdded);
        mixture.AdjustMoles(Gas.Oxygen, oxygenAdded);
        mixture.AdjustMoles(Gas.BZ, bzFormed);

        var energyReleased = producedAmount * (Atmospherics.BZFormationEnergy + nitrousOxideDecomposed);
        var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * oldHeatCapacity + energyReleased) / newHeatCapacity, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
