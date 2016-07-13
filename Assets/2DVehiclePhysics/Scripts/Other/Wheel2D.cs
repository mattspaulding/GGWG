using UnityEngine;
using System.Collections;

public class Wheel2D : MonoBehaviour
{
    public enum WheelTyp
    {
       FrontWheel, BackWheel
    }
    [System.NonSerialized]
    public WheelTyp WheelType;

    public bool UseThisSettings; //Switch to true, if you want to configure this wheel separately
    public float DamperSize = 0.1f;
    public float DamperStiffness = 6;

    [System.NonSerialized]
    public GameObject CarController;

    void Awake()
    {
        
    }

	void OnCollisionStay2D(Collision2D col)
    {
        if (col.collider == CarController.GetComponent<PolygonCollider2D>())
            return;

        if (WheelType == WheelTyp.FrontWheel)
        {
            CarController.GetComponent<CarController2D>().FrontWheel.IsGrounded = true;
            Vector2 forceTo = Quaternion.Euler(0, 0, -90)*col.contacts[0].normal.normalized;
            CarController.GetComponent<CarController2D>().FrontWheel.VectorForce = forceTo;

            //Debug.DrawRay(col.contacts[0].point, Quaternion.Euler(0, 0, -90) * col.contacts[0].normal, Color.green, 1);  //Draw wheel force direction line
            
        }
        else
        {
            CarController.GetComponent<CarController2D>().BackWheel.IsGrounded = true;
            Vector2 forceTo = Quaternion.Euler(0, 0, -90) * col.contacts[0].normal.normalized;
            CarController.GetComponent<CarController2D>().BackWheel.VectorForce = forceTo;

            //Debug.DrawRay(col.contacts[0].point, Quaternion.Euler(0, 0, -90) * col.contacts[0].normal, Color.green, 1); //Draw wheel force direction line
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (WheelType == WheelTyp.FrontWheel)
        {
            CarController.GetComponent<CarController2D>().FrontWheel.IsGrounded = false;
            CarController.GetComponent<CarController2D>().FrontWheel.VectorForce = Vector2.zero;
        }
        else
        {
            CarController.GetComponent<CarController2D>().BackWheel.IsGrounded = false;
            CarController.GetComponent<CarController2D>().BackWheel.VectorForce = Vector2.zero;
        }
    }
}
