namespace Red {
    using System;
    using UniRx;
    using UnityEngine.UI;

    public partial class RExtensions {
		
        public static IDisposable Subscribe(this Button button)
        {
            return button.OnClickAsObservable().Subscribe();
        }

        public static IDisposable Subscribe(this Button button, Action onNext)
        {
            return button.OnClickAsObservable().Subscribe(_ => onNext());
        }

        public static IDisposable Subscribe(this Button button, Action onNext, Action<Exception> onError)
        {
            return button.OnClickAsObservable().Subscribe(_ => onNext(), onError);
        }

        public static IDisposable Subscribe(this Button button, Action onNext, Action onCompleted)
        {
            return button.OnClickAsObservable().Subscribe(_ => onNext(), onCompleted);
        }

        public static IDisposable Subscribe(this Button button, Action onNext, Action<Exception> onError, Action onCompleted)
        {
            return button.OnClickAsObservable().Subscribe(_ => onNext(), onError);
        }
    }
}