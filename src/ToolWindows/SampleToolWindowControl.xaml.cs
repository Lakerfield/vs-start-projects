using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EnvDTE;

namespace AsyncToolWindowSample.ToolWindows
{
  public partial class SampleToolWindowControl : UserControl
  {
    private SampleToolWindowState _state;

    public SampleToolWindowViewModel ViewModel
    {
      get => (SampleToolWindowViewModel) DataContext;
      set => DataContext = value;
    }

    public SampleToolWindowControl(SampleToolWindowState state)
    {
      _state = state;

      InitializeComponent();

      ViewModel = new SampleToolWindowViewModel();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      string version = _state.DTE.FullName;

      MessageBox.Show($"Visual Studio is located here: '{version}'");
    }

    private void Button2_Click(object sender, RoutedEventArgs e)
    {
      var projects = new List<ProjectViewModel>();
      foreach (Project project in GetProjects(_state.DTE.Solution))
      {
        //https://docs.microsoft.com/en-us/dotnet/api/vslangproj.prjoutputtype?view=visualstudiosdk-2017
        var outputType = GetOutputType(project);
        if (outputType != "0" && outputType != "1")
          continue;

        //MessageBox.Show($"{project.Name} - {project.FullName}");
        projects.Add(new ProjectViewModel(project));
      }

      var viewModel = ViewModel;
      viewModel.Projects.Clear();
      foreach (var project in projects.OrderBy(p => p.Name))
        viewModel.Projects.Add(project);
    }

    /// <summary>
    /// Queries for all projects in solution, recursively (without recursion)
    /// </summary>
    /// <param name="sln">Solution</param>
    /// <returns>List of projects</returns>
    static List<Project> GetProjects(Solution sln)
    {
      List<Project> list = new List<Project>();
      list.AddRange(sln.Projects.Cast<Project>());

      for (int i = 0; i < list.Count; i++)
        // OfType will ignore null's.
        list.AddRange(list[i].ProjectItems.Cast<ProjectItem>().Select(x => x.SubProject).OfType<Project>());

      return list;
    }

    static string GetOutputType(Project project)
    {
      var properties = project.Properties;
      if (properties == null)
        return null;

      foreach (Property p in properties)
        if (string.Equals(p.Name, "OutputType", StringComparison.OrdinalIgnoreCase))
          return p?.Value?.ToString();
      return null;
    }

  }
}
