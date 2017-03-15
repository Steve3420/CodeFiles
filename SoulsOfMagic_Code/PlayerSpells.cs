using UnityEngine;
using System.Collections;

public class PlayerSpells : MonoBehaviour
{
    /// <summary>
    /// Delegates for each ability on each hand
    /// </summary>
    #region Delegates
    public delegate void MainSpellDelegate();
    public delegate void SecondSpellDelegate();

    public MainSpellDelegate MainSpellFunc
    {
        get { return m_MainSpellFunc;  }
        set { m_MainSpellFunc = value; }
    }

    public SecondSpellDelegate SecondSpellFunc
    {
        get { return m_SecondSpellFunc; }
        set { m_SecondSpellFunc = value; }
    }

    public MainSpellDelegate m_MainSpellFunc;
    public SecondSpellDelegate m_SecondSpellFunc;
    #endregion

    //Script member variables
    public ElementalSlot m_ElementScript;
    private PlayerScript m_PlayerScript;
    private PlayerCharges m_PlayerCharges;
    private HealthBar m_HealthScript;
    private PlayerInputScript m_InputScript;
    private PlayerTimers m_PlayerTimers;
    
    public bool FireballDistroyed = true;
    public GameObject m_FireRank1Prefab;
    public GameObject m_FireRank2Prefab;
    public GameObject m_FireRank3Prefab;
    public GameObject m_EarthRank1;
    public GameObject m_EarthRank2;
    public GameObject m_EarthRank3;
    public GameObject m_WaterSpellPrefab;
	public GameObject m_TargetPlayer;
    
    //Location for the fire ball to come out
    public GameObject m_RightHand;
    public GameObject m_LeftHand;

    public bool GlobalCooldown
    {
        get { return m_GlobalCooldown; }
        set { m_GlobalCooldown = value; }
    }

    private bool m_GlobalCooldown = true;

    //Triggers
    private float m_RightCastTime;
    private float m_LeftCastTime;
    private bool m_LeftCasted;
    private bool m_RightCasted;
    private bool m_IsCasting;

    private string JoystickNum = "";
    private string JoystickNumButton = "";
    private bool IsJoystick = false;
    
    void Start()
    {
        m_PlayerScript = GetComponent<PlayerScript>();
        m_HealthScript = GetComponent<HealthBar>();
        m_InputScript = GetComponent<PlayerInputScript>();
        m_PlayerTimers = GetComponent<PlayerTimers>();
        m_PlayerCharges = GetComponent<PlayerCharges>();

        //m_PlayerTimers.ResetGlobalCooldown();

        if (m_InputScript.PlayerLabel != null)
        {
            string Label = m_InputScript.PlayerLabel;

            int Parse = Label.IndexOf('P');

            char[] CharArray = Label.ToCharArray();

            string newLabel = CharArray[Parse + 1].ToString();

            JoystickNumButton = "joystick " + newLabel + " button";
            JoystickNum = newLabel;
            //print(JoystickNum);
            if(JoystickNum != "M")
            {
                IsJoystick = true;
            }
        }
        else
        {
            IsJoystick = false;
        }

        m_HealthScript = transform.GetComponent<HealthBar>();
    }

    void Update()
    {
        ButtonInput();
        SpellInput();
    }

