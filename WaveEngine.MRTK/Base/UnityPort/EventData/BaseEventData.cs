// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Framework;

namespace WaveEngine.EventSystems
{
    public class BaseEventData
    {
        private readonly EventSystem eventSystem;
        private bool used;

        public BaseEventData(EventSystem eventSystem)
        {
            this.eventSystem = eventSystem;
        }

        public void Reset()
        {
           this.used = false;
        }

        public void Use()
        {
            this.used = true;
        }

        public bool Used => this.used;

        public BaseInputModule currentInputModule
        {
            get { return eventSystem.currentInputModule; }
        }

        public Entity selectedObject
        {
            get => eventSystem.currentSelectedGameObject;
            set
            {
                eventSystem.SetSelectedGameObject(value, this);
            }
        }
    }
}