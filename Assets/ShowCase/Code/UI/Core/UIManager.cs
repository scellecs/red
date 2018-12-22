//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

namespace Red.Example.UI {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using UniRx.Async;
    using UnityEngine;

    internal class CanvasHolder {
        public UICanvas  Canvas;
        public CUICanvas Contract;
    }

    public class CUIManager : RContract<CUIManager> {
        [Input]
        public readonly ReactiveOperation<Type, GameObject> RequestWindowOperation =
            new ReactiveOperation<Type, GameObject>();

        [Output]
        public readonly IntReactiveProperty WindowsStackSize = new IntReactiveProperty(0);

        public async UniTask<T> ResolveWindow<T>() where T : RContract<T>, IWindow<T>, new() {
            var go       = await this.RequestWindowOperation.Execute(typeof(T));
            var contract = go.GetOrCreate<T>();

            return contract;
        }
    }

    [RequireComponent(typeof(Canvas))]
    public class UIManager : MonoBehaviour {
        [SerializeField]
        private Canvas rootCanvas;
        [SerializeField]
        private UICanvas[] preloadWindows;

        private readonly Dictionary<CanvasLayer, List<CanvasHolder>> layers =
            new Dictionary<CanvasLayer, List<CanvasHolder>> {
                {CanvasLayer.Layer1, new List<CanvasHolder>(10)},
                {CanvasLayer.Layer2, new List<CanvasHolder>(10)},
                {CanvasLayer.Layer3, new List<CanvasHolder>(10)},
                {CanvasLayer.Layer4, new List<CanvasHolder>(10)}
            };

        private readonly Dictionary<Type, CanvasHolder> canvasByViewType     = new Dictionary<Type, CanvasHolder>();
        private readonly Dictionary<Type, GameObject>   prefabsByViewType    = new Dictionary<Type, GameObject>();
        private readonly ReactiveCollection<CUICanvas>  closableWindowsStack = new ReactiveCollection<CUICanvas>();

        private readonly Dictionary<Type, Type> contractToWindow = new Dictionary<Type, Type>();

        private CUIManager contract;

        private void Awake() {
            this.contract = this.GetOrCreate<CUIManager>();
            App.UI.RegisterManager(this.contract);
        }

        private void Start() {
            this.Bind();

            var prefabs = Resources.LoadAll<UICanvas>("");
            foreach (var canvasPrefab in prefabs) {
                var window = canvasPrefab.GetComponentInChildren<IPresenter>();
                this.prefabsByViewType[window.GetType()]   = canvasPrefab.gameObject;
                this.contractToWindow[window.ContractType] = window.GetType();
            }

            this.CacheExistingWindows();

            foreach (var prefab in this.preloadWindows) {
                this.InstantiateWindow(prefab.gameObject);
            }
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                this.CloseLastWindowInStack();
            }
        }

        private void Bind() {
            this.contract.RequestWindowOperation.Subscribe(context => {
                var type = context.Parameter;
                var win  = this.GetOrInstantiateCanvas(type);
                context.OnNext(win);
                context.OnCompleted();
            });
            this.closableWindowsStack.ObserveCountChanged()
                .Subscribe(newCount => this.contract.WindowsStackSize.Value = newCount);
        }

        private void CloseLastWindowInStack() {
            if (this.closableWindowsStack.Count == 0) {
                return;
            }

            var target = this.closableWindowsStack.Last();
            target.CloseByEscape.Execute();
        }

        private void CacheExistingWindows() {
            foreach (var canvas in this.rootCanvas.GetComponentsInChildren<UICanvas>()) {
                var contract = canvas.GetOrCreate<CUICanvas>();
                this.CacheSceneWindow(canvas, contract);
            }
        }

        private GameObject GetOrInstantiateCanvas(Type contractType, bool instantiateMissing = true) {
            this.contractToWindow.TryGetValue(contractType, out var windowType);
            if (windowType == null) {
                Debug.LogError("No relation to window ContractType: " + contractType);
                return null;
            }

            if (!this.canvasByViewType.ContainsKey(windowType)) {
                return instantiateMissing == false ? null : this.InstantiateWindow(windowType).gameObject;
            }

            var holder = this.canvasByViewType[windowType];
            return holder.Canvas.gameObject;
        }

        private UICanvas InstantiateWindow(Type windowType) {
            if (windowType == null || this.prefabsByViewType.ContainsKey(windowType) == false) {
                Debug.LogError("UI Prefab  Windows/" + windowType + " doesn't exists");
                return default(UICanvas);
            }

            return this.InstantiateWindow(this.prefabsByViewType[windowType]);
        }

        private UICanvas InstantiateWindow(GameObject prefab) {
            var go       = Instantiate(prefab, this.rootCanvas.transform);
            var canvas   = go.GetComponent<UICanvas>();
            var contract = canvas.GetOrCreate<CUICanvas>();
            this.CacheSceneWindow(canvas, contract);

            return canvas;
        }

        private void CacheSceneWindow(UICanvas canvas, CUICanvas contract) {
            var holder = new CanvasHolder {
                Canvas   = canvas,
                Contract = contract
            };
            contract.State.Subscribe(state => this.WindowStateChanged(contract, state));

            this.canvasByViewType.Add(canvas.Presenter.GetType(), holder);
            this.layers[contract.Layer.Value].Add(holder);

            this.contractToWindow[contract.GetType()] = canvas.Presenter.ContractType;
        }

        private void WindowStateChanged(CUICanvas contract, CanvasStage state) {
            switch (state) {
                case CanvasStage.None:
                    break;
                case CanvasStage.Opening:
                    this.MoveOnTop(contract);
                    if (contract.Modal.Value.IsStackable) {
                        this.closableWindowsStack.Add(contract);
                    }

                    break;
                case CanvasStage.Opened:
                    break;
                case CanvasStage.Closing:
                    break;
                case CanvasStage.Closed:
                    contract.Order.Value = -1;
                    this.closableWindowsStack.Remove(contract);
                    break;
            }
        }

        private void MoveOnTop(CUICanvas contract) {
            var maxOrder = 100 * (int) contract.Layer.Value;
            foreach (var cached in this.layers[contract.Layer.Value]) {
                if (cached.Contract == contract) {
                    continue;
                }

                maxOrder = Mathf.Max(maxOrder, cached.Contract.Order.Value);
            }

            var order = maxOrder + 5;
            if (maxOrder <= 0) {
                order = 0;
            }

            contract.Order.Value = order;
        }

        [Serializable]
        public enum CanvasLayer {
            Layer1 = 0, // Back (In-Game UI)
            Layer2 = 1, // Windows
            Layer3 = 2, // Whatever
            Layer4 = 3  // Errors
        }
    }
}