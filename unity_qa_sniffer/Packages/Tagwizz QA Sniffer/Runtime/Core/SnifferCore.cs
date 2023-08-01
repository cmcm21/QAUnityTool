using Tagwizz_QA_Sniffer.Core.InputSystem;

namespace Tagwizz_QA_Sniffer.Core 
{
    public enum SnifferState {RECORDING,PLAYING_BACK}
    public class SnifferCore : Singleton<SnifferCore>
    {
        private SnifferState _state;
        public SnifferState State => _state;

        private ISnifferInput _input;

        public void Start()
        {
            //TODO: Select input system depending on the package that is installed in the project
            _input = new OldSystemInput();
            _input.Init();
        }
    }
}
