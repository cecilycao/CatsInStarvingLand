using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // include so we can load new scenes
//using UnityStandardAssets.CrossPlatformInput;

public class CharacterController2D : MonoBehaviour {

	// player controls
	[Range(0.0f, 10.0f)] // create a slider in the editor and set limits on moveSpeed
	public float moveSpeed = 3f;

	public float jumpForce = 600f;

	// player health
	public int playerHealth = 1;

	// LayerMask to determine what is considered ground for the player
	public LayerMask whatIsGround;

	// Transform just below feet for checking if player is grounded
	public Transform groundCheck;

	// player can move?
	// we want this public so other scripts can access it but we don't want to show in editor as it might confuse designer
	[HideInInspector]
	public bool playerCanMove = true;

	// SFXs
	public AudioClip coinSFX;
	public AudioClip deathSFX;
	public AudioClip fallSFX;
	public AudioClip jumpSFX;
	public AudioClip victorySFX;

	// private variables below

	// store references to components on the gameObject
	Transform _transform;
	Rigidbody2D _rigidbody;
	Animator _animator;
	AudioSource _audio;

	// hold player motion in this timestep
	float _vx;
	float _vy;

	// player tracking
	public bool _facingRight = true;
	public bool _isGrounded = false;
	public bool _isRunning = false;
	public bool _canDoubleJump = false;

	// store the layer the player is on (setup in Awake)
	int _playerLayer;

	// number of layer that Platforms are on (setup in Awake)
	int _platformLayer;
	
	void Awake () {
		// get a reference to the components we are going to be changing and store a reference for efficiency purposes
		_transform = GetComponent<Transform> ();
		
		_rigidbody = GetComponent<Rigidbody2D> ();
		if (_rigidbody==null) // if Rigidbody is missing
			Debug.LogError("Rigidbody2D component missing from this gameobject");
		
		_animator = GetComponent<Animator>();
		if (_animator==null) // if Animator is missing
			Debug.LogError("Animator component missing from this gameobject");
		
		_audio = GetComponent<AudioSource> ();
		if (_audio==null) { // if AudioSource is missing
			Debug.LogWarning("AudioSource component missing from this gameobject. Adding one.");
			// let's just add the AudioSource component dynamically
			_audio = gameObject.AddComponent<AudioSource>();
		}

		// determine the player's specified layer
		_playerLayer = this.gameObject.layer;

		// determine the platform's specified layer
		_platformLayer = LayerMask.NameToLayer("Ground");
	}

	// this is where most of the player controller magic happens each game event loop
	void Update()
	{
		// exit update if player cannot move or game is paused
		if (!playerCanMove || (Time.timeScale == 0f))
			return;

		// determine horizontal velocity change based on the horizontal CrossPlatformInputManager
#if UNITY_STANDALONE || UNITY_WEBPLAYER

        //Get input from the input manager ,store in horizontal to set x axis move direction
        _vx = Input.GetAxisRaw("Horizontal");

        //Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
        _vy = Input.GetAxisRaw("Vertical");

        //Check if moving horizontally, if so set vertical to zero.
        if (_vx != 0)
        {
            _vy = 0;
        }
        //Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			
			//Check if Input has registered more than zero touches
			if (Input.touchCount > 0)
			{
				//Store the first touch detected.
				Touch myTouch = Input.touches[0];
				
				//Check if the phase of that touch equals Began
				if (myTouch.phase == TouchPhase.Began)
				{
					//If so, set touchOrigin to the position of that touch
					touchOrigin = myTouch.position;
				}
				
				//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
				else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
				{
					//Set touchEnd to equal the position of this touch
					Vector2 touchEnd = myTouch.position;
					
					//Calculate the difference between the beginning and end of the touch on the x axis.
					float x = touchEnd.x - touchOrigin.x;
					
					//Calculate the difference between the beginning and end of the touch on the y axis.
					float y = touchEnd.y - touchOrigin.y;
					
					//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
					touchOrigin.x = -1;
					
					//Check if the difference along the x axis is greater than the difference along the y axis.
					if (Mathf.Abs(x) > Mathf.Abs(y))
						//If x is greater than zero, set horizontal to 1, otherwise set it to -1
						_vx = x > 0 ? 1 : -1;
					else
						//If y is greater than zero, set horizontal to 1, otherwise set it to -1
						_vy = y > 0 ? 1 : -1;
				}
			}
			
#endif //End of mobile platform dependendent compilation section started above with #elif

        // Determine if running based on the horizontal movement
        if (_vx != 0) 
		{
			_isRunning = true;
		} else {
			_isRunning = false;
		}

        //todo: add animator
		// set the running animation state
		//_animator.SetBool("Running", _isRunning);

		// get the current vertical velocity from the rigidbody component
		_vy = _rigidbody.velocity.y;

		// Check to see if character is grounded by raycasting from the middle of the player
		// down to the groundCheck position and see if collected with gameobjects on the
		// whatIsGround layer
		_isGrounded = Physics2D.Linecast(_transform.position, groundCheck.position, whatIsGround);  

		if (_isGrounded) {
			_canDoubleJump = true;
		}

		// todo: Set the grounded animation states
		//_animator.SetBool("Grounded", _isGrounded);

		if (_isGrounded && Input.GetButtonDown ("Jump")) { // If grounded AND jump button pressed, then allow the player to jump
            print("jump!");
            doJump();
		} else if (_canDoubleJump && Input.GetButtonDown ("Jump")) {
			doJump ();
			_canDoubleJump = false;
		}
	
		// If the player stops jumping mid jump and player is not yet falling
		// then set the vertical velocity to 0 (he will start to fall from gravity)
		if(Input.GetButtonUp("Jump") && _vy>0f)
		{
			_vy = 0f;
		}

		// Change the actual velocity on the rigidbody
		_rigidbody.velocity = new Vector2(_vx * moveSpeed, _vy);

		// if moving up then don't collide with platform layer
		// this allows the player to jump up through things on the platform layer
		// NOTE: requires the platforms to be on a layer named "Platform"
		//Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, (_vy > 0.0f)); 
	}

