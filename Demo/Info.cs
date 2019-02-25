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

		private Vector2 _fpsPosition = new Vector2(20, 10);
		private Vector2 _gpuPosition = new Vector2(200, 10);
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
			RenderText(_gpu, _gpuPosition);

			_spriteBatch.End();
		}

		private void RenderText(string text, Vector2 position)
		{
			_spriteBatch.DrawString(_font, text, new Vector2(position.X + 2, position.Y + 2), Color.Black);
			_spriteBatch.DrawString(_font, text, position, Color.White);
		}
	}
}