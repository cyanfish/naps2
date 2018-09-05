using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
{
    /// <summary>
    /// A singleton class used to track whether the user has made unsaved changes, and therefore should be prompted before exiting.
    /// </summary>
    public class ChangeTracker
    {
        private int counter;

        /// <summary>
        /// Gets a token representing the current state to be used in a subsequent call to Saved.
        /// This correctly handles the situation where the user initiates a save, makes changes on another thread, and then the save completes.
        /// </summary>
        public ChangeState State => new ChangeState { Counter = counter };

        /// <summary>
        /// Indicates that the state has been saved, and sets HasUnsavedChanges to false if the user has not made intermediate changes to the state.
        /// </summary>
        /// <param name="preSaveState"></param>
        public void Saved(ChangeState preSaveState)
        {
            lock (this)
            {
                if (preSaveState.Counter == counter)
                {
                    HasUnsavedChanges = false;
                }
            }
        }

        /// <summary>
        /// Indicates that all state has been purged and sets HasUnsavedChanges to false.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                HasUnsavedChanges = false;
            }
        }

        /// <summary>
        /// Indicates that a change has been made. This invalidates the state and sets HasUnsavedChanges to true.
        /// </summary>
        public void Made()
        {
            lock (this)
            {
                HasUnsavedChanges = true;
                counter++;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user has made changes to the state that haven't been saved.
        /// </summary>
        public bool HasUnsavedChanges { get; private set; }

        public class ChangeState
        {
            internal int Counter { get; set; }
        }
    }
}
