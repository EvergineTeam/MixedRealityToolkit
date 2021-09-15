// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System.Collections.Generic;
using System.Linq;
using WaveEngine.Framework;
using WaveEngine.Framework.Managers;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;
using WaveEngine.MRTK.Emulation;

namespace WaveEngine.MRTK.Services.InputSystem
{
    /// <summary>
    /// A <see cref="SceneManager"/> to fire speech events for speech handlers.
    /// </summary>
    public class VoiceCommandsProvider : SceneManager
    {
        [BindService(isRequired: false)]
        private IVoiceCommandService voiceCommandsService = null;

        private HashSet<IMixedRealitySpeechHandler> speechHandlers;

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            if (this.voiceCommandsService != null)
            {
                this.voiceCommandsService.CommandRecognized += this.VoiceCommandsService_CommandRecognized;

                this.Managers.EntityManager.EntityAdded += this.EntityManager_EntityAdded;
                this.Managers.EntityManager.EntityDetached += this.EntityManager_EntityDetached;

                var speechHandlersList = this.Managers.EntityManager.AllEntities
                    .SelectMany(e => this.FindSpeechHandlersInEntity(e))
                    .ToList();
                this.speechHandlers = new HashSet<IMixedRealitySpeechHandler>(speechHandlersList);
            }
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            if (this.voiceCommandsService != null)
            {
                this.voiceCommandsService.CommandRecognized -= this.VoiceCommandsService_CommandRecognized;

                this.Managers.EntityManager.EntityAdded -= this.EntityManager_EntityAdded;
                this.Managers.EntityManager.EntityDetached -= this.EntityManager_EntityDetached;

                this.speechHandlers = null;
            }
        }

        private void EntityManager_EntityAdded(object sender, Entity entity)
        {
            foreach (var handler in this.FindSpeechHandlersInEntity(entity))
            {
                this.speechHandlers.Add(handler);
            }
        }

        private void EntityManager_EntityDetached(object sender, Entity entity)
        {
            foreach (var handler in this.FindSpeechHandlersInEntity(entity))
            {
                this.speechHandlers.Remove(handler);
            }
        }

        private void VoiceCommandsService_CommandRecognized(object sender, string keyword)
        {
            foreach (var handler in this.speechHandlers)
            {
                handler.OnSpeechKeywordRecognized(keyword);
            }
        }

        private IEnumerable<IMixedRealitySpeechHandler> FindSpeechHandlersInEntity(Entity e)
        {
            return e
                .FindComponents(typeof(IMixedRealitySpeechHandler), isExactType: false)
                .Cast<IMixedRealitySpeechHandler>();
        }
    }
}
