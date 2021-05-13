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
        AudioFrameOutputNode frameOutputNode;

        AudioAnalyzer analyzer = new AudioAnalyzer(10000000, 1, 48000, 480, 120, 1024, false);
        AudioProcessor audioProcessor = new AudioProcessor(0.7, 2000, 0, 80);

        static double frames = 120;
        static double scale = 33/frames;
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await lottieVisualSource.SetSourceAsync(new Uri("https://assets2.lottiefiles.com/datafiles/QeC7XD39x4C1CIj/data.json"));

            analyzer.Output += Analyzer_Output;

            await InitAudioGraph();

            //Create input, output, wire up
            frameOutputNode = AudioNodeFactory.CreateFrameOutputNode(audioGraph);

            var deviceInputNode = await AudioNodeFactory.CreateDeviceInputNode(audioGraph);
            deviceInputNode.AddOutgoingConnection(frameOutputNode);
            
            //var deviceOutputNode = await AudioNodeFactory.CreateDeviceOutputNode(audioGraph);
            
            //var fileInputNode = await AudioNodeFactory.CreateFileInputNode(audioGraph);
            //fileInputNode.AddOutgoingConnection(frameOutputNode);
            
            audioGraph.QuantumStarted += AudioGraph_QuantumStarted;            

            audioGraph.Start();

            base.OnNavigatedTo(e);
        }

        private async Task InitAudioGraph()
        {

            AudioGraphSettings settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);
            settings.DesiredSamplesPerQuantum = 100;
            //var factor = settings.MaxPlaybackSpeedFactor;

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
            {
                //ShowErrorMessage("AudioGraph creation error: " + result.Status.ToString());
            }

            audioGraph = result.Graph;
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
            var dB = audioProcessor.GetLevel(rms);
            var progress = audioProcessor.GetProgress(dB);

#if DEBUG
            System.Diagnostics.Trace.WriteLine($"{DateTime.Now:HH:mm:ss:fff}: Frame processed, prog={progress} dB={dB} RMS={rms} Peak={peak}");
#endif

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var value = progress * scale;
                
                animatedVisualPlayer.SetProgress(value);
            });            
        }

        
    }

    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }
}