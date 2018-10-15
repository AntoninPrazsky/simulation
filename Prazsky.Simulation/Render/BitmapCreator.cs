using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Prazsky.Simulation;

namespace Prazsky.Render
{
    public static class BitmapCreator
    {
        private const int BITMAP_SCALE = 100;
        private const int MAX_BITMAP_WIDTH = 4096;
        private const int MAX_BITMAP_HEIGHT = 4096;

        /// <summary>
        /// Vrátí ortogonální projekci modelu v podobě bitmapy
        /// </summary>
        /// <param name="graphicsDevice">Grafické zařízení, které má být použito k vykreslení modelu</param>
        /// <param name="model">Trojrozměný model k vykreslení</param>
        /// <param name="bitmapsScale">Poměr modelu vůči jeho bitmapové reprezentaci. Výchozí hodnota 100 znamená, že 1 jednotka modelu odpovídá 100 pixelům bitmapy.</param>
        /// <returns>Vrací ortogonální projekci trojrozměného modelu v podobě bitmapy typu Texture2D</returns>
        public static Texture2D CreateBitmap(GraphicsDevice graphicsDevice, Model model, int bitmapsScale = BITMAP_SCALE)
        {
            #region Velikost modelu

            BoundingBox box = Geometry.GetBoundingBox(model);

            float modelWidth = box.GetCorners()[2].X * 2;
            float modelHeight = Math.Abs(box.GetCorners()[2].Y) * 2;

            if (modelWidth <= 0)
                throw new ArgumentException("Chyba při výpočtu šířky modelu.", nameof(model)); //TODO

            #endregion Velikost modelu

            #region Velikost bitmapy

            int renderWidth = (int)(modelWidth * bitmapsScale);
            int renderHeight = (int)(modelHeight * bitmapsScale);

            if (renderWidth > MAX_BITMAP_WIDTH)
                throw new ArgumentException(String.Format("Chyba při vytváření bitmapového podkladu. Šířka výsledného bitmapového podkladu ({0}) překračuje maximální povolenou šířku ({1}). Použijte menší model nebo snižte parametr bitmapScale ({2}).", renderWidth, MAX_BITMAP_WIDTH, bitmapsScale));

            if (renderHeight > MAX_BITMAP_HEIGHT)
                throw new ArgumentException(String.Format("Chyba při vytváření bitmapového podkladu. Výška výsledného bitmapového podkladu ({0}) překračuje maximální povolenou výšku ({1}). Použijte menší model nebo snižte parametr bitmapScale ({2}).", renderWidth, MAX_BITMAP_WIDTH, bitmapsScale));

            RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, renderWidth, renderHeight, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth16);

            #endregion Velikost bitmapy

            #region Matice pro vykreslení modelu

            Matrix world = Matrix.Identity;
            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10f), Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreateOrthographic(modelWidth, modelHeight, 1f, 100f);

            Matrix[] transformation = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transformation);

            #endregion Matice pro vykreslení modelu

            #region Vykreslení modelu

            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            graphicsDevice.Clear(Color.Transparent);
            model.Draw(world, view, projection);

            graphicsDevice.SetRenderTarget(null);

            #endregion Vykreslení modelu

            return renderTarget;
        }


    }
}