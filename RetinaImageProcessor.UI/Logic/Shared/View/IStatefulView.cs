// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.Logic.Shared.View.IStatefulView
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace RetinaImageProcessor.UI.Logic.Shared.View
{
  public interface IStatefulView : INotifyPropertyChanged
  {
    void Undo_Click(object sender, RoutedEventArgs e);

    void Redo_Click(object sender, RoutedEventArgs e);

    void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e);

    void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e);
  }
}
