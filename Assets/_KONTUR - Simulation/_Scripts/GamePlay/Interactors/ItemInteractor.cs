using UnityEngine;
using KofeyekToolkit.LifeCycle;
using _KONTUR___Simulation._Scripts.GamePlay.Progression;
using _KONTUR___Simulation._Scripts.GamePlay.Player;
using System.Collections.Generic;

namespace _KONTUR___Simulation._Scripts.GamePlay.Interactors
{
    public class ItemInteractor : InteractorBase
    {
        [Header("Progression")]
        [SerializeField] private string _progressionKey = "items_collected";
        [SerializeField] private int _progressionValue = 1;

        [Header("Audio")]
        [SerializeField] private AudioClip _infectionSound;
        [Range(0f, 1f)] [SerializeField] private float _soundVolume = 1f;

        [Header("Infection Visuals & Effects")]
        [SerializeField] private Material _infectedMaterial;
        [SerializeField] private List<Renderer> _renderers = new();
        [SerializeField] private Light _pointLight;
        [SerializeField] private Collider _itemCollider;

        private ProgressionService _progression;
        private bool _isCollected;

        public override void OnSpawn()
        {
            base.OnSpawn();
            _progression = ServiceLocator.Get<ProgressionService>();
            _isCollected = false;

            // Если объект используется из пула, возвращаем коллайдер на место
            if (_itemCollider != null)
                _itemCollider.enabled = true;
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            _progression = null;
        }

        public override void Interact(GameObject interactor)
        {
            if (_isCollected) return;
            
            if (!interactor.TryGetComponent<PlayerState>(out _))
                return;
            
            Collect();
        }

        private void Collect()
        {
            _isCollected = true;
            
            int current = _progression.GetState(_progressionKey);
            _progression.SetState(_progressionKey, current + _progressionValue);
            
            Debug.Log($"Collected {_progressionKey}: {current + _progressionValue}");

            if (_infectionSound != null)
            {
                AudioSource.PlayClipAtPoint(_infectionSound, transform.position, _soundVolume);
            }

            if (_pointLight != null)
            {
                Destroy(_pointLight.gameObject);
            }

            if (_itemCollider != null)
            {
                _itemCollider.enabled = false;
            }

            if (_infectedMaterial != null)
            {
                foreach (var rend in _renderers)
                {
                    if (rend == null) continue;

                    Material[] materials = rend.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = _infectedMaterial;
                    }
                    rend.materials = materials;
                }
            }
            
            OnDespawn();
        }
    }
}