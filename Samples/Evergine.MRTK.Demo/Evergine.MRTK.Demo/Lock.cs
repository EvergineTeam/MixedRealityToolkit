namespace Evergine.MRTK.Demo
{
    public struct LockCounter
    {
        public event System.Action OnLock;
        public event System.Action OnUnlock;

        private int locked;

        public bool IsLocked
        {
            get
            {
                return locked != 0;
            }
        }

        public void Lock()
        {
            locked++;
            if (locked == 1)
                OnLock?.Invoke();
        }

        public void Unlock()
        {
            if (locked > 0)
            {
                locked--;
                if (locked == 0)
                    OnUnlock?.Invoke();
            }
        }
    }
}
