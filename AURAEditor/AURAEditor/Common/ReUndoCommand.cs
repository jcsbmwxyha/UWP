using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuraEditor.UserControls.EffectLine;

namespace AuraEditor.Common
{
    public interface IReUndoCommand
    {
        void ExecuteRedo();
        void ExecuteUndo();
    }

    /* The Invoker class */
    static public class ReUndoManager
    {
        #region RelayCommand
        static private RelayCommand undoCommand = null;
        static public RelayCommand UndoCommand
        {
            get { return (undoCommand ?? (undoCommand = new RelayCommand(ReUndoManager.Undo, ReUndoManager.CanUndo))); }
        }
        static private RelayCommand redoCommand = null;
        static public RelayCommand RedoCommand
        {
            get { return (redoCommand ?? (redoCommand = new RelayCommand(ReUndoManager.Redo, ReUndoManager.CanRedo))); }
        }

        static public bool CanRedo()
        {
            return RedoStack.Count != 0;
        }
        static public bool CanUndo()
        {
            return UndoStack.Count != 0;
        }

        static private void RaiseCanExecute()
        {
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }
        #endregion

        static public IReUndoCommand CurUndoCommand { get { return UndoStack.Count == 0 ? null : UndoStack.Peek(); } }
        static private readonly Stack<IReUndoCommand> RedoStack;
        static private readonly Stack<IReUndoCommand> UndoStack;
        static private bool _mutex;

        static ReUndoManager()
        {
            RedoStack = new Stack<IReUndoCommand>();
            UndoStack = new Stack<IReUndoCommand>();
            _mutex = false;
        }

        static public void Store(IReUndoCommand command)
        {
            if (_mutex == true)
                return;

            UndoStack.Push(command);
            RedoStack.Clear();
            RaiseCanExecute();
        }
        static public void Redo()
        {
            if (RedoStack.Count == 0)
                return;

            _mutex = true;
            IReUndoCommand command = RedoStack.Pop();
            command.ExecuteRedo();
            UndoStack.Push(command);
            _mutex = false;

            if (command is MoveEffectCommand c)
            {
                if (c.Conflict())
                    Redo();
            }

            RaiseCanExecute();
        }
        static public void Undo()
        {
            if (UndoStack.Count == 0)
                return;

            _mutex = true;
            IReUndoCommand command = UndoStack.Pop();
            command.ExecuteUndo();
            RedoStack.Push(command);
            _mutex = false;

            if (command is MoveEffectCommand c)
            {
                if (c.Conflict())
                    Undo();
            }

            RaiseCanExecute();
        }
        static public void Clear()
        {
            RedoStack.Clear();
            UndoStack.Clear();
            _mutex = false;
            RaiseCanExecute();
        }
        static public void Enable()
        {
            _mutex = false;
        }
        static public void Disable()
        {
            _mutex = true;
        }
    }
}
