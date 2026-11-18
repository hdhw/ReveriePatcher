using System;
using System.Net.Http;
using System.Text.Json;
using Il2Cpp;
using Il2CppRecRoom.AntiCheat;
using MelonLoader;

[assembly: MelonInfo(typeof(Reverie.Patcher.Core), "Reverie Patcher", "0.1.0", "Reverie")]
[assembly: MelonGame("Against Gravity", "Rec Room")]
[assembly: MelonColor(255, 107, 91, 214)]

namespace Reverie.Patcher
{
    public class Core : MelonMod
    {
        // Reverie servers run Mocha, which rejects clients whose X-Mocha-Build is below 1.1.0.
        internal const string GateVersion = "1.1.0";
        private const string DefaultServer = "https://127.0.0.1:5480";

        internal static string Server = DefaultServer;
        internal static string RealtimeAppId = "";
        internal static string VoiceAppId = "";
        internal static string AppVersion = "20230414_prod";
        internal static string Region = "us";

        public override void OnInitializeMelon()
        {
            MelonPreferences_Category category = MelonPreferences.CreateCategory("Reverie");
            MelonPreferences_Entry<string> entry = category.CreateEntry("ServerUrl", DefaultServer, "Server address",
                "Address of the Reverie server to play on, e.g. https://192.168.1.20:5480");
            Server = Resolve(CommandLineServer() ?? entry.Value);
            LoggerInstance.Msg("Playing on " + Server);

            SteamCache.Load();
            FetchPhoton();
        }

        internal static string Resolve(string value)
        {
            if (Uri.TryCreate(value?.Trim(), UriKind.Absolute, out Uri uri) && (uri.Scheme == "http" || uri.Scheme == "https"))
            {
                if (uri.Scheme == "http")
                {
                    // The client refuses to build http RecNet requests ("Invalid URI scheme").
                    MelonLogger.Warning("ServerUrl uses http; the game only talks https, so using https instead.");
                    uri = new UriBuilder(uri) { Scheme = "https" }.Uri;
                }
                return uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
            }
            MelonLogger.Error("ServerUrl \"" + value + "\" is not an https address; using " + DefaultServer +
                ". Fix it in UserData/MelonPreferences.cfg under [Reverie].");
            return DefaultServer;
        }

        private static string CommandLineServer()
        {
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("--reverie-server=", StringComparison.OrdinalIgnoreCase))
                {
                    return arg.Substring("--reverie-server=".Length);
                }
            }
            return null;
        }

        private static void FetchPhoton()
        {
            try
            {
                // The game itself skips certificate checks (AlwaysVerifyTLSPatch); Reverie servers use self-signed certificates.
                using HttpClientHandler handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
                using HttpClient http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5.0) };
                string json = http.GetStringAsync(Server + "/api/photon/config").GetAwaiter().GetResult();
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;
                RealtimeAppId = Read(root, "RealtimeAppId", RealtimeAppId);
                VoiceAppId = Read(root, "VoiceAppId", VoiceAppId);
                AppVersion = Read(root, "AppVersion", AppVersion);
                Region = Read(root, "Region", Region);
                if (RealtimeAppId.Length == 0)
                {
                    MelonLogger.Warning("The server has no Photon App ID set, so rooms will not load. The host sets it in the Reverie panel.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error("Could not reach " + Server + " (" + ex.Message + "). Is the server running, and is ServerUrl right?");
            }
        }

        private static string Read(JsonElement root, string name, string fallback)
        {
            return root.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? fallback
                : fallback;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "TitleScreen" || sceneName == "empty")
            {
                return;
            }

            CheatManager cheatManager = UnityEngine.Object.FindObjectOfType<CheatManager>();
            if (cheatManager != null)
            {
                UnityEngine.Object.Destroy(cheatManager.gameObject);
            }

            EACManager eacManager = UnityEngine.Object.FindObjectOfType<EACManager>();
            if (eacManager != null)
            {
                UnityEngine.Object.Destroy(eacManager.gameObject);
            }
        }
    }
}
