#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
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
        private static Texture redCircle;
        private static Texture greenCircle;

        private class ContractView {
            public string       Name;
            public MemberView[] Members;
        }

        private class MemberView {
            public string Name;
            public string TypeName;
            public object LastValue;
            public bool   IsChanged;
        }

        private ReactiveCollection<ContractView> contractsView = new ReactiveCollection<ContractView>();
        private CompositeDisposable              disposables   = new CompositeDisposable();

        private readonly GUIContent title = new GUIContent("Red Contracts");


        public override void Initialize(Object[] targets) {
            this.InitializeGUIStyles();

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
                            obs.Value.Subscribe(obj => {
                                mv.LastValue = obj;
                                mv.IsChanged = true;
                                EditorUtility.SetDirty(this.target);
                            }).AddTo(this.disposables);
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

        private void InitializeGUIStyles() {
            Texture2D LoadTexture(string path) {
                var temp = AssetDatabase.LoadAssetAtPath<Texture2D>(Paths.RedFolder + "Textures/" + path);
                if (temp == null) {
                    Debug.LogError($"[RED] Can't find texture for {nameof(RContractPreview)}. " +
                                   $"Maybe you move Red folder at other path, then just change path in Paths.cs");
                }

                return temp;
            }

            if (redCircle == null) {
                redCircle = LoadTexture("RedCirclesDark/32x32_r.png");
            }

            if (greenCircle == null) {
                greenCircle = LoadTexture("RedCirclesDark/32x32_g.png");
            }
        }


        public override GUIContent GetPreviewTitle() {
            return this.title;
        }

        public override bool HasPreviewGUI() {
            return true;
        }

        private float maxContentWidth = 300;
        //don't touch this, idk how it's works
        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            this.scrollPosition = GUI.BeginScrollView(r, this.scrollPosition, new Rect(0, 0, this.maxContentWidth, 220));
            r = new Rect(0, 0, this.maxContentWidth, 220);
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
                        var maxNameSize  = 1f;
                        var maxValueSize = 40f;
                        var maxTypeSize = 40f;
                        var   rt           = r;
                        c.Members.ForEach(m => {
                            r.y += position.height;

                            var textureRect = r;

                            textureRect.width =  textureRect.height = 8f;
                            textureRect.x     += 4;
                            textureRect.y     += 4;
                            var texture = redCircle;
                            if (m.IsChanged) {
                                texture     = greenCircle;
                                m.IsChanged = false;
                                EditorUtility.SetDirty(this.target);
                            }

                            GUI.DrawTexture(textureRect, texture);

                            r.x += 16f;
                            var s = background.CalcSize(new GUIContent($"{m.Name}"));
                            maxNameSize = maxNameSize > s.x ? maxNameSize : s.x;
                            GUI.Label(r, $"{m.Name}");
                            r.x -= 16f;
                        });
                        r.y = rt.y;
                        c.Members.ForEach(m => {
                            r.y += position.height;
                            r.x += 16f;
                            var s = background.CalcSize(new GUIContent($"| {m.LastValue ?? "null"}"));
                            maxValueSize =  maxValueSize > s.x ? maxValueSize : s.x;
                            r.x          += maxNameSize;
                            GUI.Label(r, $"| {m.LastValue ?? "null"}");
                            r.x -= maxNameSize;
                            r.x -= 16f;
                        });

                        r.y = rt.y;
                        c.Members.ForEach(m => {
                            r.y += position.height;
                            r.x += 16f;
                            var s = background.CalcSize(new GUIContent($"| {m.TypeName}"));
                            maxTypeSize = maxTypeSize > s.x ? maxTypeSize : s.x;
                            var ts = maxNameSize + maxValueSize;
                            r.x += ts ;
                            GUI.Label(r, $"| {m.TypeName}");
                            r.x -= ts ;
                            r.x -= 16f;
                        });

                        this.maxContentWidth = 5 + 32 + maxNameSize + maxValueSize + maxTypeSize;
                        r.x -= 16f;
                        r.y += position.height;
                        GUI.Label(r, $"______________________________________________________________________________");
                        r.y += 16f;
                    });
                    
                }
                else {
                    GUI.Label(r, "There aren't any contracts.");
                }
            }
            GUI.EndScrollView();
        }

        private List<Type> types = new List<Type> {
            typeof(IObservable<>),
//            typeof(ReactiveDictionary<,>),
//            typeof(ReactiveCollection<>),
        };
        private Vector2 scrollPosition;

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

    //TODO rework to Subject<T> ???
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
#endif