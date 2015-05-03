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
using System.Windows.Forms;

namespace PhotoSlider
{
    class ScreensController : IDisposable, IApplicationController
    {
        public ScreensController(Options options)
        {
            mImagesLoader = new ImagesScaner();

            if (!options.RunOnAllScreens)
            {
                Form1 form = new Form1(this, mImagesLoader, Screen.PrimaryScreen, options);
                form.Show();
                form.Location = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Left, Screen.PrimaryScreen.Bounds.Top);
                mForms.Add(form);
            }
            else
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    Form1 form = new Form1(this, mImagesLoader, screen, options);
                    form.Show();
                    form.Location = new System.Drawing.Point(screen.Bounds.Left, screen.Bounds.Top);
                    mForms.Add(form);
                }
            }
        }
        ImagesScaner mImagesLoader;

        public void Exit()
        {
            foreach (Form1 form in mForms)
            {
                form.Stop();
                form.Close();
            }
            Application.Exit();
        }

        public void Delete()
        {
            foreach (Form1 form in mForms)
                form.updateImages(Form1.ImageAction.Delete);
        }

        public void Skip()
        {
            foreach (Form1 form in mForms)
                form.updateImages(Form1.ImageAction.Skip);
        }

        public void Move()
        {
            foreach (Form1 form in mForms)
                form.updateImages(Form1.ImageAction.Move);
        }

        public void AddVertical()
        {
            ++mRows;
            foreach (Form1 form in mForms)
                form.showPicturesBoxes(mColumns, mRows);
        }

        public void RemoveVertical()
        {
            if (mRows == 1)
                return;

            --mRows;
            foreach (Form1 form in mForms)
                form.showPicturesBoxes(mColumns, mRows);
        }

        public void AddHorizontal()
        {
            ++mColumns;
            foreach (Form1 form in mForms)
                form.showPicturesBoxes(mColumns, mRows);
        }

        public void RemoveHorizontal()
        {
            if (mColumns == 1)
                return;

            --mColumns;
            foreach (Form1 form in mForms)
                form.showPicturesBoxes(mColumns, mRows);
        }

        int mColumns = 1;
        int mRows = 1;
        List<Form1> mForms = new List<Form1>();

        bool mDisposed = false;
        public void Dispose()
        {
            if (mDisposed)
                return;
            mDisposed = true;
            mImagesLoader.Dispose();
        }
    }
}
