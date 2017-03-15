using UnityEngine;
using System.Collections;

public class BaseShipBehaviour : MonoBehaviour
{
    protected Vector3 m_LastPos;
    protected Vector3 m_StartingPos;
    protected GameObject m_Destination;
    protected GameObject m_Current;

    protected float m_Speed = 1;
    protected bool m_IsFlying = false;
    protected bool m_InCombat = false;
    protected int m_Health;

    public GameObject Destination
    {
        get { return m_Destination; }
        set { m_Destination = value; }
    }

    public GameObject Current
    {
        get { return m_Current; }
        set { m_Current = value; }
    }

    public int Health
    {
        get { return m_Health; }
        set { m_Health = value; }
    }

    public bool InCombat
    {
        get { return m_InCombat; }
        set { m_InCombat = value; }
    }

    public bool IsFlying
    {
        get { return m_IsFlying; }
        set { m_IsFlying = value; }
    }

    public virtual void TakeDamage(int damage)
    {

    }

    protected virtual void DoDamage(int damage)
    {

    }

    public void SetDesitnation(GameObject destination, float distanceFromTarget)
    {
        m_Destination = destination;
        StartCoroutine(MoveShip_c(distanceFromTarget));
    }

    protected virtual IEnumerator MoveShip_c(float distanceMax)
    {
        float Distance = Vector3.Distance(transform.position, m_Destination.transform.position);

        while (Distance > distanceMax)
        {
            if(m_InCombat == false)
            {
                Distance = Vector3.Distance(transform.position, m_Destination.transform.position);

                //Get the direction and set the roation
                Vector3 Position = m_Destination.transform.position - transform.position;
                Position.Normalize();
                transform.eulerAngles = new Vector3(0, Mathf.Atan2(Position.x, Position.z) * Mathf.Rad2Deg + 90, 0);

                //Gets the value between the positions and uses rigidbody to move
                Position = Vector3.MoveTowards(transform.position, m_Destination.transform.position, 0.1f);
                transform.GetComponent<Rigidbody>().MovePosition(Position);
            }
            yield return null;
        }

        transform.position = m_Destination.transform.position;
        m_Current = m_Destination;
        m_IsFlying = false;
        Debug.Log("Arrived at: " + m_Current);
    }
}
