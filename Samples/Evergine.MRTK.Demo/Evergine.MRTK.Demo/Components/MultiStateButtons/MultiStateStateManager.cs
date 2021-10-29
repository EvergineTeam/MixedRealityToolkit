using System;
using System.Linq;
using System.Collections.Generic;
using Evergine.MRTK.SDK.Features.UX.Components.States;

namespace Evergine.MRTK.Demo.Components.MultiStateButtons
{
    public class MultiStateStateManager : BaseStateManager<MultiStateButtonStates>
    {
        protected override List<State<MultiStateButtonStates>> GetStateList()
        {
            return Enum.GetValues(typeof(MultiStateButtonStates))
                .Cast<MultiStateButtonStates>()
                .Select(s => new State<MultiStateButtonStates>()
                {
                    Name = s.ToString(),
                    Value = s,
                })
                .ToList();
        }
    }
}
