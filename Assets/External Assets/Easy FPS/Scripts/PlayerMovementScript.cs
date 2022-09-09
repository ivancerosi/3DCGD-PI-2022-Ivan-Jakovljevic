using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementScript : MonoBehaviour, ITransferMovement {
	Rigidbody rb;

	// changed by ladder script
	public bool isOnLadders = false;

	[Tooltip("Current players speed")]
	public float currentSpeed;
	[Tooltip("Assign players camera here")]
	[HideInInspector]public Transform cameraMain;
	[Tooltip("Force that moves player into jump")]
	public float jumpForce = 500;
	[Tooltip("Position of the camera inside the player")]
	[HideInInspector]public Vector3 cameraPosition;


	Transform mainCamera;

	/*
	 * \\ Code for enabling moving platforms to transfer movement onto player standing on top
	 */
	List<Vector3> movementContributions = new List<Vector3>();

	public void contributeToVelocity(Vector3 contribution)
	{
		movementContributions.Add(contribution);
	}

	public Vector3 collectContributions()
	{
		Vector3 total = new Vector3(0, 0, 0);
		foreach (Vector3 movement in movementContributions)
		{
			total += movement;
		}
		movementContributions.Clear();
		return total;
	}

	/*
	 * Grab components and objects
	 * 
	 */
	void Awake(){
		rb = GetComponent<Rigidbody>();
		cameraMain = transform.Find("Main Camera").transform;
		bulletSpawn = cameraMain.Find ("BulletSpawn").transform;
		ignoreLayer = 1 << LayerMask.NameToLayer ("Player");

		mainCamera = transform.Find("Main Camera");

	}
	private Vector3 slowdownV;
	private Vector2 horizontalMovement;
	/*
	* Raycasting for meele attacks and input movement handling here.
	*/
	void FixedUpdate(){
		if (ViewModel.Instance.paused) return;

		// if player collides with ladders collider then isOnLadders bool is updated
		rb.useGravity = !isOnLadders;

		RaycastForMeleeAttacks ();

		PlayerMovementLogic ();

		CollectExternalMovementInertia();

		LadderLogic();

		WallRunLogic();

		StairsLogic();

		grounded = false; // oncollisionstay will set this back to true before next FixedUpdate executes
	}

	void CollectExternalMovementInertia()
    {
		if (grounded)
		{
			transform.position += collectContributions() * Time.deltaTime;
		}
	}

	void LadderLogic()
    {
		if (isOnLadders)
		{
			float cameraXRotation = cameraMain.rotation.eulerAngles.x;
			Vector3 velo = rb.velocity;
			velo.y = 2 * Input.GetAxis("Vertical");
			if (cameraXRotation > 30 && cameraXRotation < 70)
			{
				velo.y *= -1;
			}

			rb.velocity = velo;
		}
	}

	void StairsLogic()
	{
		bool stairsHit = false;
		RaycastHit[] hits;
		hits = Physics.RaycastAll(transform.position, transform.forward-transform.up);
		foreach (RaycastHit _hit in hits)
		{
			if (stairsHit) break;
			if (grounded && _hit.collider.tag == "Stairs")
			{
				float mag = rb.velocity.magnitude;
				if (mag > maxSpeed * 0.9f)
				{
					float normalizer = mag / (maxSpeed * 0.9f);
					rb.velocity = new Vector3(rb.velocity.x / normalizer, rb.velocity.y / normalizer, rb.velocity.z / normalizer);
				}
				stairsHit = true;
				if (grounded&Input.GetAxis("Vertical") > 0f) rb.velocity += new Vector3(0f, 0.2f, 0f);
			}
		}
}

	
	bool walljumpReady = false;
	float walljumpDuration = 0f;
	float walljumpCooldown = 0f;
	bool firstRun = true;
	Vector3 wallRunVec;

	void WallRunLogic()
    {
		Debug.DrawRay(transform.position, transform.forward*6, Color.blue);
		Debug.DrawRay(transform.position, transform.right, Color.blue);
		Debug.DrawRay(transform.position, transform.right, Color.blue);
		walljumpReady = Input.GetKey(KeyCode.Space);
		if (!walljumpReady || isOnLadders || walljumpCooldown>0f)
        {
			walljumpDuration = 0f;
			walljumpCooldown -= Time.deltaTime;
			walljumpCooldown = Mathf.Max(walljumpCooldown, 0);
			return;
        }
		Debug.Log("WJ: passed first check");
		Debug.Log($"WJ: grounded:{grounded} walljumpDuration:{walljumpDuration} velocity:{rb.velocity.magnitude}");
		bool waitForJump = firstRun && Mathf.Abs(rb.velocity.y) > 0.5;
		if (!waitForJump && !grounded && walljumpDuration<3.0f && rb.velocity.magnitude>=maxSpeed/2 && Input.GetAxis("Vertical")>0)
		{
			Debug.Log("WJ: passed second check");

			walljumpDuration += Time.deltaTime;

			RaycastHit forwardHit;
			bool somethingIsForward = Physics.Raycast(transform.position, transform.forward, out forwardHit, 6f);
			if ((!somethingIsForward || !forwardHit.transform.name.Contains("Wall")) && firstRun) {
				walljumpCooldown = 2f;
				return;
            }
			Debug.Log("WJ: got forward hit");
			string rotateDirection = "clockwise";
			RaycastHit wallInfo;
			if (!Physics.Raycast(transform.position, transform.right * -1f, out wallInfo, 1.5f) || !wallInfo.transform.name.Contains("Wall"))
            {
				Debug.Log("WJ: No hit left");
				rotateDirection = "counterclockwise";
				if (!Physics.Raycast(transform.position, transform.right, out wallInfo, 1.5f) || !wallInfo.transform.name.Contains("Wall"))
                {
					Debug.Log("WJ: No hit right");
					walljumpDuration = 0f;
					firstRun = true;
					rb.useGravity = true;
					return;
                }
			}
			Debug.Log($"WJ: Got hit {wallInfo.transform.name}");
			float angleDelta = 90- Mathf.Rad2Deg*Mathf.Atan2(forwardHit.distance, wallInfo.distance);
			if (rotateDirection=="clockwise") {
				Debug.Log("WJ: got wall hit left");
				rb.velocity = Quaternion.Euler(0,360-angleDelta,0)*rb.velocity;
			}
			if (rotateDirection=="counterclockwise")
            {
				Debug.Log("WJ: got wall hit right");
				rb.velocity = Quaternion.Euler(0, angleDelta, 0) * rb.velocity;
            }
			if (firstRun)
			{
				wallRunVec = new Vector3(rb.velocity.x, 0, rb.velocity.z);
				firstRun = false;
			}
			rb.velocity = wallRunVec;
			Debug.Log($"WJ: velocity is {rb.velocity}");
			rb.useGravity = false;

		}
		else
        {
			Debug.Log($"WJ: failed second check waitForJump:{waitForJump}");
			walljumpDuration = 1.0f;
			firstRun = true;
			rb.useGravity = true;
		}
		
    }

	/*
	* Accordingly to input adds force and if magnitude is bigger it will clamp it.
	* If player leaves keys it will deaccelerate
	*/
	void PlayerMovementLogic(){
		bool blockedForward = Physics.Raycast(transform.position, transform.forward, 1f);

		currentSpeed = rb.velocity.magnitude;
		horizontalMovement = new Vector2 (rb.velocity.x, rb.velocity.z);
		if (horizontalMovement.magnitude > maxSpeed){
			horizontalMovement = horizontalMovement.normalized;
			horizontalMovement *= maxSpeed;    
		}
		rb.velocity = new Vector3 (
			horizontalMovement.x,
			rb.velocity.y,
			horizontalMovement.y
		);
		if (grounded){
			rb.velocity = Vector3.SmoothDamp(rb.velocity,
				new Vector3(0,rb.velocity.y,0),
				ref slowdownV,
				deaccelerationSpeed);
		}
		float forwardForce = Input.GetAxis("Vertical") * accelerationSpeed * Time.deltaTime;
		if (forwardForce > 0 && blockedForward) forwardForce = 0;
		if (grounded) {
			rb.AddRelativeForce (Input.GetAxis ("Horizontal") * accelerationSpeed * Time.deltaTime, 0, forwardForce);
		} else {
			rb.AddRelativeForce (Input.GetAxis ("Horizontal") * accelerationSpeed / 2 * Time.deltaTime, 0, forwardForce/2);
		}
		/*
		 * Slippery issues fixed here
		 */
		if (Input.GetAxis ("Horizontal") != 0 || Input.GetAxis ("Vertical") != 0) {
			deaccelerationSpeed = 0.5f;
		} else {
			deaccelerationSpeed = 0.1f;
		}

	}
	/*
	* Handles jumping and ads the force and sounds.
	*/
	void Jumping(){
		if (Input.GetKeyDown (KeyCode.Space) && grounded) {
			rb.AddRelativeForce (Vector3.up * jumpForce);
			if (_jumpSound)
			{
				if (!ViewModel.Instance.paused)
				{
					_jumpSound.volume = ViewModel.Instance.sfxVolume;
					_jumpSound.Play();
				}
			}
			else
				print("Missig jump sound.");
			_walkSound.Stop ();
			_runSound.Stop ();
		}
	}
	/*
	* Update loop calling other stuff
	*/
	void Update(){
		if (Input.GetKeyDown(KeyCode.Escape))
        {
			ViewModel.Instance.PressEscape();
        }
		if (ViewModel.Instance.paused) return;

		Jumping ();

		Crouching();

		WalkingSound ();

		Debug.DrawRay(transform.position, transform.forward-transform.up, Color.red, 0.1f);


	}//end update

	/*
	* Checks if player is grounded and plays the sound accorindlgy to his speed
	*/
	void WalkingSound(){
		if (_walkSound && _runSound) {
			if (RayCastGrounded ()) { //for walk sounsd using this because suraface is not straigh										  	
				if (currentSpeed > 1) {
					// if shift is not pressed->maxSpeed==3->play walk and stop playing run
					if (maxSpeed == 3) {
						if (!_walkSound.isPlaying) {
							if (!ViewModel.Instance.paused)
							{
								_walkSound.volume = ViewModel.Instance.sfxVolume;
								_walkSound.Play();
							}
							_runSound.Stop ();
						}					
					// if shift is pressed->maxSpeed==5->play run sound and stop playing walk
					} else if (maxSpeed == 5) {
						if (!_runSound.isPlaying) {
							_walkSound.Stop ();
							if (!ViewModel.Instance.paused)
							{
								_runSound.volume = ViewModel.Instance.sfxVolume;
								_runSound.Play();
							}
						}
					}
				// if not moving fast enough do not play any movement sound
				} else {
					_walkSound.Stop ();
					_runSound.Stop ();
				}
			} else { // if not on ground
				_walkSound.Stop ();
				_runSound.Stop ();
			}
		} else {
			print ("Missing walk and running sounds.");
		}

	}
	/*
	* Raycasts down to check if we are grounded along the gorunded method() because if the
	* floor is curvy it will go ON/OFF constatly this assures us if we are really grounded
	*/
	private bool RayCastGrounded(){
		RaycastHit groundedInfo;
		if(Physics.Raycast(transform.position, transform.up *-1.2f, out groundedInfo, 1.1f)){
			//Debug.DrawRay (transform.position, transform.up * -1.2f, Color.red, 4.0f);
			//  vector(0,-1,0) has hit something with transform -> player is grounded
			if(groundedInfo.transform != null){
				return true;
			}
			else{
				return false;
			}
		}
		return false;
	}

	/*
	* If player toggle the crouch it will scale the player to appear that is crouching
	*/
	void Crouching(){
		if(Input.GetKey(KeyCode.C)){
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1,0.6f,1), Time.deltaTime * 15);
		}
		else{
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1,1,1), Time.deltaTime * 15);

		}
	}


	[Tooltip("The maximum speed you want to achieve")]
	public int maxSpeed = 5;
	[Tooltip("The higher the number the faster it will stop")]
	public float deaccelerationSpeed = 15.0f;


	[Tooltip("Force that is applied when moving forward or backward")]
	public float accelerationSpeed = 50000.0f;


	[Tooltip("Tells us weather the player is grounded or not.")]
	public bool grounded;

	/*
	 * does detailed check whether player is standing on ground
	 * it is necessary because standing very near a wall will cause incorrect registering that the player is grounded
	 */

	bool DoDetailedGroundCheck()
    {
		Debug.DrawRay(transform.position, Vector3.up*-5f, Color.black, 1f, false);
		RaycastHit floorinfo;
		Vector3 rayvector = Quaternion.Euler(15, 0, 0)* (transform.up * -0.3f);
		if (Physics.Raycast(transform.position, rayvector, out floorinfo, 1.1f, ~ignoreLayer) && floorinfo.transform)
		{
			rayvector = Quaternion.Euler(-15, 0, 0) * (transform.up * -0.3f);
			if (Physics.Raycast(transform.position, rayvector, out floorinfo, 1.1f, ~ignoreLayer) && floorinfo.transform) return true;
		}

		rayvector = Quaternion.Euler(0, 0, 15) * (transform.up * -0.3f);
		if (Physics.Raycast(transform.position, rayvector, out floorinfo, 1.1f, ~ignoreLayer) && floorinfo.transform)
        {
			rayvector = Quaternion.Euler(0, 0, -15) * (transform.up * -0.3f);
			if (Physics.Raycast(transform.position, rayvector, out floorinfo, 1.1f, ~ignoreLayer) && floorinfo.transform) return true;
		}

		return false;
	}

	/*
	* checks if our player is contacting the ground in the angle less than 60 degrees
	*	if it is, set groudede to true
	*/
	void OnCollisionStay(Collision other){
		foreach (ContactPoint contact in other.contacts){
			if (Vector3.Angle(contact.normal,Vector3.up) < 60){
				RaycastHit wallInfo;
				Vector3 raycastvector = Quaternion.Euler(175, 0, 0) *(Vector3.up*1.5f);
				bool raycast=Physics.Raycast(transform.position, raycastvector, out wallInfo);
				if (!grounded && Physics.Raycast(transform.position, Vector3.up * -1.6f, out wallInfo))
				{
					grounded = true;
				}
				
			}
		}
	}
	/*
	* On collision exit set grounded to false
	*/
	void OnCollisionExit ()
	{
		grounded = false;
	}


	RaycastHit hitInfo;
	private float meleeAttack_cooldown;
	private string currentWeapo;
	[Tooltip("Put 'Player' layer here")]
	[Header("Shooting Properties")]
	private LayerMask ignoreLayer;//to ignore player layer
	Ray ray1, ray2, ray3, ray4, ray5, ray6, ray7, ray8, ray9;
	private float rayDetectorMeeleSpace = 0.15f;
	private float offsetStart = 0.05f;
	[Tooltip("Put BulletSpawn gameobject here, palce from where bullets are created.")]
	[HideInInspector]
	public Transform bulletSpawn; //from here we shoot a ray to check where we hit him;
	/*
	* This method casts 9 rays in different directions. ( SEE scene tab and you will see 9 rays differently coloured).
	* Used to widley detect enemy infront and increase meele hit detectivity.
	* Checks for cooldown after last preformed meele attack.
	*/


	public bool been_to_meele_anim = false;
	private void RaycastForMeleeAttacks(){




		if (meleeAttack_cooldown > -5) {
			meleeAttack_cooldown -= 1 * Time.deltaTime;
		}


		if (GetComponent<GunInventory> ().currentGun) {
			if (GetComponent<GunInventory> ().currentGun.GetComponent<GunScript> ()) 
				currentWeapo = "gun";
		}

		//middle row
		ray1 = new Ray (bulletSpawn.position + (bulletSpawn.right*offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace));
		ray2 = new Ray (bulletSpawn.position - (bulletSpawn.right*offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace));
		ray3 = new Ray (bulletSpawn.position, bulletSpawn.forward);
		//upper row
		ray4 = new Ray (bulletSpawn.position + (bulletSpawn.right*offsetStart) + (bulletSpawn.up*offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace) + (bulletSpawn.up * rayDetectorMeeleSpace));
		ray5 = new Ray (bulletSpawn.position - (bulletSpawn.right*offsetStart) + (bulletSpawn.up*offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace) + (bulletSpawn.up * rayDetectorMeeleSpace));
		ray6 = new Ray (bulletSpawn.position + (bulletSpawn.up*offsetStart), bulletSpawn.forward + (bulletSpawn.up * rayDetectorMeeleSpace));
		//bottom row
		ray7 = new Ray (bulletSpawn.position + (bulletSpawn.right*offsetStart) - (bulletSpawn.up*offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace) - (bulletSpawn.up * rayDetectorMeeleSpace));
		ray8 = new Ray (bulletSpawn.position - (bulletSpawn.right*offsetStart) - (bulletSpawn.up*offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace) - (bulletSpawn.up * rayDetectorMeeleSpace));
		ray9 = new Ray (bulletSpawn.position -(bulletSpawn.up*offsetStart), bulletSpawn.forward - (bulletSpawn.up * rayDetectorMeeleSpace));

		Debug.DrawRay (ray1.origin, ray1.direction, Color.cyan);
		Debug.DrawRay (ray2.origin, ray2.direction, Color.cyan);
		Debug.DrawRay (ray3.origin, ray3.direction, Color.cyan);
		Debug.DrawRay (ray4.origin, ray4.direction, Color.red);
		Debug.DrawRay (ray5.origin, ray5.direction, Color.red);
		Debug.DrawRay (ray6.origin, ray6.direction, Color.red);
		Debug.DrawRay (ray7.origin, ray7.direction, Color.yellow);
		Debug.DrawRay (ray8.origin, ray8.direction, Color.yellow);
		Debug.DrawRay (ray9.origin, ray9.direction, Color.yellow);

		if (GetComponent<GunInventory> ().currentGun) {
			if (GetComponent<GunInventory> ().currentGun.GetComponent<GunScript> ().meeleAttack == false) {
				been_to_meele_anim = false;
			}
			if (GetComponent<GunInventory> ().currentGun.GetComponent<GunScript> ().meeleAttack == true && been_to_meele_anim == false) {
				been_to_meele_anim = true;
				//	if (isRunning == false) {
				StartCoroutine ("MeeleAttackWeaponHit");
				//	}
			}
		}

	}

	/*
	 *Method that is called if the waepon hit animation has been triggered the first time via Q input
	 *and if is, it will search for target and make damage
	 */
	IEnumerator MeeleAttackWeaponHit(){
		if (Physics.Raycast (ray1, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray2, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray3, out hitInfo, 2f, ~ignoreLayer)
			|| Physics.Raycast (ray4, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray5, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray6, out hitInfo, 2f, ~ignoreLayer)
			|| Physics.Raycast (ray7, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray8, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast (ray9, out hitInfo, 2f, ~ignoreLayer)) {
			//Debug.DrawRay (bulletSpawn.position, bulletSpawn.forward + (bulletSpawn.right*0.2f), Color.green, 0.0f);
			if (hitInfo.transform.tag=="Dummie") {
				Transform _other = hitInfo.transform.root.transform;
				if (_other.transform.tag == "Dummie") {
					print ("hit a dummie");
				}
				InstantiateBlood(hitInfo,false);
			}
		}
		yield return new WaitForEndOfFrame ();
	}

	[Header("BloodForMelleAttaacks")]
	RaycastHit hit;//stores info of hit;
	[Tooltip("Put your particle blood effect here.")]
	public GameObject bloodEffect;//blod effect prefab;
	/*
	* Upon hitting enemy it calls this method, gives it raycast hit info 
	* and at that position it creates our blood prefab.
	*/
	void InstantiateBlood (RaycastHit _hitPos,bool swordHitWithGunOrNot) {		

		if (currentWeapo == "gun") {
			GunScript.HitMarkerSound ();

			if (_hitSound)
			{
				if (!ViewModel.Instance.paused)
				{
					_hitSound.volume = ViewModel.Instance.sfxVolume;
					_hitSound.Play();
				}
			}
			else
				print("Missing hit sound");
			
			if (!swordHitWithGunOrNot) {
				if (bloodEffect)
					Instantiate (bloodEffect, _hitPos.point, Quaternion.identity);
				else
					print ("Missing blood effect prefab in the inspector.");
			}
		} 
	}

    private GameObject myBloodEffect;


	[Header("Player SOUNDS")]
	[Tooltip("Jump sound when player jumps.")]
	public AudioSource _jumpSound;
	[Tooltip("Sound while player makes when successfully reloads weapon.")]
	public AudioSource _freakingZombiesSound;
	[Tooltip("Sound Bullet makes when hits target.")]
	public AudioSource _hitSound;
	[Tooltip("Walk sound player makes.")]
	public AudioSource _walkSound;
	[Tooltip("Run Sound player makes.")]
	public AudioSource _runSound;


}

