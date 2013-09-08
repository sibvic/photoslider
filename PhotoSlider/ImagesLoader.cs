//
//  Photo Slider
//  Copyright (C) 2011 Victor Tereschenko (aka sibvic)
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
// ========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;

namespace PhotoSlider
{
    class ImagesLoader : IDisposable
    {
        public ImagesLoader(ImagesScaner imagesLoader, int width, int height)
        {
            mWidth = width;
            mHeight = height;
            mImagesLoader = imagesLoader;
            for (int i = 0; i < 2; ++i)
            {
                var data = new Data();
                mData.Add(data);

                Thread preloader = new Thread(new ParameterizedThreadStart(preloaderWork));
                preloader.Start(data);
                mPreloader.Add(preloader);
            }
        }

        /// <summary>
        /// Stops layout generation.
        /// </summary>
        public void Stop()
        {
            foreach (Data data in mData)
            {
                data.Cancelled = true;
            }
            foreach (Thread preloader in mPreloader)
            {
                preloader.Join();
            }
        }

        readonly int mWidth;
        public int ImagesWidth { get { return mWidth; } }

        readonly int mHeight;
        public int ImagesHeight { get { return mHeight; } }

        readonly ImagesScaner mImagesLoader;
        
        void preloaderWork(object queue)
        {
            Data data = (Data)queue;
            while (loadImages(data))
            {
                data.mImagesLoadedEvent.Reset();
                data.mImagesLoadedEvent.WaitOne();
            }
            data.IsDone = true;
        }

        Image loadImage(string path)
        {
            try
            {
                Image original = Image.FromFile(path);
                return original;
            }
            catch (System.IO.PathTooLongException)
            {
            }
            catch (System.IO.FileNotFoundException)
            {
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
            }
            catch (System.ArgumentException)
            {
            }
            return null;
        }

        /// <summary>
        /// Loads images while we have some images to load and enought memory.
        /// </summary>
        /// <returns>true if we have some more images to load but don't have memory</returns>
        bool loadImages(Data data)
        {
            var proc = System.Diagnostics.Process.GetCurrentProcess();
            do
            {
                if (data.Cancelled)
                    return false;
                string path = mImagesLoader.Next();
                if (path == null)
                    return false;
                try
                {
                    Image original = null;
                    Image scaled = null;
                    try
                    {
                        original = loadImage(path);
                        if (original == null)
                        {
                            mImagesLoader.Return(path);
                            break;
                        }
                        try
                        {
                            if (original.Width <= mWidth && original.Height <= mHeight)
                                scaled = original;
                            else
                                scaled = ImageScaler.ScaleImage(mWidth, mHeight, original);
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        if (original != null)
                            original.Dispose();
                        mImagesLoader.Return(path);
                        break;
                    }
                    if (scaled == null)
                    {
                        original.Dispose();
                        continue;
                    }
                    data.AddImage(new StackableImage(path, original, scaled));
                }
                catch (System.OutOfMemoryException)
                {
                    mImagesLoader.Return(path);
                    break;
                }
            }
            while (true);
            return true;
        }

        class Data
        {
            public ManualResetEvent mImagesLoadedEvent = new ManualResetEvent(true);
            public bool Cancelled = false;
            public bool IsDone = false;

            object mLoadedQueueLock = new object();
            Queue<StackableImage> mPreloaded = new Queue<StackableImage>();
            
            public void AddImage(StackableImage image)
            {
                lock (mLoadedQueueLock)
                {
                    mPreloaded.Enqueue(image);
                }
            }

            public Queue<StackableImage> GetImages()
            {
                lock (mLoadedQueueLock)
                {
                    var queue = mPreloaded;
                    mPreloaded = new Queue<StackableImage>();
                    return queue;
                }
            }
        }

        List<Thread> mPreloader = new List<Thread>();
        List<Data> mData = new List<Data>();
        Queue<StackableImage> mImagesCache = new Queue<StackableImage>();
        int mImagesCacheInitialCount = 0;

        public void PushBack(StackableImage img)
        {
            mImagesCache.Enqueue(img);
        }

        public StackableImage GetNext()
        {
            while (true)
            {
            	if (mImagesCache.Count <= mImagesCacheInitialCount / 10 || mImagesCacheInitialCount < 10)
                {
                    foreach (Data data in mData)
                        data.mImagesLoadedEvent.Set();
                }
                if (mImagesCache.Count > 0)
                    return mImagesCache.Dequeue();

                bool haveWorkingLoaders = false;
                foreach (Data data in mData)
                {
                    var images = data.GetImages();
                    if (images.Count == 0 && data.IsDone)
                        continue;
                    while (images.Count > 0)
                        mImagesCache.Enqueue(images.Dequeue());
                    haveWorkingLoaders = true;
                }
                mImagesCacheInitialCount = mImagesCache.Count;
                if (mImagesCache.Count > 0)
                    return mImagesCache.Dequeue();
                if (haveWorkingLoaders)
                {
                    Thread.Sleep(100);
                    continue;
                }
                return null;
            }
        }

        bool mDisposing = false;
        public void Dispose()
        {
            if (mDisposing)
                return;
            mDisposing = true;
            foreach (Thread thread in mPreloader)
                thread.Abort();
        }
    }
}
