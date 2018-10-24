using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Prazsky.Simulation;
using System.IO;

namespace Prazsky.Render
{
    public static class BitmapCreator
    {
        private const int BITMAP_SCALE = 100;

        private const int MAX_BITMAP_WIDTH = 4096;
        private const int MAX_BITMAP_HEIGHT = 4096;

        //Tyto hodnoty by bylo možné individuálně spočítat pro každý model, ale pro ortogonální projekci kolmou k ose Z (ke kameře) to nemá smysl
        private const float CAMERA_Z_POSITION = 10f;
        private const float Z_NEAR_PLANE = 1f;
        private const float Z_FAR_PLANE = 100f;

        /// <summary>
        /// Vrátí ortogonální projekci modelu v podobě bitmapy (<see cref="Texture2D"/>).
        /// </summary>
        /// <param name="graphicsDevice">Grafické zařízení, které má být použito k vykreslení modelu.</param>
        /// <param name="model">Trojrozměný model k vykreslení.</param>
        /// <param name="bitmapScale">Poměr modelu vůči jeho bitmapové reprezentaci. Výchozí hodnota 100 znamená, že 1 jednotka modelu odpovídá 100 pixelům bitmapy.</param>
        /// <returns>Vrací ortogonální projekci trojrozměného modelu v podobě bitmapy typu <see cref="Texture2D"/>.</returns>
        public static Texture2D CreateBitmap(GraphicsDevice graphicsDevice, Model model, int bitmapScale = BITMAP_SCALE)
        {
            SizeFloat modelSize = CalculateModelSize(model);
            SizeInt renderSize = CalculateBitmapSize(modelSize, bitmapScale);

            RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, renderSize.X, renderSize.Y, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth16);

            Matrix world = Matrix.Identity;
            Matrix view = Matrix.CreateLookAt(new Vector3(0f, 0f, CAMERA_Z_POSITION), Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreateOrthographic(modelSize.X, modelSize.Y, Z_NEAR_PLANE, Z_FAR_PLANE);

            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            graphicsDevice.Clear(Color.Transparent);
            model.Draw(world, view, projection);

            graphicsDevice.SetRenderTarget(null);

            return renderTarget;
        }

        /// <summary>
        /// Vykreslí ortogonální projekci modelu využitím metody <see cref="CreateBitmap(GraphicsDevice, Model, int)"/> a zapíše ji do souboru ve formátu PNG.
        /// </summary>
        /// <param name="graphicsDevice">Grafické zařízení, které má být použito k vykreslení modelu.</param>
        /// <param name="model">Trojrozměný model k vykreslení.</param>
        /// <param name="filePath">Kompletní cesta k finálnímu souboru (například "C:\image.png" pro systém Windows).</param>
        /// <param name="bitmapScale">Poměr modelu vůči jeho bitmapové reprezentaci. Výchozí hodnota 100 znamená, že 1 jednotka modelu odpovídá 100 pixelům bitmapy.</param>
        public static void RenderBitmapAsPNG(GraphicsDevice graphicsDevice, Model model, string filePath, int bitmapScale = BITMAP_SCALE)
        {
            Texture2D bitmap = CreateBitmap(graphicsDevice, model, bitmapScale);
            Stream stream = File.Create(filePath);
            bitmap.SaveAsPng(stream, bitmap.Width, bitmap.Height);
            stream.Dispose();
            bitmap.Dispose();
        }

        private static SizeFloat CalculateModelSize(Model model)
        {
            BoundingBox box = Geometry.GetBoundingBox(model);

            SizeFloat calculatedSize;
            
            calculatedSize.X = box.GetCorners()[2].X * 2;
            calculatedSize.Y = Math.Abs(box.GetCorners()[2].Y) * 2;

            if (calculatedSize.X <= 0 || calculatedSize.Y <= 0)
                throw new ArgumentException("Chyba při výpočtu velikosti modelu. Zkontrolujte model (jeho výška a šířka musí být větší než 0).", nameof(model));

            return calculatedSize;
        }

        private static SizeInt CalculateBitmapSize(SizeFloat modelSize, int bitmapScale)
        {
            SizeInt calculatedSize;

            calculatedSize.X = (int)(modelSize.X * bitmapScale);
            calculatedSize.Y = (int)(modelSize.Y * bitmapScale);

            if (calculatedSize.X > MAX_BITMAP_WIDTH)
                throw new ArgumentException(
                    string.Format("Chyba při vytváření bitmapového podkladu. Šířka výsledného bitmapového podkladu ({0}) překračuje maximální povolenou hodnotu ({1}). Použijte menší model nebo snižte hodnotu parametru {2} ({3}).",
                    calculatedSize.X,
                    MAX_BITMAP_WIDTH,
                    nameof(bitmapScale),
                    bitmapScale));

            if (calculatedSize.Y > MAX_BITMAP_HEIGHT)
                throw new ArgumentException(
                    string.Format("Chyba při vytváření bitmapového podkladu. Výška výsledného bitmapového podkladu ({0}) překračuje maximální povolenou hodnotu ({1}). Použijte menší model nebo snižte hodnotu parametru {2} ({3}).",
                    calculatedSize.Y,
                    MAX_BITMAP_WIDTH,
                    nameof(bitmapScale),
                    bitmapScale));

            return calculatedSize;
        }

        private struct SizeFloat
        {
            public float X;
            public float Y;
        }

        private struct SizeInt
        {
            public int X;
            public int Y;
        }

    }
}