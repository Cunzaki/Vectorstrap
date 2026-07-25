using System.Reflection;

namespace Bloxstrap
{
    public class RemoteDataManager : JsonManager<RemoteDataBase>
    {
        public override string ClassName => nameof(RemoteDataManager);

        public override string LOG_IDENT_CLASS => ClassName;

        public override string FileLocation => Path.Combine(Paths.Base, "Data.json");

        public GenericTriState LoadedState = GenericTriState.Unknown;

        public event EventHandler DataLoaded = null!;

        private const string BundledDataResourceName = "Bloxstrap.Resources.DefaultData.json";

        public void Subscribe(EventHandler Handler)
        {
            switch (LoadedState)
            {
                case GenericTriState.Unknown:
                    DataLoaded += Handler;
                    break;
                case GenericTriState.Successful:
                    Handler(this, EventArgs.Empty);
                    break;
                default:
                    Handler(this, EventArgs.Empty);
                    break;
            }
        }

        public async Task WaitUntilDataFetched()
        {
            const int delay = 100;
            const int maxTries = 30;
            int tries = 0;

            while (LoadedState == GenericTriState.Unknown)
            {
                await Task.Delay(delay);
                tries++;

                if (tries >= maxTries)
                    break;
            }
        }

        /// <summary>
        /// Loads bundled/local package map data only (no Froststrap remote config).
        /// </summary>
        public async Task LoadData()
        {
            const string LOG_IDENT = $"{nameof(RemoteDataManager)}::LoadData";

            await Task.CompletedTask;

            try
            {
                EnsureBundledDataPresent();
                Load(false);
                LoadedState = GenericTriState.Successful;
                App.Logger.WriteLine(LOG_IDENT, "Local bundled data loaded");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not load bundled data");
                App.Logger.WriteException(LOG_IDENT, ex);
                LoadedState = GenericTriState.Failed;
            }

            DataLoaded?.Invoke(this, EventArgs.Empty);

            if (LoadedState == GenericTriState.Successful)
                Save();

            App.Logger.WriteLine(LOG_IDENT, $"Loading finished with status: {LoadedState}");
        }

        private void EnsureBundledDataPresent()
        {
            if (File.Exists(FileLocation))
                return;

            var directory = Path.GetDirectoryName(FileLocation);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(BundledDataResourceName)
                ?? throw new InvalidOperationException($"Missing embedded resource '{BundledDataResourceName}'");

            using var output = File.Create(FileLocation);
            resourceStream.CopyTo(output);
        }
    }
}
