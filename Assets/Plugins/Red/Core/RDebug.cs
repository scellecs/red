#if RED_COMMON_DEBUG
namespace Red.Diagnostic {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using UniRx;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

    public static class DebugRx {
        private static CompositeDisposable disposables = new CompositeDisposable();
        private static Subject<DebugUnit> replayStream = new Subject<DebugUnit>();
        private static DebugSample sample;

        private static BinaryFormatter formatter = new BinaryFormatter();

        [Serializable]
        private struct DebugUnit {
            public DebugContract Contract;
            public byte[] Value;
            public string Timestamp;
            public string ObservableType;

            [Serializable]
            public struct DebugContract {
                public string Field;
                public string Type;
                public string Identifier;
                public string Target;
            }

            //public string StackTrace;
        }

        private class DebugSample {
            public string Name;
            public string StartTime;
            public string EndTime;
            public List<DebugUnit> Units = new List<DebugUnit>();

            public void Save() {
                //_{Guid.NewGuid()}
                var path = Path.Combine(Application.persistentDataPath, this.Name + ".json");
                File.WriteAllText(path, JsonUtility.ToJson(this, false));
            }

            public static DebugSample Load(string name) {
                var path = Path.Combine(Application.persistentDataPath, name + ".json");
                return JsonUtility.FromJson<DebugSample>(File.ReadAllText(path));
            }
        }

        public static void Debug<T0>(this T0 contract, string commandName, ReactiveCommand command)
            where T0 : Contract {
            replayStream
                .Where(u => u.Contract.Type == contract.GetType().AssemblyQualifiedName)
                .Where(u => u.Contract.Field == commandName)
                .Where(u => u.Contract.Identifier == contract.Identifier)
                .Subscribe(u => command.Execute());
            Debug(contract, commandName, (IObservable<Unit>) command);
        }

        public static void Debug<T0, T1>(this T0 contract, string commandName, ReactiveCommand<T1> command)
            where T0 : Contract {
            replayStream
                .Where(u => u.Contract.Type == contract.GetType().AssemblyQualifiedName)
                .Where(u => u.Contract.Field == commandName)
                .Where(u => u.Contract.Identifier == contract.Identifier)
                .Subscribe(u => {
                    object obj;
                    using (var stream = new MemoryStream(u.Value)) {
                        obj = formatter.Deserialize(stream);
                    }
                    command.Execute((T1) obj);
                });
            Debug(contract, commandName, (IObservable<T1>) command);
        }

        public static void Debug<T0, T1>(this T0 contract, string observableName, IObservable<T1> observable)
            where T0 : Contract {
            observable
                .Where(_ => sample != null)
                .Subscribe(x => {
                    byte[] bytes;
                    using (var stream = new MemoryStream()) {
                        formatter.Serialize(stream, x);
                        bytes = stream.ToArray();
                    }
                    sample.Units.Add(
                        new DebugUnit {
                            ObservableType = observable.GetType().AssemblyQualifiedName,
                            Contract = new DebugUnit.DebugContract {
                                Type = contract.GetType().AssemblyQualifiedName,
                                Field = observableName,
                                Identifier = contract.Identifier,
                                Target = contract.Target?.ToString() ?? string.Empty,
                            },
                            Value = bytes, //.GetType().IsPrimitive ? x.ToString() : JsonUtility.ToJson(x),
                            Timestamp = Time.unscaledTime.ToString(),
                            //StackTrace = StackTraceUtility.ExtractStackTrace()
                        });
                })
                .AddTo(disposables);
        }

#if UNITY_EDITOR

        [MenuItem("DebugRx/Begin")]
#endif
        public static void BeginSample() {
            BeginSample("DEFAULT");
        }

        public static void BeginSample(string name) {
            sample?.Save();
            sample = new DebugSample {
                Name = name,
                StartTime = Time.unscaledTime.ToString() 
            };
        }
#if UNITY_EDITOR
        [MenuItem("DebugRx/ReplayLast")]
#endif
        public static async void ReplayLast() {
            var loadedSample = DebugSample.Load("DEFAULT");
            var time = float.Parse(loadedSample.StartTime);
            await Observable.EveryUpdate().SkipWhile(_ => time < Time.unscaledTime).Take(1);

            var index = 0;
            var currentUnit = loadedSample.Units[index];
            Observable.EveryUpdate()
                .TakeWhile(_ => index + 1 < loadedSample.Units.Count)
                .Select(_ => currentUnit)
                .Where(u => Time.unscaledTime >= float.Parse(u.Timestamp))
                .Subscribe(u => {
                    currentUnit = loadedSample.Units[++index];
                    replayStream.OnNext(u);
                })
                .AddTo(disposables);
        }

#if UNITY_EDITOR
        [MenuItem("DebugRx/End")]
#endif
        public static void EndSample() {
            if (sample != null) {
                sample.EndTime = Time.unscaledTime.ToString();
                sample.Save();
            }
            sample = null;
        }
    }
}
#endif