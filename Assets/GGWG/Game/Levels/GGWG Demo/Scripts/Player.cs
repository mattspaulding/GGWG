using UnityEngine;
using System.Collections;
using Spine.Unity;
using UnityStandardAssets.CrossPlatformInput;
using System.Linq;
using UnityEngine.Audio;

public class Player : MonoBehaviour
{
    //Public
    public CameraFollowPlayer mainCamera;
    // Camera follow script used to adjust camera when climbing ledges
    public float walkSpeed = 2;
    //Character walking speed.
    public float runSpeed = 7;
    // Character running speed.
    public float swimSpeed = 2;
    //Character swimming speed.
    public float pullSpeed = 2;
    //Character pulling object movement speed.
    public float pushSpeed = 2;
    // Character pushing object movement speed.
    public float sprintSpeed = 10;
    //Character sprinting speed.
    public float sprintTimer = 1.5f;
    //Time taken for character to burst in to a sprint after running.
    public float skidSpeedReduce = 1.0f;
    //Value for how fast to reduce speed to 0 when skidding.
    public float jumpHeight = 10;
    //Character jump height.
    public float landingSpeedReduce = 2.0f;
    //Speed divisor for landing, e.g. a value of 2 will half the characters movement speed when in the landing state.
    public float landingSpeedReduceTime = 4.0f;
    //time multiplier for landing, a higher number will return the character to normal movement speed quicker.
    public float machineGunFireRate = 0.3f;
    //Machine gun fire rate (button 5)
    public float pistolFireRate = 0.3f;
    //Pistol fire rate (button 2)
    public float gunFireRate = 0.4f;
    //Shotgun fire rate (button 3)
    public float skinChangeTimer = 1.0f;
    //Time taken to swap skins in automatic mode (button Y)
    public float pushPullEaseTimer = 0.6f;
    // Time base for push and pull animations so we can apply a fake ease in and out to the player speed.
    public float terminalVelocityWall = -10f;
    // Terminal velocity when on a wall, we don't want him getting skin burn when going down walls!
    public bool isFollower = false;
    public bool isEnemy = false;
    private float originalFollowPosition = 0;
    public float FollowPosition = 0;
    public int health = 10;
    public bool isActive = false;
    public bool isDoubleJump = false;
    public bool isWallJump = false;
    public bool isAiControlled = false;


    public AudioMixer audioMixer;
    public AudioMixerSnapshot phantomSnapshot;
    public AudioMixerSnapshot dontCryJenniferSnapshot;
    public AudioMixerSnapshot killCounterSnapshot;
    public AudioMixerSnapshot bossSnapshot;
    public AudioMixerSnapshot descendingOnTheLabSnapshot;
    public AudioMixerSnapshot quietSnapshot;
    public AudioSource musicPlayer;


    public Transform GroundCheck;
    public Transform WallCheck;
    public Transform LadderCheck;
    public Transform BulletPos;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public LayerMask ladderLayer;
    public LayerMask interactiveLayer;
    public LayerMask swimLayer;
    public GameObject bulletPrefab;
    public BoxCollider2D swordHitbox;
    public Transform[] spawnPoints;
    public int zoomSize = 15;


    //Private
    public enum PlayerStates
    {
        idle,
        running,
        sprinting,
        walking,
        crouchIdle,
        crouchWalk,
        jumping,
        doubleJump,
        falling,
        rolling,
        landing,
        wallIdle,
        wallJump,
        ladderIdle,
        ladderClimbUp,
        ladderClimbDown,
        pushIdle,
        push,
        pullIdle,
        pull,
        swim,
        swimIdle,
        edgeClimb,
        edgeIdle,
        skid,
        celebration
    }

    ;

    public enum CombatStates
    {
        unarmed,
        melee,
        pistol,
        gun,
        machineGun
    }

    ;

    private PlayerStates currentState, previousState;
    public CombatStates combatState = CombatStates.unarmed;
    /*private string[] skins = {"StumpyPete", "BeardyBuck", "BuckMatthews", "ChuckMatthews", "Commander-Darkstrike", "Commander-Firestrike", "Commander-Icestrike", "Commander-Stonestrike",
		"DuckMatthews", "Dummy", "Fletch", "GabrielCaine", "MetalMan", "MetalMan-Blue", "MetalMan-Red", "MetalMan-Green", "PamelaFrost",
		"PamelaFrost-02", "PamelaFrost-03", "PamelaFrost-04", "PamelaFrost-05", "TruckMatthews", "TurboTed", "TurboTed-Blue", "TurboTed-Green", "YoungBuck"};*/
    private string[] skins = {"StumpyPete", "Assassin", "PamelaFrost-05", "BuckMatthews",  "Commander-Darkstrike", "ChuckMatthews",  "TurboTed-Blue", "Commander-Icestrike",  "MetalMan-Green", "PamelaFrost-04", "Commander-Stonestrike",
        "DuckMatthews", "Dummy", "Fletch", "GabrielCaine", "MetalMan",  "PamelaFrost", "PamelaFrost-02", "TurboTed-Green",  "MetalMan-Blue", "PamelaFrost-03",  "BeardyBuck", "TruckMatthews", "TurboTed",
        "Commander-Firestrike", "MetalMan-Red", "YoungBuck"
    };
    private bool jumpFrames = true;
    private bool wallFrames = true;
    private bool ladderFrames = true;
    private bool ladderToGroundFrames = true;
    private bool isGrounded = false;
    private bool allowMovement = true;
    private bool isSwim = false;
    private bool wallTouch = false;
    private bool ladderTouch = false;
    private bool interactiveTouch = false;
    private bool flipEnabled = true;
    private bool skinChangeToggle = false;
    private bool mouseEnabled = true;
    private bool block = false;
    private bool pushPullState = false;
    private float currentSkinChangeTime = 1.0f;
    private float landingTimeSpeed = 1.0f;
    private float jumpSpeed = 0;
    private float originJumpSpeed = 0;
    private float bodyGravity = 1;
    private float currentSprintTimer = 0;
    private float currentMachineGunFireRate = 0;
    private float currentPistolFireRate = 0;
    private float currentGunFireRate = 0;
    private float currentPushPullTimer = 0;
    private int currentSpawnPoint = 0;
    private int skinCount = 0;
    private Vector2 previousVelocity = Vector2.zero;
    public Vector2 velocity = Vector2.zero;
    public Spine.Unity.SkeletonAnimation animation;
    private Rigidbody2D currentInteractiveObject;
    private Rigidbody2D body;
    private Quaternion flippedRotation = Quaternion.Euler(0, 180, 0);
    private Spine.Bone leftShoulder, rightShoulder, neck;
    private bool doMoveLeft;
    private bool doMoveRight;
    private bool aiMoveLeft;
    private bool aiMoveRight;
    private bool doWalkLeft;
    private bool doWalkRight;
    private bool doJump;
    private bool doFire;
    private bool doStartFire;
    private bool doEndFire;
    private bool previousDoFire;
    private float hitMultiplier;
    private bool isDead;
    private bool isPunch;
    private bool isZoomIn;
    private CameraFollowPlayer cameraFollowPlayer;
    private Vector3 cameraPosition;
    private float punch1time;
    private float punch2time;

    void Start()
    {
        originalFollowPosition = FollowPosition;
        currentSprintTimer = sprintTimer;
        animation = GetComponent<Spine.Unity.SkeletonAnimation>();
        body = GetComponent<Rigidbody2D>();
        bodyGravity = body.gravityScale;
        neck = animation.skeleton.FindBone("neck");
        leftShoulder = animation.skeleton.FindBone("arm_upper_far");
        rightShoulder = animation.skeleton.FindBone("arm_upper_near");
        animation.UpdateLocal += HandleUpdateLocal;
        cameraFollowPlayer = Camera.main.GetComponent<CameraFollowPlayer>();

        if (this.name.StartsWith("Player"))
        {
            animation.skeleton.SetSkin("YoungBuck");
            animation.state.SetAnimation(0, "hitBig", false);
            //GetComponent<BoxCollider2D>().enabled = false;
            //GetComponent<CircleCollider2D>().enabled = false;
            // animation.state.SetAnimation(1, "reset", true);

            // Set this to true for cave routine.
            //isDead = true;
            //StartCoroutine(Cave());
          
        }
        else
        {
            //Random Skin on startup
            skinCount = Random.Range(0, skins.Length - 1);
            animation.skeleton.SetSkin(skins[skinCount]);
            skinCount++;
        }

    }

