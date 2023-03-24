using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SerialDevice {
    private SerialPort serial;
    public string portName;
    public int baudRate;
    public int timeOut;
    public int paramCount;
    public bool reading = false;
    public bool writing = false;
    public string dataToWrite, prevDataWritten;
    public int[] receivedDataAsIntArray;
    public int[] writeDataAsIntArray;

    private Thread readThread;
    private Thread writeThread;
    
    public SerialDevice(string _portName, int _baudRate, int _timeOut, int _paramCount){
        Debug.Log("Trying to open new serial port at "+_portName+" and baudrate "+_baudRate+" with "+_paramCount+" parameters.");
        portName = _portName;
        baudRate = _baudRate;
        timeOut = _timeOut;
        paramCount = _paramCount;
        serial = new SerialPort(portName, baudRate);
        serial.ReadTimeout = 5000;
        serial.DtrEnable = true;
        //serial.NewLine = ">";

        receivedDataAsIntArray = new int[paramCount]; 
        writeDataAsIntArray = new int[paramCount-1]; //we write one less param to device than what returns
        for(int i=0; i<writeDataAsIntArray.Length; i++){
            writeDataAsIntArray[i] = 0;
        }

        if(!serial.IsOpen){
            serial.Open();

            if(serial.IsOpen){
                Debug.Log("Serial port "+portName+" is open");
                serial.DiscardInBuffer();
                serial.DiscardOutBuffer();
                serial.BaseStream.Flush();

                readThread = new Thread(readSerialData);
                readThread.Start();
                reading = true;
                writeThread = new Thread(writeSerialData);
                writeThread.Start();
                writing = true;
            }
        }
    }

     public bool IsPortOpen(){
        return serial.IsOpen;
    }

    public void closeDevice(){

        if(readThread.IsAlive){
            reading = false;
            readThread.Abort();
            Debug.Log("Closed read thread");
        }

        if(writeThread.IsAlive){
            writing = false;
            writeThread.Abort();
            Debug.Log("Closed write thread");
        }

        if(serial.IsOpen){
            serial.DiscardInBuffer();
            serial.DiscardOutBuffer();
            serial.BaseStream.Flush();
            serial.Close();
            Debug.Log("Closed port at "+portName);
        }
    }

    //****THREAD: write string to serial port
    public void writeSerialData(){
        while(writing){
            //buildDataString(writeDataAsIntArray);
            if(!serial.IsOpen){
                Debug.LogWarning("Can't write data to "+portName+", port is closed. Attempted to open...");
                serial.Open();
                if(serial.IsOpen){
                    Debug.Log("Port "+portName+" is now open.");
                }
            }else{
                if(dataToWrite!=null){
                    if(!dataToWrite.Equals(prevDataWritten)){
                        //Debug.Log("Writing "+dataToWrite+" to "+portName);
                        serial.Write(dataToWrite);
                        prevDataWritten = dataToWrite;
                    }
                }
            }
            Thread.Sleep(20);
        }
    }

    public void updateWriteData(int[] dataArray){

        //**** use SequenceEqual test to only build data string if array contents different
        string s = "<";
        
        for(int i=0; i<dataArray.Length-1; i++){
            s += (dataArray[i].ToString()+",");
        }
        s+=dataArray[dataArray.Length-1];
        s+=">";
        Debug.Log("WRITE: "+portName+": Data formatted as "+s+" with "+dataArray.Length+"params");

        dataToWrite = s;   
    
    }


    //**** THREAD: to read serial data and update int array of latest data
    public void readSerialData(){
        DateTime init = DateTime.Now;
        TimeSpan timeElapsed = default(TimeSpan);
        DateTime now;
        string buffer = "";
        string prevBuffer = "";
        bool newPacket = false;
        
        while(true){
            if(!serial.IsOpen){
                Debug.LogWarning("Serial port "+portName+" is not open. Trying to open...");
                serial.Open();
                Debug.Log("Serial port "+portName+" is open");
            }else{
                
                string dataReceived = serial.ReadLine(); //get data
                //Debug.Log("RECEIVED: " + dataReceived);
                if(dataReceived!=null && dataReceived!=""){ //check that it's not empty

                    if(dataReceived.IndexOf('<')==0){
                        //new packet
                        buffer = "";
                        newPacket = true;
                    }
                    if(newPacket==true){
                        buffer+=dataReceived;

                        if(dataReceived.IndexOf('>')==dataReceived.Length-1){
                            //complete packet, clear buffer and resend
                            newPacket = false;

                            if(!buffer.Equals(prevBuffer)){
                                //Debug.Log("Time since last packet: "+timeElapsed.Milliseconds);
                                //convert from string to int array
                                parseDataAsInts(buffer);
                                prevBuffer = buffer;
                                buffer="";
         
                            }
                            //update timeouts
                            now = DateTime.Now;
                            timeElapsed = now-init;
                        }
                        
                        //timeout
                        if(timeElapsed.Milliseconds > 1000){
                            Debug.LogWarning("Timeout of "+timeElapsed.Milliseconds+" dropping "+buffer);
                            newPacket = false;
                            prevBuffer = buffer;
                            buffer = "";
                            now = DateTime.Now;
                            timeElapsed = now-init;
                        }
                    }
                }
            }
            //Thread.Sleep(20);
        }
    }

    public void parseDataAsInts(string s){
        if(s.Length>2){
            string cleanData = s.TrimStart('<');
            cleanData = cleanData.TrimEnd('>');
            string[] splitData = cleanData.Split(',');
            if(splitData.Length == receivedDataAsIntArray.Length){
                //Debug.Log(portName+": Parsing "+s);

                for(int i=0; i<splitData.Length; i++){
                    try{
                        receivedDataAsIntArray[i] = int.Parse(splitData[i]);
                    }catch(FormatException e){
                        receivedDataAsIntArray[i] = receivedDataAsIntArray[i];

                        Debug.LogWarning("Malformed character in string, cannot parse as int: "+e);
                    }
                }

            }else{
                Debug.LogWarning("Data: "+s+"\nMalformatted data string from :"+portName+". Contains "+splitData.Length+" params when it should have "+receivedDataAsIntArray.Length);
            }
        }
    }
}
