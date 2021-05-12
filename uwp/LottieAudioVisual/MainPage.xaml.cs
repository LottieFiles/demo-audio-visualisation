using Microsoft.Toolkit.Uwp.UI.Lottie;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.Media;
using AudioVisualizer;
using Windows.Media.Audio;
using Windows.Devices.Enumeration;
using Windows.Storage.Pickers;
using System.Runtime.InteropServices;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Microsoft.UI.Xaml.Controls;

namespace LottieAudioVisual
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();            
        }

        LottieVisualSource lottieVisualSource = new LottieVisualSource();
        
        AudioGraph audioGraph;
        AudioDeviceInputNode deviceInputNode;
        AudioFrameOutputNode frameOutputNode;
               
        AudioAnalyzer analyzer = new AudioAnalyzer(10000000, 1, 16000, 480, 120, 1024, false);
        
        static double frames = 120;
        static double scale = 33/frames;
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await lottieVisualSource.SetSourceAsync(new Uri("https://assets2.lottiefiles.com/datafiles/QeC7XD39x4C1CIj/data.json"));

            analyzer.Output += Analyzer_Output;

            await InitAudioGraph();

            //Create input, output, wire up
            await CreateDeviceInputNode();
            CreateFrameOutputNode();
            deviceInputNode.AddOutgoingConnection(frameOutputNode);

            audioGraph.Start();

            base.OnNavigatedTo(e);
        }

        private async Task InitAudioGraph()
        {

            AudioGraphSettings settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Speech);

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
            {
                //ShowErrorMessage("AudioGraph creation error: " + result.Status.ToString());
            }

            audioGraph = result.Graph;

        }

        private async Task CreateDeviceInputNode()
        {
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(Windows.Media.Devices.MediaDevice.GetAudioCaptureSelector());
            var selectedDevice = devices.First(x => x.Name.Contains("Headset"));
            
            //var selectedDevice = devices.First();

            // Create a device output node
            CreateAudioDeviceInputNodeResult result = await audioGraph.CreateDeviceInputNodeAsync(MediaCategory.Speech, audioGraph.EncodingProperties, selectedDevice);
            
            if (result.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device output node
                //ShowErrorMessage(result.Status.ToString());
                return;
            }

            deviceInputNode = result.DeviceInputNode;
        }

        private void CreateFrameOutputNode()
        {
            frameOutputNode = audioGraph.CreateFrameOutputNode();
            audioGraph.QuantumStarted += AudioGraph_QuantumStarted;
        }

        private void AudioGraph_QuantumStarted(AudioGraph sender, object args)
        {
            AudioFrame frame = frameOutputNode.GetFrame();
            ProcessFrameOutput(frame);
            analyzer.ProcessInput(frame);
        }

        unsafe private void ProcessFrameOutput(AudioFrame frame)
        {
            using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                byte* dataInBytes;
                uint capacityInBytes;
                float* dataInFloat;

                // Get the buffer from the AudioFrame
                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

                dataInFloat = (float*)dataInBytes;
            }
        }

        private async void Analyzer_Output(AudioAnalyzer sender, VisualizationDataFrame args)
        {
            var rms = args.RMS[0];
            var peak = args.Peak[0];
            //System.Diagnostics.Trace.WriteLine($"{DateTime.Now:HH:mm:ss:fff}: Frame processed, RMS={rms} Peak={peak}");

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var value = rms * scale;
                //var value = Math.Max(rms, peak * 0.8);
                //var value = Convert.ToDouble(rms);
                
                animatedVisualPlayer.SetProgress(value);
            });            
        }

        //private async Task CreateFileInputNode()
        //{
        //    if (audioGraph == null)
        //        return;

        //    FileOpenPicker filePicker = new FileOpenPicker();
        //    filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
        //    filePicker.FileTypeFilter.Add(".mp3");
        //    filePicker.FileTypeFilter.Add(".wav");
        //    filePicker.FileTypeFilter.Add(".wma");
        //    filePicker.FileTypeFilter.Add(".m4a");
        //    filePicker.ViewMode = PickerViewMode.Thumbnail;
        //    StorageFile file = await filePicker.PickSingleFileAsync();

        //    // File can be null if cancel is hit in the file picker
        //    if (file == null)
        //    {
        //        return;
        //    }
        //    CreateAudioFileInputNodeResult result = await audioGraph.CreateFileInputNodeAsync(file);

        //    if (result.Status != AudioFileNodeCreationStatus.Success)
        //    {
        //        //ShowErrorMessage(result.Status.ToString());
        //    }

        //    fileInputNode = result.FileInputNode;
        //}

        //private async Task CreateFileOutputNode()
        //{
        //    FileSavePicker saveFilePicker = new FileSavePicker();
        //    saveFilePicker.FileTypeChoices.Add("Pulse Code Modulation", new List<string>() { ".wav" });
        //    saveFilePicker.FileTypeChoices.Add("Windows Media Audio", new List<string>() { ".wma" });
        //    saveFilePicker.FileTypeChoices.Add("MPEG Audio Layer-3", new List<string>() { ".mp3" });
        //    saveFilePicker.SuggestedFileName = "New Audio Track";
        //    StorageFile file = await saveFilePicker.PickSaveFileAsync();

        //    // File can be null if cancel is hit in the file picker
        //    if (file == null)
        //    {
        //        return;
        //    }

        //    Windows.Media.MediaProperties.MediaEncodingProfile mediaEncodingProfile;
        //    switch (file.FileType.ToString().ToLowerInvariant())
        //    {
        //        case ".wma":
        //            mediaEncodingProfile = MediaEncodingProfile.CreateWma(AudioEncodingQuality.High);
        //            break;
        //        case ".mp3":
        //            mediaEncodingProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
        //            break;
        //        case ".wav":
        //            mediaEncodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
        //            break;
        //        default:
        //            throw new ArgumentException();
        //    }


        //    // Operate node at the graph format, but save file at the specified format
        //    CreateAudioFileOutputNodeResult result = await audioGraph.CreateFileOutputNodeAsync(file, mediaEncodingProfile);

        //    if (result.Status != AudioFileNodeCreationStatus.Success)
        //    {
        //        // FileOutputNode creation failed
        //        //ShowErrorMessage(result.Status.ToString());
        //        return;
        //    }

        //    fileOutputNode = result.FileOutputNode;
        //}
    }

    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }
}
