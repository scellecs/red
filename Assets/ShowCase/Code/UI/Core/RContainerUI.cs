//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

namespace Red.Example.UI {
    using System;
    using UniRx;
    using UniRx.Async;    
    
    public class RContainerUi : IDisposable {
        private readonly RContainer container = new RContainer();
        private readonly IReadOnlyReactiveProperty<CuiManager> manager;

        public RContainerUi() {
            this.manager = this.container.ResolveStream<CuiManager>().ToReactiveProperty();
        }
        
        public void RegisterManager(CuiManager manager) {
            this.container.Register(manager);
        }
        
        public async UniTask<T> ResolveWindow<T>() where T : RContract<T>, IWindow<T>, new() {
            var window = this.container.Resolve<T>();
            if (window != null) {
                return window;
            }

            var managerInstance = this.manager.Value ?? await this.manager.First(m => m != null);

            window = await managerInstance.ResolveWindow<T>();
            this.container.Register(window);
            return window;
        }
        
        public IObservable<CuiManager> ResolveManager() {
            return this.container.ResolveAsync<CuiManager>();
        }

        public void Dispose() {
            this.container.Dispose();
        }
    }
}