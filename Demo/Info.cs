using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Demo
{
	public class Info : DrawableGameComponent
	{
		private ContentManager _content;
		private SpriteBatch _spriteBatch;
		private SpriteFont _font;

		private int _frameRate = 0;
		private int _frameCounter = 0;
		private TimeSpan _elapsedTime = TimeSpan.Zero;

		private Vector2 _fpsPosition = new Vector2(20f, 10f);
		private Vector2 _gpuPosition = new Vector2(200f, 12f);
		private Vector2 _controlHelpPosition = new Vector2(20f, 60f);
		private string _fps;
		private string _gpu;

		public Info(Game game) : base(game)
		{
			_content = new ContentManager(game.Services);
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_font = _content.Load<SpriteFont>("Content/Fonts/Aileron");

			_gpu = Game.GraphicsDevice.Adapter.Description;
		}

		protected override void UnloadContent()
		{
			_content.Unload();
		}

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
			_spriteBatch.DrawString(_font, text, new Vector2(position.X + 1, position.Y + 1), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			_spriteBatch.DrawString(_font, text, position, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		private string _controlHelp = 
			"Right arrow: Next scene\n" + 
			"Left arrow: Previous scene\n" +
			"W: Move forward\n" +
			"S: Move backward\n" +
			"A: Move left\n" +
			"D: Move right\n" +
			"Q: Move down\n" +
			"E: Move up\n" +
			"Left mouse button: Grab an object\n" +
			"Right mouse button: Free look\n" +
			"Shift: Fast movement\n"
			;

		private void RenderHelp()
		{
			RenderTextScale(_controlHelp, _controlHelpPosition, 0.5f);
		}

	}
}