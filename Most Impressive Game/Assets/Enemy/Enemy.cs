using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health;
    private Animator _enemyAnimator;
    private Rigidbody2D _enemyRigidBody2D;
    private BoxCollider2D _boxCollider;

    [SerializeField] bool _wasHit;
    [SerializeField] float _knockBack;
    [SerializeField] float _knockTime;
    [SerializeField] private GameObject _enemyParticlePrefab;
    [SerializeField] private GameObject _player;
    [SerializeField] private LayerMask _floorLayerMask;

    private void Start()
    {
        _enemyAnimator = gameObject.GetComponent<Animator>();
        _enemyRigidBody2D = gameObject.GetComponent<Rigidbody2D>();
        _boxCollider = gameObject.GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (_wasHit)
        {
            if (gameObject.transform.position.x < _player.transform.position.x)
            {
                _enemyRigidBody2D.velocity = new Vector2(-_knockBack, _enemyRigidBody2D.velocity.y);
            }
            else
            {
                _enemyRigidBody2D.velocity = new Vector2(+_knockBack, _enemyRigidBody2D.velocity.y);
            }
            StartCoroutine(KnockCo(_enemyRigidBody2D));
        }
        else if (IsGrounded())
        {
            //stay still update to follow player
            _enemyRigidBody2D.velocity = new Vector2(0, 0);
        }
        
        if (health <= 0 || gameObject.transform.position.y < -20)
        {
            Instantiate(_enemyParticlePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
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

    public void TakeDamage(int damage)
    {
        health -= damage;
        _wasHit = true;
        _enemyAnimator.SetBool("_wasHit", true);
        Debug.Log("Enemy dmg taken");
    }

    private IEnumerator KnockCo(Rigidbody2D enemy)
    {
        if (enemy != null)
        {
            yield return new WaitForSeconds(_knockTime);
            _wasHit = false;
            _enemyAnimator.SetBool("_wasHit", false);
        }
    }
}
