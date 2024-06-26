﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace CalculateX.ViewModels;

[DebuggerDisplay("{Name} ({History.Count})")]
public class WorkspaceViewModel : ObservableObject, IQueryAttributable
{
	public const string QUERY_DATA_WORKSPACE = "workspace";
	public const string QUERY_DATA_DELETE = "delete";
	public const string QUERY_DATA_RENAME = "rename";
	public const string QUERY_DATA_VARIABLES = "variables";
	public const string QUERY_DATA_VARIABLE_NAME = "variable";
	public const string QUERY_DATA_VARIABLE_VALUE = "value";

	private Models.Workspace _workspace;

	public string ID => _workspace.ID;
	public string Name
	{
		get => _workspace.Name;
		set => SetProperty(_workspace.Name, value, _workspace, (model, newValue) => model.Name = newValue);
	}

	public ObservableCollection<Models.Workspace.HistoryEntry> History => _workspace.History;

	public string Input
	{
		get => _input;
		set => SetProperty(ref _input, value);
	}
	private string _input = string.Empty;

	public event EventHandler? WorkspaceChanged;
	public void RaiseWorkspaceChanged() => WorkspaceChanged?.Invoke(this, EventArgs.Empty);

	public RelayCommand EvaluateCommand { get; }
	public ICommand DeleteWorkspaceCommand { get; }
	public ICommand RenameWorkspaceCommand { get; }
	public ICommand VariablesWorkspaceCommand { get; }
	public ICommand HelpCommand { get; }


	/// <summary>
	/// Called when navigating to a workspace.
	/// </summary>
	public WorkspaceViewModel()
	{
		EvaluateCommand = new RelayCommand(Evaluate, Evaluate_CanExecute);
		DeleteWorkspaceCommand = new AsyncRelayCommand(DeleteWorkspaceAsync);
		RenameWorkspaceCommand = new AsyncRelayCommand(RenameWorkspaceAsync);
		VariablesWorkspaceCommand = new AsyncRelayCommand(VariablesWorkspaceAsync);
		HelpCommand = new AsyncRelayCommand(Help);

		// We always go on to ApplyQueryAttributes() to select a workspace.
		// We don't need this workspace, but we do not want to allow null.
		_workspace = new("temporary");
	}

	/// <summary>
	/// Called when loading workspaces.
	/// </summary>
	/// <param name="workspace"></param>
	public WorkspaceViewModel(Models.Workspace workspace)
	{
		EvaluateCommand = new RelayCommand(Evaluate, Evaluate_CanExecute);
		DeleteWorkspaceCommand = new AsyncRelayCommand(DeleteWorkspaceAsync);
		RenameWorkspaceCommand = new AsyncRelayCommand(RenameWorkspaceAsync);
		VariablesWorkspaceCommand = new AsyncRelayCommand(VariablesWorkspaceAsync);
		HelpCommand = new AsyncRelayCommand(Help);

		_workspace = workspace;
	}

	public bool Evaluate_CanExecute() => !string.IsNullOrWhiteSpace(Input);
	private void Evaluate()
	{
		Debug.Assert(!string.IsNullOrWhiteSpace(Input));

		_workspace.Evaluate(Input);

		SemanticScreenReader.Announce(_workspace.Variables[MathExpressions.MathEvaluator.AnswerVariable].ToString());

		// Clear input when we're done.
		Input = string.Empty;

		RaiseWorkspaceChanged();
	}

	/// <summary>
	/// Delete the selection at the cursor, insert the passed
	/// text at the cursor and return the new cursor position.
	/// </summary>
	/// <param name="insertText">Text to insert</param>
	/// <param name="cursorPosition">Current cursor position</param>
	/// <param name="selectionLength">How many characters to replace</param>
	/// <returns>New cursor position</returns>
	public int InsertTextAtCursor(string insertText, int cursorPosition, int selectionLength)
	{
		// Delete selected text and insert new text into the string
		Input = Input
					.Remove(cursorPosition, selectionLength)
					.Insert(cursorPosition, insertText);

		// return cursor position after the inserted variable name
		return cursorPosition + insertText.Length;
	}

	/*
		When a page, or the binding context of a page, implements this interface,
		the query string parameters used in navigation are passed to the
		ApplyQueryAttributes method. This view-model is used as the binding context
		for the view. When the view is navigated to, the view's binding context
		(this view-model) is passed the query string parameters used during navigation.

		This code checks if the load key was provided in the query dictionary.
		If this key is found, the value should be the identifier of the model
		object to load. That note is loaded and set as the underlying model
		object of this view-model instance.
	 */
	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue(QUERY_DATA_WORKSPACE, out object? value))
		{
			Debug.Assert(value is not null);

			WorkspaceViewModel viewModel = (WorkspaceViewModel)value;

			// KLUDGE XAML creates a view-model using the default ctor, so the
			// workspaces view-model does not have a chance to add an event handler.
			WorkspaceChanged += viewModel.WorkspaceChanged;

			_workspace = viewModel._workspace;

			// We need to refresh anything based on _workspace.
			RefreshProperties();

			// https://stackoverflow.com/q/73755717/4858
			query.Clear();
		}
	}

	private void RefreshProperties()
	{
		OnPropertyChanged(nameof(Name));
		OnPropertyChanged(nameof(History));
	}

	private async Task DeleteWorkspaceAsync()
	{
		await Shell.Current.GoToAsync($"..?{QUERY_DATA_DELETE}={_workspace.ID}");
	}

	private async Task RenameWorkspaceAsync()
	{
		await Shell.Current.GoToAsync($"..?{QUERY_DATA_RENAME}={_workspace.ID}");
	}

	private async Task VariablesWorkspaceAsync()
	{
#if !MAUI_UNITTESTS
		// Navigate to page that displays the variables from this workspace
		await Shell.Current.GoToAsync(nameof(Views.VariablesPage),
			new Dictionary<string, object>()
			{
				{
					QUERY_DATA_VARIABLES,
					_workspace.Variables
				}
			});
#endif
	}

	private async Task Help()
	{
#if !MAUI_UNITTESTS
		await Shell.Current.GoToAsync(nameof(Views.HelpPage));
#endif
	}
}
