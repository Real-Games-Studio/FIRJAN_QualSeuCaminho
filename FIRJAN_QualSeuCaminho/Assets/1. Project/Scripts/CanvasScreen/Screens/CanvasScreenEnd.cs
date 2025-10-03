using UnityEngine;

public class CanvasScreenEnd : CanvasScreen
{
    public void ReloadCurrentScene()
    {
        // carregar a cena inicial
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