	void doJump() {
		// reset current vertical motion to 0 prior to jump
		_vy = 0f;
		// add a force in the up direction
		_rigidbody.AddForce (new Vector2 (0, jumpForce));
        print("do jump!");
		//todo: play the jump sound
		//PlaySound (jumpSFX);
	}

	// Checking to see if the sprite should be flipped
	// this is done in LateUpdate since the Animator may override the localScale
	// this code will flip the player even if the animator is controlling scale
	void LateUpdate()
	{
		// get the current scale
		Vector3 localScale = _transform.localScale;

		if (_vx > 0) // moving right so face right
		{
			_facingRight = true;
		} else if (_vx < 0) { // moving left so face left
			_facingRight = false;
		}

		// check to see if scale x is right for the player
		// if not, multiple by -1 which is an easy way to flip a sprite
		if (((_facingRight) && (localScale.x<0)) || ((!_facingRight) && (localScale.x>0))) {
			localScale.x *= -1;
		}

		// update the scale
		_transform.localScale = localScale;
	}

	// if the player collides with a MovingPlatform, then make it a child of that platform
	// so it will go for a ride on the MovingPlatform
	//void OnCollisionEnter2D(Collision2D other)
	//{
	//	if (other.gameObject.tag=="MovingPlatform")
	//	{
	//		this.transform.parent = other.transform;
	//	}
	//}

	// if the player exits a collision with a moving platform, then unchild it
	//void OnCollisionExit2D(Collision2D other)
	//{
	//	if (other.gameObject.tag=="MovingPlatform")
	//	{
	//		this.transform.parent = null;
	//	}
	//}

	// do what needs to be done to freeze the player
 	void FreezeMotion() {
		playerCanMove = false;
        _rigidbody.velocity = new Vector2(0,0);
		_rigidbody.isKinematic = true;
	}

	// do what needs to be done to unfreeze the player
	void UnFreezeMotion() {
		playerCanMove = true;
		_rigidbody.isKinematic = false;
	}

	// play sound through the audiosource on the gameobject
	void PlaySound(AudioClip clip)
	{
		_audio.PlayOneShot(clip);
	}

	// public function to apply damage to the player
	public void ApplyDamage (int damage) {
		if (playerCanMove) {
			playerHealth -= damage;

			if (playerHealth <= 0) { // player is now dead, so start dying
				//PlaySound(deathSFX);
				StartCoroutine (KillPlayer ());
			}
		}
	}

	// public function to kill the player when they have a fall death
	public void FallDeath () {
		if (playerCanMove) {
			playerHealth = 0;
			//PlaySound(fallSFX);
			StartCoroutine (KillPlayer ());
		}
	}

	// coroutine to kill the player
	IEnumerator KillPlayer()
	{
		if (playerCanMove)
		{
			// freeze the player
			FreezeMotion();

			//todo: play the death animation
			//_animator.SetTrigger("Death");
			
			// After waiting tell the GameManager to reset the game
			yield return new WaitForSeconds(2.0f);

            /*todo：
			if (GameManager.gm) // if the gameManager is available, tell it to reset the game
				GameManager.gm.ResetGame();
			else // otherwise, just reload the current level
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                */
		}
	}

	public void CollectCoin(int amount) {
		PlaySound(coinSFX);
        /*
		if (GameManager.gm) // add the points through the game manager, if it is available
			GameManager.gm.AddPoints(amount);*/
	}

	// public function on victory over the level
	public void Victory() {
		PlaySound(victorySFX);
		FreezeMotion ();
        /*
		_animator.SetTrigger("Victory");

		if (GameManager.gm) // do the game manager level compete stuff, if it is available
			GameManager.gm.LevelCompete();
            */
	}

	// public function to respawn the player at the appropriate location
	public void Respawn(Vector3 spawnloc) {
		UnFreezeMotion();
		playerHealth = 1;
		_transform.parent = null;
		_transform.position = spawnloc;
		//todo:
        //_animator.SetTrigger("Respawn");
	}

    public void EnemeyBounce() {
        doJump();
    }
}
