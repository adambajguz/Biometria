// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.RegionFeatures.View.StateRegionFeatures
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using System.Collections.Generic;
using System.Drawing;

namespace RetinaImageProcessor.UI.Logic.RegionFeatures.View
{
  public class StateRegionFeatures
  {
    public StateOperations lastOperation;
    public Bitmap current;
    public Bitmap previous1;
    public Bitmap previous2;
    public Bitmap maskState;
    public Bitmap preliminaryVessels;
    public Bitmap undeterminedRegions;
    public Bitmap topHat;
    public List<Bitmap> topHats;

    public StateRegionFeatures()
    {
    }

    public StateRegionFeatures(StateRegionFeatures state, bool mask)
    {
      if (mask)
      {
        this.current = state.current;
        this.previous1 = state.previous1;
        this.previous2 = state.previous2;
      }
      else
      {
        this.previous1 = state.current;
        this.previous2 = state.previous1;
        this.maskState = state.maskState;
      }
      this.preliminaryVessels = state.preliminaryVessels;
      this.undeterminedRegions = state.undeterminedRegions;
      this.topHat = state.topHat;
      this.topHats = state.topHats;
      this.lastOperation = state.lastOperation;
    }
  }
}
