using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

[System.Serializable]
public class AngularControlData
{
    public bool AlwaysControl; //Set false to disable angular control when vehicle on the ground
    public float AngularVelocityForce = 80;
    public bool DeadZone; //Set true if it neccessary to check when the car is flipped
    public Vector2 DeadZoneAngle = new Vector2(90, 270);
    public float TimeToDeath = 3;
}

public class CarController2D : MonoBehaviour
{
    public GameObject Driver; //Who is driving?
    public bool Enabled = true; //Can we control the vehicle?

    public enum DriveTyp
    {
        FrontWheelDrive, BackWheelDrive, AllWheelDrive
    }
    public DriveTyp DriveType;

    public float DamperSize = 0.1f;
    public float DamperStiffness = 6;

    public float Acceleration = 100;
    public float MaxSpeed = 30;
    public float BrakingForce = 20;
    public bool AutoBrake; //Can vehicle automatically use brake when the player change moving direction?
    public float BackwardAccelerationFactor = 0.75f;
    public float BackwardMaxSpeedFactor = 0.5f;

    public bool AngularVelocityControl; //Can player rotate the vehicle?
    public AngularControlData AngularControlSettings;
    
    public float VelocityLimit = 100;

    [System.NonSerialized]
    public float CurrentEngineAcceleration;
    [System.NonSerialized] 
    public bool IsGrounded; //Is the vehicle body touches the ground?
    
    public Transform FrontWheelObj; 
    public Transform BackWheelObj;

    [System.NonSerialized]
    public Wheel FrontWheel = new Wheel();
    [System.NonSerialized]
    public Wheel BackWheel = new Wheel();

    private float _deathTimer;

    void Awake()
    {
        ConfigureWheels();
    }
    
    void ConfigureWheels()
    {
        FrontWheel.WheelObj = FrontWheelObj.gameObject;
        BackWheel.WheelObj = BackWheelObj.gameObject;

        FrontWheel.DefaultMass = FrontWheelObj.GetComponent<Rigidbody2D>().mass;
        BackWheel.DefaultMass = BackWheelObj.GetComponent<Rigidbody2D>().mass;

        FrontWheel.WheelPivot = FrontWheel.WheelObj.transform.parent;
        BackWheel.WheelPivot = BackWheel.WheelObj.transform.parent;

        FrontWheelObj.GetComponent<Wheel2D>().WheelType = Wheel2D.WheelTyp.FrontWheel;
        FrontWheelObj.GetComponent<Wheel2D>().CarController = gameObject;
        BackWheelObj.GetComponent<Wheel2D>().WheelType = Wheel2D.WheelTyp.BackWheel;
        BackWheelObj.GetComponent<Wheel2D>().CarController = gameObject;

        SetJoints(FrontWheelObj.transform, FrontWheel.WheelPivot);
        SetJoints(BackWheelObj.transform, BackWheel.WheelPivot);
    }

    void SetJoints(Transform wheel, Transform wheelPivot)
    {
        wheel.GetComponent<HingeJoint2D>().connectedBody = wheelPivot.GetComponent<Rigidbody2D>();
        wheelPivot.GetComponent<SliderJoint2D>().connectedBody = gameObject.GetComponent<Rigidbody2D>();
        wheelPivot.GetComponent<SpringJoint2D>().connectedBody = gameObject.GetComponent<Rigidbody2D>();

        SliderJoint2D sliderJoint = wheelPivot.GetComponent<SliderJoint2D>();

        sliderJoint.enableCollision = true;
        sliderJoint.connectedAnchor = new Vector2(wheelPivot.transform.localPosition.x, wheelPivot.localPosition.y);

        float damper;
        float stiffness;

        if (wheel.GetComponent<Wheel2D>().UseThisSettings)
        {
            damper = wheel.GetComponent<Wheel2D>().DamperSize;
            stiffness = wheel.GetComponent<Wheel2D>().DamperStiffness;
        }
        else
        {
            damper = DamperSize;
            stiffness = DamperStiffness;
        }

        sliderJoint.useLimits = true;
        JointTranslationLimits2D limit = new JointTranslationLimits2D();
        

        limit.min = -damper;
        limit.max = damper;
        sliderJoint.limits = limit;

        SpringJoint2D springJoint = wheelPivot.GetComponent<SpringJoint2D>();

        springJoint.enableCollision = true;
        springJoint.connectedAnchor = new Vector2(wheelPivot.transform.localPosition.x, wheelPivot.localPosition.y + (-DamperSize));

        springJoint.frequency = stiffness;
    }

    private float _lastAngularVelocity;

