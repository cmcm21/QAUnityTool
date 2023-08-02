using System;
using System.Collections;
using Codice.Client.Common.WebApi;

namespace Tagwizz_QA_Sniffer.Core.InputSystem.OldSystemInput
{
    public abstract class InputTracker
    {
        public virtual void CheckInputs() {}
        public virtual async void TrackInputTask(){}
    }
}