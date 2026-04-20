using System;
using System.Collections.Generic;
using IdleRestaurant.Localization;
using IdleRestaurant.Meta;
using UnityEngine;

namespace IdleRestaurant.Adventure
{
    public sealed class AdventureRuntime : MonoBehaviour
    {
        [Serializable]
        private sealed class WaveDefinition
        {
            public string Label = "Wave";
            public int EnemyCount = 3;
            public float EnemyHealth = 24f;
            public float EnemyDamage = 5f;
            public float EnemyMoveSpeed = 2.6f;
            public float EnemyAttackRange = 1.35f;
            public float EnemyAttackInterval = 1.05f;
            public bool IsBoss;
            public Color Tint = new Color(0.72f, 0.28f, 0.28f, 1f);
            public Vector3 Scale = new Vector3(0.8f, 1.2f, 0.8f);
        }

        [Header("Scene Flow")]
        [SerializeField] private string restaurantSceneName = GameSceneCatalog.RestaurantMain;
        [SerializeField] private Vector3 playerStartPosition = new Vector3(0f, 0.9f, -2.25f);
        [SerializeField, Min(0.2f)] private float interWaveDelay = 1.35f;
        [SerializeField, Min(1f)] private float spawnRadius = 5.5f;

        [Header("Player")]
        [SerializeField, Min(1f)] private float playerHealth = 120f;
        [SerializeField, Min(0.1f)] private float playerMoveSpeed = 3.9f;
        [SerializeField, Min(0.1f)] private float playerDamage = 10f;
        [SerializeField, Min(0.5f)] private float playerAttackRange = 1.55f;
        [SerializeField, Min(0.2f)] private float playerAttackInterval = 0.8f;
        [SerializeField] private Color playerTint = new Color(0.28f, 0.76f, 0.92f, 1f);

        [Header("Skill")]
        [SerializeField, Min(1f)] private float skillCooldown = 8f;
        [SerializeField, Min(1f)] private float skillDamage = 18f;

        [Header("Waves")]
        [SerializeField] private List<WaveDefinition> waves = new List<WaveDefinition>();

        private readonly List<AdventureUnit> aliveEnemies = new List<AdventureUnit>();
        private AdventureUnit player;
        private bool runFinished;
        private float nextWaveAt = -1f;
        private float skillReadyAt;
        private int currentWaveIndex = -1;
        private int clearedWaveCount;
        private string statusKey = "adv.status.prepare";
        private object[] statusArgs = Array.Empty<object>();
        private AdventureRewardResult lastRewardResult;

        public bool HasFinishedRun => runFinished;

        public bool IsVictory => lastRewardResult.Victory;

        public AdventureRewardResult LastRewardResult => lastRewardResult;

        public int CurrentWaveNumber => Mathf.Clamp(currentWaveIndex + 1, 1, Mathf.Max(1, waves.Count));

        public int TotalWaveCount => Mathf.Max(1, waves.Count);

        public int AliveEnemyCount => CountAliveEnemies();

        public float PlayerCurrentHealth => player != null ? player.CurrentHealth : 0f;

        public float PlayerMaxHealth => player != null ? player.MaxHealth : playerHealth;

        public float PlayerDamage => player != null ? player.AttackDamage : playerDamage;

        public float PlayerHealthNormalized => player != null ? player.HealthNormalized : 0f;

        public string PlayerDisplayName => player != null && !string.IsNullOrWhiteSpace(player.DisplayName)
            ? player.DisplayName
            : "Hero";

        public bool CanUseSkill => !runFinished && player != null && player.IsAlive && AliveEnemyCount > 0 && SkillCooldownRemaining <= 0f;

        public float SkillCooldownRemaining => Mathf.Max(0f, skillReadyAt - Time.time);

        public string StatusLabel => LocalizationService.Format(statusKey, statusArgs);

        private void Awake()
        {
            EnsureDefaultWaves();
        }

        private void Start()
        {
            StartRun();
        }

        private void Update()
        {
            if (runFinished)
            {
                return;
            }

            if (player == null || !player.IsAlive)
            {
                FinishRun(false);
                return;
            }

            if (nextWaveAt > 0f && Time.time >= nextWaveAt)
            {
                nextWaveAt = -1f;
                SpawnNextWave();
            }

            TickCombat();

            if (AliveEnemyCount > 0)
            {
                return;
            }

            if (currentWaveIndex >= 0)
            {
                clearedWaveCount = Mathf.Max(clearedWaveCount, currentWaveIndex + 1);
            }

            if (currentWaveIndex >= waves.Count - 1)
            {
                FinishRun(true);
                return;
            }

            if (nextWaveAt > 0f)
            {
                return;
            }

            SetStatus("adv.status.next_wave");
            nextWaveAt = Time.time + interWaveDelay;
        }

