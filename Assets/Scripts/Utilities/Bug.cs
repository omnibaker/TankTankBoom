using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class Bug : Singleton<Bug>
    {
        [SerializeField] private bool _addDateTime;
        [SerializeField] private bool _shadeBackground = true;
        [SerializeField][Range(1, 2)] private float _sizeScale = 1f;
        [SerializeField] private int _multiLines = 10;
        [SerializeField] private Font _monospace;
        [SerializeField] private Color _color;
        private Color _arGreen = new Color(0f, 1f, 0.5f);
        private const int SIZE = 30;
        private const int GAP = 5;
        private static bool _usesDT;
        private static int _screenWidth;
        private static int trueSize;
        private static bool _addDateTimeStatic;

        private void Awake()
        {
            CreateInstance(this, gameObject);
        }

        private void Start()
        {
            _usesDT = _addDateTime;
            GuiName($"{Application.productName}");
        }

        /// <summary>
        /// -- TODO --
        /// </summary>
        private void OnGUI()
        {
            trueSize = Mathf.RoundToInt(SIZE * _sizeScale);
            int labelFontSize = Mathf.RoundToInt(SIZE * 0.75f * _sizeScale);
            int indentTop = 0;
            int indent = Mathf.RoundToInt(SIZE * 0.33f * _sizeScale);
            int length = Screen.width - 2 * indent;
            GUI.skin.label.normal.textColor = _color;
            GUI.skin.label.fontSize = labelFontSize;
            GUI.skin.label.fontSize = labelFontSize;
            GUI.skin.font = _monospace;

            _addDateTimeStatic = _addDateTime;

            // Make a background box
            if (_shadeBackground)
            {
                GUI.Box(new Rect(indent, trueSize, length, (trueSize * 7) + (_multiLines * trueSize)), "");
            }
            GUI.Label(new Rect(indent * 4, indentTop += Mathf.RoundToInt(trueSize * 1.5f), length, Mathf.RoundToInt(trueSize * 1.5f)), guiName);
            GUI.Label(new Rect(indent *= 2, indentTop += Mathf.RoundToInt(trueSize * 1.5f), length, trueSize), $"A) {a}");
            GUI.Label(new Rect(indent, indentTop += trueSize, length, trueSize), $"B) {b}");
            GUI.Label(new Rect(indent, indentTop += trueSize, length, trueSize), $"C) {c}");
            GUI.Label(new Rect(indent, indentTop += trueSize, length, trueSize), $"D) {d}");
            GUI.Label(new Rect(indent, indentTop += trueSize, length, trueSize), $"E) {e}");
            GUI.Label(new Rect(indent, indentTop += trueSize, length, trueSize), $"F) {f}");
            GUI.Label(new Rect(indent, indentTop += trueSize, length, trueSize * _multiLines), $"G) {g}");
        }

        /// <summary>
        /// -- TODO --
        /// </summary>
        public static void Say(int index, string labelString)
        {
            if (_addDateTimeStatic)
            {
                DateTime time = DateTime.Now;
                labelString = $"{time:HH:mm:ss.ff} | {labelString}";
            }
            switch (index)
            {
                case 1: a = labelString; return;
                case 2: b = labelString; return;
                case 3: c = labelString; return;
                case 4: d = labelString; return;
                case 5: e = labelString; return;
                case 6: f = labelString; return;
                case 7: g = labelString; return;
                default: return;
            }
        }



        /// <summary>
        /// -- TODO --
        /// </summary>
        public static void GuiName(string name)
        {
            guiName = name;
        }

        private static string a = "";
        private static string b = "";
        private static string c = "";
        private static string d = "";
        private static string e = "";
        private static string f = "";
        private static string g = "";
        private static string guiName = "";

    }
}