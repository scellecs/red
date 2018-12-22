//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

namespace Red.Example.UI {
    using System;
    using UniRx;
    using UnityEngine;
    using UnityEngine.UI;

    [Serializable]
    public struct CanvasModalProps {
        public bool IsStackable;
        public bool IsClosableByEscape;

        public CanvasModalProps(bool stackable, bool closable) {
            this.IsStackable        = stackable;
            this.IsClosableByEscape = closable;
        }
    }

    public class CUICanvas : RContract<CUICanvas> {
        [Output("Public states of canvas")]
        public readonly ReactiveProperty<CanvasStage> State = new ReactiveProperty<CanvasStage>(CanvasStage.Opened);
        public IReadOnlyReactiveProperty<bool> IsOpened;

        [Output]
        public readonly ReactiveProperty<UIManager.CanvasLayer> Layer = new ReactiveProperty<UIManager.CanvasLayer>();

        [Input]
        [Output]
        public readonly ReactiveProperty<int> Order = new ReactiveProperty<int>(0);

        [Output("Executed automatically at particular stage")]
        public readonly ReactiveCommand OnClosed = new ReactiveCommand();
        public readonly ReactiveCommand OnOpening = new ReactiveCommand();
        public readonly ReactiveCommand OnOpened  = new ReactiveCommand();

        [Input("Main open/close canvas logic")]
        public readonly ReactiveCommand<bool> Close = new ReactiveCommand<bool>();
        public readonly ReactiveCommand<bool> Open = new ReactiveCommand<bool>();

        [Output]
        public readonly ReactiveProperty<CanvasModalProps> Modal = new ReactiveProperty<CanvasModalProps>();

        [Input("Close canvas by android back button logic")]
        public ReactiveCommand CloseByEscape { get; private set; }

        protected override void Initialize() {
            this.CloseByEscape = this.Modal.Select(m => m.IsStackable && m.IsClosableByEscape).ToReactiveCommand();
            this.CloseByEscape.Subscribe(_ => this.Close.Execute(false));

            this.State.DistinctUntilChanged().Where(s => s == CanvasStage.Closed)
                .Subscribe(_ => this.OnClosed.Execute());
            this.State.DistinctUntilChanged().Where(s => s == CanvasStage.Opening)
                .Subscribe(_ => this.OnOpening.Execute());
            this.State.DistinctUntilChanged().Where(s => s == CanvasStage.Opened)
                .Subscribe(_ => this.OnOpened.Execute());

            this.IsOpened = this.State.Select(_ => _ == CanvasStage.Opened).ToReactiveProperty();
        }
    }

    public class UICanvas : MonoBehaviour {
        public IPresenter Presenter {
            get {
                if (this.presenter != null) {
                    return this.presenter;
                }

                this.presenter = this.GetComponent<IPresenter>();
                return this.presenter;
            }
        }

        [SerializeField]
        private Canvas canvas;
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private GraphicRaycaster raycaster;
        [SerializeField]
        private RectTransform canvasTransform;
        [SerializeField]
        private UIManager.CanvasLayer layer;
        [SerializeField]
        private CanvasModalProps modal = new CanvasModalProps(true, true);

        private readonly CompositeDisposable dispose = new CompositeDisposable();
        private          IPresenter          presenter;
        private          CUICanvas           contract;

        private bool initialBlockRaycasts;

        private void OnEnable() {
            this.canvasTransform.Stretch();
            this.canvasTransform.anchoredPosition3D = Vector3.zero;
            this.canvas.overrideSorting             = true;

            if (this.canvasGroup != null) {
                this.initialBlockRaycasts = this.canvasGroup.blocksRaycasts;
            }

            this.contract = this.GetOrCreate<CUICanvas>();
            this.contract.AddTo(this.dispose);
            this.contract.Layer.Value = this.layer;
            this.contract.Modal.Value = this.modal;

            this.Bind();
        }

        private void Bind() {
            this.contract.OnClosed.Subscribe(_ => {
                this.canvas.enabled = false;
                if (this.raycaster != null) {
                    this.raycaster.enabled = false;
                }

                if (this.canvasGroup != null) {
                    this.canvasGroup.blocksRaycasts = false;
                }
            });
            this.contract.Open.Subscribe(_ => {
                this.canvas.enabled = true;
                if (this.raycaster != null) {
                    this.raycaster.enabled = true;
                }

                if (this.canvasGroup != null) {
                    this.canvasGroup.blocksRaycasts = this.initialBlockRaycasts;
                }
            });
            this.contract.Order.Subscribe(order => this.canvas.sortingOrder = order);
        }

        private void OnDisable() {
            this.dispose.Clear();
        }
    }
}