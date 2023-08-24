using Atelier39;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace BiliLite.Extensions
{
    public static class DanmakuExtensions
    {
        public static void ParseAdvanceDanmaku(this DanmakuItem danmakuItem)
        {
            var content = danmakuItem.Text;
            if (danmakuItem.Mode == DanmakuMode.Advanced)
            {
                if (!content.StartsWith("[") || !content.EndsWith("]"))
                {
                    return;
                }

                danmakuItem.AllowDensityControl = false;

                string[] valueArray;
                try
                {
                    JArray jArray = JArray.Parse(content);
                    valueArray = new string[jArray.Count];
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        valueArray[i] = jArray[i].ToString();
                    }

                    if (valueArray.Length < 5)
                    {
                        return;
                    }
                    danmakuItem.Text = WebUtility.HtmlDecode(valueArray[4]).Replace("/n", "\n").Replace("\\n", "\n");
                    if (string.IsNullOrWhiteSpace(danmakuItem.Text))
                    {
                        return;
                    }

                    danmakuItem.StartX = string.IsNullOrWhiteSpace(valueArray[0]) ? 0f : float.Parse(valueArray[0]);
                    danmakuItem.StartY = string.IsNullOrWhiteSpace(valueArray[1]) ? 0f : float.Parse(valueArray[1]);
                    danmakuItem.EndX = danmakuItem.StartX;
                    danmakuItem.EndY = danmakuItem.StartY;

                    string[] opacitySplit = valueArray[2].Split('-');
                    danmakuItem.StartAlpha = (byte)(Math.Max(float.Parse(opacitySplit[0]), 0) * byte.MaxValue);
                    danmakuItem.EndAlpha = opacitySplit.Length > 1 ? (byte)(Math.Max(float.Parse(opacitySplit[1]), 0) * byte.MaxValue) : danmakuItem.StartAlpha;

                    danmakuItem.DurationMs = (ulong)(float.Parse(valueArray[3]) * 1000);
                    danmakuItem.TranslationDurationMs = danmakuItem.DurationMs;
                    danmakuItem.TranslationDelayMs = 0;
                    danmakuItem.AlphaDurationMs = danmakuItem.DurationMs;
                    danmakuItem.AlphaDelayMs = 0;

                    if (valueArray.Length >= 7)
                    {
                        danmakuItem.RotateZ = string.IsNullOrWhiteSpace(valueArray[5]) ? 0f : float.Parse(valueArray[5]);
                        danmakuItem.RotateY = string.IsNullOrWhiteSpace(valueArray[6]) ? 0f : float.Parse(valueArray[6]);
                    }
                    else
                    {
                        danmakuItem.RotateZ = 0f;
                        danmakuItem.RotateY = 0f;
                    }

                    if (valueArray.Length >= 11)
                    {
                        danmakuItem.EndX = string.IsNullOrWhiteSpace(valueArray[7]) ? 0f : float.Parse(valueArray[7]);
                        danmakuItem.EndY = string.IsNullOrWhiteSpace(valueArray[8]) ? 0f : float.Parse(valueArray[8]);
                        if (!string.IsNullOrWhiteSpace(valueArray[9]))
                        {
                            danmakuItem.TranslationDurationMs = (ulong)(float.Parse(valueArray[9]));
                        }
                        if (!string.IsNullOrWhiteSpace(valueArray[10]))
                        {
                            string translationDelayValue = valueArray[10];
                            if (translationDelayValue == "０") // To be compatible with legacy style
                            {
                                danmakuItem.TranslationDelayMs = 0;
                            }
                            else
                            {
                                danmakuItem.TranslationDelayMs = (ulong)(float.Parse(translationDelayValue));
                            }
                        }
                    }

                    //if (valueArray.Length >= 12 && (valueArray[11].Equals("true", StringComparison.OrdinalIgnoreCase) || valueArray[11] == "1"))
                    //{
                    //    danmakuItem.HasOutline = false;
                    //}
                    //else
                    //{
                    //    danmakuItem.OutlineColor = danmakuItem.TextColor.R + danmakuItem.TextColor.G + danmakuItem.TextColor.B > 32 ? Colors.Black : Colors.White;
                    //    danmakuItem.OutlineColor.A = danmakuItem.TextColor.A;
                    //}
                    danmakuItem.HasOutline = false;

                    //if (valueArray.Length >= 13)
                    //{
                    //    string fontFamilyName = valueArray[12];
                    //    if (!string.IsNullOrWhiteSpace(fontFamilyName))
                    //    {
                    //        danmakuItem.FontFamilyName = fontFamilyName.Replace("\"", string.Empty);
                    //    }
                    //}
                    danmakuItem.FontFamilyName = "Consolas"; // Default monospaced font

                    danmakuItem.KeepDefinedFontSize = true;
                }
                catch (Exception ex)
                {
                    return;
                }
            }
        }
    }
}
