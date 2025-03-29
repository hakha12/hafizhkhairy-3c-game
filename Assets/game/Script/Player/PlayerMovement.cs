using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
	const float PLAYER_CLIMBING_FOV = 70;
	const float PLAYER_STANDING_FOV = 40;
	[SerializeField] private InputManager _input;
	[SerializeField] private CameraManager _camera;
	[SerializeField] private PlayerAudioManager _playerAudio;
	[SerializeField] private Transform _groundDetector;
	[SerializeField] private LayerMask _groundLayer;
	[SerializeField] private Transform _resetCheckpointPos;
	[SerializeField] private float _detectorRadius;
	[SerializeField] private float _walkSpeed;
	[SerializeField] private float _jumpForce;
	[SerializeField] private float _rotationSmoothTime = 0.1f;
	[SerializeField] private float _sprintSpeed;
	[SerializeField] private float _walkSprintTransition;
	[SerializeField] private Vector3 _upperStepOffset;
	[SerializeField] private float _stepCheckerDistance;
	[SerializeField] private float _stepForce;
	[SerializeField] private Transform _climbDetector;
	[SerializeField] private float _climbCheckDistance;
	[SerializeField] private LayerMask _climbableLayer;
	[SerializeField] private Vector3 _climbOffset;
	[SerializeField] private float _climbSpeed;
	[SerializeField] private float _crouchSpeed;
	[SerializeField] private float _glideSpeed;
	[SerializeField] private float _airDrag;
	[SerializeField] private Vector3 _glideRotationSpeed;
	[SerializeField] private float _minGlideRotationX;
	[SerializeField] private float _maxGlideRotationX;
	[SerializeField] private float _resetComboInterval;
	[SerializeField] private Transform _hitDetector;
	[SerializeField] private float _hitDetectorRadius;
	[SerializeField] private LayerMask _hitLayer;

	[SerializeField] private Transform _cameraTransform;
	private PlayerStance _playerStance;
	private float _rotationSmoothVelocity;
	private float _speed;
	private bool _isGrounded;
	private bool _isPunching;
	private Vector3 rotationDegree = Vector3.zero;
	private int _punchCombo = 0;
	
	private Coroutine _resetCombo;
	private Rigidbody _rigidbody;
	private Animator _animator;
	private CapsuleCollider _collider;

	private void Start(){
		_input.OnMoveInput += Move;
		_input.OnSprintInput += Sprint;
		_input.OnJumpInput += Jump;
		_input.OnClimbInput += StartClimb;
		_input.OnCancelClimb += CancelClimb;
		_input.OnGlideInput += StartGlide;
		_input.OnCancelGlide += CancelGlide;
		_input.OnCrouchInput += Crouch;
		_input.OnPunchInput += Punch;

		_camera.OnChangePerspective += ChangePerspective;
	}

	private void Awake(){
		HideAndLockCursor();

		_rigidbody = GetComponent<Rigidbody>();
		_animator = GetComponent<Animator>();
		_collider = GetComponent<CapsuleCollider>();

		_speed = _walkSpeed;
		_playerStance = PlayerStance.Stand;
	}
	private void Update(){
		CheckIsGrounded();
		Glide();
		CheckStep();
	}
	private void OnDestroy(){
		_input.OnMoveInput -= Move;
		_input.OnSprintInput -= Sprint;
		_input.OnJumpInput -= Jump;
		_input.OnClimbInput -= StartClimb;
		_input.OnCancelClimb -= CancelClimb;
		_input.OnGlideInput -= StartGlide;
		_input.OnCancelGlide -= CancelGlide;
		_input.OnCrouchInput -= Crouch;
		_input.OnPunchInput -= Punch;

		_camera.OnChangePerspective -= ChangePerspective;
	}

	private void CheckIsGrounded(){
		_isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
		_animator.SetBool("IsGrounded", _isGrounded);

		if (_isGrounded){
			CancelGlide();
		}
	}

	public void ResetPositionToCheckpoint(){
		if (_resetCheckpointPos != null)
        {
			transform.position = _resetCheckpointPos.position;
			transform.rotation = _resetCheckpointPos.rotation;
        }
	}
	private void Move(Vector2 axisDirection){
		Vector3 movementDirection = Vector3.zero;
		bool isPlayerStanding = _playerStance == PlayerStance.Stand;
		bool isPlayerClimbing = _playerStance == PlayerStance.Climb;
		bool isPlayerCrouching = _playerStance == PlayerStance.Crouch;
		bool isPlayerGliding = _playerStance == PlayerStance.Glide;

		if ((isPlayerStanding || isPlayerCrouching) && !_isPunching){
			switch (_camera.cameraState){
				case CameraState.FirstPerson:
					transform.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);

					Vector3 verticalDirection = axisDirection.y * transform.forward;
					Vector3 horizontalDirection = axisDirection.x * transform.right;

					movementDirection = verticalDirection + horizontalDirection;

					break;
				case CameraState.ThirdPerson:
					if (axisDirection.magnitude < 0.1) return;

					float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
					float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
					transform.rotation =Quaternion.Euler(0f, smoothAngle, 0f);

					movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;

					break;
				default:
					break;
			}

			_rigidbody.AddForce(movementDirection * _speed * Time.deltaTime);

			Vector3 velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);

			_animator.SetFloat("Velocity", velocity.magnitude * axisDirection.magnitude);
			_animator.SetFloat("VelocityX", velocity.magnitude * axisDirection.y);
			_animator.SetFloat("VelocityZ", velocity.magnitude * axisDirection.x);

			Debug.Log(movementDirection);
		
		} else if (isPlayerClimbing){
			Vector3 horizontal = axisDirection.x * transform.right;
			Vector3 vertical = axisDirection.y * transform.up;

			Vector3 checkerLeftPosition = transform.position + (transform.up * 1) + (-transform.right * 0.75f);
			Vector3 checkerRightPosition = transform.position + (transform.up * 1) + (transform.right * 1f);
			Vector3 checkerUpPosition = transform.position + (transform.up * 2.5f);
			Vector3 checkerDownPosition = transform.position + (-transform.up * 0.25f);

			bool isAbleClimbLeft = Physics.Raycast(checkerLeftPosition, transform.forward, _climbCheckDistance, _climbableLayer);
			bool isAbleClimbRight = Physics.Raycast(checkerRightPosition, transform.forward, _climbCheckDistance, _climbableLayer);
			bool isAbleClimbUp = Physics.Raycast(checkerUpPosition, transform.forward, _climbCheckDistance, _climbableLayer);
			bool isAbleClimbDown = Physics.Raycast(checkerDownPosition, transform.forward, _climbCheckDistance, _climbableLayer);

			if ((isAbleClimbLeft && (axisDirection.x < 0)) || (isAbleClimbRight && (axisDirection.x > 0))){
				horizontal = axisDirection.x * transform.right;
        	}
 
			if ((isAbleClimbUp && (axisDirection.y > 0)) || (isAbleClimbDown && (axisDirection.y < 0))){
				vertical = axisDirection.y * transform.up;
        	}

			movementDirection = horizontal + vertical;

			_rigidbody.AddForce(movementDirection * _climbSpeed * Time.deltaTime);

			Vector3 velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, 0);

			_animator.SetFloat("ClimbVelocityX", velocity.magnitude * axisDirection.x);
			_animator.SetFloat("ClimbVelocityY", velocity.magnitude * axisDirection.y);
		} else if (isPlayerGliding){
			rotationDegree.x += _glideRotationSpeed.x * axisDirection.y * Time.deltaTime;
			rotationDegree.y += _glideRotationSpeed.y * axisDirection.x * Time.deltaTime;
			rotationDegree.z += _glideRotationSpeed.z * axisDirection.x * Time.deltaTime;
			
			rotationDegree.x = Mathf.Clamp(rotationDegree.x, _minGlideRotationX, _maxGlideRotationX);

			transform.rotation = Quaternion.Euler(rotationDegree);
		}
		
		
	}
	private void Sprint(bool isSprint){
		if (isSprint){
			if (_speed < _sprintSpeed){
				_speed = _speed + _walkSprintTransition * Time.deltaTime;
			}
		} else {
			if (_speed > _walkSpeed){
				_speed = _speed - _walkSprintTransition * Time.deltaTime;
			}
		}
	}
	private void Jump(){
		if (!_isGrounded || _isPunching) return;

		Vector3 jumpDirection = Vector3.up;

		_rigidbody.AddForce(jumpDirection * _jumpForce);
		_animator.SetTrigger("Jump");

		// Log the OG code
		Debug.Log("Jump power: " + (jumpDirection.y * _jumpForce * Time.deltaTime));
	}

	private void Glide(){
		if (_playerStance == PlayerStance.Glide){
			Vector3 playerRotation = transform.rotation.eulerAngles;
			float lift = playerRotation.x;
			Vector3 upForce = transform.up * (lift + _airDrag);
			Vector3 forwardForce = transform.forward * _glideSpeed;
			Vector3 totalForce = upForce + forwardForce;

			_rigidbody.AddForce(totalForce * Time.deltaTime);
		}
	}

	private void CheckStep(){
		bool isHitLowerStep = Physics.Raycast(_groundDetector.position, transform.forward, _stepCheckerDistance);
		bool isHitUpperStep = Physics.Raycast(_groundDetector.position + _upperStepOffset, transform.forward, _stepCheckerDistance);

		if (isHitLowerStep && !isHitUpperStep){
			_rigidbody.AddForce(0, _stepForce, 0);
		}
	}

	private void StartClimb(){
		bool isInfrontofClimbingWall = Physics.Raycast(_climbDetector.position, transform.forward, out RaycastHit hit, _climbCheckDistance, _climbableLayer);
		bool isNotClimbing = _playerStance != PlayerStance.Climb;

		if (isInfrontofClimbingWall && _isGrounded && isNotClimbing){
			_camera.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
			_camera.SetTPSFieldOfView(PLAYER_CLIMBING_FOV);

			Vector3 climbablePoint = hit.collider.bounds.ClosestPoint(transform.position);
			Vector3 direction = (climbablePoint - transform.position).normalized;
			direction.y = 0;
			transform.rotation = Quaternion.LookRotation(direction);

			Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);

			transform.position = hit.point - offset;
			_playerStance = PlayerStance.Climb;
			_rigidbody.useGravity = false;
			_animator.SetBool("IsClimbing", true);
			_collider.center = Vector3.up * 1.3f;
		}
	}

	private void CancelClimb(){
		if (_playerStance == PlayerStance.Climb){
			_playerStance = PlayerStance.Stand;

			_camera.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
			_camera.SetTPSFieldOfView(PLAYER_STANDING_FOV);

			_rigidbody.useGravity = true;
			_animator.SetBool("IsClimbing", false);
			_collider.center = Vector3.up * 0.9f;
			transform.position -= transform.forward * 1f;
		}
	}
	private void StartGlide(){
		if (_playerStance != PlayerStance.Glide && !_isGrounded){
			_camera.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
			_playerStance = PlayerStance.Glide;
			rotationDegree = transform.rotation.eulerAngles;
			_animator.SetBool("IsGliding", true);
			_playerAudio.PlayGlideSFX();
		}
	}

	private void CancelGlide(){
		if (_playerStance == PlayerStance.Glide){
			_camera.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
			_playerStance = PlayerStance.Stand;
			_animator.SetBool("IsGliding", false);
			_playerAudio.StopGlideSFX();
		}
	}

	private void Crouch(){
		Vector3 checkerUpPosition = transform.position + (transform.up * 1.4f);
		bool isCantStand = Physics.Raycast(checkerUpPosition, transform.up, 0.25f, _groundLayer);

		if (_playerStance == PlayerStance.Stand){
			_playerStance = PlayerStance.Crouch;
			_animator.SetBool("IsCrouch", true);
			_speed = _crouchSpeed;

			_collider.height = 1.3f;
			_collider.center = Vector3.up * 0.66f;			
		} else if (_playerStance == PlayerStance.Crouch && !isCantStand){
			_playerStance = PlayerStance.Stand;
			_animator.SetBool("IsCrouch", false);
			_speed = _walkSpeed;

			_collider.height = 1.8f;
			_collider.center = Vector3.up * 0.9f;	
		}

		Debug.Log("Collider height = " + _collider.height);
	}

	private void Punch(){
		if (_isPunching || _playerStance != PlayerStance.Stand || !_isGrounded) return;

		_isPunching = true;

		if (_punchCombo < 3){
			_punchCombo++;
		} else {
			_punchCombo = 1;
		}

		Debug.Log("Punch Combo = " + _punchCombo);

		_animator.SetInteger("Combo", _punchCombo);
		_animator.SetTrigger("Punch");
	}

	private void EndPunch(){
		_isPunching = false;

		if (_resetCombo != null){
			StopCoroutine(_resetCombo);
		}

		_resetCombo = StartCoroutine(ResetCombo());
	}

	private void Hit(){
		Collider[] hitObject = Physics.OverlapSphere(_hitDetector.position, _hitDetectorRadius, _hitLayer);

		for (int i = 0; i < hitObject.Length; i++){
			if (hitObject[i].gameObject == null) continue;

			Destroy(hitObject[i].gameObject);
		}
	}

	private IEnumerator ResetCombo(){
		yield return new WaitForSeconds(_resetComboInterval);

		_punchCombo = 0;
	}
	private void HideAndLockCursor(){
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void ChangePerspective(){
		_animator.SetTrigger("ChangePerspective");
	}
}
