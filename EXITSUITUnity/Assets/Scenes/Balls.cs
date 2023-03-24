using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balls : MonoBehaviour
{

    public int maxBalls = 150;
    public int balls = 0;

    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(spawnBalls());
    }

    // Update is called once per frame
    IEnumerator spawnBalls()
    {
       while(balls < maxBalls){
           makeBall();
           balls++;
           yield return new WaitForSeconds(0.1f);
       }
    }

    void makeBall(){
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Rigidbody ballRb = ball.AddComponent<Rigidbody>();
        Collider ballCollider = ball.AddComponent<SphereCollider>();
        // var Sphere = gameObject.GetComponent<Renderer>();
        // Sphere.material.SetColor("_Color", Color.red);
        ballRb.useGravity = true;
        ballRb.isKinematic = false;
        ballCollider.isTrigger = false;
        ball.tag = "Collidable";
    
        ball.transform.position = new Vector3(Random.Range(-1.9f, 1.9f), Random.Range(0.3f, 0.9f), Random.Range(-1.1f, 2.4f));
        ball.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
    }
}
