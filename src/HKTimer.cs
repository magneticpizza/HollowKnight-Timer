using System.IO;
using System.Reflection;
using Modding;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine;

namespace HKTimer {
    public class HKTimer : Mod, ITogglableMod {

        public static Settings settings { get; private set; } = new Settings();

        public static HKTimer instance { get; private set; }

        public GameObject gameObject { get; private set; }
        public Timer timer { get; private set; }
        public TriggerManager triggerManager { get; private set; }
        // oh god oh fuck
        public UI.UIManager ui { get; private set; }
        public static string HKTimerPath;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize() {
            if (instance != null) {
                return;
            }
            HKTimerPath = Application.persistentDataPath + "/HKTimer 1221";

            instance = this;
            gameObject = new GameObject();

            timer = gameObject.AddComponent<Timer>();
            timer.InitDisplay();

            triggerManager = gameObject.AddComponent<TriggerManager>().Initialize(timer);
            triggerManager.InitDisplay();

            ui = gameObject.AddComponent<UI.UIManager>().Initialize(triggerManager, this, timer);
            ui.InitDisplay();

            USceneManager.activeSceneChanged += SceneChanged;
            Object.DontDestroyOnLoad(gameObject);

            this.ReloadSettings();
        }

        public void Unload() {
            this.timer.UnloadHooks();
            GameObject.Destroy(gameObject);
            USceneManager.activeSceneChanged -= SceneChanged;
            HKTimer.instance = null;
        }

        public void ReloadSettings() {
            HKTimerPath = Application.persistentDataPath + "/HKTimer 1221";
            Directory.CreateDirectory(HKTimerPath);
            string timerPath = HKTimerPath + "/GlobalSettings.json";

            if (!File.Exists(timerPath)) {
                // If you used an older version of HKTimer, import settings
                if (File.Exists(Application.persistentDataPath + "/hktimer.json"))
                {
                    File.Copy(Application.persistentDataPath + "/hktimer.json", timerPath);
                } else
                {
                    Modding.Logger.Log("[HKTimer] Writing default settings to " + timerPath);
                    File.WriteAllText(timerPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
                }
            } else {
                Modding.Logger.Log("[HKTimer] Reading settings from " + timerPath);
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(timerPath));
                // just to add the default shit I guess
                // might remove this when the format stabilizes
                File.WriteAllText(timerPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
                settings.LogBindErrors();
            }
            // Reload text positions
            timer.InitDisplay();
            triggerManager.InitDisplay();
        }

        private void SceneChanged(Scene from, Scene to) {
            triggerManager.SpawnTriggers();
        }
    }
}