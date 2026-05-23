using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputScene
{
    Move,
    Backpack

}

public class InputSystemManager : Singleton<InputSystemManager>
{

    public Dictionary<InputScene, IInputController> IsRegisterInputController = new Dictionary<InputScene, IInputController>();
    public List<IInputController> CurrentInputController = new List<IInputController>();

    public InputSystem controller;





    protected override void Awake()
    {
        base.Awake();

        controller = new InputSystem();


    }



    private void OnEnable()
    {
        controller.Enable();
    }

    private void OnDisable()
    {
        controller.Disable();
    }

    private void Update()
    {

        if (CurrentInputController.Count == 0)
        {
            UseInputController(InputScene.Move);

        }
        /*
        //Debug.Log(IsRegisterInputController[InputScene.Backpack]);
        if (test)
            UseInputController(InputScene.Backpack);
        test = false;
        */
        // UseInputController(InputScene.Move);

    }


    public void RegisterInputController(InputScene inputScene,IInputController inputController)
    {
        if(!IsRegisterInputController.ContainsKey(inputScene))
            IsRegisterInputController.Add(inputScene, inputController);
    }

    public void UseInputController(InputScene inputScene)
    {
        IsRegisterInputController.TryGetValue(inputScene, out IInputController inputController);
        inputController.OpenController();
        if(!CurrentInputController.Contains(inputController))
            CurrentInputController.Add(inputController);
    }
    
    public void UnuseInputController(InputScene inputScene)
    {
        IsRegisterInputController.TryGetValue(inputScene, out IInputController inputController);
        inputController.CloseController();
        if (CurrentInputController.Contains(inputController))
            CurrentInputController.Remove(inputController);
    }










}
