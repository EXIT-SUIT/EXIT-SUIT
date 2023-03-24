using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public int deviceID = 0;
    private SerialDevice serialDevice;
    public bool serialDeviceReady = false;

    public GameObject controlPanel;

    public Button resetDataButton;
    public Button toggleDeviceStateButton;
    public Dropdown serialDropdown;
    public Dropdown baudDropdown;

    public List<Slider> controlSliders = new List<Slider>();
    public List<Slider> feedbackSliders = new List<Slider>();
    int[] controlSliderData;

    List<string> baudRates = new List<string>(){"9600", "19200", "38400", "57600", "115200"};
    private int[] deviceParams = {15, 5};
    int[,] servoRanges = new int[15,2]{{0,1}, //controllerID
                                    {0,100}, //friction 1
                                    {0,100}, //friction 2
                                    {0,100}, //friction 3
                                    {0,100}, //friction 4
                                    {0,100}, //friction 5
                                    {-1000,1000}, //arm pos
                                    {-1000,1000}, //arm vel
                                    {-1000,1000}, //arm acc
                                    {-7500,7500}, //arm torque
                                    {0,5000}, //arm kd
                                    {0,10000}, //arm kp
                                    {0,255}, //setting 1
                                    {0,255}, //setting 2
                                    {0,255} //error flag
                                    };
    int[,] stepperRanges = new int[5,2]{{0,1},{-1000, 1000},{0, 255}, {0,255}, {0,255}};
    int[,] sliderRanges;
    
    private string dataString;

    void Start()
    {
        controlPanel.SetActive(false);

        toggleDeviceStateButton.onClick.AddListener(()=> toggleDeviceState(deviceID));
        resetDataButton.onClick.AddListener(()=> resetData()); 

        populateDropdown(serialDropdown, listAvailablePorts());
        populateDropdown(baudDropdown, baudRates);

        if (deviceID == 0)
        {
            sliderRanges = servoRanges;
        }
        else
        {
            sliderRanges = stepperRanges;
        }

        controlSliders = getSliders(controlPanel, "Slider");
        feedbackSliders = getSliders(controlPanel, "Visualiser");
        controlSliderData = new int[controlSliders.Count];

        int index = 0;
        foreach(Slider controlSlider in controlSliders){
            index++; //skip first parameter--controllerID only on receiving data
            controlSlider.minValue = sliderRanges[index,0];
            controlSlider.maxValue = sliderRanges[index,1];
            controlSlider.wholeNumbers = true;
            controlSlider.onValueChanged.AddListener(delegate{ setSliderData();});
            controlSlider.value = 0;

        }

        index = 0;
        foreach(Slider feedbackSlider in feedbackSliders){
            feedbackSlider.minValue = sliderRanges[index,0];
            feedbackSlider.maxValue = sliderRanges[index,1];
            feedbackSlider.wholeNumbers = true;
            feedbackSlider.value = 0;
            index++;
        }
	}

    //every frame: set feedback sliders to latest data from serial device
    void Update(){        
        if(serialDevice != null){
            if(serialDevice.IsPortOpen()){
                for(int i=0; i<serialDevice.paramCount; i++){
                    feedbackSliders[i].value = serialDevice.receivedDataAsIntArray[i];
                }
            }
        }
    }

	public void updateDataArray(int[] data)
	{
        if(serialDevice!=null && serialDevice.IsPortOpen()){
            Debug.Log("Updating data on "+serialDevice.portName);
            serialDevice.updateWriteData(data);
        }else{
            Debug.Log("Device is not open");
        }
	}

    //called when slider value is changed
    public void setSliderData(){
        int index = 0;
        foreach(Slider controlSlider in controlSliders){
            controlSliderData[index] = (int)controlSlider.value;
            index++;
        }
        updateDataArray(controlSliderData);
    }

    //loop through sliders with specific tagName
    List<Slider> getSliders(GameObject parent, string tagName){
        List<Slider> s = new List<Slider>();
        Transform t = parent.transform;
        for(int i=0; i<t.childCount; i++){
            if(t.GetChild(i).gameObject.tag==tagName){
                s.Add(t.GetChild(i).gameObject.GetComponent<Slider>());
            }
        }
        return s;
    }

    public List<String> listAvailablePorts(){
        string[] portNames = SerialPort.GetPortNames();
        int index = 0;
        foreach(string port in SerialPort.GetPortNames()){
            if(port.Contains("usbmodem")){
                Debug.Log(index+" : "+SerialPort.GetPortNames()[index]);
            }
            index++;
        }
        List<string> portNamesList = portNames.ToList();
        return portNamesList;
    }

    //add options to dropdown menu
    public void populateDropdown(Dropdown dropdown, List<string> options){
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }

    //called when option in dropdown selected
    public void dropDownValueChanged(int deviceID, Dropdown dropdown){
        int selectionValue = dropdown.value;
        string selection = dropdown.options[dropdown.value].text;
        Debug.Log("Selected :"+selection);
    }
    
    //called on button click to open/close serial device
    public void toggleDeviceState(int deviceID){
        if(serialDevice != null && serialDevice.IsPortOpen()){

            resetData();
            serialDevice.closeDevice();
            toggleDeviceStateButton.GetComponentInChildren<Text>().text = "Open Device";
            serialDeviceReady = false;

        }else{
            int baudRate = int.Parse(baudDropdown.options[baudDropdown.value].text);
            string portName = serialDropdown.options[serialDropdown.value].text;
            createSerialDevice(deviceID, portName, baudRate, deviceParams[deviceID]); //UPDATE
            toggleDeviceStateButton.GetComponentInChildren<Text>().text = "Close Device";
            serialDeviceReady = true;

        }
    }
    
    public void createSerialDevice(int deviceID, string portName, int baudRate, int paramLength){
        if(serialDevice!=null){

            //if a serial port is already open, close it first
            if(serialDevice.IsPortOpen()){
                serialDevice.closeDevice();
                serialDevice = null;
            }
        }

        Debug.Log("Creating device ID "+deviceID+" on "+portName+" at "+baudRate+" expecting "+paramLength+" params");
        serialDevice = new SerialDevice(portName, baudRate, 1000, paramLength);
    }

    public float map(float input, float inputMin, float inputMax, float min, float max){
	    return min + (input - inputMin) * (max - min) / (inputMax - inputMin);
    }

    // TODO: UI Button and OnClick event
    public void resetData(){
        foreach(Slider controlSlider in controlSliders){
            controlSlider.value = 0;
        }
    }

    //Exit gracefully
    void OnApplicationQuit(){
        if(serialDevice != null){
            resetData();
            serialDevice.closeDevice();
        }
    
        toggleDeviceStateButton.GetComponentInChildren<Text>().text = "Open Device";
    }
}
