namespace Red.Editor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using UnityEngine;
    using UnityEditor;
    using Object = UnityEngine.Object;

    [CustomPreview(typeof(GameObject))]
    public class RContractPreview : ObjectPreview {
        private class ContractView {
            public string       Name;
            public MemberView[] Members;
        }

        private class MemberView {
            public string Name;
            public string TypeName;
            public object LastValue;
        }

        private ReactiveCollection<ContractView> contractsView = new ReactiveCollection<ContractView>();
        private CompositeDisposable              disposables   = new CompositeDisposable();

        public override void Initialize(Object[] targets) {
            this.disposables.Clear();

            void CreateView(RContract contract) {
                var view = new ContractView {
                    Name = contract.GetType().ToString()
                };

                var members = this.GetAllMembers(contract);
                view.Members = members
                    .Select(t => {
                        var type = t.Item2.GetType();
                        var mv = new MemberView {
                            Name = t.Item1, TypeName = type.ToString()
                        };
                        var args = type.FindCurrentGenericTypeImplementation(typeof(IObservable<>));
                        if (args != null && args.Length > 0) {
                            var obs = ObserverProvider.CreateObserverByParameter(args[0]);
                            obs.Value.Subscribe(obj => mv.LastValue = obj).AddTo(this.disposables);
                            var disposable =
                                (IDisposable) type.GetMethod("Subscribe")?.Invoke(t.Item2, new object[] {obs});
                            disposable.AddTo(this.disposables);
                        }

                        return mv;
                    })
                    .ToArray();

                this.contractsView.Add(view);
            }

            base.Initialize(targets);
            if (RContract.AllContracts.TryGetValue(this.target, out var contracts)) {
                contracts.ForEach(CreateView);
            }
            else {
                RContract.AllContracts
                    .ObserveAdd()
                    .Where(p => p.Key == this.target)
                    .SelectMany(p => p.Value.ObserveAdd().Select(a => a.Value))
                    .Subscribe(CreateView);
            }
        }

        ~RContractPreview() {
            this.disposables.Clear();
        }

        public override GUIContent GetPreviewTitle() {
            return new GUIContent("Red Contracts");
        }

        public override bool HasPreviewGUI() {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            if (Event.current.type == EventType.Repaint) {
                var offset     = new Vector2(140f, 16f);
                var rectOffset = new RectOffset(-5, -5, -5, -5);
                r = rectOffset.Add(r);
                var x        = r.x + 10f;
                var y        = r.y + 10f;
                var position = new Rect(x, y, offset.x, offset.y);
                if (this.contractsView != null && this.contractsView.Count > 0) {
                    this.contractsView.ForEach(c => {
                        GUI.Label(r, $"{c.Name}");
                        r.x += 16f;
                        c.Members.ForEach(m => {
                            r.y += position.height;
                            GUI.Label(r, $"{m.Name} | {m.LastValue ?? "null"} | {m.TypeName}");
                        });
                        r.x -= 16f;
                        r.y += position.height;
                    });
                }
                else {
                    GUI.Label(r, "There aren't any contracts.");
                }
            }
        }

        private List<Type> types = new List<Type> {
            typeof(IObservable<>),
//            typeof(ReactiveDictionary<,>),
//            typeof(ReactiveCollection<>),
        };

        private IEnumerable<(string, object)> GetAllMembers(object instance) {
            var fields = instance
                .GetType()
                .GetFields()
                .Where(f => this.types.Any(t => f.FieldType.InheritsOrImplements(t)))
                .Where(f => f.GetValue(instance) != null)
                .Select(f => (f.Name, f.GetValue(instance)));

            var properties = instance
                .GetType()
                .GetProperties()
                .Where(p => this.types.Any(t => p.PropertyType.InheritsOrImplements(t)))
                .Where(p => p.GetValue(instance) != null)
                .Select(p => (p.Name, p.GetValue(instance)));

            return fields.Concat(properties);
        }
    }

    public class ObserverProvider {
        public ReactiveProperty<object>    Value    { get; } = new ReactiveProperty<object>();
        public ReactiveProperty<Exception> Error    { get; } = new ReactiveProperty<Exception>();
        public ReactiveCommand             Complete { get; } = new ReactiveCommand();

        public static ObserverProvider CreateObserverByParameter(Type parameterType) {
            var createObserver = typeof(ObserverProvider).GetMethod("CreateObserver")
                .MakeGenericMethod(parameterType);

            return (ObserverProvider) createObserver.Invoke(null, new object[0]);
        }

        public static ObserverProvider CreateObserver<T>() {
            return new ObserverProvider<T>();
        }
    }

    public class ObserverProvider<T> : ObserverProvider, IObserver<T> {
        public void OnCompleted() {
            this.Complete.Execute();
        }

        public void OnError(Exception error) {
            this.Error.Value = error;
        }

        public void OnNext(T value) {
            this.Value.Value = value;
        }
    }
}