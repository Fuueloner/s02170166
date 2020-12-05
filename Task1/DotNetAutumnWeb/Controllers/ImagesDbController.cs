using ProtocolLibraryForDotNetAutumn;
using NetAutumnClassLibrary;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Linq;

namespace DotNetAutumnWeb
{
    
    [ApiController]
    [Route("image_recognition")]
    public class ImagesDbController : ControllerBase
    {
        private readonly object mDbMutex = new object();

        [HttpGet]
        public ImageStructure[] Get()
        {
            using ImageInfoContext db = new ImageInfoContext();
            int countOfImages = db.ImageInfos.Count();
            ImageStructure[] response = new ImageStructure[countOfImages];
            int i = 0;
            lock (mDbMutex)
                foreach (var imageInfo in db.ImageInfos.AsNoTracking())
                    response[i++] = new ImageStructure {
                        ClassOfImage = imageInfo.ClassName,
                        Confidence = imageInfo.Confidence.ToString(),
                        ImagePath = imageInfo.FullPath,
                        ImageData = Convert.ToBase64String(imageInfo.Image)
                    };

            return response;
        }

        [HttpPut]
        public SimplePredictionStructure Put(ImageStructure request)
        {
            using ImageInfoContext db = new ImageInfoContext();

            bool       isMatches = false;
            byte[]     image = Convert.FromBase64String(request.ImageData);
            Prediction prediction = new Prediction();

            lock (mDbMutex)
            {
                var imageDuplicates = db.ImageInfos.AsNoTracking().Where(obj => obj.FullPath.Equals(request.ImagePath));
                if (imageDuplicates.Count() != 0)
                    foreach (var obj in imageDuplicates)
                        if (ComputingTools.UnsafeCompareBytes(obj.Image, image))
                        {
                            isMatches = true;
                            prediction.Label = obj.ClassName;
                            prediction.Confidence = obj.Confidence;
                            break;
                        }


                if (!isMatches)
                {
                    SingleBlobImageProcessor imageProcessor = new SingleBlobImageProcessor(image);
                    prediction = imageProcessor.GetPrediction();
                    prediction.Path = request.ImagePath;
                    lock (mDbMutex)
                    {
                        ImageInfo imageInfo = new ImageInfo()
                        {
                            ClassName = prediction.Label,
                            FullPath = prediction.Path,
                            Confidence = prediction.Confidence,
                            Image = image
                        };

                        db.ImageInfos.Add(imageInfo);
                        db.SaveChanges();

                    }
                }
            }

            return new SimplePredictionStructure() { ClassName = prediction.Label, Confidence = prediction.Confidence.ToString() };

        }

        [HttpDelete]
        public void Delete()
        {
            using ImageInfoContext db = new ImageInfoContext();
            lock (mDbMutex)
            {
                foreach (var imageInfo in db.ImageInfos)
                    db.ImageInfos.Remove(imageInfo);
                db.SaveChanges();
            }
        }
    }
}
