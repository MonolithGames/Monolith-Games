using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class FPSCharacterController : MonoBehaviour
{
	[Header("References")]
	[RequireComponent(typeof(CharacterController))]
public sealed class FPSCharacterController : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] private float walkSpeed = 5f;
	[SerializeField] private float sprintSpeed = 8f;
	[SerializeField] private float crouchSpeed = 2.5f;
	[SerializeField] private float acceleration = 18f;
	[SerializeField] private float deceleration = 22f;
	[SerializeField, Range(0f, 1f)] private float airControl = .45f;
	[SerializeField] private float jumpHeight = 1.2f;
	[SerializeField] private float gravity = -20f;
	[SerializeField] private float slopeLimit = 45f;
	[SerializeField] private float stepOffset = .3f;
	[SerializeField] private bool requireForwardForSprint = true;

	[Header("Posture")]
	[SerializeField] private float standingHeight = 2f;
	[SerializeField] private float crouchingHeight = 1.2f;
	[SerializeField] private float crouchTransitionSpeed = 10f;
	[SerializeField] private LayerMask environmentMask = ~0;

	[Header("Stamina")]
	[SerializeField] private float maxStamina = 5f;
	[SerializeField] private float sprintDrain = 1f;
	[SerializeField] private float staminaRecovery = 1.5f;
	[SerializeField] private float sprintRecoveryDelay = .75f;

	[Header("Look and Camera")]
	[SerializeField] private Camera playerCamera;
	[SerializeField] private float mouseSensitivity = 2f;
	[SerializeField] private float minLookAngle = -85f;
	[SerializeField] private float maxLookAngle = 85f;
	[SerializeField] private float standingCameraHeight = .8f;
	[SerializeField] private float crouchingCameraHeight = .45f;
	[SerializeField] private float cameraSmooth = 12f;
	[SerializeField] private float headBobFrequency = 8f;
	[SerializeField] private float headBobAmplitude = .035f;
	[SerializeField] private float landingDip = .08f;

	[Header("Interaction")]
	[SerializeField] private KeyCode interactKey = KeyCode.E;
	[SerializeField] private float interactionDistance = 3f;
	[SerializeField] private LayerMask interactionMask = ~0;

	private CharacterController controller;
	private Vector3 planarVelocity;
	private Vector3 cameraStartPosition;
	private float verticalVelocity;
	private float stamina;
	private float recoveryTimer;
	private float pitch;
	private float bobTime;
	private float landingOffset;
	private float originalHeight;
	private bool wasGrounded;
	private bool crouching;
	private bool sprinting;

	public event Action<GameObject> Interacted;
	public event Action Jumped;
	public event Action<bool> GroundedChanged;

	public float StaminaNormalized => maxStamina <= 0f ? 0f : stamina / maxStamina;
	public Vector3 Velocity => planarVelocity + Vector3.up * verticalVelocity;
	public bool IsGrounded => controller != null && controller.isGrounded;
	public bool IsCrouching => crouching;
	public bool IsSprinting => sprinting;
	public bool IsMoving => planarVelocity.sqrMagnitude > .01f;

	private void Awake()
	{
		controller = GetComponent<CharacterController>();
		originalHeight = controller.height;
		standingHeight = originalHeight;
		controller.slopeLimit = slopeLimit;
		controller.stepOffset = stepOffset;
		stamina = maxStamina;
		if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
		if (playerCamera != null)
		{
			cameraStartPosition = playerCamera.transform.localPosition;
			cameraStartPosition.y = standingCameraHeight;
		}
		SetCursorLocked(true);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape)) SetCursorLocked(false);
		if (Cursor.lockState == CursorLockMode.Locked) Look();
		UpdatePosture();
		Move();
		UpdateStamina();
		UpdateCameraEffects();
		if (Input.GetKeyDown(interactKey)) Interact();
		if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked) SetCursorLocked(true);
	}

	private void Look()
	{
		transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);
		pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * mouseSensitivity, minLookAngle, maxLookAngle);
		if (playerCamera != null) playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
	}

	private void Move()
	{
		bool grounded = controller.isGrounded;
		if (grounded != wasGrounded) GroundedChanged?.Invoke(grounded);
		if (grounded && !wasGrounded) landingOffset = landingDip;
		wasGrounded = grounded;
		if (grounded && verticalVelocity < 0f) verticalVelocity = -2f;

		Vector2 input = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
		Vector3 direction = (transform.right * input.x + transform.forward * input.y).normalized;
		sprinting = CanSprint(input);
		float speed = crouching ? crouchSpeed : sprinting ? sprintSpeed : walkSpeed;
		Vector3 target = direction * speed;
		float rate = (target.sqrMagnitude < .01f ? deceleration : acceleration) * (grounded ? 1f : airControl);
		planarVelocity = Vector3.MoveTowards(planarVelocity, target, rate * Time.deltaTime);
		if (grounded && Input.GetButtonDown("Jump") && !crouching)
		{
			verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
			Jumped?.Invoke();
		}
		verticalVelocity += gravity * Time.deltaTime;
		controller.Move((planarVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
	}

	private bool CanSprint(Vector2 input)
	{
		return Input.GetKey(KeyCode.LeftShift) && (!requireForwardForSprint || input.y > .1f) &&
			input.sqrMagnitude > .01f && !crouching && stamina > 0f;
	}

	private void UpdatePosture()
	{
		crouching = Input.GetKey(KeyCode.LeftControl) || !CanStand();
		float target = crouching ? crouchingHeight : standingHeight;
		controller.height = Mathf.MoveTowards(controller.height, target, crouchTransitionSpeed * Time.deltaTime);
		controller.center = Vector3.up * controller.height * .5f;
	}

	private bool CanStand()
	{
		float radius = controller.radius * .95f;
		Vector3 bottom = transform.position + Vector3.up * radius;
		Vector3 top = transform.position + Vector3.up * (standingHeight - radius);
		return !Physics.CheckCapsule(bottom, top, radius, environmentMask, QueryTriggerInteraction.Ignore);
	}

	private void UpdateStamina()
	{
		if (sprinting)
		{
			stamina = Mathf.Max(0f, stamina - sprintDrain * Time.deltaTime);
			recoveryTimer = sprintRecoveryDelay;
		}
		else if (recoveryTimer > 0f) recoveryTimer -= Time.deltaTime;
		else stamina = Mathf.Min(maxStamina, stamina + staminaRecovery * Time.deltaTime);
	}

	private void UpdateCameraEffects()
	{
		if (playerCamera == null) return;
		Vector3 target = cameraStartPosition;
		target.y = crouching ? crouchingCameraHeight : standingCameraHeight;
		if (controller.isGrounded && planarVelocity.magnitude > .1f)
		{
			bobTime += Time.deltaTime * headBobFrequency * (sprinting ? 1.35f : 1f);
			target.x += Mathf.Cos(bobTime * .5f) * headBobAmplitude;
			target.y += Mathf.Abs(Mathf.Sin(bobTime)) * headBobAmplitude;
		}
		landingOffset = Mathf.MoveTowards(landingOffset, 0f, Time.deltaTime * .35f);
		target.y -= landingOffset;
		playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, target, cameraSmooth * Time.deltaTime);
	}

	private void Interact()
	{
		if (playerCamera == null) return;
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, interactionDistance, interactionMask, QueryTriggerInteraction.Ignore))
		{
			IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
			if (interactable != null) interactable.Interact(gameObject);
			Interacted?.Invoke(hit.collider.gameObject);
		}
	}

	private void SetCursorLocked(bool locked)
	{
		Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = !locked;
	}

	private void OnValidate()
	{
		walkSpeed = Mathf.Max(0f, walkSpeed);
		sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
		crouchSpeed = Mathf.Clamp(crouchSpeed, 0f, walkSpeed);
		standingHeight = Mathf.Max(.5f, standingHeight);
		crouchingHeight = Mathf.Clamp(crouchingHeight, .5f, standingHeight);
		maxStamina = Mathf.Max(.1f, maxStamina);
		gravity = Mathf.Min(-.01f, gravity);
	}
}