    private IEnumerator Cave()
    {
        // Wake up laying down
        animation.state.SetAnimation(0, "hitBig", false);
        this.allowMovement = false;
        this.isDead = true;
        GameObject.Find("CaveDialog").GetComponent<AudioSource>().Play();
        GameObject.Find("CaveGuard0").GetComponent<Player>().velocity.x = -1f;
        yield return new WaitForSeconds(2);
        // Kneel
        this.isDead = false;
        GameObject.Find("CaveDialog").GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(2);
        // Move camera to guard
        cameraFollowPlayer.isFollowPlayer = false;
        cameraPosition = new Vector3(-570.45f, -46.57f, -10f);
        GameObject.Find("CaveDialog").GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(2);
        // Move camera to player
        cameraFollowPlayer.isFollowPlayer = true;
        yield return new WaitForSeconds(2);
        // Stand up
        this.isActive = true;
        this.allowMovement = true;
        this.SetCurrentState(Player.PlayerStates.idle);
        GameObject.Find("CaveDialog").GetComponent<AudioSource>().Play();
        GameObject.Find("CaveGuard0").GetComponent<Player>().velocity.x = 1f;


    }

    private IEnumerator CoughUpItem()
    {
        this.transform.FindChild("Item").gameObject.SetActive(true);
        this.transform.FindChild("Item").GetComponent<Rigidbody2D>().velocity = new Vector2(-4f, 5f);

        yield return new WaitForSeconds(1);
        this.transform.FindChild("Item").GetComponent<BoxCollider2D>().enabled = true;
        this.transform.FindChild("Item").GetComponent<CircleCollider2D>().enabled = true;



    }



