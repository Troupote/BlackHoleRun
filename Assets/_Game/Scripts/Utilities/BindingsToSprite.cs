using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BHR
{
    public static class BindingsToSprite
    {
        public static List<InputBinding> KeyboardBindings(List<InputBinding> bindings) => bindings.Where(b => b.effectivePath.Contains("Keyboard") || b.effectivePath.Contains("Mouse") || b.effectivePath.Contains("Pointer")).ToList();
        public static List<InputBinding> GamepadBindings(List<InputBinding> bindings) => bindings.Where(b => b.effectivePath.Contains("Gamepad") || b.effectivePath.Contains("*")).ToList();


        public static string ConvertQwertyToAzerty(string controlPath)
        {
            string input = controlPath.Split("/")[controlPath.Split("/").Length - 1];
            if (!_qwertyToAzerty.ContainsKey(input))
                return controlPath;
            else
            {
                string newControlPath = "";
                for (int i = 0; i < controlPath.Split("/").Length; i++)
                    if (i == controlPath.Split("/").Length - 1)
                        newControlPath += _qwertyToAzerty[input];
                    else
                        newControlPath += controlPath.Split("/")[i] + "/";
                return newControlPath;
            }
        }

        private static Dictionary<string, string> _qwertyToAzerty = new Dictionary<string, string>
        {
            // Lettres diff�rentes
            { "a", "q" },
            { "q", "a" },
            { "w", "z" },
            { "z", "w" },
            { "m", ";" },

            // Chiffres avec shift (ex: 1 = !)
            //{ "1", "&" },
            //{ "2", "�" },
            //{ "3", "\"" },
            //{ "4", "'" },
            //{ "5", "(" },
            //{ "6", "-" },
            //{ "7", "�" },
            //{ "8", "_" },
            //{ "9", "�" },
            //{ "0", "�" },

            // Symboles
            { "-", ")" },
            { "=", "=" },  // Pas toujours mapp� diff�remment, d�pend du clavier
            { "[", "^" },
            { "]", "$" },
            { ";", "m" },  // Inverse de "m" ci-dessus
            { "'", "�" },
            { ",", "," },  // Virgule reste
            { ".", ":" },
            { "/", "!" },
            { "\\", "*" },
            { "`", "�" }
        };

        public static bool IsCurrentKeyboardAzerty() => Keyboard.current.wKey.displayName.ToLower() == "z";
    }
}
