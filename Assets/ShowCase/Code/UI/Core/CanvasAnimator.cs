//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

namespace Red.Example.UI {
    using System.Collections;
    using UniRx;
    using UnityEngine;
    
    public class CanvasAnimator : MonoBehaviour {
        private static readonly Vector2 ShowPositionStart = new Vector3(0, 60);
        private static readonly Vector2 HidePositionEnd = new Vector3(0, -60);

        [SerializeField] private CanvasGroup canvasGroup;        
        [SerializeField] private RectTransform contents;
        [SerializeField] private bool closeOnAwake = true;                
        [SerializeField] private bool disableContentOnClose = true;        
        
        private CUIManager managerContract;
        private CUICanvas canvas;
        private Vector2 initialPosition;

        private void Awake() {
            this.canvas = this.GetOrCreate<CUICanvas>();
            this.initialPosition = this.contents.anchoredPosition;

            this.Bind();

            if (this.closeOnAwake) {
                this.canvas.Close.Execute(true);
            }
        }

        private void Bind() {
            this.canvas.Close.Subscribe(this.Close);
            this.canvas.Open.Subscribe(this.Open);
            this.canvas.OnClosed.Subscribe(_ => {
                if (this.disableContentOnClose == true) {
                    this.contents.gameObject.SetActive(false);
                }
            });
        }

        /// <summary>
        /// Perform open animation and global window state changes
        /// </summary>
        /// <param name="force">Skip animation, jump through states</param>
        public void Open(bool force) {
            if (this.contents.gameObject.activeSelf == false) {
                this.contents.gameObject.SetActive(true);
            }

            this.canvas.State.Value = CanvasStage.Opening;

            if (force) {
                this.contents.anchoredPosition = this.initialPosition;
                this.canvasGroup.alpha = 1;
                this.canvas.State.Value = CanvasStage.Opened;
                return;
            }

            this.canvasGroup.alpha = 0;
            
            Observable.FromCoroutine(this.AnimateOpen).Subscribe(_ => this.canvas.State.Value = CanvasStage.Opened);
        }

        /// <summary>
        /// Perform close animation and global window state changes
        /// </summary>
        /// <param name="force">Skip animation, jump through states</param>
        public void Close(bool force) {
            if (force == false && this.canvas.State.Value == CanvasStage.Closed) {
                return;
            }

            this.canvas.State.Value = CanvasStage.Closing;

            if (force) {
                this.canvas.State.Value = CanvasStage.Closed;
                this.canvasGroup.alpha = 0;
                return;
            }

            Observable.FromCoroutine(this.AnimateClose).Subscribe(_ => this.canvas.State.Value = CanvasStage.Closed);
        }

        private IEnumerator AnimateOpen() {
            const float duration = 0.2f;

            var time = 0f;
            var positionFrom = this.initialPosition + ShowPositionStart;
            var positionTo = this.initialPosition;
            
            do {
                yield return null;                                
                time += Time.deltaTime;
                var t = Mathf.Clamp01(time / duration);
                this.contents.anchoredPosition = Vector2.Lerp(positionFrom, positionTo, t);
                this.canvasGroup.alpha = t;
            } while (time < duration);
        }        
        
        private IEnumerator AnimateClose() {
            const float duration = 0.2f;

            var time = 0f;
            var positionFrom = this.initialPosition;
            var positionTo = this.initialPosition + HidePositionEnd;
            
            do {
                yield return null;                                
                time += Time.deltaTime;
                var t = Mathf.Clamp01(time / duration);
                this.contents.anchoredPosition = Vector2.Lerp(positionFrom, positionTo, t);
                this.canvasGroup.alpha = 1f - t;
            } while (time < duration);
        }  
    }
}