        public bool TryUseSkill()
        {
            if (!CanUseSkill)
            {
                return false;
            }

            bool hitAny = false;
            for (int index = aliveEnemies.Count - 1; index >= 0; index--)
            {
                AdventureUnit enemy = aliveEnemies[index];
                if (enemy == null || !enemy.IsAlive)
                {
                    aliveEnemies.RemoveAt(index);
                    continue;
                }

                enemy.ReceiveDamage(skillDamage);
                hitAny = true;
            }

            if (!hitAny)
            {
                return false;
            }

            skillReadyAt = Time.time + skillCooldown;
            SetStatus("adv.status.skill_burst");
            return true;
        }

        public void ReturnToRestaurant()
        {
            if (!SceneTransitionService.TryLoadScene(restaurantSceneName))
            {
                Debug.LogWarning(LocalizationService.Get("adv.warning.restaurant_scene_missing"), this);
            }
        }

        public void RestartRun()
        {
            if (!SceneTransitionService.TryReloadActiveScene())
            {
                Debug.LogWarning("Adventure scene reload failed.", this);
            }
        }

        private void StartRun()
        {
            CleanupRuntimeUnits();
            EnsureDefaultWaves();
            CreatePlayer();
            runFinished = false;
            nextWaveAt = Time.time + 0.25f;
            skillReadyAt = Time.time;
            currentWaveIndex = -1;
            clearedWaveCount = 0;
            SetStatus("adv.status.wave_number", 1);
            lastRewardResult = default;
        }

        private void TickCombat()
        {
            AdventureUnit target = GetNearestEnemy();
            if (player != null)
            {
                player.ManualTick(target, Time.deltaTime);
            }

            for (int index = aliveEnemies.Count - 1; index >= 0; index--)
            {
                AdventureUnit enemy = aliveEnemies[index];
                if (enemy == null)
                {
                    aliveEnemies.RemoveAt(index);
                    continue;
                }

                if (!enemy.IsAlive)
                {
                    aliveEnemies.RemoveAt(index);
                    Destroy(enemy.gameObject);
                    continue;
                }

                enemy.ManualTick(player, Time.deltaTime);
            }
        }

        private void CreatePlayer()
        {
            GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObject.name = "AdventureHero";
            playerObject.transform.SetParent(transform, false);
            playerObject.transform.position = playerStartPosition;
            playerObject.transform.localScale = new Vector3(0.9f, 1.3f, 0.9f);

            player = playerObject.AddComponent<AdventureUnit>();
            player.Configure(
                "Hero",
                true,
                playerHealth,
                playerMoveSpeed,
                playerDamage,
                playerAttackRange,
                playerAttackInterval,
                playerTint);
        }

        private void SpawnNextWave()
        {
            currentWaveIndex++;
            if (currentWaveIndex < 0 || currentWaveIndex >= waves.Count)
            {
                FinishRun(true);
                return;
            }

            WaveDefinition wave = waves[currentWaveIndex];
            if (wave.IsBoss)
            {
                SetStatus("adv.status.boss_wave");
            }
            else
            {
                SetStatus("adv.status.wave_number", currentWaveIndex + 1);
            }

            for (int index = 0; index < wave.EnemyCount; index++)
            {
                SpawnEnemy(wave, index);
            }
        }

        private void SpawnEnemy(WaveDefinition wave, int enemyIndex)
        {
            GameObject enemyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyObject.name = wave.IsBoss ? "BossEnemy" : "Enemy";
            enemyObject.transform.SetParent(transform, false);
            enemyObject.transform.position = GetSpawnPosition(enemyIndex, wave.EnemyCount);
            enemyObject.transform.localScale = wave.Scale;

            AdventureUnit enemy = enemyObject.AddComponent<AdventureUnit>();
            enemy.Configure(
                wave.IsBoss ? "Boss" : wave.Label,
                false,
                wave.EnemyHealth,
                wave.EnemyMoveSpeed,
                wave.EnemyDamage,
                wave.EnemyAttackRange,
                wave.EnemyAttackInterval,
                wave.Tint);
            enemy.Died += HandleEnemyDied;
            aliveEnemies.Add(enemy);
        }

        private void HandleEnemyDied(AdventureUnit deadEnemy)
        {
            if (deadEnemy == null)
            {
                return;
            }

            aliveEnemies.Remove(deadEnemy);
            Destroy(deadEnemy.gameObject);
        }

