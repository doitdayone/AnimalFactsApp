using Plugin.Maui.Audio;
using System;
using System.Diagnostics;

namespace AnimalFactsApp;

public partial class AnimalPage : ContentPage
{
    private List<Animal> _animals;
    private int _currentIndex = 0;
    private IAudioPlayer _audioPlayer;
    private CancellationTokenSource _ttsCts;
    private readonly IAudioManager _audioManager = AudioManager.Current;

    public AnimalPage()
    {
        InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("animal_data.json");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        _animals = System.Text.Json.JsonSerializer.Deserialize<List<Animal>>(json);

        ShowAnimal();
    }

    private void CancelSpeech()
    {
        if (_ttsCts != null && !_ttsCts.IsCancellationRequested)
        {
            _ttsCts.Cancel();
        }
    }
    private void CancelAudio()
    {
        if (_audioPlayer != null)
        {
            _audioPlayer.Stop();
            _audioPlayer.Dispose();
            _audioPlayer = null;
        }
    }

    private async void ShowAnimal()
    {
        if (_animals == null || !_animals.Any()) return;

        var animal = _animals[_currentIndex];
        AnimalImage.Source = !string.IsNullOrEmpty(animal.ImagePath) ? animal.ImagePath : "default_animal.png";
        AnimalName.Text = animal.Name;
        AnimalFact.Text = animal.Fact;

        CancelAudio();
        CancelSpeech();
        _ttsCts = new CancellationTokenSource();
        try
        {
            await TextToSpeech.SpeakAsync(animal.Fact, cancelToken: _ttsCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Bị hủy, không làm gì
        }
    }

    private void OnNextClicked(object sender, EventArgs e)
    {
        if (_animals == null) return;
        _currentIndex = (_currentIndex + 1) % _animals.Count;
        ShowAnimal();
    }

    private void OnPrevClicked(object sender, EventArgs e)
    {
        if (_animals == null) return;
        _currentIndex = (_currentIndex - 1 + _animals.Count) % _animals.Count;
        ShowAnimal();
    }

    private async void OnFactTapped(object sender, EventArgs e)
    {
        if (_animals == null) return;
        CancelSpeech();
        _ttsCts = new CancellationTokenSource();
        try
        {
            await TextToSpeech.SpeakAsync(_animals[_currentIndex].Fact, cancelToken: _ttsCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Bị hủy
        }
    }

    private async void OnImageTapped(object sender, EventArgs e)
    {
        if (_animals == null) return;

        var soundPath = _animals[_currentIndex].SoundPath;
        if (string.IsNullOrEmpty(soundPath)) return;
        CancelAudio();
        try
        {
            var stream = await FileSystem.OpenAppPackageFileAsync(soundPath);
            _audioPlayer = _audioManager.CreatePlayer(stream);
            _audioPlayer.Play();
        }
        catch (FileNotFoundException fnfEx)
        {
            Debug.WriteLine($"Không tìm thấy file âm thanh: {soundPath}. Chi tiết: {fnfEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi khi phát âm thanh: {ex.Message}");
        }
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CancelSpeech();
    }
}