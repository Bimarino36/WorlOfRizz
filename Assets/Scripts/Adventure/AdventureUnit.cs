using System;
using UnityEngine;

namespace IdleRestaurant.Adventure
{
    public sealed class AdventureUnit : MonoBehaviour
    {
        [SerializeField] private string displayName = "Unit";
        [SerializeField] private bool playerControlled;
        [SerializeField, Min(1f)] private float maxHealth = 20f;
        [SerializeField, Min(0.1f)] private float moveSpeed = 3f;
        [SerializeField, Min(0.1f)] private float attackDamage = 5f;
        [SerializeField, Min(0.5f)] private float attackRange = 1.4f;
        [SerializeField, Min(0.2f)] private float attackInterval = 1f;
        [SerializeField] private Renderer[] renderers;

        private float currentHealth;
        private float attackCooldown;

        public event Action<AdventureUnit> Died;

        public string DisplayName => displayName;

        public bool IsPlayerControlled => playerControlled;

        public bool IsAlive => currentHealth > 0f;

        public float CurrentHealth => currentHealth;

        public float MaxHealth => maxHealth;

        public float AttackDamage => attackDamage;

        public float HealthNormalized => maxHealth > 0f ? currentHealth / maxHealth : 0f;

        public void Configure(
            string unitName,
            bool isPlayer,
            float healthValue,
            float speedValue,
            float damageValue,
            float rangeValue,
            float intervalValue,
            Color tint)
        {
            displayName = string.IsNullOrWhiteSpace(unitName) ? "Unit" : unitName;
            playerControlled = isPlayer;
            maxHealth = Mathf.Max(1f, healthValue);
            moveSpeed = Mathf.Max(0.1f, speedValue);
            attackDamage = Mathf.Max(0.1f, damageValue);
            attackRange = Mathf.Max(0.5f, rangeValue);
            attackInterval = Mathf.Max(0.2f, intervalValue);
            currentHealth = maxHealth;
            attackCooldown = 0f;
            CacheRenderers();
            ApplyTint(tint);
        }

        public void ManualTick(AdventureUnit target, float deltaTime)
        {
            if (!IsAlive)
            {
                return;
            }

            attackCooldown = Mathf.Max(0f, attackCooldown - deltaTime);
            if (target == null || !target.IsAlive)
            {
                return;
            }

            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = target.transform.position;
            currentPosition.y = 0f;
            targetPosition.y = 0f;

            Vector3 delta = targetPosition - currentPosition;
            float distance = delta.magnitude;
            if (distance > attackRange)
            {
                Vector3 step = delta.normalized * moveSpeed * deltaTime;
                if (step.sqrMagnitude > delta.sqrMagnitude)
                {
                    step = delta;
                }

                transform.position += new Vector3(step.x, 0f, step.z);
            }
            else if (attackCooldown <= 0f)
            {
                target.ReceiveDamage(attackDamage);
                attackCooldown = attackInterval;
            }

            if (delta.sqrMagnitude > 0.001f)
            {
                Vector3 lookDirection = new Vector3(delta.x, 0f, delta.z);
                transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }

        public bool ReceiveDamage(float damage)
        {
            if (!IsAlive)
            {
                return false;
            }

            currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, damage));
            if (currentHealth > 0f)
            {
                return true;
            }

            currentHealth = 0f;
            Died?.Invoke(this);
            return true;
        }

        private void Awake()
        {
            CacheRenderers();
            if (currentHealth <= 0f)
            {
                currentHealth = maxHealth;
            }
        }

        private void CacheRenderers()
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(true);
            }
        }

        private void ApplyTint(Color tint)
        {
            if (renderers == null)
            {
                return;
            }

            for (int index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] == null)
                {
                    continue;
                }

                renderers[index].material.color = tint;
            }
        }
    }
}