        private AdventureUnit GetNearestEnemy()
        {
            AdventureUnit nearest = null;
            float bestDistance = float.MaxValue;
            Vector3 origin = player != null ? player.transform.position : Vector3.zero;

            for (int index = aliveEnemies.Count - 1; index >= 0; index--)
            {
                AdventureUnit enemy = aliveEnemies[index];
                if (enemy == null)
                {
                    aliveEnemies.RemoveAt(index);
                    continue;
                }

                if (!enemy.IsAlive)
                {
                    continue;
                }

                float distance = Vector3.SqrMagnitude(enemy.transform.position - origin);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        private Vector3 GetSpawnPosition(int enemyIndex, int totalEnemies)
        {
            float step = 360f / Mathf.Max(1, totalEnemies);
            float angle = (enemyIndex * step + currentWaveIndex * 23f) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * spawnRadius;
            float z = Mathf.Sin(angle) * spawnRadius;
            return new Vector3(x, 0.9f, z);
        }

        private void FinishRun(bool victory)
        {
            if (runFinished)
            {
                return;
            }

            runFinished = true;
            SetStatus(victory ? "adv.status.run_complete" : "adv.status.run_failed");
            AdventureRewardResult result = BuildRewardResult(victory);
            lastRewardResult = result;
            MetaProgressService.AddAdventureRewards(result);
        }

        private AdventureRewardResult BuildRewardResult(bool victory)
        {
            int wavesCleared = Mathf.Clamp(clearedWaveCount, 0, waves.Count);
            int seeds = Mathf.Max(0, wavesCleared);
            int pendingCoins = wavesCleared * 18;
            int rareResources = 0;
            bool bossDefeated = victory && waves.Count > 0 && waves[waves.Count - 1].IsBoss;

            if (victory)
            {
                rareResources = bossDefeated ? 2 : 1;
                seeds += 2;
                pendingCoins += 48;
            }
            else if (wavesCleared >= 2)
            {
                seeds += 1;
            }

            return new AdventureRewardResult(
                victory,
                bossDefeated,
                wavesCleared,
                rareResources,
                seeds,
                pendingCoins);
        }

        private int CountAliveEnemies()
        {
            int count = 0;
            for (int index = aliveEnemies.Count - 1; index >= 0; index--)
            {
                AdventureUnit enemy = aliveEnemies[index];
                if (enemy == null)
                {
                    aliveEnemies.RemoveAt(index);
                    continue;
                }

                if (enemy.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        private void CleanupRuntimeUnits()
        {
            for (int index = aliveEnemies.Count - 1; index >= 0; index--)
            {
                AdventureUnit enemy = aliveEnemies[index];
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            aliveEnemies.Clear();

            if (player != null)
            {
                Destroy(player.gameObject);
                player = null;
            }
        }

        private void EnsureDefaultWaves()
        {
            if (waves.Count > 0)
            {
                return;
            }

            waves.Add(new WaveDefinition
            {
                Label = LocalizationService.Format("adv.status.wave_number", 1),
                EnemyCount = 3,
                EnemyHealth = 24f,
                EnemyDamage = 5f,
                EnemyMoveSpeed = 2.5f,
                EnemyAttackRange = 1.35f,
                EnemyAttackInterval = 1.05f,
                Tint = new Color(0.78f, 0.43f, 0.32f, 1f),
                Scale = new Vector3(0.82f, 1.18f, 0.82f)
            });

            waves.Add(new WaveDefinition
            {
                Label = LocalizationService.Format("adv.status.wave_number", 2),
                EnemyCount = 4,
                EnemyHealth = 28f,
                EnemyDamage = 5.1f,
                EnemyMoveSpeed = 2.75f,
                EnemyAttackRange = 1.35f,
                EnemyAttackInterval = 1f,
                Tint = new Color(0.71f, 0.35f, 0.4f, 1f),
                Scale = new Vector3(0.84f, 1.2f, 0.84f)
            });

            waves.Add(new WaveDefinition
            {
                Label = LocalizationService.Format("adv.status.wave_number", 3),
                EnemyCount = 5,
                EnemyHealth = 30f,
                EnemyDamage = 5.7f,
                EnemyMoveSpeed = 2.9f,
                EnemyAttackRange = 1.4f,
                EnemyAttackInterval = 0.95f,
                Tint = new Color(0.64f, 0.31f, 0.51f, 1f),
                Scale = new Vector3(0.86f, 1.24f, 0.86f)
            });

            waves.Add(new WaveDefinition
            {
                Label = LocalizationService.Get("adv.status.boss_wave"),
                EnemyCount = 1,
                EnemyHealth = 78f,
                EnemyDamage = 8.8f,
                EnemyMoveSpeed = 2.5f,
                EnemyAttackRange = 1.65f,
                EnemyAttackInterval = 0.95f,
                IsBoss = true,
                Tint = new Color(0.45f, 0.24f, 0.8f, 1f),
                Scale = new Vector3(1.25f, 1.8f, 1.25f)
            });
        }

        private void SetStatus(string key, params object[] args)
        {
            statusKey = string.IsNullOrWhiteSpace(key) ? "adv.status.prepare" : key;
            statusArgs = args ?? Array.Empty<object>();
        }
    }
}
