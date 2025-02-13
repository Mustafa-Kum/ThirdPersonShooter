using System;
using System.Collections.Generic;
using EnemyLogic;
using Manager;
using MissionLogic;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LevelGeneration
{
    public class LevelGenerator : MonoBehaviour
    {
        public static LevelGenerator instance;
        
        [SerializeField] private NavMeshSurface _navMeshSurface;
        [SerializeField] private List<Transform> _levelParts;
        [SerializeField] private Transform _lastLevelPart;
        [SerializeField] private SnapPoint _nextSnapPoint;
        [SerializeField] private float _generationCooldown;

        private List<Transform> _currentLevelParts;
        private List<Transform> _generatedLevelParts = new List<Transform>();
        private List<Enemy> _enemiesList;
        private SnapPoint _defaultSnapPoint;
        private float _cooldownTimer;
        private bool _generationOver = true;

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            EventManager.GameEvents.LevelGeneration += LevelGenerationStart;
        }
        
        private void OnDisable()
        {
            EventManager.GameEvents.LevelGeneration -= LevelGenerationStart;
        }

        private void Update()
        {
            if (_generationOver)
                return;
            
            _cooldownTimer -= Time.deltaTime;

            if (_cooldownTimer < 0)
            {
                if (_currentLevelParts.Count > 0)
                {
                    _cooldownTimer = _generationCooldown;
                    GenerateNextLevelPart();
                }
                else if (_generationOver == false)
                {
                    FinishGeneration();
                }
            }
        }
        
        public Enemy GetRandomEnemy()
        {
            int randomIndex = Random.Range(0, _enemiesList.Count);

            return _enemiesList[randomIndex];
        }
        
        public List<Enemy> GetEnemiesList()
        {
            return _enemiesList;
        }

        [ContextMenu("Create Next Level Part")]
        private void GenerateNextLevelPart()
        {
            Transform newPart = null;

            if (_generationOver)
                newPart = Instantiate(_lastLevelPart);
            else
                newPart = Instantiate(ChooseRandomPart());
                
            _generatedLevelParts.Add(newPart);
            
            LevelPart levelPartScript = newPart.GetComponent<LevelPart>();
            
            levelPartScript.SnapAndAlignPartTo(_nextSnapPoint);

            if (levelPartScript.IntersectionDetected())
            {
                InitializeGeneration();
                return;
            }
            
            _nextSnapPoint = levelPartScript.GetExitPoint();
            _enemiesList.AddRange(levelPartScript.MyEnemies());
         }
        
        private void FinishGeneration()
        {
            _generationOver = true;
            
            GenerateNextLevelPart();
            
            _navMeshSurface.BuildNavMesh();
            
            foreach (Enemy enemy in _enemiesList)
            {
                enemy.transform.parent = null;
                enemy.gameObject.SetActive(true);
            }
            
            EventManager.GameEvents.MissionStart?.Invoke();
        }

        [ContextMenu("Restart Generation")]
        private void InitializeGeneration()
        {
            _nextSnapPoint = _defaultSnapPoint;
            _generationOver = false;
            _currentLevelParts = new List<Transform>(_levelParts);

            DestroyWrongLevelPartsAndEnemies();
        }

        private void DestroyWrongLevelPartsAndEnemies()
        {
            foreach (Enemy enemy in _enemiesList)
            {
                Destroy(enemy.gameObject);
            }
            
            foreach (Transform levelParts in _generatedLevelParts)
            {
                Destroy(levelParts.gameObject);
            }

            _generatedLevelParts = new List<Transform>();
            _enemiesList = new List<Enemy>();
        }
        
        private void LevelGenerationStart()
        {
            _enemiesList = new List<Enemy>(); 
            _defaultSnapPoint = _nextSnapPoint;
            InitializeGeneration();
        }

        private Transform ChooseRandomPart()
        {
            int randomIndex = Random.Range(0, _currentLevelParts.Count);

            Transform choosenPart = _currentLevelParts[randomIndex];

            _currentLevelParts.RemoveAt(randomIndex);

            return choosenPart;
        }
    }
}