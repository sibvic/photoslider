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
using PhotoSlider.Image;

namespace PhotoSlider
{
    /// <summary>
    /// Loads images with specified resolution.
    /// </summary>
    class ImageLoader
    {
        private int mWidth;
        private int mHeight;
        public ImageLoader(int width, int height)
        {
            mWidth = width;
            mHeight = height;
        }
        
        public SliderImage Load(string path)
        {
            var decoder = System.Windows.Media.Imaging.BitmapDecoder.Create(new Uri(path), System.Windows.Media.Imaging.BitmapCreateOptions.None,
                System.Windows.Media.Imaging.BitmapCacheOption.None);
            var frame = decoder.Frames[0];
            int targetWidth;
            int targetHeight;
            if (frame.PixelWidth > mWidth || frame.PixelHeight > mHeight)
            {
                double wrate = (double)frame.PixelWidth / (double)mWidth;
                double hrate = (double)frame.PixelHeight / (double)mHeight;
                if (wrate > hrate)
                {
                    targetWidth = mWidth;
                    targetHeight = (int)Math.Round(((double)mWidth * (double)frame.PixelHeight / (double)frame.PixelWidth));
                }
                else
                {
                    targetHeight = mHeight;
                    targetWidth = (int)Math.Round(((double)mHeight * (double)frame.PixelWidth / (double)frame.PixelHeight));
                }
                return new SliderImage(targetWidth, targetHeight, path);
            }
            return new SliderImage(frame.PixelWidth, frame.PixelHeight, path);            
        }
    }
}
