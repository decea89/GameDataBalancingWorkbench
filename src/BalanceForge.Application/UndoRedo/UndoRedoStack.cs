namespace BalanceForge.Application.UndoRedo;

/// <summary>
/// Service managing undo and redo stacks for unit edits.
/// When a new edit is pushed, the redo stack is cleared.
/// Raises StackChanged event when the undo/redo state changes.
/// </summary>
public class UndoRedoStack
{
    private readonly Stack<UnitEditCommand> _undoStack = new();
    private readonly Stack<UnitEditCommand> _redoStack = new();

    /// <summary>
    /// Raised when the stack state changes (undo, redo, or push operation).
    /// Subscribers can check CanUndo/CanRedo to update UI state.
    /// </summary>
    public event EventHandler? StackChanged;

    /// <summary>
    /// Gets a value indicating whether there are edits to undo.
    /// </summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>
    /// Gets a value indicating whether there are edits to redo.
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Gets the number of edits in the undo stack.
    /// </summary>
    public int UndoCount => _undoStack.Count;

    /// <summary>
    /// Gets the number of edits in the redo stack.
    /// </summary>
    public int RedoCount => _redoStack.Count;

    /// <summary>
    /// Pushes a new edit onto the undo stack and clears the redo stack.
    /// Raises StackChanged event.
    /// </summary>
    /// <param name="command">The edit command to record.</param>
    /// <exception cref="ArgumentNullException">Thrown if command is null.</exception>
    public void Push(UnitEditCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        _undoStack.Push(command);
        _redoStack.Clear();
        OnStackChanged();
    }

    /// <summary>
    /// Pops an edit from the undo stack and returns it.
    /// Returns null if the undo stack is empty.
    /// Raises StackChanged event if an edit was popped.
    /// </summary>
    public UnitEditCommand? Undo()
    {
        if (!CanUndo)
        {
            return null;
        }

        var command = _undoStack.Pop();
        _redoStack.Push(command);
        OnStackChanged();
        return command;
    }

    /// <summary>
    /// Pops an edit from the redo stack and returns it.
    /// Returns null if the redo stack is empty.
    /// Raises StackChanged event if an edit was popped.
    /// </summary>
    public UnitEditCommand? Redo()
    {
        if (!CanRedo)
        {
            return null;
        }

        var command = _redoStack.Pop();
        _undoStack.Push(command);
        OnStackChanged();
        return command;
    }

    /// <summary>
    /// Clears both undo and redo stacks.
    /// Raises StackChanged event.
    /// </summary>
    public void Clear()
    {
        var hadContent = _undoStack.Count > 0 || _redoStack.Count > 0;
        _undoStack.Clear();
        _redoStack.Clear();
        if (hadContent)
        {
            OnStackChanged();
        }
    }

    /// <summary>
    /// Peeks at the top edit in the undo stack without removing it.
    /// Returns null if the undo stack is empty.
    /// </summary>
    public UnitEditCommand? PeekUndo() => _undoStack.Count > 0 ? _undoStack.Peek() : null;

    /// <summary>
    /// Peeks at the top edit in the redo stack without removing it.
    /// Returns null if the redo stack is empty.
    /// </summary>
    public UnitEditCommand? PeekRedo() => _redoStack.Count > 0 ? _redoStack.Peek() : null;

    protected virtual void OnStackChanged()
    {
        StackChanged?.Invoke(this, EventArgs.Empty);
    }
}
