﻿using CalculateX.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace CalculateX.ViewModels;

public class WorkspacesViewModel : ObservableObject
{
	private readonly Workspaces _workspaces;
	public ObservableCollection<WorkspaceViewModel> TheWorkspaceViewModels { get; private init; }

	public WorkspaceViewModel SelectedWorkspaceVM
	{
		get => _selectedWorkspaceVM;
		set => SetProperty(ref _selectedWorkspaceVM, value);
	}
	private WorkspaceViewModel _selectedWorkspaceVM;

	private int _windowNumber = 0;

	//TODO: [RelayCommand] https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/relaycommand
	/// <summary>
	/// Commands for this view-model.
	/// </summary>
	public ICommand NewWorkspaceCommand { get; }
	public RelayCommand SelectPreviousWorkspaceCommand { get; }
	public RelayCommand SelectNextWorkspaceCommand { get; }


	public WorkspacesViewModel(string pathWorkspacesFile)
	{
		_workspaces = new(pathWorkspacesFile);

		NewWorkspaceCommand = new RelayCommand(NewWorkspace);
		SelectPreviousWorkspaceCommand = new RelayCommand(SelectPreviousWorkspace, SelectPreviousWorkspace_CanExecute);
		SelectNextWorkspaceCommand = new RelayCommand(SelectNextWorkspace, SelectNextWorkspace_CanExecute);

//TODO: Handle CollectionChanged and add the below handlers for NewItems and remove them for OldItems (see MSJ article)
//Have to create empty collection first and THEN add vms to it. Use Lazy<>?
		TheWorkspaceViewModels = new(_workspaces.TheWorkspaces.Select(model => new WorkspaceViewModel(model)));
		foreach (var viewModel in TheWorkspaceViewModels)
		{
			SubscribeViewModelEvents(viewModel);
		}

		if (!TheWorkspaceViewModels.Any())
		{
			WorkspaceViewModel newViewModel = CreateWorkspace();
			TheWorkspaceViewModels.Insert(0, newViewModel);
			SelectedWorkspaceVM = newViewModel;
		}

		// Add the "+" tab to the view-model but not to the model.
		TheWorkspaceViewModels.Add(new(canCloseTab: false));

		SelectPreviousWorkspaceCommand.NotifyCanExecuteChanged();
		SelectNextWorkspaceCommand.NotifyCanExecuteChanged();

		_selectedWorkspaceVM = TheWorkspaceViewModels.FirstOrDefault(vm => vm.ID == _workspaces.LoadedSelectedWorkspaceID) ?? TheWorkspaceViewModels.First();
	}

	private void SubscribeViewModelEvents(WorkspaceViewModel viewModel)
	{
		viewModel.RequestDelete += OnRequestDelete;
		viewModel.WorkspaceChanged += OnWorkspaceChanged;
	}

	public void SaveWorkspaces() => _workspaces.SaveWorkspaces(SelectedWorkspaceVM.ID);

	private void OnWorkspaceChanged(object? sender, EventArgs e) => SaveWorkspaces();

	public void SelectWorkspace()
	{
		/// If the user selected a closable tab, select it.
		if (SelectedWorkspaceVM.CanCloseTab)
		{
			SaveWorkspaces();
			return;
		}

		/// The user selected the "+" tab...
		/// Make new closable tab and insert before "+" tab.
		WorkspaceViewModel newViewModel = CreateWorkspace();
		TheWorkspaceViewModels.Insert(TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM), newViewModel);
		SelectedWorkspaceVM = newViewModel;

		SelectPreviousWorkspaceCommand.NotifyCanExecuteChanged();
		SelectNextWorkspaceCommand.NotifyCanExecuteChanged();

