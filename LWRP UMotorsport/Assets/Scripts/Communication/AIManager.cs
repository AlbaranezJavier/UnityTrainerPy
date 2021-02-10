using UnityEngine;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections;
using System;
using System.Linq;
using System.Globalization;


/*
 This script controls and manages the vehicle's information and actuators.
     */
public class AIManager : ServerUser
{
    #region Variables
    public static Queue<string> actionsQueue = new Queue<string>();
    public static BlockingCollection<byte[]> dataQueue = new BlockingCollection<byte[]>(new ConcurrentQueue<byte[]>(), 1);
    public CarController telemetryCar;
    public int numberCameras = 1;
    public int decimalAccuracy = 1000;
    public string receivedData;

    private byte[] _numberCameras;
    private byte[] _decimalAccuracy;
    private CultureInfo culture = new CultureInfo("en-EN");
    #endregion

    public void Start()
    {
        _numberCameras = BitConverter.GetBytes(numberCameras);
        _decimalAccuracy = BitConverter.GetBytes(decimalAccuracy);
        StartCoroutine(AIBehavior());
    }

    public override void Receive_msg(string msg)
    {
        actionsQueue.Enqueue(msg);
    }

    public override byte[] Send_msg()
    {
        return dataQueue.Take();
    }

    /*
     Collects the image from the game window and stores it in an array of bytes
         */
    private byte[] GetScreenImage()
    {
        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
        tex.Apply();
        byte[] bytes = tex.GetRawTextureData();
        //byte[] bytes = tex.EncodeToJPG();
        //File.WriteAllBytes(Application.dataPath + "/Images/1.jpg", bytes);
        Destroy(tex);
        return bytes;
    }

    /*
     If there is any pending action, collects the current status of the simulation and stores it in the data queue that is sent to the client
         */
    private IEnumerator AIBehavior()
    {
        while (true)
        {
            if (actionsQueue.Count > 0)
            {
                AIDo();
                yield return new WaitForEndOfFrame();
                byte[] imageWidth = BitConverter.GetBytes(Screen.width);
                byte[] imageHeight = BitConverter.GetBytes(Screen.height);
                byte[] throttle = BitConverter.GetBytes((int)(telemetryCar.throttle * decimalAccuracy));
                byte[] speed = BitConverter.GetBytes((int)telemetryCar.speed);
                byte[] steer = BitConverter.GetBytes((int)(telemetryCar.steer * decimalAccuracy));
                byte[] brake = BitConverter.GetBytes(telemetryCar.brake);
                //msg = image parameters + number of cameras + image size + telemetry (decimalAccuracy, throttle, speed, steer, brake) 4 bytes except brake 1 + images
                dataQueue.Add(imageWidth.Concat(imageHeight).Concat(_numberCameras).Concat(_decimalAccuracy).Concat(throttle).Concat(speed).Concat(steer).Concat(brake).Concat(GetScreenImage()).ToArray());
            }
            yield return null;
        }
    }

    /*
     Performs the action received, the data could be [throttle (0,1), brake(0,1), steer(-1,1)]
     */
    private void AIDo()
    {
        receivedData = actionsQueue.Dequeue();
        if (receivedData.Equals("ready"))
        {
            telemetryCar.AIGetControl = true;
        }
        else {
            telemetryCar.newAIAction(Array.ConvertAll(receivedData.Split(' '), i => float.Parse(i, CultureInfo.InvariantCulture)));
        }
    }
 }
