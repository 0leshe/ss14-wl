using Content.Server.Atmos.EntitySystems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class KritiumFireReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialHyperNoblium = mixture.GetMoles(Gas.HyperNoblium);
        if (initialHyperNoblium >= 2.5f && mixture.Temperature > 20f)
        {
            mixture.AdjustMoles(Gas.HyperNoblium, initialHyperNoblium - 0.1f);
            return ReactionResult.NoReaction;
        }

        return ReactionResult.Reacting;
    }
}