		SaveWorkspaces();
	}

	/// <summary>
	/// This is called when the user presses a shortcut to create a new workspace.
	/// </summary>
	private void NewWorkspace()
	{
		Debug.Assert(SelectedWorkspaceVM is not null);
		Debug.Assert(TheWorkspaceViewModels.Any(w => w.CanCloseTab));
		Debug.Assert(SelectNextWorkspace_CanExecute());

		WorkspaceViewModel newViewModel = CreateWorkspace();

		TheWorkspaceViewModels.Insert(TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM) + 1, newViewModel);

		SelectNextWorkspace();
	}

	private WorkspaceViewModel CreateWorkspace()
	{
		Workspace newWorkspace = new(FormWorkspaceName());
		_workspaces.AddWorkspace(newWorkspace);

		WorkspaceViewModel viewModel = new(newWorkspace);
		SubscribeViewModelEvents(viewModel);

		return viewModel;
	}

	private void OnRequestDelete(object? /*WorkspaceViewModel*/ sender, EventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);

		WorkspaceViewModel deletedWorkspaceVM = (WorkspaceViewModel)sender;

		// Delete workspace model
		/// Note: We must remove the workspace model first so <see cref="FormWorkspaceName"/> knows to reset the workspaceNumber.
		_workspaces.DeleteWorkspace(deletedWorkspaceVM.ID);

		if (deletedWorkspaceVM.ID == SelectedWorkspaceVM.ID)
		{
			int indexSelectedWorkspace = TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM);
			bool isRightmostTab = (deletedWorkspaceVM.ID == TheWorkspaceViewModels[^2].ID);

			/// If closing last workspace, create one.
			if (!_workspaces.TheWorkspaces.Any())
			{
				Debug.Assert(TheWorkspaceViewModels.Count == 2);

				/// [deleted][+]
				WorkspaceViewModel newViewModel = CreateWorkspace();
				TheWorkspaceViewModels.Insert(0, newViewModel);
				/// [new][deleted][+]
			}

			/// Select the workspace we want to be selected AFTER we delete the workspace.

			/// Select tab that will be in the same position (unless it's the "+" tab, then select last tab).
			if (isRightmostTab)
			{
				/// [a]...[z][deleted][+]
				SelectedWorkspaceVM = TheWorkspaceViewModels[^3];
			}
			else
			{
				/// [a]...[m][deleted][n]...[z][+]
				SelectedWorkspaceVM = TheWorkspaceViewModels[indexSelectedWorkspace + 1];
			}
		}
		else
		{
			// Note: No need to change the selected workspace.
		}

		// Note: We must remove the view-model last because the binding changes the selected tab.
		// Delete workspace view-model
		TheWorkspaceViewModels.Remove(deletedWorkspaceVM);

		SelectPreviousWorkspaceCommand.NotifyCanExecuteChanged();
		SelectNextWorkspaceCommand.NotifyCanExecuteChanged();

		SaveWorkspaces();
	}

	private bool SelectPreviousWorkspace_CanExecute()
	{
		return (TheWorkspaceViewModels.Any(w => w.CanCloseTab));	// Do not count the "+" tab
	}
	private void SelectPreviousWorkspace()
	{
		Debug.Assert(SelectedWorkspaceVM is not null);
		Debug.Assert(TheWorkspaceViewModels.Any(w => w.CanCloseTab));

		if (SelectedWorkspaceVM == TheWorkspaceViewModels.First(w => w.CanCloseTab))
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels.Last(w => w.CanCloseTab);
		}
		else
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels[TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM) - 1];
		}
	}

	private bool SelectNextWorkspace_CanExecute()
	{
		return (TheWorkspaceViewModels.Any(w => w.CanCloseTab));	// Do not count the "+" tab
	}
	private void SelectNextWorkspace()
	{
		Debug.Assert(SelectedWorkspaceVM is not null);
		Debug.Assert(TheWorkspaceViewModels.Any(w => w.CanCloseTab));

		if (SelectedWorkspaceVM == TheWorkspaceViewModels.Last(w => w.CanCloseTab))
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels.First(w => w.CanCloseTab);
		}
		else
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels[TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM) + 1];
		}
	}

	private string FormWorkspaceName()
	{
		// If we have closed the last tab, reset the window ID.
		// (This prevents the ID from incrementing when we close the last tab.)
		if (!_workspaces.TheWorkspaces.Any())
		{
			_windowNumber = 0;
		}

		IEnumerable<string> bannedNames = _workspaces.TheWorkspaces.Select(w => w.Name);

		string name = string.Empty;
		do
		{
			++_windowNumber;
			name = $"{nameof(Workspace)}{_windowNumber}";
		} while (bannedNames.Any(n => n == name));

		return name;
	}
}
