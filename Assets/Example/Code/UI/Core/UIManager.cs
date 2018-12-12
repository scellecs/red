//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Async;
using UnityEngine;

namespace Red.Example.UI {
    internal class CanvasHolder {
        public UICanvas Canvas;
        public CUICanvas Contract;
    }

    public class CUIManager : RContract<CUIManager> {
        
        [Input]
        public readonly ReactiveOperation<Type, GameObject> RequestWindowOperation = new ReactiveOperation<Type, GameObject>();
        
        [Output]
        public readonly IntReactiveProperty WindowsStackSize = new IntReactiveProperty(0);

        public async UniTask<T> ResolveWindow<T>() where T : RContract<T>, IWindow<T>, new() {
            var go = await RequestWindowOperation.Execute(typeof(T));
            var contract = go.GetOrCreate<T>();

            return contract;
        }
    }

    [RequireComponent(typeof(Canvas))]
    public class UIManager : MonoBehaviour {
        [SerializeField] private Canvas _rootCanvas;        
        [SerializeField] private UICanvas[] _preloadWindows;

        private readonly Dictionary<CanvasLayer, List<CanvasHolder>> _layers =
            new Dictionary<CanvasLayer, List<CanvasHolder>>() {
                {CanvasLayer.Layer1, new List<CanvasHolder>(10)},
                {CanvasLayer.Layer2, new List<CanvasHolder>(10)},
                {CanvasLayer.Layer3, new List<CanvasHolder>(10)},
                {CanvasLayer.Layer4, new List<CanvasHolder>(10)},
            };

        private readonly Dictionary<Type, CanvasHolder> _canvasByViewType = new Dictionary<Type, CanvasHolder>();
        private readonly Dictionary<Type, GameObject> _prefabsByViewType = new Dictionary<Type, GameObject>();
        private readonly ReactiveCollection<CUICanvas> _closableWindowsStack = new ReactiveCollection<CUICanvas>();

        private readonly Dictionary<Type, Type> _contractToWindow = new Dictionary<Type, Type>();

        private CUIManager _contract;

        private void Awake() {
            _contract = this.GetOrCreate<CUIManager>();       
            App.UI.RegisterManager(_contract);
        }        
        
        private void Start() {
            Bind();            

            var prefabs = Resources.LoadAll<UICanvas>("");
            foreach (var canvasPrefab in prefabs) {
                var window = canvasPrefab.GetComponentInChildren<UIPresenter>();
                _prefabsByViewType[window.GetType()] = canvasPrefab.gameObject;
                _contractToWindow[window.ContractType] = window.GetType();
            }

            CacheExistingWindows();

            foreach (var prefab in _preloadWindows)
                InstantiateWindow(prefab.gameObject);
        }
        
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                CloseLastWindowInStack();
            }
        }

        private void Bind() {
            _contract.RequestWindowOperation.Subscribe(context => {
                var type = context.Parameter;
                var win = GetOrInstantiateCanvas(type);
                context.OnNext(win);
                context.OnCompleted();
            });
            _closableWindowsStack.ObserveCountChanged().Subscribe(newCount => _contract.WindowsStackSize.Value = newCount);
        }

        private void CloseLastWindowInStack() {
            if (_closableWindowsStack.Count == 0) return;

            var target = _closableWindowsStack.Last();
            target.CloseByEscape.Execute();
        }

        private void CacheExistingWindows() {
            foreach (var canvas in _rootCanvas.GetComponentsInChildren<UICanvas>()) {
                var contract = canvas.GetOrCreate<CUICanvas>();
                CacheSceneWindow(canvas, contract);
            }
        }

        private GameObject GetOrInstantiateCanvas(Type contractType, bool instantiateMissing = true) {
            _contractToWindow.TryGetValue(contractType, out var windowType);
            if (windowType == null) {
                Debug.LogError("No relation to window ContractType: " + contractType);
                return null;
            }

            if (!_canvasByViewType.ContainsKey(windowType)) {
                return instantiateMissing == false ? null : InstantiateWindow(windowType).gameObject;
            }

            var holder = _canvasByViewType[windowType];
            return holder.Canvas.gameObject;
        }

        private UICanvas InstantiateWindow(Type windowType) {
            if (windowType == null || _prefabsByViewType.ContainsKey(windowType) == false) {
                Debug.LogError("UI Prefab  Windows/" + windowType + " doesn't exists");
                return default(UICanvas);
            }

            return InstantiateWindow(_prefabsByViewType[windowType]);
        }

        private UICanvas InstantiateWindow(GameObject prefab) {
            var go = Instantiate(prefab, _rootCanvas.transform);
            var canvas = go.GetComponent<UICanvas>();
            var contract = canvas.GetOrCreate<CUICanvas>();
            CacheSceneWindow(canvas, contract);

            return canvas;
        }

        private void CacheSceneWindow(UICanvas canvas, CUICanvas contract) {
            var holder = new CanvasHolder() {
                Canvas = canvas,
                Contract = contract
            };
            contract.State.Subscribe(state => WindowStateChanged(contract, state));

            _canvasByViewType.Add(canvas.Presenter.GetType(), holder);
            _layers[contract.Layer.Value].Add(holder);

            _contractToWindow[contract.GetType()] = canvas.Presenter.ContractType;
        }

        private void WindowStateChanged(CUICanvas contract, CanvasStage state) {
            switch (state) {
                case CanvasStage.None:
                    break;
                case CanvasStage.Opening:
                    MoveOnTop(contract);
                    if (contract.Modal.Value.IsStackable) _closableWindowsStack.Add(contract);
                    break;
                case CanvasStage.Opened:
                    break;
                case CanvasStage.Closing:
                    break;
                case CanvasStage.Closed:
                    contract.Order.Value = -1;
                    _closableWindowsStack.Remove(contract);
                    break;
            }
        }

        private void MoveOnTop(CUICanvas contract) {
            var maxOrder = 100 * (int) contract.Layer.Value;
            foreach (var cached in _layers[contract.Layer.Value]) {
                if (cached.Contract == contract) continue;
                maxOrder = Mathf.Max(maxOrder, cached.Contract.Order.Value);
            }

            var order = maxOrder + 5;
            if (maxOrder <= 0) order = 0;

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