using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Demo
{
	/// <summary>
	/// Vykresluje snímkovou frekvenci grafiky a další informace v podobě textu.
	/// </summary>
	public class Info : DrawableGameComponent
	{
		private ContentManager _content;
		private SpriteBatch _spriteBatch;
		private SpriteFont _font;

		private int _frameRate = 0;
		private int _frameCounter = 0;
		private TimeSpan _elapsedTime = TimeSpan.Zero;

		private Vector2 _fpsPosition = new Vector2(20f, 10f);
		private Vector2 _gpuPosition = new Vector2(180f, 12f);
		private Vector2 _controlHelpPositionKeys = new Vector2(20f, 60f);
		private Vector2 _controlHelpPositionText = new Vector2(180f, 60f);
		private string _fps;
		private string _gpu;

		private string _controlHelpKeys =
			"Right arrow:\n" +
			"Left arrow:\n" +
			"W:\n" +
			"S:\n" +
			"A:\n" +
			"D:\n" +
			"Q:\n" +
			"E:\n" +
			"Left mouse button:\n" +
			"Right mouse button:\n" +
			"Shift:\n" +
			"F12:";

		private string _controlHelpText =
			"Next scene\n" +
			"Previous scene\n" +
			"Move forward\n" +
			"Move backward\n" +
			"Move left\n" +
			"Move right\n" +
			"Move down\n" +
			"Move up\n" +
			"Grab an object\n" +
			"Free look\n" +
			"Fast movement\n" +
			"Show/hide this text";

		/// <summary>
		/// Konstruktor třídy Info pro vykreslování snímkové frekvence zobrazení a dalších informací.
		/// </summary>
		/// <param name="game">Hra, ve které se mají informace vykreslovat.</param>
		public Info(Game game) : base(game)
		{
			_content = new ContentManager(game.Services);
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_font = _content.Load<SpriteFont>("Content/Fonts/Aileron");

			_gpu = "GPU: " + Game.GraphicsDevice.Adapter.Description;
		}

		protected override void UnloadContent()
		{
			_content.Unload();
		}

		/// <summary>
		/// Aktualizace snímkové frekvence vykreslování grafiky.
		/// </summary>
		/// <param name="gameTime">Herní čas.</param>
		public override void Update(GameTime gameTime)
		{
			_elapsedTime += gameTime.ElapsedGameTime;

			if (_elapsedTime > TimeSpan.FromSeconds(1))
			{
				_elapsedTime -= TimeSpan.FromSeconds(1);
				_frameRate = _frameCounter;
				_frameCounter = 0;
			}

			_fps = "FPS: " + _frameRate.ToString();
		}

		/// <summary>
		/// Vykreslení snímkové frekvence a dalších informací.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			_frameCounter++;

			_spriteBatch.Begin();

			RenderText(_fps, _fpsPosition);
			RenderTextScale(_gpu, _gpuPosition, 0.75f);
			RenderHelp();

			_spriteBatch.End();
		}

		private void RenderText(string text, Vector2 position)
		{
			_spriteBatch.DrawString(_font, text, new Vector2(position.X + 2, position.Y + 2), Color.Black);
			_spriteBatch.DrawString(_font, text, position, Color.White);
		}

		private void RenderTextScale(string text, Vector2 position, float scale)
		{
			_spriteBatch.DrawString(
				_font,
				text,
				new Vector2(position.X + 1, position.Y + 1),
				Color.Black,
				0f,
				Vector2.Zero,
				scale,
				SpriteEffects.None,
				0f);

			_spriteBatch.DrawString(
				_font,
				text,
				position,
				Color.White,
				0f,
				Vector2.Zero,
				scale,
				SpriteEffects.None,
				0f);
		}

		private void RenderHelp()
		{
			RenderTextScale(_controlHelpKeys, _controlHelpPositionKeys, 0.5f);
			RenderTextScale(_controlHelpText, _controlHelpPositionText, 0.5f);
		}
	}
}