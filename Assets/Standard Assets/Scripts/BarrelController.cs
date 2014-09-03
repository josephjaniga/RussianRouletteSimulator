using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* 
 * Mechanics i want:
 * -----------------------
 * load your own rounds
 * spin the barrel
 * shoot
 * misfire (extra life mechanic?) 1 in 250? accurate?
 *
 * story mode? level 1 - one round / level 6 - full barrel?
 * blindfold level?
 *
 * obviously audio Win / Loss
 */


public class BarrelController : MonoBehaviour {


	public int Level;

	Rigidbody2D rb;

	public TextMesh title;
	public TextMesh description;
	
	public Transform[] bullets = new Transform[6];

	public Transform shell; 

	public int shellCount = 0;
	public List<Transform> shells;


	private AudioSource audio;
	public AudioClip spinClickSound;
	public AudioClip gunCockSound;
	public AudioClip gunFireSound;
	public AudioClip gunDryFireSound;	
	public AudioClip applause;
		
		
	public float nextClick = 0.0f;
	public float rotesMCgoats = 0.0f;


	public int[] barrels = new int[6];


	// 0 1 2 3
	public enum barrelStates { empty, full, hover, discharged };


	public bool isSpinning = false;
	public bool canSpin = false;
	public bool finishedSpinning = false;
	public bool doneLoading = false;
	
	public bool hasFired = false;
	
	
	public bool YOU_WIN = false;
	public bool YOU_LOSE = false;


	public GameObject DeathScreen;


	// Use this for initialization
	void Start (){
		
		doneLoading = false;
		
		this.rb = this.GetComponent<Rigidbody2D>();
		
		audio = this.GetComponent<AudioSource>();
		audio.clip = spinClickSound;
		
		shells = new List<Transform>();
		populateBullets();
		zeroBullets();

	}

	void Awake () {


		for ( int i = 0; i < Level; i++ ){

			/*
			Transform temp = (Transform) Instantiate(shell, new Vector3(-13f, -0.2f, 0), Quaternion.identity);
			shellCount++;
			shells.Add(temp);
			temp.GetComponent<CartridgeController>().spin();
			*/
			
			addShell();
			
		}

	
	}
	
	
	// Update is called once per frame
	void Update(){
		
		//debugga.text = "" + rb.angularVelocity;
		
		// check the velocity
		// if it slows enough snap the rotation at the next barrel
		if ( rb.angularVelocity <= 100 ) {
			
			// snap the rotation to the nearest barrel always
			if ( rb.rotation % 60.0f >= -10.0f && rb.rotation % 60.0f <= 10.0f ){
				rb.angularVelocity = 0;
				float remainder = rb.rotation % 60.0f;
				rb.rotation = rb.rotation - remainder;
			}

		} 
		
		
		// everytime rb rotates 60.0f
		
		rotesMCgoats = rb.rotation;
		
		if ( nextClick >= 420.0f ){
			
			nextClick = nextClick % 360.0f;
			
		}
		
		if ( rb.rotation >= nextClick ){
			
			nextClick += 60.0f;
			
			audio.clip = spinClickSound;
			audio.Play();
			
		}
		
		

		// reset the barrel to the center if it gets pushed
		if ( rb.transform.position != Vector3.zero ) {
			rb.transform.position = Vector3.zero;
		}
		
		
		/*
		// press "1" get a bullet
		if ( Input.GetKeyDown(KeyCode.Alpha1) ){
			addShell();
		}
		*/
		
		
		// draw the barrel states
		for ( int i = 0; i < 6; i++ ) {
			
			if ( barrels[i] == (int)barrelStates.empty  ) {
				
				bullets[i].renderer.enabled = false;
				bullets[i].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
				
			} else if ( barrels[i] == (int)barrelStates.full ) { 
				
				bullets[i].renderer.enabled = true;
				bullets[i].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
				
			} else if ( barrels[i] == (int)barrelStates.hover ) {
				
				bullets[i].renderer.enabled = true;
				bullets[i].GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, 0.35f);
				
			} else if ( barrels[i] == (int)barrelStates.discharged ) {
				
				bullets[i].renderer.enabled = true;
				bullets[i].GetComponent<SpriteRenderer>().color = new Color(0.39f, 0.35f, 0.3f, 1f);
				
			}
			
		}
		
		
		
		// set spinning when certain torque is reached
		// set can spin to false if spining and torque falls below a value
		// after spinning stops, check to see
		
		if ( !doneLoading ) {
			
			if ( getChamberCount() == Level ){
				rb.angularVelocity = 0;
				canSpin = true;
				isSpinning = false;
				doneLoading = true;
			}

		}
		
		// "Space to Spin"
		if ( Input.GetKeyDown(KeyCode.Space) && canSpin ){
			// left click on object to spin barrel
			rb.AddTorque(1550);

		}
		
		if ( rb.angularVelocity < 10 && isSpinning ){
			canSpin = false;
		}
		
		
		if ( rb.angularVelocity == 0 ){
			isSpinning = false;
		} else { 
			isSpinning = true;
		}

		
		
