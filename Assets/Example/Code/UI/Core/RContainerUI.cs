//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

namespace Red.Example.UI {
    using System;
    using UniRx;
    using UniRx.Async;    
    
    public class RContainerUI : IDisposable {
        private readonly RContainer _container = new RContainer();
        private readonly IReadOnlyReactiveProperty<CUIManager> _manager;

        public RContainerUI() {
            _manager = _container.ResolveStream<CUIManager>().ToReactiveProperty();
        }
        
        public void RegisterManager(CUIManager manager) {
            _container.Register(manager);
        }
        
        public async UniTask<T> ResolveWindow<T>() where T : RContract<T>, IWindow<T>, new() {
            var window = _container.Resolve<T>();
            if (window != null) return window;
            
            var manager = _manager.Value ?? await _container.ResolveAsync<CUIManager>();

            window = await manager.ResolveWindow<T>();
            _container.Register(window);
            return window;
        }
        
        public IObservable<CUIManager> ResolveManager() {
            return _container.ResolveAsync<CUIManager>();
        }

        public void Dispose() {
            _container.Dispose();
        }
    }
}