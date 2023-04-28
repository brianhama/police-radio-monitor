using MathNet.Numerics.IntegralTransforms;
using NAudio.Wave;
using Newtonsoft.Json;
using RestSharp;

namespace PoliceRadioMonitor
{
    class Program
    {
        private const string API_KEY = "";
        private const string SEARCH_KEYWORD = "";

        private const int SampleRate = 44100;
        private const double Threshold = 400000;

        private static bool isRecording = false;
        private static List<short> recordedData = new List<short>();

        static void Main(string[] args)
        {
            using (var audioInput = new WaveInEvent())
            {
                audioInput.DeviceNumber = 0;
                audioInput.WaveFormat = new WaveFormat(SampleRate, 1);
                audioInput.BufferMilliseconds = 1000;
                audioInput.DataAvailable += AudioInput_DataAvailable;

                audioInput.StartRecording();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();

                audioInput.StopRecording();
            }
        }

        private static void AudioInput_DataAvailable(object? sender, WaveInEventArgs e)
        {
            short[] audioData = new short[e.BytesRecorded / 2];
            Buffer.BlockCopy(e.Buffer, 0, audioData, 0, e.BytesRecorded);

            var complexData = new MathNet.Numerics.Complex32[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
            {
                complexData[i] = new MathNet.Numerics.Complex32(audioData[i], 0.0f);
            }

            Fourier.Forward(complexData, FourierOptions.NoScaling);

            float maxAmplitude = 0;
            for (int i = 0; i < complexData.Length / 2; i++)
            {
                float amplitude = complexData[i].Magnitude;
                if (amplitude > maxAmplitude)
                {
                    maxAmplitude = amplitude;
                }
            }

            if (maxAmplitude > Threshold)
            {
                isRecording = true;
                recordedData.AddRange(audioData);
            }
            else if (isRecording)
            {
                SaveTransmission();
                isRecording = false;
                recordedData.Clear();
            }
        }

        private static void SaveTransmission()
        {
            using (MemoryStream waveStream = new MemoryStream())
            {
                using (var waveFileWriter = new WaveFileWriter(waveStream, new WaveFormat(SampleRate, 1)))
                {
                    waveFileWriter.WriteSamples(recordedData.ToArray(), 0, recordedData.Count);
                }

                string transcription = WhisperTranscribe(waveStream.GetBuffer());
                Console.WriteLine("Transcription: " + transcription);
                if (transcription.ToLower().Contains(SEARCH_KEYWORD.ToLower())) {
                    KeywordDetected();
                }
            }
        }

        private static void KeywordDetected()
        {
            Console.WriteLine("DETECTED");
        }

        static string WhisperTranscribe(byte[] fileBytes)
        {
            RestClient client = new RestClient("https://api.openai.com");
            RestRequest request = new RestRequest("/v1/audio/transcriptions", Method.Post);
            request.AddHeader("Authorization", $"Bearer {API_KEY}");
            request.AddFile("file", fileBytes, "audio.wav");
            request.AddParameter("model", "whisper-1");

            RestResponse response = client.Execute(request);

            if (response.IsSuccessful && response.Content != null)
            {
                TranscriptionResponse transcriptionResponse = JsonConvert.DeserializeObject<TranscriptionResponse>(response.Content);
                if (transcriptionResponse != null && String.IsNullOrEmpty(transcriptionResponse.text) == false)
                {
                    return transcriptionResponse.text;
                }
            }

            return "";
        }

        class TranscriptionResponse
        {
            public String? text { get; set; }
        }
    }
}