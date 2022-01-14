using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody2D _rigidBody2D;
    private BoxCollider2D _boxCollider;

    private string _lastKeyPressed;
    private int _idleCount;
    private int _idleLimit = 6666;
    private int _attackTic = 0;

    private GameObject _lastEnemyContact;

    [SerializeField] bool _idleTic;
    [SerializeField] bool _isRunning;
    [SerializeField] bool _isJumping;
    [SerializeField] bool _isAttacking;
    [SerializeField] bool _isHit;
    [SerializeField] int _attackTime;
    [SerializeField] float _knockBack;
    [SerializeField] float _knockTime;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpHeight = 5f;
    [SerializeField] private LayerMask _floorLayerMask;

    [SerializeField] int _playerHealth = 10;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        //Check if fell off map, reset scene
        if (gameObject.transform.position.y < -25 || _playerHealth < 1)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
        //Update Sprite
        UpdateSprite();
        //Check if jumping
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            _isJumping = true;
        }
    }

    private void UpdateSprite()
    {
        //check if hit
        if (_isHit)
        {
            _idleCount = 0;
            _animator.SetBool("_idleTic", false);
            _animator.SetBool("_isRunning", false);
            _animator.SetBool("_isJumping", false);
            _animator.SetBool("_isAttacking", false);
            _animator.SetBool("_isHit", true);
            return;
        }
        else
        {
            _animator.SetBool("_isHit", false);
        }

        //do attack if true
        if (_isAttacking)
        {
            if (_attackTic == 0)
            {
                _attackTic++;
                _idleCount = 0;
                _animator.SetBool("_idleTic", false);
                _animator.SetBool("_isRunning", false);
                _animator.SetBool("_isJumping", false);
                _animator.SetBool("_isHit", false);
                _animator.SetBool("_isAttacking", true);
                return;
            }
            else if (_attackTic > _attackTime)
            {
                _animator.SetBool("_isAttacking", false);
                _isAttacking = false;
            }
            else
            {
                _attackTic++;
            }
        }

        //get direction
        if (Input.GetKey(KeyCode.A))
        {
            _lastKeyPressed = "a";
        }
        else if (Input.GetKey(KeyCode.D))
        {
            _lastKeyPressed = "d";
        }
        //Update left/right direction first
        if (_lastKeyPressed == "a")
        {
            if (gameObject.transform.rotation.y != 180)
            {
                gameObject.transform.rotation = Quaternion.Euler(0, -180, 0);
            }
        }
        else if (_lastKeyPressed == "d")
        {
            if (gameObject.transform.rotation.y != 0)
            {
                gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }

        //if idleCount reaches idleLimit play idle anim
        if (_idleCount > _idleLimit)
        {
            _idleCount = 0;
            _animator.SetBool("_idleTic", true);
        }
        else
        {
            _animator.SetBool("_idleTic", false);
            //Check if on floor or in air
            if (IsGrounded())
            {
                //If on floor check for movement or idle
                _animator.SetBool("_isJumping", false);
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                {
                    //Player is moving
                    _animator.SetBool("_isRunning", true);
                    _idleCount = 0;
                }
                else
                {
                    //else player is idle
                    _animator.SetBool("_isRunning", false);
                    _idleCount++;
                }
            }
            else
            {
                //else player is in air
                _animator.SetBool("_isJumping", true);
                _animator.SetBool("_isRunning", false);
            }
        }
    }

    private bool IsGrounded()
    {
        //add extra height under sprite
        float extraHeight = 1f;
        //Use raycast to check if player is in contact with 'floor' layer
        RaycastHit2D raycastHit = Physics2D.BoxCast(
            _boxCollider.bounds.center, _boxCollider.bounds.size, 0f, Vector2.down, extraHeight, _floorLayerMask);

        return raycastHit.collider != null;
    }

    private void FixedUpdate()
    {
        if (_isHit)
        {
            GetHit();
        }
        else
        {
            Move();
            Jump();
        }
    }

    private void GetHit()
    {
        if (_lastEnemyContact != null)
        {
            if (gameObject.transform.position.x < _lastEnemyContact.transform.position.x)
            {
                _rigidBody2D.velocity = new Vector2(-_knockBack, _rigidBody2D.velocity.y);
            }
            else
            {
                _rigidBody2D.velocity = new Vector2(+_knockBack, _rigidBody2D.velocity.y);
            }
            StartCoroutine(KnockCo());
        }
        else
        {
            StartCoroutine(KnockCo());
        }
    }
    private IEnumerator KnockCo()
    {
        if (_rigidBody2D != null)
        {
            yield return new WaitForSeconds(_knockTime);
            _isHit = false;
        }
    }

    private void Move()
    {
        _rigidBody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (Input.GetKey(KeyCode.A))
        {
            _rigidBody2D.velocity = new Vector2(-_moveSpeed, _rigidBody2D.velocity.y);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            _rigidBody2D.velocity = new Vector2(+_moveSpeed, _rigidBody2D.velocity.y);
        }
        else
        {
            // No keys pressed
            _rigidBody2D.velocity = new Vector2(0, _rigidBody2D.velocity.y);
            _rigidBody2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        }
    }

    private void Jump()
    {
        if (_isJumping)
        {
            //Move sprite
            _rigidBody2D.velocity = Vector2.up * _jumpHeight;
            _isJumping = false;
        }
    }

    public void QueueAttacking()
    {
        _attackTic = 0;
        _isAttacking = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Debug.Log("Player collision with enemy.");
            _isHit = true;
            _playerHealth -= 1;
            _lastEnemyContact = collision.gameObject;
        }
    }

}
