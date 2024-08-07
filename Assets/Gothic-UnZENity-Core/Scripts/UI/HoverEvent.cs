﻿using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GUZ.Core.UI
{
    /// <summary>
    /// Alter font of Text based on G1 default/highlight fonts.
    /// </summary>
    public class HoverEvent : MonoBehaviour
    {
        public void OnPointerEnter(BaseEventData evt)
        {
            if (evt is not PointerEventData pointerEventData)
            {
                return;
            }

            foreach (var hoveredObj in pointerEventData.hovered)
            {
                var textComponent = hoveredObj.GetComponent<TMP_Text>();

                if (textComponent == null)
                {
                    continue;
                }
                
                textComponent.spriteAsset = GameGlobals.Font.HighlightSpriteAsset;
            }
        }

        public void OnPointerExit(BaseEventData evt)
        {
            if (evt is not PointerEventData pointerEventData)
            {
                return;
            }

            foreach (var hoveredObj in pointerEventData.hovered)
            {
                var textComponent = hoveredObj.GetComponent<TMP_Text>();

                if (textComponent == null)
                {
                    continue;
                }

                textComponent.spriteAsset = GameGlobals.Font.DefaultSpriteAsset;
            }
        }
    }
}
