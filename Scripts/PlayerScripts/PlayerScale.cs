using System;
using Manager;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerScale : MonoBehaviour
    {
        [SerializeField] private Transform _character;

        private float _characterDefaultScale;

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            EventManager.PlayerEvents.PlayerScaleToZeroForCar += OnCharacterScaleToZeroForCar;
            EventManager.PlayerEvents.PlayerScaleToDefault += OnCharacterScaleToDefault;
        }

        private void UnsubscribeFromEvents()
        {
            EventManager.PlayerEvents.PlayerScaleToZeroForCar -= OnCharacterScaleToZeroForCar;
            EventManager.PlayerEvents.PlayerScaleToDefault -= OnCharacterScaleToDefault;
        }

        /// <summary>
        /// Karakteri araç içine alırken boyutunu en küçük hale getirir.
        /// </summary>
        private void OnCharacterScaleToZeroForCar(Transform parent)
        {
            StoreDefaultScale();
            ScaleCharacterToZero();
            ParentCharacterTo(parent);
            PositionCharacterInParent();
        }

        /// <summary>
        /// Karakter boyutunu varsayılan değere geri döndürür.
        /// </summary>
        private void OnCharacterScaleToDefault()
        {
            ResetCharacterScale();
            UnparentCharacter();
        }

        /// <summary>
        /// Varsayılan karakter boyutunu saklar.
        /// </summary>
        private void StoreDefaultScale()
        {
            _characterDefaultScale = _character.localScale.x;
        }

        /// <summary>
        /// Karakteri çok küçük bir boyuta ayarlar.
        /// </summary>
        private void ScaleCharacterToZero()
        {
            _character.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }

        /// <summary>
        /// Karakteri varsayılan boyutuna geri döndürür.
        /// </summary>
        private void ResetCharacterScale()
        {
            _character.localScale = new Vector3(_characterDefaultScale, _characterDefaultScale, _characterDefaultScale);
        }

        /// <summary>
        /// Karakteri verilen parent nesnenin altına yerleştirir.
        /// </summary>
        private void ParentCharacterTo(Transform parent)
        {
            _character.transform.parent = parent;
        }

        /// <summary>
        /// Karakteri parent'tan çıkartır.
        /// </summary>
        private void UnparentCharacter()
        {
            _character.transform.parent = null;
        }

        /// <summary>
        /// Karakterin parent içindeki konumunu ayarlar.
        /// </summary>
        private void PositionCharacterInParent()
        {
            _character.transform.localPosition = Vector3.up / 2;
        }
    }
}
