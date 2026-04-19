using UnityEngine;
using UnityEngine.InputSystem;

public class UITestOpen : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            UIManager.Instance.Open<PlanetsPanel>(
            new PlanetsPanel.OpenData { correctPlanetId = 5 }
            );
        }
    }

    [ContextMenu("Open Planets Panel")]
    public void OpenNow()
    {
        UIManager.Instance.Open<PlanetsPanel>();
    }
}
