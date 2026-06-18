// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using MA.Mathematics;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace MA.Core
{
    /// <summary>Utility methods for working with colors in different color spaces.</summary>
    [BurstCompile]
    public static class ColorUtility
    {
        /// <summary>Determines the interpolation mode to use when interpolating between two colors.</summary>
        public enum InterpolationMode
        {
            /// <summary>Interpolates between the two colors in the shortest direction.</summary>
            Shortest,
            /// <summary>Interpolates between the two colors in the longest direction.</summary>
            Longest,
            /// <summary>Interpolates between the two colors in the clockwise direction.</summary>
            Clockwise,
            /// <summary>Interpolates between the two colors in the counter-clockwise direction.</summary>
            CounterClockwise
        }

        /// <summary>Lerps between two colors in LCH space.</summary>
        /// <param name="a">The first color.</param>
        /// <param name="b">The second color.</param>
        /// <param name="t">The interpolation value.</param>
        /// <param name="mode">The interpolation mode to use.</param>
        /// <param name="outInterpolatedColor">The output interpolated color.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LerpLCH(in Color a, in Color b, float t, InterpolationMode mode, out Color outInterpolatedColor)
        {
            // Convert to LCH
            (float l, float c, float h) aLCH = ColorToLCH(a);
            (float l, float c, float h) bLCH = ColorToLCH(b);
            
            float difference = math.abs(bLCH.h - aLCH.h);
            switch (mode)
            {
                case InterpolationMode.Shortest:
                    if (difference > 180f)
                    {
                        if (bLCH.h > aLCH.h)
                        {
                            aLCH.h += 360f;
                        }
                        else
                        {
                            bLCH.h += 360f;
                        }
                    }
                    break;
                case InterpolationMode.Longest:
                    if (difference < 180f)
                    {
                        if (bLCH.h > aLCH.h)
                        {
                            bLCH.h += 360f;
                        }
                        else
                        {
                            aLCH.h += 360f;
                        }
                    }
                    break;
                case InterpolationMode.Clockwise:
                    if (bLCH.h < aLCH.h)
                    {
                        bLCH.h += 360f;
                    }
                    break;
                case InterpolationMode.CounterClockwise:
                    if (bLCH.h > aLCH.h)
                    {
                        aLCH.h += 360f;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
            
            // Interpolate LCH
            (float l, float c, float h) lch = (
                math.lerp(aLCH.l, bLCH.l, t), 
                math.lerp(aLCH.c, bLCH.c, t), 
                math.lerp(aLCH.h, bLCH.h, t) % 360f);
            
            // Convert back to RGB
            outInterpolatedColor = LCHToColor(lch);
            outInterpolatedColor.a = math.lerp(a.a, b.a, t);
        }
        
        /// <summary>Lerps between two colors in LCH space.</summary>
        /// <param name="a">The first color.</param>
        /// <param name="b">The second color.</param>
        /// <param name="t">The interpolation value.</param>
        /// <param name="outInterpolatedColor">The output interpolated color.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LerpLCH(in Color a, in Color b, float t, out Color outInterpolatedColor) => LerpLCH(a, b, t, InterpolationMode.Shortest, out outInterpolatedColor);
        
        /// <summary>Lerps between two colors in LAB space.</summary>
        /// <param name="a">The first color.</param>
        /// <param name="b">The second color.</param>
        /// <param name="t">The interpolation value.</param>
        /// <param name="outInterpolatedColor">The output interpolated color.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LerpLAB(in Color a, in Color b, float t, out Color outInterpolatedColor)
        {
            // Convert to LAB
            (float l, float a, float b) aLAB = ColorToLAB(a);
            (float l, float a, float b) bLAB = ColorToLAB(b);
            
            // Interpolate LAB
            (float l, float a, float b) lab = (math.lerp(aLAB.l, bLAB.l, t), math.lerp(aLAB.a, bLAB.a, t), math.lerp(aLAB.b, bLAB.b, t));
            
            // Convert back to RGB
            outInterpolatedColor = LABToColor(lab);
            outInterpolatedColor.a = math.lerp(a.a, b.a, t);
        }
        
        /// <summary>Creates an RGB tuple from a color.</summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>A tuple containing the RGB values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float r, float g, float b) ColorToRGB(in Color color) => (color.r, color.g, color.b);
        
        /// <summary>Converts a color from RGB to HSL.</summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>A tuple containing the HSL values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float h, float s, float l) ColorToHSL(in Color color) => RGBToHSL((color.r, color.g, color.b));
        
        /// <summary>Converts a color from RGB to XYZ.</summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>A tuple containing the XYZ values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float x, float y, float z) ColorToXYZ(in Color color) => RGBToXYZ((color.r, color.g, color.b));
        
        /// <summary>Converts a color from RGB to LAB.</summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>A tuple containing the LAB values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float l, float a, float b) ColorToLAB(in Color color) => XYZToLAB(ColorToXYZ(color));
        
        /// <summary>Returns the LCH representation of a color.</summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>A tuple containing the LCH values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float l, float c, float h) ColorToLCH(in Color color) => LABToLCH(ColorToLAB(color));
        
        /// <summary>Converts a color to RGB.</summary>
        /// <param name="rgb">The rgb values to convert.</param>
        /// <returns>A color.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color RGBToColor((float r, float g, float b) rgb) => new Color(rgb.r, rgb.g, rgb.b, 1.0f);

        /// <summary>Converts a color to HSL.</summary>
        /// <param name="hsl">The hsl values to convert.</param>
        /// <returns>A color.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color HSLToColor((float h, float s, float l) hsl) => RGBToColor(HSLToRGB(hsl));
        
        /// <summary>Converts an XYZ color to a Color.</summary>
        /// <param name="xyz">The XYZ values to convert.</param>
        /// <returns>A color.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color XYZToColor((float x, float y, float z) xyz) => RGBToColor(XYZToRGB(xyz));
        
        /// <summary>Converts a LAB color to a Color.</summary>
        /// <param name="lab">The LAB values to convert.</param>
        /// <returns>A color.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color LABToColor((float l, float a, float b) lab) => XYZToColor(LABToXYZ(lab));
        
        /// <summary>Converts a LCH color to a Color.</summary>
        /// <param name="lch">The LCH values to convert.</param>
        /// <returns>A color.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color LCHToColor((float l, float c, float h) lch) => LABToColor(LCHToLAB(lch));
        
        /// <summary>Converts a color from RGB to HSL.</summary>
        /// <param name="rgb">The RGB values to convert.</param>
        /// <returns>A tuple containing the HSL values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float h, float s, float l) RGBToHSL((float r, float g, float b) rgb)
        {
            float r = rgb.r;
            float g = rgb.g;
            float b = rgb.b;

            float max = math.max(math.max(r, g), b);
            float min = math.min(math.min(r, g), b);
            float h, s, l = (max + min) / 2f;

            if (max.NearlyEquals(min))
            {
                h = s = 0f; // achromatic
            }
            else
            {
                float d = max - min;
                s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

                if (max.NearlyEquals(r))
                {
                    h = (g - b) / d + (g < b ? 6f : 0f);
                }
                else if (max.NearlyEquals(g))
                {
                    h = (b - r) / d + 2f;
                }
                else
                {
                    h = (r - g) / d + 4f;
                }

                h /= 6f;
            }

            return (h, s, l);
        }

        /// <summary>Converts a color from HSL to RGB.</summary>
        /// <param name="hsl">The HSL values to convert.</param>
        /// <returns>A tuple containing the RGB values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float r, float g, float b) HSLToRGB((float h, float s, float l) hsl)
        {
            float HueToRGB(float p, float q, float t)
            {
                if (t < 0f) t += 1f;
                if (t > 1f) t -= 1f;
                if (t < 1f / 6f) return p + (q - p) * 6f * t;
                if (t < 1f / 2f) return q;
                if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
                return p;
            }

            float r, g, b;

            if (hsl.s == 0f)
            {
                r = g = b = hsl.l; // achromatic
            }
            else
            {
                float q = hsl.l < 0.5f ? hsl.l * (1f + hsl.s) : hsl.l + hsl.s - hsl.l * hsl.s;
                float p = 2f * hsl.l - q;

                r = HueToRGB(p, q, hsl.h + 1f / 3f);
                g = HueToRGB(p, q, hsl.h);
                b = HueToRGB(p, q, hsl.h - 1f / 3f);
            }

            return (r, g, b);
        }

        /// <summary>Converts a color from RGB to XYZ.</summary>
        /// <param name="rgb">The RGB values to convert.</param>
        /// <returns>A tuple containing the XYZ values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float x, float y, float z) RGBToXYZ((float r, float g, float b) rgb)
        {
            float r = StandardToLinear(rgb.r);
            float g = StandardToLinear(rgb.g);
            float b = StandardToLinear(rgb.b);

            // Observer = 2°, Illuminant = D65
            float x = r * 0.4124f + g * 0.3576f + b * 0.1805f;
            float y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
            float z = r * 0.0193f + g * 0.1192f + b * 0.9505f;

            return (x, y, z);
        }

        /// <summary>Converts a color from XYZ to RGB.</summary>
        /// <param name="xyz">The XYZ values to convert.</param>
        /// <returns>A tuple containing the RGB values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float r, float g, float b) XYZToRGB((float x, float y, float z) xyz)
        {
            float r = xyz.x *  3.2406f + xyz.y * -1.5372f + xyz.z * -0.4986f;
            float g = xyz.x * -0.9689f + xyz.y *  1.8758f + xyz.z *  0.0415f;
            float b = xyz.x *  0.0557f + xyz.y * -0.2040f + xyz.z *  1.0570f;

            r = LinearToStandard(r);
            g = LinearToStandard(g);
            b = LinearToStandard(b);

            return (r, g, b);
        }
        
        const float k_IlluminantD65X = 0.95047f;
        const float k_IlluminantD65Y = 1.00000f;
        const float k_IlluminantD65Z = 1.08883f;

        /// <summary>Converts a color from XYZ to LAB.</summary>
        /// <param name="xyz">The XYZ values to convert.</param>
        /// <returns>A tuple containing the LAB values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float l, float a, float b) XYZToLAB((float x, float y, float z) xyz)
        {
            float ForwardTransform(float c) => c > 0.008856f
                ? math.pow(c, 0.333333333f) 
                : (7.787f * c) + (16f / 116f);
            
            // Observer = 2°, Illuminant = D65
            float x = ForwardTransform(xyz.x / k_IlluminantD65X);
            float y = ForwardTransform(xyz.y / k_IlluminantD65Y);
            float z = ForwardTransform(xyz.z / k_IlluminantD65Z);

            float l = (116f * y) - 16f;
            float a = 500f * (x - y);
            float b = 200f * (y - z);

            return (l, a, b);
        }

        /// <summary>Converts a color from LAB to XYZ.</summary>
        /// <param name="lab">The LAB values to convert.</param>
        /// <returns>A tuple containing the XYZ values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float x, float y, float z) LABToXYZ((float l, float a, float b) lab)
        {
            float ReverseTransform(float c) => c > 0.206897f
                ? math.pow(c, 3f)
                : (c - 0.137931034f) / 7.787f;
            
            float y = (lab.l + 16f) / 116f;
            float x = lab.a / 500f + y;
            float z = y - lab.b / 200f;

            x = k_IlluminantD65X * ReverseTransform(x);
            y = k_IlluminantD65Y * ReverseTransform(y);
            z = k_IlluminantD65Z * ReverseTransform(z);

            return (x, y, z);
        }

        /// <summary>Converts a color from LAB to LCH.</summary>
        /// <param name="lab">The LAB values to convert.</param>
        /// <returns>A tuple containing the LCH values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float l, float c, float h) LABToLCH((float l, float a, float b) lab)
        {
            float c = math.sqrt(math.pow(lab.a, 2f) + math.pow(lab.b, 2f));
            float h = math.degrees(math.atan2(lab.b, lab.a));
            
            if (h < 0f) h = 360f + h;

            return (lab.l, c, h);
        }

        /// <summary>Converts a color from LCH to LAB.</summary>
        /// <param name="lch">The LCH values to convert.</param>
        /// <returns>A tuple containing the LAB values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float l, float a, float b) LCHToLAB((float l, float c, float h) lch)
        {
            float a = math.cos(math.radians(lch.h)) * lch.c;
            float b = math.sin(math.radians(lch.h)) * lch.c;
            return (lch.l, a, b);
        }
        
        /// <summary>Converts a color channel from standard RGB to linear RGB.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float StandardToLinear(float c) => c > 0.04045f ? math.pow((c + 0.055f) / 1.055f, 2.4f) : c / 12.92f;

        /// <summary>Converts a color channel from linear RGB to standard RGB.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float LinearToStandard(float c) => c > 0.0031308f ? 1.055f * math.pow(c, 1f / 2.4f) - 0.055f : 12.92f * c;
    }
}