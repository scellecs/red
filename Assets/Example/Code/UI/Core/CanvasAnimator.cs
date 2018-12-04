//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

using DG.Tweening;
using Red;
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
        private Vector3 _initialPosition;

        private void Awake() {
            _canvas = this.GetOrCreate<CUICanvas>();
            _initialPosition = _contents.anchoredPosition3D;

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

        public async void Open(bool force) {
            if (_contents.gameObject.activeSelf == false)
                _contents.gameObject.SetActive(true);
            
            _canvas.State.Value = CanvasStage.Opening;

            if (force) {
                _contents.anchoredPosition3D = _initialPosition;
                _canvasGroup.alpha = 1;
                _canvas.State.Value = CanvasStage.Opened;
                return;
            }

            _contents.anchoredPosition = _initialPosition + ShowPositionStart;
            _canvasGroup.alpha = 0;
 
            _contents.DOKill();
            _contents.DOAnchorPos3D(_initialPosition, 0.2f)
                .SetEase(Ease.OutSine)
                .OnComplete(() => _canvas.State.Value = CanvasStage.Opened);
            
            _canvasGroup.DOFade(1f, 0.2f);
        }

        public void Close(bool force) {
            if (force == false && _canvas.State.Value == CanvasStage.Closed) return;

            _canvas.State.Value = CanvasStage.Closing;

            if (force) {
                _canvas.State.Value = CanvasStage.Closed;
                _canvasGroup.alpha = 0;
                return;
            }

            _contents.DOKill();
            _contents.DOAnchorPos3D(_initialPosition + HidePositionEnd, 0.2f)
                .SetEase(Ease.InSine)
                .OnComplete(() => _canvas.State.Value = CanvasStage.Closed);

            _canvasGroup.DOFade(0f, 0.2f);
        }
    }
}