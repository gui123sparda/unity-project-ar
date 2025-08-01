using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TwoStatesButton : MonoBehaviour
{
    bool isClicked = false;
    public MultipleImageTrackingManager trackingManager;
    public TextMeshProUGUI text;

    public void ButtonToggle()
    {
        if (isClicked)
        {
            text.SetText("Resumir");
            trackingManager.ResumeTracking();
            isClicked = false;
        }
        else
        {
            text.SetText("Parar");
            trackingManager.StopTracking();
            isClicked = true;

        }
    }
    
}
