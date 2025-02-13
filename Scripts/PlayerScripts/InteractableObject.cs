using Logic;
using UnityEngine;

namespace Interactable
{
    public class InteractableObject : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Material _highlightMaterial;
        [SerializeField] private Material _defaultMaterial;

        private void Start()
        {
            InitializeMeshRendererAndMaterials();
        }

        /// <summary>
        /// Nesnenin varsayılan materyal ve MeshRenderer'ını başlatır.
        /// </summary>
        private void InitializeMeshRendererAndMaterials()
        {
            if (_meshRenderer == null)
                _meshRenderer = GetComponentInChildren<MeshRenderer>();

            _defaultMaterial = _meshRenderer.material;
        }

        /// <summary>
        /// Nesneyi vurgular veya varsayılan haline çevirir.
        /// </summary>
        internal void HighlightObject(bool active)
        {
            SetMaterial(active ? _highlightMaterial : _defaultMaterial);
        }

        /// <summary>
        /// Nesne ile etkileşime geçildiğinde yapılacak işlemler.
        /// Türetilen sınıflar farklı davranışlar ekleyebilir.
        /// </summary>
        internal virtual void Interaction()
        {
            LogInteraction();
            ResetToDefaultMaterial();
        }

        /// <summary>
        /// Etkileşim bilgisini loglar (SRP gereği Interaction içinde doğrudan log yerine ayrı metot).
        /// </summary>
        private void LogInteraction()
        {
            Debug.Log($"Interacted with {gameObject.name}");
        }

        /// <summary>
        /// Nesneyi varsayılan materyaline geri döndürür.
        /// </summary>
        private void ResetToDefaultMaterial()
        {
            SetMaterial(_defaultMaterial);
        }

        /// <summary>
        /// Nesnenin materyalini günceller. Böylece materyal değiştirme tek bir yerde kontrol edilir.
        /// </summary>
        private void SetMaterial(Material material)
        {
            if (_meshRenderer != null)
                _meshRenderer.material = material;
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            HandleTriggerExit(other);
        }

        /// <summary>
        /// Trigger'a girince oyuncu etkileşim listesine nesneyi ekler.
        /// </summary>
        private void HandleTriggerEnter(Collider other)
        {
            var playerInteractionLogic = GetPlayerInteractionLogic(other);
            if (playerInteractionLogic == null) return;

            playerInteractionLogic.InteractableObjects.Add(this);
            playerInteractionLogic.UpdateClosestInteractableObject();
        }

        /// <summary>
        /// Trigger'dan çıkınca oyuncu etkileşim listesinden nesneyi çıkarır.
        /// </summary>
        private void HandleTriggerExit(Collider other)
        {
            var playerInteractionLogic = GetPlayerInteractionLogic(other);
            if (playerInteractionLogic == null) return;

            playerInteractionLogic.InteractableObjects.Remove(this);
            playerInteractionLogic.UpdateClosestInteractableObject();
        }

        /// <summary>
        /// Collider üzerinden PlayerInteractionLogic bileşenini alır.
        /// Ayrı bir metot ile bu işlem tek bir yerde yönetilir.
        /// </summary>
        private PlayerInteractionLogic GetPlayerInteractionLogic(Collider other)
        {
            return other.GetComponent<PlayerInteractionLogic>();
        }
    }
}