using UnityEngine;
using System.Collections;

public class SpaceShipBehaviour : BaseShipBehaviour
{
    public GameObject m_StartPlanet;
    public GameObject m_CameraMan;
    public GameObject m_Target;
    public MissionManager m_MissionMan;
    
	public float Money
	{
		get { return m_MoneyValue;}
		set { m_MoneyValue = value;}
	}

	public bool InputControl
	{
		get { return m_InputControl;}
		set { m_InputControl = value;}
	}

	private EnemyBehaviour m_EnemyBehaviour;
	private bool m_InputControl = false;
    private GameObject m_LastPosition;
    private RaycastHit m_Hit;
    private int m_SpaceLayerMask;
	private float m_MoneyValue = 0;
	private float TimetilFire = 5;
	private float m_Angle =0;

    void Start()
    {
        m_Destination = null;
        m_Current = m_StartPlanet;

        m_EnemyBehaviour = m_Target.GetComponent<EnemyBehaviour>();
        m_SpaceLayerMask = ~LayerMask.GetMask("Default", "Ignore Raycast","Exception", "Player");
        StartCoroutine(Orbit_c());
    }

    void Update()
    {
        if (m_InputControl == true)
            UpdateRayCast();
    }
    
    void UpdateRayCast()
    {
        RaycastHit vHit = new RaycastHit();
        Ray vRay = m_CameraMan.GetComponent<CameraManager>().SpaceCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(vRay, out vHit, 1000, m_SpaceLayerMask))
        {
            string name = vHit.transform.name;
            string[] names = name.Split('_');

            if (names[0] == "Planet")
            {
                if (Input.GetMouseButtonDown(0))
                {
                    float Radius = vHit.transform.gameObject.GetComponent<PlanetBehaviour>().OrbitRadius;
                    StartCoroutine(CheckDestination_c(vHit.transform.gameObject, Radius));
                }
            }      
        }
    }
    
    public void SetCombat()
    {
        InCombat = true;
        Vector3 Position = m_Target.transform.position - transform.position;
        Position.Normalize();
        transform.eulerAngles = new Vector3(0, Mathf.Atan2(Position.x, Position.z) * Mathf.Rad2Deg + 90, 0);
    }

    public void StartAttack()
    {
        if(m_Target !=null)
        {
            m_Target.GetComponent<EnemyBehaviour>().TakeDamage(20);
            TimetilFire = 5;
            StartCoroutine(Attack_c());
        }
        else
        {
            InCombat = false;
        }
    }

    public Vector3 CalculatePosition(float angle, float length)
    {
        float newX = m_Current.transform.position.x + length * Mathf.Cos(angle);
        float newY = m_Current.transform.position.y;
        float newZ = m_Current.transform.position.z + length * Mathf.Sin(angle);

        return new Vector3(newX, newY, newZ);
    }

    public IEnumerator CheckDestination_c(GameObject Destination, float Radius )
    {
        float DistanceOne = Vector3.Distance(transform.position, Destination.transform.position);
        float DistanceTwo = Vector3.Distance(Current.transform.position, Destination.transform.position);

        while(DistanceTwo < DistanceOne)
        {
            DistanceOne = Vector3.Distance(transform.position, Destination.transform.position);
            DistanceTwo = Vector3.Distance(Current.transform.position, Destination.transform.position);

            yield return null;
        }
        
        IsFlying = true;
        SetDesitnation(Destination, Radius);

    }

    public IEnumerator Attack_c()
    {
        for (float i = 1; TimetilFire >= 0; i -= 0.1f)
        {
            TimetilFire -= Time.deltaTime;
            yield return null;
        }

        yield return null;
        
        StartAttack();
    }

    public IEnumerator Orbit_c()
    {
        m_Angle = 0;
        Vector3 Direction = m_Current.transform.position - transform.position;
        float Length = m_Current.GetComponent<PlanetBehaviour>().OrbitRadius;
        m_Angle = Mathf.Atan2(Vector3.forward.z - Direction.z, Vector3.forward.x - Direction.x);

        while (IsFlying == false)
        {
            m_MissionMan.CheckPlayer();

            m_Angle += m_Speed * Time.deltaTime;

            transform.position = CalculatePosition(m_Angle, Length);

            transform.LookAt(m_Current.transform);

            transform.Rotate(new Vector3(0,180,0));

            yield return null;
        }
        Debug.Log("Finished Orbit");
    }

    protected override IEnumerator MoveShip_c(float distanceMax)
    {
        float Distance = Vector3.Distance(transform.position, m_Destination.transform.position);
        m_LastPosition = m_Destination;

        while (Distance >= distanceMax + 5)
        {
            if (m_InCombat == false)
            {
                Vector3 Direction = m_Destination.transform.position - transform.position;
                Direction.Normalize();
                transform.eulerAngles = new Vector3(0, Mathf.Atan2(Direction.x, Direction.z) * Mathf.Rad2Deg + 90, 0);

                Distance = Vector3.Distance(transform.position, m_Destination.transform.position);

                Vector3 newPos = m_Destination.transform.position - Direction * m_Destination.transform.GetComponent<PlanetBehaviour>().OrbitRadius;

                //Gets the value between the positions and uses rigidbody to move
                Vector3 Position = Vector3.MoveTowards(transform.position, newPos, 0.2f);
                transform.GetComponent<Rigidbody>().MovePosition(Position);
            }
            yield return null;
        }

        //Debug.Log("Arrived at Planet");
        m_Current = m_Destination;
        m_IsFlying = false;
        StartCoroutine(Orbit_c());
    }
}
