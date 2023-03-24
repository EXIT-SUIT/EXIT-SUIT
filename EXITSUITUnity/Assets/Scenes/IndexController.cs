using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class IndexController : MonoBehaviour
{
    // Start is called before the first frame update

    public SteamVR_Action_Vector2 ThumbstickAction = null;
    public UIManager uiManager;

    public GameObject leftHand;
    public GameObject rightHand;

    public SteamVR_Action_Boolean trigger;
    public SteamVR_Input_Sources handType;
    bool triggerIsDown = false;

    private float angle;
    private float yDist;

    SerialDevice stepperDevice;

    void Start()
    {   
        Debug.Log("Setting up controller triggers");
        trigger.AddOnStateDownListener(triggerDown, handType);
        trigger.AddOnStateUpListener(triggerUp, handType);
    }

    // Update is called once per frame
    void Update()
    {

        //angle  = Vector2.Angle(leftHand.transform.position, rightHand.transform.position);
        float leftY = leftHand.transform.position.y;
        float rightY = rightHand.transform.position.y;
        float yDist = Mathf.Clamp((rightY-leftY)*500, -1000, 1000);
        //Debug.Log("leftY: "+leftY+" rightY: "+rightY+ " yDist: "+yDist);
        
        //float thumbstickX = (ThumbstickAction.axis.x+1)*0.5f;
        if(uiManager.serialDeviceReady && triggerIsDown){
            uiManager.controlSliders[0].value = yDist;
            Debug.Log("yDist "+yDist);

        }else{
            uiManager.controlSliders[0].value = 0;
        }
        
    }

    void triggerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource){
        triggerIsDown = false;
    }

    void triggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource){
 

        triggerIsDown = true;
    }
}
