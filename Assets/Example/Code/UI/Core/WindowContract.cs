//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

using System;
using Red;
using UniRx;

namespace Red.Example.UI {
    public abstract class WindowContract<T> : RContract<T> where T : RContract<T>, new() {
        public readonly ReactiveProperty<int> Result = new ReactiveProperty<int>(0);

        [Output("On<Something> happened methods")]
        public IObservable<int> OnClosed { get; private set; }        
        public IObservable<int> OnClosing { get; private set; }        

        [Output("Is<Something> state methods")]
        public IReadOnlyReactiveProperty<bool> IsClosed;

        [Internal("Standard window Close command")]
        protected ReactiveCommand<int> CloseCommand  { get; set; }
        [Internal("Standard window Open command")]
        protected ReactiveCommand OpenCommand { get; set; }
        
        private CUICanvas _canvas;
        protected readonly CompositeDisposable _dispose = new CompositeDisposable();

        protected override void Initialize() {
            _canvas = this.GetSub<CUICanvas>();
            
            OpenCommand = _canvas.State.Select(s => s == CanvasStage.Closed).ToReactiveCommand();            
            OpenCommand.Subscribe(_ => {
                Result.Value = 0;
                _canvas.Open.Execute(false);
            }).AddTo(_dispose);
                        
            CloseCommand = _canvas.State.Select(s => s == CanvasStage.Opened).ToReactiveCommand<int>();
            CloseCommand.Subscribe(res => {
                Result.Value = res;
                _canvas.Close.Execute(false);
            }).AddTo(_dispose);
            
            var onClosed = new ReactiveCommand<int>();
            OnClosed = onClosed;
            _canvas.State.DistinctUntilChanged()
                .Where(s => s == CanvasStage.Closed)
                .Select(_ => Result.Value)
                .Subscribe(_ => onClosed.Execute(_)).AddTo(_dispose);

            OnClosing = CloseCommand;

            IsClosed = _canvas.State.Select(s => s == CanvasStage.Closed).ToReactiveProperty();

            InitializeWindow();
        }

        protected abstract void InitializeWindow();

        public virtual bool Open() {
            return OpenCommand.Execute();
        }
        
        public virtual bool Close(int result = 0) {
            return CloseCommand.Execute(result);
        }
        
        public override void Dispose() {
            base.Dispose();
            _dispose.Clear();
        }
    }
}