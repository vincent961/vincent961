using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
public class SteeringWheelController : MonoBehaviour
{
     InputDevice RightController;
    InputDevice LeftController;
    public GameObject rightHand;
    private Transform rightHandOriginalParent;
    private bool rightHandOnWheel= false;

    public GameObject leftHand;
    private Transform leftHandOriginalParent;
    private bool leftHandOnWheel= false;

    public Transform[] snapPositions;
    private int numberOfHandsOnWheel = 0;

    public GameObject Vehicle;
    private Rigidbody VehicleRigidBody;

    public float currentSteeringWheelRotation = 0;

    [SerializeField] float turnDampening = 250;
   

    public Transform directionalObject;
    // Start is called before the first frame update
    void Start()
    {
    List<InputDevice> devices = new List<InputDevice>();
    InputDeviceCharacteristics ControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
    InputDevices.GetDevicesWithCharacteristics(ControllerCharacteristics, devices);
    RightController = devices[0];
    ControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
    InputDevices.GetDevicesWithCharacteristics(ControllerCharacteristics, devices);
    LeftController = devices[0];
    VehicleRigidBody = Vehicle.GetComponent<Rigidbody>();   
    }

    // Update is called once per frame
    void Update()
    {
        ReleaseHandsFromWheel();

        ConvertHandRotationToSteeringWheelRotation();

        TurnVehicle();

        currentSteeringWheelRotation = transform.rotation.eulerAngles.z;
    }

    private void TurnVehicle(){
        var turn = -transform.rotation.eulerAngles.z;
        if(turn < -350)
        {
            turn = turn +360;
        }
        VehicleRigidBody.MoveRotation(Quaternion.RotateTowards(Vehicle.transform.rotation, Quaternion.Euler(0,turn,0),Time.deltaTime * turnDampening));
    }
    private void ConvertHandRotationToSteeringWheelRotation()
    { 
            if(rightHandOnWheel == true &&leftHandOnWheel== false)
            {
                Quaternion newRot = Quaternion.Euler(0,0, rightHandOriginalParent.transform.rotation.eulerAngles.z);
                directionalObject.rotation = newRot;
                transform.parent = directionalObject;
            }
            else if (rightHandOnWheel == false && leftHandOnWheel == true)
            {
                Quaternion newRot = Quaternion.Euler(0,0, leftHandOriginalParent.transform.rotation.eulerAngles.z);
                directionalObject.rotation = newRot;
                transform.parent = directionalObject;
            }
            else if (rightHandOnWheel == true && leftHandOnWheel == true)
            {
                Quaternion newRotLeft = Quaternion.Euler(0,0, leftHandOriginalParent.transform.rotation.eulerAngles.z);
                Quaternion newRotRight = Quaternion.Euler(0,0, rightHandOriginalParent.transform.rotation.eulerAngles.z);
                Quaternion finalRot = Quaternion.Slerp(newRotLeft,newRotRight, 1.0f/ 2.0f);
                directionalObject.rotation = finalRot;
                transform.parent=directionalObject;
            }
    }
  private void OnTriggerStay(Collider other){
        LeftController.TryGetFeatureValue(CommonUsages.gripButton, out bool gripLeft);
        RightController.TryGetFeatureValue(CommonUsages.gripButton, out bool gripRight);
        bool grip = gripLeft || gripRight;
        if (other.CompareTag("PlayerHand"))
     {
         if(rightHandOnWheel == false && grip){
             PlaceHandOnWheel(ref rightHand, ref rightHandOriginalParent, ref rightHandOnWheel);
         }
         if(leftHandOnWheel == false && grip){
             PlaceHandOnWheel(ref leftHand, ref leftHandOriginalParent, ref leftHandOnWheel);
         }
     }

 }

    private void PlaceHandOnWheel(ref GameObject hand, ref Transform originalParent, ref bool handOnWheel){
        var shortestDistance = Vector3.Distance(snapPositions[0].position,hand.transform.position);
        var bestSnap = snapPositions[0];

        foreach(var snapPositions in snapPositions)
        {
            if (snapPositions.childCount ==0)
            {
                var distance = Vector3.Distance(snapPositions.position, hand.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    bestSnap = snapPositions;
                }
            }
        }
        originalParent = hand.transform.parent;
        hand.transform.parent = bestSnap.transform;
        hand.transform.position = bestSnap.transform.position;
        handOnWheel = true;
        numberOfHandsOnWheel++;
  }

    private void ReleaseHandsFromWheel()
    {
    }  
      
}