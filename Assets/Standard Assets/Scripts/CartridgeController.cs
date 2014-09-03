using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CartridgeController : MonoBehaviour {


	public Transform parentBarrel;
	public Transform standing;
	public Transform laying;
	public Transform discharged;
	
	Rigidbody2D rb;
	Rigidbody2D pbrb;
	
	public bool clickedMe = false;
	public int targetChamber = -1;

	public int shellState = (int)cartridgeStates.full;

	// 0 1
	public enum cartridgeStates { discharged, full };


	private AudioSource audio;
	public AudioClip pickSound;
	public AudioClip dropSound;


	void Awake () {
	
		audio = this.GetComponent<AudioSource>();
		audio.clip = dropSound;
	
		GameObject parentBarrelGO = GameObject.Find("barrel");
		parentBarrel = parentBarrelGO.transform;
	
		this.rb = this.GetComponent<Rigidbody2D>();
		this.pbrb = parentBarrel.GetComponent<Rigidbody2D>();
	
		laying.renderer.enabled = false;
	
	}
	
	
	// Update is called once per frame
	void Update () {

	
		// the click and drag move for a shell in the world
		if ( Input.GetMouseButton(0) && clickedMe ){
			
			// move this catridge to the mouse position
			Vector3 p = Camera.main.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y, -0.1f) );
			p.z = -0.1f;
			transform.position = p;
	
			// get the hovered barrel chamber
			int collidee = parentBarrel.GetComponent<BarrelController>().chamberHover();
			targetChamber = collidee;
			
			if ( targetChamber != -1 && parentBarrel.GetComponent<BarrelController>().barrels[targetChamber-1] != (int)BarrelController.barrelStates.full ){
				
				// draw the hovered chamber in green
				Transform b = parentBarrel.GetComponent<BarrelController>().bullets[targetChamber-1];
				b.renderer.enabled = true;
				b.GetComponent<SpriteRenderer>().color = new Color(0f, .63f, .63f, 0.65f);
				
			}
			
		}


		// lock the barrel at origin
		if ( parentBarrel.transform.position != Vector3.zero ) {
			 parentBarrel.transform.position = Vector3.zero;
		}
		
		
		
		// draw the cartridge states		
		if ( shellState == (int)cartridgeStates.discharged ) {
			
			discharged.renderer.enabled = true;
			standing.renderer.enabled = false;
			laying.renderer.enabled = false;
			
		} else if ( shellState == (int)cartridgeStates.full ) { 
			
			discharged.renderer.enabled = false;
			standing.renderer.enabled = true;
			laying.renderer.enabled = false;
			
		}
		
		
		/*
			debug states for bullets

			if ( Input.GetMouseButton(1) ){
				shellState = (int)cartridgeStates.discharged;
			} else {	
				shellState = (int)cartridgeStates.full;
			}
		*/
	
	}
	
	
	void OnMouseDown(){
		
		clickedMe = true;
		rb.Sleep();
		
		audio.clip = pickSound;
		audio.Play();
		
		//Debug.Log("shell added");
		
	}
	
	
	void OnMouseUp(){
		


		// if a bullet was dragged AND there is a legit targetChamber
		if ( clickedMe && targetChamber != -1 &&  ( parentBarrel.GetComponent<BarrelController>().barrels[targetChamber-1] != (int)BarrelController.barrelStates.full  ||   parentBarrel.GetComponent<BarrelController>().barrels[targetChamber-1] != (int)BarrelController.barrelStates.discharged )  ){
			
			rb.WakeUp();
			DestroyImmediate(this.gameObject);
			parentBarrel.GetComponent<BarrelController>().shellCount--;
			
			
			if ( shellState == (int) cartridgeStates.discharged ) {
			
				parentBarrel.GetComponent<BarrelController>().barrels[targetChamber-1] = (int)BarrelController.barrelStates.discharged;
				
			} else if ( shellState == (int) cartridgeStates.full ) {
			
				parentBarrel.GetComponent<BarrelController>().barrels[targetChamber-1] = (int)BarrelController.barrelStates.full;
								
			}

			
			
		} else {
		
			rb.WakeUp();
			
		}
		
		clickedMe = false;

	}
	
	
	public void spin(){
		rb.AddTorque(-1900);
		rb.AddForce( new Vector3(1400f, 500.0f, 0.0f) );
	}

	
	public void randomizeShell(){
		shellState = Random.Range((int)0,(int)2);
	}
	
    void OnCollisionEnter2D (Collision2D other) {
		
		Debug.Log("collision");
		
		audio.clip = dropSound;
		audio.Play();
		
    }
	
	
	
}
