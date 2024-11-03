using Evergine.MRTK.SDK.Features.UX.Components.Scrolling;
using Evergine.MRTK.SDK.Features.UX.Components.States;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Evergine.MRTK.Demo.Components.Scrolling
{
    internal class ScrollBarVisibilityStateManager : BaseStateManager<ScrollBarVisibility>
    {
        protected override List<State<ScrollBarVisibility>> GetStateList() =>
            Enum.GetValues<ScrollBarVisibility>()
                .Cast<ScrollBarVisibility>()
                .Select(value => new State<ScrollBarVisibility>
                {
                    Name = value.ToString(),
                    Value = value,
                })
                .ToList();
    }
}
