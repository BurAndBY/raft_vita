using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PersonController : MonoBehaviour
{
	[Header("Movement settings")]
	public float normalSpeed = 3f;

	public float sprintSpeed = 6f;

	public float jumpSpeed = 8f;

	public float gravity = 20f;

	[Space(10f)]
	public Transform waterChecker;

	public float footstepSoundInterval = 1f;

	[Header("Flags")]
	[Space(10f)]
	public bool inWater;

	public bool moving;

	public bool sprinting;

	private CharacterController controller;

	private Vector3 moveDirection = Vector3.zero;

	private float yVelocity;

	private float footStepTimer;

	private Vector3 velocity;

	private void Awake()
	{
		controller = GetComponent<CharacterController>();
		Helper.SetCursorVisibleAndLockState(false, CursorLockMode.Locked);
	}

	private void Update()
	{
		moving = controller.velocity.magnitude > 0.8f;
		inWater = (double)waterChecker.position.y < -0.35;
		sprinting = Input.GetButton("Sprint");
		Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		if (direction.x > 0f && direction.z > 0f)
		{
			direction.Normalize();
		}
		moveDirection = base.transform.TransformDirection(direction);
		float num = ((!sprinting || inWater) ? normalSpeed : sprintSpeed);
		if (inWater)
		{
			num *= 0.6f;
		}
		moveDirection *= num;
		Vector3 target = moveDirection * num;
		moveDirection = Vector3.SmoothDamp(moveDirection, target, ref velocity, (!moving) ? 0.01f : 0.1f);
		moveDirection.y = yVelocity;
		controller.Move(moveDirection * Time.deltaTime);
		if (controller.isGrounded)
		{
			if (Input.GetButton("Jump"))
			{
				yVelocity = jumpSpeed;
			}
			else
			{
				yVelocity = 0f;
			}
		}
		else
		{
			yVelocity -= gravity * Time.deltaTime;
		}
		PlayerAnimator.SetAnimationMoving(moving);
		PlayerAnimator.SetAnimationWater(inWater);
		PlayFootSteps();
	}

	private void PlayFootSteps()
	{
		if (moving && !inWater)
		{
			footStepTimer += Time.deltaTime;
			float num = ((!sprinting) ? footstepSoundInterval : (footstepSoundInterval * 0.75f));
			if (footStepTimer >= num)
			{
				footStepTimer = 0f;
				PlayFootStepSound();
			}
		}
	}

	private void PlayFootStepSound()
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("FootstepWood");
	}
}
