using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraEditor.Common
{
    public interface IReUndoCommand
    {
        void ExecuteRedo();
        void ExecuteUndo();
    }
    
    /* The Invoker class */
    public class ReUndoManager
    {
        static private ReUndoManager _self;
        private readonly Stack<IReUndoCommand> RedoStack;
        private readonly Stack<IReUndoCommand> UndoStack;
        bool _mutex;
        
        private ReUndoManager()
        {
            RedoStack = new Stack<IReUndoCommand>();
            UndoStack = new Stack<IReUndoCommand>();
            _mutex = false;
        }

        static public ReUndoManager GetInstance()
        {
            if (_self == null)
                _self = new ReUndoManager();

            return _self;
        }
        
        public void Store(IReUndoCommand command)
        {
            if (_mutex == true)
                return;

            UndoStack.Push(command);
            RedoStack.Clear();
        }

        public void Redo()
        {
            if (RedoStack.Count == 0)
                return;

            _mutex = true;
            IReUndoCommand command =  RedoStack.Pop();
            command.ExecuteRedo();
            UndoStack.Push(command);
            _mutex = false;
        }
        public void Undo()
        {
            if (UndoStack.Count == 0)
                return;

            _mutex = true;
            IReUndoCommand command = UndoStack.Pop();
            command.ExecuteUndo();
            RedoStack.Push(command);
            _mutex = false;
        }
        public void Clear()
        {
            RedoStack.Clear();
            UndoStack.Clear();
            _mutex = false;
        }
    }
}
