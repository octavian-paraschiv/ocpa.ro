using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web;

namespace Meteo.Helpers
{
    public class GriddedMap
    {
        public int Rows { get; private set; }
        public int Cols { get; private set; }

        string _viewport = null;

        public GriddedMap(string viewport)
        {
            SetViewport(viewport);
        }

        private void SetViewport(string viewport)
        {
            _viewport = viewport;
            switch (_viewport)
            {
                case "Europe":
                    Rows = 65;
                    Cols = 96;
                    break;

                default:
                case "Romania":
                    Rows = 13;
                    Cols = 21;
                    break;
            }
        }

        public string DrawHighlightedArea(int r, int c)
        {
            Bitmap bmp = Bitmap.FromFile(Path.Combine(AppFolders.StaticImagesFolder, $"{_viewport}.PNG")) as Bitmap;

            string bmpFile = string.Format("{2}_grid_{0}_{1}.png", r, c, _viewport);
            string bmpFilePath = string.Format("{0}\\{1}", AppFolders.DynamicImagesFolder, bmpFile);

            int dx = bmp.Width / Cols;
            int dy = bmp.Height / Rows;

            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen pGrid = new Pen(Color.LightGray, 1))
            using (Pen pHilight = new Pen(Color.Red, 3))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.CompositingMode = CompositingMode.SourceCopy;

                for (int x = 0; x <= Cols; x++)
                    g.DrawLine(pGrid, x * dx, 0, x * dx, Rows * dy);

                for (int y = 0; y <= Rows; y++)
                    g.DrawLine(pGrid, 0, y * dy, Cols * dx, y * dy);

                if (r >= 0 && c >= 0)
                    g.DrawRectangle(pHilight, c * dx, r * dy, dx, dy);

                bmp.Save(bmpFilePath);
                return bmpFile;
            }
        }
    }
}