    //All local bone rotations need to be called in UpdateLocal.
    void HandleUpdateLocal(ISkeletonAnimation skeletonRenderer)
    {
        if (!block)
        {
            Aim();
            AiAim();
        }
    }


    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!isFollower && !isEnemy)
        {
            if (collider.name.StartsWith("Tractor"))
            {
                GameObject.Find("Tractor").transform.Find("Enter").gameObject.SetActive(true);
            }

            if (collider.name.StartsWith("Bigfoot"))
            {
                GameObject.Find("Bigfoot").transform.Find("Enter").gameObject.SetActive(true);
            }

            if (collider.name.StartsWith("ZoomIn"))
            {
                // isZoomIn = true;
                zoomSize = System.Int32.Parse(collider.name.Split('-')[1]);
            }

            if (collider.name.StartsWith("QuietZone"))
            {
                quietSnapshot.TransitionTo(15f);
            }
            if (collider.name.StartsWith("PhantomZone"))
            {
                float vol;
                audioMixer.GetFloat("PhantomVolume", out vol);
                if (vol == -80f)
                {
                    ResetMusic();
                    phantomSnapshot.TransitionTo(0f);
                }
                else
                    phantomSnapshot.TransitionTo(5f);

            }
            if (collider.name.StartsWith("DontCryJenniferZone"))
            {
                float vol;
                audioMixer.GetFloat("DontCryJenniferVolume", out vol);
                if (vol == -80f)
                {
                    ResetMusic();
                    dontCryJenniferSnapshot.TransitionTo(0f);
                }
                else
                    dontCryJenniferSnapshot.TransitionTo(5f);

            }
            if (collider.name.StartsWith("BossZone"))
            {
                bossSnapshot.TransitionTo(1f);
            }
            if (collider.name.StartsWith("DescendingOnTheLabZone"))
            {
                float vol;
                audioMixer.GetFloat("DescendingOnTheLabVolume", out vol);
                if (vol == -80f)
                {
                    ResetMusic();
                    descendingOnTheLabSnapshot.TransitionTo(0f);
                }
                else
                    descendingOnTheLabSnapshot.TransitionTo(5f);
            }
            if (collider.name.StartsWith("KillCounterZone"))
            {
                float vol;
                audioMixer.GetFloat("KillCounterVolume", out vol);
                if (vol == -80f)
                {
                    ResetMusic();
                    killCounterSnapshot.TransitionTo(0f);
                }
                else
                    killCounterSnapshot.TransitionTo(5f);
            }




        }

        {

            //if (isFollower)
            //{
            //    if (collider.tag.Equals("Player"))
            //    {
            //        isActive = true;
            //    }
            //}

            if (isFollower)
            {
                if (collider.tag.Equals("Bullet") && !isActive)
                {
                    isActive = true;
                    SetCurrentState(Player.PlayerStates.celebration);
                }
            }
        }


        if ((!isEnemy && collider.tag == "BulletEnemy")
            || (isEnemy && collider.tag == "Bullet")
            || collider.tag == "Sword"
            || (isEnemy && collider.name == "Melee" && GameObject.Find("Player").GetComponent<Player>().isPunch))
        {

            if (health <= 0)
            {
                animation.state.SetAnimation(0, "hitBig", false);
                if (!isDead)
                {
                    StartCoroutine(CoughUpItem());
                }
                animation.state.SetAnimation(0, "hitBig", false);
                isDead = true;
            }
            else
            {
                if (collider.transform.position.x > transform.position.x)
                {
                    hitMultiplier = 0.25f;
                }
                else
                {
                    hitMultiplier = -0.25f;
                }
                //hitAnim = true;
                animation.state.SetAnimation(0, "hit1", false).Complete += delegate
                {
                    currentState = PlayerStates.wallIdle;
                };
                body.velocity = new Vector2(body.velocity.x * -1, 0);
                health--;
            }
            GameObject.Find("Player").GetComponent<Player>().isPunch = false;
        }
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if (!isFollower && !isEnemy)
        {
            if (collider.name.StartsWith("Tractor") && CrossPlatformInputManager.GetButtonDown("Interact"))
            {
                GameObject.Find("Tractor").transform.Find("Enter").gameObject.SetActive(false);
                GameObject.Find("Tractor").gameObject.GetComponent<CarController2D>().enabled = true;
                this.gameObject.GetComponent<Player>().gameObject.SetActive(false);
                mainCamera.GetComponent<CameraFollowPlayer>().follow = GameObject.Find("Tractor").transform;
            }

            if (collider.name.StartsWith("Bigfoot") && CrossPlatformInputManager.GetButtonDown("Interact"))
            {
                GameObject.Find("Bigfoot").transform.Find("Enter").gameObject.SetActive(false);
                GameObject.Find("Bigfoot").gameObject.GetComponent<CarController2D>().enabled = true;
                this.gameObject.GetComponent<Player>().gameObject.SetActive(false);
                mainCamera.GetComponent<CameraFollowPlayer>().follow = GameObject.Find("Bigfoot").transform;
            }

        }
        if (isEnemy && collider.name == "Melee")
        {
            var a = 3;
        }
        if (isEnemy && collider.name == "Melee" && !isDead && GameObject.Find("Player").GetComponent<Player>().isPunch)
        {
            if (health <= 0)
            {
                animation.state.SetAnimation(0, "hitBig", false);
                isDead = true;
                StartCoroutine(CoughUpItem());

            }
            else
            {
                if (collider.transform.position.x > transform.position.x)
                {
                    hitMultiplier = 0.25f;
                }
                else
                {
                    hitMultiplier = -0.25f;
                }
                //hitAnim = true;
                animation.state.SetAnimation(0, "hit1", false).Complete += delegate
                {
                    currentState = PlayerStates.wallIdle;
                };
                body.velocity = new Vector2(body.velocity.x * -1, 0);
                health--;
            }
            GameObject.Find("Player").GetComponent<Player>().isPunch = false;
        }


        {
            if (isFollower && isActive)
            {
                if (collider.name.StartsWith("AiStop"))
                {
                    aiMoveRight = aiMoveLeft = false;
                }
                else if (collider.name.StartsWith("AiJumpRight"))
                {
                    if (body.velocity.x > 1)
                    {
                        doJump = true;
                    }
                }
                else if (collider.name.StartsWith("AiJumpLeft"))
                {
                    if (body.velocity.x < -1)
                    {
                        doJump = true;
                    }
                }
                else if (collider.name.StartsWith("AiMoveRight"))
                {
                    aiMoveRight = true;
                    aiMoveLeft = false;
                }
                else if (collider.name.StartsWith("AiMoveLeft"))
                {
                    aiMoveLeft = true;
                    aiMoveRight = false;
                }
                else if (!collider.gameObject.layer.Equals(1)
                    && !collider.gameObject.layer.Equals(23)
                    && !collider.tag.Equals("Player")
                    && !collider.tag.Equals("Follower"))
                {
                    doJump = true;
                    jumpHeight += 1;
                }


            }
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (!isFollower && !isEnemy)
        {
            if (collider.name.StartsWith("Tractor"))
            {
                GameObject.Find("Tractor").transform.Find("Enter").gameObject.SetActive(false);
            }
            if (collider.name.StartsWith("Bigfoot"))
            {
                GameObject.Find("Bigfoot").transform.Find("Enter").gameObject.SetActive(false);
            }
            //    if (collider.name.StartsWith("ZoomIn"))
            //    {
            //        isZoomIn = false;
            //    }



            //}

            //      {
            //          if (isFollower&&!isActive)
            //          {
            //              if (collider.tag.Equals("Player"))
            //              {
            //                  isActive = true;
            //                 SetCurrentState(Player.PlayerStates.celebration);
            //              }
            //          }
        }
    }

    void Update()
    {
        if (!isFollower && !isEnemy)
        {
            //  if (isZoomIn)
            //  {
            Camera.main.orthographicSize = Mathf.MoveTowards(Camera.main.orthographicSize, zoomSize, 2f * Time.deltaTime);
            //   }
            //   else
            //   {
            //       Camera.main.orthographicSize = Mathf.MoveTowards(Camera.main.orthographicSize, 10f, .5f * Time.deltaTime);
            //   }
            if (!cameraFollowPlayer.isFollowPlayer)
            {
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, cameraPosition, 2f * Time.deltaTime);
            }

            this.punch1time = Mathf.MoveTowards(this.punch1time, 0f, Time.deltaTime);

            this.punch2time = Mathf.MoveTowards(this.punch2time, 0f, Time.deltaTime);
        }
        CheckIsGrounded();

        if (!isDead)
        {
            ActivateEnemies();
            CheckSwim();
            CheckLadderTouch();
            CheckWallTouch();
            Interact();
            Combat();
            Weapon();
            Follow();
            Movement();
            Flip();
            SkinChange();

            if (currentState != previousState)
            {
                SetAnimation();
            }
            if (currentState != PlayerStates.ladderClimbUp && currentState != PlayerStates.ladderClimbDown && currentState != PlayerStates.ladderIdle && currentState != PlayerStates.swim && currentState != PlayerStates.swimIdle)
            {
                velocity.y = body.velocity.y;
                body.gravityScale = bodyGravity;
            }
            else
                body.gravityScale = 0;
            body.velocity = velocity;
            previousVelocity = velocity;
            velocity.x = 0;
            previousState = currentState;
            previousDoFire = doFire;
            //SpawnPointSystem();
            doMoveLeft = doMoveRight = doWalkLeft = doWalkRight = doJump = doFire = doStartFire = doEndFire = false;
        }
    }

    void ActivateEnemies()
    {
        if (this.name.Equals("Player"))
        {
            var enemies = FindObjectsOfType<Player>();
            foreach (var enemy in enemies)
            {
                if (enemy.name.StartsWith("Enemy") && !enemy.isActive)
                {
                    Vector3 diff = enemy.transform.position - this.transform.position;
                    float curDistance = diff.sqrMagnitude;
                    if (curDistance < 400)
                    {
                        enemy.isActive = true;
                    }
                }
            }
        }
    }


    //Simple change spawn for video recording.
    void SpawnPointSystem()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (currentSpawnPoint >= spawnPoints.Length)
            {
                currentSpawnPoint = 0;
            }
            transform.position = spawnPoints[currentSpawnPoint].position;
            currentSpawnPoint++;
        }
    }

    //Simple system to work with objects on the interactive layer so we can push and pull them.
    void Interact()
    {
        if ((Input.GetKeyDown(KeyCode.E) || CrossPlatformInputManager.GetButtonDown("Interact")) && isGrounded)
        {
            if (interactiveTouch)
            {
                if (currentInteractiveObject != null)
                    currentInteractiveObject.isKinematic = true;
                flipEnabled = true;
                interactiveTouch = false;
                currentState = PlayerStates.idle;
            }
            else
            {
                Collider2D collider = Physics2D.OverlapCircle(WallCheck.position, 0.8f, interactiveLayer);
                if (collider != null)
                {
                    if (collider)
                    {
                        flipEnabled = false;
                        interactiveTouch = true;
                        currentState = PlayerStates.pushIdle;
                        currentInteractiveObject = collider.transform.GetComponent<Rigidbody2D>();
                    }
                }
            }
        }
        else if (currentState == PlayerStates.pushIdle || currentState == PlayerStates.push || currentState == PlayerStates.pullIdle || currentState == PlayerStates.pull)
        {
            if (!Physics2D.OverlapCircle(WallCheck.position, 0.8f, interactiveLayer))
            {
                flipEnabled = true;
                interactiveTouch = false;
                currentState = PlayerStates.idle;
            }
        }
    }

    /*Rotate shoulder and neck bones based on mouse position. Ideally the angles should be adjusted depending on the animation as the torso bone impacts the rotation of the bones,
      therefore currently things such as crouching do not line up with the mouse.*/
    void Aim()
    {

        //		Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
        //		Vector3 lookPos = Camera.main.ScreenToWorldPoint(mousePos);
        //		if(lookPos.x < transform.position.x)
        //		{
        //			lookPos.x = lookPos.x + (transform.position.x - lookPos.x)*2;
        //		}
        //		lookPos = lookPos - transform.position;
        //		float angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg;
        if (!isFollower)
        {
            float x = Mathf.Abs(Input.GetAxis("RightHorizontal"));
            float y = Input.GetAxis("RightVertical");
              float angle = 0f;
            if (x != 0.0f || y != 0.0f)
            {
                angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            }
            if (currentState == PlayerStates.running)
            {
                angle += 25f;
            }
            else
            {
                angle += 2f;
            }
                //animation.skeleton.UpdateWorldTransform();
                //This offset is quick-fix used to reorientate the right shoulder when looking up and down (when you hold a gun, one of your arms extends as you aim up or down more.)
                float offset = angle;
            //		if (angle > 90)
            //			angle = 90;
            //		if (angle < -30)
            //			angle = -30;
            //			if (angle > 10 || angle < 0)
            //				offset = angle * 1.2f;
            //			angle -= 12;
            neck.Rotation = neck.Rotation + angle;
            leftShoulder.Rotation = leftShoulder.Rotation + angle + offset;
            rightShoulder.Rotation = rightShoulder.Rotation + angle;
        }
    }

    void AiAim()
    {
        if (isFollower)
        {
            GameObject[] targets = new GameObject[0];
            if (isEnemy)
            {
                if (!isAiControlled)
                {
                    targets = GameObject.FindGameObjectsWithTag("Player").Union(GameObject.FindGameObjectsWithTag("Follower")).ToArray();
                }
            }
            else
            {
                targets = GameObject.FindGameObjectsWithTag("Enemy");
            }
            foreach (var target in targets)
            {
                if (target != null && !target.GetComponent<Player>().isDead)
                {
                    if (Mathf.Abs(target.transform.position.x - transform.position.x) < 10 && Mathf.Abs(target.transform.position.y - transform.position.y) < 10)
                    {
                        Vector3 lookPos = new Vector3(target.transform.position.x, target.transform.position.y, 10);

                        if (lookPos.x < transform.position.x)
                        {
                            lookPos.x = lookPos.x + (transform.position.x - lookPos.x) * 2;
                        }
                        lookPos = lookPos - transform.position;
                        float angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg;

                        //animation.skeleton.UpdateWorldTransform();
                        //This offset is quick-fix used to reorientate the right shoulder when looking up and down (when you hold a gun, one of your arms extends as you aim up or down more.)
                        float offset = angle;
                        //		if (angle > 90)
                        //			angle = 90;
                        //		if (angle < -30)
                        //			angle = -30;
                        //					if (angle > 10 || angle < 0)
                        //						offset = angle * 1.2f;
                        //					angle -= 12;
                        neck.Rotation = neck.Rotation + angle;
                        leftShoulder.Rotation = leftShoulder.Rotation + angle + offset;
                        rightShoulder.Rotation = rightShoulder.Rotation + angle;

                        if (!previousDoFire)
                        {
                            doStartFire = true;
                        }
                        doFire = true;
                        if (target.transform.position.x > transform.position.x)
                            transform.rotation = Quaternion.identity;
                        else
                            transform.rotation = flippedRotation;
                    }

                }
            }
            if (previousDoFire && !doFire)
                doEndFire = true;
        }
    }

    //Simple combat system
    void Combat()
    {
        if ((CrossPlatformInputManager.GetButtonDown("Fire") && !isFollower) || doStartFire)
        {
            switch (combatState)
            {
                case CombatStates.unarmed:
                    this.isPunch = true;


                    if (punch1time == 0)
                    {
                        animation.state.SetAnimation(1, "punch1", false);
                        this.punch1time = .5f;
                        this.punch2time = 0f;
                    }
                    else if (punch2time == 0)
                    {
                        animation.state.SetAnimation(1, "punch2", false);
                        this.punch1time = .5f;
                        this.punch2time = .5f;
                    }
                    else
                    {
                        animation.state.SetAnimation(1, "punch3", false);
                        this.punch2time = .5f;
                    }
                    //animation.state.AddAnimation(1, "punch3", false, 0);
                    //allowMovement = false;
                    break;
                case CombatStates.melee:
                    if (previousVelocity.x == 0 && isGrounded == true)
                    {
                        animation.state.SetAnimation(1, "meleeSwing1-fullBody", false);
                        animation.state.AddAnimation(1, "meleeSwing2-fullBody", false, 0);
                        animation.state.AddAnimation(1, "meleeSwing3-fullBody", false, 0);
                        //allowMovement = false; //Movement should probably be limited in this situations.
                    }
                    else
                    {
                        animation.state.SetAnimation(1, "meleeSwing1", false);
                        animation.state.AddAnimation(1, "meleeSwing2", false, 0);
                        animation.state.AddAnimation(1, "meleeSwing3", false, 0);
                    }
                    swordHitbox.enabled = true;
                    break;
                case CombatStates.pistol:
                    animation.state.SetAnimation(1, "pistolNearShoot", true);
                    break;
                case CombatStates.gun:
                    animation.state.SetAnimation(1, "gunShoot", true);
                    break;
                case CombatStates.machineGun:
                    animation.state.SetAnimation(1, "machineGunShoot", true);
                    break;
            }
            currentPistolFireRate = 0;
            currentMachineGunFireRate = 0;
            currentGunFireRate = 0;
        }
        if ((CrossPlatformInputManager.GetButton("Fire") && !isFollower) || doFire)
        {
            switch (combatState)
            {
                case CombatStates.unarmed:
                    break;
                case CombatStates.melee:
                    break;
                case CombatStates.pistol:
                    if (currentPistolFireRate <= 0)
                    {
                        Instantiate(bulletPrefab, BulletPos.position, BulletPos.rotation);
                        currentPistolFireRate = pistolFireRate;
                    }
                    else
                    {
                        currentPistolFireRate -= Time.deltaTime;
                    }
                    break;
                case CombatStates.gun:
                    if (currentGunFireRate <= 0)
                    {
                        for (int i = 0; i < 21; i++)
                        {
                            GameObject bullet = (GameObject)Instantiate(bulletPrefab, BulletPos.position, BulletPos.rotation);
                            float ranX = Random.Range(-25, +25);
                            float ranY = Random.Range(-25, +25);
                            float ranZ = Random.Range(-25, +25);
                            bullet.transform.Rotate(new Vector3(ranX, ranY, ranZ));
                        }

                        currentGunFireRate = gunFireRate;
                    }
                    else
                    {
                        currentGunFireRate -= Time.deltaTime;
                    }
                    break;
                case CombatStates.machineGun:
                    if (currentMachineGunFireRate <= 0)
                    {
                        Instantiate(bulletPrefab, BulletPos.position, BulletPos.rotation);
                        currentMachineGunFireRate = machineGunFireRate;
                    }
                    else
                    {
                        currentMachineGunFireRate -= Time.deltaTime;
                    }
                    break;
            }
        }
        if ((CrossPlatformInputManager.GetButtonUp("Fire") && !isFollower) || doEndFire)
        {
            switch (combatState)
            {
                case CombatStates.unarmed:
                    this.isPunch = false;
                    //   animation.state.SetAnimation(1, "reset", false);
                    allowMovement = true;
                    break;
                case CombatStates.melee:
                    animation.state.SetAnimation(1, "meleeIdle", true);
                    allowMovement = true;
                    break;
                case CombatStates.pistol:
                    animation.state.SetAnimation(1, "pistolNearIdle", true);
                    break;
                case CombatStates.gun:
                    animation.state.SetAnimation(1, "gunIdle", true);
                    break;
                case CombatStates.machineGun:
                    animation.state.SetAnimation(1, "machineGunIdle", true);
                    break;
            }
            swordHitbox.enabled = false;
        }
        if (CrossPlatformInputManager.GetButtonDown("Block"))
        {
            animation.state.SetAnimation(1, "block", true);
            block = true;
        }
        if (CrossPlatformInputManager.GetButtonUp("Block"))
        {
            animation.state.SetAnimation(1, "reset", false);
            block = false;
        }
    }

    //Combat state controller, the combat animations run on a seperate track that allows us to override the movement animations inplace of them.
    void Weapon()
    {
        //Unarmed
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            animation.state.SetAnimation(1, "reset", false);
            combatState = CombatStates.unarmed;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            animation.state.SetAnimation(1, "meleeIdle", true);
            combatState = CombatStates.melee;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            animation.state.SetAnimation(1, "pistolNearIdle", true);
            combatState = CombatStates.pistol;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            animation.state.SetAnimation(1, "gunIdle", true);
            combatState = CombatStates.gun;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            animation.state.SetAnimation(1, "machineGunIdle", true);
            combatState = CombatStates.machineGun;
        }
        //Simple reload animations (Don't do anything, just to showcase reload.)
        if (Input.GetKeyDown(KeyCode.F) || CrossPlatformInputManager.GetButtonDown("Reload"))
        {
            switch (combatState)
            {
                case CombatStates.gun:
                    mouseEnabled = false;
                    animation.state.SetAnimation(1, "gunReload1", false).Complete += delegate
                    {
                        animation.state.SetAnimation(1, "gunIdle", true);
                        mouseEnabled = true;
                    };
                    break;
                case CombatStates.machineGun:
                    mouseEnabled = false;
                    animation.state.SetAnimation(1, "machineGunReload", false).Complete += delegate
                    {
                        animation.state.SetAnimation(1, "machineGunIdle", true);
                        mouseEnabled = true;
                    };
                    break;
            }
        }
    }

    //Simple hotkeys to cycle through character skins.
    void SkinChange()
    {
        //Single skin change.
        if (Input.GetKeyDown(KeyCode.T))
        {
            animation.skeleton.SetSkin(skins[skinCount]);
            skinCount++;
            if (skinCount >= skins.Length)
            {
                skinCount = 0;
            }
        }
        //Toggle skin change mode.
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (skinChangeToggle)
                skinChangeToggle = false;
            else
                skinChangeToggle = true;
        }

        if (skinChangeToggle)
        {
            if (currentSkinChangeTime < 0)
            {
                animation.skeleton.SetSkin(skins[skinCount]);
                skinCount++;
                if (skinCount >= skins.Length)
                {
                    skinCount = 0;
                }
                currentSkinChangeTime = skinChangeTimer;
            }
            currentSkinChangeTime -= Time.deltaTime;
        }
    }


    //State controller for all the characters movement
    void Movement()
    {
        if (!isFollower)
        {
            if (CrossPlatformInputManager.GetAxis("Horizontal") < 0)
            {
                doMoveLeft = true;
            }
            else if (CrossPlatformInputManager.GetAxis("Horizontal") > 0)
            {
                doMoveRight = true;
            }
        }

        if (!isActive)
        {
            currentState = PlayerStates.crouchIdle;
        }
        else
        {
            if (BlackListStates() && allowMovement)
            {
                //Walking/Running/Crouching/Sprinting (Default) movement.
                if (doMoveLeft)
                {
                    if (CrossPlatformInputManager.GetAxis("Vertical") < -.3 && !isFollower)
                    {
                        velocity.x = -walkSpeed;
                        currentState = PlayerStates.crouchWalk;
                    }
                    else if ((CrossPlatformInputManager.GetAxis("Horizontal") > -.9 && !isFollower) || doWalkLeft)
                    {
                        velocity.x = -walkSpeed;
                        currentState = PlayerStates.walking;
                    }
                    else if (Input.GetButton("Interact") && !isFollower)
                    {
                        velocity.x = -sprintSpeed;
                        currentState = PlayerStates.sprinting;
                    }
                    else
                    {
                        velocity.x = -runSpeed;
                        currentState = PlayerStates.running;
                    }
                }

                if (doMoveRight)
                {
                    if (CrossPlatformInputManager.GetAxis("Vertical") < -.3 && !isFollower)
                    {
                        velocity.x = walkSpeed;
                        currentState = PlayerStates.crouchWalk;
                    }
                    else if ((CrossPlatformInputManager.GetAxis("Horizontal") < .9 && !isFollower) || doWalkRight)
                    {
                        velocity.x = walkSpeed;
                        currentState = PlayerStates.walking;
                    }
                    else if (Input.GetButton("Interact") && !isFollower)
                    {
                        velocity.x = sprintSpeed;
                        currentState = PlayerStates.sprinting;
                    }
                    else
                    {
                        velocity.x = runSpeed;
                        currentState = PlayerStates.running;
                    }
                }
            }
            //Ladder movement.
            else if (currentState == PlayerStates.ladderIdle || currentState == PlayerStates.ladderClimbUp || currentState == PlayerStates.ladderClimbDown)
            {
                if (ladderTouch)
                {
                    if (doMoveRight)
                    {
                        if (transform.localRotation == Quaternion.identity)
                        {
                            currentState = PlayerStates.ladderClimbUp;
                            velocity.x = runSpeed;
                            velocity.y = walkSpeed;
                        }
                        else
                        {
                            currentState = PlayerStates.ladderClimbDown;
                            if (isGrounded)
                                velocity.x = runSpeed;
                            else
                                velocity.x = -runSpeed;
                            velocity.y = -walkSpeed;
                        }
                    }
                    else if (doMoveLeft)
                    {
                        if (transform.localRotation == Quaternion.identity)
                        {
                            currentState = PlayerStates.ladderClimbDown;
                            if (isGrounded)
                                velocity.x = -runSpeed;
                            else
                                velocity.x = runSpeed;
                            velocity.y = -walkSpeed;
                        }
                        else
                        {
                            currentState = PlayerStates.ladderClimbUp;
                            velocity.y = walkSpeed;
                            velocity.x = -runSpeed;
                        }
                    }
                    else
                    {
                        currentState = PlayerStates.ladderIdle;
                        velocity.y = 0;
                    }
                    if (CrossPlatformInputManager.GetAxis("Vertical") > 0 && !isFollower)
                    {
                        if (transform.localRotation == Quaternion.identity)
                        {
                            if (isGrounded)
                                velocity.x = -runSpeed;
                            else
                                velocity.x = runSpeed;
                        }
                        else
                        {
                            if (isGrounded)
                                velocity.x = runSpeed;
                            else
                                velocity.x = -runSpeed;
                            ;
                        }
                        velocity.y = walkSpeed;
                        currentState = PlayerStates.ladderClimbUp;
                    }
                    if (CrossPlatformInputManager.GetAxis("Vertical") < 0 && !isFollower)
                    {
                        if (transform.localRotation == Quaternion.identity)
                        {
                            if (isGrounded)
                                velocity.x = -runSpeed;
                            else
                                velocity.x = runSpeed;
                        }
                        else
                        {
                            if (isGrounded)
                                velocity.x = runSpeed;
                            else
                                velocity.x = -runSpeed;
                            ;
                        }
                        velocity.y = -walkSpeed;
                        currentState = PlayerStates.ladderClimbDown;
                    }
                }
                else
                {
                    currentState = PlayerStates.idle;
                }
            }
            //Interactive movement.
            else if (interactiveTouch && !isFollower)
            {
                if (doMoveRight)
                {

                    if (transform.localRotation == Quaternion.identity)
                    {
                        currentState = PlayerStates.push;
                        if (pushPullState)
                        {

                            if (currentPushPullTimer > ((pushPullEaseTimer / 10) * 8) || currentPushPullTimer < ((pushPullEaseTimer / 10) * 2))
                                velocity.x = pushSpeed / 2;
                            else
                                velocity.x = pushSpeed;
                            currentPushPullTimer -= Time.deltaTime;
                            currentInteractiveObject.velocity = new Vector2(velocity.x, currentInteractiveObject.velocity.y);
                        }
                    }
                    else
                    {
                        currentState = PlayerStates.pull;
                        if (pushPullState)
                        {
                            if (currentPushPullTimer > ((pushPullEaseTimer / 10) * 8) || currentPushPullTimer < ((pushPullEaseTimer / 10) * 2))
                                velocity.x = pullSpeed / 2;
                            else
                                velocity.x = pullSpeed;
                            currentInteractiveObject.velocity = new Vector2(velocity.x * 1.5f, currentInteractiveObject.velocity.y);
                            currentPushPullTimer -= Time.deltaTime;
                        }
                    }
                    currentInteractiveObject.isKinematic = false;
                }
                else if (doMoveLeft)
                {

                    if (transform.localRotation == Quaternion.identity)
                    {
                        currentState = PlayerStates.pull;
                        if (pushPullState)
                        {
                            if (currentPushPullTimer > ((pushPullEaseTimer / 10) * 8) || currentPushPullTimer < ((pushPullEaseTimer / 10) * 2))
                                velocity.x = -pullSpeed / 2;
                            else
                                velocity.x = -pullSpeed;
                            currentInteractiveObject.velocity = new Vector2(velocity.x * 1.5f, currentInteractiveObject.velocity.y);
                            currentPushPullTimer -= Time.deltaTime;
                        }
                    }
                    else
                    {
                        currentState = PlayerStates.push;
                        if (pushPullState)
                        {
                            if (currentPushPullTimer > ((pushPullEaseTimer / 10) * 8) || currentPushPullTimer < ((pushPullEaseTimer / 10) * 2))
                                velocity.x = -pushSpeed / 2;
                            else
                                velocity.x = -pushSpeed;
                            currentInteractiveObject.velocity = new Vector2(velocity.x, currentInteractiveObject.velocity.y);
                            currentPushPullTimer -= Time.deltaTime;
                        }
                    }
                    currentInteractiveObject.isKinematic = false;

                }
                else
                {
                    if (previousState == PlayerStates.push)
                        currentState = PlayerStates.pushIdle;
                    else if (previousState == PlayerStates.pull)
                        currentState = PlayerStates.pullIdle;
                    currentInteractiveObject.velocity = new Vector2(0, currentInteractiveObject.velocity.y);
                }
            }
            //Swimming movement.
            else if (isSwim)
            {
                currentState = PlayerStates.swimIdle;
                velocity.y = 0;
                if (doMoveLeft)
                {
                    transform.localRotation = flippedRotation;
                    velocity.x = -swimSpeed;
                    currentState = PlayerStates.swim;
                }
                if (doMoveRight)
                {
                    transform.localRotation = Quaternion.identity;
                    velocity.x = swimSpeed;
                    currentState = PlayerStates.swim;
                }
            }
            //Edge climb.
            else if (currentState == PlayerStates.edgeClimb || currentState == PlayerStates.edgeIdle)
            {
                if (Input.GetKeyDown(KeyCode.Space) || CrossPlatformInputManager.GetButtonDown("Jump"))
                {
                    mainCamera.follow = transform;
                    currentState = PlayerStates.edgeClimb;
                }
            }
            //A little movement speed boost when rolling.
            else if (currentState == PlayerStates.rolling)
            {
                velocity.x = jumpSpeed * 1.5f;
            }
            else if (currentState == PlayerStates.celebration)
            {

            }
            //Reduce movement towards 0 when skidding. 
            else if (currentState == PlayerStates.skid)
            {
                if (transform.rotation == Quaternion.identity)
                {
                    velocity.x = jumpSpeed;
                    if ((jumpSpeed - skidSpeedReduce * Time.deltaTime) > 0)
                        jumpSpeed -= skidSpeedReduce * Time.deltaTime;
                    else
                        jumpSpeed = 0;
                }
                else
                {
                    velocity.x = jumpSpeed;
                    if ((jumpSpeed + skidSpeedReduce * Time.deltaTime) < 0)
                        jumpSpeed += skidSpeedReduce * Time.deltaTime;
                    else
                        jumpSpeed = 0;
                }
            }
            //Landing movement control.
            else if (currentState == PlayerStates.landing)
            {
                if (doMoveLeft)
                {
                    if (jumpSpeed < 0)
                        velocity.x = (jumpSpeed / landingTimeSpeed);
                    else
                        velocity.x = -(jumpSpeed / landingTimeSpeed);
                }
                if (doMoveRight)
                {
                    if (jumpSpeed > 0)
                        velocity.x = (jumpSpeed / landingTimeSpeed);
                    else
                        velocity.x = -(jumpSpeed / landingTimeSpeed);
                }
                if (landingTimeSpeed > 1)
                    landingTimeSpeed -= landingSpeedReduceTime * Time.deltaTime;
            }
            else if (isGrounded)
            {
                landingTimeSpeed = landingSpeedReduce;
                currentState = PlayerStates.landing;

            }
            //All the jump states movement control.
            else if (currentState == PlayerStates.jumping || currentState == PlayerStates.wallJump || currentState == PlayerStates.doubleJump)
            {
                velocity.x = jumpSpeed;
                if (wallTouch && velocity.x != 0)
                {
                    if (jumpSpeed * -1 > 0)
                        transform.localRotation = Quaternion.identity;
                    else
                        transform.localRotation = flippedRotation;
                    currentState = PlayerStates.wallIdle;
                }
                if (doMoveRight)
                {
                    if (transform.rotation == Quaternion.identity)
                    {
                        velocity.x = Mathf.Lerp(velocity.x, originJumpSpeed, 2.0f * Time.deltaTime);
                    }
                    else
                    {
                        velocity.x = Mathf.Lerp(velocity.x, 0, 2.0f * Time.deltaTime);
                    }
                    jumpSpeed = velocity.x;
                }
                if (doMoveLeft)
                {
                    if (transform.rotation == Quaternion.identity)
                    {
                        velocity.x = Mathf.Lerp(velocity.x, 0, 2.0f * Time.deltaTime);
                    }
                    else
                    {
                        velocity.x = Mathf.Lerp(velocity.x, originJumpSpeed, 2.0f * Time.deltaTime);
                    }
                    jumpSpeed = velocity.x;
                }
            }
            //Wall idle
            else if (currentState == PlayerStates.wallIdle)
            {
                if (!wallTouch)
                    currentState = PlayerStates.falling;
                if (CrossPlatformInputManager.GetButtonDown("Jump") || isFollower)
                {
                    if (jumpSpeed > 0 && jumpSpeed < runSpeed)
                        jumpSpeed = runSpeed;
                    else if (jumpSpeed < 0 && jumpSpeed > -runSpeed)
                        jumpSpeed = -runSpeed;
                    jumpSpeed = -jumpSpeed;
                    originJumpSpeed = jumpSpeed;
                    body.velocity = new Vector2(body.velocity.x / 2, 0);
                    body.AddForce(new Vector2(0, jumpHeight));
                    currentState = PlayerStates.wallJump;
                    wallTouch = false;
                    StartCoroutine(WallFrames());
                }
                //Set a terminal velocity when touching a wall to account for friction. (Can't do any cool moves if your fall at a realistic speed!)
                if (body.velocity.y < terminalVelocityWall)
                    body.velocity = new Vector2(body.velocity.x, terminalVelocityWall);
                velocity.x = jumpSpeed;
            }
            //Fall!
            else if (currentState == PlayerStates.falling)
            {
                velocity.x = jumpSpeed;
                if (wallTouch && !isFollower)
                {
                    if (jumpSpeed * -1 > 0)
                        transform.localRotation = Quaternion.identity;
                    else
                        transform.localRotation = flippedRotation;
                    currentState = PlayerStates.wallIdle;
                }
                if (doMoveRight)
                {
                    if (transform.rotation == Quaternion.identity)
                    {
                        velocity.x = Mathf.Lerp(velocity.x, originJumpSpeed, 2.0f * Time.deltaTime);
                    }
                    else
                    {
                        velocity.x = Mathf.Lerp(velocity.x, 0, 2.0f * Time.deltaTime);
                    }
                    jumpSpeed = velocity.x;
                }
                if (doMoveLeft)
                {
                    if (transform.rotation == Quaternion.identity)
                    {
                        velocity.x = Mathf.Lerp(velocity.x, 0, 2.0f * Time.deltaTime);
                    }
                    else
                    {
                        velocity.x = Mathf.Lerp(velocity.x, originJumpSpeed, 2.0f * Time.deltaTime);
                    }
                    jumpSpeed = velocity.x;
                }
            }
            //Jump!
            if ((CrossPlatformInputManager.GetButtonDown("Jump") && !isFollower) || doJump)
            {
                if (isGrounded && currentState != PlayerStates.edgeIdle && currentState != PlayerStates.edgeClimb && currentState != PlayerStates.push && currentState != PlayerStates.pull
                     && currentState != PlayerStates.pushIdle && currentState != PlayerStates.pullIdle)
                {
                    currentState = PlayerStates.jumping;
                    body.AddForce(new Vector2(0, jumpHeight));
                    isGrounded = false;
                    jumpSpeed = velocity.x;
                    if (velocity.x > 0 && velocity.x < runSpeed)
                        jumpSpeed = runSpeed;
                    else if (velocity.x < 0 && velocity.x > -runSpeed)
                        jumpSpeed = -runSpeed;
                    originJumpSpeed = jumpSpeed;
                    StartCoroutine(JumpFrames());
                }
                else if (currentState == PlayerStates.jumping && !isFollower && isDoubleJump)
                {
                    currentState = PlayerStates.doubleJump;
                    if (doMoveRight)
                    {
                        if (transform.rotation == flippedRotation)
                        {
                            body.velocity = new Vector2(-velocity.x, 0);
                            body.AddForce(new Vector2(-velocity.x, jumpHeight));
                            originJumpSpeed = -originJumpSpeed;
                            jumpSpeed = originJumpSpeed;
                            transform.rotation = Quaternion.identity;
                        }
                        else
                        {
                            body.velocity = new Vector2(body.velocity.x, 0);
                            body.AddForce(new Vector2(velocity.x, jumpHeight));
                        }
                    }
                    else if (doMoveLeft)
                    {
                        if (transform.rotation == Quaternion.identity)
                        {
                            body.velocity = new Vector2(-velocity.x, 0);
                            body.AddForce(new Vector2(-velocity.x, jumpHeight));
                            originJumpSpeed = -originJumpSpeed;
                            jumpSpeed = originJumpSpeed;
                            transform.rotation = flippedRotation;
                        }
                        else
                        {
                            body.velocity = new Vector2(body.velocity.x, 0);
                            body.AddForce(new Vector2(velocity.x, jumpHeight));
                        }
                    }
                    else
                    {
                        body.velocity = new Vector2(body.velocity.x, 0);
                        body.AddForce(new Vector2(velocity.x, jumpHeight));
                    }
                    isGrounded = false;
                }

            }
            //Roll animation state.
            if (Input.GetKeyDown(KeyCode.R) && isGrounded && currentState != PlayerStates.rolling)
            {
                currentState = PlayerStates.rolling;
                jumpSpeed = velocity.x;
            }
            //Ground Idle animation states.
            if (velocity.x == 0 && BlackListStates() == true)
            {
                if (CrossPlatformInputManager.GetAxis("Vertical") < -.3)
                {
                    currentState = PlayerStates.crouchIdle;
                }
                else
                {
                    currentState = PlayerStates.idle;
                }
            }
            //Trigger Fall animation state.
            if (body.velocity.y < 0 && !isGrounded && BlackListStates() == true && ladderToGroundFrames == true)
            {
                jumpSpeed = velocity.x;
                originJumpSpeed = jumpSpeed;
                currentState = PlayerStates.falling;
            }
            //Reset the sprint timer transition if we stop running.
            //		if (previousState == PlayerStates.running || previousState == PlayerStates.sprinting) {
            //			if (currentState != PlayerStates.running && currentState != PlayerStates.sprinting) {
            //				currentSprintTimer = sprintTimer;
            //			}
            //		}
            //Simple idle ladder.
            if (ladderTouch && currentState != PlayerStates.ladderClimbUp && currentState != PlayerStates.ladderClimbDown && currentState != PlayerStates.ladderIdle)
            {
                currentState = PlayerStates.ladderIdle;
            }
            //Player will skid when changing state from sprint unless he jumps.
            if (previousState == PlayerStates.sprinting && currentState != PlayerStates.skid && currentState != PlayerStates.rolling)
            {
                if (previousVelocity.x > 0 && velocity.x <= 0)
                {
                    currentSprintTimer = sprintTimer;
                    currentState = PlayerStates.skid;
                    jumpSpeed = previousVelocity.x;
                    velocity.x = previousVelocity.x;
                }
                else if (previousVelocity.x < 0 && velocity.x >= 0)
                {
                    currentSprintTimer = sprintTimer;
                    currentState = PlayerStates.skid;
                    jumpSpeed = previousVelocity.x;
                    velocity.x = previousVelocity.x;
                }
                else if (previousState == PlayerStates.sprinting && currentState != PlayerStates.sprinting && isGrounded)
                {
                    currentSprintTimer = sprintTimer;
                    currentState = PlayerStates.skid;
                    jumpSpeed = previousVelocity.x;
                }
            }
            //Don't play the falling animation when going from the ladder to the ground for a few frames.
            if (previousState == PlayerStates.ladderClimbUp && currentState != PlayerStates.ladderClimbUp && currentState != PlayerStates.ladderClimbDown && currentState != PlayerStates.ladderIdle)
            {
                StartCoroutine(LadderToGroundFrames());
            }
        }
    }


    /*The animation controller for our class, it is only called when the previous and current character states are not equal (so we never call the same animation twice in a row) 
	 * as we are looping most of the animations. Anything that doesn't loop can be linked on to another animation e.g. jumping -> falling as these are animated to go together.*/
    void SetAnimation()
    {
        switch (currentState)
        {
            case PlayerStates.idle:
                animation.state.SetAnimation(0, "idle", true);
                break;
            case PlayerStates.running:
                animation.state.SetAnimation(0, "run", true);
                break;
            case PlayerStates.walking:
                animation.state.SetAnimation(0, "walk", true);
                break;
            case PlayerStates.jumping:
                animation.state.SetAnimation(0, "jump", false).Complete += delegate
                {
                    currentState = PlayerStates.falling;
                };
                break;
            case PlayerStates.doubleJump:
                animation.state.SetAnimation(0, "jump3", false);
                break;
            case PlayerStates.crouchIdle:
                animation.state.SetAnimation(0, "crouchIdle", true);
                break;
            case PlayerStates.crouchWalk:
                animation.state.SetAnimation(0, "crouchWalk", true);
                break;
            case PlayerStates.sprinting:
                animation.state.SetAnimation(0, "run2", true);
                break;
            case PlayerStates.rolling:
                animation.state.SetAnimation(0, "roll", true).Complete += delegate
                {
                    currentState = PlayerStates.idle;
                };
                break;
            case PlayerStates.landing:
                animation.state.SetAnimation(0, "land", false).Complete += delegate
                {
                    currentState = PlayerStates.idle;
                };
                break;
            case PlayerStates.wallIdle:
                animation.state.SetAnimation(0, "wallIdle", true);
                break;
            case PlayerStates.wallJump:
                animation.state.SetAnimation(0, "wallJump", false).Complete += delegate
                {
                    currentState = PlayerStates.falling;
                };
                break;
            case PlayerStates.falling:
                animation.state.SetAnimation(0, "falling", true);
                break;
            case PlayerStates.ladderIdle:
                animation.state.SetAnimation(0, "climbIdle", true);
                break;
            case PlayerStates.ladderClimbUp:
                animation.state.SetAnimation(0, "climbUp", true);
                break;
            case PlayerStates.ladderClimbDown:
                animation.state.SetAnimation(0, "climbDown", true);
                break;
            case PlayerStates.pushIdle:
                animation.state.SetAnimation(0, "pushIdle", true);
                break;
            case PlayerStates.push:
                animation.state.SetAnimation(0, "push", true).Event += delegate (Spine.AnimationState state, int trackIndex, Spine.Event e)
                {
                    /*A few animations have events keyed into their spine animations, the push animation for example has two so that we can control the character's movement speed throughout the animation
                      as the character will NOT be moving at a constant speed when pushing or pulling an object. Another use for event keys is for things such as melee attacks, you can place
                     event keys in the frames where you want an active hitbox.*/
                    if (e.Data.name == "Move Start")
                    {
                        currentPushPullTimer = pushPullEaseTimer;
                        pushPullState = true;
                    }
                    else if (e.Data.name == "Move End")
                    {
                        pushPullState = false;
                    }
                };
                break;
            case PlayerStates.pullIdle:
                animation.state.SetAnimation(0, "pullIdle", true);
                break;
            case PlayerStates.pull:
                animation.state.SetAnimation(0, "pull", true).Event += delegate (Spine.AnimationState state, int trackIndex, Spine.Event e)
                {
                    if (e.Data.name == "Move Start")
                    {
                        currentPushPullTimer = pushPullEaseTimer;
                        pushPullState = true;
                    }
                    else if (e.Data.name == "Move End")
                    {
                        pushPullState = false;
                    }
                };
                break;
            case PlayerStates.swim:
                animation.state.SetAnimation(0, "swim", true);
                break;
            case PlayerStates.swimIdle:
                animation.state.SetAnimation(0, "swimIdle", true);
                break;
            case PlayerStates.edgeIdle:
                break;
            case PlayerStates.edgeClimb:
                animation.state.SetAnimation(0, "edgeClimb", false).Complete += delegate
                {
                    currentState = PlayerStates.idle;
                };
                break;
            case PlayerStates.skid:
                animation.state.SetAnimation(0, "skid", false).Complete += delegate
                {
                    currentState = PlayerStates.idle;
                };
                break;
            case PlayerStates.celebration:
                animation.state.SetAnimation(0, "celebration", false).Complete += delegate
                {
                    currentState = PlayerStates.idle;
                };
                break;
            default:
                break;
        }
    }

    //4 Physics checks called every frame:
    void CheckSwim()
    {
        isSwim = Physics2D.OverlapCircle(GroundCheck.position, 0.1f, swimLayer);
        if (!isSwim)
        {
            if (currentState == PlayerStates.swim || currentState == PlayerStates.swimIdle)
            {
                currentState = PlayerStates.idle;
            }
        }
    }

    void CheckIsGrounded()
    {
        if (jumpFrames)
        {
            if (Physics2D.OverlapCircle(GroundCheck.position, 0.1f, groundLayer))
                isGrounded = true;
        }
        if (Physics2D.OverlapCircle(GroundCheck.position, 1f, interactiveLayer) && this.velocity.y <= 0f)
        {
            isGrounded = true;
            Physics2D.IgnoreCollision(this.GetComponent<CircleCollider2D>(), GameObject.Find("Cart").GetComponent<PolygonCollider2D>(), false);
            Physics2D.IgnoreCollision(this.GetComponent<BoxCollider2D>(), GameObject.Find("Cart").GetComponent<PolygonCollider2D>(), false);
        }
        else
        {
            Physics2D.IgnoreCollision(this.GetComponent<CircleCollider2D>(), GameObject.Find("Cart").GetComponent<PolygonCollider2D>());
            Physics2D.IgnoreCollision(this.GetComponent<BoxCollider2D>(), GameObject.Find("Cart").GetComponent<PolygonCollider2D>());
        }
    }

    void CheckLadderTouch()
    {
        if (ladderFrames)
        {
            ladderTouch = Physics2D.OverlapCircle(LadderCheck.position, 0.8f, ladderLayer);
        }
    }

    void CheckWallTouch()
    {
        if (wallFrames && isWallJump)
        {
            wallTouch = Physics2D.OverlapCircle(WallCheck.position, 0.8f, wallLayer);
        }
    }

    //Check called every frame incase we need to flip the character based on their velocity.
    void Flip()
    {
        if (flipEnabled)
        {


            if (!isFollower && currentState != PlayerStates.wallIdle && currentState != PlayerStates.falling && currentState != PlayerStates.doubleJump && ladderTouch == false)
            {
                if (velocity.x > 0 && Input.GetAxis("RightHorizontal") > 0)
                    transform.localRotation = Quaternion.identity;
                else if (velocity.x < 0 && Input.GetAxis("RightHorizontal") > 0)
                    transform.localRotation = Quaternion.identity;
                else if (velocity.x > 0 && Input.GetAxis("RightHorizontal") < 0)
                    transform.localRotation = flippedRotation;
                else if (velocity.x < 0 && Input.GetAxis("RightHorizontal") < 0)
                    transform.localRotation = flippedRotation;
                else if (velocity.x > 0 && Input.GetAxis("RightHorizontal") == 0)
                    transform.localRotation = Quaternion.identity;
                else if (velocity.x < 0 && Input.GetAxis("RightHorizontal") == 0)
                    transform.localRotation = flippedRotation;
                else if (velocity.x == 0 && Input.GetAxis("RightHorizontal") > 0)
                    transform.localRotation = Quaternion.identity;
                else if (velocity.x == 0 && Input.GetAxis("RightHorizontal") < 0)
                    transform.localRotation = flippedRotation;

            }

            if (isFollower)
            {
                if (velocity.x > 0)
                    transform.localRotation = Quaternion.identity;
                else if (velocity.x < 0)
                    transform.localRotation = flippedRotation;
            }
        }
    }

    //Check called every frame to follow the target.
    void Follow()
    {
        if (isFollower && isActive && !isAiControlled)
        {
            var player = GameObject.FindWithTag("Player");

            // Uncomment for followers in tractor
            //if (player == null)
            //{
            //    player = GameObject.Find("Tractor");
            //    FollowPosition = 4;
            //}
            //else
            //{
            //    FollowPosition = originalFollowPosition;
            //}

            var playerTranform = player.transform;



            if (Mathf.Abs(playerTranform.localPosition.x - transform.localPosition.x) < 30)
            {
                jumpHeight = 900;
            }


            if (aiMoveLeft)
            {
                doMoveLeft = true;
                velocity.x = -runSpeed;
            }
            else if (aiMoveRight)
            {
                doMoveRight = true;
                velocity.x = runSpeed;
            }
            else if (playerTranform.rotation == Quaternion.identity) // facing right
            {
                if (playerTranform.localPosition.x + FollowPosition > transform.localPosition.x)
                {
                    doMoveRight = true;
                    velocity.x = runSpeed;
                }
                else if (playerTranform.localPosition.x + FollowPosition < transform.localPosition.x - 1)
                {
                    doMoveLeft = true;
                    velocity.x = -runSpeed;
                }
                else
                {
                    currentState = PlayerStates.idle;
                }
            }
            else // facing left
            {
                if (playerTranform.localPosition.x - FollowPosition > transform.localPosition.x + 1)
                {
                    doMoveRight = true;
                    velocity.x = runSpeed;
                }
                else if (playerTranform.localPosition.x - FollowPosition < transform.localPosition.x)
                {
                    doMoveLeft = true;
                    velocity.x = -runSpeed;
                }
                else
                {
                    currentState = PlayerStates.idle;

                }
                //else if (Mathf.Abs(target.localPosition.x - transform.localPosition.x) > FollowPosition + 4)
                //{
                //    if (target.localPosition.x < transform.localPosition.x)
                //    {
                //        doMoveLeft = true;
                //        velocity.x = -runSpeed;
                //    }
                //    if (target.localPosition.x > transform.localPosition.x)
                //    {
                //        doMoveRight = true;
                //        velocity.x = runSpeed;
                //    }
                //}
                //        else if (Mathf.Abs (target.localPosition.x - transform.localPosition.x) > FollowPosition) {
                //if (target.localPosition.x < transform.localPosition.x) {
                //	doMoveLeft = true;
                //	doWalkLeft = true;
                //	velocity.x = -walkSpeed;
                //}
                //if (target.localPosition.x > transform.localPosition.x) {
                //	doMoveRight = true;
                //	doWalkRight = true;
                //	velocity.x = walkSpeed;
                //}
            }
            //         else {
            ////	jumpHeight = 900;
            //	//currentState = PlayerStates.idle;
            //}

        }
    }

    //Blacklisted states for special cases when velocity is 0 and we do not want to go to the idle animation.
    bool BlackListStates()
    {
        if (currentState != PlayerStates.jumping &&
            currentState != PlayerStates.doubleJump &&
            currentState != PlayerStates.rolling &&
            currentState != PlayerStates.landing &&
            currentState != PlayerStates.wallIdle &&
            currentState != PlayerStates.wallJump &&
            currentState != PlayerStates.falling &&
            currentState != PlayerStates.ladderIdle &&
            currentState != PlayerStates.ladderClimbUp &&
            currentState != PlayerStates.ladderClimbDown &&
            currentState != PlayerStates.pushIdle &&
            currentState != PlayerStates.push &&
            currentState != PlayerStates.pullIdle &&
            currentState != PlayerStates.pull &&
            currentState != PlayerStates.swim &&
            currentState != PlayerStates.swimIdle &&
            currentState != PlayerStates.edgeIdle &&
            currentState != PlayerStates.edgeClimb &&
            currentState != PlayerStates.skid &&
            currentState != PlayerStates.celebration)
        {
            return true;
        }
        return false;
    }

    //Set currentstate of player, useful for triggering bullets and hanging off ledges.
    public void SetCurrentState(PlayerStates state)
    {
        currentState = state;
    }

    //Delay groundChecks for a few frames after jumping incase we are still touching the floor after jumping.
    IEnumerator JumpFrames()
    {

        jumpFrames = false;
        yield return new WaitForSeconds(0.2f);
        jumpFrames = true;
    }

    //Controls a boolean to prevent character from proceeding to falling animation between ladder to ground transition.
    IEnumerator LadderToGroundFrames()
    {
        ladderToGroundFrames = false;
        yield return new WaitForSeconds(0.4f);
        ladderToGroundFrames = true;
    }

    //Delay wallChecks for a few frames after wall jumping incase we are still touching the wall after jumping.
    IEnumerator WallFrames()
    {
        wallFrames = false;
        yield return new WaitForSeconds(0.2f);
        wallFrames = true;
    }


    private void ResetMusic()
    {
        var allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioS in allAudioSources)
        {
            audioS.Stop();
            audioS.Play();
        }
    }


}
