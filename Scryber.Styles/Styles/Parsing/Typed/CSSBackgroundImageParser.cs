﻿using System;
using Scryber.Html;

namespace Scryber.Styles.Parsing.Typed
{
    /// <summary>
    /// Parses and sets the background values for a component based on the shorthand css background property
    /// </summary>
    public class CSSBackgroundImageParser : CSSUrlStyleParser
    {
        public CSSBackgroundImageParser()
            : base(CSSStyleItems.BackgroundImage, StyleKeys.BgImgSrcKey)
        {
        }

    }
}
