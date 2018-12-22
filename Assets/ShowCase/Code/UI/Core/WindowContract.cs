//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

namespace Red.Example.UI {
    using System;
    using UniRx;
    using UniRx.Async;

    public abstract class WindowContract<T, TIn, TOut> : RContractAsync<T>, IWindow<T>
        where T : RContractAsync<T>, new() {
        public readonly ReactiveProperty<TOut>                 Result = new ReactiveProperty<TOut>(default(TOut));
        public          IReadOnlyReactiveProperty<CanvasStage> State { get; private set; }

        [Output("On<Something> happened methods")]
        public readonly Subject<Unit> OnOpening = new Subject<Unit>();
        public readonly Subject<TOut> OnClosed  = new Subject<TOut>();
        public readonly Subject<TOut> OnClosing = new Subject<TOut>();

        [Output("Is<Something> state methods")]
        public IReadOnlyReactiveProperty<bool> IsClosed;

        [Internal("Standard window Close command")]
        public ReactiveCommand<TOut> CloseCommand { get; set; }

        [Internal("Standard window Open command")]
        public ReactiveCommand<TIn> OpenCommand { get; set; }

        private            CUICanvas           canvas;
        protected readonly CompositeDisposable Disposables = new CompositeDisposable();

        protected override async UniTask InitializeAsync() {
            this.canvas = this.GetSub<CUICanvas>();
            this.State  = this.canvas.State;

            this.OpenCommand = this.canvas.State.Select(s => s == CanvasStage.Closed).ToReactiveCommand<TIn>();
            this.OpenCommand.Subscribe(_ => {
                this.Result.Value = default(TOut);
                this.canvas.Open.Execute(false);
            }).AddTo(this.Disposables);

            this.CloseCommand = this.canvas.State.Select(s => s == CanvasStage.Opened).ToReactiveCommand<TOut>();
            this.CloseCommand.Subscribe(res => {
                this.Result.Value = res;
                this.canvas.Close.Execute(false);
            }).AddTo(this.Disposables);

            this.canvas.State.DistinctUntilChanged()
                .Where(s => s == CanvasStage.Opening)
                .Subscribe(_ => this.OnOpening.OnNext(Unit.Default));

            this.canvas.State.DistinctUntilChanged()
                .Where(s => s == CanvasStage.Closed)
                .Select(_ => this.Result.Value)
                .Subscribe(_ => this.OnClosed.OnNext(_)).AddTo(this.Disposables);

            this.canvas.State.DistinctUntilChanged()
                .Where(s => s == CanvasStage.Closing)
                .Select(_ => this.Result.Value)
                .Subscribe(_ => this.OnClosing.OnNext(_)).AddTo(this.Disposables);

            this.IsClosed = this.canvas.State.Select(s => s == CanvasStage.Closed).ToReactiveProperty();

            await this.InitializeWindow();
        }

        protected virtual UniTask InitializeWindow() {
            return UniTask.CompletedTask;
        }

        /// <summary>
        ///     Advanced async method with async result on desired closing stage
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
            return this.OpenCommand.Execute(default(TIn));
        }

        public bool Open(TIn settings) {
            return this.OpenCommand.Execute(settings);
        }

        public bool Close() {
            return this.CloseCommand.Execute(default(TOut));
        }

        public virtual bool Close(TOut result) {
            return this.CloseCommand.Execute(result);
        }

        public override void Dispose() {
            base.Dispose();
            this.Disposables.Clear();
        }
    }
}