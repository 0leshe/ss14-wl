using Content.Shared._WL.CharacterInformation;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client._WL.CharacterInformation;

[UsedImplicitly]
public sealed class CharacterInformationBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private CharacterInformationWindow? _window;

    public CharacterInformationBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = new CharacterInformationWindow();
        _window.OnClose += Close;
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not CharacterInformationBuiState msg)
            return;
        _window?.UpdateState(msg);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _window?.Dispose();
    }
}
