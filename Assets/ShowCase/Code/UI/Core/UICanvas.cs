//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

using System;
using Red.Example.UI;
using Red;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Red.Example.UI {
    [Serializable]
    public struct CanvasModalProps {
        public bool IsStackable;
        public bool IsClosableByEscape;

        public CanvasModalProps(bool stackable, bool closable) {
            this.IsStackable = stackable;
            this.IsClosableByEscape = closable;
        }
    }
    
    public class CUICanvas : RContract<CUICanvas> {
        [Output("Public states of canvas")]
        public readonly ReactiveProperty<CanvasStage> State = new ReactiveProperty<CanvasStage>(CanvasStage.Opened);
        public IReadOnlyReactiveProperty<bool> IsOpened;
        
        [Output]
        public readonly ReactiveProperty<UIManager.CanvasLayer> Layer = new ReactiveProperty<UIManager.CanvasLayer>();

        [Input, Output]
        public readonly ReactiveProperty<int> Order = new ReactiveProperty<int>(0);

        [Output("Executed automatically at particular stage")]
        public readonly ReactiveCommand OnClosed = new ReactiveCommand();        
        public readonly ReactiveCommand OnOpening = new ReactiveCommand();
        public readonly ReactiveCommand OnOpened = new ReactiveCommand();
        
        [Input("Main open/close canvas logic")]
        public readonly ReactiveCommand<bool> Close = new ReactiveCommand<bool>();
        public readonly ReactiveCommand<bool> Open = new ReactiveCommand<bool>();

        [Output]
        public readonly ReactiveProperty<CanvasModalProps> Modal = new ReactiveProperty<CanvasModalProps>();
        
        [Input("Close canvas by android back button logic")]
        public ReactiveCommand CloseByEscape { get; private set; }

        protected override void Initialize() {
            CloseByEscape = Modal.Select(m => m.IsStackable && m.IsClosableByEscape).ToReactiveCommand();
            CloseByEscape.Subscribe(_ => Close.Execute(false));
            
            State.DistinctUntilChanged().Where(s => s == CanvasStage.Closed).Subscribe(_ => OnClosed.Execute());
            State.DistinctUntilChanged().Where(s => s == CanvasStage.Opening).Subscribe(_ => OnOpening.Execute());
            State.DistinctUntilChanged().Where(s => s == CanvasStage.Opened).Subscribe(_ => OnOpened.Execute());
            
            IsOpened = State.Select(_ => _ == CanvasStage.Opened).ToReactiveProperty();
        }
    }

    public class UICanvas : MonoBehaviour {
        public UIPresenter Presenter {
            get {
                if (_presenter != null) return _presenter;
                _presenter = GetComponent<UIPresenter>();
                return _presenter;
            }
        }

        [SerializeField] private Canvas _canvas;    
        [SerializeField] private CanvasGroup _canvasGroup;  
        [SerializeField] private GraphicRaycaster _raycaster;
        [SerializeField] private RectTransform _canvasTransform;
        [SerializeField] private UIManager.CanvasLayer _layer;
        [SerializeField] private CanvasModalProps _modal = new CanvasModalProps(true, true);
        
        private CompositeDisposable _dispose = new CompositeDisposable();
        private UIPresenter _presenter;
        private CUICanvas _contract;

        private bool _initialBlockRaycasts;

        private void OnEnable() {
            _canvasTransform.Stretch();
            _canvasTransform.anchoredPosition3D = Vector3.zero;
            _canvas.overrideSorting = true;

            if (_canvasGroup != null) {
                _initialBlockRaycasts = _canvasGroup.blocksRaycasts;
            }

            _contract = this.GetOrCreate<CUICanvas>();
            _contract.AddTo(_dispose);
            _contract.Layer.Value = _layer;
            _contract.Modal.Value = _modal;

            Bind();
        }

        private void Bind() {
            _contract.OnClosed.Subscribe(_ => {
                _canvas.enabled = false;
                if (_raycaster != null) _raycaster.enabled = false;
                if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;
                
            });
            _contract.Open.Subscribe(_ => {
                _canvas.enabled = true;
                if (_raycaster != null) _raycaster.enabled = true;
                if (_canvasGroup != null) _canvasGroup.blocksRaycasts = _initialBlockRaycasts;                
            });
            _contract.Order.Subscribe(order => _canvas.sortingOrder = order);
        }

        private void OnDisable() {
            _dispose.Clear();
        }
    }
}