// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.RegionFeatures.View.StateOperations
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

namespace RetinaImageProcessor.UI.Logic.RegionFeatures.View
{
  public enum StateOperations
  {
    LoadImage,
    ConvertToGrayscale,
    RemoveNoise,
    CreateMask,
    ApplyTopHatVesselEnhancement,
    DivideImage,
    DenoisePreliminaryVessels,
    ApplyIsotropicUndecimatedWaveletTransform,
    Binarize,
    ExtractSkeleton,
    MergeWithPreliminaryVessels,
    ApplyHierarchicalGrowth,
    RemoveNonVesselRegions,
    Skeletonize,
    RemoveLooseVessels,
    ExtractMinutiae,
  }
}
