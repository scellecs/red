//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

namespace Red.Example.UI {
    using System;
    using UniRx;
    using UnityEngine;

    public class CDummyWindow1 : WindowContract<CDummyWindow1, int, Unit> {
    }

    public class DummyWindow1 : MonoBehaviour, IPresenter {
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