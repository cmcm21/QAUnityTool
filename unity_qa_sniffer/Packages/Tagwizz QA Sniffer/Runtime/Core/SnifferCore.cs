using Tagwizz_QA_Sniffer.Core.InputSystem;
using Tagwizz_QA_Sniffer.Core.InputSystem.OldSystemInput;

namespace Tagwizz_QA_Sniffer.Core 
{
    public enum SnifferState {RECORDING,PLAYING_BACK}
    public class SnifferCore : Singleton<SnifferCore>
    {
        private SnifferState _state;
        public SnifferState State => _state;

        private SnifferInputSystem _inputSystem;

        public void Start()
        {
            //TODO: Select input system depending on the package that is installed in the project
            _inputSystem = new OldSystemInput();
            _inputSystem.Init();
        }
    }
}
