using UnityEngine;

public class CanvasScreenEnd : CanvasScreen
{
    public Transform childToMove;
    public Transform originalParet;
    public Transform newParent;

    public override void TurnOn()
    {
        childToMove.SetParent(newParent);
        base.TurnOn();
    }

    public override void TurnOff()
    {   
        childToMove.SetParent(originalParet);
        base.TurnOff();
    }


    public void ReloadCurrentScene()
    {
        // carregar a cena inicial
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
