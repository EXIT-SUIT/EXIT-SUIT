using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public Button selectServoPanel;
    public Button selectStepperPanel;

    public GameObject servoPanel;
    public GameObject stepperPanel;

    // Start is called before the first frame update
    void Start()
    {
        selectServoPanel.onClick.AddListener(()=> togglePanel(0));
        selectStepperPanel.onClick.AddListener(()=> togglePanel(1));
    }

    // Update is called once per frame
    void togglePanel(int panelID)
    {
        switch(panelID){
            case 0:
                servoPanel.SetActive(true);
                stepperPanel.SetActive(false);
                break;
            case 1:
                servoPanel.SetActive(false);
                stepperPanel.SetActive(true);
                break;
        }
        
    }
}
