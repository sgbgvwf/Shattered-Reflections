using Core;
public interface IInputController
{
    public void RegisterInput(InputEvent inputEvent, IInputController inputController);

    public void LoadAction();

    public void UnloadAction();
}