    void Start()
    {
        GameObject carPivot = new GameObject(gameObject.name + "Pivot");

        gameObject.transform.parent = carPivot.transform;

        FrontWheel.WheelObj.transform.parent = carPivot.transform;
        BackWheel.WheelObj.transform.parent = carPivot.transform;
        FrontWheel.WheelPivot.transform.parent = carPivot.transform;
        BackWheel.WheelPivot.transform.parent = carPivot.transform;
    }

    void FixedUpdate()
    {
        if (!Enabled)
            return;

        if ( CrossPlatformInputManager.GetButtonDown("Interact"))
        {
            Driver.SetActive(true);
            Driver.transform.position = this.transform.position;
            Driver.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 500f));
            GameObject.Find("Main Camera").gameObject.GetComponent<CameraFollowPlayer>().follow = GameObject.Find("Player").transform;
            this.gameObject.GetComponent<CarController2D>().enabled = false;
        }

        #region UserInput
        float vertical = Input.GetAxis("Vertical"); //Forward & Backward drive input
        float horizontal = Input.GetAxis("Horizontal"); //Angular velocity control input
        float space = Input.GetAxis("Jump"); //break control input
        #endregion

        if (vertical > 0)
            CurrentEngineAcceleration = Acceleration * vertical;
        else CurrentEngineAcceleration = (Acceleration * vertical) * BackwardAccelerationFactor;

        #region AWD
        if (DriveType == DriveTyp.AllWheelDrive)
        {
            if (FrontWheel.IsGrounded)
            {
                AddWheelForce(FrontWheel, FrontWheel.VectorForce, vertical);
                FrontWheel.WheelStoredForce = Mathf.Lerp(FrontWheel.WheelStoredForce, 0, Time.deltaTime * GetComponent<Rigidbody2D>().velocity.magnitude);
            }
            else
            {
                FrontWheel.WheelStoredForce = FrontWheel.CurWheelTorque * 2;
            }

            if (BackWheel.IsGrounded)
            {
                AddWheelForce(BackWheel, BackWheel.VectorForce, vertical);
                BackWheel.WheelStoredForce = Mathf.Lerp(FrontWheel.WheelStoredForce, 0, Time.deltaTime * GetComponent<Rigidbody2D>().velocity.magnitude);
            }
            else
            {
                BackWheel.WheelStoredForce = BackWheel.CurWheelTorque * 2;
            }

            AddWheelTorque(FrontWheel, vertical);
            AddWheelTorque(BackWheel, vertical);
        }
        #endregion
        #region BWD
        else if (DriveType == DriveTyp.BackWheelDrive)
        {
            if (BackWheel.IsGrounded)
            {
                AddWheelForce(BackWheel, BackWheel.VectorForce, vertical);
            }
            else
            {
                BackWheel.WheelStoredForce = BackWheel.CurWheelTorque;
            }

            AddWheelTorque(BackWheel, vertical);
        }
        #endregion
        #region FWD
        else if (DriveType == DriveTyp.FrontWheelDrive)
        {
            if (FrontWheel.IsGrounded)
            {
                AddWheelForce(FrontWheel, FrontWheel.VectorForce, vertical);
            }
            else
            {
                FrontWheel.WheelStoredForce = FrontWheel.CurWheelTorque;
            }

            AddWheelTorque(FrontWheel, vertical);
        }
        #endregion

        #region Break
        if (space != 0)
        {
            Break(FrontWheel);
            Break(BackWheel);
        }
        else
        {
            FrontWheel.WheelObj.GetComponent<Rigidbody2D>().mass = FrontWheel.DefaultMass;
            BackWheel.WheelObj.GetComponent<Rigidbody2D>().mass = BackWheel.DefaultMass;
        }
        #endregion

        #region angularVelocityControl
        if (AngularVelocityControl)
        {
            if (AngularControlSettings.DeadZone)
            {
                if ((transform.rotation.eulerAngles.z > AngularControlSettings.DeadZoneAngle.x && transform.rotation.eulerAngles.z < AngularControlSettings.DeadZoneAngle.y) && IsGrounded)
                {
                    _deathTimer += Time.deltaTime;
                    if (_deathTimer >= AngularControlSettings.TimeToDeath && (Mathf.Abs(GetComponent<Rigidbody2D>().angularVelocity - _lastAngularVelocity) < 1))
                    {
                        // >> YOUR CODE HERE (Smth like "GAME OVER")
                    }
                }
                else
                {
                    _deathTimer = 0;
                    AddAngularForce(horizontal);
                }
            }
            else
            {
                AddAngularForce(horizontal);
            }

            _lastAngularVelocity = Mathf.Abs(GetComponent<Rigidbody2D>().angularVelocity);
        }
        #endregion

        LimitVelocity();
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.collider == FrontWheelObj.GetComponent<CircleCollider2D>() || col.collider == BackWheelObj.GetComponent<CircleCollider2D>())
        {
            return;
        }
            
        IsGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        IsGrounded = false;
    }

    void AddWheelTorque(Wheel wheel, float input)
    {
        wheel.CurWheelTorque = (-wheel.WheelObj.GetComponent<Rigidbody2D>().angularVelocity * wheel.WheelObj.GetComponent<CircleCollider2D>().radius) * 0.054f;

        if (input == 0)
            return;

        if (AutoBrake)
        {
            float localVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody2D>().velocity).x;
            if (Mathf.Abs(localVelocity) > 2.5f)
            {
                if (input < 0 && localVelocity > 0)
                {
                    return;
                }
                if (input > 0 && localVelocity < 0)
                {
                    return;
                }
            }
        }

        if (Mathf.Abs(wheel.CurWheelTorque) <= MaxSpeed * 2)
        {
            wheel.WheelObj.GetComponent<Rigidbody2D>().AddTorque(-CurrentEngineAcceleration * 0.15f);
        }
        else
        {
            wheel.WheelObj.GetComponent<Rigidbody2D>().angularVelocity = Mathf.Lerp(wheel.WheelObj.GetComponent<Rigidbody2D>().angularVelocity, 0, Time.deltaTime * 2);
        }
    }

    void AddWheelForce(Wheel wheel, Vector2 forceTo, float input)
    {
        if (AutoBrake)
        {
            float localVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody2D>().velocity).x;
            if (Mathf.Abs(localVelocity) > 5f)
            {
                if (input < 0 && localVelocity > 0)
                {
                    Break(wheel);
                    return;
                }
                if (input > 0 && localVelocity < 0)
                {
                    Break(wheel);
                    return;
                }
            }
        }

        float curMaxSpeed = MaxSpeed;
        if (input < 0)
            curMaxSpeed *= BackwardMaxSpeedFactor;

        if (Mathf.Abs(wheel.WheelObj.GetComponent<Rigidbody2D>().velocity.magnitude) < curMaxSpeed)
        {
            wheel.WheelObj.GetComponent<Rigidbody2D>().AddForce(forceTo * (CurrentEngineAcceleration + wheel.WheelStoredForce));
            
        }
    }

    public void Break(Wheel wheel)
    {
        var rb = wheel.WheelObj.GetComponent<Rigidbody2D>();

        if (wheel.IsGrounded)
        {
            rb.AddForce(-rb.velocity * BrakingForce);
            rb.mass = Mathf.Lerp(rb.mass, (GetComponent<Rigidbody2D>().mass * 0.5f), Time.deltaTime * BrakingForce);
        }

        rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0, Time.deltaTime * (BrakingForce * 0.25f));
    }

    void AddAngularForce(float input)
    {
        if (input == 0)
            return;

        if (!AngularControlSettings.AlwaysControl)
        {
            if (FrontWheel.IsGrounded || BackWheel.IsGrounded)
                return;
        }

        float angVelocity = GetComponent<Rigidbody2D>().angularVelocity;
        if (angVelocity > -AngularControlSettings.AngularVelocityForce && input > 0)
            GetComponent<Rigidbody2D>().angularVelocity += -input * 30;
        if (angVelocity < AngularControlSettings.AngularVelocityForce && input < 0)
            GetComponent<Rigidbody2D>().angularVelocity += -input * 30;
    }

    void LimitVelocity()
    {
        var rb = GetComponent<Rigidbody2D>();

        if (Mathf.Abs(rb.velocity.x) > VelocityLimit)
        {
            float limitX = Mathf.Lerp(rb.velocity.x, VelocityLimit, Time.deltaTime * (Mathf.Abs(rb.velocity.x) - VelocityLimit));
            rb.velocity = new Vector2(limitX, rb.velocity.y);
        }

        if (Mathf.Abs(rb.velocity.y) > VelocityLimit)
        {
            float limitY = Mathf.Lerp(rb.velocity.y, VelocityLimit, Time.deltaTime * (Mathf.Abs(rb.velocity.y) - VelocityLimit));
            rb.velocity = new Vector2(rb.velocity.x, limitY);
        }
    }

    [System.Serializable]
    public class Wheel
    {
        public GameObject WheelObj;
        public Transform WheelPivot;
        public bool IsGrounded; //Is this wheel touches the ground?
        public Vector2 VectorForce;
        public float CurWheelTorque;
        public float WheelStoredForce;
        public float DefaultMass;
    }
}
