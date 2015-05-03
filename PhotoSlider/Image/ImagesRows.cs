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

using System.Collections.Generic;

namespace PhotoSlider
{
    class ImagesRows : IRowItem
    {
        List<IRowItem> mRows = new List<IRowItem>();

        public IRowItem this[int index]
        {
            get { return mRows[index]; }
        }

        public int RowCount
        {
            get { return mRows.Count; }
        }

        public void Add(IRowItem row)
        {
            mRows.Add(row);
        }

        public void Insert(IRowItem item, int position)
        {
            mRows.Insert(position, item);
        }

        public void Remove(IRowItem item)
        {
            mRows.Remove(item);
        }

        public int Width
        {
            get 
            {
                int width = 0;
                for (int i = 0; i < mRows.Count; ++i)
                {
                    int rowWidth = mRows[i].Width;
                    if (width < rowWidth)
                        width = rowWidth;
                }

                return width;
            }
        }

        public int Height
        {
            get
            {
                int height = 0;
                for (int i = 0; i < mRows.Count; ++i)
                {
                    height += mRows[i].Height;
                }

                return height;
            }
        }

        public RowType GetItemType()
        {
            return RowType.Rows;
        }

        public void Dispose()
        {
            foreach (IRowItem row in mRows)
                row.Dispose();
        }
    }
}
