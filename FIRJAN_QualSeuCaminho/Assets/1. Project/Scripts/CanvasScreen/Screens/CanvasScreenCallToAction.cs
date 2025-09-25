using UnityEngine;

public class CanvasScreenCallToAction : CanvasScreen
{
    public override void TurnOn()
    {
        if (SessionPlacar.Instance != null)
        {
            SessionPlacar.Instance.ResetInstance();
        }
        base.TurnOn();
    }
}