public interface IInteractable
{
	void Interact(GameObject interactor);
}
	{
		[SerializeField] private float walkSpeed = 5f;
		[SerializeField] private float sprintSpeed = 8f;
		[SerializeField] private float crouchSpeed = 2.5f;
		[SerializeField] private float acceleration = 18f;
		[SerializeField] private float deceleration = 22f;
		[SerializeField] private float airControl = 0.45f;
		[SerializeField] private float jumpHeight = 1.2f;
		[SerializeField] private float gravity = -20f;
		[SerializeField] private float standingHeight = 2f;
		[SerializeField] private float crouchingHeight = 1.2f;
		[SerializeField] private float crouchTransitionSpeed = 10f;
		[SerializeField] private float slopeLimit = 45f;
		[SerializeField] private float stepOffset = 0.3f;
		[SerializeField] private float maxStamina = 5f;
		[SerializeField] private float sprintDrain = 1f;
		[SerializeField] private float staminaRecovery = 1.5f;
		[SerializeField] private float sprintRecoveryDelay = 0.75f;

		private CharacterController controller;
		private Vector3 planarVelocity;
		private float verticalVelocity;
		private float stamina;
		private float recoveryTimer;
		private bool crouching;
		private bool sprinting;

		public float StaminaNormalized => maxStamina <= 0f ? 0f : stamina / maxStamina;
		public bool IsCrouching => crouching;
		public bool IsSprinting => sprinting;
		public bool IsMoving => planarVelocity.sqrMagnitude > 0.01f;

		private void Awake()
		{
			controller = GetComponent<CharacterController>();
			standingHeight = controller.height;
			controller.slopeLimit = slopeLimit;
			controller.stepOffset = stepOffset;
			stamina = maxStamina;
		}

		private void Update()
		{
			UpdatePosture();
			UpdatePlanarMovement();
			UpdateVerticalMovement();
			UpdateStamina();
		}

		 [RequireComponent(typeof(CharacterController))]
		public sealed class FPSCharacterController : MonoBehaviour
			float horizontal = Input.GetAxisRaw("Horizontal");
			[SerializeField] float walkSpeed = 5f;
			[SerializeField] float sprintSpeed = 8f;
			[SerializeField] float crouchSpeed = 2.5f;
			[SerializeField] float acceleration = 18f;
			[SerializeField] float airControl = .45f;
			[SerializeField] float jumpHeight = 1.2f;
			[SerializeField] float gravity = -20f;
			[SerializeField] float standingHeight = 2f;
			[SerializeField] float crouchingHeight = 1.2f;
			[SerializeField] float crouchTransitionSpeed = 10f;
			[SerializeField] float maxStamina = 5f;
			[SerializeField] float sprintDrain = 1f;
			[SerializeField] float staminaRecovery = 1.5f;
			[SerializeField] float sprintRecoveryDelay = .75f;
			[SerializeField] float mouseSensitivity = 2f;
			[SerializeField] float minLookAngle = -85f;
			[SerializeField] float maxLookAngle = 85f;
			[SerializeField] Camera playerCamera;
			[SerializeField] float interactionDistance = 3f;
			[SerializeField] LayerMask interactionMask = ~0;
			[SerializeField] float standingCameraHeight = .8f;
			[SerializeField] float crouchingCameraHeight = .45f;
			[SerializeField] float cameraSmooth = 12f;

			CharacterController controller;
			Vector3 velocity;
			float verticalVelocity;
			float stamina;
			float recoveryTimer;
			float pitch;
			float cameraBaseHeight;
			bool crouching;
			bool sprinting;
			public event Action<GameObject> Interacted;
			public float StaminaNormalized => maxStamina <= 0 ? 0 : stamina / maxStamina;
			public bool IsCrouching => crouching;
			public bool IsSprinting => sprinting;
			public bool IsGrounded => controller != null && controller.isGrounded;
			public bool IsMoving => velocity.sqrMagnitude > .01f;
			public Vector3 Velocity => velocity + Vector3.up * verticalVelocity;

			void Awake()
			{
				controller = GetComponent<CharacterController>();
				standingHeight = controller.height;
				stamina = maxStamina;
				if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
				if (playerCamera != null) cameraBaseHeight = playerCamera.transform.localPosition.y;
			}

			void Update()
			{
				Look();
				UpdatePosture();
				Move();
				UpdateStamina();
				if (Input.GetKeyDown(KeyCode.E)) Interact();
				if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
			}

			void Look()
			{
				transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);
				pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * mouseSensitivity, minLookAngle, maxLookAngle);
				if (playerCamera != null) playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
			}

			void Move()
			{
				if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -2;
				Vector2 input = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1);
				Vector3 direction = (transform.right * input.x + transform.forward * input.y).normalized;
				sprinting = Input.GetKey(KeyCode.LeftShift) && input.y > .1f && !crouching && stamina > 0;
				float speed = crouching ? crouchSpeed : sprinting ? sprintSpeed : walkSpeed;
				Vector3 target = direction * speed;
				velocity = Vector3.MoveTowards(velocity, target, acceleration * (controller.isGrounded ? 1 : airControl) * Time.deltaTime);
				if (controller.isGrounded && Input.GetButtonDown("Jump") && !crouching) verticalVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
				verticalVelocity += gravity * Time.deltaTime;
				controller.Move((velocity + Vector3.up * verticalVelocity) * Time.deltaTime);
			}

			void UpdatePosture()
			{
				crouching = Input.GetKey(KeyCode.LeftControl) || !CanStand();
				float height = Mathf.MoveTowards(controller.height, crouching ? crouchingHeight : standingHeight, crouchTransitionSpeed * Time.deltaTime);
				controller.height = height;
				controller.center = Vector3.up * height * .5f;
				if (playerCamera != null)
				{
					Vector3 p = playerCamera.transform.localPosition;
					p.y = Mathf.MoveTowards(p.y, crouching ? crouchingCameraHeight : cameraBaseHeight, cameraSmooth * Time.deltaTime);
					playerCamera.transform.localPosition = p;
				}
			}

			bool CanStand()
			{
				float radius = controller.radius * .95f;
				return !Physics.CheckCapsule(transform.position + Vector3.up * radius, transform.position + Vector3.up * (standingHeight - radius), radius, ~0, QueryTriggerInteraction.Ignore);
			}

			void UpdateStamina()
			{
				if (sprinting && IsMoving) { stamina = Mathf.Max(0, stamina - sprintDrain * Time.deltaTime); recoveryTimer = sprintRecoveryDelay; }
				else if (recoveryTimer > 0) recoveryTimer -= Time.deltaTime;
				else stamina = Mathf.Min(maxStamina, stamina + staminaRecovery * Time.deltaTime);
			}

			void Interact()
			{
				if (playerCamera == null) return;
				if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, interactionDistance, interactionMask, QueryTriggerInteraction.Ignore))
				{
					IInteractable target = hit.collider.GetComponentInParent<IInteractable>();
					if (target != null) target.Interact(gameObject);
					Interacted?.Invoke(hit.collider.gameObject);
				}
			}

			void OnValidate()
			{
				walkSpeed = Mathf.Max(0, walkSpeed); sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed); crouchSpeed = Mathf.Clamp(crouchSpeed, 0, walkSpeed);
				standingHeight = Mathf.Max(.5f, standingHeight); crouchingHeight = Mathf.Clamp(crouchingHeight, .5f, standingHeight);
				maxStamina = Mathf.Max(.01f, maxStamina); slopeLimit = 45f;
			}
	[SerializeField] private float maxLookAngle = 85f;
	[SerializeField] private float slopeLimit = 45f;
	[SerializeField] private float stepOffset = 0.3f;

	[Header("Stamina")]
	[SerializeField] private float maxStamina = 5f;
	[SerializeField] private float sprintDrain = 1f;
	[SerializeField] private float staminaRecovery = 1.5f;
	[SerializeField] private float sprintRecoveryDelay = 0.75f;

	[Header("Camera Effects")]
	[SerializeField] private float standingCameraHeight = 0.8f;
	[SerializeField] private float crouchingCameraHeight = 0.45f;
	[SerializeField] private float cameraPositionSpeed = 12f;
	[SerializeField] private float headBobFrequency = 8f;
	[SerializeField] private float headBobAmplitude = 0.035f;
	[SerializeField] private float sprintBobMultiplier = 1.35f;
	[SerializeField] private float landingDip = 0.08f;

	[Header("Interaction")]
	[SerializeField] private float interactionDistance = 3f;
	[SerializeField] private LayerMask interactionMask = ~0;

	private Vector3 planarVelocity;
	private Vector3 cameraStartPosition;
	private float stamina;
	private float recoveryTimer;
	private float bobTime;
	private float landingOffset;
	private bool wasGrounded;

	private CharacterController controller;
	private float verticalVelocity;
	private float cameraPitch;
	private bool isCrouching;

	private void Awake()
	{
		controller = GetComponent<CharacterController>();

		if (playerCamera == null)
			playerCamera = GetComponentInChildren<Camera>();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		stamina = maxStamina;
		standingHeight = controller.height;
		controller.slopeLimit = slopeLimit;
		controller.stepOffset = stepOffset;
		if (playerCamera != null)
		{
			cameraStartPosition = playerCamera.transform.localPosition;
			cameraStartPosition.y = standingCameraHeight;
		}
	}

	private void Update()
	{
		Look();
		Move();
		UpdateCrouch();
		UpdateCameraEffects();
		UpdateStamina();
		HandleInteraction();

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	private void Look()
	{
		float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

		transform.Rotate(Vector3.up * mouseX);
		cameraPitch = Mathf.Clamp(cameraPitch - mouseY, minLookAngle, maxLookAngle);

		if (playerCamera != null)
			playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
	}

	private void Move()
	{
		bool grounded = controller.isGrounded;
		if (grounded && !wasGrounded)
			landingOffset = landingDip;
		wasGrounded = grounded;
		if (grounded && verticalVelocity < 0f)
			verticalVelocity = -2f;

		float horizontal = Input.GetAxisRaw("Horizontal");
		float vertical = Input.GetAxisRaw("Vertical");
		Vector3 direction = (transform.right * horizontal + transform.forward * vertical).normalized;

		bool sprinting = CanSprint(horizontal, vertical);
		float speed = isCrouching ? crouchSpeed : sprinting ? sprintSpeed : walkSpeed;
		Vector3 targetVelocity = direction * speed;
		float rate = grounded ? acceleration : acceleration * airControl;
		if (targetVelocity.sqrMagnitude < 0.01f)
			rate = deceleration;
		planarVelocity = Vector3.MoveTowards(planarVelocity, targetVelocity, rate * Time.deltaTime);
		controller.Move(planarVelocity * Time.deltaTime);

		if (grounded && Input.GetButtonDown("Jump") && !isCrouching)
			verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

		verticalVelocity += gravity * Time.deltaTime;
		controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
	}

	private bool CanSprint(float horizontal, float vertical)
	{
		bool requested = Input.GetKey(KeyCode.LeftShift) && vertical > 0.1f;
		return requested && !isCrouching && stamina > 0f;
	}

	private void UpdateStamina()
	{
		bool moving = planarVelocity.sqrMagnitude > 0.2f;
		bool sprinting = Input.GetKey(KeyCode.LeftShift) && moving && !isCrouching && stamina > 0f;
		if (sprinting)
		{
			stamina = Mathf.Max(0f, stamina - sprintDrain * Time.deltaTime);
			recoveryTimer = sprintRecoveryDelay;
		}
		else if (recoveryTimer > 0f)
			recoveryTimer -= Time.deltaTime;
		else
			stamina = Mathf.Min(maxStamina, stamina + staminaRecovery * Time.deltaTime);
	}

	private void UpdateCrouch()
	{
		bool wantsToCrouch = Input.GetKey(KeyCode.LeftControl);
		if (!wantsToCrouch && !CanStand())
			wantsToCrouch = true;
		isCrouching = wantsToCrouch;
		float targetHeight = isCrouching ? crouchingHeight : standingHeight;
		controller.height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
		controller.center = Vector3.up * (controller.height * 0.5f);
	}

	private bool CanStand()
	{
		float radius = controller.radius * 0.95f;
		Vector3 bottom = transform.position + Vector3.up * radius;
		Vector3 top = transform.position + Vector3.up * (standingHeight - radius);
		return !Physics.CheckCapsule(bottom, top, radius, ~0, QueryTriggerInteraction.Ignore);
	}

	private void UpdateCameraEffects()
	{
		if (playerCamera == null)
			return;

		float targetY = isCrouching ? crouchingCameraHeight : standingCameraHeight;
		Vector3 target = cameraStartPosition;
		target.y = targetY;
		bool moving = controller.isGrounded && planarVelocity.magnitude > 0.1f;
		if (moving)
		{
			float bobSpeed = headBobFrequency * (Input.GetKey(KeyCode.LeftShift) ? sprintBobMultiplier : 1f);
			bobTime += Time.deltaTime * bobSpeed;
			target.x += Mathf.Cos(bobTime * 0.5f) * headBobAmplitude;
			target.y += Mathf.Abs(Mathf.Sin(bobTime)) * headBobAmplitude;
		}
		landingOffset = Mathf.MoveTowards(landingOffset, 0f, Time.deltaTime * 0.35f);
		target.y -= landingOffset;
		playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, target, cameraPositionSpeed * Time.deltaTime);
	}

	private void HandleInteraction()
	{
		if (!Input.GetKeyDown(KeyCode.E) || playerCamera == null)
			return;
		Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
		if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionMask, QueryTriggerInteraction.Ignore))
		{
			IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
			if (interactable != null)
				interactable.Interact(gameObject);
		}
	}

	public float StaminaNormalized => maxStamina <= 0f ? 0f : stamina / maxStamina;

	public bool IsCrouching => isCrouching;

	public bool IsMoving => planarVelocity.sqrMagnitude > 0.01f;

	private void OnValidate()
	{
		walkSpeed = Mathf.Max(0f, walkSpeed);
		sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
		crouchSpeed = Mathf.Clamp(crouchSpeed, 0f, walkSpeed);
		crouchingHeight = Mathf.Clamp(crouchingHeight, 0.5f, standingHeight);
		maxStamina = Mathf.Max(0.1f, maxStamina);
	}
}

public interface IInteractable
{
	void Interact(GameObject interactor);
}

