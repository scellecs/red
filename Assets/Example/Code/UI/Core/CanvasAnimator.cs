//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

using System.Collections;
using UniRx;
using UnityEngine;

namespace Red.Example.UI {
    public class CanvasAnimator : MonoBehaviour {
        private static readonly Vector3 ShowPositionStart = new Vector3(0, 60, 0);
        private static readonly Vector3 HidePositionEnd = new Vector3(0, -60, 0);

        [SerializeField] private CanvasGroup _canvasGroup;        
        [SerializeField] private RectTransform _contents;
        [SerializeField] private bool _closeOnAwake = true;                
        [SerializeField] private bool _disableContentOnClose = true;        
        
        private CUIManager _managerContract;
        private CUICanvas _canvas;
        private Vector2 _initialPosition;

        private void Awake() {
            _canvas = this.GetOrCreate<CUICanvas>();
            _initialPosition = _contents.anchoredPosition;

            Bind();

            if (_closeOnAwake)
                _canvas.Close.Execute(true);
        }

        private void Bind() {
            _canvas.Close.Subscribe(Close);
            _canvas.Open.Subscribe(Open);
            _canvas.OnClosed.Subscribe(_ => {
                if (_disableContentOnClose == true)
                    _contents.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// Perform open animation and global window state changes
        /// </summary>
        /// <param name="force">Skip animation, jump through states</param>
        public void Open(bool force) {
            if (_contents.gameObject.activeSelf == false)
                _contents.gameObject.SetActive(true);
            
            _canvas.State.Value = CanvasStage.Opening;

            if (force) {
                _contents.anchoredPosition = _initialPosition;
                _canvasGroup.alpha = 1;
                _canvas.State.Value = CanvasStage.Opened;
                return;
            }

            _canvasGroup.alpha = 0;
            
            Observable.FromCoroutine(AnimateOpen).Subscribe(_ => _canvas.State.Value = CanvasStage.Opened);
        }

        /// <summary>
        /// Perform close animation and global window state changes
        /// </summary>
        /// <param name="force">Skip animation, jump through states</param>
        public void Close(bool force) {
            if (force == false && _canvas.State.Value == CanvasStage.Closed) return;

            _canvas.State.Value = CanvasStage.Closing;

            if (force) {
                _canvas.State.Value = CanvasStage.Closed;
                _canvasGroup.alpha = 0;
                return;
            }

            Observable.FromCoroutine(AnimateClose).Subscribe(_ => _canvas.State.Value = CanvasStage.Closed);
        }

        private IEnumerator AnimateOpen() {
            const float duration = 0.2f;

            var time = 0f;
            var positionFrom = _initialPosition + ShowPositionStart;
            var positionTo = _initialPosition;
            
            do {
                yield return null;                                
                time += Time.deltaTime;
                var t = Mathf.Clamp01(time / duration);
                _contents.anchoredPosition = Vector2.Lerp(positionFrom, positionTo, t);
                _canvasGroup.alpha = t;
            } while (time < duration);
        }        
        
        private IEnumerator AnimateClose() {
            const float duration = 0.2f;

            var time = 0f;
            var positionFrom = _initialPosition;
            var positionTo = _initialPosition + HidePositionEnd;
            
            do {
                yield return null;                                
                time += Time.deltaTime;
                var t = Mathf.Clamp01(time / duration);
                _contents.anchoredPosition = Vector2.Lerp(positionFrom, positionTo, t);
                _canvasGroup.alpha = 1f - t;
            } while (time < duration);
        }  
    }
}