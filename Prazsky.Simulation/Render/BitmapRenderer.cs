﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Tools;
using System;
using System.IO;

namespace Prazsky.Render
{
    /// <summary>
    /// Poskytuje metody pro vykreslení ortogonální projekce trojrozměrného modelu.
    /// Výsledek může být (<see cref="Texture2D"/>) nebo export do souboru ve formátu PNG.
    /// </summary>
    public static class BitmapRenderer
    {
        private const int DEFAULT_BITMAP_SCALE = 100;

        //Tyto hodnoty jsou dány možnostmi použitého grafického zařízení (a platformou, na které kód běží), zjistit,
        //jestli je lze jednoduše zjistit z této třídy
        private const int MAX_BITMAP_WIDTH = 4096;
        private const int MAX_BITMAP_HEIGHT = 4096;

        //Tyto hodnoty by bylo možné individuálně spočítat pro každý model (jde o ortogonální projekci kolmou k
        //ose Z - ke kameře), promyslet, jestli by to mělo smysl
        //Výpočet na základě AABB modelu: Zadní ořezová plocha je zadní strana kvádru, přední ořezová plocha
        //je přední strana kvádru, přičemž pozice kamery odpovídá vzdálenosti přední strany kvádru od počátku
        private const float CAMERA_Z_POSITION = 10f;
        private const float Z_NEAR_PLANE = 1f;
        private const float Z_FAR_PLANE = 100f;

        /// <summary>
        /// Vrátí ortogonální projekci modelu v podobě bitmapy (<see cref="Texture2D"/>).
        /// </summary>
        /// <param name="graphicsDevice">Grafické zařízení, které má být použito k vykreslení modelu.</param>
        /// <param name="model">Trojrozměný model k vykreslení.</param>
        /// <param name="bitmapScale">Poměr modelu vůči jeho bitmapové reprezentaci. Výchozí hodnota 100 znamená,
        /// že 1 jednotka modelu odpovídá 100 pixelům bitmapy.</param>
        /// <returns>Vrací ortogonální projekci trojrozměného modelu v podobě bitmapy.</returns>
        public static Texture2D RenderOrthographic(
            GraphicsDevice graphicsDevice,
            Model model,
            int bitmapScale = DEFAULT_BITMAP_SCALE)
        {
            SizeFloat modelSize = CalculateModelSize(model);
            SizeInt renderSize = CalculateBitmapSize(modelSize, bitmapScale);

            RenderTarget2D renderTarget = new RenderTarget2D(
                graphicsDevice,
                renderSize.X,
                renderSize.Y,
                false,
                graphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth16);

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
        /// Vykreslí ortogonální projekci modelu využitím metody 
        /// <see cref="RenderOrthographic(GraphicsDevice, Model, int)"/> a zapíše ji do souboru ve formátu PNG.
        /// </summary>
        /// <param name="graphicsDevice">Grafické zařízení, které má být použito k vykreslení modelu.</param>
        /// <param name="model">Trojrozměný model k vykreslení.</param>
        /// <param name="filePath">Kompletní cesta k finálnímu souboru
        /// (například "C:\image.png" pro systém Windows).</param>
        /// <param name="bitmapScale">Poměr modelu vůči jeho bitmapové reprezentaci.
        /// Výchozí hodnota 100 znamená, že 1 jednotka modelu odpovídá 100 pixelům bitmapy.</param>
        public static void RenderOrthographicAsPNG(
            GraphicsDevice graphicsDevice,
            Model model,
            string filePath,
            int bitmapScale = DEFAULT_BITMAP_SCALE)
        {
            Texture2D bitmap = RenderOrthographic(graphicsDevice, model, bitmapScale);
            Stream stream = File.Create(filePath);
            bitmap.SaveAsPng(stream, bitmap.Width, bitmap.Height);
            stream.Dispose();
            bitmap.Dispose();
        }

        private static SizeFloat CalculateModelSize(Model model)
        {
            BoundingBox box = Geometry.GetBoundingBox(model);

            SizeFloat calculatedSize;

            Vector3[] boxCorners = box.GetCorners();
            calculatedSize.X = boxCorners[2].X * 2;
            calculatedSize.Y = Math.Abs(boxCorners[2].Y) * 2;

            if (calculatedSize.X <= 0 || calculatedSize.Y <= 0)
                throw new ArgumentException("Chyba při výpočtu velikosti modelu. Zkontrolujte model (jeho výška a " +
                    "šířka musí být větší než 0).", nameof(model));

            return calculatedSize;
        }

        private static SizeInt CalculateBitmapSize(SizeFloat modelSize, int bitmapScale)
        {
            SizeInt calculatedSize;

            calculatedSize.X = (int)(modelSize.X * bitmapScale);
            calculatedSize.Y = (int)(modelSize.Y * bitmapScale);

            if (calculatedSize.X > MAX_BITMAP_WIDTH)
                throw new ArgumentException(
                    string.Format("Chyba při vytváření bitmapového podkladu. Šířka výsledného bitmapového podkladu " +
                    "({0}) překračuje maximální povolenou hodnotu ({1}). Použijte menší model nebo snižte hodnotu " +
                    "parametru {2} ({3}).",
                    calculatedSize.X,
                    MAX_BITMAP_WIDTH,
                    nameof(bitmapScale),
                    bitmapScale));

            if (calculatedSize.Y > MAX_BITMAP_HEIGHT)
                throw new ArgumentException(
                    string.Format("Chyba při vytváření bitmapového podkladu. Výška výsledného bitmapového podkladu " +
                    "({0}) překračuje maximální povolenou hodnotu ({1}). Použijte menší model nebo snižte hodnotu " +
                    "parametru {2} ({3}).",
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