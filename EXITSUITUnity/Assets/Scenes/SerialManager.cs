using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;

public class SerialManager : MonoBehaviour{
    static public int numDevices = 2;
    public SerialDevice[] serialDevice = new SerialDevice[numDevices];

    public int timeOut = 1000; //in milliseconds

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

    public void createSerialDevice(int deviceID, string portName, int baudRate, int paramLength){
        if(serialDevice[deviceID]!=null){

            //if a serial port is already open, close it first
            if(serialDevice[deviceID].IsPortOpen()){
                serialDevice[deviceID].closeDevice();
                serialDevice[deviceID] = null;
            }
        }

        Debug.Log("Creating device ID "+deviceID+" on "+portName+" at "+baudRate+" expecting "+paramLength+" params");
        serialDevice[deviceID] = new SerialDevice(portName, baudRate, timeOut, paramLength);
    }

    public void updateWriteData(int deviceID, int[] dataArray){
        if(serialDevice[deviceID]!=null){

            //**** use SequenceEqual test to only build data string if array contents different
            string s = "<";
            
            for(int i=0; i<dataArray.Length-1; i++){
                s += (dataArray[i].ToString()+",");
            }
            s+=dataArray[dataArray.Length-1];
            s+=">";
            Debug.Log("WRITE: "+serialDevice[deviceID].portName+": Data formatted as "+s+" with "+dataArray.Length+"params");

            serialDevice[deviceID].dataToWrite = s;   
        }
    }

}
