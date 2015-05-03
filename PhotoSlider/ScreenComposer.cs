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
using System.Threading;

namespace PhotoSlider
{
    class ScreenComposer : IDisposable
    {
        public ScreenComposer(ImagesScaner imageScaner, System.Drawing.Size layoutSize, System.Drawing.Size imagesSize)
        {
            mImages = new ImagesLoaderJob(imageScaner, imagesSize.Width, imagesSize.Height);
            mLayoutSize = layoutSize;
            mImagesScaner = imageScaner;
            mThread = new Thread(new ThreadStart(doWork));
            mThread.Start();
        }
        ImagesLoaderJob mImages;
        Thread mThread;
        bool mWorkDone = false;
        bool mCanceled = false;
        System.Drawing.Size mLayoutSize;
        List<ImagesLayout> mLayouts = new List<ImagesLayout>();
        ManualResetEvent mLayoutCreated = new ManualResetEvent(false);
        ImagesScaner mImagesScaner;

        /// <summary>
        /// Stops layout generation.
        /// </summary>
        public void Stop()
        {
            mCanceled = true;
            mImages.Stop();
            var image = mImages.GetNext();
            while (image != null)
            {
                mImagesScaner.Return(image.Path);
                image.Dispose();
                image = mImages.GetNext();
            }
            mThread.Join();
        }

        /// <summary>
        /// Gets next layout.
        /// </summary>
        /// <returns>May return null if there is no more images to form layouts.</returns>
        public ImagesLayout GetNextLayout()
        {
            lock (mLayouts)
            {
                if (mLayouts.Count > 0)
                {
                    var result = mLayouts[0];
                    mLayouts.RemoveAt(0);
                    return result;
                }
                mLayoutCreated.Reset();
            }

            if (mWorkDone)
                return null;

            mLayoutCreated.WaitOne();

            if (mWorkDone)
                return null;

            lock (mLayouts)
            {
                var result = mLayouts[0];
                mLayouts.RemoveAt(0);
                return result;
            }
        }

        /// <summary>
        /// Does composition work
        /// </summary>
        void doWork()
        {
            while (!mCanceled)
            {
                StackableImage image = mImages.GetNext();
                if (image == null)
                    break;

                lock (mLayouts)
                {
                    if (!addImage(image))
                    {
                        ImagesLayout layout = new ImagesLayout(mLayoutSize.Width, mLayoutSize.Height);
                        mLayouts.Add(layout);
                        if (!layout.Add(image))
                        {
                            // terrible error! Shouldn't happen!
                            image.Dispose();
                        }
                        mLayoutCreated.Set();
                    }
                }
            }
            mWorkDone = true;
            mLayoutCreated.Set();
        }

        /// <summary>
        /// Tries to add an image to one of the existing layouts.
        /// The caller need to lock mLayouts.
        /// </summary>
        /// <param name="image"></param>
        /// <returns>True if the image was added to the layout.</returns>
        bool addImage(StackableImage image)
        {
            foreach (ImagesLayout layout in mLayouts)
            {
                if (layout.Add(image))
                    return true;
            }
            return false;
        }

        bool mDisposed = false;
        public void Dispose()
        {
            if (mDisposed)
                return;
            mDisposed = true;
            Stop();
            mImages.Dispose();
            lock (mLayouts)
            {
                foreach (ImagesLayout layout in mLayouts)
                {
                    layout.Dispose();
                }
            }
        }
    }
}
