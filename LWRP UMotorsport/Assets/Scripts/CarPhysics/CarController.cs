using System.Collections.Generic;
using UnityEngine;


/*
 This code manages the car's actuators and controls visual aspects of the physics
     */
public class CarController : MonoBehaviour
{
    // Visual aspects
    public List<GameObject> tailLights;
    public List<WheelCollider> motorWheels;
    public List<WheelCollider> steeringWheels;
    [Space(15)]
    // Features
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public float maxBrakeTorque;
    [Space(15)]
    // Telemetry
    public Rigidbody car;
    public float throttle;
    public float speed;
    public float steer;
    public bool brake;

    /*
    Fix wheel position with the wheel collider
         */
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    public void Start()
    {
        car.centerOfMass -= new Vector3(0f,0.25f,0f);
    }

    public void Update()
    {
        // Telemetry
        throttle = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");
        brake = Input.GetKey(KeyCode.Space);
        speed = car.velocity.magnitude * 3.6f;

        // Turn on the brake lights
        foreach (GameObject tl in tailLights)
        {
            tl.GetComponent<Renderer>().material.SetColor("_EmissionColor", brake ? new Color(0.5f, 0.111f, 0.111f) : Color.black);
        }
    }

    public void FixedUpdate()
    {
        float motorTorque = maxMotorTorque * throttle;
        float steering = maxSteeringAngle * steer;
        float brakeTorque = maxBrakeTorque * (brake ? 1 : 0);

        // Manages the car's acceleration and braking
        foreach (WheelCollider wheel in motorWheels)
        {
            wheel.motorTorque = motorTorque;
            wheel.brakeTorque = brakeTorque;
            ApplyLocalPositionToVisuals(wheel);
        }

        // Manages the steering and braking of the car
        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = steering;
            wheel.brakeTorque = brakeTorque;
            ApplyLocalPositionToVisuals(wheel);
        }
    }
}
