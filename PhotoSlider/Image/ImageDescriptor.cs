//
//  Photo Slider
//  Copyright (C) 2012 Victor Tereschenko (aka sibvic)
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
    /// <summary>
    /// Provides path, original and scaled images.
    /// </summary>
    public class ImageDescriptor : IDisposable
    {
        public ImageDescriptor(string fileName, Image original, Image scaled)
        {
            Path = fileName;
            Original = original;
            Scaled = scaled;
        }

        public string Path { get; private set; }
        public Image Original { get; private set; }
        public Image Scaled { get; private set; }
        
        public void Dispose()
        {
            Original.Dispose();
            if (Original != Scaled)
                Scaled.Dispose();
        }
    }
}
