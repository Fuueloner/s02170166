using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;

using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System;

namespace NetAutumnClassLibrary
{
    public class ConcurrentImageProcessor
    {
        public readonly ManualResetEvent     isStopped = new ManualResetEvent(false);

        readonly ConcurrentQueue<Prediction> mSummaryInfo = new ConcurrentQueue<Prediction>();

        readonly ConcurrentQueue<string>     imagePaths;
        private readonly object              dbMutex = new object();

        public ConcurrentImageProcessor(string directoryPath)
        {
            imagePaths = new ConcurrentQueue<string>(Directory.GetFiles(directoryPath, "*.jpg"));
        }

        public string GetInfo()
        {
            if (mSummaryInfo.TryDequeue(out Prediction info))
                return $"Path: {info.Path} Label: {info.Label} Confidence: {info.Confidence}";
            else
                return "";
        }

        void ImageProcessingThread()
        {
            while (imagePaths.TryDequeue(out string name))
            {
                if (isStopped.WaitOne(0))
                {
                    Console.WriteLine("Stopping thread by signal.");
                    return;
                }
                bool isMatches = false;
                lock (dbMutex)
                {
                    using (ImageInfoContext db = new ImageInfoContext())
                    {
                        var imageDuplicates = db.ImageInfos.Where(obj => obj.FullPath.Equals(name));
                        if (imageDuplicates.Count() != 0)
                        {

                            foreach (var obj in imageDuplicates)
                                using (var ms = new MemoryStream())
                                {
                                    Image.Load<Rgb24>(name, out IImageFormat format).Save(ms, format);
                                    if (obj.Image == ms.ToArray())
                                    {
                                        isMatches = true;
                                        break;
                                    }
                                }
                        }
                    }
                }
                if (!isMatches)
                {
                    SingleImageProcessor imageProcessor = new SingleImageProcessor(name);
                    Prediction info = imageProcessor.GetPrediction();
                    mSummaryInfo.Enqueue(info);
                    lock (dbMutex)
                    {
                        using (ImageInfoContext db = new ImageInfoContext())
                        {
                            ImageInfo imageInfo = new ImageInfo() { ClassName = info.Label, Confidence = info.Confidence, FullPath = info.Path };
                            using (var ms = new MemoryStream())
                            {
                                Image.Load<Rgb24>(name, out IImageFormat format).Save(ms, format);
                                imageInfo.Image = ms.ToArray();
                            }
                            db.ImageInfos.Add(imageInfo);
                            db.SaveChanges();
                        }

                    }
                }
            }

            Console.WriteLine("Thread has finished working.");
        }

        public void Work()
        {
            int maxProcCount = Environment.ProcessorCount;
            var threads = new Thread[maxProcCount];

            for (int i = 0; i < maxProcCount; ++i)
            {
                Console.WriteLine($"Starting thread {i}.");
                threads[i] = new Thread(ImageProcessingThread);
                threads[i].Start();
            }

            for (var i = 0; i < maxProcCount; ++i)
            {
                threads[i].Join();
            }

            isStopped.Set();
        }
    }
}
