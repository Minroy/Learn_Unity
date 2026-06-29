///--------------------------------------------///
///-----MADE WITH: UNODE VISUAL SCRIPTING-----///
///------------------------------------------///
#pragma warning disable
using PurrNet;
using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : NetworkBehaviour, ITick {	
	public bool CanMove;
	public bool isInside;
	[Header("Movement Settings")]
	[SerializeField]
	[Range(0F, 300F)]
	private float moveSpeed = 5F;
	[SerializeField]
	[Range(0F, 300F)]
	private float sprintSpeed = 8F;
	[SerializeField]
	private float gravity = -9.81F;
	[Header("Look Settings")]
	[SerializeField]
	[Range(0F, 20F)]
	private float lookSensitivity = 0.1F;
	[SerializeField]
	[Range(0F, 100F)]
	private float maxLookAngle = 80F;
	[Header("References")]
	[SerializeField]
	private Camera playerCamera;
	[SerializeField]
	private CharacterController characterController;
	private Vector3 velocity;
	private float verticalRotation;
	private bool isPaused;
	
	private void Awake() {
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 27, "exit");
		isPaused = false;
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 24, "exit");
		CanMove = true;
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 25, "exit");
		isInside = false;
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 26, true);
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 25, true);
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 24, true);
	}
	
	private void OnEnable() {
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 30, "exit");
		characterController = this.GetComponent<CharacterController>();
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 29, true);
	}
	
	protected override void OnSpawned() {
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 47, "exit");
		this.enabled = this.isOwner;
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 34, "exit");
		if(!(this.isOwner)) {
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 36, "onTrue");
			UnityEngine.Object.Destroy(playerCamera.gameObject);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 40, "exit");
			return;
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 40, true);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 36, "exit");
			if((playerCamera == null)) {
				MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 42, "onTrue");
				this.enabled = false;
				MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 45, "exit");
				return;
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 45, true);
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 42, true);
			} else {
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 42, false);
			}
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 36, true);
		} else {
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 36, "exit");
			if((playerCamera == null)) {
				MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 42, "onTrue");
				this.enabled = false;
				MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 45, "exit");
				return;
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 45, true);
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 42, true);
			} else {
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 42, false);
			}
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 36, false);
		}
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 34, true);
	}
	
	protected override void OnDespawned() {
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 53, "exit");
		if(!(this.isOwner)) {
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 50, "onTrue");
			return;
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 50, true);
		} else {
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 50, false);
		}
	}
	
	private void Update() {
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 61, "exit");
		if(Input.GetKeyDown(KeyCode.Escape)) {
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 54, "onTrue");
			TogglePause();
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 56, true);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 54, "exit");
			if(CanMove) {
				MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 57, "onTrue");
				HandleMovement();
				MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 59, "exit");
				HandleRotation();
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 60, true);
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 59, true);
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 57, true);
			} else {
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 57, false);
			}
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 54, true);
		} else {
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 54, "exit");
			if(CanMove) {
				MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 57, "onTrue");
				HandleMovement();
				MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 59, "exit");
				HandleRotation();
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 60, true);
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 59, true);
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 57, true);
			} else {
				MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 57, false);
			}
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 54, false);
		}
	}
	
	private void ToggleMouse() {
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 68, "exit");
		if(CanMove) {
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 62, "onTrue");
			Cursor.lockState = CursorLockMode.Locked;
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 64, "exit");
			Cursor.visible = false;
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 65, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 64, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 62, true);
		} else {
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 62, "onFalse");
			Cursor.lockState = CursorLockMode.None;
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 66, "exit");
			Cursor.visible = true;
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 67, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 66, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 62, false);
		}
	}
	
	public void TogglePause() {
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 76, "exit");
		isPaused = !(isPaused);
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 70, "exit");
		CanMove = !(isPaused);
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 72, "exit");
		Debug.Log((isPaused ? "Player paused" : "Player resumed"));
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 74, "exit");
		ToggleMouse();
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 75, true);
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 74, true);
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 72, true);
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 70, true);
	}
	
	private void HandleMovement() {
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 117, "exit");
		var isGrounded = characterController.isGrounded;
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 77, "exit");
		if((isGrounded && (velocity.y < 0))) {
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 80, "onTrue");
			velocity.y = -2F;
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 87, true);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 80, "exit");
			var horizontal = Input.GetAxisRaw("Horizontal");
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 88, "exit");
			var vertical = Input.GetAxisRaw("Vertical");
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 90, "exit");
			var moveDirection = ((this.transform.right * horizontal) + (this.transform.forward * vertical));
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 92, "exit");
			moveDirection = Vector3.ClampMagnitude(moveDirection, 1F);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 101, "exit");
			var isRunning = Input.GetKey(KeyCode.LeftShift);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 102, "exit");
			var currentSpeed = (isRunning ? sprintSpeed : moveSpeed);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 104, "exit");
			characterController.Move(((currentSpeed * Time.deltaTime) * moveDirection));
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 109, "exit");
			velocity.y += (gravity * Time.deltaTime);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 113, "exit");
			characterController.Move((velocity * Time.deltaTime));
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 116, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 113, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 109, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 104, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 102, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 101, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 92, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 90, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 88, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 80, true);
		} else {
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 80, "exit");
			var horizontal = Input.GetAxisRaw("Horizontal");
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 88, "exit");
			var vertical = Input.GetAxisRaw("Vertical");
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 90, "exit");
			var moveDirection = ((this.transform.right * horizontal) + (this.transform.forward * vertical));
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 92, "exit");
			moveDirection = Vector3.ClampMagnitude(moveDirection, 1F);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 101, "exit");
			var isRunning = Input.GetKey(KeyCode.LeftShift);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 102, "exit");
			var currentSpeed = (isRunning ? sprintSpeed : moveSpeed);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 104, "exit");
			characterController.Move(((currentSpeed * Time.deltaTime) * moveDirection));
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 109, "exit");
			velocity.y += (gravity * Time.deltaTime);
			MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 113, "exit");
			characterController.Move((velocity * Time.deltaTime));
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 116, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 113, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 109, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 104, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 102, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 101, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 92, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 90, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 88, true);
			MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 80, false);
		}
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 77, true);
	}
	
	private void HandleRotation() {
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 136, "exit");
		var mouseX = (Input.GetAxis("Mouse X") * lookSensitivity);
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 118, "exit");
		var mouseY = (Input.GetAxis("Mouse Y") * lookSensitivity);
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 121, "exit");
		verticalRotation -= mouseY;
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 124, "exit");
		verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 127, "exit");
		playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0F, 0F);
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 132, "exit");
		this.transform.Rotate((Vector3.up * mouseX));
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 135, true);
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 132, true);
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 127, true);
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 124, true);
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 121, true);
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 118, true);
	}
	
	public void OnTick(float delta) {
		MaxyGames.UNode.GraphDebug.Flow(this, -1622702134, 164, "exit");
		Debug.Log("777");
		MaxyGames.UNode.GraphDebug.FlowNode(this, -1622702134, 168, true);
	}
}

