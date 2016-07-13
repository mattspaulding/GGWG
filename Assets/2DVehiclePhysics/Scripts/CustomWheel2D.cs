using UnityEngine;
using System.Collections;

public class CustomWheel2D : MonoBehaviour
{
    public Rigidbody2D Parent;
    public Rigidbody2D Wheel;
    public float DamperSize = 0.1f;
    public float DamperStiffness = 6;
    
	void Awake() 
    {
        ConfigureWheel();
	}
	
	void ConfigureWheel()
	{
        gameObject.GetComponent<SliderJoint2D>().connectedBody = Parent;
        gameObject.GetComponent<SpringJoint2D>().connectedBody = Parent;
	    Wheel.GetComponent<HingeJoint2D>().connectedBody = GetComponent<Rigidbody2D>();
        
        SliderJoint2D sliderJoint = gameObject.GetComponent<SliderJoint2D>();

        sliderJoint.enableCollision = true;
        sliderJoint.connectedAnchor = new Vector2(transform.localPosition.x, transform.localPosition.y);

        sliderJoint.useLimits = true;
        JointTranslationLimits2D limit = new JointTranslationLimits2D();
        limit.min = -DamperSize;
        limit.max = DamperSize;
        sliderJoint.limits = limit;

        SpringJoint2D springJoint = gameObject.GetComponent<SpringJoint2D>();

        springJoint.enableCollision = true;
        springJoint.connectedAnchor = new Vector2(transform.localPosition.x, transform.localPosition.y + (-DamperSize));
        
        springJoint.frequency = DamperStiffness;

	    gameObject.transform.parent = null;
	    Wheel.transform.parent = null;

	}

	void Update () 
    {
	
	}
}
