using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace LottieAudioVisual
{
    static class AudioNodeFactory
    {
        public static async Task<AudioDeviceInputNode> CreateDeviceInputNode(AudioGraph audioGraph)
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
                throw new Exception();
            }

            return result.DeviceInputNode;
        }

        public static async Task<AudioDeviceOutputNode> CreateDeviceOutputNode(AudioGraph audioGraph)
        {
            //DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(Windows.Media.Devices.MediaDevice.GetAudioRenderSelector());
            //var selectedDevice = devices.First(x => x.Name.Contains("Headset"));

            //var selectedDevice = devices.First();

            // Create a device output node
            CreateAudioDeviceOutputNodeResult result = await audioGraph.CreateDeviceOutputNodeAsync();

            if (result.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device output node
                //ShowErrorMessage(result.Status.ToString());
                throw new Exception();
            }

            return result.DeviceOutputNode;
        }

        public static AudioFrameOutputNode CreateFrameOutputNode(AudioGraph audioGraph)
        {
            return audioGraph.CreateFrameOutputNode();            
        }

        public static async Task<AudioFileInputNode> CreateFileInputNode(AudioGraph audioGraph)
        {
            if (audioGraph == null)
                throw new Exception();

            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            filePicker.FileTypeFilter.Add(".mp3");
            filePicker.FileTypeFilter.Add(".wav");
            filePicker.FileTypeFilter.Add(".wma");
            filePicker.FileTypeFilter.Add(".m4a");
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            StorageFile file = await filePicker.PickSingleFileAsync();

            // File can be null if cancel is hit in the file picker
            if (file == null)
            {
                throw new Exception();
            }
            CreateAudioFileInputNodeResult result = await audioGraph.CreateFileInputNodeAsync(file);

            if (result.Status != AudioFileNodeCreationStatus.Success)
            {
                //ShowErrorMessage(result.Status.ToString());
                throw new Exception();
            }

            return result.FileInputNode;
        }

        public static async Task<AudioFileOutputNode> CreateFileOutputNode(AudioGraph audioGraph)
        {
            FileSavePicker saveFilePicker = new FileSavePicker();
            saveFilePicker.FileTypeChoices.Add("Pulse Code Modulation", new List<string>() { ".wav" });
            saveFilePicker.FileTypeChoices.Add("Windows Media Audio", new List<string>() { ".wma" });
            saveFilePicker.FileTypeChoices.Add("MPEG Audio Layer-3", new List<string>() { ".mp3" });
            saveFilePicker.SuggestedFileName = "New Audio Track";
            StorageFile file = await saveFilePicker.PickSaveFileAsync();

            // File can be null if cancel is hit in the file picker
            if (file == null)
            {
                throw new Exception();
            }

            Windows.Media.MediaProperties.MediaEncodingProfile mediaEncodingProfile;
            switch (file.FileType.ToString().ToLowerInvariant())
            {
                case ".wma":
                    mediaEncodingProfile = MediaEncodingProfile.CreateWma(AudioEncodingQuality.High);
                    break;
                case ".mp3":
                    mediaEncodingProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
                    break;
                case ".wav":
                    mediaEncodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
                    break;
                default:
                    throw new ArgumentException();
            }


            // Operate node at the graph format, but save file at the specified format
            CreateAudioFileOutputNodeResult result = await audioGraph.CreateFileOutputNodeAsync(file, mediaEncodingProfile);

            if (result.Status != AudioFileNodeCreationStatus.Success)
            {
                // FileOutputNode creation failed
                //ShowErrorMessage(result.Status.ToString());
                throw new Exception();
            }

            return result.FileOutputNode;
        }
    }
}
