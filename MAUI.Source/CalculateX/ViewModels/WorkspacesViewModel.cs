﻿using CalculateX.Models;
using CommunityToolkit.Mvvm.Input;
using Shared;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CalculateX.ViewModels;

public class WorkspacesViewModel
{
	private readonly Workspaces _workspaces;
	public ObservableCollection<WorkspaceViewModel> TheWorkspaceViewModels { get; private init; }
	private Func<WorkspaceViewModel, WorkspaceViewModel, bool> SortByName = (WorkspaceViewModel item1, WorkspaceViewModel item2) => (item1.Name.CompareTo(item2.Name) < 0);

	private string? _selectedWorkspaceID;

	private int _windowId = 0;

	public ICommand AddWorkspaceCommand { get; }
	public ICommand AboutCommand { get; }
	public ICommand SelectWorkspaceCommand { get; }


	public WorkspacesViewModel(string pathWorkspacesFile)
	{
		_workspaces = new(pathWorkspacesFile);

		AddWorkspaceCommand = new RelayCommand(AddWorkspace);
		AboutCommand = new AsyncRelayCommand(About);
		SelectWorkspaceCommand = new AsyncRelayCommand<WorkspaceViewModel>(SelectWorkspaceAsync);

		TheWorkspaceViewModels = new(
			_workspaces.TheWorkspaces
			.Select(model => new WorkspaceViewModel(model))
			.OrderBy(vm => vm.Name)
		);
		foreach (var workspace in TheWorkspaceViewModels)
		{
			workspace.WorkspaceChanged += OnWorkspaceChanged;
		}

		if (!TheWorkspaceViewModels.Any())
		{
			AddWorkspace();
			SaveWorkspaces();
		}
	}

	private void AddWorkspace()
	{
		Workspace newWorkspace = new(FormWorkspaceName());
		_workspaces.AddWorkspace(newWorkspace);

		WorkspaceViewModel viewModel = new(newWorkspace);
		viewModel.WorkspaceChanged += OnWorkspaceChanged;
		TheWorkspaceViewModels.AddSorted(viewModel, SortByName);

		_selectedWorkspaceID ??= newWorkspace.ID;
	}

	private async Task About()
	{
#if !MAUI_UNITTESTS
		await Shell.Current.GoToAsync(nameof(Views.AboutPage));
#endif
	}

	public void SaveWorkspaces() => _workspaces.SaveWorkspaces(_selectedWorkspaceID);

	public void DeleteWorkspace(WorkspaceViewModel workspaceVM)
	{
		// Delete workspace model
		_workspaces.DeleteWorkspace(workspaceVM.ID);

		// Delete workspace view-model
		TheWorkspaceViewModels.Remove(workspaceVM);

		// If closing last workspace, create one.
		if (TheWorkspaceViewModels.Count == 0)
		{
			AddWorkspace();
		}

		SaveWorkspaces();
	}

	public void RenameWorkspace(WorkspaceViewModel workspaceVM, string name)
	{
		workspaceVM.Name = name;

		// If the sorted location is different, move the workspace VM to the sorted location.
		int ixSorted = TheWorkspaceViewModels.FindSortedIndex(workspaceVM, SortByName);
		int ixCur = TheWorkspaceViewModels.IndexOf(workspaceVM);
		if (ixSorted != ixCur)
		{
			// The destination index is the position within the existing list,
			// so we have to pretend the item has been removed from the list.
			if (ixSorted > ixCur)
			{
				--ixSorted;
			}

			// If they're still different, move the view-model.
			if (ixSorted != ixCur)
			{
				TheWorkspaceViewModels.Move(ixCur, ixSorted);
			}
		}

		SaveWorkspaces();
	}

	private void OnWorkspaceChanged(object? sender, EventArgs e) => SaveWorkspaces();

	public async Task SelectWorkspaceAsync(WorkspaceViewModel? workspaceVM)
	{
		if (workspaceVM is null)
		{
			return;
		}

		_selectedWorkspaceID = workspaceVM.ID;

#if !MAUI_UNITTESTS
		// Should navigate to "NotePage?ItemId=path\on\device\XYZ.notes.txt"
		//Shell.Current.GoToAsync($"{nameof(Views.WorkspacePage)}?select={workspace.ID}");
		await Shell.Current.GoToAsync(nameof(Views.WorkspacePage),
			new Dictionary<string, object>()
			{
				{
					WorkspaceViewModel.QUERY_DATA_WORKSPACE,
					TheWorkspaceViewModels.First(workspaceView => workspaceView.ID == workspaceVM.ID)
				}
			});
#endif
	}

	private string FormWorkspaceName()
	{
		// If we have closed the last tab, reset the window ID.
		// (This prevents the ID from incrementing when we close the last tab.)
		if (!_workspaces.TheWorkspaces.Any())
		{
			_windowId = 0;
		}

		IEnumerable<string> bannedNames = TheWorkspaceViewModels.Select(w => w.Name);

		string name = string.Empty;
		do
		{
			++_windowId;
			name = $"{nameof(Workspace)}{_windowId}";
		} while (bannedNames.Any(n => n == name));

		return name;
	}
}
