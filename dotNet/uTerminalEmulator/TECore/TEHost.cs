namespace TerminalEmulator
{
    public interface TEHost
    {
        void Init(TECore core);
        void OnObtainChar(char item);
        void Write(string str);
        void WriteLine(string str);
    }
}
