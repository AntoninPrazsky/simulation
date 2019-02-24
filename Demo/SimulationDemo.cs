﻿/*
 * Způsob načítání obrazovek ze jmenného prostoru Demo.Scenes s využitím reflexe je inspirován ukázkovým projektem
 * JitterDemo (https://github.com/mattleibow/jitterphysics/tree/master/samples/JitterDemo), jehož autorem je Matthew
 * Leibowitz.
 */

using Demo.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prazsky.Simulation;
using Prazsky.Simulation.Camera;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Demo
{
    /// <summary>
    /// Hlavní třída testovacího projektu, jehož smyslem je ukázat schopnosti knihovny Prazsky.Simulation.
    /// </summary>
    public class SimulationDemo : Game
    {
        private int _currentDemo = 0;

        /// <summary>
        /// Konstruktor hlavní třídy testovacího projektu.
        /// </summary>
        /// <param name="windowed">Aktivace režimu v okně.</param>
        /// <param name="preferHiDef">Použití maximálního počtu funkcí a možností grafického hardwaru, je-li to
        /// podporováno</param>
        /// <param name="preferMultiSampling">Použití multisample anti-aliasingu (MSAA), je-li podporováno.</param>
        public SimulationDemo(bool windowed = false, bool preferHiDef = true, bool preferMultiSampling = true)
        {
            _windowed = windowed;
            _preferHiDef = preferHiDef;
            _preferMultiSampling = preferMultiSampling;

            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreparingDeviceSettings += _graphics_PreparingDeviceSettings;
            Content.RootDirectory = "Content";
        }

        public BasicCamera3D Camera3D { private set; get; }
        public List<Scene> DemoScenes { private set; get; }
        public Info Info { private set; get; }
        public World3D World3D { private set; get; }

        #region Ovládání

        private const float _mouseMovementDenominator = 50f;

        private GamePadState _currentGamePadState = new GamePadState();
        private KeyboardState _currentKeyboardState = new KeyboardState();
        private MouseState _currentMouseState = new MouseState();

        private GamePadState _previousGamePadState = new GamePadState();
        private KeyboardState _previousKeyboardState = new KeyboardState();
        private MouseState _previousMouseState = new MouseState();

        private int _heightHalf;
        private int _widthHalf;
        private bool _mousePanMode = false;
        private bool _mouseRotationMode = false;

        #endregion Ovládání

        #region Grafika

        //Velikost grafické plochy okna, pokud není použito celoobrazovkové zobrazení
        private const int _windowWidth = 1920;

        private const int _windowHeight = 1080;

        private GraphicsDeviceManager _graphics;
        private bool _windowed;
        private bool _preferHiDef;
        private bool _preferMultiSampling;

        #endregion Grafika

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Bisque);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            World3D.Draw();
            DemoScenes[_currentDemo].Draw();

            base.Draw(gameTime);
        }

        protected override void Initialize()
        {
            SetGraphics(_windowed);

            Camera3D = new BasicCamera3D(
                DemoHelper.Positions.DefaultCameraPosition,
                _graphics.GraphicsDevice.Viewport.AspectRatio);

            World3D = new World3D(DemoHelper.Gravity.Earth, Camera3D);

            Info = new Info(this) { DrawOrder = int.MaxValue };
            Components.Add(Info);

            CreateScenesInstances();

            base.Initialize();
        }

        protected override void LoadContent()
        {
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            //Aktuální stav vstupních zařízení
            _currentKeyboardState = Keyboard.GetState();
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
            _currentMouseState = Mouse.GetState();

            if (PressedOnce(Keys.Escape, Buttons.Back)) Exit();

            #region Přepínání ukázkových scén

            if (PressedOnce(Keys.Left, Buttons.DPadLeft))
            {
                DestructCurrentScene();
                _currentDemo++;
                _currentDemo = _currentDemo % DemoScenes.Count;
                DemoScenes[_currentDemo].Construct();
            }

            if (PressedOnce(Keys.Right, Buttons.DPadRight))
            {
                DestructCurrentScene();
                _currentDemo += DemoScenes.Count - 1;
                _currentDemo = _currentDemo % DemoScenes.Count;
                DemoScenes[_currentDemo].Construct();
            }

            #endregion Přepínání ukázkových scén

            //Aktualizace pohybu kamerou
            CameraMovement(gameTime);

            //Aktualizace herního světa (a tím probíhající fyzikální simulace)
            World3D.Update(gameTime);

            //Možnost uchopení tělesa myší
            GrabWorldObject();
            DemoScenes[_currentDemo].Update(_currentKeyboardState, _previousKeyboardState, _currentGamePadState, _previousGamePadState);

            //Předchozí stav vstupních zařízení (pro další aktualizaci)
            _previousKeyboardState = _currentKeyboardState;
            _previousGamePadState = _currentGamePadState;
            _previousMouseState = _currentMouseState;

            base.Update(gameTime);
        }

        private void _graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            //Obnovovací frekvence vykreslování
            e.GraphicsDeviceInformation.PresentationParameters.PresentationInterval = PresentInterval.One;

            if (_preferHiDef && e.GraphicsDeviceInformation.Adapter.IsProfileSupported(GraphicsProfile.HiDef))
                e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
            else
                e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.Reach;
        }

        /// <summary>
        /// Demonstruje možnosti ovládání kamery z různých vstupních zařízení.
        /// </summary>
        /// <param name="gameTime">Herní čas.</param>
        private void CameraMovement(GameTime gameTime)
        {
            Debug.WriteLine(Camera3D.Position.ToString() + " " + Camera3D.Target.ToString());

            #region kompletní ovládání kamery gamepadem

            if (_currentGamePadState.IsConnected)
            {
                float Z = 0f;
                if (_currentGamePadState.Triggers.Right > 0) Z = -_currentGamePadState.Triggers.Right;
                if (_currentGamePadState.Triggers.Left > 0) Z = _currentGamePadState.Triggers.Left;

                Camera3D.Move(
                    _currentGamePadState.ThumbSticks.Left.X,
                    _currentGamePadState.ThumbSticks.Left.Y,
                    Z, gameTime);
                Camera3D.Rotate(
                    _currentGamePadState.ThumbSticks.Right.Y,
                    -_currentGamePadState.ThumbSticks.Right.X,
                    gameTime);
            }

            #endregion kompletní ovládání kamery gamepadem

            #region ovládání kamery klávesnicí

            float speed = 1f;
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift)) speed = 3f;

            if (Keyboard.GetState().IsKeyDown(Keys.W))
                Camera3D.Move(0, 0f, -speed, gameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                Camera3D.Move(0, 0f, speed, gameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                Camera3D.Move(-speed, 0f, 0f, gameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                Camera3D.Move(speed, 0f, 0f, gameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.E))
                Camera3D.Move(0f, speed, 0f, gameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
                Camera3D.Move(0f, -speed, 0f, gameTime);

            if (PressedOnce(Keys.O, Buttons.Y))
                World3D.DisableSimulationWhenOutOfBoundingFrustum = !World3D.DisableSimulationWhenOutOfBoundingFrustum;

            #endregion ovládání kamery klávesnicí

            #region ovládání kamery myší

            if (PressedOnceMouse(leftButton: false, middleButton: false, rightButton: true))
            {
                CenterMouse();
                _mouseRotationMode = !_mouseRotationMode;
                return;
            }

            if (_currentMouseState.RightButton == ButtonState.Pressed)
                _mousePanMode = true;

            IsMouseVisible = !_mousePanMode && !_mouseRotationMode;

            if (_mouseRotationMode || _mousePanMode)
            {
                float mDeltaA = 0f;
                float mDeltaB = 0f;

                if (_currentMouseState.X != _widthHalf)
                    mDeltaB = -(_currentMouseState.X - _widthHalf) / _mouseMovementDenominator;

                if (_currentMouseState.Y != _heightHalf)
                    mDeltaA = -(_currentMouseState.Y - _heightHalf) / _mouseMovementDenominator;

                CenterMouse();

                if (_mouseRotationMode && !_mousePanMode)
                    Camera3D.Rotate(mDeltaA, mDeltaB, gameTime);

                if (_currentMouseState.RightButton == ButtonState.Pressed)
                {
                    _mousePanMode = true;
                    Camera3D.Move(-mDeltaB, mDeltaA, 0f, gameTime);
                }
                else
                    _mousePanMode = false;
            }

            #endregion ovládání kamery myší
        }

        private void CenterMouse()
        {
            Mouse.SetPosition(_widthHalf, _heightHalf);
        }

        private void CreateScenesInstances()
        {
            DemoScenes = new List<Scene>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.Namespace == "Demo.Scenes" && !type.IsAbstract && type.DeclaringType == null)
                {
                    Scene scene = (Scene)Activator.CreateInstance(type, this);
                    if (type.Name == "BridgeScene") _currentDemo = DemoScenes.Count;
                    DemoScenes.Add(scene);
                }
            }

            if (DemoScenes.Count > 0) DemoScenes[_currentDemo].Construct();
        }

        private void DestructCurrentScene()
        {
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                IGameComponent component = Components[i];
                if (component is Info) continue;
                Components.RemoveAt(i);
            }

            World3D.Clear();
        }

        private void GrabWorldObject()
        {
            if (_currentMouseState.LeftButton == ButtonState.Pressed)
                World3D.GrabBody(_currentMouseState.Position, force: 50f, viewport: _graphics.GraphicsDevice.Viewport);
            if (_currentMouseState.LeftButton == ButtonState.Released)
                World3D.ReleaseGrabbedBody();
        }

        private bool PressedOnce(Keys key, Buttons button)
        {
            return DemoHelper.PressedOnce(
                key,
                button,
                _currentKeyboardState,
                _currentGamePadState,
                _previousKeyboardState,
                _previousGamePadState);
        }

        private bool PressedOnceMouse(bool leftButton, bool middleButton, bool rightButton)
        {
            return DemoHelper.PressedOnce(
                leftButton,
                middleButton,
                rightButton,
                _currentMouseState,
                _previousMouseState);
        }

        private void SetGraphics(bool windowed = false)
        {
            _graphics.PreferredBackBufferWidth = windowed ? _windowWidth : GraphicsDevice.DisplayMode.Width;
            _graphics.PreferredBackBufferHeight = windowed ? _windowHeight : GraphicsDevice.DisplayMode.Height;
            _graphics.IsFullScreen = !windowed;

            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.PreferMultiSampling = _preferMultiSampling;
            _graphics.ApplyChanges();

            IsMouseVisible = false;
            IsFixedTimeStep = false;

            _widthHalf = Window.ClientBounds.Width / 2;
            _heightHalf = Window.ClientBounds.Height / 2;

            CenterMouse();
        }
    }
}