//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

using System;
using Red.Example.UI;
using UnityEngine;

namespace Red.Example.UI {
    public class CDummyWindow1 : WindowContract<CDummyWindow1> {
        protected override void InitializeWindow() { }
    }
    
    public class DummyWindow1 : MonoBehaviour, UIPresenter {
        public Type ContractType => typeof(CDummyWindow1);

        private CDummyWindow1 contract;

        private void Awake() {
            this.contract = this.GetOrCreate<CDummyWindow1>();
        }

        // For closing through unity UI
        public void Close() {
            this.contract.Close();
        }
    }
}