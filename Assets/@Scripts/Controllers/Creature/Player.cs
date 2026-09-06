using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public enum PlayerState
{
    Idle,
    Walk,
    Run,
    Jump,
    Block,
}

[RequireComponent(typeof(CharacterController))]
public class Player : Creature
{
    [Header("Input")]
    [SerializeField] private InputActionAsset _inputActions;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float _walkSpeed = 3f;
    [SerializeField, Min(0f)] private float _runSpeed = 6f;
    [SerializeField, Min(0f)] private float _jumpHeight = 1.5f;
    [SerializeField] private float _gravity = -20f;
    [SerializeField, Min(0f)] private float _rotationSpeed = 720f;
    [SerializeField, Range(0f, 1f)] private float _airControl = 0.65f;
    [SerializeField] private Transform _cameraTransform;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField, Min(0f)] private float _animationFadeTime = 0.12f;
    [SerializeField, Min(0.01f)] private float _blockAnimationSpeed = 1.3f;

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    public bool IsGrounded { get; private set; }
    public bool IsBlocking => CurrentState == PlayerState.Block;

    public void TakeDamage(int damage)
    {
       Debug.Log($"Damage Á÷Àü HP : {GameManager.Instance.HP}");
        if (damage <= 0)
            return;

        GameManager game = GameManager.Instance;
        game.HP = Mathf.Max(0, game.HP - damage);
        Debug.Log($"Player Hit - Current HP : {game.HP}", this);
        EventManager.Instance.TriggerEvent(Define.EEventType.PlayerHit);
    }

    private CharacterController _controller;
    private float _verticalVelocity;
    private Vector2 _moveInput;
    private bool _sprintHeld;
    private bool _jumpPressed;
    private bool _blockHeld;

    private InputActionMap _playerActions;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _blockAction;

    // Full paths keep animation names in one place; gameplay owns the state.
    private static readonly int IdleHash = Animator.StringToHash("Base Layer.Idle");
    private static readonly int WalkHash = Animator.StringToHash("Base Layer.Walk");
    private static readonly int RunHash = Animator.StringToHash("Base Layer.Run");
    private static readonly int JumpHash = Animator.StringToHash("Base Layer.Jump");
    private static readonly int BlockHash = Animator.StringToHash("Base Layer.Block");

    #region Lifecycle
    public override void Init()
    {
        base.Init();
        _controller = GetComponent<CharacterController>();
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
        if (_animator != null)
            _animator.applyRootMotion = false;

        ResetInput();
        _verticalVelocity = 0f;
        IsGrounded = _controller.isGrounded;
        ChangeState(PlayerState.Idle, true);
    }

    private void OnEnable()
    {
        if (!InitializeInput())
        {
            enabled = false;
            return;
        }

        _moveAction.Enable();
        _jumpAction.Enable();
        _sprintAction.Enable();
        _blockAction.Enable();
        _verticalVelocity = 0f;
        ChangeState(PlayerState.Idle, true);
    }

    private void OnDisable()
    {
        _playerActions?.Disable();
        ResetInput();
        if (_animator != null)
            _animator.speed = 1f;
    }

    private void OnDestroy()
    {
        _playerActions?.Dispose();
    }

    private void OnValidate()
    {
        _gravity = -Mathf.Max(0.01f, Mathf.Abs(_gravity));
    }

    private void Update()
    {
        if (!_controller.enabled || Time.deltaTime <= 0f)
            return;

        IsGrounded = _controller.isGrounded && _verticalVelocity <= 0f;
        UpdateInput();
        UpdateState();
        UpdateMovement();
    }
    #endregion

    #region Input
    private bool InitializeInput()
    {
        if (_playerActions != null)
            return true;

        InputActionAsset source = _inputActions != null ? _inputActions : InputSystem.actions;
        InputActionMap sourceMap = source != null ? source.FindActionMap("Player") : null;
        if (sourceMap == null)
        {
            Debug.LogError("Player: Assign an Input Actions asset with a Player map.", this);
            return false;
        }

        // A private copy lets this Player disable/dispose input without affecting UI or others.
        _playerActions = sourceMap.Clone();
        _moveAction = _playerActions.FindAction("Move");
        _jumpAction = _playerActions.FindAction("Jump");
        _sprintAction = _playerActions.FindAction("Sprint");
        _blockAction = _playerActions.FindAction("Block");
        if (_moveAction == null || _jumpAction == null || _sprintAction == null || _blockAction == null)
        {
            Debug.LogError("Player: Move, Jump, Sprint and Block actions are required.", this);
            _playerActions.Dispose();
            _playerActions = null;
            return false;
        }

        return true;
    }

    private void UpdateInput()
    {
        _moveInput = Vector2.ClampMagnitude(_moveAction.ReadValue<Vector2>(), 1f);
        _sprintHeld = _sprintAction.IsPressed();
        _jumpPressed = _jumpAction.WasPressedThisFrame();
        _blockHeld = _blockAction.IsPressed();
    }

    private void ResetInput()
    {
        _moveInput = Vector2.zero;
        _sprintHeld = false;
        _jumpPressed = false;
        _blockHeld = false;
    }
    #endregion

    #region State
    private void UpdateState()
    {
        // Falling also uses Jump. Block can be used while airborne, but jumping again is not allowed.
        if (!IsGrounded)
        {
            ChangeState(_blockHeld ? PlayerState.Block : PlayerState.Jump);
            return;
        }

        if (CurrentState == PlayerState.Block)
        {
            if (!_blockHeld)
                ChangeState(GetMovementState());
            return;
        }

        // Block takes priority when Block and Jump arrive together on the ground.
        if (_blockHeld)
        {
            ChangeState(PlayerState.Block);
            return;
        }

        if (_jumpPressed)
        {
            _verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(_gravity) * _jumpHeight);
            IsGrounded = false;
            ChangeState(PlayerState.Jump);
            return;
        }

        ChangeState(GetMovementState());
    }

    private PlayerState GetMovementState()
    {
        if (_moveInput.sqrMagnitude < 0.0001f)
            return PlayerState.Idle;
        return _sprintHeld ? PlayerState.Run : PlayerState.Walk;
    }

    private void ChangeState(PlayerState newState, bool force = false)
    {
        if (!force && CurrentState == newState)
            return;

        CurrentState = newState;
        PlayAnimation(newState);
    }
    #endregion

    #region Movement
    private void UpdateMovement()
    {
        Vector3 direction = GetMovementDirection();
        float speed = _sprintHeld ? _runSpeed : _walkSpeed;
        if (CurrentState == PlayerState.Block)
            direction = Vector3.zero;
        else if (!IsGrounded)
            speed *= _airControl;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                _rotationSpeed * Time.deltaTime);
        }

        // A small downward velocity maintains contact on slopes and steps.
        if (IsGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;
        _verticalVelocity -= Mathf.Abs(_gravity) * Time.deltaTime;

        Vector3 velocity = direction * speed + Vector3.up * _verticalVelocity;
        CollisionFlags collisions = _controller.Move(velocity * Time.deltaTime);
        if ((collisions & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
            _verticalVelocity = 0f;

        IsGrounded = (collisions & CollisionFlags.Below) != 0 && _verticalVelocity <= 0f;
        if (IsGrounded && CurrentState == PlayerState.Jump)
            ChangeState(GetMovementState());
        else if (!IsGrounded && CurrentState != PlayerState.Jump && CurrentState != PlayerState.Block)
            ChangeState(_blockHeld ? PlayerState.Block : PlayerState.Jump);
    }

    private Vector3 GetMovementDirection()
    {
        if (_cameraTransform == null && Camera.main != null)
            _cameraTransform = Camera.main.transform;

        Vector3 forward = _cameraTransform != null ? _cameraTransform.forward : Vector3.forward;
        forward.y = 0f;
        // Keep a usable basis even for a camera looking straight down.
        if (forward.sqrMagnitude < 0.0001f && _cameraTransform != null)
            forward = Vector3.Cross(_cameraTransform.right, Vector3.up);
        forward.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        return forward * _moveInput.y + right * _moveInput.x;
    }
    #endregion

    #region Animation
    private void PlayAnimation(PlayerState state)
    {
        if (_animator == null)
            return;

        _animator.speed = state == PlayerState.Block ? _blockAnimationSpeed : 1f;
        if (_animator.runtimeAnimatorController == null)
            return;

        int stateHash = state switch
        {
            PlayerState.Walk => WalkHash,
            PlayerState.Run => RunHash,
            PlayerState.Jump => JumpHash,
            PlayerState.Block => BlockHash,
            _ => IdleHash,
        };
        _animator.CrossFadeInFixedTime(stateHash, _animationFadeTime, 0, 0f);
    }
    #endregion
}
