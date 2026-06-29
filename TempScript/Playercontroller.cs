///--------------------------------------------///
///-----MADE WITH: UNODE VISUAL SCRIPTING-----///
///------------------------------------------///
#pragma warning disable
using UnityEngine;
using System.Collections.Generic;
using PurrNet;

[RequireComponent(typeof(CharacterController))]
public class Playercontroller : NetworkBehaviour {	
	private float x;
	private float z;
	[SerializeField]
	public float speed;
	public CharacterController Character;
	public Camera main_Camera;
	public float jumpforce;
	public Vector3 velocity;
	
	protected override void OnSpawned() {
		if(!(base.isOwner)) {
			Character.enabled = false;
			main_Camera.enabled = false;
			return;
		}
	}
	
	private void Update() {
		HandleMove();
		HandleRotation();
	}
	
	public void HandleMove() {
		bool isGrounded = Character;
		if((isGrounded && Input.GetKeyDown(KeyCode.Space))) {
			velocity = new Vector3(x, (velocity.y * jumpforce), z);
		}
	}
	
	public void HandleRotation() {
	}
}

