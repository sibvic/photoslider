//
//  Photo Slider
//  Copyright (C) 2015 Victor Tereschenko (aka sibvic)
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

namespace PhotoSlider.Image
{
    public class SliderImage : IDisposable
    {
        public SliderImage(int width, int height, string path)
        {
            Width = width;
            Height = height;
            mPath = path;
        }
        string mPath;
        System.Drawing.Image mImage;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public System.Drawing.Image GetImage()
        {
            if (mImage != null)
                return mImage;
            try
            {
                System.Drawing.Image original = System.Drawing.Image.FromFile(mPath);
                if (original.Width > Width || original.Height > Height)
                {
                    try
                    {
                        mImage = new System.Drawing.Bitmap(Width, Height);
                        using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(mImage))
                        {
                            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphics.DrawImage(original, 0, 0, Width, Height);
                        }
                    }
                    finally
                    {
                        original.Dispose();
                    }
                    return mImage;
                }
                else
                {
                    mImage = original;
                    return mImage;
                }
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

        public void Dispose()
        {
            if (mImage != null)
            {
                mImage.Dispose();
                mImage = null;
            }
        }
    }
}
