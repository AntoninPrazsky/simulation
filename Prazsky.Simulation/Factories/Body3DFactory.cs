﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prazsky.Render;
using tainicom.Aether.Physics2D.Dynamics;

namespace Prazsky.Simulation.Factories
{
	/// <summary>
	/// Vytváří objekty typu <see cref="Body3D"/> na základě zadaných parametrů.
	/// </summary>
	public static class Body3DFactory
	{
		/// <summary>
		/// Výchozí typ tělesa.
		/// </summary>
		public const BodyType DEFAULT_BODY_TYPE = BodyType.Dynamic;

		/// <summary>
		/// Vrátí objekt typu <see cref="Body3D"/>. Tvar odpovídající dvourozměrné reprezentaci pro fyzikální simulaci
		/// je nalezen podle ortogonální projekce zadaného trojrozměrného modelu.
		/// </summary>
		/// <param name="model">Trojrozměrný model.</param>
		/// <param name="world2D">Dvourozměrný svět, do kterého má být těleso zařazeno.</param>
		/// <param name="graphicsDevice">Grafické zařízení.</param>
		/// <param name="position">Výchozí pozice objektu v dvourozměrném světě.</param>
		/// <param name="bodyType">Typ simulovaného tělesa (statické, kinematické nebo dynamické).</param>
		/// <param name="basicEffectParams">Parametry pro třídu <see cref="BasicEffect"/>.</param>
		/// <returns>Objekt typu <see cref="Body3D"/>.</returns>
		public static Body3D CreateBody3D(
				Model model,
				World world2D,
				GraphicsDevice graphicsDevice,
				Vector2 position = new Vector2(),
				BodyType bodyType = DEFAULT_BODY_TYPE,
				BasicEffectParams basicEffectParams = null)
		{
			using (Texture2D orthoRender = BitmapRenderer.RenderOrthographic(graphicsDevice, model))
			{
				Body body2D = BodyCreator.CreatePolygonBody(orthoRender, world2D, position, bodyType);

				Body3D body3D = new Body3D(model, body2D)
				{
					BasicEffectParams = basicEffectParams
				};

				return body3D;
			}
		}

		/// <summary>
		/// Vrátí objekt typu <see cref="Body3D"/>. Tvar pro dvourozměrnou fyzikální simulaci je dán parametrem body.
		/// </summary>
		/// <param name="model">Trojrozměrný model.</param>
		/// <param name="world2D">Dvourozměrný svět, do kterého má být těleso zařazeno.</param>
		/// <param name="body">Dvourozměrné těleso.</param>
		/// <param name="position">Výchozí pozice objektu v dvourozměrném světě.</param>
		/// <param name="bodyType">Typ simulovaného tělesa (statické, kinematické nebo dynamické).</param>
		/// <param name="basicEffectParams">Parametry pro třídu <see cref="BasicEffect"/>.</param>
		/// <returns>Objekt typu <see cref="Body3D"/>.</returns>
		public static Body3D CreateBody3D(
				Model model,
				World world2D,
				Body body,
				Vector2 position = new Vector2(),
				BodyType bodyType = DEFAULT_BODY_TYPE,
				BasicEffectParams basicEffectParams = null)
		{
			body.Position = position;
			body.BodyType = bodyType;
			world2D.Add(body);

			Body3D body3D = new Body3D(model, body)
			{
				BasicEffectParams = basicEffectParams
			};

			return body3D;
		}
	}
}