using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Player _player;
    private float _attackTimer;
    [SerializeField] private float attackTimerLimit;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private Transform attackPos;
    [SerializeField] private float attackRange;
    [SerializeField] private int damage;


    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void Update()
    {
        if (_attackTimer <= 0)
        {
            //Ready to attack
            if (Input.GetMouseButtonDown(0))
            {
                //tell player to attack
                _player.QueueAttacking();
                //Check enemies hit
                Collider2D[] enemiesHit = Physics2D.OverlapBoxAll(attackPos.position, attackPos.transform.localScale, 0f, enemyMask);
                for (int i = 0; i < enemiesHit.Length; i++)
                {
                    enemiesHit[i].GetComponent<Enemy>().TakeDamage(damage);
                }
                _attackTimer = attackTimerLimit;
            }
        }
        else
        {
            _attackTimer -= Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPos.position, attackPos.transform.localScale);
    }
}
