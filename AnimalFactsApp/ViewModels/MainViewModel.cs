using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace AnimalFactsApp.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<Animal> Animals { get; set; } = new();

        private readonly IAudioManager _audioManager = AudioManager.Current;

        public MainViewModel()
        {
            LoadData();
        }

        private async void LoadData()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("animal_data.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var list = JsonSerializer.Deserialize<List<Animal>>(json);

            if (list != null) 
            {
                foreach (var animal in list)
                    Animals.Add(animal);
            }
        }

        public async Task PlaySound(string soundFile)
        {
            var player = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(soundFile));
            player.Play();
        }

        public async Task SpeakFact(string fact)
        {
            await TextToSpeech.SpeakAsync(fact);
        }
    }
}