		// if loaded spun and stopped
		if (  rb.angularVelocity == 0 && !isSpinning && !canSpin && doneLoading ) {

			if ( !hasFired ){
				StartCoroutine( MyCoroutine() );		
				hasFired = true;		
			}

		}
		
		

		
	}
	
	
	IEnumerator MyCoroutine (){
				
		// cock the gun
		audio.clip = gunCockSound;
		audio.Play();
		
		yield return new WaitForSeconds( (float)1.5 );
		
		if ( barrels[getNorthChamber()-1] == (int)barrelStates.full ){
			
			// shoot the gun
			audio.clip = gunFireSound;
			audio.Play();
			YOU_WIN = false;
			YOU_LOSE = true;
			DeathScreen.SetActive(true);
			
		} else {
			
			// shoot the gun
			audio.clip = gunDryFireSound;
			audio.Play();
			YOU_WIN = true;
			YOU_LOSE = false;
			
		}
		
		yield return new WaitForSeconds( (float)1.0 );
		
		
		if ( YOU_WIN && !YOU_LOSE ){
			// shoot the gun
			audio.clip = applause;
			audio.Play();
			StartCoroutine(AdvanceLevel());
		}
		
		
	}
	
	IEnumerator AdvanceLevel (){
				
		Level++;
		
		yield return new WaitForSeconds( (float)0.5 );
		
		reset();

	}
	
	IEnumerator Restart (){
		
		Level = 1;
		reset();
		yield return new WaitForSeconds( (float)0.5 );
		
	}
	
	
	void reset(){
		
		// set the text
		title.text = "Level " + Level;
		description.text = "Load " + Level + " Cartridges - Spin the Barrel - Pull the Trigger";
		
		// set the bools
		hasFired = false;
		isSpinning = false;
		canSpin = false;
		finishedSpinning = false;
		doneLoading = false;
		YOU_WIN = false;
		YOU_LOSE = false;
		
		// reset the shell count
		shellCount = 0;
		shells = null;
		
		// clear the barrels and shells
		zeroBullets();
		
		Awake();
		
		DeathScreen.SetActive(false);
		
	}
	
	
	int getChamberCount (){
		
		int x = 0;
		for ( int i = 0; i < 6; i++ ) {
			if ( bullets[i].renderer.enabled == true ){
				x++;
			}
		}
		
		Debug.Log(x);
		
		return x;
		
	}
	
	int getNorthChamber (){
		
		int x = 0;
		
		x = (int)( (rotesMCgoats + 60.0) / 60.0 );
		
		return x;
		
	}


	// assign all of the bullets to the array references
	void populateBullets(){

		bullets[0] = this.transform.Find("bullet1");
		bullets[1] = this.transform.Find("bullet2");
		bullets[2] = this.transform.Find("bullet3");
		bullets[3] = this.transform.Find("bullet4");
		bullets[4] = this.transform.Find("bullet5");
		bullets[5] = this.transform.Find("bullet6");
		
	}
	
	
	// clear all the chambers
	public void zeroBullets(){
		
		for ( int i = 0; i < 6; i++ ) {
			bullets[i].renderer.enabled = false;
			bullets[i].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
			barrels[i] = (int)barrelStates.empty;
		}
		
	}
	
	
	public int chamberHover(){
		
		int chamber = -1;
		
		// loop the bullets
		for ( int i = 0; i < 6; i++ ) {
			
			// get the chamber collider
			CircleCollider2D collider = bullets[i].GetComponent<CircleCollider2D>();

			Vector3 p = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, -0.2f);
			
			if ( collider.bounds.Contains( p ) ){
				chamber = i+1;
			}
			
		}
		
		return chamber;
		
	}



    void OnDrawGizmos() {
		
		Vector2 target = new Vector2( Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        Gizmos.color = Color.white;
        Gizmos.DrawSphere( new Vector3(target.x, target.y, 0f), 0.1f);
		
    }
	
	
	void addShell( bool random = false ){
		Debug.Log("shell added");
		
		Transform temp = (Transform) Instantiate(shell, new Vector3(-10f, -0.4f, 0), Quaternion.identity);
		shellCount++;
		//shells.Add(temp);
		
		if ( random ){
			temp.GetComponent<CartridgeController>().randomizeShell();			
		}

		temp.GetComponent<CartridgeController>().spin();
	}
	
	
	void OnMouseDown(){
		
		
		if ( YOU_LOSE ){
			
			StartCoroutine(Restart());
			
		}
		
		
		// is the player free to remove the bullet
		if ( canSpin ) {
			
			//remove the bullets from the barrel on click
			if ( chamberHover() != -1 &&  ( barrels[chamberHover()-1] == (int)barrelStates.full || barrels[chamberHover()-1] == (int)barrelStates.discharged ) ){
			
				Vector3 p = Camera.main.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y, -0.1f) );
				p.z = -0.1f;
			
				// create a shell prefab
				// set the position to mouse cursor
				Transform temp = (Transform) Instantiate(shell, p, Quaternion.identity);
			
				if ( barrels[chamberHover()-1] == (int)barrelStates.discharged ) {
				
					// set the shell to discharged
					temp.GetComponent<CartridgeController>().shellState = (int)CartridgeController.cartridgeStates.discharged;
							
				} else {
				
					// set the shell to discharged
					temp.GetComponent<CartridgeController>().shellState = (int)CartridgeController.cartridgeStates.full;				
				
				}
			
			
				// set the barrel state to empty
				barrels[chamberHover()-1] = (int)barrelStates.empty;

				// increment the shells count
				shellCount++;
				// add it to the shells array
				shells.Add(temp);
			
				//temp.GetComponent<CartridgeController>().clickedMe = true;
			
			
			}
						
		}
		
	}


}
