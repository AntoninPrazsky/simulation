using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Demo
{
    public static class DemoHelper
    {
        /// <summary>
        /// Přibližná gravitace vesmírných těles v m/s² jako dvourozměrný vektor.
        /// </summary>
        public class Gravity
        {
            public static Vector2 Earth => new Vector2(0f, -9.807f);
            public static Vector2 Mars => new Vector2(0f, -3.710f);
            public static Vector2 Sun => new Vector2(0f, -274f);
            public static Vector2 Moon => new Vector2(0f, -1.62f);
        }

        public class Positions
        {
            public static Vector3 DefaultCameraPosition => new Vector3(0f, 5f, 25f);
        }

        public static bool PressedOnce(
            Keys key,
            Buttons button,
            KeyboardState keyboardState,
            GamePadState gamePadState,
            KeyboardState previousKeyboardState,
            GamePadState previousGamePadState)
        {
            bool keyboardPressed = keyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
            bool gamePadPressed = gamePadState.IsButtonDown(button) && !previousGamePadState.IsButtonDown(button);
            return keyboardPressed || gamePadPressed;
        }

        public static bool PressedOnce(
            Keys key,
            KeyboardState keyboardState,
            KeyboardState previousKeyboardState)
        {
            bool keyboardPressed = keyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);

            return keyboardPressed;
        }

        public static bool PressedOnce(
            bool leftMouseButton,
            bool middleMouseButton,
            bool rightMouseButton,
            MouseState mouseState,
            MouseState previsousMouseState)
        {
            bool mousePressed = leftMouseButton &&
                (mouseState.LeftButton == ButtonState.Pressed)
                && (previsousMouseState.LeftButton == ButtonState.Released);

            mousePressed = middleMouseButton &&
                (mouseState.MiddleButton == ButtonState.Pressed)
                && (previsousMouseState.MiddleButton == ButtonState.Released);

            mousePressed = rightMouseButton &&
                (mouseState.RightButton == ButtonState.Pressed)
                && (previsousMouseState.RightButton == ButtonState.Released);

            return mousePressed;
        }
    }
}