    void ButtonInput()
    {
        //Checks if this player is using a joystick or mouse and keyboard
        if (IsJoystick)
        {
            // Buttons
            if (Input.GetKeyDown(JoystickNumButton + " 1"))
            {
                m_PlayerCharges.ChangeState(PlayerCharges.Element.Fire);
            }

            if (Input.GetKeyDown(JoystickNumButton + " 2"))
            {
                m_PlayerCharges.ChangeState(PlayerCharges.Element.Water);
            }

            if (Input.GetKeyDown(JoystickNumButton + " 3"))
            {
                m_PlayerCharges.ChangeState(PlayerCharges.Element.Earth);
            }
        }
        else
        {
            // Keys
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                m_PlayerCharges.ChangeState(PlayerCharges.Element.Fire);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                m_PlayerCharges.ChangeState(PlayerCharges.Element.Water);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                m_PlayerCharges.ChangeState(PlayerCharges.Element.Earth);
            }
        }
    }

    void SpellInput()
    {
        if (Time.timeScale != 0)
        {
            if (IsJoystick)
            {
                // Right Trigger
                m_RightCasted = Input.GetAxis("P" + JoystickNum + "_CastRight") * 100 <= -15;

                if (m_RightCasted == true)
                {
					if (m_IsCasting == false)
						m_PlayerScript.UpdateCastingTime ("Main", 2);
                }

                // Left Trigger
                m_LeftCasted = Input.GetAxis("P" + JoystickNum + "_CastRight") * 100 >= 15;

                if (m_LeftCasted == true)
                {
					if (m_IsCasting == false)
						m_PlayerScript.UpdateCastingTime ("Off", 2);
                }
            }
            else
            {
                //Mouse and Keyboard Controls
                if (Input.GetMouseButton(0))
                {
                    if (m_IsCasting == false)
						m_PlayerScript.UpdateCastingTime ("Main", 2);
                }
                if (Input.GetMouseButtonDown(1))
                {
                    if (m_IsCasting == false)
						m_PlayerScript.UpdateCastingTime ("Off", 2);
                }
            }
        }
    }

    void TriggerReset()
    {
        if (m_RightCastTime > 0.1f)
        {
            m_RightCastTime -= Time.deltaTime;
            m_IsCasting = true;
        }
        else
        {
            m_RightCastTime = 0;
            m_IsCasting = false;
            //m_PlayerScript.m_Animator.SetBool("RightCast", false);
            m_RightCasted = false;
        }

        if (m_LeftCastTime >= 0.1f)
        {
            m_LeftCastTime -= Time.deltaTime;
            m_IsCasting = true;
        }
        else
        {
            m_LeftCastTime = 0;
            m_IsCasting = false;
           // m_PlayerScript.m_Animator.SetBool("LeftCast", false);
            m_LeftCasted = false;
        }
    }
    
    void RotatePlayer()
    {
        //Get the rotation of where the player is looking
        Vector3 Rotation = m_PlayerScript.m_VerticalLookPivot.transform.rotation.eulerAngles;

        //Get rid of the Rotation values we do not need
        Rotation.x = 0;
        Rotation.z = 0;

        //Rotate the player toward the camera angle
        transform.eulerAngles = Rotation;
    }

    /// <summary>
    ///  FireSpells are the Offensive abilities in the game. They will allow 
    ///  you to damage other players.
    /// </summary>
    #region Fire Spells

    public void FireSpellRank1()
    {
        if(m_HealthScript.Mana >= m_HealthScript.Rank1ManaCost)
			if(m_GlobalCooldown == true && m_PlayerScript.IsCasting == false)
	            {
	                //m_PlayerScript.UpdateCastingTime(0.8f);
	                RotatePlayer();

	                //Temperaly store the fire ball that is going to be launch
	                GameObject FireBall = (GameObject)Instantiate(m_FireRank1Prefab, m_RightHand.transform.position + transform.forward * 2, transform.rotation);

	                //Add this player to the excluded player which means it wont be damaged by the fire ball
					FireBall.GetComponent<FireSpell1>().ExcludedPlayer = transform.gameObject;
					
			        FireBall.GetComponent<FireSpell1>().TargetedPlayer = m_TargetPlayer;
					
	                m_HealthScript.DecreaseMana(m_HealthScript.Rank1ManaCost);

	                //Reset the cooldown timer in the timer scripts
	                m_PlayerTimers.ResetGlobalCooldown();

	                //this bool is so we can't shoot multiple fire balls at once
	                FireballDistroyed = false;

					m_PlayerScript.FinishedCast = false;
	                //if(FireballDistroyed == true)
	            }
    }

    public void FireSpellRank2()
    {
        if (m_HealthScript.Mana >= m_HealthScript.Rank2ManaCost)
			if (m_GlobalCooldown == true && m_PlayerScript.IsCasting == false)
	            {
	                //m_PlayerScript.UpdateCastingTime(2f);
	                RotatePlayer();

					GameObject FireNova = (GameObject)Instantiate(m_FireRank2Prefab, transform.position + transform.forward * 4, transform.rotation);
	                FireNova.GetComponent<FireSpell2>().ExcludedPlayer = transform.gameObject;

	                m_HealthScript.DecreaseMana(m_HealthScript.Rank2ManaCost);
	                m_PlayerTimers.ResetGlobalCooldown();

					m_PlayerScript.FinishedCast = false;
	            }    
    }

    public void FireSpellRank3()
    {
		if (m_HealthScript.Mana >= m_HealthScript.Rank3ManaCost)
		{
			if (m_GlobalCooldown == true && m_PlayerScript.IsCasting == false)
			{
				Vector3 Pos = transform.position + new Vector3(0,15,0);
				Instantiate(m_FireRank3Prefab, Pos, transform.rotation);

				m_HealthScript.DecreaseMana(m_HealthScript.Rank3ManaCost);
				m_PlayerTimers.ResetGlobalCooldown();
				m_PlayerScript.FinishedCast = false;
			}
		}
    }
    #endregion

    /// <summary>
    ///  Water Spells are for mostly healing but also some defensive capabilities.
    /// </summary>
    #region Water Spells
    
    //Heal Over Time
    public void WaterSpellRank1()
    {
        if (m_HealthScript.Mana >= m_HealthScript.Rank1ManaCost)
            if (m_GlobalCooldown == true && m_PlayerScript.IsCasting == false)
            {
                //m_PlayerScript.UpdateCastingTime(1f);
                RotatePlayer();

                GameObject WaterAura = (GameObject)Instantiate(m_WaterSpellPrefab, transform.position, transform.rotation);
                WaterAura.transform.SetParent(transform);

                m_HealthScript.DecreaseMana(m_HealthScript.Rank1ManaCost);
                m_PlayerTimers.ResetHealOverTime();
                m_PlayerTimers.ResetGlobalCooldown();
				m_PlayerScript.FinishedCast = false;
            }
    }
    
    //Heal
    public void WaterSpellRank2()
    {
        if (m_HealthScript.Mana >= m_HealthScript.Rank2ManaCost)
            if (m_GlobalCooldown == true && m_PlayerScript.IsCasting == false)
            {
                //m_PlayerScript.UpdateCastingTime(2f);
                RotatePlayer();

                m_HealthScript.DecreaseMana(m_HealthScript.Rank2ManaCost);
                m_HealthScript.IncreaseHealth(20);
                m_PlayerTimers.ResetGlobalCooldown();
				m_PlayerScript.FinishedCast = false;
            }   
    }

    //Heal Over Time and Damage Reduction
    public void WaterSpellRank3()
    {
        if (m_HealthScript.Mana >= m_HealthScript.Rank3ManaCost)
            if (m_GlobalCooldown == true && m_PlayerScript.IsCasting == false)
            {
                //m_PlayerScript.UpdateCastingTime(3f);
                RotatePlayer();

                GameObject WaterAura = (GameObject)Instantiate(m_WaterSpellPrefab, transform.position, transform.rotation);
                WaterAura.transform.SetParent(transform);
                m_HealthScript.DecreaseMana(m_HealthScript.Rank3ManaCost);
                m_PlayerTimers.ResetDamageReduction();
                m_PlayerTimers.ResetHealOverTime();
                m_PlayerTimers.ResetGlobalCooldown();
				m_PlayerScript.FinishedCast = false;
            }
    }
    #endregion

    /// <summary>
    ///  Earth Spells are the players main defensive spells allowing to midigate damage
    /// </summary>
    #region Earth Spells

    //Temparary shield
    public void EarthSpellRank1()
    {
        if (m_HealthScript.Mana >= m_HealthScript.Rank1ManaCost)
            if (m_GlobalCooldown == true && m_PlayerScript.IsCasting == false)
            {
                //m_PlayerScript.UpdateCastingTime(2f);
                RotatePlayer();

                m_EarthRank1.GetComponent<EarthSpell1>().ActivateEarthSheild(true);
                m_HealthScript.DecreaseMana(m_HealthScript.Rank1ManaCost);
                m_PlayerTimers.ResetGlobalCooldown();

            }
    }
    
    //Create a stationary rock wall
    public void EarthSpellRank2()
    {
        if (m_HealthScript.Mana >= m_HealthScript.Rank2ManaCost)
            if (m_GlobalCooldown == true && m_PlayerScript.IsCasting == false)
            {
                //Tell the other script the player is casting
                //m_PlayerScript.UpdateCastingTime(2f);

                //Rotate the player toward the view of the camera
                RotatePlayer();

                //First we must make the object storing it localy so we can manipulate it
                GameObject RockWall = Instantiate(m_EarthRank2);

                //Make the Rock wall the same position and rotation as the player
                RockWall.transform.position = transform.position;
                RockWall.transform.rotation = transform.rotation;

                //Make the front of the Rock Wall infront of the player
                RockWall.transform.forward = transform.right;

                //Make the Rock wall spawn a little bit farther infront of the player
                RockWall.transform.position = RockWall.transform.position + (transform.forward * 3);
            
                //Finally it have completed the spell so we decrease the mana from the pool
                m_HealthScript.DecreaseMana(m_HealthScript.Rank2ManaCost);

                //Reset the global timer so the player cant spam the abilities
                m_PlayerTimers.ResetGlobalCooldown();
            }
    }

    //Call upon the earth to surround your self with rock
    public void EarthSpellRank3()
    {
        if (m_HealthScript.Mana >= m_HealthScript.Rank3ManaCost)
            if (m_GlobalCooldown == true && m_PlayerScript.IsCasting == false)
            {
                //m_PlayerScript.UpdateCastingTime(4f);
                RotatePlayer();

                m_EarthRank3.GetComponent<EarthSpell1>().ActivateEarthSheild(true);
                m_HealthScript.DecreaseMana(m_HealthScript.Rank3ManaCost);
			    m_HealthScript.DmgImmunity = true;
				m_PlayerTimers.ResetDamageImmunity();
                m_PlayerTimers.ResetGlobalCooldown();
            }
    }
    #endregion

    /// <summary>
    ///  Spells that are moded using the elements
    /// </summary>
    #region Combination Spells

    void FireEarthSpell()
    {

    }
    void FireWaterSpell()
    {

    }
    void WaterEarthSpell()
    {

    }
    void WaterFireSpell()
    {

    }
    void EarthWaterSpell()
    {

    }
    void EarthFireSpell()
    {

    }
    #endregion

    public void EmptySpell()
    {
        MainSpellFunc();
    }

    public static bool IsOdd(int value)
    {
        return value % 2 != 0;
    }
}
