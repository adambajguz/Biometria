// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.Shared.Processing.Thinning
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace RetinaImageProcessor.UI.Logic.Shared.Processing
{
  public class Thinning
  {
    private static int[] A0 = new int[48]
    {
      3,
      6,
      7,
      12,
      14,
      15,
      24,
      28,
      30,
      31,
      48,
      56,
      60,
      62,
      63,
      96,
      112,
      120,
      124,
      126,
      (int) sbyte.MaxValue,
      129,
      131,
      135,
      143,
      159,
      191,
      192,
      193,
      195,
      199,
      207,
      223,
      224,
      225,
      227,
      231,
      239,
      240,
      241,
      243,
      247,
      248,
      249,
      251,
      252,
      253,
      254
    };
    private static int[] A1 = new int[8]
    {
      7,
      14,
      28,
      56,
      112,
      131,
      193,
      224
    };
    private static int[] A2 = new int[16]
    {
      7,
      14,
      15,
      28,
      30,
      56,
      60,
      112,
      120,
      131,
      135,
      193,
      195,
      224,
      225,
      240
    };
    private static int[] A3 = new int[24]
    {
      7,
      14,
      15,
      28,
      30,
      31,
      56,
      60,
      62,
      112,
      120,
      124,
      131,
      135,
      143,
      193,
      195,
      199,
      224,
      225,
      227,
      240,
      241,
      248
    };
    private static int[] A4 = new int[32]
    {
      7,
      14,
      15,
      28,
      30,
      31,
      56,
      60,
      62,
      63,
      112,
      120,
      124,
      126,
      131,
      135,
      143,
      159,
      193,
      195,
      199,
      207,
      224,
      225,
      227,
      231,
      240,
      241,
      243,
      248,
      249,
      252
    };
    private static int[] A5 = new int[36]
    {
      7,
      14,
      15,
      28,
      30,
      31,
      56,
      60,
      62,
      63,
      112,
      120,
      124,
      126,
      131,
      135,
      143,
      159,
      191,
      193,
      195,
      199,
      207,
      224,
      225,
      227,
      231,
      239,
      240,
      241,
      243,
      248,
      249,
      251,
      252,
      254
    };
    private static int[] A1Pix = new int[120]
    {
      3,
      5,
      7,
      12,
      13,
      14,
      15,
      20,
      21,
      22,
      23,
      28,
      29,
      30,
      31,
      48,
      52,
      53,
      54,
      55,
      56,
      60,
      61,
      62,
      63,
      65,
      67,
      69,
      71,
      77,
      79,
      80,
      81,
      83,
      84,
      85,
      86,
      87,
      88,
      89,
      91,
      92,
      93,
      94,
      95,
      97,
      99,
      101,
      103,
      109,
      111,
      112,
      113,
      115,
      116,
      117,
      118,
      119,
      120,
      121,
      123,
      124,
      125,
      126,
      (int) sbyte.MaxValue,
      131,
      133,
      135,
      141,
      143,
      149,
      151,
      157,
      159,
      181,
      183,
      189,
      191,
      192,
      193,
      195,
      197,
      199,
      205,
      207,
      208,
      209,
      211,
      212,
      213,
      214,
      215,
      216,
      217,
      219,
      220,
      221,
      222,
      223,
      224,
      225,
      227,
      229,
      231,
      237,
      239,
      240,
      241,
      243,
      244,
      245,
      246,
      247,
      248,
      249,
      251,
      252,
      253,
      254,
      (int) byte.MaxValue
    };
    private static int[,] N = new int[3, 3]
    {
      {
        128,
        1,
        2
      },
      {
        64,
        0,
        4
      },
      {
        32,
        16,
        8
      }
    };
    private static List<int[]> K3M_Lookups = new List<int[]>()
    {
      Thinning.A0,
      Thinning.A1,
      Thinning.A2,
      Thinning.A3,
      Thinning.A4,
      Thinning.A5,
      Thinning.A1Pix
    };
    private static Thinning instance;

    public static Thinning Instance
    {
      get
      {
        if (Thinning.instance == null)
          Thinning.instance = new Thinning();
        return Thinning.instance;
      }
    }

    private Thinning()
    {
    }

    public Bitmap K3MThinning(Bitmap workingBitmap)
    {
      Bitmap output = AForge.Imaging.Image.Clone(workingBitmap, PixelFormat.Format32bppArgb);
      bool[,] isBorder = new bool[output.Width, output.Height];
      bool isModified = false;
      do
      {
        this.K3MFlagBorders(output, isBorder, isModified);
        isModified = this.K3MThinningPhases(output, isBorder, isModified);
      }
      while (isModified);
      this.K3MOnePixelWidthPhase(output);
      return output;
    }

    public void K3MFlagBorders(Bitmap output, bool[,] isBorder, bool isModified)
    {
      for (int x = 0; x < output.Width; ++x)
      {
        for (int y = 0; y < output.Height; ++y)
        {
          Color pixel = output.GetPixel(x, y);
          if (pixel.G > (byte) 0)
          {
            int num1 = 0;
            for (int index1 = -1; index1 < 2; ++index1)
            {
              for (int index2 = -1; index2 < 2; ++index2)
              {
                if (x + index1 >= 0 && x + index1 < output.Width && y + index2 >= 0 && y + index2 < output.Height)
                {
                  int num2 = num1;
                  int num3 = Thinning.N[index1 + 1, index2 + 1];
                  pixel = output.GetPixel(x + index1, y + index2);
                  int g = (int) pixel.G;
                  int num4 = num3 & g;
                  num1 = num2 + num4;
                }
              }
            }
            isBorder[x, y] = ((IEnumerable<int>) Thinning.K3M_Lookups[0]).Contains<int>(num1);
          }
          else
            isBorder[x, y] = false;
        }
      }
    }

    public bool K3MThinningPhases(Bitmap output, bool[,] isBorder, bool isModified)
    {
      isModified = false;
      for (int index1 = 1; index1 < 6; ++index1)
      {
        for (int x = 1; x < output.Width - 1; ++x)
        {
          for (int y = 1; y < output.Height - 1; ++y)
          {
            if (isBorder[x, y])
            {
              int num = 0;
              for (int index2 = -1; index2 < 2; ++index2)
              {
                for (int index3 = -1; index3 < 2; ++index3)
                  num += Thinning.N[index2 + 1, index3 + 1] & (int) output.GetPixel(x + index2, y + index3).G;
              }
              if (((IEnumerable<int>) Thinning.K3M_Lookups[index1]).Contains<int>(num))
              {
                output.SetPixel(x, y, Color.Black);
                isModified = true;
                isBorder[x, y] = false;
              }
            }
          }
        }
      }
      return isModified;
    }

    public void K3MOnePixelWidthPhase(Bitmap output)
    {
      for (int x = 0; x < output.Width; ++x)
      {
        for (int y = 0; y < output.Height; ++y)
        {
          Color pixel = output.GetPixel(x, y);
          if (pixel.G > (byte) 0)
          {
            int num1 = 0;
            for (int index1 = -1; index1 < 2; ++index1)
            {
              for (int index2 = -1; index2 < 2; ++index2)
              {
                if (x + index1 >= 0 && x + index1 < output.Width && y + index2 >= 0 && y + index2 < output.Height)
                {
                  int num2 = num1;
                  int num3 = Thinning.N[index1 + 1, index2 + 1];
                  pixel = output.GetPixel(x + index1, y + index2);
                  int g = (int) pixel.G;
                  int num4 = num3 & g;
                  num1 = num2 + num4;
                }
              }
            }
            if (((IEnumerable<int>) Thinning.K3M_Lookups[6]).Contains<int>(num1))
              output.SetPixel(x, y, Color.Black);
          }
        }
      }
    }
  }
}
