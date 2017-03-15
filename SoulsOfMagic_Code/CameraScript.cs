using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{
    public GameObject m_Camera;
    public GameObject m_FollowPosition;
    public GameObject m_Player;
    public float m_FollowSpeed;
	public float m_MaxSpeed = 100;
	public float m_TurnSpeed;
	public bool m_IsDead = false;

    private Vector3 m_Offset;
    private bool m_CameraRotate;
    private float m_InputTimer;
    bool m_JoystickControl;

	void Start ()
    {
		m_IsDead = false;

        string playerLabel = m_Player.GetComponent<PlayerInputScript>().PlayerLabel;
        
        if(playerLabel == "MK_")
        {
            m_JoystickControl = false;
            //Debug.Log("Mouse Control is active");
        }
        else
        {
            m_JoystickControl = true;
            //Debug.Log("Joystick Control is active");
        }
    }

	void Update()
	{
		if (m_IsDead == false)
		{
			//Create our local variables to store the values while we change them
			float ValueX = transform.eulerAngles.x;
			float newValue = transform.eulerAngles.x;

			//Because the input is to fast for Mathf.Clamp() we check the position of
			//the camera and return it to the right area of vision
			if (ValueX >= 60 && ValueX <= 180)
				ValueX = 50;

			if (ValueX >= 180 && ValueX <= 270)
				ValueX = 300;

			//Check to make sure the input did not flip the camera
			if (transform.eulerAngles.z <= 182 && transform.eulerAngles.z >= 178)
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);

			//Since we know the camera is not in the right area we want to clamp
			//the value so that the camera can not look to far down and to far up
			if (ValueX <= 360 && ValueX >= 270)
				newValue = Mathf.Clamp(ValueX, 300, 360);

			if (ValueX >= 0 && ValueX <= 60)
				newValue = Mathf.Clamp(ValueX, 0, 50);

			//Commit the changes to our rotation since we are now done limiting
			//the camera
			transform.eulerAngles = new Vector3(newValue, transform.eulerAngles.y, transform.eulerAngles.z);
		}
	}

	void FixedUpdate ()
    {
		if (m_IsDead == false)
		{
			//WE calculate the distance between the camera and the player position
			//so we can lerp around without taking the camera to the players position
			m_Offset = transform.position - m_Player.transform.position;

			//We only need the y value of distance so we set the other values of
			//the vector to zero
			m_Offset.z = 0;
			m_Offset.x = 0;

			//Calculate the new follow position using the Vector3 slerp so we can
			//have a smooth transition from current position to the new position
			transform.position = Vector3.Slerp(transform.position, m_Player.transform.position + m_Offset, m_FollowSpeed * Time.deltaTime);
        
			//Get the player label from the script so we know what player it is 
			string playerLabel = m_Player.GetComponent<PlayerInputScript>().PlayerLabel;

			//Set up local variable so we can see changes before commiting them
			//to the character
			Vector3 InputAxis = Vector3.zero;

			if (m_JoystickControl)
			{
				InputAxis = new Vector3(-Input.GetAxis(playerLabel + "RJoystickVertical") * m_TurnSpeed / 2, Input.GetAxis(playerLabel + "RJoystickHorizontal") * m_TurnSpeed, 0);
			}
			else
			{
				InputAxis = new Vector3(-Input.GetAxis(playerLabel + "MouseY") * m_TurnSpeed / 2, Input.GetAxis(playerLabel + "MouseX") * m_TurnSpeed, 0);
			}
        
			transform.eulerAngles += InputAxis;
		}
    }

    public void CameraLock()
    {

    }
}
