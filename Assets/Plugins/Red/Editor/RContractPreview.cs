#if (CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))) && UNITY_EDITOR
namespace Red.Editor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using UniRx;
    using UnityEngine;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    [CustomPreview(typeof(GameObject))]
    public class RContractPreview : ObjectPreview {
        private static Texture redCircle;
        private static Texture greenCircle;
        private static Texture blueCircle;
        private static Texture yellowCircle;

        private static GUIStyle labelStyle;

        private class ContractView {
            public string                               Name;
            public MemberView[]                         Members;
            public List<RContract.AdditionalObservable> AdditionalObservables;
            public ReorderableList                      ReorderableList;
        }

        private class MemberView {
            public string    Name;
            public string    TypeName;
            public object    LastValue;
            public Exception Exception;
            public bool      IsChanged;
            public bool      IsCompleted;
            public bool      IsErrors;
            public Type      TypeOfValue;
        }

        private readonly ReactiveCollection<ContractView> contractsView = new ReactiveCollection<ContractView>();
        private readonly CompositeDisposable              disposables   = new CompositeDisposable();

        private readonly GUIContent title = new GUIContent("Red Contracts");

        private float maxContentHeight = 80;

        private ContractView currentContractView;

        private List<Type> types = new List<Type> {
            typeof(IObservable<>),
//            typeof(ReactiveDictionary<,>),
//            typeof(ReactiveCollection<>),
        };

        private Vector2 scrollPosition;

        public override void Initialize([NotNull] Object[] targets) {
            this.InitializeGUIStyles();

            this.disposables.Clear();

            void CreateView(RContract contract) {
                string GetFriendlyName(Type type) {
                    var friendlyName = type.Name;
                    if (!type.IsGenericType) {
                        return friendlyName;
                    }

                    var iBacktick = friendlyName.IndexOf('`');
                    if (iBacktick > 0) {
                        friendlyName = friendlyName.Remove(iBacktick);
                    }

                    friendlyName += "<";
                    var typeParameters = type.GetGenericArguments();
                    for (var i = 0; i < typeParameters.Length; ++i) {
                        var typeParamName = GetFriendlyName(typeParameters[i]);
                        friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                    }

                    friendlyName += ">";

                    return friendlyName;
                }

                var view = new ContractView {
                    Name = contract.GetType().ToString(),
                };

                var members = this.GetAllMembers(contract);
                view.Members = members
                    .Concat(contract.AdditionalObservables.Select(ao => (ao.Name, ao.Observable)))
                    .Select(t => {
                        var type = t.Item2.GetType();
                        var mv   = new MemberView {Name = t.Item1, TypeName = GetFriendlyName(type)};

                        var args = type.FindCurrentGenericTypeImplementation(typeof(IObservable<>));
                        if (args != null && args.Length > 0) {
                            var argType = args[0];
                            mv.TypeOfValue = argType;

                            var obs = ObserverProvider.CreateObserverByParameter(argType);
                            obs.Value.Subscribe(obj => {
                                mv.LastValue = obj;
                                mv.IsChanged = true;
                                EditorUtility.SetDirty(this.target);
                            }).AddTo(this.disposables);
                            obs.Complete.Subscribe(_ => {
                                mv.IsCompleted = true;
                                EditorUtility.SetDirty(this.target);
                            }).AddTo(this.disposables);
                            obs.Error.Subscribe(e => {
                                mv.Exception = e;
                                mv.IsErrors  = true;
                                EditorUtility.SetDirty(this.target);
                            }).AddTo(this.disposables);

                            var disposable =
                                (IDisposable) type.GetMethod("Subscribe")?.Invoke(t.Item2, new object[] {obs});
                            disposable.AddTo(this.disposables);
                        }

                        return mv;
                    }).ToArray();

                view.ReorderableList                     =  new ReorderableList(view.Members, typeof(MemberView), false, true, false, false);
                view.ReorderableList.drawElementCallback += DrawElementCallback;
                view.ReorderableList.drawHeaderCallback  += DrawHeaderCallback;
                view.ReorderableList.onMouseUpCallback   += OnMouseUpCallback;
                this.contractsView.Add(view);
            }

            base.Initialize(targets);
            if (RContract.AllContracts.TryGetValue(this.target, out var contracts)) {
                contracts.ForEach(CreateView);
            }
            else {
                RContract.AllContracts
                    .ObserveAdd()
                    .Where(p => ReferenceEquals(p.Key, this.target))
                    .SelectMany(p => p.Value.ObserveAdd().Select(a => a.Value))
                    .Subscribe(CreateView);
            }
        }

        ~RContractPreview() {
            this.disposables.Clear();
        }

        private void InitializeGUIStyles() {
            Texture2D LoadTexture(string path) {
                var temp = Resources.Load<Texture2D>(path);
                if (temp == null) {
                    Debug.LogError($"[RED] Can't find texture for {nameof(RContractPreview)}.");
                }

                return temp;
            }

            if (redCircle == null) {
                redCircle = LoadTexture("RedCirclesDark/32x32_r");
            }

            if (greenCircle == null) {
                greenCircle = LoadTexture("RedCirclesDark/32x32_g");
            }

            if (blueCircle == null) {
                blueCircle = LoadTexture("RedCirclesDark/32x32_b");
            }

            if (yellowCircle == null) {
                yellowCircle = LoadTexture("RedCirclesDark/32x32_y");
            }

            if (labelStyle == null) {
                labelStyle = EditorStyles.label;

                labelStyle.richText = true;
            }
        }


        public override GUIContent GetPreviewTitle() => this.title;

        public override bool HasPreviewGUI() => this.contractsView.Count > 0;

        private void DrawHeaderCallback(Rect rect) {
            GUI.Label(rect, new GUIContent(this.currentContractView.Name));
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused) {
            if (Event.current.type == EventType.Repaint) {
                var memberView = this.currentContractView.Members[index];

                var status = "Idle";
                
                if (memberView.IsErrors) {
                    status = "Errors";
                }
                else if (memberView.IsCompleted) {
                    status = "Completed";
                }
                else if (memberView.IsChanged) {
                    status = "Changed";
                }

                rect.y += 2;

                var textureRect = rect;

                textureRect.width =  textureRect.height = 8f;
                textureRect.x     += 4;
                textureRect.y     += 4;
                var texture = redCircle;
                if (memberView.IsErrors) {
                    texture = yellowCircle;
                }
                else if (memberView.IsCompleted) {
                    texture = blueCircle;
                }
                else if (memberView.IsChanged) {
                    texture              = greenCircle;
                    memberView.IsChanged = false;
                    EditorUtility.SetDirty(this.target);
                }

                GUI.DrawTexture(textureRect, texture);

                rect.x += 16f;

                var temp = new GUIContent($"Name: <b>{memberView.Name}</b> Value: <b>{memberView.LastValue}</b> Status: <i>{status}</i>");
                GUI.Label(rect, temp, labelStyle);
            }
        }

        private double lastTimeClick = 0;

        private void OnMouseUpCallback(ReorderableList list) {
            var memberView = this.currentContractView.Members[list.index];
            if (memberView.IsErrors) {
                if (EditorApplication.timeSinceStartup - this.lastTimeClick < 0.200) {
                    Debug.LogException(memberView.Exception);
                }
            }
            else {
                if (memberView.TypeOfValue.InheritsOrImplements(typeof(Object))) {
                    var value = (Object) memberView.LastValue;
                    if (EditorApplication.timeSinceStartup - this.lastTimeClick < 0.200) {
                        Selection.objects = new[] {value};
                    }

                    EditorGUIUtility.PingObject(value);
                }
            }

            this.lastTimeClick = EditorApplication.timeSinceStartup;
        }


        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            var offsetWidth = 5;
            if (this.maxContentHeight > r.yMax) {
                offsetWidth = 18;
            }

            this.scrollPosition = GUI.BeginScrollView(r, this.scrollPosition, new Rect(0, 0, r.xMax - offsetWidth, this.maxContentHeight));
            r                   = new Rect(0, 0, r.xMax - offsetWidth - 2, this.maxContentHeight);

            if (this.contractsView.Count > 0) {
                foreach (var contractView in this.contractsView) {
                    this.currentContractView = contractView;

                    contractView.ReorderableList.DoList(r);
                    r.y += contractView.ReorderableList.GetHeight();
                }

                this.maxContentHeight = r.y;
            }
            else {
                GUI.Label(r, "There aren't any contracts.");
            }

            GUI.EndScrollView();
        }

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
        public ReactiveCommand<object>    Value    { get; } = new ReactiveCommand<object>();
        public ReactiveCommand<Exception> Error    { get; } = new ReactiveCommand<Exception>();
        public ReactiveCommand            Complete { get; } = new ReactiveCommand();

        public static ObserverProvider CreateObserverByParameter(Type parameterType) {
            var createObserver = typeof(ObserverProvider).GetMethod("CreateObserver")
                .MakeGenericMethod(parameterType);

            return (ObserverProvider) createObserver.Invoke(null, new object[0]);
        }

        public static ObserverProvider CreateObserver<T>() => new ObserverProvider<T>();
    }

    public class ObserverProvider<T> : ObserverProvider, IObserver<T> {
        public void OnNext(T value)          => this.Value.Execute(value);
        public void OnError(Exception error) => this.Error.Execute(error);
        public void OnCompleted()            => this.Complete.Execute();
    }
}
#endif