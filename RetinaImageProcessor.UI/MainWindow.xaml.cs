// Decompiled with JetBrains decompiler
// Type: RetinaImageProcessor.UI.MainWindow
// Assembly: RetinaImageProcessor.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B95127C7-816D-47CE-8C3F-D69C34AD1109
// Assembly location: X:\GitHub\Biometria\Aplikacja\RetinaImageProcessor.UI.exe

using RetinaImageProcessor.UI.Logic.MatchedFilters.View;
using RetinaImageProcessor.UI.Logic.RegionFeatures.View;
using RetinaImageProcessor.UI.Logic.Shared.View;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace RetinaImageProcessor.UI
{
  public partial class MainWindow : Window, IComponentConnector
  {
    private int lastIndex = -1;
    private CommandBinding undoBinding;
    private CommandBinding redoBinding;
    internal Grid myPanel;
    internal TabControl TabControl;
    private bool _contentLoaded;

    public MainWindow()
    {
      this.InitializeComponent();
      this.WindowState = WindowState.Maximized;
      this.undoBinding = new CommandBinding();
      this.undoBinding.Command = (ICommand) ApplicationCommands.Undo;
      this.CommandBindings.Add(this.undoBinding);
      this.redoBinding = new CommandBinding();
      this.redoBinding.Command = (ICommand) ApplicationCommands.Redo;
      this.CommandBindings.Add(this.redoBinding);
      this.AddNewMatchedFiltersTab();
      this.AddNewRegionFeaturesTab();
    }

    private void AddNewMatchedFiltersTab()
    {
      MatchedFiltersView matchedFiltersView = new MatchedFiltersView();
      TabItem tabItem1 = new TabItem();
      tabItem1.Header = (object) "Matched Filters";
      tabItem1.FontWeight = FontWeights.Bold;
      TabItem tabItem2 = tabItem1;
      tabItem2.Content = (object) matchedFiltersView;
      ContextMenu contextMenu = new ContextMenu();
      MenuItem menuItem1 = new MenuItem();
      menuItem1.Header = (object) "New Matched Filters Tab";
      MenuItem menuItem2 = menuItem1;
      contextMenu.Items.Add((object) menuItem2);
      menuItem2.Click += new RoutedEventHandler(this.NewMatchedFiltersButton_Click);
      tabItem2.ContextMenu = contextMenu;
      this.TabControl.Items.Add((object) tabItem2);
    }

    private void NewMatchedFiltersButton_Click(object sender, RoutedEventArgs e)
    {
      this.AddNewMatchedFiltersTab();
    }

    private void AddNewRegionFeaturesTab()
    {
      RegionFeaturesView regionFeaturesView = new RegionFeaturesView();
      TabItem tabItem1 = new TabItem();
      tabItem1.Header = (object) "Region Features";
      tabItem1.FontWeight = FontWeights.Bold;
      TabItem tabItem2 = tabItem1;
      tabItem2.Content = (object) regionFeaturesView;
      ContextMenu contextMenu = new ContextMenu();
      MenuItem menuItem1 = new MenuItem();
      menuItem1.Header = (object) "New Region Features Tab";
      MenuItem menuItem2 = menuItem1;
      contextMenu.Items.Add((object) menuItem2);
      menuItem2.Click += new RoutedEventHandler(this.NewRegionFeaturesButton_Click);
      tabItem2.ContextMenu = contextMenu;
      this.TabControl.Items.Add((object) tabItem2);
    }

    private void NewRegionFeaturesButton_Click(object sender, RoutedEventArgs e)
    {
      this.AddNewRegionFeaturesTab();
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.undoBinding == null || this.redoBinding == null)
        return;
      if (this.lastIndex != this.TabControl.SelectedIndex)
      {
        if (this.lastIndex != -1)
        {
          IStatefulView content = (IStatefulView) ((ContentControl) this.TabControl.Items[this.lastIndex]).Content;
          this.undoBinding.Executed -= new ExecutedRoutedEventHandler(content.Undo_Click);
          this.undoBinding.CanExecute -= new CanExecuteRoutedEventHandler(content.Undo_CanExecute);
          this.redoBinding.Executed -= new ExecutedRoutedEventHandler(content.Redo_Click);
          this.redoBinding.CanExecute -= new CanExecuteRoutedEventHandler(content.Redo_CanExecute);
        }
        IStatefulView content1 = (IStatefulView) ((ContentControl) this.TabControl.Items[this.TabControl.SelectedIndex]).Content;
        this.undoBinding.Executed += new ExecutedRoutedEventHandler(content1.Undo_Click);
        this.undoBinding.CanExecute += new CanExecuteRoutedEventHandler(content1.Undo_CanExecute);
        this.redoBinding.Executed += new ExecutedRoutedEventHandler(content1.Redo_Click);
        this.redoBinding.CanExecute += new CanExecuteRoutedEventHandler(content1.Redo_CanExecute);
      }
      this.lastIndex = this.TabControl.SelectedIndex;
      CommandManager.InvalidateRequerySuggested();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/RetinaImageProcessor.UI;component/mainwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.myPanel = (Grid) target;
          break;
        case 2:
          this.TabControl = (TabControl) target;
          this.TabControl.SelectionChanged += new SelectionChangedEventHandler(this.TabControl_SelectionChanged);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
