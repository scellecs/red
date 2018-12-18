//
// Created by Needle on 2018-11-01.
// Copyright (c) 2018 Needle. No rights reserved :)
//

using UnityEngine;

namespace Red.Example.UI {
    public static class RectTransformExtensions {
        public static void Stretch(this RectTransform rect) {
            rect.anchoredPosition3D = Vector3.zero;
            rect.localScale = Vector3.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
        }
    }
}