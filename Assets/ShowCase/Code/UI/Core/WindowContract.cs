//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

using System;
using UniRx;
using UniRx.Async;

namespace Red.Example.UI {
    public abstract class WindowContract<T, TIn, TOut> : RContractAsync<T>, IWindow<T>
        where T : RContractAsync<T>, new() {
        
        public readonly ReactiveProperty<TOut> Result = new ReactiveProperty<TOut>(default(TOut));
        public IReadOnlyReactiveProperty<CanvasStage> State { get; private set; }
        
        [Output("On<Something> happened methods")]
        public readonly Subject<Unit> OnOpening = new Subject<Unit>();
        public readonly Subject<TOut> OnClosed = new Subject<TOut>();
        public readonly Subject<TOut> OnClosing = new Subject<TOut>();    

        [Output("Is<Something> state methods")]
        public IReadOnlyReactiveProperty<bool> IsClosed;

        [Internal("Standard window Close command")]
        public ReactiveCommand<TOut> CloseCommand  { get; set; }
        [Internal("Standard window Open command")]
        public ReactiveCommand<TIn> OpenCommand { get; set; }
        
        private CUICanvas _canvas;
        protected readonly CompositeDisposable _dispose = new CompositeDisposable();

        protected override async UniTask InitializeAsync() {
            _canvas = this.GetSub<CUICanvas>();
            State = _canvas.State;

            OpenCommand = _canvas.State.Select(s => s == CanvasStage.Closed).ToReactiveCommand<TIn>();
            OpenCommand.Subscribe(_ => {
                Result.Value = default(TOut);
                _canvas.Open.Execute(false);
            }).AddTo(_dispose);
            
            CloseCommand = _canvas.State.Select(s => s == CanvasStage.Opened).ToReactiveCommand<TOut>();
            CloseCommand.Subscribe(res => {
                Result.Value = res;
                _canvas.Close.Execute(false);
            }).AddTo(_dispose);
            
            _canvas.State.DistinctUntilChanged()
                .Where(s => s == CanvasStage.Opening)
                .Subscribe(_ => OnOpening.OnNext(Unit.Default));
            
            _canvas.State.DistinctUntilChanged()
                .Where(s => s == CanvasStage.Closed)
                .Select(_ => Result.Value)
                .Subscribe(_ => OnClosed.OnNext(_)).AddTo(_dispose);

            _canvas.State.DistinctUntilChanged()
                .Where(s => s == CanvasStage.Closing)
                .Select(_ => Result.Value)
                .Subscribe(_ => OnClosing.OnNext(_)).AddTo(_dispose);

            IsClosed = _canvas.State.Select(s => s == CanvasStage.Closed).ToReactiveProperty();

            await InitializeWindow();
        }

        protected virtual UniTask InitializeWindow() {
            return UniTask.CompletedTask;
        }
        
        /// <summary>
        /// Advanced async method with async result on desired closing stage
        /// </summary>
        /// <param name="settings">Input parameter</param>
        /// <param name="stage">Window closing stage, when observable is completed</param>
        /// <typeparam name="TOut">Output result</typeparam>
        /// <returns></returns>
        public IObservable<TOut> OpenAndResultAsync(TIn settings, ObserveWindowStage stage) {
            return Observable.Create<TOut>(o => {
                this.OpenCommand.Execute(settings);
                switch (stage) {
                    case ObserveWindowStage.Closed:
                        return this.OnClosed
                            .Subscribe(res => {
                                o.OnNext(res);
                                o.OnCompleted();
                            });
                    case ObserveWindowStage.Closing:
                        return this.OnClosing
                            .Subscribe(res => {
                                o.OnNext(res);
                                o.OnCompleted();
                            });
                    default:
                        throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
                }
            });
        }

        public virtual bool Open() {
            return OpenCommand.Execute(default(TIn));
        }
        
        public bool Open(TIn settings) {
            return OpenCommand.Execute(settings);
        }
        
        public bool Close() {
            return CloseCommand.Execute(default(TOut));
        }
        
        public virtual bool Close(TOut result) {
            return CloseCommand.Execute(result);
        }        
        
        public override void Dispose() {
            base.Dispose();
            _dispose.Clear();
        }
    }
}