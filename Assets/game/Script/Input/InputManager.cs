using System;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour {
	public Action<Vector2> OnMoveInput;
	public Action<bool> OnSprintInput;
	public Action OnJumpInput;
	public Action OnClimbInput;
	public Action onCancelClimb;
	private void Update(){
        CheckMovementInput();
		CheckSprintInput();
		CheckJumpInput();
		CheckCrouchInput();
		CheckChangePOVInput();
		CheckClimbInput(); 
		CheckGlideInput(); 
		CheckCancelInput(); 
		CheckPunchInput(); 
		CheckMainMenuInput(); 
    }
	private void CheckMovementInput(){
		float verticalAxis = Input.GetAxis("Vertical");
		float horizontalAxis = Input.GetAxis("Horizontal");

		Vector2 inputAxis = new Vector2(horizontalAxis, verticalAxis);

		Debug.Log("Vertical Axis 	: " + verticalAxis);
		Debug.Log("Horizontal Axis 	: " + horizontalAxis);

		if (OnMoveInput == null) return;

		OnMoveInput(inputAxis);
		
	}

	private void CheckSprintInput(){
		bool isSprintInputPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

		if (isSprintInputPressed){
			if (OnSprintInput == null) return;

			OnSprintInput(true);
			Debug.Log("Sprinting!");
			
		} else {
			if (OnSprintInput == null) return;

			OnSprintInput(false);
			Debug.Log("Not sprinting!");
	
		}
	}

	private void CheckJumpInput(){
		bool isJumpInputPressed = Input.GetKeyDown(KeyCode.Space);

		if (isJumpInputPressed){
			if (OnJumpInput == null) return;

			OnJumpInput();
			Debug.Log("Jump!!");
			
		}
	}

	private void CheckCrouchInput(){
		bool isCrouchInputPressed = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);

		if (isCrouchInputPressed){
			Debug.Log("Crouching");
		}
	}

	private void CheckChangePOVInput(){
		bool isPOVInputPressed = Input.GetKeyDown(KeyCode.Q);

		if (isPOVInputPressed){
			Debug.Log("POV Changed");
		}
	}

	private void CheckClimbInput(){
		bool isClimbInputPressed = Input.GetKeyDown(KeyCode.E);

		if (isClimbInputPressed){
			OnClimbInput();

			Debug.Log("Climbing");
		}
	}

	private void CheckGlideInput(){
		bool isGlideInputPressed = Input.GetKeyDown(KeyCode.G);

		if (isGlideInputPressed){
			Debug.Log("Gliding");
		}
	}

	private void CheckCancelInput(){
		bool isCancelInputPressed = Input.GetKeyDown(KeyCode.C);

		if (isCancelInputPressed){
			if (onCancelClimb == null) return;

			onCancelClimb();
			Debug.Log("Cancel jumping and climbing");
		}
	}

	private void CheckPunchInput(){
		bool isPunchInputPressed = Input.GetKeyDown(KeyCode.Mouse0);

		if (isPunchInputPressed){
			Debug.Log("I hit you know scream");
		}
	}

	private void CheckMainMenuInput(){
		bool isMainMenuInputPressed = Input.GetKeyDown(KeyCode.Escape);

		if (isMainMenuInputPressed){
			Debug.Log("Go back to main menu");
		}
	}

}
