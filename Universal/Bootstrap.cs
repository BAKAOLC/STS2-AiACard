using System.Reflection;
using System.Runtime.Loader;
using MegaCrit.Sts2.Core.Modding;

namespace AiACard.Universal
{
    [ModInitializer(nameof(Initialize))]
    public static class Bootstrap
    {
        private static readonly object Sync = new();
        private static Assembly? _payload;
        private static bool _initialized;

        public static string SelectApiVersion(Assembly gameAssembly)
        {
            var cardType = gameAssembly.GetType("MegaCrit.Sts2.Core.Models.CardModel", true)!;
            return cardType.GetMethod("GetResultLocationForCardPlay", BindingFlags.NonPublic | BindingFlags.Instance)
                != null ? "0.111.0" : "0.107.1";
        }

        public static Assembly LoadPayload()
        {
            lock (Sync)
            {
                if (_payload != null)
                    return _payload;
                var entry = typeof(Bootstrap).Assembly;
                var version = SelectApiVersion(typeof(ModManager).Assembly);
                using var stream = entry.GetManifestResourceStream($"payload.{version}.dll")
                    ?? throw new InvalidOperationException($"Missing embedded implementation for {version}.");
                var context = AssemblyLoadContext.GetLoadContext(entry) ?? AssemblyLoadContext.Default;
                _payload = context.LoadFromStream(stream);
                return _payload;
            }
        }

        public static void Initialize()
        {
            lock (Sync)
            {
                if (_initialized)
                    return;
                var entry = typeof(Bootstrap).Assembly;
                var modId = entry.GetCustomAttributes<AssemblyMetadataAttribute>()
                    .Single(a => a.Key == "ModAssemblyName").Value!;
                var payload = LoadPayload();
                var associate = typeof(ModManager).GetMethod("AssociateAssemblyWithMod",
                    BindingFlags.Public | BindingFlags.Static);
                if (associate != null)
                    associate.Invoke(null, [modId, payload]);
                else
                    ModManager.OnModDetected += mod =>
                    {
                        if (mod.manifest?.id == modId)
                        {
                            var field = mod.GetType().GetField("assembly")
                                ?? throw new MissingFieldException(mod.GetType().FullName, "assembly");
                            field.SetValue(mod, payload);
                        }
                    };

                var main = payload.GetType($"{modId}.Main", true)!;
                main.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, null);
                if (main.GetProperty("IsModActive")?.GetValue(null) is not true)
                    throw new InvalidOperationException($"{modId} did not initialize successfully.");
                _initialized = true;
            }
        }
    }
}
