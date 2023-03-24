using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionTriggerScript : MonoBehaviour{

    public UIManager uiManager;

    public float maxDistance = 1.25f;
    public bool isTouching = false;

    public void start(){
        uiManager = GameObject.FindWithTag("Servo").GetComponent<UIManager>();
    }

    private void OnTriggerStay(Collider other){
        //Debug.Log("Collider onTriggerStay: "+other.gameObject.name);
        if(other.gameObject.tag == "Collidable"){

        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Collidable"){
            Debug.Log("Enter collide with "+other.gameObject.name);
            isTouching = true;
            uiManager.controlSliders[8].value = 5000;

        }        
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Collidable"){
            Debug.Log("Exit collide with "+other.gameObject.name);
            isTouching = false;
            uiManager.controlSliders[8].value = 0;
        }
    }
}
