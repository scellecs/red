//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Red.Example.UI.Windows.Popups {

    public class CCommonPopup : WindowContract<CCommonPopup> {
        public IReactiveCommand<string> OpenWithParamsCommand = new ReactiveCommand<string>();

        protected override void InitializeWindow() {
            // We don't want command to be executable while this window is not fully closed
            this.OpenWithParamsCommand = new ReactiveCommand<string>(canExecuteSource: this.IsClosed);
            
            // Let's proxy parametrized open command to generic open command and then to CUICanvas 
            this.OpenWithParamsCommand.Subscribe(_ => this.OpenCommand.Execute());
        }

        /// <summary>
        /// Simple sync open method
        /// </summary>
        /// <param name="text">Example parameter</param>
        public bool Open(string text) {
            return this.OpenWithParamsCommand.Execute(text);
        }
        
        /// <summary>
        /// Advanced async method with async result on desired closing stage
        /// </summary>
        /// <param name="text">Example parameter</param>
        /// <param name="stage">Window closing stage, when observable is completed</param>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public IObservable<TEnum> OpenAndResultAsync<TEnum>(string text, ObservePopupStage stage)
            where TEnum : struct, IConvertible {
            if (typeof(TEnum).IsEnum == false) {
                throw new ArgumentException("T must be an enumerated type");
            }

            return Observable.Create<TEnum>(o => {
                this.OpenWithParamsCommand.Execute(text);
                switch (stage) {
                    case ObservePopupStage.Closed:
                        return this.OnClosed
                            .Select(intResult => (TEnum)Enum.ToObject(typeof(TEnum), intResult))
                            .Subscribe(res => {
                                o.OnNext(res);
                                o.OnCompleted();
                            });
                    case ObservePopupStage.Closing:
                        return this.OnClosing
                            .Select(intResult => (TEnum)Enum.ToObject(typeof(TEnum), intResult))
                            .Subscribe(res => {
                                o.OnNext(res);
                                o.OnCompleted();
                            });
                    default:
                        throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
                }
            });
        }

        public bool Close(CommonPopupResult result) {
            return this.CloseCommand.Execute((int)result);
        }
    }

    public class CommonPopup : MonoBehaviour, UIPresenter {
        public Type ContractType => typeof(CCommonPopup);

        [SerializeField] private Button okButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Text description;

        private CCommonPopup contract;
        private CUICanvas canvas;

        private void Awake() {
            Bind();
        }

        /// <summary>
        /// Main Model-to-View binding logic
        /// </summary>
        private void Bind() {
            this.contract = this.GetOrCreate<CCommonPopup>();
            this.canvas = this.GetOrCreate<CUICanvas>();
            this.contract.OpenWithParamsCommand.Subscribe(Setup);

            var selector = canvas.State.Select(s => s == CanvasStage.Opened);
            var okCommand = selector.ToReactiveCommand();

            okCommand.Subscribe(_ => Close(CommonPopupResult.Ok));
            okCommand.BindTo(this.okButton);

            var cancelCommand = selector.ToReactiveCommand();
            cancelCommand.Subscribe(_ => Close(CommonPopupResult.Cancel));
            cancelCommand.BindTo(this.cancelButton);
        }

        private void Setup(string text) {
            this.description.text = text;
        }

        /// <summary>
        /// For closing through internal code
        /// </summary>
        /// <param name="result">Result to push in underlying contract Result property</param>
        private void Close(CommonPopupResult result) {
            this.contract.Close(result);
        }
        
        /// <summary>
        /// For closing through unity UI
        /// </summary>
        /// <param name="result">Result to push in underlying contract Result property</param>
        public void Close(int result) {
            this.contract.Close(result);
        }
    }
}