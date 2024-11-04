using System.Drawing;

namespace MealSync.Application.Common.Utils;

public static class BitMapUtils
{
    public static byte[] BitmapToByteArray(Bitmap bitmap)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png); // Save as PNG or other format
            return memoryStream.ToArray();
        }
    }
}