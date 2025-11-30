#if UNITY_EDITOR
using System;
using System.Collections;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using Appcharge.PaymentLinks.Platforms.Base;
using UnityEngine;
using UnityEngine.Networking;

namespace Appcharge.PaymentLinks.Platforms.Editor {
    public class EditorPricePoints : BasePricePoints
    {
        private EditorPlatform _editorPlatform;
        
        public EditorPricePoints(ICheckoutPlatform platform, EditorPlatform editorPlatform) : base(platform)
        {
            _editorPlatform = editorPlatform;
        }

        public override void GetPricePoints()
        {
            if (Application.isPlaying)
            {
                EditorPlatform.SharedCoroutineRunner.StartCoroutine(GetPricePointsCoroutine());
            }
        }
        
        private IEnumerator GetPricePointsCoroutine()
        {
            if (!ValidatePlatform()) yield break;
            
            var url = $"{_editorPlatform.BootData.appchargeUrl}{_editorPlatform.BootData.pricePointsPath}";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var pricePointsModel = JsonUtility.FromJson<PricePointsModel>(request.downloadHandler.text);
                        _platform.Callback.OnPricePointsSuccess(pricePointsModel);
                    }
                    catch (Exception ex)
                    {
                        _platform.Callback.OnPricePointsFail(new ErrorMessage { message = ex.Message, code = 0 });
                    }
                }
                else
                {
                    _platform.Callback.OnPricePointsFail(new ErrorMessage { message = request.error, code = 0 });
                }
            }
        }
        
        private bool ValidatePlatform()
        {
            if (_editorPlatform.BootData == null || string.IsNullOrEmpty(_editorPlatform.BootData.pricePointsPath))
            {
                _platform.Callback.OnPricePointsFail(new ErrorMessage { message = "Platform not initialized", code = 0 });
                return false;
            }
            return true;
        }
    }
}
#endif
