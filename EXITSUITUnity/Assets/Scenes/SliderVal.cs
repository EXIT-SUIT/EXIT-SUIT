using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderVal : MonoBehaviour
{

    public Slider slider;
    public Text valLabel;
    // Start is called before the first frame update
    void Start()
    {
        slider.onValueChanged.AddListener(delegate{ updateValue();});   
    }

    // Update is called once per frame
    void updateValue()
    {
        valLabel.text = slider.value.ToString();
    }
}
