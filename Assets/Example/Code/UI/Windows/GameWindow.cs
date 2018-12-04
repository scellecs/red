//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

using System;
using Red.Example.UI;
using Red.Example.UI.Windows.Popups;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Red.Example.UI {
    // Contract
    // Model
    // Interface
    public class CGameWindow : WindowContract<CGameWindow> {

        // Local window interface, abstraction from external interfaces
        // Local view is binded to local model        
        [Internal] public IReadOnlyReactiveProperty<float> Health;        
        [Internal] public ReactiveCommand WindowButtonCommand;
        [Internal] public ReactiveCommand DialogButtonCommand;
        [Internal] public ReactiveCommand JumpButtonCommand;
        
        // All "external model composition" logic
        protected override async void InitializeWindow() {
            var player = await App.Player.ResolveAsync<CPlayer>();
            var uiManager = await App.UI.ResolveAsync<CUIManager>();
            this.Health = player.HP;

            this.WindowButtonCommand = uiManager.WindowsStackSize
                .Select(size => size == 0)
                .ToReactiveCommand();
            
            this.DialogButtonCommand = uiManager.WindowsStackSize
                .Select(size => size == 0)
                .ToReactiveCommand();

            this.JumpButtonCommand = player.State
                .Select(state => state == PlayerState.Idle)
                .ToReactiveCommand();
            
            this.JumpButtonCommand.Subscribe(_ => player.JumpCommand.Execute());
        }
    }
    
    public class GameWindow : MonoBehaviour, UIPresenter {
        public Type ContractType => typeof(CGameWindow);

        [SerializeField] private Text hpText;
        [SerializeField] private Button windowButton;
        [SerializeField] private Button dialogWindow;
        [SerializeField] private Button jumpButton;
        
        private CGameWindow contract;
        private CUIManager ui;

        private async void Awake() {
            // Waiting in advance, before creating model
            // Otherwise there is a chance to bind to null properties
            await App.Player.ResolveAsync<CPlayer>();
            this.ui = await App.UI.ResolveAsync<CUIManager>();
            
            this.contract = this.GetOrCreate<CGameWindow>();
            Bind();
        }

        /// <summary>
        /// Main Model-to-View binding logic
        /// </summary>
        private void Bind() {
            this.contract.Health
                .Select(f => $"{f:0.0}")
                .SubscribeToText(this.hpText);

            this.contract.WindowButtonCommand.BindTo(this.windowButton);
            this.contract.DialogButtonCommand.BindTo(this.dialogWindow);
            this.contract.JumpButtonCommand.BindTo(this.jumpButton);

            this.contract.WindowButtonCommand.Subscribe(_ => OpenWindow1());
            this.contract.DialogButtonCommand.Subscribe(_ => OpenCommonPopup());
        }

        private async void OpenWindow1() {
            var window = await this.ui.GetWindow<CDummyWindow1>();
            window.Open();
        }

        private async void OpenCommonPopup() {
            // Async popup resolve
            var popup = await this.ui.GetWindow<CCommonPopup>();

            var randomText = "Text " + UnityEngine.Random.Range(0, int.MaxValue);            
            // Calling Observable creation, waiting for the first result
            var result = await popup.OpenAndResultAsync<CommonPopupResult>(randomText, ObservePopupStage.Closing);
            
            // Do something depends on result
            switch (result) {
                case CommonPopupResult.NotOpened:
                    Debug.LogError("Popup was not opened!");
                    break;
                case CommonPopupResult.Ok:
                    Debug.LogWarning("Ok pressed! Do something");
                    break;
                case CommonPopupResult.Close:
                case CommonPopupResult.Cancel:
                    Debug.LogWarning(result+" pressed! Do nothing");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}