using Content.Server.Explosion.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Database;

namespace Content.Server.Atmos.EntitySystems
{
    public sealed partial class AtmosphereSystem
    {
        [Dependency] private readonly ExplosionSystem _explosions = default!;

        private bool Explosion(TileAtmosphere tile, float explosionPower)
        {
            var mixture = tile.Air;
            if (mixture == null)
                return false;
            var initialKritium = mixture.GetMoles(Gas.Kritium);
            var initialNO = mixture.GetMoles(Gas.NitrousOxide);
            _adminLog.Add(LogType.Flammable, LogImpact.Extreme, $"Atmos EXPLOSION with gas: {initialNO}mol nitrous oxide, {initialKritium}mol kritium with power {explosionPower}");
            _explosions.QueueExplosion(_transformSystem.ToMapCoordinates(_mapSystem.ToCenterCoordinates(tile.GridIndex, tile.GridIndices)), "Default", explosionPower, 5, 50, cause: null, addLog: false);
            return true;
        }
    }
}
