// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.MatchedFilters.View.StateOperations
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

namespace RetinaImageProcessor.UI.Logic.MatchedFilters.View
{
  public enum StateOperations
  {
    LoadImage,
    ConvertToGrayscale,
    RemoveNoise,
    CreateMask,
    EqualizeHistogram,
    CorrectBrightness,
    EnhanceContrastBeforeSegmentation,
    SegmentVessels,
    EnhanceContrastAfterSegmentation,
    FindLocalEtropyThreshold,
    Binarize,
    Skeletonize,
    RemoveLooseVessels,
    ExtractMinutiae,
  }
}
