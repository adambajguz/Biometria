// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.MatchedFilters.View.StateMatchedFilters
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using System.Collections.Generic;
using System.Drawing;

namespace RetinaImageProcessor.UI.Logic.MatchedFilters.View
{
  public class StateMatchedFilters
  {
    public StateOperations lastOperation;
    public Bitmap current;
    public Bitmap previous1;
    public Bitmap previous2;
    public Bitmap maskState;
    public Bitmap coOccurence;
    public List<Bitmap> gaussians;

    public StateMatchedFilters()
    {
    }

    public StateMatchedFilters(StateMatchedFilters state, bool mask)
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
      this.coOccurence = state.coOccurence;
      this.gaussians = state.gaussians;
      this.lastOperation = state.lastOperation;
    }
  }
}
