using Robust.Shared.Configuration;

namespace Content.Shared._WL.CCVars;

/// <summary>
///     WL modules console variables
/// </summary>
[CVarDefs]
public sealed class WLCVars
{
    /// <summary>
    /// Через сколько времени(в секундах) появится кнопка возвращения в лобби.
    /// </summary>
    public static readonly CVarDef<int> GhostReturnToLobbyButtonCooldown =
        CVarDef.Create("ghost.return_to_lobby_button_cooldown", 1200, CVar.SERVERONLY);

    /// <summary>
    /// Нужно ли проверять игрока на возраст при выборе роли.
    /// </summary>
    public static readonly CVarDef<bool> IsAgeCheckNeeded =
        CVarDef.Create("game.is_age_check_needed", true, CVar.SERVER | CVar.REPLICATED);
}
