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
using System.Threading;

namespace PhotoSlider
{
    /// <summary>
    /// Scans current directory for images.
    /// </summary>
    public class ImagesScaner : IDisposable
    {
        public ImagesScaner()
        {
            mScanner = new Thread(new ParameterizedThreadStart(scanFolderJob));
            mScanner.Start(System.IO.Directory.GetCurrentDirectory());
        }

        void scanFolder(string path)
        {
            try
            {
                foreach (string folder in System.IO.Directory.GetDirectories(path))
                {
                    scanFolder(folder);
                }
                string[] extensions = new string[] { "*.jpg", "*.jpe", "*.png", "*.gif", "*.bmp" };
                foreach (string extension in extensions)
                {
                    foreach (string file in System.IO.Directory.GetFiles(path, extension))
                    {
                        mImages.Enqueue(file);
                        mEvent.Set();
                    }
                }
            }
            catch (System.IO.PathTooLongException)
            {
            }
        }

        void scanFolderJob(object path)
        {
            scanFolder((string)path);
            mScanned = true;
        }

        bool mScanned = false;
        Thread mScanner;
        ManualResetEvent mEvent = new ManualResetEvent(true);
        ConcurrentQueue<string> mImages = new ConcurrentQueue<string>();
        public string Next()
        {
            string img = null;
            while (!mImages.TryDequeue(out img) && !mScanned)
            {
                mEvent.WaitOne();
                mEvent.Reset();
            }
            return img;
        }

        public void Return(string img)
        {
            mImages.Enqueue(img);
        }

        bool mDisposed;
        public void Dispose()
        {
            if (mDisposed)
                return;

            mDisposed = true;
            mScanner.Abort();
        }
    }
}
