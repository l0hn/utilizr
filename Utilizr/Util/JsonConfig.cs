using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Utilizr.Async;

namespace Utilizr.Util
{
    public static class JsonConfig<T> where T : Loadable<T>, new()
    {
        static readonly object LOCK = new();

        static T? _instance;
        public static T Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (LOCK)
                {
                    _instance ??= LoadFromConfigFile(new T());
                }

                return _instance;
            }
        }

        internal static void Reload()
        {
            lock (LOCK)
            {
                _instance = LoadFromConfigFile(Instance);
            }
        }


        static T LoadFromConfigFile(T currentT)
        {
            try
            {
                return Loadable<T>.Load(currentT);
            }
            catch
            {
                return new T();
            }
        }
    }

    public abstract class Loadable<T> where T : Loadable<T>, new()
    {
        private static readonly object ATOMIC_LOCK = new();
        public event EventHandler? Saved;

        public static T Instance => JsonConfig<T>.Instance;

        /// <summary>
        /// Indicates if the config file exists on disk or not
        /// </summary>
        [JsonIgnore]
        public bool Exists => File.Exists(LoadPath);

        /// <summary>
        /// The amount of times to retry if saving fails. Will always try at least once.
        /// </summary>
        [JsonIgnore]
        public int SaveRetries { get; set; } = 3;

        /// <summary>
        /// If true, this will write to disk the current instance's data when the file has been
        /// deleted since it was originally loaded.
        /// </summary>
        [JsonIgnore]
        public virtual bool SaveOnLoadFaliure { get; set; } = false;
        

        /// <summary>
        /// Setting ReadOnly to true will cause an exception if a SaveInstance() attempt is made on the instance without explicitly providing a file path
        /// </summary>
        [JsonIgnore] public virtual bool ReadOnly => false;

        [JsonIgnore]
        public string LoadPath => GetLoadPath();

        public virtual void Reload()
        {
            ReloadInstance();
        }

        public static void ReloadInstance()
        {
            JsonConfig<T>.Reload();
        }

        public virtual Task SaveAsync(string customFilePath = "")
        {
            return Task.Run(() => Save(customFilePath));
        }

        /// <summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// </summary>
        public virtual void Save(string customFilePath = "")
        {
            Save(Instance, customFilePath);
        }

        /// <summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// </summary>
        public virtual void Save(T currentT, string customFilePath = "")
        {
            if (currentT.ReadOnly && string.IsNullOrEmpty(customFilePath))
                throw new InvalidOperationException($"Attempted to call {nameof(Save)}() or {nameof(SaveInstance)}() on a readonly loadable");

            int localRetryCount = SaveRetries;
            bool done = false;
            try
            {
                while (!done)
                {
                    try
                    {
                        var path = string.IsNullOrEmpty(customFilePath)
                            ? LoadPath
                            : customFilePath;

                        var dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir!);
                        }

                        lock (this)
                        {
                            var json = JsonSerializer.Serialize(currentT);
                            json = CustomSerializeStep(json);
                            File.WriteAllText(path, json);
                        }
                        done = true;
                    }
                    catch (Exception)
                    {
                        localRetryCount--;

                        if (localRetryCount <= 0)
                            throw;

                        Sleeper.Sleep(2);
                    }
                }
            }
            catch (IOException)
            {
                throw;
                //TODO: potentially throw ProcessHelper.WhoIsLockingChecker(ioEx, LoadPath);
            }

            OnSaved();
        }

        /// <summary>
        /// Locks this instance for the duration of updateAction and saves back to disk when complete
        /// NOTE: this is only atomic with other calls to AtomicUpdate or AtomicRead. if you directly access Instance or manually call SaveInstance you're on your own.
        /// IMPORTANT! Do NOT call saveinstance or load instance inside your updateAction.
        /// </summary>
        /// <param name="updateAction"></param>
        public static void AtomicUpdate(Action<T> updateAction)
        {
            lock (ATOMIC_LOCK)
            {
                updateAction(Instance);
                SaveInstance();
            }
        }

        /// <summary>
        /// Locks this instance for the duration of readAction
        /// NOTE: this is only atomic with other calls to AtomicUpdate or AtomicRead. if you directly access Instance or manually call SaveInstance you're on your own.
        /// IMPORTANT! Do NOT call saveinstance or load instance inside your readAction.
        /// </summary>
        /// <param name="readAction"></param>
        public static void AtomicRead(Action<T> readAction)
        {
            lock (ATOMIC_LOCK)
            {
                readAction(Instance);
            }
        }

        public static T Load(string customLoadPath = "")
        {
            // Could be first load, or invoked on previously loaded instance.
            // Null check so we can use the current values in memory if file deleted, etc
            var t = Instance ?? new T();
            return Load(t, customLoadPath);
        }

        public static T Load(T t, string customLoadPath = "")
        {
            var newT = Load(t, out bool failed, customLoadPath);

            if (failed && newT.SaveOnLoadFaliure && !newT.ReadOnly)
                newT.Save(newT, customLoadPath);

            return newT;
        }

        public virtual string RawLoad(string customLoadPath)
        {
            return null;
        }
        
        public static T Load(T t, out bool loadFailed, string customLoadPath = "")
        {
            loadFailed = true;
            try
            {
                string? json = null;
                try
                {
                    json = t.RawLoad(customLoadPath);
                    
                }
                catch (Exception)
                {

                }

                if (string.IsNullOrEmpty(json))
                {
                    var loadPath = string.IsNullOrEmpty(customLoadPath) ? t.LoadPath : customLoadPath;
                    if (!File.Exists(loadPath))
                        return t;

                    json = File.ReadAllText(loadPath);
                }

                if (string.IsNullOrEmpty(json))
                    return t;

                json = t.CustomDeserializeStep(json);

                if (string.IsNullOrEmpty(json))
                    return t;
                
                var loadedObj = JsonSerializer.Deserialize<T>(json);
                if (loadedObj != null)
                {
                    loadFailed = false;
                    return loadedObj;
                }
            }
            catch { }
            return t;
        }

        /// <summary>
        /// Allows the string read from file to be manipulated before json decode takes place
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        protected abstract string CustomDeserializeStep(string source);

        /// <summary>
        /// Allows the json encoded string to be manipulated before writing to file
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        protected abstract string CustomSerializeStep(string source);

        /// <summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// </summary>
        public static void SaveInstance()
        {
            Instance.Save();
        }

        public static Task SaveInstanceAsync()
        {
            return Instance.SaveAsync();
        }

        protected abstract string GetLoadPath();

        protected virtual void OnSaved()
        {
            Saved?.Invoke(this, new EventArgs());
        }
    }
}
