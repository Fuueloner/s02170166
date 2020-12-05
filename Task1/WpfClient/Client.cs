using ProtocolLibraryForDotNetAutumn;

using Newtonsoft.Json;

using System.IO;
using System.Net.Http;
using System;
using System.Collections.Generic;

namespace WpfWebClient
{
    public class Client
    {
        public delegate void OnAllImagesRecievedHandler(ImageView[] imageViews);
        public delegate void OnImageProcessedHandler(ImageView imageView);
        public delegate void OnStatisticsRecievedHandler(Dictionary<string, int> stats);

        readonly HttpClient httpClient = new HttpClient();

        public event OnAllImagesRecievedHandler  OnAllImagesRecieved;
        public event OnImageProcessedHandler     OnImageProcessed;
        public event OnStatisticsRecievedHandler OnStatisticsRecieved;
        public event Action                      OnDbCleared;
        public event Action                      OnConnectionFailed;

        public async void GetAllImages()
        {

            Console.WriteLine("Invoking the client...");
            string answerAsString = "";
            try
            {
                answerAsString = await httpClient.GetStringAsync("http://localhost:5000/image_recognition");
            }
            catch
            {
                OnConnectionFailed?.Invoke();
            }

            ImageStructure[] deserializedAnswer = JsonConvert.DeserializeObject<ImageStructure[]>(answerAsString);
            ImageView[] allImages = new ImageView[deserializedAnswer.Length];
            int i = 0;
            foreach (var item in JsonConvert.DeserializeObject<ImageStructure[]>(answerAsString))
                allImages[i++] = new ImageView() {
                    ClassOfImage = item.ClassOfImage,
                    Confidence = float.Parse(item.Confidence),
                    ImagePath = item.ImagePath,
                    ImageData = Convert.FromBase64String(item.ImageData)
                };

            OnAllImagesRecieved?.Invoke(allImages);

        }
        public async void ProcessImage(string imagePath)
        {
            byte[] imageAsBytes = File.ReadAllBytes(imagePath);
            ImageStructure request = new ImageStructure()
            {
                ImageData = Convert.ToBase64String(imageAsBytes),
                ImagePath = imagePath
            };

            var requestAsJson = JsonConvert.SerializeObject(request);
            StringContent stringContent = new StringContent(requestAsJson);
            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            string responseAsString = "";
            try
            {
                var answer = await httpClient.PutAsync("http://localhost:5000/image_recognition", stringContent);
                responseAsString = await answer.Content.ReadAsStringAsync();

                SimplePredictionStructure simplePredictionStructure = JsonConvert.DeserializeObject<SimplePredictionStructure>(responseAsString);

                ImageView imageView = new ImageView()
                {
                    ClassOfImage = simplePredictionStructure.ClassName,
                    Confidence = float.Parse(simplePredictionStructure.Confidence),
                    ImagePath = imagePath,
                    ImageData = imageAsBytes
                };

                OnImageProcessed?.Invoke(imageView);
            }
            catch
            {
                OnConnectionFailed?.Invoke();
            }
        }

        public async void GetStatistics()
        {
            string answerAsString = "";
            try
            {
                answerAsString = await httpClient.GetStringAsync("http://localhost:5000/image_recognition");
            }
            catch
            {
                OnConnectionFailed?.Invoke();
            }

            ImageStructure[] deserializedAnswer = JsonConvert.DeserializeObject<ImageStructure[]>(answerAsString);
            Dictionary<string, int> statistics = new Dictionary<string, int>();
            foreach (var item in JsonConvert.DeserializeObject<ImageStructure[]>(answerAsString))
                if (!statistics.ContainsKey(item.ClassOfImage))
                    statistics.Add(item.ClassOfImage, 1);
                else
                    ++statistics[item.ClassOfImage];

            OnStatisticsRecieved?.Invoke(statistics);
        }
        public async void ClearDb()
        {
            try
            {
                await httpClient.DeleteAsync("http://localhost:5000/image_recognition");
            }
            catch
            {
                OnConnectionFailed?.Invoke();
            }

            OnDbCleared?.Invoke();
        }

    }

}
