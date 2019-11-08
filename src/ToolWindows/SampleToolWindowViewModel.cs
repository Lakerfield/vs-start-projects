using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using AsyncToolWindowSample.Tools;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MessageBox = System.Windows.MessageBox;

namespace AsyncToolWindowSample.ToolWindows
{
  public class SampleToolWindowViewModel
  {
    public ObservableCollection<ProjectViewModel> Projects { get; set; }

    public SampleToolWindowViewModel()
    {
      Projects = new ObservableCollection<ProjectViewModel>();
    }
  }

  public class ProjectViewModel
  {
    private readonly Project _project;
    public string Name { get; set; }

    public ProjectViewModel(Project project)
    {
      _project = project;
      Name = project?.Name ?? "- Unknown -";
    }

    private ICommand _runCommand;
    public ICommand RunCommand => _runCommand ?? (_runCommand = new CommandHandler(Run, () => CanRunExecute));
    public bool CanRunExecute => true;
    public void Run()
    {
      //MessageBox.Show($"Running {Name}");
      BuildAndRun();
    }

    private ICommand _debugCommand;
    public ICommand DebugCommand => _debugCommand ?? (_debugCommand = new CommandHandler(Debug, () => CanDebugExecute));
    public bool CanDebugExecute => true;
    public void Debug()
    {
      MessageBox.Show($"Running {Name}");
    }

    public void BuildAndRun()
    {
      //https://github.com/vurdalakov/startwithoutdebugging/blob/master/src/StartWithoutDebugging/SolutionExplorerCommand.cs
      var project = _project;
      var dte2 = project.DTE;

      if (dbgDebugMode.dbgDesignMode == dte2.Debugger.CurrentMode)
      {
        var solutionBuild = project.DTE.Solution.SolutionBuild;
        solutionBuild.BuildProject(solutionBuild.ActiveConfiguration.Name, project.UniqueName, true);
      }

      // get project start options

      var activeConfigurationProperties = project.ConfigurationManager.ActiveConfiguration.Properties;

      var executableFilePath = activeConfigurationProperties.GetValue<string>("StartProgram"); // not null if "Start external program" is set
      if (string.IsNullOrWhiteSpace(executableFilePath))
      {
        // otherwise create executable file path from project properties
        var projectProperties = project.Properties;
        var fullPath = projectProperties.GetValue<string>("FullPath");
        var outputPath = activeConfigurationProperties.GetValue<string>("OutputPath");
        var outputFileName = projectProperties.GetValue<string>("OutputFileName");
        executableFilePath = Path.Combine(fullPath, outputPath, outputFileName);
      }

      var workingDirectory = activeConfigurationProperties.GetValue<string>("StartWorkingDirectory");
      if (string.IsNullOrWhiteSpace(workingDirectory))
      {
        workingDirectory = Path.GetDirectoryName(executableFilePath);
      }

      var arguments = activeConfigurationProperties.GetValue<string>("StartArguments");

      if (executableFilePath.EndsWith(".dll"))
      {
        var tempExecutableFilePath = executableFilePath.Substring(0, executableFilePath.Length - 3) + "exe";
        if (File.Exists(tempExecutableFilePath))
          executableFilePath = tempExecutableFilePath;
        else
        {
          arguments = executableFilePath;
          executableFilePath = "dotnet";
        }
      }

      try
      {
        var processStartInfo = new ProcessStartInfo(executableFilePath, arguments)
        {
          WorkingDirectory = workingDirectory,
        };

        System.Diagnostics.Process.Start(processStartInfo);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Cannot start process: {ex.Message}", "Start without debugging");
      }
    }

  }
}
