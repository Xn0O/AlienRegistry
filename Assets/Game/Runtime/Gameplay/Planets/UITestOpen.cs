using UnityEngine;

public class UITestOpen : MonoBehaviour
{
    [SerializeField] private int correctPlanetId = 5;
    [SerializeField] private string planetsViewKey = "demo_planets";

    public void OpenNow()
    {
        Debug.Log("OpenNow clicked");
        UIManager.Instance.Open<PlanetsPanel>(
            new PlanetsPanel.OpenData
            {
                correctPlanetId = correctPlanetId,
                planetsViewKey = planetsViewKey
            }
        );
    }
}
