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
using System.Windows.Forms;

namespace PhotoSlider
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Options options = new Options();
            options.Load(args);

            if (options.PrintHelp)
            {
                // TODO: doesn't work for now
                Console.WriteLine("Use like this: PhotoSlider.exe [options]");
                Console.WriteLine("Available options (all options are optional):");
                Console.WriteLine("  --help");
                Console.WriteLine("  --rows [1-50]");
                Console.WriteLine("  --cols [1-50]");
                Console.WriteLine("  as | run on all screens");
                Console.WriteLine("  --movepath [path]");
                Console.WriteLine("  --movedeleted [path]");
                return;
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ScreensController controller = new ScreensController(options);
            Application.Run();
            controller.Dispose();
        }
    }
}
