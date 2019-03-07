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
        }
        static public void Redo()
        {
            if (RedoStack.Count == 0)
                return;

            _mutex = true;
            IReUndoCommand command =  RedoStack.Pop();
            command.ExecuteRedo();
            UndoStack.Push(command);
            _mutex = false;

            if(command is MoveEffectCommand c)
            {
                if (c.Conflict())
                    Redo();
            }
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
        }
        static public void Clear()
        {
            RedoStack.Clear();
            UndoStack.Clear();
            _mutex = false;
        }
    }
}
