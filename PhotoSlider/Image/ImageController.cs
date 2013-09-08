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
using System.Linq;
using System.Text;
using System.Drawing;

namespace PhotoSlider
{
    public class ImageController : IDisposable
    {
        public ImageController(string fileName)
        {
            mFileName = fileName;
        }

        struct ImageDesc
        {
            public int Width;
            public int Height;
            public Image Image;
        };
        private List<ImageDesc> mImages = new List<ImageDesc>();

        string mFileName;
        Image mImage;
        Image Image
        {
            get
            {
                if (mImage == null)
                {
                    try
                    {
                        mImage = Image.FromFile(mFileName);
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
                    catch (System.OutOfMemoryException)
                    {
                        return null;
                    }
                    if (mImage == null)
                        return null;

                    ImageDesc desc = new ImageDesc();
                    desc.Height = mImage.Height;
                    desc.Width = mImage.Width;
                    desc.Image = mImage;
                }
                
                return mImage;
            }
        }

        public Image TryGet(int width, int height)
        {
            for (int i = 0; i < mImages.Count; ++i)
            {
                if (mImages[i].Height == height && mImages[i].Width == width)
                    return mImages[i].Image;
            }
            return null;
        }

        public Image Get(int width, int height)
        {
            lock (mImages)
            {
                Image img = TryGet(width, height);
                if (img != null)
                    return img;
                return addImage(width, height);
            }
        }

        Image addImage(int width, int height)
        {
            if (Image == null)
                return null;
            Image img;
            try
            {
                img = ImageScaler.ScaleImage(width, height, Image);
            }
            catch (OutOfMemoryException)
            {
                return null;
            }
            if (img == null)
                return null;

            if (img == Image)
                return null;

            ImageDesc desc = new ImageDesc();
            desc.Width = width;
            desc.Height = height;
            desc.Image = img;
            mImages.Add(desc);
            mImage.Dispose();
            mImage = null;
            return img;

        }

        bool mDisposed = false;
        public void Dispose()
        {
            if (mDisposed)
                return;

            mDisposed = true;
            foreach (ImageDesc image in mImages)
                image.Image.Dispose();
            mImage.Dispose();
            mImage = null;
        }
    }
}
