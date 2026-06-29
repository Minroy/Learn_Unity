///--------------------------------------------///
///-----MADE WITH: UNODE VISUAL SCRIPTING-----///
///------------------------------------------///
#pragma warning disable
using UnityEngine;
using System.Collections.Generic;
using PurrNet;

[RequireComponent(typeof(CharacterController))]
public class Playercontroller : NetworkBehaviour {	
	[SerializeField]
	public float speed;
	public CharacterController Character;
	public Camera main_Camera;
	
	protected override void OnSpawned() {
		if(!(base.isOwner)) {
			Character.enabled = false;
		}
	}
	
	private void Update() {
		HandleMove();
	}
	
	public void HandleMove() {
		Character.Move(new Vector3(((this.speed * Input.GetAxisRaw("Horizontal")) * Time.deltaTime), 0F, ((this.speed * Input.GetAxisRaw("Horizontal")) * Time.deltaTime)));
	}
}

